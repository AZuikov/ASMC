namespace ASMC.Devices.Port.Profigrupp
{
    /// <summary>
    /// Класс для блока питания модели Б5-71/2-ПРО
    /// производства ООО "Профигрупп"
    /// </summary>
    public class B571Pro2 : B5_71_PRO
    {
        public B571Pro2()
        {
            VoltMax = 50;
            CurrMax = 6;
            //погрешность для нестабильности по напряжению
            tolleranceVoltageUnstability = (decimal)0.001 * VoltMax + (decimal)0.02;
            //погрешность для нестабильности по току
            tolleranceCurrentUnstability = (decimal)0.001 * CurrMax + (decimal)0.05;
            tolleranceVoltPuls = 2;
            //пульсации по току
            tolleranceCurrentPuls = 5;
        }

        public B571Pro2(string PortName) : base(PortName)
        {
        }



    }
}
