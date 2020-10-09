using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using ASMC.Data.Model.Interface;

namespace ASMC.Core.View
{
    /// <summary>
    ///     Логика взаимодействия для InputTableView.xaml
    /// </summary>
    public partial class InputTableControl : UserControl
    {

        public static readonly DependencyProperty DataProperty;

        public static readonly DependencyProperty IsOnlyReadProperty;

        public static readonly DependencyProperty SelectValueProperty = DependencyProperty.Register(nameof(SelectValue),
            typeof(object), typeof(InputTableControl),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));


        public static readonly DependencyProperty WidthCellProperty =
            DependencyProperty.Register(nameof(WidthCell), typeof(double), typeof(InputTableControl), new PropertyMetadata(70.0));

        static InputTableControl()
        {
            
            DataProperty =
                DependencyProperty.Register(nameof(Data), typeof(BindingList<ICell>), typeof(InputTableControl), new FrameworkPropertyMetadata(new BindingList<ICell>(), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
            IsOnlyReadProperty =
                DependencyProperty.Register(nameof(IsOnlyRead), typeof(bool), typeof(InputTableControl), new PropertyMetadata(false));
        }

        public double WidthCell
        {
            get => (double) GetValue(WidthCellProperty);  
            set
            {
                SetValue(WidthCellProperty,value-10);
            }
        }
        public InputTableControl()
        {
            InitializeComponent();
        }
        public bool IsOnlyRead
        {
            get => (bool)GetValue(IsOnlyReadProperty);
            set
            {
                SetValue(WidthCellProperty, value);
            }
        }
        public BindingList<ICell> Data
        {

            get => (BindingList<ICell>) GetValue(DataProperty);
            set
            {
                SetValue(DataProperty, value);
            }
        }

        public object SelectValue { get; set; }
    }
}