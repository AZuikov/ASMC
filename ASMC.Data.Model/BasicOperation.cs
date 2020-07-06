using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ASMC.Data.Model.Interface;

namespace ASMC.Data.Model
{
    /// <summary>
    /// Предоставляет реализацию
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class BasicOperation<T>  : IBasicOperation<T>
    {
        public Guid Guid { get; } = Guid.NewGuid();
        /// <inheritdoc />
        public T Getting { get; set; }
        /// <inheritdoc />
        public T Expected{ get; set; }
        /// <inheritdoc />
        public Predicate<T> IsGood { get; set; }
    }
    public class MeasuringOperation<T> : BasicOperation <T>, IMeasuringOperation<T>
    {   
        public T Error
        {
            get => ErrorCalculation(Getting, Expected);
        }
        public Func<T, T, T> ErrorCalculation
        {
            set; private get;
        }
    }

    public class BasicOperationVerefication<T> : MeasuringOperation<T>
    {     
        public T LowerTolerance { get; set; }
        public T UpperTolerance { get; set; }
    }
}
