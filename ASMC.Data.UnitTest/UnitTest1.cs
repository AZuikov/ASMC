using System;
using System.Data;
using System.Net.Configuration;
using AP.Reports;
using AP.Utils.Data;
using ASMC.Core.ValidationRules;
using ASMC.Data.Model.Metr;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ASMC.Data.UnitTest
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            var dp = new SqlDataProvider();
            dp.LoadFromUdl(@"C:\Users\02tav01\Documents\Palitra_System\Metr6\UDL\Metr6.udl");
            var ekz = new Ekz
            {
                Id = 3838
            };
            Barcode выфв = new Barcode();
            //var dsdsa = new EntityContext(dp);
            //dsdsa.Load(ekz);

        }
    }
}
