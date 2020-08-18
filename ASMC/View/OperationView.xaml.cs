using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ASMC.View
{
    /// <summary>
    /// Логика взаимодействия для OperationView.xaml
    /// </summary>
    public partial class OperationView : UserControl
    {
        public readonly DependencyProperty IsManualPropery = DependencyProperty.Register(nameof(IsManual), typeof(bool), typeof(OperationView),new FrameworkPropertyMetadata(true));
       public bool IsManual
       {
           get { return (bool) GetValue(IsManualPropery);}
           set{SetValue(IsManualPropery,value);}
       }
        public OperationView()
        {
            InitializeComponent();
        }
    }
}
