namespace ASMC.Devices.IEEE.Keysight.ElectronicLoad
{
    public class N3306A : MainN3300
    {

        
        public N3306A() 
        {
            UserType = "N3306A";
            ModuleModel = UserType;

            this.ResistanceLoad.Ranges = new ICommand[]
            {
                new Command("RESistance:RANGe 1", "", 1), new Command("RESistance:RANGe 10", "", 10), new Command("RESistance:RANGe 100", "", 100), new Command("RESistance:RANGe 1000", "", 1000)
            };
            this.RangeVoltArr = new decimal[2] { 6, 60 };
            this.RangeCurrentArr = new decimal[2] { 12, 120 };
            
        }


        



        

        
              

        
    }
}