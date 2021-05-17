using System;
using AP.Utils.Data;
using ASMC.Data.Model;
using ASMC.Data.Model.PhysicalQuantity;
using ASMC.Devices.Interface;
using ASMC.Devices.Interface.SourceAndMeter;

namespace ASMC.Devices.IEEE.Fluke.Calibrator
{
    public interface ICalibratorMultimeterFlukeBase : IVoltageGroupForCalibrator, ICurrnetGroupForCalibrator, IResistance2W, ICapacity, ITemperature, IProtocolStringLine, IFrequency, ISourceOutputControl
    {

    }

    public interface IVoltageGroupForCalibrator : IDcVoltage, IAcVoltage
    {

    }

    public interface ICurrnetGroupForCalibrator : IDcCurrent, IAcCurrent
    {

    }


    
    public interface IFrequency
    {
        ISourcePhysicalQuantity<Frequency, Voltage> Frequency { get;  }
    }
    public interface IDcVoltage
    {
        ISourcePhysicalQuantity<Voltage> DcVoltage { get; }
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

    public interface ICapacity
    {
        ISourcePhysicalQuantity<Capacity> Capacity { get; }
    }

   

    public enum Compensation
    {
        CompNone=0,
        Comp2W=2
        
    }

    /// <summary>
    /// Интерфейс режима воспроизведения сопротивления калибратора. Двухпроводная схема.
    /// </summary>
    public interface IResistance2W
    {
        IResistance Resistance2W { get; }
        
    }
    public interface IResistance : ISourcePhysicalQuantity<Resistance>
    {
        ICommand []CompensationMode { get; set; }
        void SetCompensation(Compensation compMode);


    }
    /// <summary>
    /// Интерфейс режима воспроизведения сопротивления калибратора. Четырехпроводная схема.
    /// </summary>
    public interface IResistance4W
    {
        ISourcePhysicalQuantity<Resistance> Resistance4W { get;  }
    }
    public enum FlukeTypeTermocouple
    {
        [StringValue("B")] B,
        [StringValue("C")] C,
        [StringValue("E")] E,
        [StringValue("J")] J,
        [StringValue("K")] K,
        [StringValue("L")] L,
        [StringValue("N")] N,
        [StringValue("R")] R,
        [StringValue("S")] S,
        [StringValue("T")] T,
        [StringValue("X")] LinOut10mV,
        [StringValue("Y")] Humidity,
        [StringValue("Z")] LinOut1mV
    }
    public interface ITermocoupleType : ISourcePhysicalQuantity<Temperature>
    {
        ICommand[] TermoCoupleType { get; set; }
        void SetTermoCoupleType(FlukeTypeTermocouple flukeTypeTermocouple);
    }

    public interface ITemperature 
    {
        ITermocoupleType Temperature { get; }
    }
}