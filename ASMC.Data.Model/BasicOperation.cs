using System;
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
        private Func<CancellationToken, Task> _bodyWork;

        /// <inheritdoc />
        public Func<Task> InitWorkAsync
        {
            get
            {
                return _initWork ?? (() =>  Task.CompletedTask);
            }
            set => _initWork = value;
        }

        /// <inheritdoc />
        public Func<Task<bool>> CompliteWorkAsync
        {
            get
            {
#pragma warning disable CS1998 // В асинхронном методе отсутствуют операторы await, будет выполнен синхронный метод
                return _compliteWork ??  (async () => true);
#pragma warning restore CS1998 // В асинхронном методе отсутствуют операторы await, будет выполнен синхронный метод
            }

            set => _compliteWork = value;
        }

        /// <inheritdoc />
        public object Name { get; set; }

        /// <inheritdoc />
        public Func<CancellationToken, Task> BodyWorkAsync
        {
            get
            {
                return _bodyWork ?? (cancellationToken => Task.CompletedTask);
            }
            set => _bodyWork = value;
        }

        /// <inheritdoc />
        public Func<bool> IsGood { get; set; }

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
                await Cheked(InitWorkAsync());
                Logger.Debug("Закончено выполнение инициализации");
                Logger.Debug("Начато выполнение тела");
                await Cheked(BodyWorkAsync(token.Token));
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

            #region Inner Methods

            async Task Cheked(Task task)
            {
                try
                {
                    await task;
                }
                catch
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
            }

            #endregion

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

    public class MultiErrorMeasuringOperation<T> :  BasicOperation<T>,  IMultiErrorMeasuringOperation<T>
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

        /// <inheritdoc />
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
