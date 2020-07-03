using System;
using System.Windows;
using System.Windows.Input; 
using DevExpress.Mvvm;

namespace ASMC.Core.ViewModel
{
    public class DialogBaseContent : BaseViewModel, ISupportDialog, ISupportClose
    {
        #region Fields

        private bool _isSubmit;
        private bool? _dialogResult;

        private readonly object _syncRoot = new object();

        #endregion

        public event EventHandler RequestClose;

        public event EventHandler Submitted;

        #region Properties

        public bool IsSubmit
        {
            get => _isSubmit;
            protected set
            {
                lock(_syncRoot)
                {
                    if(SetProperty(ref _isSubmit, value, nameof(IsSubmit)))
                        CommandManager.InvalidateRequerySuggested();
                }
            }
        }

        /// <summary>
        /// Возвращает или задает результат,
        /// возращаемый при закрытии диалога
        /// </summary>
        public bool? DialogResult
        {
            get => _dialogResult;
            protected set => SetProperty(ref _dialogResult, value, () => DialogResult);
        }

        /// <summary>
        /// Возвращает команду для
        /// обновления данных
        /// </summary>
        public ICommand RefreshCommand
        {
            get;
        }

        /// <summary>
        /// Возвращает команду для
        /// подтверждения изменений
        /// </summary>
        public ICommand SubmitCommand
        {
            get;
        }

        /// <summary>
        /// Возвращает команду для
        /// отмены изменений
        /// </summary>
        public ICommand CancelCommand
        {
            get;
        }

        /// <summary>
        /// Возвращает команду запроса
        /// на закрытие свзяанного
        /// представления.
        /// </summary>
        public ICommand CloseCommand
        {
            get;
        }

        #endregion

        protected DialogBaseContent()
        {
            RefreshCommand = new DelegateCommand(OnRefreshCommand, CanRefreshCommand);
            SubmitCommand = new DelegateCommand(OnSubmitCommand, CanSubmitCommand);
            CancelCommand = new DelegateCommand(OnCancelCommand, CanCancelCommand);
            CloseCommand = new DelegateCommand(OnCloseCommand, CanCloseCommand);
        }

        #region Methods

        public virtual bool CanClose(object state)
        {
            return !IsBusy;
        }

        public void Close()
        {
            if(CanClose(null))
                RaiseRequestClose();
        }

        protected bool Alert(string message, string additionalText = null, bool isError = true)
        {
            var srv = GetService<IMessageBoxService>();
            return srv?.Show(message, "Ошибка", MessageBoxButton.OK,
                       isError ? MessageBoxImage.Error : MessageBoxImage.Exclamation) != MessageBoxResult.None;
        }

        protected virtual bool CanRefreshCommand()
        {
            return !IsBusy && !IsSubmit;
        }

        protected virtual void OnRefreshCommand()
        {
        }

        protected virtual bool CanSubmitCommand()
        {
            return !IsBusy && !IsSubmit;
        }

        protected virtual void OnSubmitCommand()
        {
            RaiseSubmitted();

            DialogResult = null;
            DialogResult = true;
        }

        protected virtual bool CanCancelCommand()
        {
            return !IsSubmit;
        }

        protected virtual void OnCancelCommand()
        {
            DialogResult = null;
            DialogResult = false;
        }

        protected virtual bool CanCloseCommand()
        {
            return true;
        }

        protected virtual void OnCloseCommand()
        {
            RaiseRequestClose();
        }

        protected void RaiseRequestClose()
        {
            RequestClose?.Invoke(this, new EventArgs());
        }

        protected void RaiseSubmitted(EventArgs args = null)
        {
            Submitted?.Invoke(this, args);
        }

        #endregion
    }
}
