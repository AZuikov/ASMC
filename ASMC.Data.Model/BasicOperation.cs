using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using ASMC.Data.Model.Interface;
using ASMC.Data.Model.PhysicalQuantity;
using NLog;

namespace ASMC.Data.Model
{
    /// <summary>
    /// Предоставляет реализацию базоовой операции.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class BasicOperation<T>  : IBasicOperation<T>, ICloneable
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private Func<Task<bool>> _compliteWork;
        private Func<Task> _initWork;
        private Action _bodyWork;

        /// <inheritdoc />
        public Func<Task> InitWork
        {
            get
            {
                if (_initWork == null) return () =>  Task.CompletedTask;
                return _initWork;
            }
            set => _initWork = value;
        }

        /// <inheritdoc />
        public Func<Task<bool>> CompliteWork
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
        public async Task WorkAsync(CancellationToken token )
        {
            do
            {
                if(token.IsCancellationRequested)
                { 
                    token.ThrowIfCancellationRequested();
                }

                Logger.Debug("Начата инициализация");
                await InitWork();
                Logger.Debug("Закончено выполнение инициализации");
                Logger.Debug("Начато выполнение тела");
                var task = Task.Run(BodyWorkAsync, token);
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
            } while (!await CompliteWork());
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
        /// <inheritdoc />
        public Func<bool> IsGood { get; set; }

        public virtual object Clone()
        {
            return new BasicOperation<T> { InitWork = InitWork, BodyWorkAsync = BodyWorkAsync, IsGood = IsGood, Comment = Comment, Expected = Expected, Getting = Getting, CompliteWork = CompliteWork };
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
                //return ErrorCalculation.Select(ec => ec(Getting, Expected)).ToArray();
            }
        }

        /// <inheritdoc />
        public Func<T, T, T>[] ErrorCalculation { get; set; }
        public override object Clone()
        {
            var @base = (BasicOperation<T>)base.Clone();
            return new MultiErrorMeasuringOperation<T> { ErrorCalculation = ErrorCalculation, CompliteWork = @base.CompliteWork, IsGood = @base.IsGood, Getting = @base.Getting, Expected = @base.Expected, InitWork = @base.InitWork, BodyWorkAsync = @base.BodyWorkAsync, Comment = @base.Comment };
        }
    }

    public class MeasuringOperation<T> : BasicOperation <T>, IMeasuringOperation<T> 
    {
        /// <inheritdoc />
        public T Error => ErrorCalculation(Getting, Expected);

        /// <inheritdoc />
        public Func<T, T, T> ErrorCalculation
        {
            set; protected get;
        }
        public override object Clone()
        {
            var @base = (BasicOperation<T>)base.Clone();
            return new MeasuringOperation<T> { ErrorCalculation = ErrorCalculation, CompliteWork = @base.CompliteWork, IsGood = @base.IsGood, Getting = @base.Getting, Expected = @base.Expected, InitWork = @base.InitWork, BodyWorkAsync = @base.BodyWorkAsync, Comment = @base.Comment };
        }

    }
    /// <summary>
    /// Педоставляет реализации операцию с нижней и верхней границей.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class BasicOperationVerefication<T> : MeasuringOperation<T>, IBasicOperationVerefication<T>
    {     
        /// <summary>
        /// Позволяет получить или задать нижнюю допустимую границу.
        /// </summary>
        public T LowerTolerance { get; set; }

        /// <summary>
        /// Позволяет получить или задать верхнюю допустимую границу.
        /// </summary>
        public T UpperTolerance { get; set; }
        public override object Clone()
        {
            var @base = (MeasuringOperation<T>)base.Clone();
            return new BasicOperationVerefication<T> { LowerTolerance = LowerTolerance, UpperTolerance = UpperTolerance, ErrorCalculation = ErrorCalculation, CompliteWork = @base.CompliteWork, IsGood = @base.IsGood, Getting = @base.Getting, Expected = @base.Expected, InitWork = @base.InitWork, BodyWorkAsync = @base.BodyWorkAsync, Comment = @base.Comment };
        }

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
