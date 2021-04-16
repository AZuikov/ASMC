using System;
using ASMC.Data.Model;
using ASMC.Data.Model.PhysicalQuantity;

namespace ASMC.Devices.WithoutInterface.HourIndicator.IchGost577
{
    /// <summary>
    ///     Предоставляет реализацию индикатора часовога дити с диапазоном 10мм  по <see cref="IchGost577" />
    /// </summary>
    public abstract class Ich_10_03_02 : IchGost577
    {
        /// <inheritdoc />
        protected override MeasPoint<Length> GetArresting(AccuracyClass.Standart currentAccuracyClass)
        {
            switch (currentAccuracyClass)
            {
                case AccuracyClass.Standart.Zero:
                    return new MeasPoint<Length>(3, UnitMultiplier.Micro);
                case AccuracyClass.Standart.First:
                    return new MeasPoint<Length>(3, UnitMultiplier.Micro);
                case AccuracyClass.Standart.Second:
                    return new MeasPoint<Length>(4, UnitMultiplier.Micro);
                default:
                    throw new ArgumentOutOfRangeException(nameof(currentAccuracyClass), currentAccuracyClass, null);
            }
        }

        /// <inheritdoc />
        protected override MeasPoint<Length> GetVariation(AccuracyClass.Standart currentAccuracyClass)
        {
            switch (currentAccuracyClass)
            {
                case AccuracyClass.Standart.Zero:
                    return new MeasPoint<Length>(2, UnitMultiplier.Micro);
                case AccuracyClass.Standart.First:
                    return new MeasPoint<Length>(3, UnitMultiplier.Micro);
                case AccuracyClass.Standart.Second:
                    return new MeasPoint<Length>(5, UnitMultiplier.Micro);
                default:
                    throw new ArgumentOutOfRangeException(nameof(currentAccuracyClass), currentAccuracyClass, null);
            }
        }
    }
}