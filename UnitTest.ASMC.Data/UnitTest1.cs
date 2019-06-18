using System.Data;
using System.Data.Common;
using AP.Utils.Data;
using ASMC.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTest.ASMC.Data
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            IDataProvider dataProvider = new SqlDataProvider();
            dataProvider.LoadFromUdl(@"C:\Users\02tav01\Documents\Palitra_System\Metr6\UDL\Metr6.udl");
            dataProvider.OpenConnection();
            DataTable dataTable = dataProvider.Execute("[dbo].[up_ds_EkzSelect]",dataProvider.GetParameter("@fltr", DbType.AnsiString, "IDEKZ=8"));

        }
    }
}
