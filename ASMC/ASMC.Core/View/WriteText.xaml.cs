using System.Windows.Controls;
using System.Windows.Documents;

namespace ASMC.Core.View
{
    /// <summary>
    /// Логика взаимодействия для WriteText.xaml
    /// </summary>
    public partial class WriteText : UserControl
    {
        /// <summary>
        /// Текст из RichTextBox
        /// </summary>
        /// <value>
        /// The get text.
        /// </value>
        public FlowDocument GetText { get; set; }
        public WriteText()
        {
            InitializeComponent();
        }
    }
}
