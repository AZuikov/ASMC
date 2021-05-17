using ASMC.Data.Model;
using ASMC.Data.Model.PhysicalQuantity;

namespace ASMC.Devices.Interface.SourceAndMeter
{
    public interface ISourceOutputControl: IDeviceSettingsControl
    {
    
    bool IsEnableOutput { get; }

    void OutputOn();

    void OutputOff();

    }

    public interface ISourcePhysicalQuantity<TPhysicalQuantity, TPhysicalQuantity2> : IDeviceSettingsControl

        where TPhysicalQuantity : class, IPhysicalQuantity<TPhysicalQuantity>, new() where TPhysicalQuantity2 : class, IPhysicalQuantity<TPhysicalQuantity2>, new()

    {
        MeasPoint< TPhysicalQuantity, TPhysicalQuantity2> Value { get; }

        /// <summary>
        /// Установить значение величины.
        /// </summary>
        void SetValue(MeasPoint< TPhysicalQuantity, TPhysicalQuantity2> value);
        /// <summary>
        /// Диапазон воспроизведения и погрешность.
        /// </summary>
        IRangePhysicalQuantity<TPhysicalQuantity, TPhysicalQuantity2> RangeStorage { get; }
    }
    public interface ISourcePhysicalQuantity<T> :  ISourcePhysicalQuantityRange<T> where T : class, IPhysicalQuantity<T>, new()
    {
        MeasPoint<T> Value { get; }

        /// <summary>
        /// Установить значение величины.
        /// </summary>
        void SetValue(MeasPoint<T> value);
        /// <summary>
        /// Диапазон воспроизведения и погрешность.
        /// </summary>
        IRangePhysicalQuantity<T> RangeStorage { get; }
    }
    /// <summary>
    /// Интерфейс источника одной физической величины.
    /// </summary>
    /// <typeparam name="TPhysicalQuantity">Физическая величина.</typeparam>
    public interface ISourcePhysicalQuantityRange<T> where T : class, IPhysicalQuantity<T>, new()
    {
        IRangePhysicalQuantity<T> RangeStorage { get; }
    }

    /// <summary>
    /// Интерфейс источника составной физической величины.
    /// </summary>
    /// <typeparam name="TPhysicalQuantity">Физическая величина.</typeparam>
    /// /// <typeparam name="TPhysicalQuantity2">Физическая величина.</typeparam> 
    //public interface ISourcePhysicalQuantity<TPhysicalQuantity, TPhysicalQuantity2>  where TPhysicalQuantity : class, IPhysicalQuantity<TPhysicalQuantity>, new() where TPhysicalQuantity2 : class, IPhysicalQuantity<TPhysicalQuantity2>, new()
    //{
    //    IRangePhysicalQuantity<TPhysicalQuantity, TPhysicalQuantity2> RangeStorage { get; }
    //}

    public interface IMeterPhysicalQuantityBase<TPhysicalQuantity> : IDeviceSettingsControl
        where TPhysicalQuantity : class, IPhysicalQuantity<TPhysicalQuantity>, new()
    {
        /// <summary>
        /// Считывает измеренное значение с устройства и обновляет значение <see cref="Value"/>.
        /// </summary>
        /// <returns>A MeasPoint.</returns>
        public MeasPoint<TPhysicalQuantity> GetValue();
      
        /// <summary>
        /// Позволяет получать последнее измеренное значение.
        /// </summary>
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