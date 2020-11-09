using System.Linq;
using ASMC.Core.ViewModel;

namespace ASMC.Common.ViewModel
{
    public class OneTableViewModel: SelectionViewModel
    {
        private TableViewModel _data;

        public TableViewModel Data
        {
            get => _data;
            set => SetProperty(ref _data, value, nameof(Data));
        }

        /// <inheritdoc />
        protected override bool CanSelect()
        {
            return Data.Cells.All(p => !string.IsNullOrWhiteSpace(p?.Value?.ToString()));
        }
    }
}