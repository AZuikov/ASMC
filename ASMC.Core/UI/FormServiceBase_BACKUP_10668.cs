using System;
using System.ComponentModel;
using System.Reflection;
using System.Windows;
using System.Windows.Data;
using ASMC.Core.Settings;
using ASMC.Core.ViewModel;
using DevExpress.Mvvm;
using DevExpress.Mvvm.UI;
using DevExpress.Utils.OAuth;

namespace ASMC.Core.UI
{
    public abstract class FormServiceBase : ServiceBase, IFormService
    {
        #region Fields



        /// <summary>
        /// Определяет свойство зависимостей <see cref="Title"/>.
        /// </summary>
        public static readonly DependencyProperty TitleProperty =
            WindowService.TitleProperty.AddOwner(typeof(FormServiceBase));

        /// <summary>
        /// Определяет свойство зависимостей <see cref="AllowChanges"/>.
        /// </summary>
        public static readonly DependencyProperty AllowChangesProperty =
            DependencyProperty.Register(nameof(AllowChanges), typeof(bool), typeof(FormServiceBase),
                new PropertyMetadata(true));

        /// <summary>
        /// Определяет свойство зависимостей <see cref="Entity"/>.
        /// </summary>
        public static readonly DependencyProperty EntityProperty =
            DependencyProperty.Register(nameof(Entity), typeof(object), typeof(FormServiceBase));



        /// <summary>
        /// Определяет свойство зависимостей <see cref="ResizeMode"/>.
        /// </summary>
        public static readonly DependencyProperty ResizeModeProperty =
            WindowService.ResizeModeProperty.AddOwner(typeof(FormServiceBase));

        /// <summary>
        /// Определяет свойство зависимостей <see cref="Settings"/>.
        /// </summary>
        public static readonly DependencyProperty SettingsProperty =
            WindowService.SettingsProperty.AddOwner(typeof(FormServiceBase));

        /// <summary>
        /// Определяет свойство зависимостей <see cref="SettingsSelector"/>.
        /// </summary>
        public static readonly DependencyProperty SettingsSelectorProperty =
            WindowService.SettingsSelectorProperty.AddOwner(typeof(FormServiceBase));

        private static readonly ViewLocator ViewLocatorCore = new ViewLocator(Assembly.GetExecutingAssembly());

        //private object _viewModel;
        private bool? _dialogResult;

        #endregion



        #region Properties



        /// <inheritdoc />
        public string Title
        {
            get => (string) GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        /// <inheritdoc />
        public object Entity
        {
            get => GetValue(EntityProperty);
            set => SetValue(EntityProperty, value);
        }

        /// <summary>
        /// Возвращает или задает значение, определяющее
        /// возможность добавления, удаления или изменения
        /// записей в справочнике.
        /// </summary>
        public bool AllowChanges
        {
            get => (bool) GetValue(AllowChangesProperty);
            set => SetValue(AllowChangesProperty, value);
        }

        /// <summary>
        /// Возвращает или задает локатор
        /// представлений для поиска
        /// представления справочника.
        /// </summary>
        public ViewLocator ViewLocator { get; set; }

        /// <summary>
        /// Возвращает или задает режим
        /// изменения размеров справочника.
        /// </summary>
        public ResizeMode ResizeMode
        {
            get => (ResizeMode) GetValue(ResizeModeProperty);
            set => SetValue(ResizeModeProperty, value);
        }

        /// <summary>
        /// Возвращает или задает объект,
        /// содержащий пользовательские
        /// параметры для справочника.
        /// </summary>
        public object Settings
        {
            get => GetValue(SettingsProperty);
            set => SetValue(SettingsProperty, value);
        }

        /// <summary>
        /// Возвращает или задает селектор
        /// пользовательских параметров для
        /// справочника.
        /// </summary>
        public SettingsSelector SettingsSelector
        {
            get => (SettingsSelector) GetValue(SettingsSelectorProperty);
            set => SetValue(SettingsSelectorProperty, value);
        }

        /// <summary>
        /// Возвращает или задает название
        /// представления для справочника.
        /// </summary>
        protected string DocumentType { get; set; }

        /// <summary>
        /// Возвращает или задает минимальный
        /// размер для карточки.
        /// </summary>
        protected Size MinSize { get; set; }

        /// <summary>
        /// Возвращает или задает максимальный
        /// размер для карточки.
        /// </summary>
        protected Size MaxSize { get; set; } = new Size(int.MaxValue, int.MaxValue);

        #endregion

        #region Methods

        /// <inheritdoc />
        public bool Show()
        {
            _dialogResult = null;

            var wndService = new WindowService
            {
                Title = Title,
                MinWidth = MinSize.Width,
                MinHeight = MinSize.Height,
                MaxWidth = MaxSize.Width,
                MaxHeight = MaxSize.Height,
                WindowShowMode = WindowShowMode.Dialog,
                ResizeMode = ResizeMode,
                ViewLocator = ViewLocatorCore,
                Settings = Settings as IWindowSettings,
                SettingsSelector = SettingsSelector
            };

            var viewModel = CreateViewModel(); //_viewModel ?? (_viewModel = CreateViewModel());
            if (viewModel is BaseViewModel vm)
            {
                vm.Settings = Settings;
            }

            var cb = viewModel as BaseViewModel;
            if (cb != null)
            {
                cb.Entity = Entity; 
                Subscribe(cb);  
            }

            ViewInjectionManager.Default.Inject(cb?.RegionName, null, () => viewModel,
                ViewLocator?.ResolveViewType(GetDocumentType(viewModel)));

            try
            {
                SubscribeWindowServiceEvents(wndService);
                wndService.Show("CatalogView", viewModel, null, null);
            }
            finally
            {
                ViewInjectionManager.Default.Remove(cb?.RegionName, null);

                UnsubscribeWindowServiceEvents(wndService);
                if (cb != null)
                    Unsubscribe(cb);
            }

            return _dialogResult == true;
        }

        /// <summary>
        /// Создает модель представления справочника.
        /// </summary>
        protected abstract object CreateViewModel();

        /// <inheritdoc />
        protected override void OnAttached()
        {
            base.OnAttached();

            //BindingOperations.SetBinding(this, SettingsProperty,
            //    new Binding
            //    {
            //        Source = AssociatedObject,
            //        Path = new PropertyPath(ViewExtensions.SettingsProperty)
            //    });

            BindingOperations.SetBinding(this, SettingsSelectorProperty,
                new Binding
                {
                    Source = AssociatedObject,
                    Path = new PropertyPath(ViewExtensions.SettingsSelectorProperty)
                });
        }

        private string GetDocumentType(object viewModel)
        {
            if (DocumentType != null)
                return DocumentType;

            var type = viewModel.GetType().Name;
            return type.Replace("ViewModel", "View");
        }

        private void SubscribeWindowServiceEvents(WindowService service)
        {
            var dpd = DependencyPropertyDescriptor.FromProperty(WindowService.ActualSettingsProperty,
                service.GetType());
            dpd.AddValueChanged(service, WindowService_ActualSettingsChanged);
        }

        private void UnsubscribeWindowServiceEvents(WindowService service)
        {
            var dpd = DependencyPropertyDescriptor.FromProperty(WindowService.ActualSettingsProperty,
                service.GetType());
            dpd.RemoveValueChanged(service, WindowService_ActualSettingsChanged);
        }

        private void Subscribe(BaseViewModel viewModel)
        {
            //viewModel.Created += CardBaseViewModel_Created;
            //viewModel.Updated += CardBaseViewModel_Updated;
            //viewModel.Deleted += CardBaseViewModel_Deleted;

            viewModel.PropertyChanged += CatalogViewModel_PropertyChanged;
        }

        private void Unsubscribe(BaseViewModel viewModel)
        {
            //viewModel.Created -= CardBaseViewModel_Created;
            //viewModel.Updated -= CardBaseViewModel_Updated;
            //viewModel.Deleted -= CardBaseViewModel_Deleted;

            viewModel.PropertyChanged -= CatalogViewModel_PropertyChanged;
        }

        private void WindowService_ActualSettingsChanged(object sender, EventArgs e)
        {
            //if (_viewModel is CatalogBaseViewModel cb)
            //    cb.Settings = ((WindowService) sender).ActualSettings;
        }

        private void CatalogViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
               
                case nameof(BaseViewModel.Entity):
                    Entity = ((BaseViewModel) sender).Entity;
                    break;
                //case nameof(BaseViewModel.DialogResult):
<<<<<<< HEAD
                  //  _dialogResult = ((BaseViewModel) sender).DialogResult;
                    break;
=======
                //    _dialogResult = ((BaseViewModel) sender).DialogResult;
                //    break;
>>>>>>> origin/Цивинский
            }
        }

        #endregion

        /// <summary>
        /// Определяет свойство зависимостей <see cref="Parameter"/>.
        /// </summary>
        public static readonly DependencyProperty ParameterProperty =
            DependencyProperty.Register(nameof(Parameter), typeof(object), typeof(FormServiceBase));

        /// <inheritdoc />
        public object Parameter
        {
            get => GetValue(ParameterProperty);
            set => SetValue(ParameterProperty, value);
        }
    }
}
