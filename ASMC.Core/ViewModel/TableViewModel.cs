using System.ComponentModel;
using System.Timers;
using ASMC.Common.ViewModel;
using ASMC.Data.Model;
using ASMC.Data.Model.Interface;
using DevExpress.Mvvm;

namespace ASMC.Core.ViewModel
{
    public class TableViewModel : BaseViewModel
    {
        public TableViewModel()
        {
            Cells = new BindingList<ICell>();
        }
        #region Fields

        private ICell _selected;
        private BindingList<ICell> _cells;


        #endregion

        #region Property

        public ICell Selected
        {
            get => _selected;
            set => SetProperty(ref _selected, value, nameof(Selected));
        }

        public BindingList<ICell> Cells
        {
            get => _cells;
            set => SetProperty(ref _cells, value, nameof(Cells));
        }

        #endregion

    }

    public class Cell : BindableBase, ICell
    {
        #region Fields

        private readonly MeasPoint _measPoint = new MeasPoint();
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