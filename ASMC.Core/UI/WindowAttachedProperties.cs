using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ASMC.Core.UI
{
    public static class WindowAttachedProperties
    {
        public static readonly DependencyProperty DialogResultProperty =
            DependencyProperty.RegisterAttached(
                "DialogResult",
                typeof(bool?),
                typeof(WindowAttachedProperties),
                new PropertyMetadata(DialogResultChanged));

        private static void DialogResultChanged(
            DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            if(d is Window window)
                window.DialogResult = e.NewValue as bool?;
        }

        public static bool? GetDialogResult(Window target, bool? value)
        {
            return (bool?)target.GetValue(DialogResultProperty);
        }

        public static void SetDialogResult(Window target, bool? value)
        {
            target.SetValue(DialogResultProperty, value);
        }
    }
}
