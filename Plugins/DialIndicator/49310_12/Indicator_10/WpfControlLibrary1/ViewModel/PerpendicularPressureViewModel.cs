using System.Linq;
using AP.Extension;
using ASMC.Common.ViewModel;
using ASMC.Core.ViewModel;
using ASMC.Devices.WithoutInterface.HourIndicator;

namespace Indicator_10.ViewModel
{
    public class PerpendicularPressureViewModel: MultiGridViewModel
    {
        public int RowCount { get; }
        public IchBase IchBase { get; set; }
        public PerpendicularPressureViewModel(IchBase ichBase)
        {
            //IchBase = ichBase;
            //var arrPoints = IchBase.Range.GetArayMeasPointsInParcent(50);
            //for (int i = 0; i < arrPoints.Length; i++)
            //{
            //    this.Cells.Add(new Cell { ColumnIndex = i, RowIndex = 0, Name = arrPoints[i].ToString() });
            //}

            //var reverse = arrPoints.Reverse().ToArray();
            //for (int i = 0; i < arrPoints.Length; i++)
            //{
            //    this.Cells.Add(new Cell { ColumnIndex = i + 1, RowIndex = 1, Name = reverse[i].ToString() });
            //}

        }

    }
}