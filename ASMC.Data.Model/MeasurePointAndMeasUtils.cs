using AP.Utils.Data;
using AP.Utils.Helps;


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

        public MeasPoint()
        {
            Units = MeasureUnits.V;
            MultipliersUnit = Multipliers.None;
            NominalVal = 0;
        }

        public MeasPoint(MeasureUnits units, Multipliers multipliersUnit, decimal nominalVal)
        {
            Units = units;
            MultipliersUnit = multipliersUnit;
            NominalVal = nominalVal;
        }

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
}
