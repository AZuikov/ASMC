using System;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using AP.Reports.AutoDocumets;
using AP.Reports.Utils;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Document = AP.Reports.AutoDocumets.Document;

namespace UnitTest.AP.Reports
{
    [TestClass]
    public class UtWord
    {
        string _pathToTestFolder = @"C:\Users\02tav01\Documents";

        private Word word;
        [DataRow(@"321321")]
        //[DataRow(@"вфывфыdsadasDac")]
        //[DataRow(@"≥®")]
        //[DataRow(@"C°")]
        [TestMethod]
        public void InsertText(string value)
        {
            word=new Word();
           // word.OpenDocument(@"C:\Users\02tav01\Documents\Документ Microsoft Word1 — копия — копия.docx");
            //word.MoveEnd();
            word.NewDocumentTemp(@"C:\Users\02tav01\Documents\Документ Microsoft Word.dotx");
            word.MergeDocuments(@"C:\Users\02tav01\Documents\Документ Microsoft Word1 — копия — копия.docx");
            
          
            ////word.InsertText("Таблица 1");
            //word.MoveEnd();
            //var dt = GetRandomDataTable();
            //word.InsertTable(dt, GetCondition());
            //word.MoveEnd();
            //word.InsertText("fdsfs");
           // word.InsertTable(dt, GetCondition());

            //word.InsertText("Таблица 2");
            //word.MoveHome();
            //word.InsertText("Таблица 3");
            //dt = GetRandomDataTable();
            //word.InsertTable(dt, GetCondition());
            //dt = GetRandomDataTable();
            //word.InsertTable(dt, GetCondition());

            word.SaveAs("hghfhfg");
            //word.Close();








            // word.NewDocument();
            //for (int i = 0; i < 5; i++)
            //{   
            //    word.InsertText($"{value}");
            //}
            //word.SaveAs(@"C:\Users\02tav01\Documents\Документ Microsoft WorD шаблон.docx");
            //word.Close();
            // Assert.AreEqual(word.SetElement.LastChild.InnerText, $"{value}");
        }

        [TestMethod]
        [DataRow(@"C:\Документ Microsoft Word1.docx")]
        [DataRow(@"C:\Документ Microsoft Word1.dotx")]
        [DataRow(@"C:\Документ Microsoft Word1.docm")]
        [DataRow(@"C:\Документ Microsoft Word1.dfocm.docm")]
        public void CheckPathToDocument(string path)
        {
            word = new Word {Path = $"{path}"};
        }  

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        [DataRow(@"C:\Документ Microsoft Word1.Docx")]
        [DataRow(@"C:\Документ Microsoft Word1.d0tx")]
        [DataRow(@"C:\Документ Microsoft Word1.doc")]
        [DataRow(@"C:\Документ Microsoft Word1.dfocm.txt")]
        public void CheckPathToDocumentFormatException(string path)
        {
            word = new Word { Path = $"{path}" };
        }    

        //[TestMethod]
        //public void SetMoveHome()
        //{
        //    word = new Word(); 
        //    word.NewDocument();
        //    word.MoveHome();
        //    Assert.IsTrue(word.SetElement.Parent is Body); 
        //}

        //[TestMethod]
        //public void SetMoveEnd()
        //{
        //    word = new Word();
        //    word.NewDocument();
        //    word.MoveEnd();
        //    Assert.IsTrue(word.SetElement is Paragraph);
        //}

        [TestMethod]
        public void TestMethod2()
        {
            word = new Word();
            word.OpenDocument(@"C:\Users\02tav01\Documents\Документ Microsoft Word1 — копия — копия.docx");
            //var dataTable = new DataTable();
            //for(var i = 0; i < 6; i++)
            //{
            //    dataTable.Columns.Add("a" + i);
            //}
            //DataRow[] dataRow = new DataRow[10];
            //for(var i = 0; i < 10; i++)
            //{
            //    dataRow[i] = dataTable.NewRow();
            //    for(var j = 0; j < 6; j++)
            //    {
            //        dataRow[i][j] = i + j.ToString();
            //    }
            //    dataTable.Rows.Add(dataRow[i]);
            //}
            //word.FillTableToBookmark("test1",dataTable);
            word.FillTableToBookmark("test", new DataTable());
            word.SaveAs(@"C:\Users\02tav01\Documents\Документ Microsoft Word3.docx");
            word.Close();
        }

        [TestMethod]
        public void TestMethodTableIsaev()
        {
            word = new Word();
            word.OpenDocument(@"C:\Users\02tav01\Documents\Документ Microsoft Word1 — копия — копия.docx");
            // word.NewDocument();
            //word.InsertText("Таблица 1");
            word.MoveEnd();
            var dt = GetRandomDataTable();
            word.InsertTable(dt, GetCondition());

            //word.InsertText("Таблица 2");
            //word.MoveEnd();
            ////word.InsertText("Таблица 3");
            //dt = GetRandomDataTable();
            //word.InsertTable(dt, GetCondition());
            //word.MoveEnd();
            //dt = GetRandomDataTable();
            //word.InsertTable(dt, GetCondition());

            word.SaveAs(_pathToTestFolder + "TableTestResult.docx");
            //word.Save();
            //word.Close();
        }

        [TestMethod]
        public void TestMethodImageIsaev()
        {
            word = new Word();
            word.OpenDocument(@"C:\Users\02tav01\Documents\Документ Microsoft Word1.docx");
            CreateBitmapImage("TestImage.bmp", System.Drawing.Color.DarkSeaGreen, 200, 200);
            CreateBitmapImage("TestImage2.bmp", System.Drawing.Color.LightSkyBlue, 300, 100);

            word.InsertImage(new Bitmap(_pathToTestFolder + "TestImage.bmp"));
            word.InsertImage(new Bitmap(_pathToTestFolder + "TestImage2.bmp"));
            word.InsertImage(new Bitmap(_pathToTestFolder + "TestImage.bmp"), (float)0.5);

            word.InsertImage(new Bitmap(@"C:\Users\02tav01\Pictures\График.JPG"), (float)0.5);

            word.InsertImage(new Bitmap(_pathToTestFolder + "TestImage2.bmp"), (float)0.5);
            word.InsertImage(new Bitmap(_pathToTestFolder + "Безымянный.jpg"), (float)2);
            

            word.SaveAs(_pathToTestFolder + "ImageTest.docx");
            word.Save();
            word.Close();
        }
        [TestMethod]
        public void FindStringAndAllReplaceImage()
        {
            word = new Word();
            word.OpenDocument(@"C:\Users\02tav01\Documents\Документ Microsoft Word1.docx");  
            word.FindStringAndAllReplaceImage("#dasdas#", new Bitmap(@"C:\Users\02tav01\Pictures\График.JPG"), (float)0.1);
            word.Save();
            word.Close();
        }
        [TestMethod]
        public void TestMethod3()
        {
            word = new Word();
            word.OpenDocument(@"C:\Users\02tav01\Documents\Документ Microsoft Word1.docx");
            Bitmap mb = new Bitmap(100,100);
            Graphics graphics = Graphics.FromImage(mb);
            Rectangle rectangle = new Rectangle(0, 0, mb.Width, mb.Height);
            System.Drawing.Imaging.BitmapData bitmapData =
                mb.LockBits(rectangle, ImageLockMode.ReadOnly, mb.PixelFormat);
            mb.UnlockBits(bitmapData);
            //word.InsertImage(mb);   
            word.InsertImage(new Bitmap(@"C:\Users\02tav01\Pictures\График.JPG"));
            word.SaveAs(@"C:\Users\02tav01\Documents\Документ Microsoft Word3.docx");
            word.Close();
        }

        private Document.ConditionalFormatting GetCondition()
        {
            Document.ConditionalFormatting cf = new Document.ConditionalFormatting();
            cf.Value = "5";
            cf.Color = System.Drawing.Color.MediumTurquoise;
            cf.NameColumn = "col3";
            cf.Condition = Document.ConditionalFormatting.Conditions.Less;
            cf.Region = Document.ConditionalFormatting.RegionAction.Cell;
            return cf;
        }

        private void CreateBitmapImage(string name, System.Drawing.Color color, int x, int y)
        {
            Bitmap image;
            if (!System.IO.File.Exists(_pathToTestFolder + name))
            {
                image = new Bitmap(x, y);
                for (int i = 0; i < x; i++)
                for (int j = 0; j < y; j++)
                {
                    image.SetPixel(i, j, color);
                }
                image.Save(_pathToTestFolder + name, System.Drawing.Imaging.ImageFormat.Bmp);
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
                    dt.Rows[i][j] = rnd.Next(10);
                }
            }

            dt.TableName = "Table" + rnd.Next(100) + "_CreatedByRandom";
            return dt;
        }
    }

}
