namespace ASMC.Devices.Port.Profigrupp
{

    /// <summary>
    /// Класс для блока питания модели Б5-71/1-ПРО
    /// производства ООО "Профигрупп"
    /// </summary>
    public class B571Pro1 : B571Pro
    {
        public B571Pro1()
        {
            UserType = "Б5-71/1-ПРО";
            VoltMax = 30;
            CurrMax = 10;
            //погрешность для нестабильности по напряжению
            TolleranceVoltageUnstability = (decimal)0.001 * VoltMax + (decimal)0.02;
            //погрешность для нестабильности по току
            TolleranceCurrentUnstability = (decimal)0.001 * CurrMax + (decimal)0.05;
            //пульсации по апряжению
            TolleranceVoltPuls = 2;
            //пульсации по току
            TolleranceCurrentPuls = 5;
        }

        public B571Pro1(string PortName) : base(PortName)
        {
           
        }


       
    }
}