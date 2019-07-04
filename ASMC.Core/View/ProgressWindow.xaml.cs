using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Input;

namespace ASMC.Core.View
{
     /// <summary>
    /// Interaction logic for ProgressView.xaml
    /// </summary>
    public partial class ProgressWindow : Window
    {
        #region Fields

        public static readonly DependencyProperty AllowCancelProperty =
            DependencyProperty.Register("AllowCancel", typeof(bool), typeof(ProgressWindow));

        public static readonly DependencyProperty CancellingProperty =
            DependencyProperty.Register("Cancelling", typeof(bool), typeof(ProgressWindow));

        public static readonly RoutedUICommand CancelCommand =
            new RoutedUICommand("Cancel", "Cancel", typeof(ProgressWindow));

        #endregion

        #region Properties

        public bool AllowCancel
        {
            get => (bool) GetValue(AllowCancelProperty);
            set => SetValue(AllowCancelProperty, value);
        }

        public bool Cancelling
        {
            get => (bool) GetValue(CancellingProperty);
            set => SetValue(CancellingProperty, value);
        }

        #endregion

        public ProgressWindow()
        {
            CommandBindings.Add(new CommandBinding(CancelCommand, OnCancelCommandExecuted, CancelCommandCanExecute));

            InitializeComponent();
        }

        #region Methods

        protected override void OnClosing(CancelEventArgs e)
        {
            e.Cancel = !(Tag is bool);
            base.OnClosing(e);
        }

        private void CancelCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = AllowCancel && !Cancelling;
        }

        private void OnCancelCommandExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            Cancelling = true;
        }

        #endregion
    }

   
}
