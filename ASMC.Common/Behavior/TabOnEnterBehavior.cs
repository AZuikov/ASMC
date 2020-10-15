using DevExpress.Mvvm.UI.Interactivity;
using System.Windows;
using System.Windows.Input;

namespace ASMC.Common.Behavior
{
    public class TabOnEnterBehavior: Behavior<UIElement>
    {
        /// <inheritdoc />
        protected override void OnAttached()
        {
            AssociatedObject.PreviewKeyDown += AssociatedObject_PreviewKeyDown;
        }

        private void AssociatedObject_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key != Key.Enter) return;

            var request = new TraversalRequest(FocusNavigationDirection.Next);
            request.Wrapped = true;
            AssociatedObject.MoveFocus(request);
        }

        /// <inheritdoc />
        protected override void OnDetaching()
        {
            AssociatedObject.PreviewKeyDown -= AssociatedObject_PreviewKeyDown;
        }
    }
}
