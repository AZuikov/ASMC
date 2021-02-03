using System;
using ASMC.Data.Model.PhysicalQuantity;

namespace ASMC.Data.Model
{
    public interface IMeasPoint<TPhysicalQuantity> : ICloneable, IComparable<IMeasPoint<TPhysicalQuantity>>
        where TPhysicalQuantity : IPhysicalQuantity 
    {
        #region Property

        /// <summary>
        /// Строковое описание измерительной точки вида: "номинальное значение" "единицы измерения".
        /// </summary>
        string Description { get; }

        /// <summary>
        /// Позволяет задать или получить основную физическую величину.
        /// </summary>
        TPhysicalQuantity MainPhysicalQuantity { get; set; }
        /// <summary>
        /// Позволяет получить численное значение основной физической величины <see cref="IPhysicalQuantity.Value"/>
        /// </summary>
        /// <returns></returns>
        decimal GetMainValue();

        #endregion
    }

    public interface IMeasPoint<TPhysicalQuantity, TAddPhysicalQuantity> : IMeasPoint<TPhysicalQuantity>,
                                                                           IComparable<IMeasPoint<TPhysicalQuantity,
                                                                               TAddPhysicalQuantity>>
        where TAddPhysicalQuantity : class, IPhysicalQuantity<TAddPhysicalQuantity>, new()
        where TPhysicalQuantity : class, IPhysicalQuantity<TPhysicalQuantity>, new()
    {
        #region Property

        TAddPhysicalQuantity AdditionalPhysicalQuantity { get; set; }

        #endregion
    }

    public class MeasPoint<TPhysicalQuantity> : IMeasPoint<TPhysicalQuantity>
        where TPhysicalQuantity : class, IPhysicalQuantity<TPhysicalQuantity>, new()
    {
        public TPhysicalQuantity MainPhysicalQuantity { get; set; }

        /// <inheritdoc />
        public decimal GetMainValue()
        {
            return MainPhysicalQuantity.Value;
        }

        public virtual string Description => MainPhysicalQuantity.ToString();

        /// <inheritdoc />
        public virtual object Clone()
        {
            return new MeasPoint<TPhysicalQuantity>((TPhysicalQuantity) MainPhysicalQuantity.Clone());
        }

        /// <inheritdoc />
        public int CompareTo(IMeasPoint<TPhysicalQuantity> other)
        {
            return MainPhysicalQuantity.CompareTo(other.MainPhysicalQuantity);
        }

        #region Operators

        #region Arifmetic

        public static MeasPoint<TPhysicalQuantity> operator +(MeasPoint<TPhysicalQuantity> a,
            MeasPoint<TPhysicalQuantity> b)
        {
            if (!Equals(a.MainPhysicalQuantity.Unit, b.MainPhysicalQuantity.Unit))
                throw new ArgumentException("Не возможно выполнить операцию с разными физическими величинами");

            return new MeasPoint<TPhysicalQuantity>(a.MainPhysicalQuantity.GetNoramalizeValueToSi()
                                                    + b.MainPhysicalQuantity.GetNoramalizeValueToSi());
        }

        public static MeasPoint<TPhysicalQuantity> operator -(MeasPoint<TPhysicalQuantity> a,
            MeasPoint<TPhysicalQuantity> b)
        {
            if (!Equals(a.MainPhysicalQuantity.Unit, b.MainPhysicalQuantity.Unit))
                throw new ArgumentException("Не возможно выполнить операцию с разными физическими величинами");

            return new MeasPoint<TPhysicalQuantity>(a.MainPhysicalQuantity.GetNoramalizeValueToSi()
                                                    - b.MainPhysicalQuantity.GetNoramalizeValueToSi());
        }

        public static MeasPoint<TPhysicalQuantity> operator *(MeasPoint<TPhysicalQuantity> a,
            MeasPoint<TPhysicalQuantity> b)
        {
            if (!Equals(a.MainPhysicalQuantity.Unit, b.MainPhysicalQuantity.Unit))
                throw new ArgumentException("Не возможно выполнить операцию с разными физическими величинами");

            return new MeasPoint<TPhysicalQuantity>(a.MainPhysicalQuantity.GetNoramalizeValueToSi()
                                                    * b.MainPhysicalQuantity.GetNoramalizeValueToSi());
        }

        public static decimal operator /(MeasPoint<TPhysicalQuantity> a,
            MeasPoint<TPhysicalQuantity> b)
        {
            if (!Equals(a.MainPhysicalQuantity.Unit, b.MainPhysicalQuantity.Unit))
                throw new ArgumentException("Не возможно выполнить операцию с разными физическими величинами");

            return a.MainPhysicalQuantity.GetNoramalizeValueToSi() / b.MainPhysicalQuantity.GetNoramalizeValueToSi();
        }

        public static MeasPoint<TPhysicalQuantity> operator +(MeasPoint<TPhysicalQuantity> a,
            decimal b)
        {
            var mp = (MeasPoint<TPhysicalQuantity>) a.Clone();
            mp.MainPhysicalQuantity.Value = mp.MainPhysicalQuantity.Value + b;
            return mp;
        }

        public static MeasPoint<TPhysicalQuantity> operator -(MeasPoint<TPhysicalQuantity> a,
            decimal b)
        {
            var mp = (MeasPoint<TPhysicalQuantity>) a.Clone();
            mp.MainPhysicalQuantity.Value = mp.MainPhysicalQuantity.Value - b;
            return mp;
        }

        public static MeasPoint<TPhysicalQuantity> operator *(MeasPoint<TPhysicalQuantity> a,
            decimal b)
        {
            var mp = (MeasPoint<TPhysicalQuantity>) a.Clone();
            mp.MainPhysicalQuantity.Value = mp.MainPhysicalQuantity.Value * b;
            return mp;
        }

        public static MeasPoint<TPhysicalQuantity> operator /(MeasPoint<TPhysicalQuantity> a,
            decimal b)
        {
            var mp = (MeasPoint<TPhysicalQuantity>) a.Clone();
            mp.MainPhysicalQuantity.Value = mp.MainPhysicalQuantity.Value / b;
            return mp;
        }

        #endregion Arifmetic

        public static bool operator <(MeasPoint<TPhysicalQuantity> a,
            MeasPoint<TPhysicalQuantity> b)
        {
            if (!Equals(a.MainPhysicalQuantity.Unit, b.MainPhysicalQuantity.Unit))
                throw new ArgumentException("Не возможно сравнить точки с разными физическими величинами");

            return a.MainPhysicalQuantity.GetNoramalizeValueToSi() < b.MainPhysicalQuantity.GetNoramalizeValueToSi();
        }

        public static bool operator >(MeasPoint<TPhysicalQuantity> a,
            MeasPoint<TPhysicalQuantity> b)
        {
            if (!Equals(a.MainPhysicalQuantity.Unit, b.MainPhysicalQuantity.Unit))
                throw new ArgumentException("Не возможно сравнить точки с разными физическими величинами");

            return a.MainPhysicalQuantity.GetNoramalizeValueToSi() > b.MainPhysicalQuantity.GetNoramalizeValueToSi();
        }

        public static bool operator <=(MeasPoint<TPhysicalQuantity> a,
            MeasPoint<TPhysicalQuantity> b)
        {
            if (!Equals(a?.MainPhysicalQuantity?.Unit, b?.MainPhysicalQuantity?.Unit)) return false;

            return a?.MainPhysicalQuantity?.GetNoramalizeValueToSi() <=
                   b?.MainPhysicalQuantity?.GetNoramalizeValueToSi();
        }

        public static bool operator >=(MeasPoint<TPhysicalQuantity> a,
            MeasPoint<TPhysicalQuantity> b)
        {
            if (!Equals(a?.MainPhysicalQuantity.Unit, b?.MainPhysicalQuantity.Unit)) return false;

            return a?.MainPhysicalQuantity?.GetNoramalizeValueToSi() >=
                   b?.MainPhysicalQuantity?.GetNoramalizeValueToSi();
        }

        public static bool operator ==(MeasPoint<TPhysicalQuantity> a,
            MeasPoint<TPhysicalQuantity> b)
        {
            if (!Equals(a?.MainPhysicalQuantity.Unit, b?.MainPhysicalQuantity.Unit)) return false;

            return a?.MainPhysicalQuantity?.GetNoramalizeValueToSi() ==
                   b?.MainPhysicalQuantity?.GetNoramalizeValueToSi();
        }

        public static bool operator !=(MeasPoint<TPhysicalQuantity> a, MeasPoint<TPhysicalQuantity> b)
        {
            if (!Equals(a?.MainPhysicalQuantity.Unit, b?.MainPhysicalQuantity.Unit))
                return true;
            //throw new ArgumentException("Не возможно сравнить точки с разными физическими величинами");
            return a?.MainPhysicalQuantity?.GetNoramalizeValueToSi() !=
                   b?.MainPhysicalQuantity?.GetNoramalizeValueToSi();
        }

        #endregion Operators

        #region Constructor

        /// <summary>
        /// Создает экземпляр измерительной точки <see cref = "MeasPoint{TPhysicalQuantity}" />
        /// </summary>
        public MeasPoint(TPhysicalQuantity quantity) 
        {
            MainPhysicalQuantity = quantity;
        }

      

        /// <summary>
        /// Создает экземпляр измерительной точки <see cref = "MeasPoint{TPhysicalQuantity}" />
        /// </summary>
        /// <param name = "value">Значение</param>
        /// <param name = "multiplier">Множитель, по умолчению <see cref = "UnitMultiplier.None" /></param>
        public MeasPoint(decimal value, UnitMultiplier multiplier = UnitMultiplier.None) : this()
        {
            MainPhysicalQuantity.Value = value;
            MainPhysicalQuantity.Multiplier = multiplier;
        }

        public MeasPoint()
        {
            MainPhysicalQuantity = new TPhysicalQuantity();
        }

        #endregion Constructor
    }

    public class MeasPoint<TPhysicalQuantity, TAddPhysicalQuantity> : MeasPoint<TPhysicalQuantity>,
                                                                      IMeasPoint<TPhysicalQuantity, TAddPhysicalQuantity
                                                                      > where TAddPhysicalQuantity : class, IPhysicalQuantity<TAddPhysicalQuantity>, new()
                                                                        where TPhysicalQuantity : class,
                                                                        IPhysicalQuantity<TPhysicalQuantity>, new()
    {
        public MeasPoint(TPhysicalQuantity physical, TAddPhysicalQuantity addPhysicalQuantity)
        {
            MainPhysicalQuantity = physical;
            AdditionalPhysicalQuantity = addPhysicalQuantity;
        }

        public MeasPoint()
        {
            AdditionalPhysicalQuantity = new TAddPhysicalQuantity();
        }

        public MeasPoint(decimal value, TAddPhysicalQuantity addPhysicalQuantity) : this()
        {
            MainPhysicalQuantity.Value = value;
            AdditionalPhysicalQuantity = addPhysicalQuantity;
        }

        public MeasPoint(decimal physical, UnitMultiplier multiplier, TAddPhysicalQuantity addPhysicalQuantity) : this()
        {
            MainPhysicalQuantity.Value = physical;
            MainPhysicalQuantity.Multiplier = multiplier;
            AdditionalPhysicalQuantity = addPhysicalQuantity;
        }

        public MeasPoint(decimal physical, UnitMultiplier multiplier, decimal addPhysicalQuantity,
            UnitMultiplier addmultipliers = UnitMultiplier.None) : this()
        {
            MainPhysicalQuantity.Value = physical;
            MainPhysicalQuantity.Multiplier = multiplier;
            AdditionalPhysicalQuantity.Value = addPhysicalQuantity;
            AdditionalPhysicalQuantity.Multiplier = addmultipliers;
        }

        public MeasPoint(decimal @decimal, decimal decimal1) : this()
        {
            MainPhysicalQuantity.Value = @decimal;
            AdditionalPhysicalQuantity.Value = decimal1;
        }

        /// <inheritdoc />
        public TAddPhysicalQuantity AdditionalPhysicalQuantity { get; set; }

        /// <inheritdoc />
        public override string Description => MainPhysicalQuantity + " " + AdditionalPhysicalQuantity;

        /// <inheritdoc />
        public override object Clone()
        {
            return new
                MeasPoint<TPhysicalQuantity, TAddPhysicalQuantity>((TPhysicalQuantity) MainPhysicalQuantity.Clone(),
                                                                   (TAddPhysicalQuantity) AdditionalPhysicalQuantity
                                                                      .Clone());
        }

        /// <inheritdoc />
        public int CompareTo(IMeasPoint<TPhysicalQuantity, TAddPhysicalQuantity> other)
        {
            return MainPhysicalQuantity.CompareTo(other.MainPhysicalQuantity);
        }

        #region Operators

        #region Arifmetic

        public static MeasPoint<TPhysicalQuantity, TAddPhysicalQuantity> operator +(
            MeasPoint<TPhysicalQuantity, TAddPhysicalQuantity> a,
            MeasPoint<TPhysicalQuantity, TAddPhysicalQuantity> b)
        {
            if (!Equals(a.MainPhysicalQuantity.Unit, b.MainPhysicalQuantity.Unit)
                ||
                !Equals(a.AdditionalPhysicalQuantity.Unit, b.AdditionalPhysicalQuantity.Unit)
                ||
                !Equals(a.AdditionalPhysicalQuantity.GetNoramalizeValueToSi(),
                        b.AdditionalPhysicalQuantity.GetNoramalizeValueToSi()))
                throw new ArgumentException("Не возможно выполнить операцию с разными физическими величинами");

            return new
                MeasPoint<TPhysicalQuantity, TAddPhysicalQuantity>(a.MainPhysicalQuantity.GetNoramalizeValueToSi()
                                                                   + b.MainPhysicalQuantity.GetNoramalizeValueToSi(),
                                                                   (TAddPhysicalQuantity) a
                                                                                         .AdditionalPhysicalQuantity
                                                                                         .Clone());
        }

        public static MeasPoint<TPhysicalQuantity, TAddPhysicalQuantity> operator -(
            MeasPoint<TPhysicalQuantity, TAddPhysicalQuantity> a,
            MeasPoint<TPhysicalQuantity, TAddPhysicalQuantity> b)
        {
            if (!Equals(a.MainPhysicalQuantity.Unit, b.MainPhysicalQuantity.Unit)
                ||
                !Equals(a.AdditionalPhysicalQuantity.Unit, b.AdditionalPhysicalQuantity.Unit)
                ||
                !Equals(a.AdditionalPhysicalQuantity.GetNoramalizeValueToSi(),
                        b.AdditionalPhysicalQuantity.GetNoramalizeValueToSi()))
                throw new ArgumentException("Не возможно выполнить операцию с разными физическими величинами");

            return new
                MeasPoint<TPhysicalQuantity, TAddPhysicalQuantity>(a.MainPhysicalQuantity.GetNoramalizeValueToSi()
                                                                   - b.MainPhysicalQuantity.GetNoramalizeValueToSi(),
                                                                   (TAddPhysicalQuantity) a
                                                                                         .AdditionalPhysicalQuantity
                                                                                         .Clone());
        }

        public static MeasPoint<TPhysicalQuantity, TAddPhysicalQuantity> operator *(
            MeasPoint<TPhysicalQuantity, TAddPhysicalQuantity> a,
            MeasPoint<TPhysicalQuantity, TAddPhysicalQuantity> b)
        {
            if (!Equals(a.MainPhysicalQuantity.Unit, b.MainPhysicalQuantity.Unit)
                ||
                !Equals(a.AdditionalPhysicalQuantity.Unit, b.AdditionalPhysicalQuantity.Unit)
                ||
                !Equals(a.AdditionalPhysicalQuantity.GetNoramalizeValueToSi(),
                        b.AdditionalPhysicalQuantity.GetNoramalizeValueToSi()))
                throw new ArgumentException("Не возможно выполнить операцию с разными физическими величинами");

            return new
                MeasPoint<TPhysicalQuantity, TAddPhysicalQuantity>(a.MainPhysicalQuantity.GetNoramalizeValueToSi()
                                                                   * b.MainPhysicalQuantity.GetNoramalizeValueToSi(),
                                                                   (TAddPhysicalQuantity) a
                                                                                         .AdditionalPhysicalQuantity
                                                                                         .Clone());
        }

        public static decimal operator /(MeasPoint<TPhysicalQuantity, TAddPhysicalQuantity> a,
            MeasPoint<TPhysicalQuantity, TAddPhysicalQuantity> b)
        {
            if (!Equals(a.MainPhysicalQuantity.Unit, b.MainPhysicalQuantity.Unit)
                ||
                !Equals(a.AdditionalPhysicalQuantity.Unit, b.AdditionalPhysicalQuantity.Unit)
                ||
                !Equals(a.AdditionalPhysicalQuantity.GetNoramalizeValueToSi(),
                        b.AdditionalPhysicalQuantity.GetNoramalizeValueToSi()))
                throw new ArgumentException("Не возможно выполнить операцию с разными физическими величинами");

            return a.MainPhysicalQuantity.GetNoramalizeValueToSi() / b.MainPhysicalQuantity.GetNoramalizeValueToSi();
        }

        public static MeasPoint<TPhysicalQuantity, TAddPhysicalQuantity> operator +(
            MeasPoint<TPhysicalQuantity, TAddPhysicalQuantity> a,
            decimal b)
        {
            var mp = (MeasPoint<TPhysicalQuantity, TAddPhysicalQuantity>) a.Clone();
            mp.MainPhysicalQuantity.Value = mp.MainPhysicalQuantity.Value + b;
            return mp;
        }

        public static MeasPoint<TPhysicalQuantity, TAddPhysicalQuantity> operator -(
            MeasPoint<TPhysicalQuantity, TAddPhysicalQuantity> a,
            decimal b)
        {
            var mp = (MeasPoint<TPhysicalQuantity, TAddPhysicalQuantity>) a.Clone();
            mp.MainPhysicalQuantity.Value = mp.MainPhysicalQuantity.Value - b;
            return mp;
        }

        public static MeasPoint<TPhysicalQuantity, TAddPhysicalQuantity> operator *(
            MeasPoint<TPhysicalQuantity, TAddPhysicalQuantity> a,
            decimal b)
        {
            var mp = (MeasPoint<TPhysicalQuantity, TAddPhysicalQuantity>) a.Clone();
            mp.MainPhysicalQuantity.Value = mp.MainPhysicalQuantity.Value * b;
            return mp;
        }

        public static MeasPoint<TPhysicalQuantity, TAddPhysicalQuantity> operator /(
            MeasPoint<TPhysicalQuantity, TAddPhysicalQuantity> a,
            decimal b)
        {
            var mp = (MeasPoint<TPhysicalQuantity, TAddPhysicalQuantity>) a.Clone();
            mp.MainPhysicalQuantity.Value = mp.MainPhysicalQuantity.Value / b;
            return mp;
        }

        #endregion Arifmetic

        public static bool operator <(MeasPoint<TPhysicalQuantity, TAddPhysicalQuantity> a,
            MeasPoint<TPhysicalQuantity, TAddPhysicalQuantity> b)
        {
            if (!Equals(a.MainPhysicalQuantity.Unit, b.MainPhysicalQuantity.Unit) ||
                !Equals(a.AdditionalPhysicalQuantity.Unit, b.AdditionalPhysicalQuantity.Unit))
                throw new ArgumentException("Не возможно выполнить сравнение точек с разными физическими величинами");

            return a.MainPhysicalQuantity.GetNoramalizeValueToSi() < b.MainPhysicalQuantity.GetNoramalizeValueToSi();
        }

        public static bool operator >(MeasPoint<TPhysicalQuantity, TAddPhysicalQuantity> a,
            MeasPoint<TPhysicalQuantity, TAddPhysicalQuantity> b)
        {
            if (!Equals(a.MainPhysicalQuantity.Unit, b.MainPhysicalQuantity.Unit) ||
                !Equals(a.AdditionalPhysicalQuantity.Unit, b.AdditionalPhysicalQuantity.Unit))
                throw new ArgumentException("Не возможно выполнить сравнение точек с разными физическими величинами");

            return a.MainPhysicalQuantity.GetNoramalizeValueToSi() > b.MainPhysicalQuantity.GetNoramalizeValueToSi();
        }

        public static bool operator <=(MeasPoint<TPhysicalQuantity, TAddPhysicalQuantity> a,
            MeasPoint<TPhysicalQuantity, TAddPhysicalQuantity> b)
        {
            if (!Equals(a?.MainPhysicalQuantity.Unit, b?.MainPhysicalQuantity.Unit) ||
                !Equals(a?.AdditionalPhysicalQuantity.Unit, b?.AdditionalPhysicalQuantity.Unit))
                return false;

            return a?.MainPhysicalQuantity?.GetNoramalizeValueToSi() <=
                   b?.MainPhysicalQuantity?.GetNoramalizeValueToSi();
        }

        public static bool operator >=(MeasPoint<TPhysicalQuantity, TAddPhysicalQuantity> a,
            MeasPoint<TPhysicalQuantity, TAddPhysicalQuantity> b)
        {
            if (!Equals(a?.MainPhysicalQuantity.Unit, b?.MainPhysicalQuantity.Unit) ||
                !Equals(a?.AdditionalPhysicalQuantity.Unit, b?.AdditionalPhysicalQuantity.Unit))
                return false;

            return a?.MainPhysicalQuantity?.GetNoramalizeValueToSi() >=
                   b?.MainPhysicalQuantity?.GetNoramalizeValueToSi();
        }

        public static bool operator ==(MeasPoint<TPhysicalQuantity, TAddPhysicalQuantity> a,
            MeasPoint<TPhysicalQuantity, TAddPhysicalQuantity> b)
        {
            if (!Equals(a?.MainPhysicalQuantity.Unit, b?.MainPhysicalQuantity.Unit) ||
                !Equals(a?.AdditionalPhysicalQuantity.Unit, b?.AdditionalPhysicalQuantity.Unit))
                return false;

            return a?.MainPhysicalQuantity?.GetNoramalizeValueToSi() ==
                   b?.MainPhysicalQuantity?.GetNoramalizeValueToSi();
        }

        public static bool operator !=(MeasPoint<TPhysicalQuantity, TAddPhysicalQuantity> a,
            MeasPoint<TPhysicalQuantity, TAddPhysicalQuantity> b)
        {
            if (!Equals(a?.MainPhysicalQuantity.Unit, b?.MainPhysicalQuantity.Unit) ||
                !Equals(a?.AdditionalPhysicalQuantity.Unit, b?.AdditionalPhysicalQuantity.Unit))
                return true;
            return a?.MainPhysicalQuantity?.GetNoramalizeValueToSi() !=
                   b?.MainPhysicalQuantity?.GetNoramalizeValueToSi();
        }

        #endregion Operators
    }

    /// <summary>
    /// Предоставляет реализацию допустимых диапазнов (пределов) воспроизведения/измерения физических величин.
    /// </summary>
    public class PhysicalRange<T> : IPhysicalRange<T> where T : class, IPhysicalQuantity<T>, new()
    {
        public PhysicalRange(MeasPoint<T> stopRange, AccuracyChatacteristic accuracy = null)
        {
            AccuracyChatacteristic = accuracy;
            Start = new MeasPoint<T>();
            End = stopRange;
        }

        /// <summary>
        /// Расчитывает для текущего предела погрешность, с учетом заданных точностных характеристик
        /// </summary>
        /// <param name="inPoint">Точка, внутри предела, для которой считаем погрешность.</param>
        /// <returns></returns>
        public MeasPoint<T> CalculateTollerance(MeasPoint<T> inPoint)
        {
            if (inPoint < (MeasPoint<T>) Start || inPoint > (MeasPoint<T>) End)
                throw new
                    ArgumentOutOfRangeException($"Невозможно рассчитать погрешность предела ({this}). Значение точки {inPoint.Description} лежит вне диапазонов предела.");
            decimal tol = AccuracyChatacteristic.GetAccuracy(inPoint.MainPhysicalQuantity.GetNoramalizeValueToSi(), End.MainPhysicalQuantity.GetNoramalizeValueToSi());
            MeasPoint<T> tolPoint = new MeasPoint<T>(tol);
            tolPoint.MainPhysicalQuantity = (T)tolPoint.MainPhysicalQuantity.ChangeMultiplier(inPoint.MainPhysicalQuantity.Multiplier);
            return tolPoint;

        }

        public PhysicalRange(MeasPoint<T> startRange, MeasPoint<T> stopRange, AccuracyChatacteristic accuracy = null)
        {
            if (!Equals(startRange.MainPhysicalQuantity.Unit, stopRange.MainPhysicalQuantity.Unit))
                throw new ArgumentException("Не возможно сравнить точки с разными физическими величинами");
            AccuracyChatacteristic = accuracy;
            Start = startRange;
            End = stopRange;
        }

        public PhysicalRange()
        {
            Start = new MeasPoint<T>();
            End = new MeasPoint<T>();
        }

        public override string ToString()
        {
            return $"от {Start.Description} до {End.Description}";
        }
        /// <summary>
        /// Считает протяженность диапазона от начала до конца. Не учитывает знаки начала и конца.
        /// </summary>
        public IMeasPoint<T> GetRangeLeght
        {
            get
            {
                decimal start = Start.MainPhysicalQuantity.GetNoramalizeValueToSi();
                decimal end = End.MainPhysicalQuantity.GetNoramalizeValueToSi();
                decimal leght = Math.Abs(start) + Math.Abs(end);
                MeasPoint < T > resultMeasPoint = new MeasPoint<T>(leght);
                resultMeasPoint.MainPhysicalQuantity.ChangeMultiplier(Start.MainPhysicalQuantity.Multiplier);
                return resultMeasPoint;
            }
        }

        /// <inheritdoc />
        public AccuracyChatacteristic AccuracyChatacteristic { get; set; }

        /// <inheritdoc />
        public IMeasPoint<T> Start { get; set; }

        /// <inheritdoc />
        public bool IsPointBelong<T1>(IMeasPoint<T1> point) where T1 : class, IPhysicalQuantity<T1>, new()
        {
            return Start.MainPhysicalQuantity.GetNoramalizeValueToSi() <= point.MainPhysicalQuantity.GetNoramalizeValueToSi()&& End.MainPhysicalQuantity.GetNoramalizeValueToSi() >=
                   point.MainPhysicalQuantity.GetNoramalizeValueToSi();
        }

        /// <inheritdoc />
        public IMeasPoint<T> End { get; set; }

        public MeasureUnits Unit => Start.MainPhysicalQuantity.Unit;

        /// <inheritdoc />
        public int CompareTo(IPhysicalRange<T> other)
        {
            return Start.CompareTo(other.Start);
        }
    }

    /// <summary>
    /// Предоставляет реализацию допустимых диапазнов (пределов) воспроизведения/измерения физических величин.
    /// </summary>
    public class PhysicalRange<TPhysicalQuantity, TAddition> : IPhysicalRange<TPhysicalQuantity, TAddition>
        where TAddition : class, IPhysicalQuantity<TAddition>, new()
        where TPhysicalQuantity : class, IPhysicalQuantity<TPhysicalQuantity>, new()
    {
        public PhysicalRange(IMeasPoint<TPhysicalQuantity, TAddition> stopRange, AccuracyChatacteristic accuracy = null)
        {
            AccuracyChatacteristic = accuracy;
            Start = new MeasPoint<TPhysicalQuantity, TAddition>();
            End = stopRange;
            Unit = stopRange.MainPhysicalQuantity.Unit;
        }

        public PhysicalRange(IMeasPoint<TPhysicalQuantity, TAddition> startRange,
            IMeasPoint<TPhysicalQuantity, TAddition> stopRange, AccuracyChatacteristic accuracy = null)
        {
            if (!Equals(startRange.MainPhysicalQuantity.Unit, stopRange.MainPhysicalQuantity.Unit))
                throw new ArgumentException("Не возможно сравнить точки с разными физическими величинами");
            AccuracyChatacteristic = accuracy;
            Start = startRange;
            End = stopRange;

            Unit = startRange.MainPhysicalQuantity.Unit;
        }



        public PhysicalRange()
        {
            Start = new MeasPoint<TPhysicalQuantity, TAddition>();
            End = new MeasPoint<TPhysicalQuantity, TAddition>();
        }

        /// <inheritdoc />
        public AccuracyChatacteristic AccuracyChatacteristic { get; set; }

        public MeasPoint<TPhysicalQuantity,TAddition> CalculateTollerance(MeasPoint<TPhysicalQuantity, TAddition> inPoint)
        {
            if (inPoint < (MeasPoint<TPhysicalQuantity, TAddition>)Start || inPoint > (MeasPoint<TPhysicalQuantity>)End)
                throw new
                    ArgumentOutOfRangeException($"Невозможно рассчитать погрешность предела ({this}). Значение точки {inPoint.Description} лежит вне диапазонов предела.");
            decimal tol = AccuracyChatacteristic.GetAccuracy(inPoint.MainPhysicalQuantity.GetNoramalizeValueToSi(), End.MainPhysicalQuantity.GetNoramalizeValueToSi());
            MeasPoint < TPhysicalQuantity, TAddition > tolPoint = new MeasPoint<TPhysicalQuantity, TAddition>(tol, UnitMultiplier.None,inPoint.AdditionalPhysicalQuantity);
            tolPoint.MainPhysicalQuantity= (TPhysicalQuantity)tolPoint.MainPhysicalQuantity.ChangeMultiplier(inPoint.MainPhysicalQuantity.Multiplier);
            return tolPoint;

        }

        /// <inheritdoc />
        public IMeasPoint<TPhysicalQuantity, TAddition> Start { get; set; }

        /// <inheritdoc />
        public bool IsPointBelong<T, T1>(IMeasPoint<T, T1> point) where T : class, IPhysicalQuantity<T>, new() where T1 : class, IPhysicalQuantity<T1>, new()
        {
                if (point == null) return false;

               return Start.MainPhysicalQuantity.GetNoramalizeValueToSi() <=
                                                       point.MainPhysicalQuantity.GetNoramalizeValueToSi()
                                                       && Start.AdditionalPhysicalQuantity.GetNoramalizeValueToSi() <=
                                                       point.AdditionalPhysicalQuantity.GetNoramalizeValueToSi()
                                                      &&
                                                      (End.MainPhysicalQuantity.GetNoramalizeValueToSi() >=
                                                       point.MainPhysicalQuantity.GetNoramalizeValueToSi()
                                                       && End.AdditionalPhysicalQuantity.GetNoramalizeValueToSi() >=
                                                       point.AdditionalPhysicalQuantity.GetNoramalizeValueToSi());
        }

        /// <inheritdoc />
        public IMeasPoint<TPhysicalQuantity, TAddition> End { get; set; }

        public MeasureUnits Unit { get; protected set; }

        /// <inheritdoc />
        public int CompareTo(IPhysicalRange<TPhysicalQuantity> other)
        {
            return Start.CompareTo(other.Start);
        }
    }

    public interface IPhysicalRange
    {
        #region Property

        /// <summary>
        /// позвляет получить харатеристику точности диапазона.
        /// </summary>
        AccuracyChatacteristic AccuracyChatacteristic { get; set; }

        MeasureUnits Unit { get; }

        #endregion
    }

    public interface IPhysicalRange<T, T1> : IComparable<IPhysicalRange<T>>, IPhysicalRange
        where T : class, IPhysicalQuantity<T>, new() where T1 : class, IPhysicalQuantity<T1>, new()
    {

        bool IsPointBelong<T, T1>(IMeasPoint<T, T1> point) where T : class, IPhysicalQuantity<T>, new()
            where T1 : class, IPhysicalQuantity<T1>, new();
        
        #region Property

        /// <summary>
        /// Значение величины описывающая верхнюю (граничную) точку диапазона (входит в диапазон).
        /// </summary>
        IMeasPoint<T, T1> End { get; }

        /// <summary>
        /// Значение величины, описывающее начало диапазона (входит в диапазон).
        /// </summary>
        IMeasPoint<T, T1> Start { get; }

        #endregion
    }

    public interface IPhysicalRange<T> : IComparable<IPhysicalRange<T>>, IPhysicalRange
        where T : class, IPhysicalQuantity<T>, new()
    {
        #region Property

        /// <summary>
        /// Значение величины описывающая верхнюю (граничную) точку диапазона (входит в диапазон).
        /// </summary>
        IMeasPoint<T> End { get; set; }

        /// <summary>
        /// Значение величины, описывающее начало диапазона (входит в диапазон).
        /// </summary>
        IMeasPoint<T> Start { get; set; }

        #endregion

        /// <summary>
        /// </summary>
        /// <typeparam name = "T1"></typeparam>
        /// <param name = "point"></param>
        /// <returns></returns>
        bool IsPointBelong<T>(IMeasPoint<T> point) where T : class, IPhysicalQuantity<T>, new();


    }
}