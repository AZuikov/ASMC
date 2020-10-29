using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using ASMC.Common.ViewModel;

namespace Indicator_10.ViewModel
{
    public class MeasuringForceViewModel : MultiGridViewModel
    {
        public MeasuringForceViewModel()
        {
            Content.CollectionChanged += Content_CollectionChanged;
        }

        private void Content_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (Content.Count <= 2) return;

            ((Cell)Content[0].Cells[1]).PropertyChanged += MeasuringForceViewModel_PropertyChanged;
            ((Cell)Content[1].Cells[1]).PropertyChanged += ReversMeasuringForceViewModel_PropertyChanged;

        }

        private void ReversMeasuringForceViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Cell.Value))
            {
                Content[2].Cells[1].Value = ((Cell)sender).Value;
            }
        }

        private void MeasuringForceViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName==nameof(Cell.Value))
            {
                Content[2].Cells[0].Value = ((Cell) sender).Value;
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