using AP.Utils.Data;


namespace ASMC.Data.Model
{
   

    /// <summary>
    /// Предоставляет реализация измерительной точки с номиналом величины и множителем.
    /// </summary>
    public class MeasPoint
    {
        #region Property

        public MeasureUnits Units { get; set; }

        //множитель единицы
        public Multipliers MultipliersUnit { get; set; }

        //номинал величины
        public decimal NominalVal { get; set; }

        #endregion

        public override string ToString()
        {

            //todo: Необходимо верно конвертировать значение decimal в строку, что бы не появлялась подпись со степенью десятки.
            return $"{NominalVal} {MultipliersUnit.GetStringValue()}{Units.GetStringValue()}";
        }
    }

    /// <summary>
    /// Точка для переменного величины с дополнительным параметром. Например для переменного напряжения/тока.
    /// </summary>
    public class AcVariablePoint
    {
        #region Fields

        /// <summary>
        /// Основное значение точки (тока/напряжения).
        /// </summary>
        public MeasPoint VariableBaseValueMeasPoint = new MeasPoint();

        /// <summary>
        /// Массив частот для данной точки.
        /// </summary>
        public MeasPoint[] Herz;

        #endregion

        /// <summary>
        /// Конструктор можно использовать для точек с постоянным напряжением (массива частоты нет).
        /// </summary>
        /// <param name = "inNominal">Предел измерения прибора.</param>
        /// <param name = "inMultipliersUnit">Множитель единицы измерения.</param>
        public AcVariablePoint(decimal inNominal, Multipliers inMultipliersUnit) : this(inNominal, inMultipliersUnit,
                                                                                        null)
        {
        }

        /// <summary>
        /// Конструктор для точек переменного напряжения и тока (массив с частотами вложен).
        /// </summary>
        /// <param name = "inNominal">номинал предела измерения.</param>
        /// <param name = "inMultipliersUnit">Множитель единицы измерения.</param>
        /// <param name = "inHerzArr">Массив частот для данной точки.</param>
        public AcVariablePoint(decimal inNominal, Multipliers inMultipliersUnit, MeasPoint[] inHerzArr)
        {
            VariableBaseValueMeasPoint.NominalVal = inNominal;
            VariableBaseValueMeasPoint.MultipliersUnit = inMultipliersUnit;

            Herz = inHerzArr;
        }
    }

    /// <summary>
    /// Обозначения единиц измерения.
    /// </summary>
    public enum MeasureUnits
    {
        [StringValue("В")] V,
        [StringValue("А")] I,
        [StringValue("Ом")] Ohm,
        [StringValue("Ф")] Far,
        [StringValue("Гц")] Herz,
        [StringValue("°C")] degC,
        [StringValue("°F")] DegF,

    }

    /// <summary>
    /// Содержит доступные множители и их обозначения.
    /// </summary>
    public enum Multipliers
    {
        /// <summary>
        /// Множетель нано 1Е-9.
        /// </summary>
        [StringValue("н")] [DoubleValue(1E-9)] Nano,

        /// <summary>
        /// Множетель микро 1Е-6.
        /// </summary>
        [StringValue("мк")] [DoubleValue(1E-6)] Micro,

        /// <summary>
        /// Множетель мили 1Е-3.
        /// </summary>
        [StringValue("м")] [DoubleValue(1E-3)] Mili,

        /// <summary>
        /// Без множителя.
        /// </summary>
        [StringValue("")] [DoubleValue(1)] None,

        /// <summary>
        /// Множитель кило 1Е3
        /// </summary>
        [StringValue("к")] [DoubleValue(1E3)] Kilo,

        /// <summary>
        /// Мноэитель мега 1Е6
        /// </summary>
        [StringValue("М")] [DoubleValue(1E6)] Mega,

        /// <summary>
        /// Мноэитель мега 1Е6
        /// </summary>
        [StringValue("Г")] [DoubleValue(1E9)] Giga
    }
}
