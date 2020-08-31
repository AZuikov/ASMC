using System.Windows;
using System.Windows.Controls;
using ASMC.Data.Model;

namespace ASMC.View
{
    /// <summary>
    /// Логика взаимодействия для OperationView.xaml
    /// </summary>
    public partial class OperationView : UserControl
    {
        #region Fields

        public static readonly DependencyProperty IsManualPropery =
            DependencyProperty.Register(nameof(IsManual), typeof(bool), typeof(OperationView),
                                        new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        //public readonly DependencyProperty SelectItemTestPropery =
        //    DependencyProperty.Register(nameof(SelectItemTest), typeof(object), typeof(OperationView),
        //                                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        #endregion

        #region Property

        public bool IsManual
        {
            get => (bool) GetValue(IsManualPropery);
            set => SetValue(IsManualPropery, value);
        }
        public IUserItemOperationBase SelectItemTest { get; set; }

        //public object SelectItemTest
        //{
        //    get => (object)GetValue(SelectItemTestPropery);
        //    set => SetValue(SelectItemTestPropery, value);
        //}

        #endregion

        public OperationView()
        {
            InitializeComponent();
        }
    }
}