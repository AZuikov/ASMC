using DevExpress.Mvvm.UI.Interactivity;
using System.Windows;
using System.Windows.Controls;

namespace ASMC.Common.Behavior
{
    public class IsFocesedUiBehavior :Behavior<UIElement>
    {
        public static readonly DependencyProperty SelectValueProperty = DependencyProperty.Register(nameof(SelectValue),
            typeof(object), typeof(IsFocesedUiBehavior), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public object SelectValue
        {
            get => SelectValueProperty;
            set => SetValue(SelectValueProperty, value);
        }


        protected override void OnAttached()
        {
            base.OnAttached();
            this.AssociatedObject.GotFocus += AssociatedObject_GotFocus;
        }
    
        private void AssociatedObject_GotFocus(object sender, RoutedEventArgs e)
        {
            var tb = (Control) sender;
            SelectValue =tb.DataContext;
            
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            this.AssociatedObject.GotFocus -= AssociatedObject_GotFocus;
        }
    }
}
