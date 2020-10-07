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
        public bool IsFake { get; protected set; }

        public MeasureUnits Units { get; set; }

        //множитель единицы
        public UnitMultipliers UnitMultipliersUnit { get; set; }

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
            UnitMultipliersUnit = UnitMultipliers.None;
            Value = 0;
            IsFake = false;
        }
        /// <summary>
        /// Измерительная точка.
        /// </summary>
        /// <param name="units">Единицы измерения величины.</param>
        /// <param name="unitMultipliersUnit">Множитель единицы измерения (килоб милли и т.д.).</param>
        /// <param name="value">Номинальное значение величины.</param>
        /// <param name="isFakePoint">Точка реально подается на прибор (если false)</param>
        public MeasPoint(MeasureUnits units, UnitMultipliers unitMultipliersUnit, decimal value, bool isFakePoint  = false)
        {
            Units = units;
            UnitMultipliersUnit = unitMultipliersUnit;
            Value = value;
            IsFake = isFakePoint;
        }

        public override string ToString()
        {

            //todo: Необходимо верно конвертировать значение decimal в строку, что бы не появлялась подпись со степенью десятки.
            return $"{Value} {UnitMultipliersUnit.GetStringValue()}{Units.GetStringValue()}";
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
        /// <param name = "inUnitMultipliersUnit">Множитель единицы измерения.</param>
        public AcVariablePoint(decimal inNominal, MeasureUnits inMeasureUnits, UnitMultipliers inUnitMultipliersUnit) : this(inNominal, inMeasureUnits, inUnitMultipliersUnit,
                                                                                                                     null)
        {
        }

        /// <summary>
        /// Конструктор для точек переменного напряжения и тока (массив с частотами вложен).
        /// </summary>
        /// <param name = "inNominal">номинал предела измерения.</param>
        /// <param name = "inUnitMultipliersUnit">Множитель единицы измерения.</param>
        /// <param name = "inHerzArr">Массив частот для данной точки.</param>
        public AcVariablePoint(decimal inNominal, MeasureUnits inMeasureUnits, UnitMultipliers inUnitMultipliersUnit, MeasPoint[] inHerzArr, bool fakePoint = false)
        {
            VariableBaseValueMeasPoint.Value = inNominal;
            VariableBaseValueMeasPoint.UnitMultipliersUnit = inUnitMultipliersUnit;
            VariableBaseValueMeasPoint.Units = inMeasureUnits;
            fakePointFlag = fakePoint;
            Herz = inHerzArr;
        }
    }
}
