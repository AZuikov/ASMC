using System.Windows;
using ASMC.View;

namespace ASMC
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : Application
    {
     
      
        protected override  void OnStartup(StartupEventArgs e)
        {
            /*Картинка загрузки*/
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
            var mainWindow = new MainWindow()
            {
                DataContext = new LoginWindow(),
            };
            mainWindow.Show();
        }

    }
}
