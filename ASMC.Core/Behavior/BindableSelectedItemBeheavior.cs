using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;

namespace ASMC.Core.Behavior
{
    public class BindableSelectedItemBeheavior : Behavior<TreeView>
    {
        public static readonly DependencyProperty SelectItemProperty = DependencyProperty.Register(nameof(SelectItem),
            typeof(object), typeof(BindableSelectedItemBeheavior), new UIPropertyMetadata(null, PropertyChangedCallback));

        private static void PropertyChangedCallback(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            if(!(e.NewValue is TreeViewItem item)) return;
            item.SetValue(TreeViewItem.IsSelectedProperty, true);
        }

        public object SelectItem
        {
            get
            {
                return SelectItemProperty;
            }
            set
            {
                SetValue(SelectItemProperty, value);
            }
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
