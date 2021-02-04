using ASMC.Data.Model;
using ASMC.Data.Model.PhysicalQuantity;
using ASMC.Devices.Interface.Multimetr.Mode;

namespace ASMC.Devices.Interface.Multimetr.Mode
{
   

    public interface IVoltageGroup: IDcVoltage, IAcVoltage
    {
    
    }


    public interface IResolution<T>
    {
        void SetResolution(T inResolution);
    }
    public interface IDcVoltage
    {
        #region Property

        IMeterPhysicalQuantity<Voltage> DcVoltage { get; set; }

        #endregion
    }

    public interface IAcVoltage
    {
        #region Property

        IMeterPhysicalQuantity<Voltage> AcVoltage { get; set; }

        #endregion
    }

    public interface IAcFilter
    {
        void SetFilter(MeasPoint<Frequency> filterFreq);
    }

    public interface ICurrentGroup: IDcCurrent,IAcCurrent
    {
        
    }

    

    public interface IDcCurrent
    {
        #region Property

        IMeterPhysicalQuantity<Current> DcCurrent { get; set; }

        #endregion
    }

    public interface IAcCurrent
    {
        #region Property

        IMeterPhysicalQuantity<Current> AcCurrent { get; set; }

        #endregion
    }

    

    public interface IResistanceGroup: IResistance2W, IResistance4W
    {
       
    }

    public interface IResistance2W
    {
        #region Property

        IMeterPhysicalQuantity<Resistance> Resistance2W { get; set; }

        #endregion
    }

    public interface IResistance4W
    {
        #region Property

        IMeterPhysicalQuantity<Resistance> Resistance4W { get; set; }

        #endregion
    }

    public interface ICapacity
    {
        #region Property

        IMeterPhysicalQuantity<Capacity> Capacity { get; set; }

        #endregion
    }

    public interface IFrequency
    {
        #region Property

        IMeterPhysicalQuantity<Frequency> Frequency { get; set; }

        #endregion
    }

    public interface ITime
    {
        #region Property

        IMeterPhysicalQuantity<Time> Time { get; set; }

        #endregion
    }

    public interface ITemperature
    {
        #region Property

        IMeterPhysicalQuantity<Temperature> Temperature { get; set; }

        #endregion
    }
}

namespace ASMC.Devices.Interface.Multimetr
{
    public interface IMultimetr : IResistanceGroup, IVoltageGroup, ICurrentGroup, IUserType, IDevice
    {
    }
}