namespace ASMC.Devices.Port.Profigrupp
{
    /// <summary>
    /// Класс для блока питания модели Б5-71/2-ПРО
    /// производства ООО "Профигрупп"
    /// </summary>
    public class B571Pro2 : B571Pro
    {
        public B571Pro2()
        {
            VoltMax = 50;
            CurrMax = 6;
            
        }

        public B571Pro2(string PortName) : base(PortName)
        {
        }



    }
}
