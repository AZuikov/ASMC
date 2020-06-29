using System;
using System.Data;
using System.Net.Configuration;
using AP.Reports;
using AP.Utils.Data;
using ASMC.Core.ValidationRules;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ASMC.Data.UnitTest
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            using(var dp = new SqlDataProvider())
            {
                //var Query = $"up_gr_EkzmkSelect  @fltr = 'ekzmk.idekzmk = 72701'";
                //dp.LoadFromUdl(@"C:\Users\" + Environment.UserName + @"\Documents\Palitra_System\Metr6\UDL\Metr6.udl");
                //var EventMc = (new EntityMapper()).Map<EventMc>(dp.Execute(Query).Rows[0]);
                //dp.CloseConnection();
                //var fdfsdnew = new EntityContext(dp);
                //fdfsdnew.Load(EventMc.Ekz);
                //var fdfsdnew1 = new EntityContext(dp);
                //fdfsdnew1.Load(EventMc.Ekz.StandardSizeMi);
            }
         

        }
    }
}
