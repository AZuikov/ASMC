using System;
using System.Collections;
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
        /// <summary>
        /// Выполняет иннициализацию устройств, данных пере выполнением основной работы <see cref="BodyWorkAsync"/> .
        /// </summary>
        Func<Task> InitWork
        {
            get; set;
        }
        /// <summary>
        /// Выполняет основную работку в асинхронном режиме. После чего вызыается метод проверки и завершения работы <see cref="CompliteWork"/>.
        /// </summary>
        Action BodyWorkAsync
        {
            get; set;
        }
        /// <summary>
        /// Вызывает окончательный метод работы, после основного тела <see cref="BodyWorkAsync"/>
        /// </summary>
        /// <remarks>
        /// В случае если возвращает <see cref="false"/> перезапускает выполнение операций загого начиная с инициализации <see cref="InitWork"/>.
        /// </remarks>
        Func<Task<bool>> CompliteWork
        {
            get;
            set;
        }
        /// <summary>
        /// Предоставляет метод который при вызове заполняет <see cref="IBasicOperation{T}"/>.
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


    public interface IMultiErrorMeasuringOperation<T> :IBasicOperation<T>
    {
        /// <summary>
        /// Позволяет получить значение погрешности.
        /// </summary>
        T[] Error
        {
            get;
        }
        /// <summary>
        /// Позваляет задать функции расчета погрешности.
        /// </summary>
        Func<T, T, T>[] ErrorCalculation
        {
            set;
        }

    }

    /// <summary>
    /// Предоставляет интерфейс операции измерения.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IMeasuringOperation<T> :IBasicOperation<T>   
    {
        /// <summary>
        /// Позволяет получить значение погрешности.
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
    public interface IBasicOperationVerefication<T> : IMeasuringOperation<T>
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
