using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ASMC.Data.Model;
using ASMC.Data.Model.PhysicalQuantity;

namespace ASMC.Devices.WithoutInterface.Voltmetr
{
    public class B3_57:HelpDeviceBase, IUserType
    {
        public double AnalogOutput3 { get; set; }
        public double AnalogOutput1 { get; set; }
        public MeasPoint<Voltage>[] Ranges { get; set; }


        public MeasureUnits[] MeasureUnits { get; } = {Data.Model.MeasureUnits.Db, Data.Model.MeasureUnits.V};

        public UnitMultiplier[] MultipliersEnum { get; } = {UnitMultiplier.None, UnitMultiplier.Mili};

        public B3_57()
        {
            UserType = "В3-57";
            Multipliers = new ICommand[]
            {
                new Command("", "дБ", 1),
                new Command("", "В", 1),
                new Command("", "мВ", 1E-3)
                
            };

            Ranges = new MeasPoint<Voltage>[15];
            Ranges[0] = new MeasPoint<Voltage>(300);
            Ranges[1] = new MeasPoint<Voltage>(100);
            Ranges[2] = new MeasPoint<Voltage>(30);
            Ranges[3] = new MeasPoint<Voltage>(10);
            Ranges[4] = new MeasPoint<Voltage>(3);
            Ranges[5] = new MeasPoint<Voltage>(1);
            Ranges[6] = new MeasPoint<Voltage>(300, UnitMultiplier.Mili);
            Ranges[7] = new MeasPoint<Voltage>(100, UnitMultiplier.Mili);
            Ranges[8] = new MeasPoint<Voltage>(30, UnitMultiplier.Mili);
            Ranges[9] = new MeasPoint<Voltage>(10, UnitMultiplier.Mili);  
            Ranges[10] = new MeasPoint<Voltage>(3, UnitMultiplier.Mili);
            Ranges[11] = new MeasPoint<Voltage>(1,UnitMultiplier.Mili);
            Ranges[12] = new MeasPoint<Voltage>((decimal) 0.3,UnitMultiplier.Mili);
            Ranges[13] = new MeasPoint<Voltage>((decimal) 0.1, UnitMultiplier.Mili);
            Ranges[14] = new MeasPoint<Voltage>((decimal) 0.03, UnitMultiplier.Mili);
        }

        
        /// <inheritdoc />
        public string UserType { get; }

    }

   
}
