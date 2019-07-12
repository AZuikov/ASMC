using AP.Reports.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AP.Reports.AutoDocumets;
using AP.Utils.Data;

namespace AP.Reports.Utils
{
    public class FromFormatToStringConverter
    {
        /// <summary>
        /// Возвращает массив строк, соответствующий всем поддерживаемым форматам
        /// </summary>
        /// <param name="report"></param>
        /// <returns></returns>
        public static string[] GetFormatsStringArray(ITextGraphicsReport report)
        {
            string[] formats = new string[report.Formats.Length];
            for (int i = 0; i < report.Formats.Length; i++)
            {
                formats[i]  = ((Enum)report.Formats.GetValue(i)).GetStringValue();
            }
            return formats;
        }

        /// <summary>
        /// Возвращает строку, использующуюся для фильтра файлов в FileDialog
        /// </summary>
        /// <param name="report"></param>
        /// <returns></returns>
        public static string GetStringForFileDialog(ITextGraphicsReport report)
        {
            StringBuilder strb = new StringBuilder(report.Formats.Length*4);
            foreach (var format in GetFormatsStringArray(report))
            {
                strb.Append($"(*.{format})|*.{format}|");
            }
            strb.Remove(strb.Length - 1, 1);
            return strb.ToString();
        }

        /// <summary>
        /// Возвращает строку, использующуюся для фильтра файлов в FileDialog
        /// </summary>
        /// <param name="reports"></param>
        /// <returns></returns>
        public static string GetAllStringForDialog(ITextGraphicsReport[] reports)
        {
            StringBuilder strb = new StringBuilder("Документы ");
            //Filter = "Документы (.docx), (.xlsx)|*.docx;*.xlsx"
            foreach (var report in reports)
            {
                foreach (var format in GetFormatsStringArray(report))
                {
                    strb.Append($"(.{format}), ");
                }
            }
            strb.Remove(strb.Length - 2, 2);
            strb.Append("|");
            foreach (var report in reports)
            {
                foreach (var format in GetFormatsStringArray(report))
                {
                    strb.Append($"*.{format}; ");
                }
            }
            strb.Remove(strb.Length - 2, 2);
            return strb.ToString();
        }

        public static bool IsItWordFile(string format)
        {
            var formatsArray = Enum.GetValues(typeof(Word.FileFormat));
            string[] formats = new string[formatsArray.Length];

            for (int i = 0; i < formatsArray.Length; i++)
            {
                formats[i] = ((Enum)formatsArray.GetValue(i)).GetStringValue();
            }
            return formats.Where(a => a == format).Any();
        }
    
    }
}
