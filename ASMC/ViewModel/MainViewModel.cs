using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Windows.Input;
using DevExpress.Mvvm;
using Palsys.Core.ViewModel;
using Palsys.Metr.Modules;
using Palsys.Utils.Localization;

namespace ASMC.ViewModel
{
    public class MainViewModel : ClosableViewModel
    {
        private CultureInfo _currentLanguage;
        private IEnumerable<IModule> _modules;

        public MenuViewModel Menu { get; } = new MenuViewModel();

        public CultureInfo[] LanguageList
        {
            get;
        }

        public CultureInfo CurrentLanguage
        {
            get => _currentLanguage;
            set => SetProperty(ref _currentLanguage, value, nameof(CurrentLanguage),
                () => LocalizationManager.Default.CurrentCulture = value);
        }

        public ICommand SelectLanguageCommand
        {
            get;
        }

        public ICommand ShowCompanyCommand
        {
            get;
        }

        public MainViewModel()
        {
            SelectLanguageCommand = new DelegateCommand<CultureInfo>(lang => CurrentLanguage = lang);
            ShowCompanyCommand = new DelegateCommand(() => OpenSite(LocalizationManager.Default["CompanySiteAddress"]));

            LanguageList = new[]
            {
                CultureInfo.GetCultureInfo("ru"),
                CultureInfo.GetCultureInfo("en")
            };
            CurrentLanguage = LocalizationManager.Default.CurrentCulture;

            LocalizationManager.Default.CurrentCultureChanged += (sender, args) =>
            {
                CurrentLanguage = LocalizationManager.Default.CurrentCulture;
                RebuildItems(_modules);
            };
        }

        public void RebuildItems(IEnumerable<IModule> modules)
        {
            _modules = modules;

            Menu.Items.Clear();

            if(_modules == null)
                return;

            foreach(var mod in _modules)
            {
                var methods = mod.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public).Where(m =>
                    m.IsDefined(typeof(BrowsableAttribute)) && m.GetParameters().Length == 0);

                foreach(var mi in methods)
                {
                    var description = mi.GetCustomAttribute<DescriptionAttribute>()?.Description;

                    var locName = LocalizationManager.Default[mi.GetCustomAttribute<DisplayNameAttribute>()?.DisplayName ?? mi.Name];
                    var locDescription = description != null ? LocalizationManager.Default[description] : null;

                    Menu.Items.Add(new MenuViewModel.Item(locName, locDescription, () =>
                    {
                        try
                        {
                            mi.Invoke(mod, null);
                        }
                        catch(Exception e)
                        {
                            if(!Alert(e.InnerException ?? e))
                                throw;
                        }
                    }));
                }
            }
        }

        private void OpenSite(string uri)
        {
            try
            {
                Process.Start(uri);
            }
            catch(Exception e)
            {
                if(!Alert(e))
                    throw;
            }
        }
    }
}
