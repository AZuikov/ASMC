using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace AP.Utils
{
    /// <summary>
    /// Представляет элемент отображения
    /// контрольного значения с заголовком.
    /// </summary>
    public class ValueContentControl : HeaderedContentControl
    {
        /// <summary>
        /// Определяет свойство зависимостей <see cref="Orientation"/>.
        /// </summary>
        public static readonly DependencyProperty OrientationProperty;

        /// <summary>
        /// Определяет свойство зависимостей <see cref="AllowBrowse"/>.
        /// </summary>
        public static readonly DependencyProperty AllowBrowseProperty;

        /// <summary>
        /// Определяет свойство зависимостей <see cref="AllowCheck"/>.
        /// </summary>
        public static readonly DependencyProperty AllowCheckProperty;

        /// <summary>
        /// Определяет свойство зависимостей <see cref="BrowseCommand"/>.
        /// </summary>
        public static readonly DependencyProperty BrowseCommandProperty;

        /// <summary>
        /// Определяет свойство зависимостей <see cref="IsRequired"/>.
        /// </summary>
        public static readonly DependencyProperty IsRequiredProperty;

        /// <summary>
        /// Определяет свойство зависимостей <see cref="IsChecked"/>.
        /// </summary>
        public static readonly DependencyProperty IsCheckedProperty;

        /// <summary>
        /// Возвращает или задает ориентацию
        /// заголовка и дочернего содержимого.
        /// </summary>
        public Orientation Orientation
        {
            get => (Orientation) GetValue(OrientationProperty);
            set => SetValue(OrientationProperty, value);
        }

        /// <summary>
        /// Возвращает или задает значение, определяющее
        /// доступность функции обзора.
        /// </summary>
        public bool AllowBrowse
        {
            get => (bool) GetValue(AllowBrowseProperty);
            set => SetValue(AllowBrowseProperty, value);
        }

        /// <summary>
        /// Возвращает или задает значение, определяющее
        /// доступность флажка для отметки.
        /// </summary>
        public bool AllowCheck
        {
            get => (bool)GetValue(AllowCheckProperty);
            set => SetValue(AllowCheckProperty, value);
        }

        /// <summary>
        /// Возвращает или задает команду
        /// обзора.
        /// </summary>
        public ICommand BrowseCommand
        {
            get => (ICommand)GetValue(BrowseCommandProperty);
            set => SetValue(BrowseCommandProperty, value);
        }

        /// <summary>
        /// Возвращает или задает состояние,
        /// определяющее обязательность значения
        /// для дочернего содержимого.
        /// </summary>
        public bool IsRequired
        {
            get => (bool)GetValue(IsRequiredProperty);
            set => SetValue(IsRequiredProperty, value);
        }

        /// <summary>
        /// Возвращает или задает значение,
        /// определяющее состояние установки
        /// флажка для отметки.
        /// </summary>
        public bool IsChecked
        {
            get => (bool)GetValue(IsCheckedProperty);
            set => SetValue(IsCheckedProperty, value);
        }

        static ValueContentControl()
        {
            var type = typeof(ValueContentControl);

            FocusableProperty.OverrideMetadata(typeof(ValueContentControl), new FrameworkPropertyMetadata(false));
            IsTabStopProperty.OverrideMetadata(typeof(ValueContentControl), new FrameworkPropertyMetadata(false));

            DefaultStyleKeyProperty.OverrideMetadata(typeof(ValueContentControl),
                new FrameworkPropertyMetadata(typeof(ValueContentControl)));

            OrientationProperty = StackPanel.OrientationProperty.AddOwner(
                typeof(ValueContentControl));

            AllowBrowseProperty = DependencyProperty.Register(
                nameof(AllowBrowse),
                typeof(bool),
                type);

            AllowCheckProperty = DependencyProperty.Register(
                nameof(AllowCheck),
                typeof(bool),
                type);

            BrowseCommandProperty = DependencyProperty.Register(
                nameof(BrowseCommand),
                typeof(ICommand),
                type);

            IsRequiredProperty = DependencyProperty.Register(
                nameof(IsRequired),
                typeof(bool),
                type);

            IsCheckedProperty = DependencyProperty.Register(
                nameof(IsChecked),
                typeof(bool),
                type);
        }
    }
}