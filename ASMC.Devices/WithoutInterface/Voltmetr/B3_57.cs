using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AP.Utils.Helps;
using ASMC.Data.Model;

namespace ASMC.Devices.WithoutInterface.Voltmetr
{
    public class B3_57:HelpDeviceBase, UserType
    {
        public double AnalogOutput3 { get; set; }
        public double AnalogOutput1 { get; set; }
        public MeasPoint[] Ranges { get; set; }


        public MeasureUnits[] MeasureUnits { get; } = {AP.Utils.Helps.MeasureUnits.Db, AP.Utils.Helps.MeasureUnits.V};

        public Multipliers[] MultipliersEnum { get; } = {AP.Utils.Helps.Multipliers.None, AP.Utils.Helps.Multipliers.Mili};

        public B3_57()
        {
            UserType = "В3-57";
            Multipliers = new ICommand[]
            {
                new Command("", "дБ", 1),
                new Command("", "В", 1),
                new Command("", "мВ", 1E-3)
                
            };

            Ranges = new MeasPoint[15];
            Ranges[0] = new MeasPoint(MeasureUnits[1], MultipliersEnum[0], 300);
            Ranges[1] = new MeasPoint(MeasureUnits[1], MultipliersEnum[0], 100);
            Ranges[2] = new MeasPoint(MeasureUnits[1], MultipliersEnum[0], 30);
            Ranges[3] = new MeasPoint(MeasureUnits[1], MultipliersEnum[0], 10);
            Ranges[4] = new MeasPoint(MeasureUnits[1], MultipliersEnum[0], 3);
            Ranges[5] = new MeasPoint(MeasureUnits[1], MultipliersEnum[0], 1);
            Ranges[6] = new MeasPoint(MeasureUnits[1], MultipliersEnum[1], 300);
            Ranges[7] = new MeasPoint(MeasureUnits[1], MultipliersEnum[1], 100);
            Ranges[8] = new MeasPoint(MeasureUnits[1], MultipliersEnum[1], 30);
            Ranges[9] = new MeasPoint(MeasureUnits[1], MultipliersEnum[1], 10);
            Ranges[10] = new MeasPoint(MeasureUnits[1], MultipliersEnum[1], 3);
            Ranges[11] = new MeasPoint(MeasureUnits[1], MultipliersEnum[1], 1);
            Ranges[12] = new MeasPoint(MeasureUnits[1], MultipliersEnum[1], (decimal) 0.3);
            Ranges[13] = new MeasPoint(MeasureUnits[1], MultipliersEnum[1], (decimal) 0.1);
            Ranges[14] = new MeasPoint(MeasureUnits[1], MultipliersEnum[1], (decimal) 0.03);
        }

        
        /// <inheritdoc />
        public string UserType { get; }

    }

   
}
