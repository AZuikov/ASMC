using ASMC.Data.Model;
using ASMC.Data.Model.PhysicalQuantity;
using ASMC.Devices.IEEE.Fluke.Calibrator;

namespace ASMC.Devices.Interface
{
    public interface ICalibrator: IDevice
    {
    void SetVoltageDc(MeasPoint<Voltage> setPoint);

    void SetVoltageAc(MeasPoint<Voltage, Frequency> setPoint);

    void SetResistance2W(MeasPoint<Resistance> setPoint);
    void SetResistance4W(MeasPoint<Resistance> setPoint);

    void SetCurrentDc(MeasPoint<Current> setPoint);

    void SetCurrentAc(MeasPoint<Current, Frequency> setPoint);

    void SetTemperature(MeasPoint<Temperature> setPoint,
        CalibrMain.COut.CSet.СTemperature.TypeTermocouple typeTermocouple, string unitDegreas);

    void SetOutputOn();

    void SetOutputOff();

    void Reset();
    bool IsEnableOutput { get; set; }
    }
}