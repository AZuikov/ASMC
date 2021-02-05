// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

using System.Collections.Generic;
using System.Runtime.CompilerServices;
using AP.Utils.Data;
using ASMC.Data.Model;
using ASMC.Data.Model.PhysicalQuantity;
using ASMC.Devices.IEEE.Keysight.Multimeter;
using ASMC.Devices.Interface;
using ASMC.Devices.Interface.SourceAndMeter;
using Current = ASMC.Data.Model.PhysicalQuantity.Current;

namespace ASMC.Devices.IEEE.Fluke.Calibrator
{
    public class Calib5522A : CalibrMain , IResistance4W
    {
       

        public Calib5522A()
        {
            UserType = "Fluke 5522A";
            Resistance4W = new Resist4W(Device);
        }
       
        public ISourcePhysicalQuantity<Resistance> Resistance4W { get; protected set; }

        public class Resist4W : SimplyPhysicalQuantity<Resistance>
        {
            public enum Zcomp
            {
                /// компенсация 4х проводная
                /// </summary>
                [StringValue("ZCOMP WIRE4")] Wire4
            }
            
            public Resist4W(IeeeBase device) : base(device)
            {
            }

            protected override string GetUnit()
            {
                return "OHM";
            }

            public override void SetValue(MeasPoint<Resistance> value)
            {
                base.SetValue(value);
                _calibrMain.WriteLine(GetCompensationString(Zcomp.Wire4));
                CheckErrors();
            }
        }


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


        #endregion
    }


}