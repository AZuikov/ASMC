using System;
using System.Collections.Generic;
using ASMC.Data.Model;
using ASMC.Data.Model.PhysicalQuantity;

namespace ASMC.Devices.WithoutInterface.HourIndicator.IchGost577
{
    public sealed class Ich_02 : Ich_10_03_02
    {
        public Ich_02()
        {
            UserType = "ИЧ02";
        }

        /// <param name="currentAccuracyClass"></param>
        /// <inheritdoc />
        protected override RangeStorage<PhysicalRange<Length>> GetRangesFull(
            AccuracyClass.Standart currentAccuracyClass)
        {
            var arr = new List<PhysicalRange<Length>>();
            decimal fullRange;
            decimal range;
            switch (currentAccuracyClass)
            {
                case AccuracyClass.Standart.Zero:
                    fullRange = 0.000015m;
                    range = 0.000008m;
                    break;
                case AccuracyClass.Standart.First:
                    fullRange = 0.000020m;
                    range = 0.000010m;
                    break;
                case AccuracyClass.Standart.Second:
                    fullRange = 0.000025m;
                    range = 0.000012m;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(currentAccuracyClass), currentAccuracyClass, null);
            }

            for (var i = 1; i <= 2; i++) arr.Add(GeneratoRanges(i - 1, i));


            PhysicalRange<Length> GeneratoRanges(decimal start, decimal end)
            {
                var st = new MeasPoint<Length>(new Length(start, UnitMultiplier.Mili));
                var ed = new MeasPoint<Length>(new Length(end, UnitMultiplier.Mili));
                return new PhysicalRange<Length>(st, ed, new AccuracyChatacteristic(range, null, null));
            }

            var ac = new AccuracyChatacteristic(fullRange, null, null);
            return new RangeStorage<PhysicalRange<Length>>(ac, arr.ToArray());
        }
    }
}