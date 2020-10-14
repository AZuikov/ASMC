using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AP.Extension;
using ASMC.Core.ViewModel;
using ASMC.Data.Model;
using ASMC.Devices.WithoutInterface.HourIndicator;

namespace Indicator_10.ViewModel
{
    public class MeasuringForceViewModel: ASMC.Common.ViewModel.FromBaseViewModel
    {
        public ObservableCollection<ITemTable> Content { get; } = new ObservableCollection<ITemTable>();
        public IchBase IchBase { get; set; }
        public MeasuringForceViewModel(IchBase ichBase)
        {
            this.
            IchBase = ichBase;
            var arrPoints = IchBase.Range.GetArayMeasPointsInParcent(0, 50,100);
            Content.Add(new TableViewModel{Header= "Прямой ход"});
            Content.Add(new TableViewModel { Header = "Обратный ход" });
            Content.Add(new TableViewModel { Header = "Прямой/обатный ход" });
            Content.Add(new TableViewModel { Header = "Прямой/обатный ход1" });
            Content.Add(new TableViewModel { Header = "Прямой/обатный ход2" });

            for (int i = 0; i < arrPoints.Length; i++)
            {
                Content[0].Cells.Add(new Cell { ColumnIndex = 0, RowIndex = i, Name = arrPoints[i].ToString() });
            }

            var reverse = arrPoints.Reverse().ToArray();
            for (int i = 0; i < arrPoints.Length; i++)
            {
                this.Content[1].Cells.Add(new Cell { ColumnIndex = 0, RowIndex = i, Name = reverse[i].ToString() });
            }
            this.Content[2].Cells.Add(new Cell { ColumnIndex = 0, RowIndex = 0, Name = arrPoints[1].ToString() });
            this.Content[2].Cells.Add(new Cell { ColumnIndex = 0, RowIndex = 1, Name = arrPoints[1].ToString() });

        }

    }
}
