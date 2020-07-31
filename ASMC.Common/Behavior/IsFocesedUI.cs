using DevExpress.Mvvm.UI.Interactivity;
using System.Windows;
using System.Windows.Controls;

namespace ASMC.Common.Behavior
{
    public class IsFocesedUi :Behavior<UIElement>
    {
        public static readonly DependencyProperty SelectValueProperty = DependencyProperty.Register(nameof(SelectValue),
            typeof(object), typeof(IsFocesedUi), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public object SelectValue
        {
            get { return SelectValueProperty; }
            set {SetValue(SelectValueProperty, value);} }


        protected override void OnAttached()
        {
            base.OnAttached();
            this.AssociatedObject.GotFocus += AssociatedObject_GotFocus;
        }
    
        private void AssociatedObject_GotFocus(object sender, RoutedEventArgs e)
        {
            var tb = (Control) sender;
            SelectValue =tb.DataContext;
            //SelectValue1.Getting = rnd.NextDouble();
            //tb.Text = SelectValue1.Getting.ToString();
            // throw new NotImplementedException();
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            this.AssociatedObject.GotFocus -= AssociatedObject_GotFocus;
        }
    }
}
