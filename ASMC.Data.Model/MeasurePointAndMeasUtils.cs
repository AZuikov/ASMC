using System;
using System.Linq;
using System.Text;
using AP.Utils.Data;
using AP.Utils.Helps;


namespace ASMC.Data.Model
{
   

    /// <summary>
    /// Предоставляет реализация измерительной точки с номиналом величины и множителем.
    /// </summary>
    public class MeasPoint<TPhysicalQuantity>  where TPhysicalQuantity : IPhysicalQuantity, new()
    {
        #region Property
        public IPhysicalQuantity MainPhysicalQuantity{ get; }
        public IPhysicalQuantity[] AdditionalPhysicalQuantity { get; set; }
        /// <summary>
        /// Флаг поддельной точки. Подразумевается, если значение false, значит точка НЕ поддельная.
        /// </summary>
        public bool IsFake { get; protected set; }

        //public MeasureUnits Units { get; set; }

        ////множитель единицы
        //public UnitMultipliers UnitMultipliersUnit { get; set; }

        ////номинал величины
        //public decimal Value { get; set; }

        /// <summary>
        /// Строковое описание измерительной точки вида: "номинальное значение" "единицы измерения".
        /// </summary>
        public string Description
        {
            get
            {
                return ToString();
            }
        }
        /// <inheritdoc />
        public override string ToString()
        {
            //todo: Необходимо верно конвертировать значение decimal в строку, что бы не появлялась подпись со степенью десятки.
            return string.Join(" ", Array.ConvertAll(AdditionalPhysicalQuantity, s => s.ToString()));
        }

        #endregion

        public MeasPoint(TPhysicalQuantity quantity)
        {
            MainPhysicalQuantity = quantity;
        }
        /// <summary>
        /// Создает экземпляр измерительной точки <see cref="MeasPoint{TPhysicalQuantity}"/>
        /// </summary>
        public MeasPoint()
        {
            MainPhysicalQuantity = new TPhysicalQuantity();
        }
        /// <summary>
        /// Создает экземпляр измерительной точки <see cref="MeasPoint{TPhysicalQuantity}"/>
        /// </summary>
        /// <param name="value">Значение</param>
        /// <param name="multipliers">Множитель, по умолчению <see cref="UnitMultipliers.None"/></param>
        public MeasPoint(decimal value, UnitMultipliers multipliers=  UnitMultipliers.None):this()
        {
            MainPhysicalQuantity.Value = value;
            MainPhysicalQuantity.Multipliers = multipliers;
        }
        /// <summary>
        /// Создает экземпляр измерительной точки <see cref="MeasPoint{TPhysicalQuantity}"/>
        /// </summary>
        /// <param name="value">Значение</param>
        /// <param name="multipliers">Множитель</param>
        /// <param name="physicalQuantities">Перечень дополнительных состовляющих визических величин <see cref="MeasPoint{TPhysicalQuantity}.AdditionalPhysicalQuantity"/> </param>
        public MeasPoint(decimal value, UnitMultipliers multipliers, params IPhysicalQuantity[] physicalQuantities) : this()
        {
            MainPhysicalQuantity.Value = value;
            MainPhysicalQuantity.Multipliers = multipliers;
            AdditionalPhysicalQuantity = physicalQuantities;
        }
        /// <summary>
        /// Создает экземпляр измерительной точки <see cref="MeasPoint{TPhysicalQuantity}"/>
        /// </summary>
        /// <param name="value">Значение</param>
        /// <param name="physicalQuantities">Перечень дополнительных состовляющих визических величин <see cref="MeasPoint{TPhysicalQuantity}.AdditionalPhysicalQuantity"/> </param>
        public MeasPoint(decimal value, params IPhysicalQuantity[] physicalQuantities) : this()
        {
            MainPhysicalQuantity.Value = value;
            AdditionalPhysicalQuantity = physicalQuantities;
        }
        /// <summary>
        /// Создает экземпляр измерительной точки <see cref="MeasPoint{TPhysicalQuantity}"/>
        /// </summary>
        /// <param name="physicalQuantities">Перечень дополнительных состовляющих визических величин <see cref="MeasPoint{TPhysicalQuantity}.AdditionalPhysicalQuantity"/> </param>
        public MeasPoint(params IPhysicalQuantity[] physicalQuantities):this()
        {
            AdditionalPhysicalQuantity = physicalQuantities;
        }

        /// <summary>
        /// Производит сложение измерительных точек в пределах одной физической величины
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns>Возвращает результат сложения в единицах СИ</returns>
        public static MeasPoint<TPhysicalQuantity> operator +(MeasPoint<TPhysicalQuantity> a,
            MeasPoint<TPhysicalQuantity> b)
        {
            if (!Equals(a.MainPhysicalQuantity.Unit, b.MainPhysicalQuantity.Unit)
                || !Enumerable.SequenceEqual(a.AdditionalPhysicalQuantity, b.AdditionalPhysicalQuantity))
                throw new InvalidCastException("Не возможно производить омпрации с разными физическими величинами");

            var val = a.MainPhysicalQuantity.Value * (decimal) a.MainPhysicalQuantity.Unit.GetDoubleValue() +
                      b.MainPhysicalQuantity.Value * (decimal) b.MainPhysicalQuantity.Unit.GetDoubleValue();
            return new MeasPoint<TPhysicalQuantity>(val, a.AdditionalPhysicalQuantity);
        }
        /// <summary>
        /// Производит вычитание измерительных точек в пределах одной физической величины
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns>Возвращает результат вычитания в единицах СИ</returns>
        public static MeasPoint<TPhysicalQuantity> operator -(MeasPoint<TPhysicalQuantity> a,
            MeasPoint<TPhysicalQuantity> b)
        {
            if (!Equals(a.MainPhysicalQuantity.Unit, b.MainPhysicalQuantity.Unit)
                || !Enumerable.SequenceEqual(a.AdditionalPhysicalQuantity, b.AdditionalPhysicalQuantity))
                throw new InvalidCastException("Не возможно производить омпрации с разными физическими величинами");

            var val = a.MainPhysicalQuantity.Value * (decimal)a.MainPhysicalQuantity.Unit.GetDoubleValue() -
                      b.MainPhysicalQuantity.Value * (decimal)b.MainPhysicalQuantity.Unit.GetDoubleValue();
            return new MeasPoint<TPhysicalQuantity>(val, a.AdditionalPhysicalQuantity);
        }

        /// <summary>
        /// Производит умножение измерительных точек в пределах одной физической величины
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns>Возвращает результат умножения в единицах СИ</returns>
        public static MeasPoint<TPhysicalQuantity> operator *(MeasPoint<TPhysicalQuantity> a,
            MeasPoint<TPhysicalQuantity> b)
        {
            if (!Equals(a.MainPhysicalQuantity.Unit, b.MainPhysicalQuantity.Unit)
                || !Enumerable.SequenceEqual(a.AdditionalPhysicalQuantity, b.AdditionalPhysicalQuantity))
                throw new InvalidCastException("Не возможно производить омпрации с разными физическими величинами");

            var val = a.MainPhysicalQuantity.Value * (decimal)a.MainPhysicalQuantity.Unit.GetDoubleValue() *
                      b.MainPhysicalQuantity.Value * (decimal)b.MainPhysicalQuantity.Unit.GetDoubleValue();
            return new MeasPoint<TPhysicalQuantity>(val, a.AdditionalPhysicalQuantity);
        }
        /// <summary>
        /// Производит деление измерительных точек в пределах одной физической величины
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns>Возвращает результат деления в единицах СИ</returns>
        public static MeasPoint<TPhysicalQuantity> operator /(MeasPoint<TPhysicalQuantity> a,
            MeasPoint<TPhysicalQuantity> b)
        {
            if (!Equals(a.MainPhysicalQuantity.Unit, b.MainPhysicalQuantity.Unit)
                || !Enumerable.SequenceEqual(a.AdditionalPhysicalQuantity, b.AdditionalPhysicalQuantity))
                throw new InvalidCastException("Не возможно производить омпрации с разными физическими величинами");

            var val = a.MainPhysicalQuantity.Value * (decimal)a.MainPhysicalQuantity.Unit.GetDoubleValue() /
                      b.MainPhysicalQuantity.Value * (decimal)b.MainPhysicalQuantity.Unit.GetDoubleValue();
            return new MeasPoint<TPhysicalQuantity>(val, a.AdditionalPhysicalQuantity);
        }
        /// <summary>
        /// Производит вычитание из измерительной точки относительной величины
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns>Возвращает результат в виде измерительной точкиу без изменения едениц измерения</returns>
        public static MeasPoint<TPhysicalQuantity> operator -(MeasPoint<TPhysicalQuantity> a, decimal b)
        {
            return new MeasPoint<TPhysicalQuantity>(a.MainPhysicalQuantity.Value - b,
                                                    a.MainPhysicalQuantity.Multipliers,
                                                    a.AdditionalPhysicalQuantity);
        }
        /// <summary>
        /// Производит сложение измерительной точки и относительной величины
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns>Возвращает результат в виде измерительной точки без изменения едениц измерения</returns>
        public static MeasPoint<TPhysicalQuantity> operator +(MeasPoint<TPhysicalQuantity> a, decimal b)
        {
            return new MeasPoint<TPhysicalQuantity>(a.MainPhysicalQuantity.Value + b,
                                                    a.MainPhysicalQuantity.Multipliers,
                                                    a.AdditionalPhysicalQuantity);
        }
        /// <summary>
        /// Производит умножение из измерительной точки относительной величины
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns>Возвращает результат в виде измерительной точки без изменения едениц измерения</returns>
        public static MeasPoint<TPhysicalQuantity> operator *(MeasPoint<TPhysicalQuantity> a, decimal b)
        {
            return new MeasPoint<TPhysicalQuantity>(a.MainPhysicalQuantity.Value * b,
                                                    a.MainPhysicalQuantity.Multipliers,
                                                    a.AdditionalPhysicalQuantity);
        }
        /// <summary>
        /// Производит деление  измерительной точки на относительную величину
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns>Возвращает результат в виде измерительной точки без изменения едениц измерения</returns>
        public static MeasPoint<TPhysicalQuantity> operator /(MeasPoint<TPhysicalQuantity> a, decimal b)
        {
            return new MeasPoint<TPhysicalQuantity>(a.MainPhysicalQuantity.Value / b,
                                                    a.MainPhysicalQuantity.Multipliers,
                                                    a.AdditionalPhysicalQuantity);
        }
    }

    
    /// <summary>
    /// Точка для переменного величины с дополнительным параметром. Например для переменного напряжения/тока.
    /// </summary>
    //public class AcVariablePoint
    //{
    //    #region Fields

    //    /// <summary>
    //    /// Флаг для поддельной точки.
    //    /// </summary>
    //    public bool fakePointFlag { get; protected set; }

    //    /// <summary>
    //    /// Основное значение точки (тока/напряжения).
    //    /// </summary>
    //    public MeasPoint VariableBaseValueMeasPoint = new MeasPoint();

    //    /// <summary>
    //    /// Массив частот для данной точки.
    //    /// </summary>
    //    public MeasPoint[] Herz;

    //    #endregion

    //    /// <summary>
    //    /// Конструктор можно использовать для точек с постоянным напряжением (массива частоты нет).
    //    /// </summary>
    //    /// <param name = "inNominal">Предел измерения прибора.</param>
    //    /// <param name = "inUnitMultipliersUnit">Множитель единицы измерения.</param>
    //    public AcVariablePoint(decimal inNominal, MeasureUnits inMeasureUnits, UnitMultipliers inUnitMultipliersUnit) : this(inNominal, inMeasureUnits, inUnitMultipliersUnit,
    //                                                                                                                 null)
    //    {
    //    }

    //    /// <summary>
    //    /// Конструктор для точек переменного напряжения и тока (массив с частотами вложен).
    //    /// </summary>
    //    /// <param name = "inNominal">номинал предела измерения.</param>
    //    /// <param name = "inUnitMultipliersUnit">Множитель единицы измерения.</param>
    //    /// <param name = "inHerzArr">Массив частот для данной точки.</param>
    //    public AcVariablePoint(decimal inNominal, MeasureUnits inMeasureUnits, UnitMultipliers inUnitMultipliersUnit, MeasPoint[] inHerzArr, bool fakePoint = false)
    //    {
    //        VariableBaseValueMeasPoint.Value = inNominal;
    //        VariableBaseValueMeasPoint.UnitMultipliersUnit = inUnitMultipliersUnit;
    //        VariableBaseValueMeasPoint.Units = inMeasureUnits;
    //        fakePointFlag = fakePoint;
    //        Herz = inHerzArr;
    //    }
    //}
}
