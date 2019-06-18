using System;
using System.Data;
using System.Net.Configuration;
using AP.Utils.Data;
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
           var  ekz = new Ekz(3838);
            var test = new AP.Utils.Data.EntityMapper<Data.Model.Metr.Ekz>();
           EntityContext dsada = new EntityContext(dp);
         var  teest =dsada.Load(ekz);
        }
    }
}
