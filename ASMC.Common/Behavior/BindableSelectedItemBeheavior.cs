using DevExpress.Mvvm.UI.Interactivity;
using System.Windows;
using System.Windows.Controls;

namespace ASMC.Common.Behavior
{
    public class BindableSelectedItemBeheavior : Behavior<TreeView>
    {
        public static readonly DependencyProperty SelectItemProperty = DependencyProperty.Register(nameof(SelectItem),
            typeof(object), typeof(BindableSelectedItemBeheavior), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        //private static void PropertyChangedCallback(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        //{
        //    if(!(e.NewValue is TreeViewItem item)) return;
        //    item.SetValue(TreeViewItem.IsSelectedProperty, true);
        //}

        public object SelectItem
        {
            get => SelectItemProperty;
            set => SetValue(SelectItemProperty, value);
        }


        protected override void OnAttached()
        {
            base.OnAttached();
            this.AssociatedObject.SelectedItemChanged += AssociatedObject_SelectedItemChanged;
        }

        private void AssociatedObject_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            this.SelectItem = e.NewValue;
        }



        protected override void OnDetaching()
        {
            base.OnDetaching();
            if (this.AssociatedObject!=null)
            {
                this.AssociatedObject.SelectedItemChanged -= AssociatedObject_SelectedItemChanged;
            }
            
        }
    }
}
