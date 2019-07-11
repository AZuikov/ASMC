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

namespace ASMC.Core.View
{
    /// <summary>
    /// Логика взаимодействия для UserControl1.xaml
    /// </summary>
    public partial class UserControl1 : UserControl
    {
        public static readonly  DependencyProperty DocumentProperty = DependencyProperty.Register("Document", typeof(string), typeof(UserControl1), new PropertyMetadata(string.Empty));
        public static readonly DependencyProperty Document1Property = DependencyProperty.Register("Document1", typeof(string), typeof(UserControl1), new PropertyMetadata(string.Empty));
        public string Document
        {
            get { return GetValue(DocumentProperty).ToString(); }
            set
            {
                SetValue(DocumentProperty, value);
            }
        }
        public string Document1
        {
            get
            {
                return GetValue(Document1Property).ToString();
            }
            set
            {
                SetValue(Document1Property, value);
            }
        }
        public UserControl1()
        {
            InitializeComponent();
        }
    }
}
