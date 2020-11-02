using System;
using System.Windows;
using System.Windows.Interop;

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

        public static readonly DependencyProperty ShowIconProperty =
            DependencyProperty.RegisterAttached(
                "ShowIcon",
                typeof(bool),
                typeof(WindowAttachedProperties),
                new FrameworkPropertyMetadata(true, ShowIconChanged));

        private static void DialogResultChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Window window && ComponentDispatcher.IsThreadModal && window.IsLoaded) 
                window.DialogResult = e.NewValue as bool?;
        }

        private static void ShowIconChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if(d is Window window)
            {
                window.SourceInitialized += delegate
                {
                    var hwnd = new WindowInteropHelper(window).Handle;

                    var extendedStyle = NativeMethods.GetWindowLong(hwnd, NativeMethods.GWL_EXSTYLE);
                    NativeMethods.SetWindowLong(hwnd,
                        NativeMethods.GWL_EXSTYLE,
                        extendedStyle |
                        NativeMethods.WS_EX_DLGMODALFRAME);

                    NativeMethods.SendMessage(hwnd, NativeMethods.WM_SETICON, IntPtr.Zero, IntPtr.Zero);
                    NativeMethods.SendMessage(hwnd, NativeMethods.WM_SETICON, new IntPtr(1), IntPtr.Zero);

                    NativeMethods.SetWindowPos(hwnd, IntPtr.Zero, 0, 0, 0, 0,
                        NativeMethods.SWP_NOMOVE |
                        NativeMethods.SWP_NOSIZE |
                        NativeMethods.SWP_NOZORDER |
                        NativeMethods.SWP_FRAMECHANGED);
                };
            }
        }

        public static bool? GetDialogResult(Window target)
        {
            return (bool?)target.GetValue(DialogResultProperty);
        }

        public static void SetDialogResult(Window target, bool? value)
        {
            target.SetValue(DialogResultProperty, value);
        }

        public static bool GetShowIcon(Window target)
        {
            return (bool)target.GetValue(ShowIconProperty);
        }

        public static void SetShowIcon(Window target, bool value)
        {
            target.SetValue(ShowIconProperty, value);
        }
    }
}
