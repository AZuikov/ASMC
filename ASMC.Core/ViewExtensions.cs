using System.Windows;
using ASMC.Core.Settings;

namespace ASMC.Core
{
    /// <summary>
    /// Класс расширений для представления
    /// </summary>
    public sealed class ViewExtensions
    {
        /// <summary>
        /// Определяет свойство зависимостей ActualSettings.
        /// </summary>
        public static DependencyProperty ActualSettingsProperty =
            DependencyProperty.RegisterAttached(
                "ActualSettings",
                typeof(object),
                typeof(ViewExtensions),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits));

        /// <summary>
        /// Определяет свойство зависимостей Settings.
        /// </summary>
        public static DependencyProperty SettingsProperty =
            DependencyProperty.RegisterAttached(
                "Settings",
                typeof(object),
                typeof(ViewExtensions),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits));

        /// <summary>
        /// Определяет свойство зависимостей SettingsSelector.
        /// </summary>
        public static DependencyProperty SettingsSelectorProperty =
            DependencyProperty.RegisterAttached(
                "SettingsSelector",
                typeof(SettingsSelector),
                typeof(ViewExtensions),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits));

        public static object GetActualSettings(DependencyObject obj)
        {
            return obj.GetValue(ActualSettingsProperty);
        }

        public static void SetActualSettings(DependencyObject obj, object value)
        {
            obj.SetValue(ActualSettingsProperty, value);
        }

        public static object GetSettings(DependencyObject obj)
        {
            return obj.GetValue(SettingsProperty);
        }

        public static void SetSettings(DependencyObject obj, object value)
        {
            obj.SetValue(SettingsProperty, value);
        }

        public static void SetSettingsSelector(DependencyObject obj, SettingsSelector value)
        {
            obj.SetValue(SettingsSelectorProperty, value);
        }

        public static SettingsSelector GetSettingsSelector(DependencyObject obj)
        {
            return (SettingsSelector)obj.GetValue(SettingsSelectorProperty);
        }

        private static void OnSettingsSelectorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            //if (d.ReadLocalValue(SettingsProperty) == DependencyProperty.UnsetValue)
            //    d.SetCurrentValue(SettingsProperty, GetSettingsSelector(d)?.SelectSettings(d, d));

        }
    }
}
