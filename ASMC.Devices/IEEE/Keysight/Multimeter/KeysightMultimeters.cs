using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AP.Utils.Data;
using ASMC.Devices.Interface;

namespace ASMC.Devices.IEEE.Keysight.Multimeter
{
    public class Keysight34401A: BaseDigitalMultimetr344xx, IFrontRearPanel
    {
        public Keysight34401A()
        {
            UserType = "34401A";
        }

        
        
        /// <inheritdoc />
        public bool IsFrontTerminal
        {
            get => IsFrontTerminalActive();
        }
        
        protected enum Terminals344xx
        {
            [StringValue("FRON")]Front,
            [StringValue("REAR")]Rear
        }
        protected bool IsFrontTerminalActive()
        {
           
           string answer = _device.QueryLine("ROUT:TERM?");
           return answer.Equals(Terminals344xx.Front.GetStringValue());
        }
    }
}
