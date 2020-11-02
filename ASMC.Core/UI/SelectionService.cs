using System;
using System.ComponentModel;
using System.Reflection;
using System.Windows;
using ASMC.Core.ViewModel;
using DevExpress.Mvvm;
using DevExpress.Mvvm.UI;

namespace ASMC.Core.UI
{
    public class SelectionService : ServiceBase, ISelectionService
    {
        

        private bool? _dialogResult;

        #region Property


        /// <summary>
        ///     Возвращает или задает минимальный
        ///     размер для карточки.
        /// </summary>
        protected Size MinSize { get; set; }

        /// <summary>
        ///     Возвращает или задает максимальный
        ///     размер для карточки.
        /// </summary>
        protected Size MaxSize { get; set; } = new Size(int.MaxValue, int.MaxValue);

        #endregion

        #region ISelectionService Members



        /// <inheritdoc />
        public object Parameter { get; set; }

        /// <inheritdoc />
        public object Entity { get; set; }

        /// <inheritdoc />
        public INotifyPropertyChanged ViewModel { get; set; }

        /// <inheritdoc />
        public ViewLocator ViewLocatorCore { get; set; } = new ViewLocator(Assembly.GetExecutingAssembly());

        /// <inheritdoc />
        public string DocumentType { get; set; }

        /// <inheritdoc />
        public ViewLocator ViewLocator { get; set; }

        /// <inheritdoc />
        public string Title { get; set; }

        /// <inheritdoc />
        public bool? Show()
        {
            var wndService = new WindowService
            {
                Title = Title,
                MinWidth = MinSize.Width,
                MinHeight = MinSize.Height,
                MaxWidth = MaxSize.Width,
                MaxHeight = MaxSize.Height,
                WindowShowMode = WindowShowMode.Dialog,
                ResizeMode = ResizeMode.CanResize,
                ViewLocator = ViewLocatorCore
            };
            if (CreateViewModel()!=null) ViewModel = CreateViewModel();
            if (ViewModel is BaseViewModel vm)
            {
                vm.Initialize();
            }

            var cb = ViewModel as SelectionViewModel;
            if (cb != null)
            {
                cb.Entity = Entity;
                cb.PropertyChanged += SelectionViewModel_PropertyChanged;
            }

            ViewInjectionManager.Default.Inject(cb?.RegionName, null, () => ViewModel,
                ViewLocator?.ResolveViewType(DocumentType));
            try
            {
                wndService.Show("SelectionView", ViewModel, Parameter, null);
            }
            finally
            {
                ViewInjectionManager.Default.Remove(cb?.RegionName, null);

                if (cb != null)
                    cb.PropertyChanged-= SelectionViewModel_PropertyChanged;
            }

            return cb?.DialogResult;
        }

        #endregion


        protected virtual INotifyPropertyChanged CreateViewModel()
        {
            return null;
        }
        private void SelectionViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(SelectionViewModel.Entity):
                    Entity = ((SelectionViewModel)sender).Entity;
                    break;
                case nameof(SelectionViewModel.DialogResult):
                    _dialogResult = ((SelectionViewModel)sender).DialogResult;
                    break;
            }
        }
    }
}