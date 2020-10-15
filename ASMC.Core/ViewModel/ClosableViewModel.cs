using System;
using System.Windows.Input;
using DevExpress.Mvvm;

namespace ASMC.Core.ViewModel
{
    /// <summary>
    /// Модель представления с поддержкой
    /// закрытия свзянного представления
    /// (View).
    /// </summary>
    public abstract class ClosableViewModel : BaseViewModel, ISupportClose
    {
        /// <summary>
        /// Возвращает команду закрытия
        /// связанного представления.
        /// </summary>
        public ICommand CloseCommand
        {
            get;
        }

        /// <inheritdoc />
        public event EventHandler RequestClose;

        /// <summary>
        /// Инициализирует новый экземпляр класса
        /// <see cref="ClosableViewModel"/>.
        /// </summary>
        protected ClosableViewModel()
        {
            CloseCommand = new DelegateCommand(OnClose);
        }
        /// <summary>
        /// Закрывает связанное представление.
        /// </summary>
        public void OnClose()
        {
            RequestClose?.Invoke(this, EventArgs.Empty);
        }


        /// <summary>
        /// Возвращает значение, задающее
        /// возможность закрыть связанное
        /// представление.
        /// </summary>
        /// <returns>Возвращает истино, если
        /// закрытие возможно; иначе ложно.</returns>
        public virtual bool CanClose(object state)
        {
            return true;
        }

        /// <inheritdoc />
        public virtual void Close()
        {
        }
    }
}
