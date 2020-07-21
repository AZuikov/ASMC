using System.Text.RegularExpressions;

namespace ASMC.Devices.Port.Profigrupp
{

    /// <summary>
    /// Класс для блока питания модели Б5-71/1-ПРО
    /// производства ООО "Профигрупп"
    /// </summary>
    public class B571Pro1 : B5_71_PRO
    {
        public B571Pro1()
        {
            DeviceType = "Б5-71/1-ПРО";
            VoltMax = 30;
            CurrMax = 10;
            //погрешность для нестабильности по напряжению
            tolleranceVoltageUnstability = (decimal)0.001 * VoltMax + (decimal)0.02;
            //погрешность для нестабильности по току
            tolleranceCurrentUnstability = (decimal)0.001 * CurrMax + (decimal)0.05;
            //пульсации по апряжению
            tolleranceVoltPuls = 2;
            //пульсации по току
            tolleranceCurrentPuls = 5;
        }

        public B571Pro1(string portName) : base(portName)
        {
           
        }  
    }
}