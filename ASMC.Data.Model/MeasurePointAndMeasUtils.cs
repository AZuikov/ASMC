using AP.Utils.Data;
using AP.Utils.Helps;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ASMC.Data.Model
{
    /// <summary>
    /// Предоставляет реализация измерительной точки с номиналом величины и множителем.
    /// </summary>
    public class MeasPoint<TPhysicalQuantity> : ICloneable, IComparable<MeasPoint<TPhysicalQuantity>>
        where TPhysicalQuantity : IPhysicalQuantity, IEquatable<TPhysicalQuantity>, new()
    {
        #region Property

        public IPhysicalQuantity[] AdditionalPhysicalQuantity { get; set; }

        //public MeasureUnits Units { get; set; }

        ////множитель единицы
        //public UnitMultipliers UnitMultipliersUnit { get; set; }

        ////номинал величины
        //public decimal Value { get; set; }

        /// <summary>
        /// Строковое описание измерительной точки вида: "номинальное значение" "единицы измерения".
        /// </summary>
        public string Description => ToString();

        /// <summary>
        /// Флаг поддельной точки. Подразумевается, если значение false, значит точка НЕ поддельная.
        /// </summary>
        public bool IsFake { get; protected set; }

        public IPhysicalQuantity MainPhysicalQuantity { get; }

        #endregion Property

        #region Methods

        public override bool Equals(object obj) => Equals(obj as MeasPoint<TPhysicalQuantity>);

        public  bool Equals(MeasPoint<TPhysicalQuantity> pointToCompare)
        {
            var mainResult = false; // равенство основной единицы
            var additionalResult = false; // равенство вложений
            //var pointToCompare = (MeasPoint<TPhysicalQuantity>)obj;

            if (MainPhysicalQuantity.Unit == pointToCompare.MainPhysicalQuantity.Unit &&
                (decimal)MainPhysicalQuantity.Multipliers.GetDoubleValue() * MainPhysicalQuantity.Value ==
                (decimal)pointToCompare.MainPhysicalQuantity.Multipliers.GetDoubleValue() *
                pointToCompare.MainPhysicalQuantity.Value)
                mainResult = true;
            if (this.AdditionalPhysicalQuantity != null && pointToCompare.AdditionalPhysicalQuantity != null)
            {
                Array.Sort(AdditionalPhysicalQuantity);
                Array.Sort(pointToCompare.AdditionalPhysicalQuantity);
                additionalResult = AdditionalPhysicalQuantity.SequenceEqual(pointToCompare.AdditionalPhysicalQuantity);
                return mainResult && additionalResult;
            }
            else if ((this.AdditionalPhysicalQuantity == null && pointToCompare.AdditionalPhysicalQuantity != null) ||
                     (this.AdditionalPhysicalQuantity != null && pointToCompare.AdditionalPhysicalQuantity == null))
            {
                additionalResult = false;
                return mainResult && additionalResult;
            }

            return mainResult;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            var str = string.Join(" ", MainPhysicalQuantity.Value, MainPhysicalQuantity.Multipliers.GetStringValue()) +
                     MainPhysicalQuantity.Unit.GetStringValue();
            //todo: Необходимо верно конвертировать значение decimal в строку, что бы не появлялась подпись со степенью десятки.
            return AdditionalPhysicalQuantity == null
                ? str
                : string.Join(" ", str, Array.ConvertAll(AdditionalPhysicalQuantity, s => s.ToString()));
        }

        #endregion Methods

        /// <inheritdoc />
        public object Clone()
        {
            var clone = new MeasPoint<TPhysicalQuantity>();
            clone.MainPhysicalQuantity.Unit = MainPhysicalQuantity.Unit;
            clone.MainPhysicalQuantity.Multipliers = MainPhysicalQuantity.Multipliers;
            clone.MainPhysicalQuantity.Value = MainPhysicalQuantity.Value;
            clone.AdditionalPhysicalQuantity = AdditionalPhysicalQuantity?.ToArray();
            return clone;
        }

        /// <inheritdoc />
        public int CompareTo(MeasPoint<TPhysicalQuantity> other)
        {
            if (!Equals(MainPhysicalQuantity.Unit, other.MainPhysicalQuantity.Unit))
                throw new ArgumentException("Не возможно сравнить точки с разными физическими величинами");
            if (AdditionalPhysicalQuantity != null)
            {
                if (other.AdditionalPhysicalQuantity != null ||
                    AdditionalPhysicalQuantity.SequenceEqual(other.AdditionalPhysicalQuantity))
                    throw new ArgumentException("Не возможно сравнить точки с разными физическими величинами");
            }
            else
            {
                if (other.AdditionalPhysicalQuantity != null)
                    throw new ArgumentException("Не возможно сравнить точки с разными физическими величинами");
            }

            return (MainPhysicalQuantity.Value *
                    (decimal)MainPhysicalQuantity.Multipliers.GetDoubleValue())
               .CompareTo(other.MainPhysicalQuantity.Value *
                          (decimal)other.MainPhysicalQuantity.Multipliers.GetDoubleValue());
        }

        #region Constructor

        public MeasPoint(IPhysicalQuantity quantity)
        {
            MainPhysicalQuantity = quantity;
        }

        /// <summary>
        /// Создает экземпляр измерительной точки <see cref = "MeasPoint{TPhysicalQuantity}" />
        /// </summary>
        public MeasPoint()
        {
            MainPhysicalQuantity = new TPhysicalQuantity();
        }

        /// <summary>
        /// Создает экземпляр измерительной точки <see cref = "MeasPoint{TPhysicalQuantity}" />
        /// </summary>
        /// <param name = "value">Значение</param>
        /// <param name = "multipliers">Множитель, по умолчению <see cref = "UnitMultipliers.None" /></param>
        public MeasPoint(decimal value, UnitMultipliers multipliers = UnitMultipliers.None) : this()
        {
            MainPhysicalQuantity.Value = value;
            MainPhysicalQuantity.Multipliers = multipliers;
        }

        /// <summary>
        /// Создает экземпляр измерительной точки <see cref = "MeasPoint{TPhysicalQuantity}" />
        /// </summary>
        /// <param name = "value">Значение</param>
        /// <param name = "multipliers">Множитель</param>
        /// <param name = "physicalQuantities">
        /// Перечень дополнительных состовляющих визических величин
        /// <see cref = "MeasPoint{TPhysicalQuantity}.AdditionalPhysicalQuantity" />
        /// </param>
        public MeasPoint(decimal value, UnitMultipliers multipliers,
            params IPhysicalQuantity[] physicalQuantities) : this()
        {
            MainPhysicalQuantity.Value = value;
            MainPhysicalQuantity.Multipliers = multipliers;
            AdditionalPhysicalQuantity = physicalQuantities;
        }

        /// <summary>
        /// Создает экземпляр измерительной точки <see cref = "MeasPoint{TPhysicalQuantity}" />
        /// </summary>
        /// <param name = "value">Значение</param>
        /// <param name = "physicalQuantities">
        /// Перечень дополнительных состовляющих визических величин
        /// <see cref = "MeasPoint{TPhysicalQuantity}.AdditionalPhysicalQuantity" />
        /// </param>
        public MeasPoint(decimal value, params IPhysicalQuantity[] physicalQuantities) : this()
        {
            MainPhysicalQuantity.Value = value;
            AdditionalPhysicalQuantity = physicalQuantities;
        }

        /// <summary>
        /// Создает экземпляр измерительной точки <see cref = "MeasPoint{TPhysicalQuantity}" />
        /// </summary>
        /// <param name = "physicalQuantity"></param>
        /// <param name = "physicalQuantities">
        /// Перечень дополнительных состовляющих визических величин
        /// <see cref = "MeasPoint{TPhysicalQuantity}.AdditionalPhysicalQuantity" />
        /// </param>
        public MeasPoint(IPhysicalQuantity physicalQuantity, params IPhysicalQuantity[] physicalQuantities) : this()
        {
            MainPhysicalQuantity = physicalQuantity;
            AdditionalPhysicalQuantity = physicalQuantities;
        }

        #endregion Constructor

        #region Operators

        /// <summary>
        /// Производит сложение измерительных точек в пределах одной физической величины
        /// </summary>
        /// <param name = "a"></param>
        /// <param name = "b"></param>
        /// <returns>Возвращает результат сложения в единицах СИ</returns>
        public static MeasPoint<TPhysicalQuantity> operator +(MeasPoint<TPhysicalQuantity> a,
            MeasPoint<TPhysicalQuantity> b)
        {
            if (!Equals(a.MainPhysicalQuantity.Unit, b.MainPhysicalQuantity.Unit))
                throw new InvalidCastException("Не возможно производить операции с разными физическими величинами");
            if (a.AdditionalPhysicalQuantity != null)
            {
                if (b.AdditionalPhysicalQuantity != null ||
                    a.AdditionalPhysicalQuantity.SequenceEqual(b.AdditionalPhysicalQuantity))
                    throw new InvalidCastException("Не возможно производить операции с разными физическими величинами");
            }
            else
            {
                if (b.AdditionalPhysicalQuantity != null)
                    throw new InvalidCastException("Не возможно производить операции с разными физическими величинами");
            }

            var val = a.MainPhysicalQuantity.Value * (decimal)a.MainPhysicalQuantity.Multipliers.GetDoubleValue() +
                      b.MainPhysicalQuantity.Value * (decimal)b.MainPhysicalQuantity.Multipliers.GetDoubleValue();
            return new MeasPoint<TPhysicalQuantity>(val, a.AdditionalPhysicalQuantity);
        }

        /// <summary>
        /// Производит вычитание измерительных точек в пределах одной физической величины
        /// </summary>
        /// <param name = "a"></param>
        /// <param name = "b"></param>
        /// <returns>Возвращает результат вычитания в единицах СИ</returns>
        public static MeasPoint<TPhysicalQuantity> operator -(MeasPoint<TPhysicalQuantity> a,
            MeasPoint<TPhysicalQuantity> b)
        {
            if (!Equals(a.MainPhysicalQuantity.Unit, b.MainPhysicalQuantity.Unit))
                throw new InvalidCastException("Не возможно производить операции с разными физическими величинами");
            if (a.AdditionalPhysicalQuantity != null)
            {
                if (b.AdditionalPhysicalQuantity != null ||
                    a.AdditionalPhysicalQuantity.SequenceEqual(b.AdditionalPhysicalQuantity))
                    throw new InvalidCastException("Не возможно производить операции с разными физическими величинами");
            }
            else
            {
                if (b.AdditionalPhysicalQuantity != null)
                    throw new InvalidCastException("Не возможно производить операции с разными физическими величинами");
            }

            var val = a.MainPhysicalQuantity.Value * (decimal)a.MainPhysicalQuantity.Multipliers.GetDoubleValue() -
                      b.MainPhysicalQuantity.Value * (decimal)b.MainPhysicalQuantity.Multipliers.GetDoubleValue();
            return new MeasPoint<TPhysicalQuantity>(val, a.AdditionalPhysicalQuantity);
        }

        /// <summary>
        /// Производит умножение измерительных точек в пределах одной физической величины
        /// </summary>
        /// <param name = "a"></param>
        /// <param name = "b"></param>
        /// <returns>Возвращает результат умножения в единицах СИ</returns>
        public static MeasPoint<TPhysicalQuantity> operator *(MeasPoint<TPhysicalQuantity> a,
            MeasPoint<TPhysicalQuantity> b)
        {
            if (!Equals(a.MainPhysicalQuantity.Unit, b.MainPhysicalQuantity.Unit))
                throw new InvalidCastException("Не возможно производить операции с разными физическими величинами");
            if (a.AdditionalPhysicalQuantity != null)
            {
                if (b.AdditionalPhysicalQuantity != null ||
                    a.AdditionalPhysicalQuantity.SequenceEqual(b.AdditionalPhysicalQuantity))
                    throw new InvalidCastException("Не возможно производить операции с разными физическими величинами");
            }
            else
            {
                if (b.AdditionalPhysicalQuantity != null)
                    throw new InvalidCastException("Не возможно производить операции с разными физическими величинами");
            }

            var val = a.MainPhysicalQuantity.Value * (decimal)a.MainPhysicalQuantity.Multipliers.GetDoubleValue() *
                      b.MainPhysicalQuantity.Value * (decimal)b.MainPhysicalQuantity.Multipliers.GetDoubleValue();
            return new MeasPoint<TPhysicalQuantity>(val, a.AdditionalPhysicalQuantity);
        }

        /// <summary>
        /// Производит деление измерительных точек в пределах одной физической величины
        /// </summary>
        /// <param name = "a"></param>
        /// <param name = "b"></param>
        /// <returns>Возвращает результат деления в единицах СИ</returns>
        public static MeasPoint<TPhysicalQuantity> operator /(MeasPoint<TPhysicalQuantity> a,
            MeasPoint<TPhysicalQuantity> b)
        {
            if (!Equals(a.MainPhysicalQuantity.Unit, b.MainPhysicalQuantity.Unit))
                throw new InvalidCastException("Не возможно производить операции с разными физическими величинами");
            if (a.AdditionalPhysicalQuantity != null)
            {
                if (b.AdditionalPhysicalQuantity != null ||
                    a.AdditionalPhysicalQuantity.SequenceEqual(b.AdditionalPhysicalQuantity))
                    throw new InvalidCastException("Не возможно производить операции с разными физическими величинами");
            }
            else
            {
                if (b.AdditionalPhysicalQuantity != null)
                    throw new InvalidCastException("Не возможно производить операции с разными физическими величинами");
            }

            var val = a.MainPhysicalQuantity.Value * (decimal)a.MainPhysicalQuantity.Multipliers.GetDoubleValue() /
                b.MainPhysicalQuantity.Value * (decimal)b.MainPhysicalQuantity.Multipliers.GetDoubleValue();
            return new MeasPoint<TPhysicalQuantity>(val, a.AdditionalPhysicalQuantity);
        }

        /// <summary>
        /// Производит вычитание из измерительной точки относительной величины
        /// </summary>
        /// <param name = "a"></param>
        /// <param name = "b"></param>
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
        /// <param name = "a"></param>
        /// <param name = "b"></param>
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
        /// <param name = "a"></param>
        /// <param name = "b"></param>
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
        /// <param name = "a"></param>
        /// <param name = "b"></param>
        /// <returns>Возвращает результат в виде измерительной точки без изменения едениц измерения</returns>
        public static MeasPoint<TPhysicalQuantity> operator /(MeasPoint<TPhysicalQuantity> a, decimal b)
        {
            return new MeasPoint<TPhysicalQuantity>(a.MainPhysicalQuantity.Value / b,
                                                    a.MainPhysicalQuantity.Multipliers,
                                                    a.AdditionalPhysicalQuantity);
        }

        public static bool operator >(MeasPoint<TPhysicalQuantity> a, MeasPoint<TPhysicalQuantity> b)
        {
            if (!Equals(a.MainPhysicalQuantity.Unit, b.MainPhysicalQuantity.Unit))
                throw new InvalidCastException("Не возможно производить операции с разными физическими величинами");
            if (a.AdditionalPhysicalQuantity != null)
            {
                if (b.AdditionalPhysicalQuantity != null ||
                    a.AdditionalPhysicalQuantity.SequenceEqual(b.AdditionalPhysicalQuantity))
                    throw new InvalidCastException("Не возможно производить операции с разными физическими величинами");
            }
            else
            {
                if (b.AdditionalPhysicalQuantity != null)
                    throw new InvalidCastException("Не возможно производить операции с разными физическими величинами");
            }

            var A = a.MainPhysicalQuantity.Value * (decimal)a.MainPhysicalQuantity.Multipliers.GetDoubleValue();
            var B = b.MainPhysicalQuantity.Value * (decimal)b.MainPhysicalQuantity.Multipliers.GetDoubleValue();
            return A > B;
        }

        public static bool operator <(MeasPoint<TPhysicalQuantity> a, MeasPoint<TPhysicalQuantity> b)
        {
            if (!Equals(a.MainPhysicalQuantity.Unit, b.MainPhysicalQuantity.Unit))
                throw new InvalidCastException("Не возможно производить операции с разными физическими величинами");
            if (a.AdditionalPhysicalQuantity != null)
            {
                if (b.AdditionalPhysicalQuantity != null ||
                    a.AdditionalPhysicalQuantity.SequenceEqual(b.AdditionalPhysicalQuantity))
                    throw new InvalidCastException("Не возможно производить операции с разными физическими величинами");
            }
            else
            {
                if (b.AdditionalPhysicalQuantity != null)
                    throw new InvalidCastException("Не возможно производить операции с разными физическими величинами");
            }

            var A = a.MainPhysicalQuantity.Value * (decimal)a.MainPhysicalQuantity.Multipliers.GetDoubleValue();
            var B = b.MainPhysicalQuantity.Value * (decimal)b.MainPhysicalQuantity.Multipliers.GetDoubleValue();
            return A < B;
        }

        public static bool operator >=(MeasPoint<TPhysicalQuantity> a, MeasPoint<TPhysicalQuantity> b)
        {
            if (!Equals(a.MainPhysicalQuantity.Unit, b.MainPhysicalQuantity.Unit))
                throw new InvalidCastException("Не возможно производить операции с разными физическими величинами");
            if (a.AdditionalPhysicalQuantity != null)
            {
                if (b.AdditionalPhysicalQuantity != null ||
                    a.AdditionalPhysicalQuantity.SequenceEqual(b.AdditionalPhysicalQuantity))
                    throw new InvalidCastException("Не возможно производить операции с разными физическими величинами");
            }
            else
            {
                if (b.AdditionalPhysicalQuantity != null)
                    throw new InvalidCastException("Не возможно производить операции с разными физическими величинами");
            }

            var A = a.MainPhysicalQuantity.Value * (decimal)a.MainPhysicalQuantity.Multipliers.GetDoubleValue();
            var B = b.MainPhysicalQuantity.Value * (decimal)b.MainPhysicalQuantity.Multipliers.GetDoubleValue();
            return A >= B;
        }

        public static bool operator <=(MeasPoint<TPhysicalQuantity> a, MeasPoint<TPhysicalQuantity> b)
        {
            if (!Equals(a.MainPhysicalQuantity.Unit, b.MainPhysicalQuantity.Unit))
                throw new InvalidCastException("Не возможно производить операции с разными физическими величинами");
            if (a.AdditionalPhysicalQuantity != null)
            {
                if (b.AdditionalPhysicalQuantity != null ||
                    a.AdditionalPhysicalQuantity.SequenceEqual(b.AdditionalPhysicalQuantity))
                    throw new InvalidCastException("Не возможно производить операции с разными физическими величинами");
            }
            else
            {
                if (b.AdditionalPhysicalQuantity != null)
                    throw new InvalidCastException("Не возможно производить операции с разными физическими величинами");
            }

            var A = a.MainPhysicalQuantity.Value * (decimal)a.MainPhysicalQuantity.Multipliers.GetDoubleValue();
            var B = b.MainPhysicalQuantity.Value * (decimal)b.MainPhysicalQuantity.Multipliers.GetDoubleValue();
            return A <= B;
        }

        public static bool operator !=(MeasPoint<TPhysicalQuantity> a, MeasPoint<TPhysicalQuantity> b)
        {
            if (a == null || b == null) return false;
            if (!Equals(a.MainPhysicalQuantity.Unit, b.MainPhysicalQuantity.Unit)
                || !a.AdditionalPhysicalQuantity.SequenceEqual(b.AdditionalPhysicalQuantity))
                return true;

            var A = a.MainPhysicalQuantity.Value * (decimal)a.MainPhysicalQuantity.Multipliers.GetDoubleValue();
            var B = b.MainPhysicalQuantity.Value * (decimal)b.MainPhysicalQuantity.Multipliers.GetDoubleValue();
            return A != B;
        }

        public static bool operator ==(MeasPoint<TPhysicalQuantity> a, MeasPoint<TPhysicalQuantity> b)
        {
            if (!(a != null || b != null)) return false;
            if (!Equals(a.MainPhysicalQuantity.Unit, b.MainPhysicalQuantity.Unit)
                || !a.AdditionalPhysicalQuantity.SequenceEqual(b.AdditionalPhysicalQuantity))
                return false;
            // проверка вложений
            if (a.AdditionalPhysicalQuantity != null && b.AdditionalPhysicalQuantity != null)
            {
                if (a.AdditionalPhysicalQuantity.Length == b.AdditionalPhysicalQuantity.Length)
                {
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }

            var A = a.MainPhysicalQuantity.Value * (decimal)a.MainPhysicalQuantity.Multipliers.GetDoubleValue();
            var B = b.MainPhysicalQuantity.Value * (decimal)b.MainPhysicalQuantity.Multipliers.GetDoubleValue();
            return A == B;
        }

        #endregion Operators
    }

    /// <summary>
    /// Предоставляет реализацию допустимых диапазнов (пределов) воспроизведения/измерения физических величин.
    /// </summary>
    public class PhysicalRange<T> : IPhysicalRange<object> where T : IPhysicalQuantity, IEquatable<T>, new()
    {
        public PhysicalRange(MeasPoint<T> startRange, MeasPoint<T> stopRange)
        {
            if (!Equals(startRange.MainPhysicalQuantity.Unit, stopRange.MainPhysicalQuantity.Unit))
                throw new ArgumentException("Не возможно сравнить точки с разными физическими величинами");
            if (startRange.AdditionalPhysicalQuantity != null)
            {
                if (stopRange.AdditionalPhysicalQuantity != null ||
                    startRange.AdditionalPhysicalQuantity.SequenceEqual(stopRange.AdditionalPhysicalQuantity))
                    throw new
                        ArgumentException("Не возможно сравнить точки с разными физическими величинами (вложение)");
            }
            else
            {
                if (stopRange.AdditionalPhysicalQuantity != null)
                    throw new ArgumentException("Первая точка в конструкторе диапазона не инициализирована (null).");
            }

            Start = startRange;
            Stop = stopRange;
            Unit = startRange.MainPhysicalQuantity.Unit;
        }

        /// <inheritdoc />
        public object Start { get; protected set; }

        /// <inheritdoc />
        public object Stop { get; protected set; }

        public MeasureUnits Unit { get; protected set; }
    }

    public interface IPhysicalRange<T>
    {
        #region Property

        /// <summary>
        /// Значение величины, описывающее начало диапазона (входит в диапазон).
        /// </summary>
        T Start { get; }

        /// <summary>
        /// Значение величины описывающая верхнюю (граничную) точку диапазона (входит в диапазон).
        /// </summary>
        T Stop { get; }

        MeasureUnits Unit { get; }

        #endregion Property
    }

    /// <summary>
    /// Предоставляет реализацию хранилища диапазонов (по виду измерения). Фактически перечень пределов СИ.
    /// </summary>
    //public class RangeStorage<T> where T : IPhysicalQuantity, new()
    public class RangeStorage
    {
        #region Property

        /// <summary>
        /// Наименование.
        /// </summary>
        public string Name { get; set; }

        public IPhysicalRange<object>[] Ranges { get; set; }

        #endregion Property

        public RangeStorage(params IPhysicalRange<object>[] inPhysicalRange)
        {
            Ranges = inPhysicalRange;
        }

        #region Methods

        /// <summary>
        /// Запрос перечня физических величин в диапазоне.
        /// </summary>
        /// <returns>Возвращает список физических величин, содержащихся в диапазоне.</returns>
        public List<MeasureUnits> GetPhysicalQuantity()
        {
            var result = new List<MeasureUnits>();

            foreach (var range in Ranges)
                result.Add(range.Unit);
            // удаляем дубликаты, если такое возможно!
            return new HashSet<MeasureUnits>(result).ToList();
        }

        public bool PointIsInRange<T>(MeasPoint<T> inPoint) where T : IPhysicalQuantity, IEquatable<T>, new()
        {
            //Если нужно сравнивать вложение точки с пределом, товсе усложняется
            // нас спасет рекурсия!
            foreach (var range in Ranges)
                if (range.Unit == inPoint.MainPhysicalQuantity.Unit)
                {
                    var start = range.Start as MeasPoint<T>;
                    var stop = range.Stop as MeasPoint<T>;
                    return start <= inPoint && stop >= inPoint;

                    if (inPoint.AdditionalPhysicalQuantity != null)
                        foreach (var additional in inPoint.AdditionalPhysicalQuantity)
                        {
                            //inPoint.AdditionalPhysicalQuantity.Where(q => Equals(q.GetType(), typeof(Frequency)))
                            //new MeasPoint<additional.Unit>(additional.Value, additional.Multipliers);
                        }
                }

            return false;
        }

        #endregion Methods
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