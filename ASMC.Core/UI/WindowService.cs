using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using DevExpress.Mvvm;
using DevExpress.Mvvm.UI;

namespace ASMC.Core.UI
{
    class WindowService : ViewServiceBase, IWindowService
    {
        private Window _window;
        public void Show(string documentType, object viewModel, object parameter, object parentViewModel)
        {
            throw new NotImplementedException();
        }

        public void SetWindowState(WindowState state)
        {
            throw new NotImplementedException();
        }

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

        public string Title { get; set; }
        public bool IsWindowAlive { get; }
     

        private static void SetBinding(object source, string path, DependencyObject target, DependencyProperty property, BindingMode mode = BindingMode.TwoWay)
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
    }
}
