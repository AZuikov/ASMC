using System;
using System.Threading;
using System.Threading.Tasks;

namespace ASMC.Data.Model.Interface
{
    /// <summary>
    ///     Предоставляет интерфейс базовой операции.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IBasicOperation<T>
    {
        #region Property

        /// <summary>
        ///     Выполняет инициализацию устройств, данных пере выполнением основной работы <see cref="BodyWorkAsync" /> .
        /// </summary>
        Func<Task> InitWorkAsync { get; set; }

        /// <summary>
        ///     Выполняет основную работку в асинхронном режиме. После чего вызывается метод проверки и завершения работы
        ///     <see cref="CompliteWorkAsync" />.
        /// </summary>
        Action BodyWorkAsync { get; set; }

        /// <summary>
        ///     Вызывает окончательный метод работы, после основного тела <see cref="BodyWorkAsync" />
        /// </summary>
        /// <remarks>
        ///     В случае если возвращает <see cref="false" /> перетаскает выполнение операций заново начиная с инициализации
        ///     <see cref="InitWorkAsync" />.
        /// </remarks>
        Func<Task<bool>> CompliteWorkAsync { get; set; }
        /// <summary>
        /// Имя операции.
        /// </summary>
        /// <remarks>
        /// Может использоваться для обозначения измерения, указание придела, номера измерения и т.д.
        /// </remarks>
        object Name { get; set; }
        /// <summary>
        ///     Уникальный идентификатор операции
        /// </summary>
        Guid Guid { get; }

        /// <summary>
        ///     Позволяет получить и задать получаемое значение.
        /// </summary>
        T Getting { get; set; }

        /// <summary>
        ///     Позволяет получить и задать ожидаемое значение.
        /// </summary>
        T Expected { get; set; }

        /// <summary>
        ///     Позволяет задавать или получать комментарий.
        /// </summary>
        string Comment { get; set; }

        /// <summary>
        ///     Позволяет задать функцию условия проверки соответствия полученного значения ожидаемому.
        /// </summary>
        Predicate<T> IsGood { get; set; }

        #endregion

        /// <summary>
        ///     Предоставляет метод который при вызове заполняет <see cref="IBasicOperation{T}" />.
        /// </summary>
        Task WorkAsync(CancellationTokenSource token);
    }


    public interface IMultiErrorMeasuringOperation<T> : IBasicOperation<T>
    {
        #region Property

        /// <summary>
        ///     Позволяет получить значение погрешности.
        /// </summary>
        T[] Error { get; }

        /// <summary>
        ///     Поваляет задать функции расчета погрешности.
        /// </summary>
        Func<T, T, T>[] ErrorCalculation { set; }

        #endregion
    }

    /// <summary>
    ///     Предоставляет интерфейс операции измерения.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IMeasuringOperation<T> : IBasicOperation<T>
    {
        #region Property

        /// <summary>
        ///     Позволяет получить значение погрешности.
        /// </summary>
        T Error { get; }

        /// <summary>
        ///     Позволяет задать функцию расчета погрешности.
        /// </summary>
        Func<T, T, T> ErrorCalculation { set; }

        #endregion
    }

   /// <summary>
    ///     Предоставляет интерфейс операции измерения.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IBasicOperationVerefication<T> : IMeasuringOperation<T>
    {
        #region Property

        /// <summary>
        ///     Позволяет получить или задать нижнюю допустимую границу.
        /// </summary>
        T LowerTolerance { get; }

        /// <summary>
        ///     Позволяет получить или задать верхнюю допустимую границу.
        /// </summary>
        T UpperTolerance { get; }

        #endregion
    }
}