using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AP.Extension;
using ASMC.Core.ViewModel;
using ASMC.Data.Model;
using ASMC.Devices.WithoutInterface.HourIndicator;

namespace Indicator_10.ViewModel
{
    public class MeasuringForceViewModel: TableViewModel
    {
        public int RowCount { get; }
        public Ich Ich { get; set; }
        public MeasuringForceViewModel(Ich ich)
        {
            Ich = ich;
            var arrPoints = Ich.Range.GetArayMeasPointsInParcent(0, 50, 100);


            for (int i = 0; i < arrPoints.Length; i++)
            {
                this.Cells.Add(new Cell { ColumnIndex = i, RowIndex = 0, Name = arrPoints[i].ToString() });
            }

            var reverse = arrPoints.Reverse().ToArray();
            for (int i = 0; i < arrPoints.Length; i++)
            {
                this.Cells.Add(new Cell { ColumnIndex = i + 1, RowIndex = 1, Name = reverse[i].ToString() });
            }

        }

    }
}
