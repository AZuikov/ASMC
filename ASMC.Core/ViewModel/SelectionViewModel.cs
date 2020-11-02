using System;
using System.Windows.Input;
using DevExpress.Mvvm;

namespace ASMC.Core.ViewModel
{
    public class SelectionViewModel : ClosableViewModel, ISupportDialog
    {

        private string _regionName = Guid.NewGuid().ToString();
        /// <summary>
        /// Возвращает или задает имя региона
        /// для представления. Служебное свойство.
        /// </summary>
        public string RegionName
        {
            get => _regionName;
            set => SetProperty(ref _regionName, value, nameof(RegionName));
        }
        /// <inheritdoc />
        public SelectionViewModel()
        {
            SelectCommand = new DelegateCommand(OnSelectCommand, CanSelectCommand);
        }

        private bool CanSelectCommand()
        {
            return CanSelect();
        }

        /// <summary>
        /// Возвращает значение, задающее
        /// возможность выбрать текущую
        /// запись.
        /// </summary>
        /// <returns>Возвращает истино, если
        /// выполнение возможно; иначе ложно.</returns>
        protected virtual bool CanSelect()
        {
            return Entity != null;
        }

        private void OnSelectCommand()
        {
            DialogResult = null;
            OnSelect();
        }
        /// <summary>
        /// Вызывается при подтверждении
        /// выбора.
        /// </summary>
        protected virtual void OnSelect()
        {
            DialogResult = true;
        }

        private object _entity;
        private bool? _dialogResult;

        /// <summary>
        /// Возвращает команду для подтверждения
        /// выбора текущей записи.
        /// </summary>
        public ICommand SelectCommand { get; }
        /// <summary>
        /// Возвращает или задает сущность,
        /// данные которой содержит справочник.
        /// </summary>
        public object Entity
        {
            get => _entity;
            set => SetProperty(ref _entity, value, nameof(Entity), OnEntityChanged);
        }


        /// <summary>
        /// Вызывается при изменении текущей
        /// выбранной сущности справочника.
        /// </summary>
        protected virtual void OnEntityChanged()
        {

        }

        /// <inheritdoc />
        public bool? DialogResult
        {
            get => _dialogResult;
            private set => SetProperty(ref _dialogResult, value, nameof(DialogResult));
        }
    }
}