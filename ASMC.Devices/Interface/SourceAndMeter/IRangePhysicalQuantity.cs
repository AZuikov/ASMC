using ASMC.Data.Model;
using ASMC.Data.Model.PhysicalQuantity;

namespace ASMC.Devices.Interface.SourceAndMeter
{
    public interface IRangePhysicalQuantity<TPhysicalQuantity, TPhysicalQuantity2> where TPhysicalQuantity : class, IPhysicalQuantity<TPhysicalQuantity>, new() where TPhysicalQuantity2 : class, IPhysicalQuantity<TPhysicalQuantity2>, new()
    {
        RangeStorage<PhysicalRange<TPhysicalQuantity, TPhysicalQuantity2>> Ranges { get; }
        /// <summary>
        /// Выбранный предел измерения.
        /// </summary>
        PhysicalRange<TPhysicalQuantity, TPhysicalQuantity2> SelectRange { get; }

        /// <summary>
        /// Установить диапазон физ. величины.
        /// </summary>
        /// <param name="inRange">Предел, который нужно установить.</param>
        void SetRange(PhysicalRange<TPhysicalQuantity, TPhysicalQuantity2> inRange);
        /// <summary>
        /// Установить диапазон физ. величины.
        /// </summary>
        /// <param name="inRange">Величина, для которой нужно подобрать и установить диапазон.</param>
        void SetRange(MeasPoint<TPhysicalQuantity, TPhysicalQuantity2> inRange);

        /// <summary>
        /// Автоматический выбор предела.
        /// </summary>
        bool IsAutoRange { get; set; }
    }

    public interface IRangePhysicalQuantity<TPhysicalQuantity> where TPhysicalQuantity : class, IPhysicalQuantity<TPhysicalQuantity>, new()
    {
        RangeStorage<PhysicalRange<TPhysicalQuantity>> Ranges { get; }
        /// <summary>
        /// Выбранный предел измерения.
        /// </summary>
        PhysicalRange<TPhysicalQuantity> SelectRange { get; }

        /// <summary>
        /// Установить диапазон физ. величины.
        /// </summary>
        /// <param name="inRange">Предел, который нужно установить.</param>
        void SetRange(PhysicalRange<TPhysicalQuantity> inRange);
        /// <summary>
        /// Установить диапазон физ. величины.
        /// </summary>
        /// <param name="inRange">Величина, для которой нужно подобрать и установить диапазон.</param>
        void SetRange(MeasPoint<TPhysicalQuantity> inRange);

        /// <summary>
        /// Автоматический выбор предела.
        /// </summary>
        bool IsAutoRange { get; set; }
    }
}