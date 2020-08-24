using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AP.Utils.Helps;

namespace ASMC.Devices.WithoutInterface.Voltmetr
{
    public class B3_57:HelpDeviceBase
    {
        public double AnalogOutput3 { get; set; }
        public double AnalogOutput1 { get; set; }
        public ICommand[] Ranges { get; set; }


        public MeasureUnits[] MeasureUnits { get; } = {AP.Utils.Helps.MeasureUnits.Db, AP.Utils.Helps.MeasureUnits.V};

        public Multipliers[] MultipliersEnum { get; } = {AP.Utils.Helps.Multipliers.None, AP.Utils.Helps.Multipliers.Mili};

        public B3_57()
        {
            Multipliers = new ICommand[]
            {
                new Command("", "дБ", 1),
                new Command("", "В", 1),
                new Command("", "мВ", 1E-3)
                
            };
            Ranges = new[]
            {
                new Command("", "300", 300),
                new Command("", "100", 100)
            };


        }
    }
}
