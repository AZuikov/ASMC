using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASMC.Devices.WithoutInterface.Voltmetr
{
    public class B5_57:HelpDeviceBase
    {
        public double AnalogOutput3 { get; set; }
        public double AnalogOutput1 { get; set; }
        public ICommand[] Ranges { get; set; }
        public B5_57()
        {
            Ranges = new[]
            {
                new Command("", "300", 300),
                new Command("", "100", 100)
            };


        }
    }
}
