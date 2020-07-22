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
using ASMC.ViewModel;

namespace ASMC.View
{
    /// <summary>
    /// Логика взаимодействия для SettingView.xaml
    /// </summary>
    public partial class SettingView : UserControl
    {
        /// <inheritdoc />
        public SettingViewModel SettingViewModel
        {
            get => (SettingViewModel)GetValue(SettingViewModelProperty);
            set => SetValue(SettingViewModelProperty, value);
        }
        /// <summary>
        /// Определяет свойство зависимостей <see cref="AllowCreate"/>.
        /// </summary>
        public static readonly DependencyProperty SettingViewModelProperty =
            DependencyProperty.Register(
                nameof(SettingViewModel),
                typeof(SettingViewModel), typeof(SettingView) ,
        new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));  
       

        public SettingView()
        {
            InitializeComponent();
        }
    }
}
