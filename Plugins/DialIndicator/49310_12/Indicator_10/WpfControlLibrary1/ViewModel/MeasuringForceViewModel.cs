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
            this.Content.CollectionChanged += Content_CollectionChanged;
        }

        private void Content_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (Content.Count <= 2) return;

            ((Cell)Content[0].Cells[1]).PropertyChanged += MeasuringForceViewModel_PropertyChanged;
            ((Cell)Content[1].Cells[1]).PropertyChanged += ReversMeasuringForceViewModel_PropertyChanged;

        }

        private void ReversMeasuringForceViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Cell.Value))
            {
                this.Content[2].Cells[1].Value = ((Cell)sender).Value;
            }
        }

        private void MeasuringForceViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName==nameof(Cell.Value))
            {
                this.Content[2].Cells[0].Value = ((Cell) sender).Value;
            }
        }

        /// <inheritdoc />
        protected override bool CanSelect()
        {
            if (Content.Count <= 2) return false;
            return Content.All(q => q.Cells.All(p => !string.IsNullOrWhiteSpace(p?.Value?.ToString())));
        }
    }
}