using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ASMC.Data.Model;
using ASMC.Data.Model.PhysicalQuantity;

namespace ASMC.Devices.IEEE.Fluke.Calibrator
{
    interface ICalibrator
    {
        void SetVoltageDc(MeasPoint<Voltage> setPoint);
        void SetVoltageAc(MeasPoint<Voltage, Frequency> setPoint);
        void SetResistance(MeasPoint<Resistance> setPoint);
        void SetCurrentDc (MeasPoint<Current> setPoint);
        void SetCurrentAc(MeasPoint<Current, Frequency> setPoint);
        void SetTemperature(MeasPoint<Temperature> setPoint, CalibrMain.COut.CSet.СTemperature.TypeTermocouple typeTermocouple);
    }
}
