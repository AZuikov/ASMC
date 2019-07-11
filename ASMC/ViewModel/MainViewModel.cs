using ASMC.Core.ViewModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using ASMC.Core.UI;
using NLog;
using AP.Reports.AutoDocumets;

namespace ASMC.ViewModel
{
    public class MainViewModel : BaseViewModel
    {
        private FlowDocument _document;
        public FlowDocument Document1
        {
            get { return _document; }
        }

        public MainViewModel()
        {
          
            


            this.Initialize();

            var word = new Word();
            word.OpenDocument(@"Z:\ОГМетр\Внутренние\Документы - Общие\AutoMeas\Протоколы\ProtocolCreatorTemplates\HeaderAlbum.dotx");

            DataTable weatherDataTable = new DataTable();
            weatherDataTable.Columns.Add(new DataColumn("Контролируемые параметры", typeof(string)));
            weatherDataTable.Columns.Add(new DataColumn("Требования НД", typeof(string)));
            weatherDataTable.Columns.Add(new DataColumn("Измеренные значения", typeof(string)));
            int rowsNum = 0;
            for(int i = 0; i < 3; i++)
            {

                weatherDataTable.Rows.Add(weatherDataTable.NewRow());
                weatherDataTable.Rows[rowsNum][0] = i.ToString();
                weatherDataTable.Rows[rowsNum][2] = i.ToString();
                rowsNum++;
            }


            word.FillTableToBookmark("weatherTable", weatherDataTable);
            word.MoveEnd();
            word.SaveAs(@"C:\Users\02tav01\Documents\1488.docx");
            word.Close();
            _document = new FlowDocument();
            TextRange tr = new TextRange(_document.ContentStart, _document.ContentEnd);
            using (var stream = new FileStream(@"\\zrto.int\ogmetr\AutoMeas\AutoMeas\PatchInfo — копия.rtf", FileMode.Open))
            {
                tr.Load(stream, DataFormats.Rtf);
                stream.Close();
            }


            //var dsadas = new MessageBoxService();
            //Confirm("У выбранного экземпляра отсутствуют события МК.\n" +
            //        "Пожалуйста, создайте событие МК и повторите попытку.\n" +
            //        "Приложение будет закрыто.", true, MessageBoxResult.No, false, dsadas);
            //Task loadingTask = new Task(() =>
            //{

            //    Confirm("У выбранного экземпляра отсутствуют события МК.\n" +
            //            "Пожалуйста, создайте событие МК и повторите попытку.\n" +
            //            "Приложение будет закрыто.", true, MessageBoxResult.No, false, dsadas);
            //});

            //StartTaskAndShowProgressService("Идет загрузка данных из базы",
            //    "Подождите...", loadingTask, null, new ProgressService());


        }

    }
}
