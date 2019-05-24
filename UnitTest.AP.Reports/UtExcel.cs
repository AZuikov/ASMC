using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using AP.Reports.AutoDocumets;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Data;
using Microsoft.Office.Interop.Word;
using System = Microsoft.Office.Interop.Word.System;
using DataTable = System.Data.DataTable;
using AP.Reports.Utils;

namespace UnitTest.AP.Reports
{
    [TestClass]
    public class UtExcel
    {
        private Excel _excel;

        [TestMethod]
        public void TestMethodReplaceAll()
        {
            using (_excel = new Excel())
            {
                _excel.OpenDocument(@"C:\Users\02ias01\Documents\Tests\~TestReplace.xlsx");
                _excel.FindStringAndAllReplace("Слово", "Воробей");
                _excel.FindStringAndAllReplaceImage("Картинка", new Bitmap(@"C:\Users\02ias01\Documents\Tests\~Image.jpg"));
                _excel.SaveAs(@"C:\Users\02ias01\Documents\Tests\TestReplaceAllResult.xlsx");
            }
        }

        [TestMethod]
        public void TestMethodReplace()
        {
            using (_excel = new Excel())
            {
                _excel.OpenDocument(@"C:\Users\02ias01\Documents\Tests\~TestReplace.xlsx");
                _excel.FindStringAndReplace("Слово", "Воробей");
                _excel.FindStringAndReplaceImage("Картинка", new Bitmap(@"C:\Users\02ias01\Documents\Tests\~Image.jpg"));
                _excel.SaveAs(@"C:\Users\02ias01\Documents\Tests\TestReplaceResult.xlsx");
            }
        }

        [TestMethod]
        public void TestMethodReplaceAndCurcorMove()
        {
            using (_excel = new Excel())
            {
                _excel.OpenDocument(@"C:\Users\02ias01\Documents\Tests\~TestReplace.xlsx");
                _excel.MoveEnd();
                _excel.InsertText("Последняя ячейка");
                _excel.MoveHome();
                _excel.InsertImage(new Bitmap(@"C:\Users\02ias01\Documents\Tests\~Image.jpg"));
                _excel.MoveToCell(5,5,"Лист1");
                _excel.InsertText("5 - 5");
                _excel.MoveToCell("D6","Лист1");
                _excel.InsertText("D6");
                _excel.SaveAs(@"C:\Users\02ias01\Documents\Tests\TestReplaceAndCurcorMove.xlsx");
            }
        }

        [TestMethod]
        public void TestMethodReplaceByBookmark()
        {
            using (_excel = new  Excel())
            {
                _excel.OpenDocument(@"C:\Users\02ias01\Documents\Tests\~TestReplace.xlsx");
                _excel.InsertImageToBookmark("диапазон_для_картинки", new Bitmap(@"C:\Users\02ias01\Documents\Tests\~Image.jpg"));
                _excel.InsertTextToBookmark("диапазон_для_текста", "Текст, вставленный из программы");
                DataTable dt = GetRandomDataTable();
                //_excel.InsertNewTableToBookmark("диапазон_для_таблицы", dt, GetCondition());
                _excel.MoveToCell(24,13, "Лист2");
                _excel.InsertTable(dt, GetCondition());
                _excel.SaveAs(@"C:\Users\02ias01\Documents\Tests\TestReplaceByBookMarkResult.xlsx");
            }
        }

        [TestMethod]
        public void TestMethodFillsTableByBookmark()
        {
            using (_excel = new Excel())
            {
                _excel.OpenDocument(@"C:\Users\02ias01\Documents\Tests\~TestReplace.xlsx");
                _excel.FillsTableToBookmark("диапазон_для_таблицы", GetRandomDataTable(), false, GetCondition());
                _excel.SaveAs(@"C:\Users\02ias01\Documents\Tests\TestFillsTableByBookmark.xlsx");
            }
        }

        [TestMethod]
        public void TestMethodMergeDocs()
        {
            using (_excel = new Excel())
            {
                _excel.OpenDocument(@"C:\Users\02ias01\Documents\Tests\~DocToMerge1.xlsx");

                List<string> pathsList = new List<string>();
                pathsList.Add(@"C:\Users\02ias01\Documents\Tests\~DocToMerge2.xlsx");
                pathsList.Add(@"C:\Users\02ias01\Documents\Tests\~DocToMerge1.xlsx");
                pathsList.Add(@"C:\Users\02ias01\Documents\Tests\~DocToMerge2.xlsx");
                pathsList.Add(@"C:\Users\02ias01\Documents\Tests\~DocToMerge1.xlsx");
                pathsList.Add(@"C:\Users\02ias01\Documents\Tests\~DocToMerge2.xlsx");
                pathsList.Add(@"C:\Users\02ias01\Documents\Tests\~DocToMerge1.xlsx");
                pathsList.Add(@"C:\Users\02ias01\Documents\Tests\~DocToMerge2.xlsx");

                _excel.MergeDocuments(pathsList);
                _excel.SaveAs(@"C:\Users\02ias01\Documents\Tests\TestMergeDocs.xlsx");
            }
        }

        [TestMethod]
        public void TestMethodMultyTabs()
        {
            using (_excel = new Excel())
            {
                _excel.NewDocument(new string[] {"Таблицы"});
                _excel.InsertTable(GetRandomDataTable(), GetCondition());
                _excel.InsertText(" ");
                _excel.InsertTable(GetRandomDataTable(), GetCondition());
                _excel.InsertText(" ");
                _excel.InsertTable(GetRandomDataTable(), GetCondition());
                _excel.SaveAs(@"C:\Users\02ias01\Documents\Tests\TestMultyTabs.xlsx");
            }
        }

        [TestMethod]
        public void TestMethodMultyTabs2()
        {
            using (_excel = new Excel())
            {
                _excel.OpenDocument(@"C:\Users\02ias01\Documents\Tests\TestMultyTabs.xlsx");
                _excel.MoveHome();
                _excel.InsertTable(GetRandomDataTable(), GetCondition());
                _excel.InsertText(" ");
                _excel.InsertTable(GetRandomDataTable(), GetCondition());
                _excel.InsertText(" ");
                _excel.InsertTable(GetRandomDataTable(), GetCondition());
                _excel.SaveAs(@"C:\Users\02ias01\Documents\Tests\TestMultyTabs.xlsx");
            }
        }

        [TestMethod]
        public void TestMethodInsert()
        {
            using (_excel = new Excel())
            {
                _excel.NewDocument();
                _excel.MoveHome();
                _excel.InsertText("First");
                _excel.MoveHome();
                _excel.InsertText("Second");
                _excel.MoveHome();
                _excel.InsertText("Third");
                _excel.MoveHome();
                _excel.InsertText("4-th");
                _excel.MoveHome();
                _excel.InsertText("5-th");
                _excel.MoveHome();
                _excel.InsertText("6-th");
                _excel.MoveHome();
                _excel.InsertText("7-th");
                _excel.MoveHome();
                _excel.InsertText("8-th");
                _excel.MoveHome();
                _excel.InsertText("9-th");

                _excel.SaveAs(@"C:\Users\02ias01\Documents\Tests\TestInsert.xlsx");
            }
        }


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
                    dt.Rows[i][j] = rnd.Next(1);
                }
            }

            dt.Rows[3][2] = 2;
            dt.TableName = "Table" + rnd.Next(100) + "_CreatedByRandom";
            return dt;
        }

        private ConditionalFormatting GetCondition()
        {
            ConditionalFormatting cf = new ConditionalFormatting();
            cf.Value = "2";
            cf.Color = Color.MediumTurquoise;
            cf.NameColumn = "col3";
            cf.Condition = ConditionalFormatting.Conditions.Equal;
            cf.Region = ConditionalFormatting.RegionAction.Row;
            return cf;
        }
    }
}