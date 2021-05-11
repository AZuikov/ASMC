using ASMC.Data.Model;
using ASMC.Data.Model.PhysicalQuantity;

namespace ASMC.Devices.Interface.SourceAndMeter
{
    public interface ISourcePhysicalQuantityBase<T> : IDeviceSettingsControl
    {
        T Value { get; }

        /// <summary>
        /// Установить значение величины.
        /// </summary>
        void SetValue(T value);

        bool IsEnableOutput { get; }

        void OutputOn();

        void OutputOff();
    }

    /// <summary>
    /// Интерфейс источника одной физической величины.
    /// </summary>
    /// <typeparam name="TPhysicalQuantity">Физическая величина.</typeparam>
    public interface ISourcePhysicalQuantity<TPhysicalQuantity> : ISourcePhysicalQuantityBase<MeasPoint<TPhysicalQuantity>> where TPhysicalQuantity : class, IPhysicalQuantity<TPhysicalQuantity>, new()
    {
        IRangePhysicalQuantity<TPhysicalQuantity> RangeStorage { get; }
    }

    /// <summary>
    /// Интерфейс источника составной физической величины.
    /// </summary>
    /// <typeparam name="TPhysicalQuantity">Физическая величина.</typeparam>
    /// /// <typeparam name="TPhysicalQuantity2">Физическая величина.</typeparam>
    public interface ISourcePhysicalQuantity<TPhysicalQuantity, TPhysicalQuantity2> :
        ISourcePhysicalQuantityBase<MeasPoint<TPhysicalQuantity, TPhysicalQuantity2>> where TPhysicalQuantity : class, IPhysicalQuantity<TPhysicalQuantity>, new()
                                                                                      where TPhysicalQuantity2 : class, IPhysicalQuantity<TPhysicalQuantity2>, new()
    {
        IRangePhysicalQuantity<TPhysicalQuantity, TPhysicalQuantity2> RangeStorage { get; }
    }

    public interface IMeterPhysicalQuantityBase<TPhysicalQuantity> : IDeviceSettingsControl
        where TPhysicalQuantity : class, IPhysicalQuantity<TPhysicalQuantity>, new()
    {
        public MeasPoint<TPhysicalQuantity> GetValue();

        public MeasPoint<TPhysicalQuantity> Value { get; }
    }

    /// <summary>
    /// Измерение физической величины.
    /// </summary>
    /// <typeparam name="TPhysicalQuantity"></typeparam>
    public interface IMeterPhysicalQuantity<TPhysicalQuantity> : IMeterPhysicalQuantityBase<TPhysicalQuantity>
        where TPhysicalQuantity : class, IPhysicalQuantity<TPhysicalQuantity>, new()
    {
        IRangePhysicalQuantity<TPhysicalQuantity> RangeStorage { get; }
    }

    /// <summary>
    /// Измерение физической величины.
    /// </summary>
    /// <typeparam name="TPhysicalQuantity"></typeparam>
    /// <typeparam name="TPhysicalQuantity1"></typeparam>
    public interface IMeterPhysicalQuantity<TPhysicalQuantity, TPhysicalQuantity1> : IMeterPhysicalQuantityBase<TPhysicalQuantity>
        where TPhysicalQuantity : class, IPhysicalQuantity<TPhysicalQuantity>, new() where TPhysicalQuantity1 : class, IPhysicalQuantity<TPhysicalQuantity1>, new()
    {
        IRangePhysicalQuantity<TPhysicalQuantity, TPhysicalQuantity1> RangeStorage { get; }
    }
}