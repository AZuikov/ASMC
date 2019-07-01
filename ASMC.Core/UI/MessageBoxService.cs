using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Threading;
using System.Windows;
using DevExpress.Mvvm;
using DevExpress.Mvvm.UI;
using DevExpress.XtraEditors.ButtonPanel;

namespace ASMC.Core.UI
{
    public class MessageBoxService : ServiceBase, IMessageBoxService
    {
        public MessageResult Show(string messageBoxText, string caption, MessageButton button, MessageIcon icon,
            MessageResult defaultResult)
        {
            Window owner = null;
            if (!CheckAccess())
            {
                // return MessageResult.None; 
                Dispatcher.InvokeAsync(() =>
                    owner = Application.Current.Windows.OfType<Window>().FirstOrDefault(x => x.IsActive));
            }
            else
            {
                try
                {
                    Dispatcher.InvokeAsync(() =>
                        owner = Application.Current.Windows.OfType<Window>().FirstOrDefault(x => x.IsActive));
                }
                catch
                {
                    owner = Application.Current.Windows.OfType<Window>().FirstOrDefault(x => x.IsActive);
                }
            }

            var title = caption ?? owner?.Title;

            return (owner != null
                ? MessageBox.Show(owner, messageBoxText, title, button.ToMessageBoxButton(), icon.ToMessageBoxImage(),
                    defaultResult.ToMessageBoxResult())
                : MessageBox.Show(messageBoxText, title, button.ToMessageBoxButton(), icon.ToMessageBoxImage(),
                    defaultResult.ToMessageBoxResult())).ToMessageResult();
        }
    }
}