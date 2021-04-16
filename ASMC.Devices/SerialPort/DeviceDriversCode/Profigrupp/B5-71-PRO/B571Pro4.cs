namespace ASMC.Devices.Port.Profigrupp
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
            
        }

        public B571Pro4(string PortName) : base(PortName)
        {
        }
    }
}
