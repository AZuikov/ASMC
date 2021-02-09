using ASMC.Data.Model;
using ASMC.Data.Model.PhysicalQuantity;
using ASMC.Devices.Interface.Multimetr.Mode;
using ASMC.Devices.Interface.SourceAndMeter;

namespace ASMC.Devices.Interface.Multimetr.Mode
{
   

    public interface IVoltageGroup: IDcVoltage, IAcVoltage
    {
    
    }

    public interface IDcVoltage
    {
        #region Property

        IMeterPhysicalQuantity<Voltage> DcVoltage { get;  }

        #endregion
    }

    public interface IAcVoltage
    {
        #region Property

        IMeterPhysicalQuantity<Voltage> AcVoltage { get;  }

        #endregion
    }

    public interface ICurrentGroup: IDcCurrent,IAcCurrent
    {
        
    }

    

    public interface IDcCurrent
    {
        #region Property

        IMeterPhysicalQuantity<Current> DcCurrent { get;  }

        #endregion
    }

    public interface IAcCurrent
    {
        #region Property

        IMeterPhysicalQuantity<Current> AcCurrent { get; }

        #endregion
    }

    

    public interface IResistanceGroup: IResistance2W, IResistance4W
    {
       
    }

    public interface IResistance2W
    {
        #region Property

        IMeterPhysicalQuantity<Resistance> Resistance2W { get;}

        #endregion
    }

    public interface IResistance4W
    {
        #region Property

        IMeterPhysicalQuantity<Resistance> Resistance4W { get;  }

        #endregion
    }

    public interface ICapacity
    {
        #region Property

        IMeterPhysicalQuantity<Capacity> Capacity { get;}

        #endregion
    }

    public interface IFrequency
    {
        #region Property

        IMeterPhysicalQuantity<Frequency> Frequency { get; }

        #endregion
    }

    public interface ITime
    {
        #region Property

        IMeterPhysicalQuantity<Time> Time { get;  }

        #endregion
    }

    public interface ITemperature
    {
        #region Property

        IMeterPhysicalQuantity<Temperature> Temperature { get;  }

        #endregion
    }
}

namespace ASMC.Devices.Interface.Multimetr
{
    public interface IMultimetr : IResistanceGroup, IVoltageGroup, ICurrentGroup, IUserType, IDevice
    {
    }
}