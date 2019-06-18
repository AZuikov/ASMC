using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using DevExpress.Mvvm;

namespace ASMC.Core.ViewModel
{
    public abstract class BaseSelectorViewModel : BaseViewModel
    {
        #region Fields

        private DataTable _data;
        private DataRow _currentRow;

        #endregion

        #region Properties

        /// <summary>
        /// Возвращает данные, полученные из
        /// связанного источника данных.
        /// </summary>
        public DataTable Data
        {
            get => _data;
            protected set => SetProperty(ref _data, value, nameof(Data));
        }

        /// <summary>
        /// Возвращает или задает
        /// текущую запись данных.
        /// </summary>
        public DataRow CurrentRow
        {
            get => _currentRow;
            set => SetProperty(ref _currentRow, value, nameof(CurrentRow), OnCurrentRowChanged);
        }

        /// <summary>
        /// Возвращает команду добавления
        /// новой записи.
        /// </summary>
        public ICommand AddRecordCommand
        {
            get;
        }

        /// <summary>
        /// Возвращает команду редактирования
        /// существующей записи.
        /// </summary>
        public ICommand EditRecordCommand
        {
            get;
        }

        /// <summary>
        /// Возвращает команду удаления
        /// существующей записи.
        /// </summary>
        public ICommand RemoveRecordCommand
        {
            get;
        }

        /// <summary>
        /// Возвращает команду обновления
        /// из связанного источника данных.
        /// </summary>
        public ICommand RefreshCommand
        {
            get;
        }

        #endregion

        /// <summary>
        /// Инициализирует новый экземпляр класса
        /// <see cref="BaseSelectorViewModel"/>.
        /// </summary>
        protected BaseSelectorViewModel()
        {
            AddRecordCommand = new DelegateCommand(AddRecord, CanAddRecord);
            EditRecordCommand = new DelegateCommand<object>(p => EditRecord((p as DataRowView)?.Row), p => CanEditRecord((p as DataRowView)?.Row));
            RemoveRecordCommand = new DelegateCommand<object>(p => RemoveRecord((p as DataRowView)?.Row), p => CanRemoveRecord((p as DataRowView)?.Row));
            RefreshCommand = new AsyncCommand(RefreshAsync, CanRefresh);
        }

        #region Methods

        /// <inheritdoc />
        protected override void OnInitialized()
        {
            base.OnInitialized();
            RefreshAsync();
        }

        protected virtual void OnCurrentRowChanged()
        {

        }

        /// <summary>
        /// Вызывается при запросе состояния
        /// команды <see cref="AddRecordCommand"/>.
        /// </summary>
        /// <returns>Возвращает истинно, если
        /// команда <see cref="AddRecordCommand"/>
        /// может быть выполнена; иначе ложно.</returns>
        protected virtual bool CanAddRecord()
        {
            return !IsBusy && Data != null;
        }

        /// <summary>
        /// Добавляет новую запись.
        /// </summary>
        public abstract void AddRecord();

        /// <summary>
        /// Вызывается при запросе состояния
        /// команды <see cref="EditRecordCommand"/>.
        /// </summary>
        /// <returns>Возвращает истинно, если
        /// команда <see cref="EditRecordCommand"/>
        /// может быть выполнена; иначе ложно.</returns>
        protected virtual bool CanEditRecord(DataRow dataRow)
        {
            return !IsBusy && dataRow != null;
        }

        /// <summary>
        /// Вызывает редактирование существующей записи.
        /// </summary>
        /// <param name="dataRow"></param>
        public abstract void EditRecord(DataRow dataRow);

        /// <summary>
        /// Вызывается при запросе состояния
        /// команды <see cref="RemoveRecordCommand"/>.
        /// </summary>
        /// <returns>Возвращает истинно, если
        /// команда <see cref="RemoveRecordCommand"/>
        /// может быть выполнена; иначе ложно.</returns>
        protected virtual bool CanRemoveRecord(DataRow dataRow)
        {
            return !IsBusy && dataRow != null;
        }

        /// <summary>
        /// Удаляет существующую запись.
        /// </summary>
        /// <param name="dataRow"></param>
        public abstract void RemoveRecord(DataRow dataRow);

        /// <summary>
        /// Вызывается при запросе состояния
        /// команды <see cref="RefreshCommand"/>.
        /// </summary>
        /// <returns>Возвращает истинно, если
        /// команда <see cref="RefreshCommand"/>
        /// активна; иначе ложно.</returns>
        protected virtual bool CanRefresh()
        {
            return !IsBusy;
        }

        /// <summary>
        /// Обновляет данные из связанного
        /// источника данных.
        /// </summary>
        public abstract void Refresh();

        /// <summary>
        /// Выполняет асинхронное обновление данных
        /// из связанного источника данных.
        /// </summary>
        public abstract Task RefreshAsync();

        #endregion
    }
}
