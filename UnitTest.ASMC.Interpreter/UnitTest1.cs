using System;
using ASMC.Interpreter;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTest.ASMC.Interpreter
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            var pars = new Parse();
              pars.Pars("var a=1+1E10\nvar b=\"Hello WOLD\"\nvar d=44--5\nvar e=d+a\nvar c=b+d");

            //var srt=NormalizerRegular.Spaces.Replace(" dsadsdasdsa " +'\n'+
            //                                         "dsadsadsad  ffds     4234 ", " ");
            //   srt = NormalizerRegular.SpacesTrim.Replace(srt, string.Empty).TrimEnd();
           
        }
    }
}
