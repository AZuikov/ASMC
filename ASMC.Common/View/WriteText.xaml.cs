using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace ASMC.Common.View
{
    /// <summary>
    /// Логика взаимодействия для WriteText.xaml
    /// </summary>
    public partial class WriteText : UserControl
    {
        public static readonly DependencyProperty DocumentProperty = DependencyProperty.Register("Document", typeof(FlowDocument), typeof(WriteText), new FrameworkPropertyMetadata(null));
       
        public FlowDocument Document
        {
            get
            {
                return (FlowDocument)GetValue(DocumentProperty);
            }
            set
            {
                SetValue(DocumentProperty, value);
            }
        }    
        public WriteText()
        {
            //InitializeComponent();
            //RichTextBox dsad = new RichTextBox();
            //TextRange tr = new TextRange(dsad.Document.ContentStart, dsad.Document.ContentEnd);
            //using(var stream = new FileStream(@"\\zrto.int\ogmetr\AutoMeas\AutoMeas\PatchInfo — копия.rtf", FileMode.Open))
            //{
            //    tr.Load(stream, DataFormats.Rtf);
            //    stream.Close();
            //}

            // Documentdsfs = dsad.Document;
        }
    }
}
