using System.ComponentModel;
using System.Linq;
using ASMC.Data.Model.Interface;
using DevExpress.Mvvm;

namespace ASMC.Common.ViewModel
{
    public class TableViewModel : BindableBase, ITemTable
    {
        public TableViewModel()
        {
            Cells = new BindingList<ICell>();
            _cells.ListChanged += _cells_ListChanged;
        }

        private void _cells_ListChanged(object sender, ListChangedEventArgs e)
        {
            CountRow= Cells.AsQueryable().Max(q => q.RowIndex)+1;

            CountColumn = Cells.AsQueryable().Max(q => q.RowIndex)+1;
        }

        #region Fields

        private ICell _selected;
        private BindingList<ICell> _cells;
        private string _header;
        private int _countRow;
        private int _countColumn;

        #endregion

        #region Property

        public int CountColumn
        {
            get => _countColumn;
            set => SetProperty(ref _countColumn, value, nameof(CountColumn));
        }
        public int CountRow
        {
            get => _countRow;
            set => SetProperty(ref _countRow, value, nameof(CountRow));
        }

        public ICell Selected
        {
            get => _selected;
            set => SetProperty(ref _selected, value, nameof(Selected));
        }

        /// <inheritdoc />
        public string Header
        {
            get => _header;
            set => SetProperty(ref _header, value, nameof(Header));
        }

        public BindingList<ICell> Cells
        {
            get => _cells;
            set => SetProperty(ref _cells, value, nameof(Cells));
        }

        #endregion

    }
}