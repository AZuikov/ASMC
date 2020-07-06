using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASMC.Data.Model.Interface
{
    /// <summary>
    /// Предоставляет инерфейс базаовой операции.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IBasicOperation<T>
    {
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
<<<<<<< HEAD
        /// Позволяет задать условие проверки соответствия полученого знаечния ожидаемому.
=======
        /// Позволяет задавать или получать коментарий.
        /// </summary>
        string Comment { get; set; }
        /// <summary>
        /// Позволяет задать условие провеверки соответствия полученого знаечния ожидаемому.
>>>>>>>  мелкие изменения
        /// </summary>
        Predicate<T> IsGood
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
