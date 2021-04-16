using ASMC.Data.Model;
using ASMC.Data.Model.PhysicalQuantity;
using ASMC.Devices.Interface;
using ASMC.Devices.Interface.Multimetr;
using ASMC.Devices.Interface.Multimetr.Mode;
using ASMC.Devices.Interface.SourceAndMeter;

namespace ASMC.Devices.IEEE.Keysight.Multimeter
{
    public interface IDigitalMultimetr344xx : IAcVoltage, IResistance2W, IFrequency, IDcVoltage, IDcCurrent, IAcCurrent, IProtocolStringLine
    {

    }
    public interface IAcFilter<T, T2> : IMeterPhysicalQuantity<T, T2> where T : class, IPhysicalQuantity<T>, new() where T2 : class, IPhysicalQuantity<T2>, new()
    {
        IFilter<T,T2> Filter { get; }
    }

    public interface IAcVoltage
    {
        #region Property

        IAcFilter<Voltage,Frequency> AcVoltage { get; }

        #endregion
    }
    public interface IAcCurrent
    {
        #region Property

        IAcFilter<Data.Model.PhysicalQuantity.Current, Frequency> AcCurrent { get; }

        #endregion
    }
    public interface IFilter<T, T2> where T : class, IPhysicalQuantity<T>, new() where T2 : class, IPhysicalQuantity<T2>, new()
    {
        void SetFilter(MeasPoint<T2> filterFreq);
        void SetFilter(MeasPoint<T, T2> filterFreq);
        void SetFilter(ICommand filter);
        ICommand FilterSelect { get; }

        ICommand[] Filters { get; }
    }

}