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

        /// <summary>
        /// Флаг поддельной точки. Подразумевается, если значение false, значит точка НЕ поддельная.
        /// </summary>
        public bool fakePoinFlag { get; protected set; }

        public MeasureUnits Units { get; set; }

        //множитель единицы
        public Multipliers MultipliersUnit { get; set; }

        //номинал величины
        public decimal Value { get; set; }

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

        #endregion

        public MeasPoint()
        {
            Units = MeasureUnits.V;
            MultipliersUnit = Multipliers.None;
            Value = 0;
            fakePoinFlag = false;
        }
        /// <summary>
        /// Измерительная точка.
        /// </summary>
        /// <param name="units">Единицы измерения величины.</param>
        /// <param name="multipliersUnit">Множитель единицы измерения (килоб милли и т.д.).</param>
        /// <param name="value">Номинальное значение величины.</param>
        /// <param name="fakePoint">Точка реально подается на прибор (если false)</param>
        public MeasPoint(MeasureUnits units, Multipliers multipliersUnit, decimal value, bool fakePoint  = false)
        {
            Units = units;
            MultipliersUnit = multipliersUnit;
            Value = value;
            fakePoinFlag = fakePoint;
        }

        public override string ToString()
        {

            //todo: Необходимо верно конвертировать значение decimal в строку, что бы не появлялась подпись со степенью десятки.
            return $"{Value} {MultipliersUnit.GetStringValue()}{Units.GetStringValue()}";
        }
    }

    /// <summary>
    /// Точка для переменного величины с дополнительным параметром. Например для переменного напряжения/тока.
    /// </summary>
    public class AcVariablePoint
    {
        #region Fields

        /// <summary>
        /// Флаг для поддельной точки.
        /// </summary>
        public bool fakePointFlag { get; protected set; }

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
        public AcVariablePoint(decimal inNominal, MeasureUnits inMeasureUnits, Multipliers inMultipliersUnit) : this(inNominal, inMeasureUnits, inMultipliersUnit,
                                                                                                                     null)
        {
        }

        /// <summary>
        /// Конструктор для точек переменного напряжения и тока (массив с частотами вложен).
        /// </summary>
        /// <param name = "inNominal">номинал предела измерения.</param>
        /// <param name = "inMultipliersUnit">Множитель единицы измерения.</param>
        /// <param name = "inHerzArr">Массив частот для данной точки.</param>
        public AcVariablePoint(decimal inNominal, MeasureUnits inMeasureUnits, Multipliers inMultipliersUnit, MeasPoint[] inHerzArr, bool fakePoint = false)
        {
            VariableBaseValueMeasPoint.Value = inNominal;
            VariableBaseValueMeasPoint.MultipliersUnit = inMultipliersUnit;
            VariableBaseValueMeasPoint.Units = inMeasureUnits;
            fakePointFlag = fakePoint;
            Herz = inHerzArr;
        }
    }
}
