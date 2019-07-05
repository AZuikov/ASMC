using System;
using System.Text;
using System.Collections.Generic;
using System.Data;
using AP.Utils.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTest.AP.Utils
{
    /// <summary>
    /// Сводное описание для UnitTest2
    /// </summary>
    [TestClass]
    public class UnitTest2
    {
        public enum MyEnum
        {    [StringValue("dasda")]
           Dsadasda 
        }
        [TestMethod]
        public void TestMethod1()
        {
            MyEnum.Dsadasda.GetDoubleValue();
        }
    }
}
