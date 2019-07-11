using System.Windows;
using ASMC.Core.UI;
using ASMC.View;
using ASMC.ViewModel;
using DevExpress.Mvvm;

namespace ASMC
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : Application
    {
     
      
        protected override  void OnStartup(StartupEventArgs e)
        {
            var  splashScreen = new SplashScreen("Resources/ce91c577bcf0a799e275968ebe599d0e.png");
            splashScreen.Show(true,true);
            base.OnStartup(e);
            InvokeMainWindow();
        }
        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
        }
        /// <summary>
        /// Вызов главного окна приложения
        /// </summary>
        private void InvokeMainWindow()
        {
            var service = new WindowService();  
            service.Show("LoginWindow", new MainViewModel(), null, this);
        }

    }
}
