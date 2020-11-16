using System;
using ASMC.Data.Model;
using ASMC.Data.Model.PhysicalQuantity;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTestProject1
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestIsPointBelong()
        {
            var point = new MeasPoint<Voltage, Frequency>( 2.5m, 20);
            var a = new RangeStorage<PhysicalRange<Voltage, Frequency>>(
                new PhysicalRange<Voltage, Frequency>(
                    new MeasPoint<Voltage, Frequency>(1, 1), 
                    new MeasPoint<Voltage, Frequency>(2, 50)),

                new PhysicalRange<Voltage, Frequency>(
                    new MeasPoint<Voltage, Frequency>(3, 1),
                    new MeasPoint<Voltage, Frequency>(4, 50)));
            Assert.IsTrue(a.IsPointBelong(point));
            //a = new RangeStorage<PhysicalRange<Voltage, Frequency>>(
            //    new PhysicalRange<Voltage, Frequency>(new MeasPoint<Voltage, Frequency>(20, 1), 
            //        new MeasPoint<Voltage, Frequency>(50, 50)));
            //Assert.IsTrue(!a.IsPointBelong(point));

        }
        [TestMethod]
        public void TestTolMeasPointInRange()
        {
            var point = new MeasPoint<Current, Frequency>(4, UnitMultiplier.Mili, 40);
            var a = new RangeStorage<PhysicalRange<Current, Frequency>>(
                new PhysicalRange<Current, Frequency>(
                    new MeasPoint<Current, Frequency>(0, 40),
                    new MeasPoint<Current, Frequency>(20, UnitMultiplier.Mili, 500), new AccuracyChatacteristic(0.000050m, null, 0.8m))
               );
            //Assert.IsTrue(a.GetTolMeasPoint(point) as MeasPoint<Current, Frequency> ==
            //              new MeasPoint<Current, Frequency>(0.082m, UnitMultiplier.Mili, point.AdditionalPhysicalQuantity));
            Assert.IsTrue((MeasPoint<Current, Frequency>)a.GetTolMeasPoint(point) ==
                         new MeasPoint<Current, Frequency>(0.082m, UnitMultiplier.Mili, point.AdditionalPhysicalQuantity));
            //a = new RangeStorage<PhysicalRange<Voltage, Frequency>>(
            //    new PhysicalRange<Voltage, Frequency>(new MeasPoint<Voltage, Frequency>(20, 1), 
            //        new MeasPoint<Voltage, Frequency>(50, 50)));
            //Assert.IsTrue(!a.IsPointBelong(point));

        }

        [TestMethod]
        public void TestGetRangeInMeasPoint()
        {
            var point = new MeasPoint<Current, Frequency>(4, UnitMultiplier.Mili, 40);
            var range = new PhysicalRange<Current, Frequency>(
                new MeasPoint<Current, Frequency>(0, 40),
                new MeasPoint<Current, Frequency>(20, UnitMultiplier.Mili, 500),
                new AccuracyChatacteristic(0.000050m, null, 0.8m)); 
            var range1 = new PhysicalRange<Current, Frequency>(
                new MeasPoint<Current, Frequency>(30, 40),
                new MeasPoint<Current, Frequency>(40, UnitMultiplier.Mili, 500),

                new AccuracyChatacteristic(0.000050m, null, 0.8m));

            var a = new RangeStorage<PhysicalRange<Current, Frequency>>(range, range1);
                Assert.IsTrue(a.GetRangePointBelong(point)== range);


            //a = new RangeStorage<PhysicalRange<Voltage, Frequency>>(
            //    new PhysicalRange<Voltage, Frequency>(new MeasPoint<Voltage, Frequency>(20, 1), 
            //        new MeasPoint<Voltage, Frequency>(50, 50)));
            //Assert.IsTrue(!a.IsPointBelong(point));

        }
        [TestMethod]
        public void TestGetRangeNotExistMeasPoint()
        {
            var point = new MeasPoint<Current, Frequency>(25, UnitMultiplier.Mili, 40);
            var range = new PhysicalRange<Current, Frequency>(
                new MeasPoint<Current, Frequency>(0, 40),
                new MeasPoint<Current, Frequency>(20, UnitMultiplier.Mili, 500),
                new AccuracyChatacteristic(0.000050m, null, 0.8m));
            var range1 = new PhysicalRange<Current, Frequency>(
                new MeasPoint<Current, Frequency>(30, 40),
                new MeasPoint<Current, Frequency>(40, UnitMultiplier.Mili, 500),

                new AccuracyChatacteristic(0.000050m, null, 0.8m));

            var a = new RangeStorage<PhysicalRange<Current, Frequency>>(range, range1);
            Assert.IsTrue(a.GetRangePointBelong(point) == null);
            //a = new RangeStorage<PhysicalRange<Voltage, Frequency>>(
            //    new PhysicalRange<Voltage, Frequency>(new MeasPoint<Voltage, Frequency>(20, 1), 
            //        new MeasPoint<Voltage, Frequency>(50, 50)));
            //Assert.IsTrue(!a.IsPointBelong(point));

        }
        [TestMethod]
        public void TestGetNoramalizeValueToSi()
        {
            var point = new MeasPoint<Voltage, Frequency>(15, 20);
            Assert.IsTrue(point.MainPhysicalQuantity.GetNoramalizeValueToSi() == 15);
            point = new MeasPoint<Voltage, Frequency>(15, UnitMultiplier.Kilo, 20, UnitMultiplier.Mega);
            Assert.IsTrue(point.MainPhysicalQuantity.GetNoramalizeValueToSi() == (decimal) (15*1E3));
        }
        [TestMethod]
        public void TestEquallyMeasPoint()
        {
            Assert.IsTrue(new MeasPoint<Voltage>(15, UnitMultiplier.Mili)== new MeasPoint<Voltage>(15, UnitMultiplier.Mili));
        }
        [TestMethod]
        public void TestNotEquallyMeasPoint()
        {
            Assert.IsTrue(new MeasPoint<Voltage>(15, UnitMultiplier.Mili) != new MeasPoint<Voltage>(16, UnitMultiplier.Mili));
        }
        [TestMethod]
        public void TestEquallyMeasPointAdditional()
        {
            Assert.IsTrue(new MeasPoint<Current, Frequency>(15, UnitMultiplier.Mili,50) == new MeasPoint<Current, Frequency>(15, UnitMultiplier.Mili, 50));
        }
        [TestMethod]
        public void TestNotEquallyMeasPointAdditional()
        {
            Assert.IsTrue(new MeasPoint<Voltage, Frequency>(15, UnitMultiplier.Mili,50) != new MeasPoint<Voltage, Frequency>(16, UnitMultiplier.Mili,50));
        }
        [TestMethod]
        public void TestSubtractionMeasPoint()
        {
            var point = new MeasPoint<Voltage>(15, UnitMultiplier.Mili );
            var point1 = new MeasPoint<Voltage>(15, UnitMultiplier.Micro);
            Assert.IsTrue(point - point1 == new MeasPoint<Voltage>(0.014985m));
          
        }
        [TestMethod]
        public void TestAdditionMeasPoint()
        {
            var point = new MeasPoint<Voltage>(15, UnitMultiplier.Mili);
            var point1 = new MeasPoint<Voltage>(15, UnitMultiplier.Micro);
            Assert.IsTrue(point + point1 == new MeasPoint<Voltage>(0.015015m));
        }
    }
}
