using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Timers;
using ASMC.Common.ViewModel;
using ASMC.Data.Model;
using ASMC.Data.Model.Interface;
using DevExpress.Mvvm;

namespace ASMC.Core.ViewModel
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

    public interface ITemTable:INotifyPropertyChanged
    {
        string Header { get; set; }
        
            BindingList<ICell> Cells { get; }
        ICell Selected { get; set; }
    }
    public class Cell : BindableBase, ICell
    {
        #region Fields

        private object _value;

        #endregion

        /// <inheritdoc />
        public string Name { get; set; }

        /// <inheritdoc />
        public object Value
        {
            get => _value;
            set => SetProperty(ref _value, value, nameof(Value));
        }

        /// <inheritdoc />
        public string Description { get; }

        /// <inheritdoc />
        public int RowIndex { get; set; }

        /// <inheritdoc />
        public int ColumnIndex { get; set; }
    }
}