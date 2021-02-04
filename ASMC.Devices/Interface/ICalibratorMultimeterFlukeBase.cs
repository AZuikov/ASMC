using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ASMC.Data.Model.PhysicalQuantity;

namespace ASMC.Devices.Interface.ICalibrator
{
    public interface ICalibratorMultimeterFlukeBase : IVoltageGroupForCalibrator, ICurrnetGroupForCalibrator, IResistanceGroupForCalibrator, ITemperature
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
        ISourcePhysicalQuantity<Voltage> DcVoltage { get; set; }
    }

    public interface IAcVoltage
    {
        ISourcePhysicalQuantity<Voltage, Frequency> AcVoltage { get; set; }
    }

    public interface IDcCurrent
    {
        ISourcePhysicalQuantity<Current> DcCurrent { get; set; }
    }

    public interface IAcCurrent
    {
        ISourcePhysicalQuantity<Current, Frequency> AcCurrent { get; set; }
    }

    /// <summary>
    /// Интерфейс режима воспроизведения сопротивления калибратора. Двухпроводная схема.
    /// </summary>
    public interface IResistance2W
    {
        ISourcePhysicalQuantity<Resistance> Resistance2W { get; set; }
    }

    /// <summary>
    /// Интерфейс режима воспроизведения сопротивления калибратора. Четырехпроводная схема.
    /// </summary>
    public interface IResistance4W
    {
        ISourcePhysicalQuantity<Resistance> Resistance4W { get; set; }
    }

    public interface ITemperature
    {
        ISourcePhysicalQuantity<Temperature> Temperature { get; set; }
    }
}