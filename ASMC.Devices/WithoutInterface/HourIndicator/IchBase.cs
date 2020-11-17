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
        public decimal PerpendicularPressureMax { get; protected set; }

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
        public MeasPoint<Length> Diametr { get; set; }
        /// <summary>
        /// Позволяет получить тест соответствия допуску шероховатости наружной поверхности гильзы.
        /// </summary>
        public string LinerRoughness { get; protected set; }
        /// <summary>
        /// Позволяет получить тест соответствия допуску шероховатости поверхности измерительного наконечника.
        /// </summary>
        public string TipRoughness { get; protected set; }
        protected virtual MeasPoint<Length> GetVariation(AccuracyClass.Standart currentAccuracyClass)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// Позволяет получить допустимый диапазон ширины стрелки.
        /// </summary>
        public PhysicalRange<Length> ArrowWidch { get; protected set; }

        /// <summary>
        /// Позволяет получить минимальную допустимую длинну штриха деления.
        /// </summary>
        public MeasPoint<Length> StrokeLength { get; protected set; }

        /// <summary>
        ///   Позволяет получить 
        /// </summary>
        public ConnectionDiametr ConnectDiametr { get; protected set; } = new ConnectionDiametr();
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

        public class ConnectionDiametr
        {
            public PhysicalRange<Length> Range { get; set; }
            public MeasPoint<Length> MaxDelta { get; set; }
        }
        /// <summary>
        /// Позволяет получить максимальное растояние между концом стрелки и циферблатом.
        /// </summary>
        public MeasPoint<Length> BetweenArrowDial { get; protected set; }

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
            PerpendicularPressureMax = 0.5m;
            LinerRoughness = "Ra не более 0,63 мкм";
            TipRoughness = "Ra не более 0,1 мкм";
            ArrowWidch = new PhysicalRange<Length>(new MeasPoint<Length>(0.15m, UnitMultiplier.Mili), new MeasPoint<Length>(0.20m, UnitMultiplier.Mili));
            StrokeLength = new MeasPoint<Length>(1, UnitMultiplier.Mili);
            ConnectDiametr.Range = new PhysicalRange<Length>(new MeasPoint<Length>(7.985m, UnitMultiplier.Mili),
                new MeasPoint<Length>(8, UnitMultiplier.Mili));
            ConnectDiametr.MaxDelta = new MeasPoint<Length>(8, UnitMultiplier.Micro);
            BetweenArrowDial = new MeasPoint<Length>(0.7m, UnitMultiplier.Mili);
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


            PhysicalRange<Length> GeneratoRanges(decimal start, decimal end)
            {
                var st = new MeasPoint<Length>(new Length(start, UnitMultiplier.Mili));
                var ed = new MeasPoint<Length>(new Length(end, UnitMultiplier.Mili));
                return new PhysicalRange<Length>(st, ed, new AccuracyChatacteristic((decimal?)range, null, null));
            }

            var ac = new AccuracyChatacteristic((decimal?)fullRange, null, null);
            return new RangeStorage<PhysicalRange<Length>>(ac, arr.ToArray());
        }


    }
   
   
}