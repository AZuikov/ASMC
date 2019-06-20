using System.IO;

namespace ASMC.Core.View
{
    /// <summary>
    /// Логика взаимодействия для ReadAndWriteText.xaml
    /// </summary>
    public partial class ProgressWindow 
    {
     
        public ProgressWindow()
        {
            InitializeComponent();
           // Send = new DelegateCommand(SendMetod, () => Data != null);
            using (FileStream fs = new FileStream(@"\\zrto.int\ogmetr\AutoMeas\AutoMeas\PatchInfo.rtf", FileMode.Open))
            {
               // InputUserText = XamlReader.Load(fs, DataFormats.Rtf) as FlowDocument;
            }
        }

    }

   
}
