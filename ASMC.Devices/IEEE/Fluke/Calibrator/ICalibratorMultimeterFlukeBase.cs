using ASMC.Data.Model;
using ASMC.Data.Model.PhysicalQuantity;
using ASMC.Devices.Interface;
using ASMC.Devices.Interface.SourceAndMeter;

namespace ASMC.Devices.IEEE.Fluke.Calibrator
{
    public interface ICalibratorMultimeterFlukeBase : IVoltageGroupForCalibrator, ICurrnetGroupForCalibrator, IResistance2W, IUserType
    {

    }

    public interface IVoltageGroupForCalibrator : IDcVoltage, IAcVoltage
    {

    }

    public interface ICurrnetGroupForCalibrator : IDcCurrent, IAcCurrent
    {

    }

    public interface IResistanceGroupForCalibrator : IResistance2W, IResistance4W
    {

    }

    public interface IDcVoltage
    {
        ISourcePhysicalQuantity<Voltage> DcVoltage { get;  }
    }

    public interface IAcVoltage
    {
        ISourcePhysicalQuantity<Voltage, Frequency> AcVoltage { get;  }
    }

    public interface IDcCurrent
    {
        ISourcePhysicalQuantity<Current> DcCurrent { get;  }
    }

    public interface IAcCurrent
    {
        ISourcePhysicalQuantity<Current, Frequency> AcCurrent { get;  }
    }

    /// <summary>
    /// Интерфейс режима воспроизведения сопротивления калибратора. Двухпроводная схема.
    /// </summary>
    public interface IResistance2W
    {
        ISourcePhysicalQuantity<Resistance> Resistance2W { get;  }
    }

    /// <summary>
    /// Интерфейс режима воспроизведения сопротивления калибратора. Четырехпроводная схема.
    /// </summary>
    public interface IResistance4W
    {
        ISourcePhysicalQuantity<Resistance> Resistance4W { get;  }
    }

    public interface ITemperature
    {
        ISourcePhysicalQuantity<Temperature> Temperature { get; }
    }
}