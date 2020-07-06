namespace ASMC.Devices.IEEE.Keysight.ElectronicLoad
{
    public class N3303A : Main_N3300
    {
        public N3303A(int chanNum) : base(chanNum)
        {
            ModuleModel = "N3303A";

            //Пределы воспроизведения сопротивлений в режиме CR
            this.rangeResistanceArr = new decimal[4] { 48, 480, 4800, 12000 };

            this.rangeVoltArr = new decimal[2] { 24, 240 };
            this.rangeCurrentArr = new decimal[2] { 1, 10 };

           

           
        }

        
    }
}
