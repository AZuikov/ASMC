using ASMC.Core.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using ASMC.Core.UI;
using NLog;

namespace ASMC.ViewModel
{
    public class MainViewModel : BaseViewModel
    {
        public MainViewModel()
        {
            
            this.Initialize();
            var dsadas = new MessageBoxService();
            Confirm("У выбранного экземпляра отсутствуют события МК.\n" +
                    "Пожалуйста, создайте событие МК и повторите попытку.\n" +
                    "Приложение будет закрыто.", true, MessageBoxResult.No, false, dsadas);
            Task loadingTask = new Task(() =>
            {

                Confirm("У выбранного экземпляра отсутствуют события МК.\n" +
                        "Пожалуйста, создайте событие МК и повторите попытку.\n" +
                        "Приложение будет закрыто.", true, MessageBoxResult.No, false, dsadas);
            });

            StartTaskAndShowProgressService("Идет загрузка данных из базы",
                "Подождите...", loadingTask, null, new ProgressService());


        }

    }
}
