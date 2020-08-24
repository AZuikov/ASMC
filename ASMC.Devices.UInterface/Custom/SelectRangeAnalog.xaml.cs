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

namespace ASMC.Devices.UInterface.Custom
{
    /// <summary>
    /// Логика взаимодействия для SelectRangeAnalog.xaml
    /// </summary>
    public partial class SelectRangeAnalog : UserControl
    {
        public static readonly DependencyProperty IsComboBoxProperty =  DependencyProperty.Register(nameof(IsComboBox), typeof(bool),typeof(SelectRangeAnalog), new PropertyMetadata(true));

        public bool IsComboBox
        {
            get => (bool)GetValue(IsComboBoxProperty);
            set => SetValue(IsComboBoxProperty, value);
        }

        public SelectRangeAnalog()
        {
            InitializeComponent();
        }
    }
}
