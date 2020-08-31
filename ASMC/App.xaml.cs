using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using DevExpress.Mvvm;
using System.Windows;
using AP.Utils.Data;
using ASMC.Core;
using ASMC.Data.Model;
using ASMC.Data.Model.Interface;
using ASMC.Properties;
using ASMC.ViewModel;
using DevExpress.Mvvm.Native;
using DevExpress.Mvvm.UI;
using DevExpress.Xpf.Core;
using NLog;

namespace ASMC
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : Application
    {
        #region Fields

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        #endregion

        #region Properties

      

        public static IDataProvider DataProvider
        {
            get; private set;
        }

     

        #endregion

        static App()
        {
            // Prevents loading assembly DevExpress.Xpf.Themes.Office2016White.v17.2
            // by DevExpress.Xpf.Core.v17.2
            ApplicationThemeHelper.ApplicationThemeName = "None";
            CultureInfo.CurrentCulture= new CultureInfo("ru-Ru");
            ViewLocator.Default = new ViewLocator(Assembly.GetExecutingAssembly());
        }
        private void LoadPlugins()
        {
            var path = $@"{Directory.GetCurrentDirectory()}\Plugins";
            if(!Directory.Exists(path))
                return;

            var files = Directory.GetFiles(path, "*.dll", SearchOption.AllDirectories);
            try
            {
                foreach(var file in files)
                        Assembly.LoadFile(Path.GetFullPath(file));
            }
            catch(Exception e)
            {
                Logger.Error(e);
            }

           

        }
        #region Methods
        protected override async void OnStartup(StartupEventArgs e)
        {
            var splashScreen = new SplashScreen("Resources/ce91c577bcf0a799e275968ebe599d0e.png");
            splashScreen.Show(true, true);
            base.OnStartup(e);

            try
            {
                InitializeSettings();
                InitializeLocalization();
                LoadPlugins();
                if(!LaunchWindow("DefaultWindowService"))
                {
                    Shutdown();
                }

                await InitializeUserLangAsync();
            }
            catch(Exception error)
            {
                Logger.Fatal(error);
                ShowMessage(error.Message, MessageBoxImage.Error, MessageBoxButton.OK);
                Shutdown(1);
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            try
            {
                //if(Settings != null)
                //{
                //    Settings.Save();
                //}
            }
            catch(Exception error)
            {
                Logger.Error(error);
                ShowMessage(error.Message, MessageBoxImage.Error, MessageBoxButton.OK);
            }


            base.OnExit(e);
        }

        private void InitializeLocalization()
        {
            //LocalizationManager.Default.RegisterProviders(
            //    new ResxLocalizationProvider(
            //        "Palsys.Metr.IDA.Resources",
            //        Assembly.GetEntryAssembly()));

         
            //if(!string.IsNullOrEmpty(lang))
            //{
            //    LocalizationManager.Default.CurrentCulture = CultureInfo.GetCultureInfo(lang);
            //}
        }

        private void InitializeSettings()
        {
            //Settings = new SettingsManager<AssemblySettings>(new JsonSettingsProvider());
        }   
     

        private async Task InitializeUserLangAsync()
        {
            try
            {
                //var lang = await Task.Factory.StartNew(() => UserContext.GetLang(DataProvider));

            }
            catch(Exception error)
            {
                
            }
        }


        private bool LaunchWindow(string serviceKey)
        {
            if(!(TryFindResource(serviceKey) is IWindowService windowService))
                return false;

            windowService.Show("WizardView", CreateViewModel());
            return true;
        }

        private WizardViewModel CreateViewModel()
        {
            var viewModel = new WizardViewModel();
            //var mainSettings = Settings.UserScope.Main;

          
          

            return viewModel;
        }

        private MessageBoxResult ShowMessage(string message, MessageBoxImage icon, MessageBoxButton button)
        {
            if(!(TryFindResource("MessageBoxService") is IMessageBoxService service))
                return MessageBoxResult.None;

            return service.Show(message, null, button, icon);
        }

        #endregion
    }
 

}

