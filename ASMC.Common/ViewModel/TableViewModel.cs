using System.ComponentModel;
using System.Linq;
using ASMC.Core.ViewModel;
using ASMC.Data.Model;
using ASMC.Data.Model.Interface;
using ASMC.Data.Model.PhysicalQuantity;
using DevExpress.Mvvm;

namespace ASMC.Common.ViewModel
{
    public class TableViewModel : BindableBase, IItemTable
    {
        public TableViewModel()
        {
            Cells = new BindingList<ICell>();
            _cells.ListChanged += _cells_ListChanged;
        }

        private void _cells_ListChanged(object sender, ListChangedEventArgs e)
        {
            CountRow= Cells.AsQueryable().Max(q => q.RowIndex)+1;

            CountColumn = Cells.AsQueryable().Max(q => q.ColumnIndex)+1;
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

        /// <summary>
        ///     Создает VM
        /// </summary>
        /// <param name="name">Наименование таблицы</param>
        /// <param name="measPoints">Массив измерительных точек</param>
        /// <param name="setting"></param>
        /// <returns></returns>
        public static TableViewModel CreateTable<T>(string name, IMeasPoint<T>[] measPoints,
            SettingTableViewModel setting) where T: IPhysicalQuantity
        {
            var table = new TableViewModel { Header = name };
            var columnIndex = 0;
            var rowIndex = 0;
            foreach (var t in measPoints)
            {
                table.Cells.Add(new Cell
                {
                    ColumnIndex = columnIndex,
                    RowIndex = rowIndex,
                    Name = t.Description,
                    StringFormat = @"{0} " + setting?.CellFormat
                });
                if (setting.IsHorizontal)
                {
                    columnIndex++;
                    if (setting.Breaking == null) continue;
                    if (columnIndex % setting.Breaking != 0) continue;
                    rowIndex++;
                    columnIndex = 0;
                }
                else
                {
                    rowIndex++;
                    if (setting.Breaking == null) continue;
                    if (rowIndex % setting.Breaking != 0) continue;
                    columnIndex++;
                    rowIndex = 0;
                }
            }

            return table;
        }

        /// <summary>
        ///     Создает VM
        /// </summary>
        /// <param name="name">Наименование таблицы</param>
        /// <param name="measPoints">Массив измерительных точек</param>
        /// <param name="setting"></param>
        /// <returns></returns>
        public static TableViewModel CreateTable(string name, string[] measPoints,
            SettingTableViewModel setting)
        {
            var table = new TableViewModel { Header = name };
            var columnIndex = 0;
            var rowIndex = 0;
            foreach (var t in measPoints)
            {
                table.Cells.Add(new Cell
                {
                    ColumnIndex = columnIndex,
                    RowIndex = rowIndex,
                    Name = t,
                    StringFormat = @"{0} " + setting?.CellFormat
                });
                if (setting.IsHorizontal)
                {
                    columnIndex++;
                    if (setting.Breaking == null) continue;
                    if (columnIndex % setting.Breaking == 0)
                    {
                        rowIndex++;
                        columnIndex = 0;
                    }
                }
                else
                {
                    rowIndex++;
                    if (setting.Breaking == null) continue;
                    if (rowIndex % setting.Breaking == 0)
                    {
                        columnIndex++;
                        rowIndex = 0;
                    }
                }
            }

            return table;
        }

        public class SettingTableViewModel
        {
            #region Field

            /// <summary>
            ///     Рабите ячеек на столцы/строки в зависимости от <see cref="IsHorizontal" />
            /// </summary>
            public int? Breaking;

            /// <summary>
            ///     Форматирование яческий
            /// </summary>
            public string CellFormat;

            /// <summary>
            ///     Расположение ячеек горизонтальное
            /// </summary>
            public bool IsHorizontal = true;

            #endregion
        }
    }
}