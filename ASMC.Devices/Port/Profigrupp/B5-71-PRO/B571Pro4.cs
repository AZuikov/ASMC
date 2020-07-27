﻿namespace ASMC.Devices.Port.Profigrupp
{

    /// <summary>
    /// Класс для блока питания модели Б5-71/4-ПРО
    /// производства ООО "Профигрупп"
    /// </summary>
    public class B571Pro4 : B571Pro
    {
        public B571Pro4()
        {
            UserType = "Б5-71/4-ПРО";
            VoltMax = 75;
            CurrMax = 4;
            //погрешность для нестабильности по напряжению
            TolleranceVoltageUnstability = (decimal)0.001 * VoltMax + (decimal)0.02;
            //погрешность для нестабильности по току
            TolleranceCurrentUnstability = (decimal)0.001 * CurrMax + (decimal)0.05;
            TolleranceVoltPuls = 2;
            //пульсации по току
            TolleranceCurrentPuls = 5;
        }

        public B571Pro4(string PortName) : base(PortName)
        {
        }
    }
}
