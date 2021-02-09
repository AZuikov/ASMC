using ASMC.Data.Model;
using ASMC.Data.Model.PhysicalQuantity;
using ASMC.Devices.Interface;
using ASMC.Devices.Interface.Multimetr;
using ASMC.Devices.Interface.Multimetr.Mode;
using ASMC.Devices.Interface.SourceAndMeter;

namespace ASMC.Devices.IEEE.Keysight.Multimeter
{
    public interface IDigitalMultimetr344xx : IAcVoltage, IResistance2W, ICapacity, IFrequency, IDcVoltage, IDcCurrent, IAcCurrent, IProtocolStringLine
    {

    }
    public interface IAcFilter<T>: IMeterPhysicalQuantity<T> where T : class, IPhysicalQuantity<T>, new()
    {
        IFilter<T> Filter { get; }
    }

    public interface IAcVoltage
    {
        #region Property

        IAcFilter<Voltage> AcVoltage { get; }

        #endregion
    }
    public interface IAcCurrent
    {
        #region Property

        IAcFilter<Data.Model.PhysicalQuantity.Current> AcCurrent { get; }

        #endregion
    }
    public interface IFilter<T> where T : class, IPhysicalQuantity<T>, new()
    {
        void SetFilter(MeasPoint<Frequency> filterFreq);
         void SetFilter(MeasPoint<T, Frequency> filterFreq);
        void SetFilter(ICommand filter);
        ICommand FilterSelect { get; }

        ICommand[] Filters { get; }
    }

}