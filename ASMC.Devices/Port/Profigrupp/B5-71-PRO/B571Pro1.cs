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
           
        }

        public B571Pro1(string PortName) : base(PortName)
        {
           
        }


       
    }
}