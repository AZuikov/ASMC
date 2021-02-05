using ASMC.Data.Model;
using ASMC.Data.Model.PhysicalQuantity;

namespace ASMC.Devices.Interface.SourceAndMeter
{
    public interface ISourcePhysicalQuantityBase<T> : IDevice
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
    public interface ISourcePhysicalQuantity<TPhysicalQuantity, TPhysicalQuantity2> : ISourcePhysicalQuantityBase<MeasPoint<TPhysicalQuantity, TPhysicalQuantity2>> where TPhysicalQuantity : class, IPhysicalQuantity<TPhysicalQuantity>, new() where TPhysicalQuantity2 : class, IPhysicalQuantity<TPhysicalQuantity2>, new() 
    {
         IRangePhysicalQuantity<TPhysicalQuantity, TPhysicalQuantity2> RangeStorage { get; }
    }



    /// <summary>
    /// Измерение физической величины.
    /// </summary>
    /// <typeparam name="TPhysicalQuantity"></typeparam>
    public interface IMeterPhysicalQuantity<TPhysicalQuantity> : IDevice
        where TPhysicalQuantity : class, IPhysicalQuantity<TPhysicalQuantity>, new()
    {
        IRangePhysicalQuantity<TPhysicalQuantity> RangeStorage { get; }
        public MeasPoint<TPhysicalQuantity> GetValue();
        public MeasPoint<TPhysicalQuantity> Value { get; }
    }
}