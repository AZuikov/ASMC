using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using AP.Extension;
using ASMC.Common.ViewModel;
using ASMC.Core.ViewModel;
using ASMC.Data.Model;
using ASMC.Devices.WithoutInterface.HourIndicator;
using DevExpress.Mvvm;

namespace Indicator_10.ViewModel
{
    public class MeasuringForceViewModel : MultiGridViewModel
    {
        public MeasuringForceViewModel()
        {
            this.Content[0].Cells.ListChanged += Cells_ListChanged;
            this.Content[1].Cells.ListChanged += Cells_ListReversChanged;
        }

        private void Cells_ListReversChanged(object sender, ListChangedEventArgs e)
        {
            if (e.ListChangedType == ListChangedType.ItemChanged)
            {
                this.Content[2].Cells[0].Value = sender;
            }
        }

        private void Cells_ListChanged(object sender, System.ComponentModel.ListChangedEventArgs e)
        {
            if (e.ListChangedType == ListChangedType.ItemChanged)
            {
                this.Content[2].Cells[1].Value = sender;
            }
          
        }
    }
}