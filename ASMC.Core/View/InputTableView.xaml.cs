using System.Windows;
using System.Windows.Controls;
using ASMC.Core.Behavior;
using ASMC.Data.Model;
using ASMC.Data.Model.Interface;

namespace ASMC.Core.View
{
    /// <summary>
    ///     Логика взаимодействия для InputTableView.xaml
    /// </summary>
    public partial class InputTableView : UserControl
    {
        public static readonly DependencyProperty DataProperty =
            DependencyProperty.Register(nameof(Data), typeof(IBasicOperation<double>[]), typeof(InputTableView));

        public static readonly DependencyProperty IsOnlyReadProperty =
            DependencyProperty.Register(nameof(IsOnlyRead), typeof(bool), typeof(InputTableView), new PropertyMetadata(false));

        public static readonly DependencyProperty SelectValueProperty = DependencyProperty.Register(nameof(SelectValue),
            typeof(IBasicOperation<double>), typeof(InputTableView),
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
        public IBasicOperation<double>[] Data { get; set; }

        public IBasicOperation<double> SelectValue { get; set; }
    }
}