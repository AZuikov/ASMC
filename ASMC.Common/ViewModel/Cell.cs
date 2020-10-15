using ASMC.Data.Model.Interface;
using DevExpress.Mvvm;

namespace ASMC.Common.ViewModel
{
    public class Cell : BindableBase, ICell
    {
        #region Fields

        private object _value;

        #endregion

        /// <inheritdoc />
        public string StringFormat { get; set; }

        /// <inheritdoc />
        public string Name { get; set; }

        /// <inheritdoc />
        public object Value
        {
            get => _value ;
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