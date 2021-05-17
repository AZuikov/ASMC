﻿using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ASMC.Data.Model.Interface;
using NLog;

namespace ASMC.Data.Model
{
    /// <summary>
    /// Предоставляет реализацию базовой операции.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class BasicOperation<T>  : IBasicOperation<T>, ICloneable
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private Func<Task<bool>> _compliteWork;
        private Func<Task> _initWork;
        private Action _bodyWork;

        /// <inheritdoc />
        public Func<Task> InitWorkAsync
        {
            get
            {
                if (_initWork == null) return () =>  Task.CompletedTask;
                return _initWork;
            }
            set => _initWork = value;
        }

        /// <inheritdoc />
        public Func<Task<bool>> CompliteWorkAsync
        {
            get
            {
                if (_compliteWork == null)
#pragma warning disable 1998
                return async () => true;
#pragma warning restore 1998
                return _compliteWork;
            }

            set { _compliteWork = value; }
        }

        /// <inheritdoc />
        public object Name { get; set; }

        /// <inheritdoc />
        public Action BodyWorkAsync
        {
            get
            {
                if(_bodyWork == null)
                    return () => { };
                return _bodyWork;
            }
            set => _bodyWork = value;
        }

        /// <inheritdoc />
        public Predicate<T> IsGood { get; set; }

        /// <inheritdoc />
        public async Task WorkAsync(CancellationTokenSource token)
        {
            Task<bool> taskColmplit = null;
            do
            {
                if(token.IsCancellationRequested)
                { 
                    token.Token.ThrowIfCancellationRequested();
                }

                Logger.Debug("Начата инициализация");
                await InitWorkAsync();
                Logger.Debug("Закончено выполнение инициализации");
                Logger.Debug("Начато выполнение тела");
                var task = Task.Run(BodyWorkAsync, token.Token);
                try
                {
                    await task;
                }
                catch (Exception)
                {
                    if (task.Status == TaskStatus.Faulted)
                    {
                        if (task.Exception != null)
                        {
                            foreach (var ex in task.Exception.InnerExceptions)
                            {
                                Logger.Error(ex);
                                throw ex;
                            }
                        }
                    }
                }
                Logger.Debug("Закончено выполнение тела");
                taskColmplit = CompliteWorkAsync();
                try
                {
                    await taskColmplit;
                }
                catch (Exception e)
                {
                    Logger.Error(taskColmplit.Exception != null ? taskColmplit.Exception.InnerException : e);
                }

            } while (!taskColmplit.Result);
            Logger.Debug("Закончено точка");
        }

        /// <inheritdoc />
        public Guid Guid { get; } = Guid.NewGuid();
        /// <inheritdoc />
        public T Getting { get; set; }
        /// <inheritdoc />
        public T Expected{ get; set; }
        /// <inheritdoc />
        public string Comment { get; set; }
      

        public virtual object Clone()
        {
            return new BasicOperation<T> { InitWorkAsync = InitWorkAsync, BodyWorkAsync = BodyWorkAsync, IsGood = IsGood, Comment = Comment, Expected = Expected, Getting = Getting, CompliteWorkAsync = CompliteWorkAsync };
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"Ожидаемое значение {Expected}, а измеренное  {Getting}";
        }
    }

    public class MultiErrorMeasuringOperation<T> :  BasicOperation<T>, ICloneable, IMultiErrorMeasuringOperation<T>
    {
        /// <inheritdoc />
        public T[] Error 
        {
            get
            {
                return ErrorCalculation.Select(err => err(Getting, Expected)).ToArray();
            }
        }

        /// <inheritdoc />
        public Func<T, T, T>[] ErrorCalculation { get; set; }
        public override object Clone()
        {
            var @base = (BasicOperation<T>)base.Clone();
            return new MultiErrorMeasuringOperation<T> { ErrorCalculation = ErrorCalculation, CompliteWorkAsync = @base.CompliteWorkAsync, IsGood = @base.IsGood, Getting = @base.Getting, Expected = @base.Expected, InitWorkAsync = @base.InitWorkAsync, BodyWorkAsync = @base.BodyWorkAsync, Comment = @base.Comment };
        }

    }

    public class MeasuringOperation<T> : BasicOperation <T>, IMeasuringOperation<T> 
    {
        /// <inheritdoc />
        public T Error => ErrorCalculation(Expected,Getting);

        /// <inheritdoc />
        public Func<T, T, T> ErrorCalculation
        {
            set; protected get;
        }
        public override object Clone()
        {
            var @base = (BasicOperation<T>)base.Clone();
            return new MeasuringOperation<T> { ErrorCalculation = ErrorCalculation, CompliteWorkAsync = @base.CompliteWorkAsync, IsGood = @base.IsGood, Getting = @base.Getting, Expected = @base.Expected, InitWorkAsync = @base.InitWorkAsync, BodyWorkAsync = @base.BodyWorkAsync, Comment = @base.Comment };
        }
        /// <inheritdoc />
        public override string ToString()
        {
            return $"Ожидаемое значение {Expected}, а измеренное  {Getting}. Погрешность измерения составляет {Error}";
        }
    }
    /// <summary>
    /// Предоставляет реализации операцию с нижней и верхней границей.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Obsolete("Необходимо отказаться от использование Error в виде расчета погрешности, поскольку это свойство является фактической погрешностью.")]
    public class BasicOperationVerefication<T> : MeasuringOperation<T>, IBasicOperationVerefication<T>
    {
        /// <summary>
        /// Позволяет получить нижнюю допустимую границу.
        /// </summary>
        public T LowerTolerance => LowerCalculation(Expected);

        public Func<T, T> LowerCalculation
        {
            set; protected get;
        }
        public Func<T, T> UpperCalculation
        {
            set; protected get;
        }

        /// <summary>
        /// Позволяет получить верхнюю допустимую границу.
        /// </summary>
        public T UpperTolerance => UpperCalculation(Expected);
        public override object Clone()
        {
            var @base = (MeasuringOperation<T>)base.Clone();
            return new BasicOperationVerefication<T> { LowerCalculation = LowerCalculation, UpperCalculation = UpperCalculation, ErrorCalculation = ErrorCalculation, CompliteWorkAsync = @base.CompliteWorkAsync, IsGood = @base.IsGood, Getting = @base.Getting, Expected = @base.Expected, InitWorkAsync = @base.InitWorkAsync, BodyWorkAsync = @base.BodyWorkAsync, Comment = @base.Comment };
        }
        [Obsolete("Необходимо от казаться от использование Error в текущем контексте")]
        public override string ToString()
        {
            return $"Текущая точка {this.Expected} не проходит по допуску:\n" +
                   $"Минимально допустимое значение {this.LowerTolerance}\n" +
                   $"Максимально допустимое значение {this.UpperTolerance}\n" +
                   $"Допустимое значение погрешности {this.Error}\n" +
                   $"ИЗМЕРЕННОЕ значение {this.Getting}\n\n";
        }
    }
}
