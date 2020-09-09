using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using DevExpress.Mvvm;

namespace ASMC.Common.ViewModel
{
    public class FromBaseViewModel : ClosableViewModel, ISupportDialog
    {
        #region Fields

        private bool _allowChanges;
        private bool _allowSearch = true;
        private bool? _dialogResult;
        private bool _allowSelect;

        #endregion

        #region Properties
         

        /// <summary>
        /// Возвращает или задает значение,
        /// задающее возможность редактирования
        /// данных справочника.
        /// </summary>
        public bool AllowChanges
        {
            get => _allowChanges;
            set => SetProperty(ref _allowChanges, value, nameof(AllowChanges));
        }

        /// <summary>
        /// Возвращает или задает значение,
        /// задающее возможность поиска
        /// данных по справочнику.
        /// </summary>
        public bool AllowSearch
        {
            get => _allowSearch;
            set => SetProperty(ref _allowSearch, value, nameof(AllowSearch));
        }   
    

        #region ISupportDialog

        /// <inheritdoc />
        public bool? DialogResult
        {
            get => _dialogResult;
            private set => SetProperty(ref _dialogResult, value, nameof(DialogResult));
        }

        #endregion

        #region Commands

   
        /// <summary>
        /// Возвращает команду для подтверждения
        /// выбора текущей записи.
        /// </summary>
        public ICommand SelectCommand
        {
            get;
        }   
    
        #endregion

        #endregion

        /// <summary>
        /// Инициализирует новый экземпляр класса
        /// <see cref="FromBaseViewModel"/>.
        /// </summary>
        protected FromBaseViewModel()
        {
           
            SelectCommand = new DelegateCommand(OnSelectCommand, CanSelectCommand);
        }
        /// <summary>
        /// Указывает возможность отображения кнопки выбора. 
        /// </summary>
        public bool AllowSelect
        {
            get => _allowSelect;
            set => SetProperty(ref _allowSelect, value, nameof(AllowSelect));
        }

        #region Methods

    

        /// <summary>
        /// Возвращает значение, задающее
        /// возможность выбрать текущую
        /// запись.
        /// </summary>
        /// <returns>Возвращает истино, если
        /// выполнение возможно; иначе ложно.</returns>
        protected virtual bool CanSelect()
        {
            return !IsBusy && Entity != null;
        }

        

      

       

        

        

        /// <summary>
        /// Вызывается при подтверждении
        /// выбора записи.
        /// </summary>
        protected virtual void OnSelect()
        {
            DialogResult = true;
        }

        private bool CanSelectCommand()
        {
            return CanSelect();
        }

        

        private void OnSelectCommand()
        {
           
                OnSelect();  
                // Сбрасываем значение без уведомления связанного
                // представления. Необходимо для повторного
                // использования текущей view model.
                _dialogResult = null;
        }

        #endregion

      
    }
}
