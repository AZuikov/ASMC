﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AP.Utils.Data;
using ASMC.Data.Model;
using ASMC.Data.Model.PhysicalQuantity;
using ASMC.Devices.Interface;
using ASMC.Devices.Interface.SourceAndMeter;

namespace ASMC.Devices.IEEE.Fluke.Calibrator
{
    public class Calibr_9100: IeeeBase, IVoltageGroupForCalibrator, ICurrnetGroupForCalibrator, IResistance2W, ICapacity, ITemperature, IProtocolStringLine, IResistance4W
    {
        public Calibr_9100()
        {
            UserType = "Fluke 9100";
            //Resistance4W = 
        }


        public ISourcePhysicalQuantity<Voltage> DcVoltage { get; }
        public ISourcePhysicalQuantity<Voltage, Frequency> AcVoltage { get; }
        public ISourcePhysicalQuantity<Current> DcCurrent { get; }
        public ISourcePhysicalQuantity<Current, Frequency> AcCurrent { get; }
        public IResistance Resistance2W { get; }
        public ISourcePhysicalQuantity<Capacity> Capacity { get; }
        public ITermocoupleType Temperature { get; }
        public ISourcePhysicalQuantity<Frequency, Voltage> Frequency { get; }
        public ISourcePhysicalQuantity<Resistance> Resistance4W { get; }
    }
}

/* Remove this code!!!!!!
 public void SetVoltageDc(MeasPoint<Voltage> setPoint)
        {
            //WriteLine("Source:func DC");
            //WriteLine($"Source:VOLT {setPoint.MainPhysicalQuantity.GetNoramalizeValueToSi().ToString().Replace(',','.')}");
            
        }

        public void SetVoltageAc(MeasPoint<Voltage, Frequency> setPoint)
        {
            //WriteLine("Source:func sin");
            //WriteLine($"Source:VOLT {setPoint.MainPhysicalQuantity.GetNoramalizeValueToSi().ToString().Replace(',', '.')}");
            //WriteLine($"Source:freq {setPoint.AdditionalPhysicalQuantity.GetNoramalizeValueToSi().ToString().Replace(',', '.')}");
        }

        public void SetResistance2W(MeasPoint<Resistance> setPoint)
        {
            //WriteLine("Source:func DC");
            //WriteLine($"Source:RESistance {setPoint.MainPhysicalQuantity.GetNoramalizeValueToSi().ToString().Replace(',', '.')}");
            //WriteLine("SOUR:RES:UUT_I SUP");
            //WriteLine("outp:comp off");
        }

        public void SetResistance4W(MeasPoint<Resistance> setPoint)
        {
            //WriteLine("Source:func DC");
            //WriteLine($"Source:RESistance {setPoint.MainPhysicalQuantity.GetNoramalizeValueToSi().ToString().Replace(',', '.')}");
            //WriteLine("SOUR:RES:UUT_I SUPer}");
            //WriteLine("outp:comp on");
        }

        public void SetCurrentDc(MeasPoint<Current> setPoint)
        {
            //WriteLine("Source:func DC");
            //WriteLine("Source:CURRent {setPoint.MainPhysicalQuantity.GetNoramalizeValueToSi().ToString().Replace(',', '.')}");
        }

        public void SetCurrentAc(MeasPoint<Current, Frequency> setPoint)
        {
            //WriteLine("Source:func sin");
            //WriteLine($"Source:CURRent {setPoint.MainPhysicalQuantity.GetNoramalizeValueToSi().ToString().Replace(',', '.')}");
            //WriteLine($"Source:freq {setPoint.AdditionalPhysicalQuantity.GetNoramalizeValueToSi().ToString().Replace(',', '.')}");
        }

        //public void SetTemperature(MeasPoint<Temperature> setPoint, COut.CSet.СTemperature.TypeTermocouple typeTermocouple, string unit)
        //{
        //    //WriteLine("Source:func DC");
        //    //WriteLine($"Source:TEMPerature:THERmocouple:TYPE {typeTermocouple.GetStringValue()}");
        //    //WriteLine($"Source:TEMPerature:UNITs {unit}");
        //    //WriteLine($"Source:TEMPerature:THERmocouple {setPoint.MainPhysicalQuantity.GetNoramalizeValueToSi().ToString().Replace(',', '.')}");
        //}

        //public void SetOutputOn()
        //public void SetOutputOn()
        //public void SetOutputOn()
        //public void SetOutputOn()
        //public void SetOutputOn()ICalibratorMultimeterFluke
        //{
        //   //WriteLine("outp:stat on");
        //}

        public void SetOutputOff()
        {
            //WriteLine("outp:stat off");
        }

        public void Reset()
        {
            //WriteLine(IeeeBase.Reset);
        }

        protected override string GetError()
        {
            throw new NotImplementedException();
        }
 *
 *
 */
