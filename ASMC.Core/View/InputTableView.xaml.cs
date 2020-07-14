using System.Windows;
using System.Windows.Controls;
using ASMC.Core.Behavior;

namespace ASMC.Core.View
{
    /// <summary>
    ///     Логика взаимодействия для InputTableView.xaml
    /// </summary>
    public partial class InputTableView : UserControl
    {
        public static readonly DependencyProperty DataProperty =
            DependencyProperty.Register(nameof(Data), typeof(object[]), typeof(InputTableView));

        public static readonly DependencyProperty IsOnlyReadProperty =
            DependencyProperty.Register(nameof(IsOnlyRead), typeof(bool), typeof(InputTableView), new PropertyMetadata(false));

        public static readonly DependencyProperty SelectValueProperty = DependencyProperty.Register(nameof(SelectValue),
            typeof(object), typeof(InputTableView),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));


        public static readonly DependencyProperty WidthCellProperty =
            DependencyProperty.Register(nameof(WidthCell), typeof(double), typeof(InputTableView), new PropertyMetadata(70.0));

        public double WidthCell
        {
            get => (double) GetValue(WidthCellProperty);  
            set
            {
                SetValue(WidthCellProperty,value-10);
            }
        }
        public InputTableView()
        {
            InitializeComponent();
        }
        public bool IsOnlyRead
        {
            get; set;
        }
        public object[] Data { get; set; }

        public object SelectValue { get; set; }
    }
}