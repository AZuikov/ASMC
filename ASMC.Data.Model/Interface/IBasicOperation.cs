using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ASMC.Data.Model.PhysicalQuantity;

namespace ASMC.Data.Model.Interface
{
    /// <summary>
    /// Предоставляет инерфейс базовой операции.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IBasicOperation<T>
    {
        Func<Task> InitWork
        {
            get; set;
        }
        Action BodyWork
        {
            get; set;
        }

        Func<Task<bool>> CompliteWork
        {
            get;
            set;
        }
        /// <summary>
        /// Пhедоставляет метод который при вызове заполняет <see cref="IBasicOperation{T}"/>
        /// </summary>
        Task WorkAsync(CancellationToken token);
        /// <summary>
        /// Уникальный идентификатор операции
        /// </summary>
        Guid Guid { get; }

        /// <summary>
        /// Позволяет получить и задать получаемое значение.
        /// </summary>
        T Getting
        {
            get; set;
        }
        /// <summary>
        /// Позволяет получить и задать ожидаемое значение.
        /// </summary>
        T Expected
        {
            get; set;
        }
        /// <summary>
        /// Позволяет задавать или получать коментарий.
        /// </summary>
        string Comment { get; set; }
        /// <summary>
        /// Позволяет задать функцию условия проверки соответствия полученого знаечния ожидаемому.
        /// </summary>
        Func<bool> IsGood
        {
            get; set;
        }

    }
    /// <summary>
    /// Предоставляет интерфейс операции измерения.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IMeasuringOperation<T> :IBasicOperation<T>   
    {
        /// <summary>
        /// Позволяет получить и задать значение погрешности.
        /// </summary>
        T Error
        {
            get;
        }
        /// <summary>
        /// Позваляет задать функцию расчета погрешности.
        /// </summary>
        Func<T, T, T> ErrorCalculation
        {
            set;
        }
    }
    /// <inheritdoc />
    /// <summary>
    /// Предоставляет интерфейс операции измерения.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IBasicOperationVerefication<T> :  IBasicOperation<T>
    {
        /// <summary>
        /// Позволяет получить или задать нижнюю допустимую границу.
        /// </summary>
        T LowerTolerance
        {
            get; 
        }
        /// <summary>
        /// Позволяет получить или задать верхнюю допустимую границу.
        /// </summary>
        T UpperTolerance
        {
            get; 
        }
    }
  

}
