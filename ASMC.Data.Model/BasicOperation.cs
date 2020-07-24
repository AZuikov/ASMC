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
    public class BasicOperation<T>  : IBasicOperation<T>, ICloneable
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

        public virtual object Clone()
        {
            return new BasicOperation<T> { InitWork = InitWork, BodyWork = BodyWork, IsGood = IsGood, Comment = Comment, Expected = Expected, Getting = Getting, CompliteWork = CompliteWork };
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
            return new MeasuringOperation<T>() { ErrorCalculation = ErrorCalculation, CompliteWork = @base.CompliteWork, IsGood = @base.IsGood, Getting = @base.Getting, Expected = @base.Expected, InitWork = @base.InitWork, BodyWork = @base.BodyWork, Comment = @base.Comment };
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
            return new BasicOperationVerefication<T> { LowerTolerance = LowerTolerance, UpperTolerance = UpperTolerance, ErrorCalculation = ErrorCalculation, CompliteWork = @base.CompliteWork, IsGood = @base.IsGood, Getting = @base.Getting, Expected = @base.Expected, InitWork = @base.InitWork, BodyWork = @base.BodyWork, Comment = @base.Comment };
        }
    }
}
