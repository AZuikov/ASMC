using System.Linq;
using System.Windows;
using DevExpress.Mvvm;
using DevExpress.Mvvm.UI;

namespace ASMC.Core.UI
{
    public class MessageBoxService : ServiceBase, IMessageBoxService
    {
        public MessageResult Show(string messageBoxText, string caption, MessageButton button, MessageIcon icon, MessageResult defaultResult)
        {
            if (!CheckAccess())
                return MessageResult.None;

            var owner = Application.Current.Windows.OfType<Window>().FirstOrDefault(x => x.IsActive);
            var title = caption ?? owner?.Title;

            return (owner != null
                ? MessageBox.Show(owner, messageBoxText, title, button.ToMessageBoxButton(), icon.ToMessageBoxImage(),
                    defaultResult.ToMessageBoxResult())
                : MessageBox.Show(messageBoxText, title, button.ToMessageBoxButton(), icon.ToMessageBoxImage(),
                    defaultResult.ToMessageBoxResult())).ToMessageResult();
        }
    }
}