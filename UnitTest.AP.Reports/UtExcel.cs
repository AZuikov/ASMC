using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using AP.Reports.AutoDocumets;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Data;
using System.Drawing.Imaging;
using System.Resources;
using Microsoft.Office.Interop.Word;
using SystemOffice = Microsoft.Office.Interop.Word.System;
using DataTable = System.Data.DataTable;
using AP.Reports.Utils;
using Document = AP.Reports.AutoDocumets.Document;

namespace UnitTest.AP.Reports
{
    [TestClass]
    public class UtExcel
    {
        private Excel _excel;
        private string _pathToTestFolder = @"C:\Tests\";
        private string[] _pages = new[] {"Bookmark", "Insert", "Replase"};

        #region True tests
        [TestMethod]
        public void TestMethodCreateTestFile()
        {
            using (_excel = new Excel())
            {
                _excel.NewDocument(_pages);
                _excel.MoveToCell(2, 2, _pages[0]);
                _excel.AddBookmarkToCell("AddTable");
                _excel.MoveToCell(3, 13, _pages[0]);
                _excel.AddBookmarkToCell("AddImage");
                _excel.MoveToCell(8, 2, _pages[0]);
                _excel.AddBookmarkToCell("FillTable");
                _excel.MoveToCell(15, 13, _pages[0]);
                _excel.AddBookmarkToCell("AddImageWithScale");
                _excel.MoveToCell(10, 4, _pages[0]);
                _excel.InsertText("Этот текст должен остаться");
                _excel.MoveToCell(11, 6, _pages[0]);
                _excel.InsertText("Этот текст должен остаться");

                _excel.SaveAs(_pathToTestFolder + "Test.xlsx");
                //=======================================

                _excel.MoveToCell(5, 2, _pages[2]);
                _excel.InsertText("ЗаменитьНаИзображениеОдинРаз");
                _excel.MoveToCell(6, 2, _pages[2]);
                _excel.InsertText("ЗаменитьНаИзображениеОдинРаз");

                _excel.MoveToCell(8, 2, _pages[2]);
                _excel.InsertText("ЗаменитьНаИзображениеСМасштабомОдинРаз");
                _excel.MoveToCell(9, 10, _pages[2]);
                _excel.InsertText("ЗаменитьНаИзображениеСМасштабомОдинРаз");

                _excel.MoveToCell(12, 2, _pages[2]);
                _excel.InsertText("ЗаменитьНаТекстОдинРаз");
                _excel.MoveToCell(14, 10, _pages[2]);
                _excel.InsertText("ЗаменитьНаТекстОдинРаз");

                _excel.MoveToCell(22, 2, _pages[2]);
                _excel.InsertText("ЗаменитьНаИзображениеВсе");
                _excel.MoveToCell(24, 10, _pages[2]);
                _excel.InsertText("ЗаменитьНаИзображениеВсе");
                _excel.MoveToCell(26, 15, _pages[2]);
                _excel.InsertText("ЗаменитьНаИзображениеВсе");

                _excel.MoveToCell(28, 2, _pages[2]);
                _excel.InsertText("ЗаменитьНаИзображениеСМасштабомВсе");
                _excel.MoveToCell(30, 10, _pages[2]);
                _excel.InsertText("ЗаменитьНаИзображениеСМасштабомВсе");
                _excel.MoveToCell(32, 15, _pages[2]);
                _excel.InsertText("ЗаменитьНаИзображениеСМасштабомВсе");

                _excel.MoveToCell(34, 2, _pages[2]);
                _excel.InsertText("ЗаменитьНаТекстВсе");
                _excel.MoveToCell(36, 2, _pages[2]);
                _excel.InsertText("ЗаменитьНаТекстВсе");
                _excel.MoveToCell(38, 2, _pages[2]);
                _excel.InsertText("ЗаменитьНаТекстВсе");
                //==============================================
                _excel.Save();
            }
        }

        [TestMethod]
        public void TestMethodAllInsertFunctoins()
        {
            using (_excel = new Excel())
            {
                if (!System.IO.File.Exists(_pathToTestFolder + "Test.xlsx"))
                {
                    TestMethodCreateTestFile();
                }
                _excel.OpenDocument(_pathToTestFolder + "Test.xlsx");

                _excel.MoveToCell(1,1,_pages[1]);
                Bitmap image = GetBitmapImage();
                DataTable dt = GetRandomDataTable();
                dt.TableName = "TestTable";

                for (int i = 0; i < 10; i++)
                {
                    _excel.MoveSheetHome();
                    _excel.InsertText("Text" + i.ToString());
                    _excel.MoveSheetHome();
                    _excel.InsertText("");
                    _excel.MoveSheetHome();
                    _excel.InsertTable(dt, GetCondition());
                    _excel.MoveToCell(i + 5, 12);
                    _excel.InsertImage(image);
                }
                _excel.SaveAs(_pathToTestFolder + "TestInserted.xlsx");
            }
        }

        [TestMethod]
        public void TestMethodAllReplaceByBookmark()
        {
            using (_excel = new Excel())
            {
                if (!System.IO.File.Exists(_pathToTestFolder + "Test.xlsx"))
                {
                    TestMethodCreateTestFile();
                }
                _excel.OpenDocument(_pathToTestFolder + "Test.xlsx");
                DataTable dt = GetRandomDataTable();
                Bitmap image = GetBitmapImage();
                _excel.InsertNewTableToBookmark("AddTable", dt, GetCondition());
                _excel.InsertImageToBookmark("AddImage", image);
                _excel.InsertImageToBookmark("AddImageWithScale",image, (float)0.5);
                _excel.FillTableToBookmark("FillTable", dt, false, GetCondition());
                _excel.SaveAs(_pathToTestFolder + "TestBmReplased.xlsx");
            }
        }

        [TestMethod]
        public void TestMethodFindStringAndReplase()
        {
            using (_excel = new Excel())
            {
                //if (!System.IO.File.Exists(_pathToTestFolder + "Test.xlsx"))
                //{
                //    TestMethodCreateTestFile();
                //}
                _excel.OpenDocument(_pathToTestFolder + "Test.xlsx");
                DataTable dt = GetRandomDataTable();
                Bitmap image = GetBitmapImage();
                _excel.FindStringAndReplace("ЗаменитьНаТекстОдинРаз", "ЗамененоНаТекстОдинРаз");
                _excel.FindStringAndReplaceImage("ЗаменитьНаИзображениеОдинРаз", image);
                _excel.FindStringAndReplaceImage("ЗаменитьНаИзображениеСМасштабомОдинРаз", image, (float)0.5);

                _excel.FindStringAndAllReplace("ЗаменитьНаТекстВсе", "ЗамененоНаТекстВсе");
                _excel.FindStringAndAllReplaceImage("ЗаменитьНаИзображениеВсе", image);
                _excel.FindStringAndAllReplaceImage("ЗаменитьНаИзображениеСМасштабомВсе", image, (float)0.5);
                _excel.SaveAs(_pathToTestFolder + "TestStrReplased.xlsx");
            }
        }

        [TestMethod]
        public void TestMethodMerge()
        {
            using (_excel = new Excel())
            {
                if (!System.IO.File.Exists(_pathToTestFolder + "TestInserted.xlsx"))
                {
                    TestMethodAllInsertFunctoins();
                }
                if (!System.IO.File.Exists(_pathToTestFolder + "TestStrReplased.xlsx"))
                {
                    TestMethodFindStringAndReplase();
                }
                _excel.OpenDocument(_pathToTestFolder + "TestStrReplased.xlsx");

                List<string> pathsList = new List<string>();
                pathsList.Add(_pathToTestFolder + "TestInserted.xlsx");
                pathsList.Add(_pathToTestFolder + "TestStrReplased.xlsx");
                pathsList.Add(_pathToTestFolder + "TestInserted.xlsx");
                pathsList.Add(_pathToTestFolder + "TestStrReplased.xlsx");
                pathsList.Add(_pathToTestFolder + "TestInserted.xlsx");
                pathsList.Add(_pathToTestFolder + "TestStrReplased.xlsx");
                pathsList.Add(_pathToTestFolder + "TestInserted.xlsx");
                pathsList.Add(_pathToTestFolder + "TestStrReplased.xlsx");
                pathsList.Add(_pathToTestFolder + "TestInserted.xlsx");
                pathsList.Add(_pathToTestFolder + "TestStrReplased.xlsx");
                pathsList.Add(_pathToTestFolder + "TestInserted.xlsx");
                pathsList.Add(_pathToTestFolder + "TestStrReplased.xlsx");

                _excel.MergeDocuments(pathsList);
                _excel.SaveAs(_pathToTestFolder + "TestMerge.xlsx");
            }
        }



        public enum FileFormat
        {
            /// <summary>
            /// Документ
            /// </summary>
            Docx,
            /// <summary>
            /// Шаблон
            /// </summary>
            Dotx,
            /// <summary>
            /// С поддержкой макросов
            /// </summary>
            Docm
        }


    

        [TestMethod]
        public void AddBookmark()
        {
            using (_excel = new Excel())
            {
                _excel.NewDocument();
                _excel.SaveAs(_pathToTestFolder + "@@@@");
            }
        }

        #endregion

        private DataTable GetRandomDataTable()
        {
            DataTable dt = new DataTable();
            for (int i = 0; i < 10; i++)
            {
                dt.Columns.Add(new DataColumn("col" + i.ToString(), i.GetType()));
            }
            for (int i = 0; i < 15; i++)
            {
                dt.Rows.Add(dt.NewRow());
            }
            Random rnd = new Random();
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                for (int j = 0; j < dt.Columns.Count; j++)
                {
                    dt.Rows[i][j] = rnd.Next(10);
                }
            }

            dt.TableName = "Table" + rnd.Next(100) + "_CreatedByRandom";
            return dt;
        }

        private Document.ConditionalFormatting GetCondition()
        {
            Document.ConditionalFormatting cf = new Document.ConditionalFormatting();
            cf.Value = "10";
            cf.Color = Color.MediumTurquoise;
            cf.NameColumn = "col3";
            cf.Condition = Document.ConditionalFormatting.Conditions.MoreOrEqual;
            cf.Region = Document.ConditionalFormatting.RegionAction.Row;
            return cf;
        }

        private Bitmap GetBitmapImage()
        {
            Bitmap image;
            if (!System.IO.File.Exists(_pathToTestFolder + "TestImage.bmp"))
            {
                int x = 200;
                int y = 200;
                image = new Bitmap(x, y);
                for (int i = 0; i < x; i++)
                for (int j = 0; j < y; j++)
                {
                    image.SetPixel(i, j, Color.CadetBlue);
                }
                image.Save(_pathToTestFolder + "TestImage.bmp", System.Drawing.Imaging.ImageFormat.Bmp);
            }
            image = new Bitmap(_pathToTestFolder + "TestImage.bmp");
            return image;
        }
    }
}