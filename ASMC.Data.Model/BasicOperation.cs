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
    {
        private Func<bool> _func;
        private Action _action;

        /// <inheritdoc />
        public Action InitWork
        {
            get
            {
                if (_action == null) return () => { };
                return _action;
            }
            set => _action = value;
        }

        /// <inheritdoc />
        public Func<bool> CompliteWork
        {
            get
            {
                if (_func == null) return () => true;
                return _func;
            }

            set { _func = value; }
        }
            
        
        public Action BodyWork
        {
            get; set;
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
                InitWork(); 
                await Task.Factory.StartNew(BodyWork, token, TaskCreationOptions.AttachedToParent, TaskScheduler.Current);
            } while (!CompliteWork());
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
    }
    public class MeasuringOperation<T> : BasicOperation <T>, IMeasuringOperation<T> 
    {
        /// <inheritdoc />
        public T Error => ErrorCalculation(Getting, Expected);

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
