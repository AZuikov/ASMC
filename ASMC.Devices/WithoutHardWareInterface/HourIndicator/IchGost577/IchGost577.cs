using System;
using ASMC.Data.Model;
using ASMC.Data.Model.PhysicalQuantity;

namespace ASMC.Devices.WithoutInterface.HourIndicator.IchGost577
{
    /// <summary>
    ///     Предоставляет реализацию часового индикатора по ГОСТ 577
    /// </summary>
    public abstract class IchGost577 : IchBase
    {
        /// <summary>
        ///     Позволяет задать полный измерительный диапазон.
        /// </summary>
        /// <param name="currentAccuracyClass"></param>
        /// <returns></returns>
        protected virtual RangeStorage<PhysicalRange<Length>> GetRangesFull(AccuracyClass.Standart currentAccuracyClass)
        {
            return null;
        }

        protected virtual MeasPoint<Length> GetArresting(AccuracyClass.Standart currentAccuracyClass)
        {
            throw new NotImplementedException();
        }

        protected virtual MeasPoint<Length> GetVariation(AccuracyClass.Standart currentAccuracyClass)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        ///     Здает измерительный диапазон.
        /// </summary>
        public override RangeStorage<PhysicalRange<Length>> RangesFull => GetRangesFull(CurrentAccuracyClass);


        public override MeasPoint<Length> Variation => GetVariation(CurrentAccuracyClass);
        #region Nested type: MaxMeasuringForce

        /// <summary>
        ///     Максимальное допутсимое усилие
        /// </summary>
        public struct MaxMeasuringForce
        {
            #region Property

            /// <summary>
            ///     Измерение хода
            /// </summary>
            public MeasPoint<Force> ChangeCourse { get; set; }

            /// <summary>
            ///     Колибание при прямом/обратном ходу
            /// </summary>
            public MeasPoint<Force> Oscillatons { get; set; }

            /// <summary>
            ///     Максимальное усилие прямом ходе
            /// </summary>
            public MeasPoint<Force> StraightRun { get; set; }

            #endregion
        }

        #endregion
        #region Nested type: RangeAndDelta

        public class RangeAndDelta
        {
            #region Property

            public PhysicalRange<Length> Range { get; set; }
            public MeasPoint<Length> MaxDelta { get; set; }

            #endregion
        }

        #endregion
        public AccuracyClass.Standart CurrentAccuracyClass { get; set; }

        /// <summary>
        ///     Доступные класы точности.
        /// </summary>
        public AccuracyClass.Standart[] AvailabeAccuracyClass { get; } =
            {AccuracyClass.Standart.Zero, AccuracyClass.Standart.First, AccuracyClass.Standart.Second};


        public MeasPoint<Length> Arresting => GetArresting(CurrentAccuracyClass);
        public MaxMeasuringForce MeasuringForce { get; set; }

        /// <summary>
        ///     Максимальное допустимое отклонение стрелки при перпендикулярном нажиме на его ось.
        /// </summary>
        public decimal PerpendicularPressureMax { get; protected set; }
        public MeasPoint<Length> Diametr { get; set; }

        /// <summary>
        ///     Позволяет получить тест соответствия допуску шероховатости наружной поверхности гильзы.
        /// </summary>
        public string LinerRoughness { get; protected set; }

        /// <summary>
        ///     Позволяет получить тест соответствия допуску шероховатости поверхности измерительного наконечника.
        /// </summary>
        public string TipRoughness { get; protected set; }

        /// <summary>
        ///     Позволяет получить допустимый диапазон ширины стрелки.
        /// </summary>
        public PhysicalRange<Length> ArrowWidch { get; protected set; }

        /// <summary>
        ///     Позволяет получить минимальную допустимую длинну штриха деления.
        /// </summary>
        public MeasPoint<Length> StrokeLength { get; protected set; }

        /// <summary>
        ///     Позволяет получить допуски для присоеденительного диаметра.
        /// </summary>
        public RangeAndDelta ConnectDiametr { get; protected set; } = new RangeAndDelta();

        /// <summary>
        ///     Позволяет получить допуски для присоеденительного диаметра.
        /// </summary>
        public RangeAndDelta StrokeWidch { get; protected set; } = new RangeAndDelta();

        /// <summary>
        ///     Позволяет получить максимальное растояние между концом стрелки и циферблатом.
        /// </summary>
        public MeasPoint<Length> BetweenArrowDial { get; protected set; }
        protected IchGost577()
        {
            MeasuringForce = new MaxMeasuringForce
            {
                StraightRun = new MeasPoint<Force>(1.5M),
                Oscillatons = new MeasPoint<Force>(0.6M),
                ChangeCourse = new MeasPoint<Force>(0.5m)
            };
            PerpendicularPressureMax = 0.5m;
            LinerRoughness = "Ra не более 0,63 мкм";
            TipRoughness = "Ra не более 0,1 мкм";
            ArrowWidch = new PhysicalRange<Length>(new MeasPoint<Length>(0.15m, UnitMultiplier.Mili),
                new MeasPoint<Length>(0.20m, UnitMultiplier.Mili));
            StrokeLength = new MeasPoint<Length>(1, UnitMultiplier.Mili);
            ConnectDiametr.Range = new PhysicalRange<Length>(new MeasPoint<Length>(7.985m, UnitMultiplier.Mili),
                new MeasPoint<Length>(8, UnitMultiplier.Mili));
            ConnectDiametr.MaxDelta = new MeasPoint<Length>(8, UnitMultiplier.Micro);
            BetweenArrowDial = new MeasPoint<Length>(0.7m, UnitMultiplier.Mili);
            StrokeWidch.MaxDelta = new MeasPoint<Length>(50, UnitMultiplier.Micro);
            StrokeWidch.Range = new PhysicalRange<Length>(new MeasPoint<Length>(0.15m, UnitMultiplier.Mili),
                new MeasPoint<Length>(0.25m, UnitMultiplier.Mili));
        }
    }
}