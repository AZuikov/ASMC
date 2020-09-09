using System;
using System.Collections;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using ASMC.Common.Settings;
using ASMC.Common.ViewModel;
using DevExpress.Mvvm;
using DevExpress.Mvvm.UI;

namespace ASMC.Common.UI
{
    /// <summary>
    /// Задает режим отображения окна.
    /// </summary>
    public enum WindowShowMode
    {
        /// <summary>
        /// Задает немодальное окно.
        /// </summary>
        Default,

        /// <summary>
        /// Задает диалоговое окно.
        /// </summary>
        Dialog
    }

    /// <summary>
    /// Представляет сервис для вывода
    /// информации в оконном режиме.
    /// </summary>
    public class WindowService : ViewServiceBase, IWindowService
    {
        private static readonly Hashtable WindowPosition = new Hashtable();

        #region Fields

        private Window _window;

        #endregion

        #region Methods

        private static void SetBinding(object source, string path, DependencyObject target, DependencyProperty property,
            BindingMode mode = BindingMode.TwoWay)
        {
            var bnd = new Binding
            {
                Source = source,
                Path = new PropertyPath(path),
                Mode = mode,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            };
            BindingOperations.SetBinding(target, property, bnd);
        }

        private Window CreateWindow(object view)
        {
            var window = (Window) Activator.CreateInstance(WindowType);

            if (WindowShowMode == WindowShowMode.Dialog)
                WindowAttachedProperties.SetShowIcon(window, false);

            window.Content = view;
            window.Owner = Application.Current.Windows.OfType<Window>().FirstOrDefault(wnd => wnd.IsActive);

            InitializeDocumentContainer(window, ContentControl.ContentProperty, WindowStyle);
            window.WindowStartupLocation = WindowStartupLocation;

            SetBinding(this, nameof(MinWidth), window, FrameworkElement.MinWidthProperty, BindingMode.OneWay);
            SetBinding(this, nameof(MinHeight), window, FrameworkElement.MinHeightProperty, BindingMode.OneWay);
            SetBinding(this, nameof(MaxWidth), window, FrameworkElement.MaxWidthProperty, BindingMode.OneWay);
            SetBinding(this, nameof(MaxHeight), window, FrameworkElement.MaxHeightProperty, BindingMode.OneWay);
            SetBinding(this, nameof(ResizeMode), window, Window.ResizeModeProperty, BindingMode.OneWay);
            SetBinding(this, nameof(SizeToContent), window, Window.SizeToContentProperty, BindingMode.OneWay);
            SetBinding(this, nameof(Title), window, Window.TitleProperty, BindingMode.OneWay);

            return window;
        }

        private void ViewModel_RequestClose(object sender, EventArgs e)
        {
            _window?.Close();
        }

        private void WindowClosed(object sender, EventArgs e)
        {
            var window = (Window) sender;

            var viewModel = window.DataContext;
            if (viewModel != null)
            {
                if (viewModel is ISupportClose sc)
                {
                    sc.Close();
                    sc.RequestClose -= ViewModel_RequestClose;
                }

                WindowPosition[viewModel.GetType()] = new Point(window.Left, window.Top);
            }

            window.Closing -= WindowClosing;
            window.Closed -= WindowClosed;

            _window = null;
            IsWindowAlive = false;

            window.Owner?.Activate();
        }

        private void WindowClosing(object sender, CancelEventArgs e)
        {
            var window = (Window) sender;
            if (window.Tag is Action<CancelEventArgs> ac)
                ac(e);

            if (window.DataContext is ISupportClose scw)
                e.Cancel = !scw.CanClose(null);
        }

        private void WindowContentRendered(object sender, EventArgs e)
        {
            var window = (Window) sender;
            if (window.DataContext is ISupportDialog sdw)
                sdw.Initialize();
            else if (window.DataContext is BaseViewModel bvm)
                bvm.Initialize();

            window.ContentRendered -= WindowContentRendered;
        }

        private void WindowLoaded(object sender, EventArgs e)
        {
            var window = (Window) sender;
            if (UseWaitCursor)
                Mouse.OverrideCursor = null;

            window.Loaded -= WindowLoaded;
        }

        #endregion

        public void Activate()
        {
            _window?.Activate();
        }

        public void Restore()
        {
            _window?.Show();
        }

        public void Hide()
        {
            _window?.Hide();
        }

        public void Close()
        {
            _window.Close();
        }

        public void SetWindowState(WindowState state)
        {
            if (_window != null)
                _window.WindowState = state;
        }

        /// <summary>
        /// Отображает окно.
        /// </summary>
        /// <param name = "documentType">Строка, содержащая имя представления окна.</param>
        /// <param name = "viewModel">Модель представления, используемая как содержимое</param>
        /// <param name = "parameter">Дополнительная информация, передаваемая при инициализации окна.</param>
        /// <param name = "parentViewModel"></param>
        public void Show(string documentType, object viewModel, object parameter, object parentViewModel)
        {
            if (_window != null)
            {
                _window.Activate();
                return;
            }

            if (UseWaitCursor)
                Mouse.OverrideCursor = Cursors.Wait;

            var view = CreateAndInitializeView(documentType, viewModel, parameter, parentViewModel);

            _window = CreateWindow(view);
            IsWindowAlive = true;

            _window.Loaded += WindowLoaded;
            _window.ContentRendered += WindowContentRendered;

            _window.Closing += WindowClosing;
            _window.Closed += WindowClosed;

            var settings = Settings ?? SettingsSelector?.SelectSettings(viewModel, view as DependencyObject);
            ViewExtensions.SetActualSettings(_window, settings);

            if (settings is IWindowSettings && SizeToContent != SizeToContent.WidthAndHeight)
            {
                if (SizeToContent != SizeToContent.Width)
                    SetBinding(settings, "Width", _window, FrameworkElement.WidthProperty);
                if (SizeToContent != SizeToContent.Height)
                    SetBinding(settings, "Height", _window, FrameworkElement.HeightProperty);
            }

            SetBinding(this, nameof(SettingsSelector), _window, ViewExtensions.SettingsSelectorProperty,
                       BindingMode.OneWay);
            SetBinding(this, nameof(Settings), _window, ViewExtensions.SettingsProperty, BindingMode.OneWay);

            //Settings = settings as IWindowSettings;
            if (viewModel != null)
            {
                if (viewModel is BaseViewModel bvm)
                    bvm.Settings = settings;
                if (WindowPosition.ContainsKey(viewModel.GetType()))
                {
                    var pos = (Point) WindowPosition[viewModel.GetType()];

                    _window.WindowStartupLocation = WindowStartupLocation.Manual;
                    _window.Top = pos.Y;
                    _window.Left = pos.X;
                }

                if (viewModel is ISupportDialog)
                    SetBinding(viewModel, nameof(ISupportDialog.DialogResult), _window,
                               WindowAttachedProperties.DialogResultProperty, BindingMode.OneWay);

                if (viewModel is ISupportClose sc)
                    sc.RequestClose += ViewModel_RequestClose;
            }

            if (WindowShowMode == WindowShowMode.Dialog)
            {
                _window.ShowInTaskbar = _window.Owner == null;
                _window.ShowDialog();
            }
            else
            {
                _window.Show();
            }
        }

        #region Dependency Properties

        private static readonly DependencyPropertyKey ActualSettingsPropertyKey =
            DependencyProperty.RegisterReadOnly(
                                                nameof(ActualSettings),
                                                typeof(object),
                                                typeof(WindowService),
                                                null);

        private static readonly DependencyPropertyKey IsWindowAlivePropertyKey =
            DependencyProperty.RegisterReadOnly(
                                                nameof(IsWindowAlive),
                                                typeof(bool),
                                                typeof(WindowService),
                                                null);

        public static readonly DependencyProperty ActualSettingsProperty = ActualSettingsPropertyKey.DependencyProperty;

        public static readonly DependencyProperty IsWindowAliveProperty = IsWindowAlivePropertyKey.DependencyProperty;

        public static readonly DependencyProperty SettingsSelectorProperty =
            DependencyProperty.Register(
                                        "SettingsSelector",
                                        typeof(SettingsSelector),
                                        typeof(WindowService));

        public static readonly DependencyProperty SettingsProperty =
            DependencyProperty.Register(
                                        "Settings",
                                        typeof(IWindowSettings),
                                        typeof(WindowService));

        public static readonly DependencyProperty UseWaitCursorProperty =
            DependencyProperty.Register(
                                        "UseWaitCursor",
                                        typeof(bool),
                                        typeof(WindowService),
                                        new PropertyMetadata(true));

        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register(
                                        "Title",
                                        typeof(string),
                                        typeof(WindowService));

        public static readonly DependencyProperty WindowTypeProperty =
            DependencyProperty.Register(
                                        "WindowType",
                                        typeof(Type),
                                        typeof(WindowService),
                                        new PropertyMetadata(typeof(Window)));

        public static readonly DependencyProperty WindowShowModeProperty =
            DependencyProperty.Register(
                                        "WindowShowMode",
                                        typeof(WindowShowMode),
                                        typeof(WindowService),
                                        new PropertyMetadata(WindowShowMode.Dialog));

        public static readonly DependencyProperty WindowStartupLocationProperty =
            DependencyProperty.Register(
                                        "WindowStartupLocation",
                                        typeof(WindowStartupLocation),
                                        typeof(WindowService),
                                        new PropertyMetadata(WindowStartupLocation.CenterOwner));

        public static readonly DependencyProperty WindowStyleProperty =
            DependencyProperty.Register(
                                        "WindowStyle",
                                        typeof(Style),
                                        typeof(WindowService),
                                        new PropertyMetadata(null));

        public static readonly DependencyProperty MinWidthProperty =
            FrameworkElement.MinWidthProperty.AddOwner(
                                                       typeof(WindowService),
                                                       new PropertyMetadata(200d));

        public static readonly DependencyProperty MinHeightProperty =
            FrameworkElement.MinHeightProperty.AddOwner(
                                                        typeof(WindowService),
                                                        new PropertyMetadata(200d));

        public static readonly DependencyProperty MaxWidthProperty =
            FrameworkElement.MaxWidthProperty.AddOwner(
                                                       typeof(WindowService));

        public static readonly DependencyProperty MaxHeightProperty =
            FrameworkElement.MaxHeightProperty.AddOwner(
                                                        typeof(WindowService));

        public static readonly DependencyProperty ResizeModeProperty =
            Window.ResizeModeProperty.AddOwner(
                                               typeof(WindowService));

        public static readonly DependencyProperty SizeToContentProperty =
            Window.SizeToContentProperty.AddOwner(
                                                  typeof(WindowService));

        #endregion Dependency Properties

        #region Properties

        /// <summary>
        /// Возвращает используемые пользовательские
        /// параметры окна.
        /// </summary>
        public object ActualSettings
        {
            get => GetValue(ActualSettingsProperty);
            private set => SetValue(ActualSettingsPropertyKey, value);
        }

        /// <summary>
        /// Возвращает или задает
        /// пользовательские
        /// параметры окна.
        /// </summary>
        public IWindowSettings Settings
        {
            get => (IWindowSettings) GetValue(SettingsProperty);
            set => SetValue(SettingsProperty, value);
        }

        /// <summary>
        /// Возвращает или задает
        /// селектор пользовательских
        /// параметров окна.
        /// </summary>
        public SettingsSelector SettingsSelector
        {
            get => (SettingsSelector) GetValue(SettingsSelectorProperty);
            set => SetValue(SettingsSelectorProperty, value);
        }

        /// <summary>
        /// Возвращает или задает значение, задающее
        /// использование иконки ожидания на курсоре
        /// до полной загрузки окна.
        /// </summary>
        public bool UseWaitCursor
        {
            get => (bool) GetValue(UseWaitCursorProperty);
            set => SetValue(UseWaitCursorProperty, value);
        }

        public double MinWidth
        {
            get => (double) GetValue(MinWidthProperty);
            set => SetValue(MinWidthProperty, value);
        }

        public double MinHeight
        {
            get => (double) GetValue(MinHeightProperty);
            set => SetValue(MinHeightProperty, value);
        }

        public double MaxWidth
        {
            get => (double) GetValue(MaxWidthProperty);
            set => SetValue(MaxWidthProperty, value);
        }

        public double MaxHeight
        {
            get => (double) GetValue(MaxHeightProperty);
            set => SetValue(MaxHeightProperty, value);
        }

        public ResizeMode ResizeMode
        {
            get => (ResizeMode) GetValue(ResizeModeProperty);
            set => SetValue(ResizeModeProperty, value);
        }

        public SizeToContent SizeToContent
        {
            get => (SizeToContent) GetValue(SizeToContentProperty);
            set => SetValue(SizeToContentProperty, value);
        }

        public string Title
        {
            get => (string) GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public bool IsWindowAlive
        {
            get => (bool) GetValue(IsWindowAliveProperty);
            private set => SetValue(IsWindowAlivePropertyKey, value);
        }

        public WindowShowMode WindowShowMode
        {
            get => (WindowShowMode) GetValue(WindowShowModeProperty);
            set => SetValue(WindowShowModeProperty, value);
        }

        public WindowStartupLocation WindowStartupLocation
        {
            get => (WindowStartupLocation) GetValue(WindowStartupLocationProperty);
            set => SetValue(WindowStartupLocationProperty, value);
        }

        public Style WindowStyle
        {
            get => (Style) GetValue(WindowStyleProperty);
            set => SetValue(WindowStyleProperty, value);
        }

        public Type WindowType
        {
            get => (Type) GetValue(WindowTypeProperty);
            set => SetValue(WindowTypeProperty, value);
        }

        #endregion Properties
    }
}