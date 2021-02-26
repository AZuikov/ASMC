// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

using System;
using System.Collections.Generic;
using System.Linq;
using AP.Utils.Data;
using ASMC.Data.Model;
using ASMC.Data.Model.PhysicalQuantity;
using ASMC.Devices.Interface.SourceAndMeter;
using ASMC.Devices.Model;

namespace ASMC.Devices.IEEE.Fluke.Calibrator
{
    public class Calib5522A : CalibrMain, IResistance4W
    {
        public Calib5522A()
        {
            UserType = "Fluke 5522A";
            Resistance4W = new Resist4W(this);

        }

        public ISourcePhysicalQuantity<Resistance> Resistance4W { get; protected set; }

        protected override string GetError()
        {
            return "err?";
        }

        public ISourcePhysicalQuantity<Temperature> Temperature { get; }
        public class Resist4W : Resist
        {


            public Resist4W(CalibrMain device) : base(device)
            {
                RangeStorage = new RangeDevice();
                CompensationMode = new ICommand[]
                        {
                            new Command("ZCOMP WIRE4", "4х проводная компенсация", 4)
                        };

            }


            #region Methods

            protected override string GetUnit()
            {
                return "OHM";
            }

            public override void SetValue(MeasPoint<Resistance> value)
            {
                    base.SetValue(value);
                Calibrator.Device.WriteLine(CompensationMode.First().StrCommand);
                Calibrator.CheckErrors();
            }


            #endregion

            public class RangeDevice : RangeDeviceBase<Resistance>
            {
                [AccRange("Mode: Ohms 4W", typeof(MeasPoint<Resistance>))]
                public override RangeStorage<PhysicalRange<Resistance>> Ranges { get; set; }

            }
      
        }


        #region ErrosCatch



        #endregion ErrosCatch

        #region OldCode

        //public void SetVoltageDc(MeasPoint<Voltage> setPoint)
        //{
        //   //Out.Set.Voltage.Dc.SetValue(setPoint);

        //}

        //public void SetVoltageAc(MeasPoint<Voltage, Frequency> setPoint)
        //{
        //   //Out.Set.Voltage.Ac.SetValue(setPoint);
        //}

        //public void SetResistance2W(MeasPoint<Resistance> setPoint)
        //{
        //   //Out.Set.Resistance.SetValue(setPoint);
        //   //Out.Set.Resistance.SetCompensation(COut.CSet.CResistance.Zcomp.Wire2);

        //}

        //public void SetResistance4W(MeasPoint<Resistance> setPoint)
        //{
        //   //Out.Set.Resistance.SetValue(setPoint);
        //   //Out.Set.Resistance.SetCompensation(COut.CSet.CResistance.Zcomp.Wire4);
        //}

        //public void SetCurrentDc(MeasPoint<Current> setPoint)
        //{
        //   //Out.Set.Current.Dc.SetValue(setPoint);
        //}

        //public void SetCurrentAc(MeasPoint<Current, Frequency> setPoint)
        //{
        //   //Out.Set.Current.Ac.SetValue(setPoint);
        //}

        ////public void SetTemperature(MeasPoint<Temperature> setPoint, COut.CSet.СTemperature.TypeTermocouple typeTermocouple, string unitDegreas)
        ////{
        ////   //Out.Set.Temperature.SetTermoCoupleType(typeTermocouple);
        ////   //Out.Set.Temperature.SetValue(setPoint);
        ////}

        //public void SetOutputOn()
        //{
        //   //Out.SetOutput(COut.State.On);
        //}

        //public void SetOutputOff()
        //{
        //   //Out.SetOutput(COut.State.Off);
        //}

        //public void Reset()
        //{
        //    //WriteLine(IeeeBase.Reset);

        //}

        #endregion OldCode
    }
}