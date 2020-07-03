namespace ASMC.Devises.IEEE.Keysight.ElectronicLoad
{
    public class N3306A : Main_N3300
    {

        
        public N3306A(int chanNum) : base(chanNum)
        {
            ModuleModel = "N3306A";

            //Пределы воспроизведения сопротивлений в режиме CR
            this.rangeResistanceArr = new decimal[4] { 1, 10, 100, 1000 };

            this.rangeVoltArr = new decimal[2] { 6, 60 };
            this.rangeCurrentArr = new decimal[2] { 12, 120 };
        }


        



        

        
              

        
    }
}