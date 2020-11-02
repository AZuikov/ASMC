using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using ASMC.Data.Model;
using ASMC.Data.Model.PhysicalQuantity;

namespace ASMC.Devices.WithoutInterface.HourIndicator
{
    /// <summary>
    /// Презосталвет базовую реализацию часового интидикатора
    /// </summary>
    public class IchBase : IUserType
    {
        #region Property

        public MaxMeasuringForce MeasuringForce { get; set; }

        /// <summary>
        /// Максимальное допустимое отклонение стрелки при перпендикулярном нажиме на его ось.
        /// </summary>
        public decimal PerpendicularPressureMax { get; set; }

        /// <summary>
        /// Позволяет задать полный измерительный диапазон.
        /// </summary>
        /// <param name="currentAccuracyClass"></param>
        /// <returns></returns>
        protected virtual RangeStorage<PhysicalRange<Length>> GetRangesFull(AccuracyClass.Standart currentAccuracyClass)
        {
            return null;
        }
        /// <summary>
        /// Здает измерительный диапазон.
        /// </summary>
        public RangeStorage<PhysicalRange<Length>> RangesFull { get => GetRangesFull(CurrentAccuracyClass); }

        #endregion
        public AccuracyClass.Standart CurrentAccuracyClass { get; set; }

        /// <summary>
        /// Доступные класы точности.
        /// </summary>
        public AccuracyClass.Standart[] AvailabeAccuracyClass { get; } = {AccuracyClass.Standart.Zero, AccuracyClass.Standart.First, AccuracyClass.Standart.Second};

        /// <inheritdoc />
        public string UserType { get; set; }

       
        public MeasPoint<Length> Arresting { get => GetArresting(CurrentAccuracyClass); }

        protected virtual MeasPoint<Length> GetArresting(AccuracyClass.Standart currentAccuracyClass)
        {
            throw new NotImplementedException();
        }

        public MeasPoint<Length> Variation { get => GetVariation(CurrentAccuracyClass); }

        protected virtual MeasPoint<Length> GetVariation(AccuracyClass.Standart currentAccuracyClass)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Максимальное допутсимое усилие
        /// </summary>
        public struct MaxMeasuringForce
        {
            #region Property

            /// <summary>
            /// Измерение хода
            /// </summary>
            public MeasPoint<Force> ChangeCourse { get; set; }

            /// <summary>
            /// Колибание при прямом/обратном ходу
            /// </summary>
            public MeasPoint<Force> Oscillatons { get; set; }

            /// <summary>
            /// Максимальное усилие прямом ходе
            /// </summary>
            public MeasPoint<Force> StraightRun { get; set; }

            #endregion
        }
    }
    /// <summary>
    /// Предоставляет реализацию часового индикатора по ГОСТ 577
    /// </summary>
    public abstract class IchGost577 : IchBase
    {
        protected IchGost577()
        {
            MeasuringForce = new MaxMeasuringForce
            {
                StraightRun = new MeasPoint<Force>( 1.5M),
                Oscillatons = new MeasPoint<Force>(0.6M),
                ChangeCourse = new MeasPoint<Force>( 0.5m)
            };
        }
    }
    /// <summary>
    /// Предоставляет реализацию индикатора часовога дити с диапазоном 10мм  по <see cref="IchGost577"/>
    /// </summary>
    public sealed class Ich_10 : IchGost577
    {
        public Ich_10()
        {
            this.UserType = "ИЧ10";
        }
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

        /// <param name="currentAccuracyClass"></param>
        /// <inheritdoc />
        protected override RangeStorage<PhysicalRange<Length>> GetRangesFull(
            AccuracyClass.Standart currentAccuracyClass)
        {
            var arr = new List<PhysicalRange<Length>>();
            double fullRange;
            double range;
            switch (currentAccuracyClass)
            {
                case AccuracyClass.Standart.Zero:
                    fullRange = 0.15;
                    range = 0.08;
                    break;
                case AccuracyClass.Standart.First:
                    fullRange = 0.20;
                    range = 0.10;
                    break;
                case AccuracyClass.Standart.Second:
                    fullRange = 0.25;
                    range = 0.12;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(currentAccuracyClass), currentAccuracyClass, null);
            }
            for (int i = 1; i <= 10; i++)
            {
                arr.Add(GeneratoRanges(i - 1, i));
            }

            
            PhysicalRange <Length> GeneratoRanges (decimal start, decimal end)
            {
                var st = new MeasPoint<Length>(new Length(start, UnitMultiplier.Mili));
                var ed = new MeasPoint<Length>(new Length(end, UnitMultiplier.Mili));
                return new PhysicalRange<Length>(st, ed, new AccuracyChatacteristic((decimal?) range,null,null));
            }

            var ac = new AccuracyChatacteristic((decimal?) fullRange,null, null);
            return new RangeStorage<PhysicalRange<Length>>(ac,arr.ToArray());
        }


    }
}