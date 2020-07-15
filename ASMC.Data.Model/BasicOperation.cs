using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using ASMC.Data.Model.Interface;

namespace ASMC.Data.Model
{
    /// <summary>
    /// Предоставляет реализацию
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class BasicOperation<T>  : IBasicOperation<T>
    {     /// <inheritdoc />
        public Action InitWork { get; set; }

        /// <inheritdoc />
        public Action CompliteWork{  get;     set; }
        public Func<CancellationTokenSource, Task>BodyWork
        {
            get; set;
        }
          
        /// <inheritdoc />
        public async Task WorkAsync(CancellationTokenSource token )
        {
            InitWork();
            await BodyWork(token);
            CompliteWork();
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
        public Predicate<T> IsGood { get; set; }
    }
    public class MeasuringOperation<T> : BasicOperation <T>, IMeasuringOperation<T> 
    {
        /// <inheritdoc />
        public T Error
        {
            get => ErrorCalculation(Getting, Expected);
        }
        /// <inheritdoc />
        public Func<T, T, T> ErrorCalculation
        {
            set; private get;
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
    }
}
