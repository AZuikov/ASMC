using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ASMC.Data.Model.PhysicalQuantity;

namespace ASMC.Data.Model
{
    public interface IMeasPoint<TPhysicalQuantity> : ICloneable, IComparable<IMeasPoint<TPhysicalQuantity>>
        where TPhysicalQuantity : IPhysicalQuantity /*class, IPhysicalQuantity<TPhysicalQuantity>, IEquatable<TPhysicalQuantity>, new()*/
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
            if (!Equals(a.MainPhysicalQuantity.Unit, b.MainPhysicalQuantity.Unit)) throw new ArgumentException("Не возможно выполнить операцию с разными физическими величинами");

            return new MeasPoint<TPhysicalQuantity>(a.MainPhysicalQuantity.GetNoramalizeValueToSi()
                                                    + b.MainPhysicalQuantity.GetNoramalizeValueToSi());
        }
        public static MeasPoint<TPhysicalQuantity> operator -(MeasPoint<TPhysicalQuantity> a,
            MeasPoint<TPhysicalQuantity> b)
        {
            if (!Equals(a.MainPhysicalQuantity.Unit, b.MainPhysicalQuantity.Unit)) throw new ArgumentException("Не возможно выполнить операцию с разными физическими величинами");

            return new MeasPoint<TPhysicalQuantity>(a.MainPhysicalQuantity.GetNoramalizeValueToSi()
                                                    - b.MainPhysicalQuantity.GetNoramalizeValueToSi());
        }

        public static MeasPoint<TPhysicalQuantity> operator *(MeasPoint<TPhysicalQuantity> a,
            MeasPoint<TPhysicalQuantity> b)
        {
            if (!Equals(a.MainPhysicalQuantity.Unit, b.MainPhysicalQuantity.Unit)) throw new ArgumentException("Не возможно выполнить операцию с разными физическими величинами");

            return new MeasPoint<TPhysicalQuantity>(a.MainPhysicalQuantity.GetNoramalizeValueToSi()
                                                    * b.MainPhysicalQuantity.GetNoramalizeValueToSi());
        }
        public static decimal operator /(MeasPoint<TPhysicalQuantity> a,
            MeasPoint<TPhysicalQuantity> b)
        {
            if (!Equals(a.MainPhysicalQuantity.Unit, b.MainPhysicalQuantity.Unit)) throw new ArgumentException("Не возможно выполнить операцию с разными физическими величинами");

            return a.MainPhysicalQuantity.GetNoramalizeValueToSi() / b.MainPhysicalQuantity.GetNoramalizeValueToSi();
        }

        public static MeasPoint<TPhysicalQuantity> operator +(MeasPoint<TPhysicalQuantity> a,
            decimal b)
        {
            var mp = (MeasPoint<TPhysicalQuantity>)a.Clone();
            mp.MainPhysicalQuantity.Value = mp.MainPhysicalQuantity.Value + b;
            return mp;
        }
        public static MeasPoint<TPhysicalQuantity> operator -(MeasPoint<TPhysicalQuantity> a,
            decimal b)
        {
            var mp = (MeasPoint<TPhysicalQuantity>)a.Clone();
            mp.MainPhysicalQuantity.Value = mp.MainPhysicalQuantity.Value - b;
            return mp;
        }

        public static MeasPoint<TPhysicalQuantity> operator *(MeasPoint<TPhysicalQuantity> a,
            decimal b)
        {
            var mp = (MeasPoint<TPhysicalQuantity>)a.Clone();
            mp.MainPhysicalQuantity.Value = mp.MainPhysicalQuantity.Value * b;
            return mp;
        }
        public static MeasPoint<TPhysicalQuantity> operator /(MeasPoint<TPhysicalQuantity> a,
            decimal b)
        {
            var mp = (MeasPoint<TPhysicalQuantity>)a.Clone();
            mp.MainPhysicalQuantity.Value = mp.MainPhysicalQuantity.Value / b;
            return mp;
        }

        #endregion

        public static bool operator <(MeasPoint<TPhysicalQuantity> a,
            MeasPoint<TPhysicalQuantity> b)
        {
            if (!Equals(a.MainPhysicalQuantity.Unit, b.MainPhysicalQuantity.Unit)) throw new ArgumentException("Не возможно сравнить точки с разными физическими величинами");
            
            return a.MainPhysicalQuantity.GetNoramalizeValueToSi() < b.MainPhysicalQuantity.GetNoramalizeValueToSi();
        }
        public static bool operator >(MeasPoint<TPhysicalQuantity> a,
            MeasPoint<TPhysicalQuantity> b)
        {
            if (!Equals(a.MainPhysicalQuantity.Unit, b.MainPhysicalQuantity.Unit)) throw new ArgumentException("Не возможно сравнить точки с разными физическими величинами");

            return a.MainPhysicalQuantity.GetNoramalizeValueToSi() > b.MainPhysicalQuantity.GetNoramalizeValueToSi();
        }

        public static bool operator <=(MeasPoint<TPhysicalQuantity> a,
            MeasPoint<TPhysicalQuantity> b)
        {

            if (!Equals(a.MainPhysicalQuantity.Unit, b.MainPhysicalQuantity.Unit)) throw new ArgumentException("Не возможно сравнить точки с разными физическими величинами");

            return a.MainPhysicalQuantity.GetNoramalizeValueToSi() <= b.MainPhysicalQuantity.GetNoramalizeValueToSi();
        }
        public static bool operator >=(MeasPoint<TPhysicalQuantity> a,
            MeasPoint<TPhysicalQuantity> b)
        {
            if (!Equals(a.MainPhysicalQuantity.Unit, b.MainPhysicalQuantity.Unit)) throw new ArgumentException("Не возможно сравнить точки с разными физическими величинами");

            return a.MainPhysicalQuantity.GetNoramalizeValueToSi() >= b.MainPhysicalQuantity.GetNoramalizeValueToSi();
        }
        public static bool operator ==(MeasPoint<TPhysicalQuantity> a,
            MeasPoint<TPhysicalQuantity> b)
        {
            if (!Equals(a.MainPhysicalQuantity.Unit, b.MainPhysicalQuantity.Unit)) throw new ArgumentException("Не возможно сравнить точки с разными физическими величинами");

            return a.MainPhysicalQuantity.GetNoramalizeValueToSi() == b.MainPhysicalQuantity.GetNoramalizeValueToSi();
        }

        public static bool operator !=(MeasPoint<TPhysicalQuantity> a, MeasPoint<TPhysicalQuantity> b)
        {
            if (!Equals(a.MainPhysicalQuantity.Unit, b.MainPhysicalQuantity.Unit)) throw new ArgumentException("Не возможно сравнить точки с разными физическими величинами");
            return a.MainPhysicalQuantity.GetNoramalizeValueToSi() != b.MainPhysicalQuantity.GetNoramalizeValueToSi();
        }

        #endregion


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
        public MeasPoint(decimal value, UnitMultiplier multiplier = UnitMultiplier.None) :this()
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
                                                                      IMeasPoint<TPhysicalQuantity, TAddPhysicalQuantity> where TAddPhysicalQuantity : class, IPhysicalQuantity<TAddPhysicalQuantity>, new() where TPhysicalQuantity : class, IPhysicalQuantity<TPhysicalQuantity>, new()
    {
        public MeasPoint(TPhysicalQuantity physical, TAddPhysicalQuantity addPhysicalQuantity)
        {
            MainPhysicalQuantity = physical;
            AdditionalPhysicalQuantity = addPhysicalQuantity;
        }

        /// <inheritdoc />
        public TAddPhysicalQuantity AdditionalPhysicalQuantity { get; set; }

        /// <inheritdoc />
        public override string Description => MainPhysicalQuantity + AdditionalPhysicalQuantity.ToString();

        /// <inheritdoc />
        public override object Clone()
        {
            return new
                MeasPoint<TPhysicalQuantity, TAddPhysicalQuantity>((TPhysicalQuantity) MainPhysicalQuantity.Clone(),
                                                                   (TAddPhysicalQuantity) AdditionalPhysicalQuantity
                                                                      .Clone());
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
        public MeasPoint(decimal physical, UnitMultiplier multiplier, decimal addPhysicalQuantity, UnitMultiplier addmultipliers) :this()
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
        public int CompareTo(IMeasPoint<TPhysicalQuantity, TAddPhysicalQuantity> other)
        {
            return MainPhysicalQuantity.CompareTo(other.MainPhysicalQuantity);
        }

        #region Operators

        #region Arifmetic

        public static MeasPoint<TPhysicalQuantity, TAddPhysicalQuantity> operator +(MeasPoint<TPhysicalQuantity, TAddPhysicalQuantity> a,
           MeasPoint<TPhysicalQuantity, TAddPhysicalQuantity> b)
        {
            if (!Equals(a.MainPhysicalQuantity.Unit, b.MainPhysicalQuantity.Unit)
                ||
                !Equals(a.AdditionalPhysicalQuantity.Unit, b.AdditionalPhysicalQuantity.Unit)
                ||
                !Equals(a.AdditionalPhysicalQuantity.GetNoramalizeValueToSi(), b.AdditionalPhysicalQuantity.GetNoramalizeValueToSi())) throw new ArgumentException("Не возможно выполнить операцию с разными физическими величинами");


            return new MeasPoint<TPhysicalQuantity, TAddPhysicalQuantity>(a.MainPhysicalQuantity.GetNoramalizeValueToSi()
                                                    + b.MainPhysicalQuantity.GetNoramalizeValueToSi(),(TAddPhysicalQuantity)a.AdditionalPhysicalQuantity.Clone() );
        }
        public static MeasPoint<TPhysicalQuantity, TAddPhysicalQuantity> operator -(MeasPoint<TPhysicalQuantity, TAddPhysicalQuantity> a,
            MeasPoint<TPhysicalQuantity, TAddPhysicalQuantity> b)
        {
            if (!Equals(a.MainPhysicalQuantity.Unit, b.MainPhysicalQuantity.Unit)
                ||
                !Equals(a.AdditionalPhysicalQuantity.Unit, b.AdditionalPhysicalQuantity.Unit)
                ||
                !Equals(a.AdditionalPhysicalQuantity.GetNoramalizeValueToSi(), b.AdditionalPhysicalQuantity.GetNoramalizeValueToSi())) throw new ArgumentException("Не возможно выполнить операцию с разными физическими величинами");

            return new MeasPoint<TPhysicalQuantity, TAddPhysicalQuantity>(a.MainPhysicalQuantity.GetNoramalizeValueToSi()
                                                    - b.MainPhysicalQuantity.GetNoramalizeValueToSi(), (TAddPhysicalQuantity)a.AdditionalPhysicalQuantity.Clone());
        }

        public static MeasPoint<TPhysicalQuantity, TAddPhysicalQuantity> operator *(MeasPoint<TPhysicalQuantity, TAddPhysicalQuantity> a,
            MeasPoint<TPhysicalQuantity, TAddPhysicalQuantity> b)
        {
            if (!Equals(a.MainPhysicalQuantity.Unit, b.MainPhysicalQuantity.Unit)
                ||
                !Equals(a.AdditionalPhysicalQuantity.Unit, b.AdditionalPhysicalQuantity.Unit)
                ||
                !Equals(a.AdditionalPhysicalQuantity.GetNoramalizeValueToSi(), b.AdditionalPhysicalQuantity.GetNoramalizeValueToSi())) throw new ArgumentException("Не возможно выполнить операцию с разными физическими величинами");

            return new MeasPoint<TPhysicalQuantity, TAddPhysicalQuantity>(a.MainPhysicalQuantity.GetNoramalizeValueToSi()
                                                    * b.MainPhysicalQuantity.GetNoramalizeValueToSi(), (TAddPhysicalQuantity)a.AdditionalPhysicalQuantity.Clone());
        }
        public static decimal operator /(MeasPoint<TPhysicalQuantity, TAddPhysicalQuantity> a,
            MeasPoint<TPhysicalQuantity, TAddPhysicalQuantity> b)
        {
            if (!Equals(a.MainPhysicalQuantity.Unit, b.MainPhysicalQuantity.Unit) 
                ||
                !Equals(a.AdditionalPhysicalQuantity.Unit, b.AdditionalPhysicalQuantity.Unit)
                ||
                !Equals(a.AdditionalPhysicalQuantity.GetNoramalizeValueToSi(), b.AdditionalPhysicalQuantity.GetNoramalizeValueToSi())) throw new ArgumentException("Не возможно выполнить операцию с разными физическими величинами");

            return a.MainPhysicalQuantity.GetNoramalizeValueToSi() /b.MainPhysicalQuantity.GetNoramalizeValueToSi();
        }

        public static MeasPoint<TPhysicalQuantity, TAddPhysicalQuantity> operator +(MeasPoint<TPhysicalQuantity, TAddPhysicalQuantity> a,
            decimal b)
        {
            var mp = (MeasPoint<TPhysicalQuantity, TAddPhysicalQuantity>)a.Clone();
            mp.MainPhysicalQuantity.Value = mp.MainPhysicalQuantity.Value + b;
            return mp;
        }
        public static MeasPoint<TPhysicalQuantity, TAddPhysicalQuantity> operator -(MeasPoint<TPhysicalQuantity, TAddPhysicalQuantity> a,
            decimal b)
        {
            var mp = (MeasPoint<TPhysicalQuantity, TAddPhysicalQuantity>)a.Clone();
            mp.MainPhysicalQuantity.Value = mp.MainPhysicalQuantity.Value - b;
            return mp;
        }

        public static MeasPoint<TPhysicalQuantity, TAddPhysicalQuantity> operator *(MeasPoint<TPhysicalQuantity, TAddPhysicalQuantity> a,
            decimal b)
        {
            var mp = (MeasPoint<TPhysicalQuantity, TAddPhysicalQuantity>)a.Clone();
            mp.MainPhysicalQuantity.Value = mp.MainPhysicalQuantity.Value * b;
            return mp;
        }
        public static MeasPoint<TPhysicalQuantity, TAddPhysicalQuantity> operator /(MeasPoint<TPhysicalQuantity, TAddPhysicalQuantity> a,
            decimal b)
        {
            var mp = (MeasPoint<TPhysicalQuantity, TAddPhysicalQuantity>)a.Clone();
            mp.MainPhysicalQuantity.Value = mp.MainPhysicalQuantity.Value / b;
            return mp;
        }

        #endregion

        public static bool operator <(MeasPoint<TPhysicalQuantity, TAddPhysicalQuantity> a,
            MeasPoint<TPhysicalQuantity, TAddPhysicalQuantity> b)
        {
            if (!Equals(a.MainPhysicalQuantity.Unit, b.MainPhysicalQuantity.Unit) || !Equals(a.AdditionalPhysicalQuantity.Unit, b.AdditionalPhysicalQuantity.Unit)) throw new ArgumentException("Не возможно выполнить сравнение точек с разными физическими величинами");

            return a.MainPhysicalQuantity.GetNoramalizeValueToSi() < b.MainPhysicalQuantity.GetNoramalizeValueToSi();
        }
        public static bool operator >(MeasPoint<TPhysicalQuantity, TAddPhysicalQuantity> a,
            MeasPoint<TPhysicalQuantity, TAddPhysicalQuantity> b)
        {
            if (!Equals(a.MainPhysicalQuantity.Unit, b.MainPhysicalQuantity.Unit) || !Equals(a.AdditionalPhysicalQuantity.Unit, b.AdditionalPhysicalQuantity.Unit)) throw new ArgumentException("Не возможно выполнить сравнение точек с разными физическими величинами");

            return a.MainPhysicalQuantity.GetNoramalizeValueToSi() > b.MainPhysicalQuantity.GetNoramalizeValueToSi();
        }

        public static bool operator <=(MeasPoint<TPhysicalQuantity, TAddPhysicalQuantity> a,
            MeasPoint<TPhysicalQuantity, TAddPhysicalQuantity> b)
        {

            if (!Equals(a.MainPhysicalQuantity.Unit, b.MainPhysicalQuantity.Unit) || !Equals(a.AdditionalPhysicalQuantity.Unit, b.AdditionalPhysicalQuantity.Unit)) throw new ArgumentException("Не возможно выполнить сравнение точек с разными физическими величинами");

            return a.MainPhysicalQuantity.GetNoramalizeValueToSi() <= b.MainPhysicalQuantity.GetNoramalizeValueToSi();
        }
        public static bool operator >=(MeasPoint<TPhysicalQuantity, TAddPhysicalQuantity> a,
            MeasPoint<TPhysicalQuantity, TAddPhysicalQuantity> b)
        {
            if (!Equals(a.MainPhysicalQuantity.Unit, b.MainPhysicalQuantity.Unit) || !Equals(a.AdditionalPhysicalQuantity.Unit, b.AdditionalPhysicalQuantity.Unit)) throw new ArgumentException("Не возможно выполнить сравнение точек с разными физическими величинами");

            return a.MainPhysicalQuantity.GetNoramalizeValueToSi() >= b.MainPhysicalQuantity.GetNoramalizeValueToSi();
        }
        public static bool operator ==(MeasPoint<TPhysicalQuantity, TAddPhysicalQuantity> a,
            MeasPoint<TPhysicalQuantity, TAddPhysicalQuantity> b)
        {
            if (!Equals(a.MainPhysicalQuantity.Unit, b.MainPhysicalQuantity.Unit) || !Equals(a.AdditionalPhysicalQuantity.Unit, b.AdditionalPhysicalQuantity.Unit)) throw new ArgumentException("Не возможно выполнить сравнение точек с разными физическими величинами");

            return a.MainPhysicalQuantity.GetNoramalizeValueToSi() == b.MainPhysicalQuantity.GetNoramalizeValueToSi();
        }

        public static bool operator !=(MeasPoint<TPhysicalQuantity, TAddPhysicalQuantity> a,
            MeasPoint<TPhysicalQuantity, TAddPhysicalQuantity> b)
        {
            if (!Equals(a.MainPhysicalQuantity.Unit, b.MainPhysicalQuantity.Unit) || !Equals(a.AdditionalPhysicalQuantity.Unit, b.AdditionalPhysicalQuantity.Unit)) throw new ArgumentException("Не возможно выполнить сравнение точек с разными физическими величинами");
            return a.MainPhysicalQuantity.GetNoramalizeValueToSi() != b.MainPhysicalQuantity.GetNoramalizeValueToSi();
        }

        #endregion
    }


    /// <summary>
    /// Предоставляет реализацию допустимых диапазнов (пределов) воспроизведения/измерения физических величин.
    /// </summary>
    public class PhysicalRange<T>: IPhysicalRange<T> where T : class, IPhysicalQuantity<T>,  new()
    {
        #region Property

        /// <inheritdoc />
        public AccuracyChatacteristic AccuracyChatacteristic { get; set; }

        /// <inheritdoc />
        public IMeasPoint<T> Start { get;  set; }

        /// <inheritdoc />
        public IMeasPoint<T> Stop { get;  set; }

        public MeasureUnits Unit
        {
            get => Start.MainPhysicalQuantity.Unit;
        }

        #endregion
        public PhysicalRange(MeasPoint<T> stopRange, AccuracyChatacteristic accuracy=null)
        {
            AccuracyChatacteristic = accuracy;
            Start = new MeasPoint<T>();
            Stop = stopRange;
        }
        public PhysicalRange(MeasPoint<T> startRange, MeasPoint<T> stopRange, AccuracyChatacteristic accuracy = null)
        {
            if (!Equals(startRange.MainPhysicalQuantity.Unit, stopRange.MainPhysicalQuantity.Unit))
                throw new ArgumentException("Не возможно сравнить точки с разными физическими величинами");
            AccuracyChatacteristic = accuracy;
            Start = startRange;
            Stop = stopRange;

        }

        public PhysicalRange()
        {
            Start = new MeasPoint<T>();
            Stop = new MeasPoint<T>();
        }

        /// <inheritdoc />
        public int CompareTo(IPhysicalRange<T> other)
        {
            return Start.CompareTo(other.Start);
        }
    }
    /// <summary>
    /// Предоставляет реализацию допустимых диапазнов (пределов) воспроизведения/измерения физических величин.
    /// </summary>
    public class PhysicalRange<TPhysicalQuantity,TAddition> : IPhysicalRange<TPhysicalQuantity,TAddition> where TAddition : class, IPhysicalQuantity<TAddition>, new() where TPhysicalQuantity : class, IPhysicalQuantity<TPhysicalQuantity>, new()
    {
        #region Property

        /// <inheritdoc />
        public AccuracyChatacteristic AccuracyChatacteristic { get; set; }

        /// <inheritdoc />
        public IMeasPoint<TPhysicalQuantity, TAddition> Start { get; set; }

        /// <inheritdoc />
        public IMeasPoint<TPhysicalQuantity, TAddition> Stop { get; set; }

        public MeasureUnits Unit { get; protected set; }


        #endregion
        public PhysicalRange(IMeasPoint<TPhysicalQuantity, TAddition> stopRange, AccuracyChatacteristic accuracy = null)
        {
            AccuracyChatacteristic = accuracy;
            Start = new MeasPoint<TPhysicalQuantity, TAddition>();
            Stop = stopRange;
            Unit = stopRange.MainPhysicalQuantity.Unit;
        }
        public PhysicalRange(IMeasPoint<TPhysicalQuantity, TAddition> startRange, IMeasPoint<TPhysicalQuantity, TAddition> stopRange, AccuracyChatacteristic accuracy = null)
        {
            if (!Equals(startRange.MainPhysicalQuantity.Unit, stopRange.MainPhysicalQuantity.Unit))
                throw new ArgumentException("Не возможно сравнить точки с разными физическими величинами");
            AccuracyChatacteristic = accuracy;
            Start = startRange;
            Stop = stopRange;

            Unit = startRange.MainPhysicalQuantity.Unit;
        }

        public PhysicalRange()
        {
            Start = new MeasPoint<TPhysicalQuantity,TAddition>();
            Stop = new MeasPoint<TPhysicalQuantity, TAddition>();
        }

        /// <inheritdoc />
        public int CompareTo(IPhysicalRange<TPhysicalQuantity> other)
        {
           return Start.CompareTo(other.Start);
        }
    }

    public interface IPhysicalRange
    {
        /// <summary>
        /// позвляет получить харатеристику точности диапазона.
        /// </summary>
        AccuracyChatacteristic AccuracyChatacteristic { get; set; }
        MeasureUnits Unit { get; }
    }



    public interface IPhysicalRange<T,T1> : IComparable<IPhysicalRange<T>>, IPhysicalRange where T : class, IPhysicalQuantity<T>, new() where T1 : class, IPhysicalQuantity<T1>, new()
    {
        #region Property


        /// <summary>
        /// Значение величины, описывающее начало диапазона (входит в диапазон).
        /// </summary>
        IMeasPoint<T,T1> Start { get; }

        /// <summary>
        /// Значение величины описывающая верхнюю (граничную) точку диапазона (входит в диапазон).
        /// </summary>
        IMeasPoint<T, T1> Stop { get; }


        #endregion
    }


    public interface IPhysicalRange<T>:IComparable<IPhysicalRange<T>>, IPhysicalRange where T : class, IPhysicalQuantity<T>, new()
    {
        #region Property


        /// <summary>
        /// Значение величины, описывающее начало диапазона (входит в диапазон).
        /// </summary>
        IMeasPoint<T> Start { get; set; }

        /// <summary>
        /// Значение величины описывающая верхнюю (граничную) точку диапазона (входит в диапазон).
        /// </summary>
        IMeasPoint<T> Stop { get; set; }


        #endregion
    }

    /// <summary>
    /// Предоставляет реализацию хранилища диапазонов (по виду измерения). Фактически перечень пределов СИ.
    /// </summary>
    public class RangeStorage<T> :IEnumerable where T : IPhysicalRange
    {
        #region Property

        /// <summary>
        /// Наименование.
        /// </summary>
        public string Name { get; set; }

        public T[] RealRangeStor { get; set; }

        #endregion

        public RangeStorage(AccuracyChatacteristic accuracy, params T[] inPhysicalRealRangeStor)
        {
            AccuracyChatacteristic = accuracy;
            RealRangeStor = inPhysicalRealRangeStor;
        }

        public RangeStorage(params T[] inPhysicalRealRangeStor)
        {
            RealRangeStor = inPhysicalRealRangeStor;
        }
        public bool IsPointBelong<T1>(IMeasPoint<T1> point) where T1 : class, IPhysicalQuantity<T1>, new()
        {
                var range = RealRangeStor as IPhysicalRange<T1>[];

                if (range== null) return false;

                var start = range.FirstOrDefault(q => q.Start.MainPhysicalQuantity.GetNoramalizeValueToSi() <=
                                                      point.MainPhysicalQuantity.GetNoramalizeValueToSi());

                var end = range.FirstOrDefault(q => q.Stop.MainPhysicalQuantity.GetNoramalizeValueToSi() >=
                                                    point.MainPhysicalQuantity.GetNoramalizeValueToSi());
                return start != null && end != null;

        }
        public bool IsPointBelong<T1,T2>(IMeasPoint<T1,T2> point) where T1 : class, IPhysicalQuantity<T1>, new() where T2 : class, IPhysicalQuantity<T2>, new()
        {
            var range = RealRangeStor as IPhysicalRange<T1, T2>[];

            if (range == null) return false;

                var start = range.FirstOrDefault(q => q.Start.MainPhysicalQuantity.GetNoramalizeValueToSi() <=
                                                      point.MainPhysicalQuantity.GetNoramalizeValueToSi()
                                                      && q.Start.AdditionalPhysicalQuantity.GetNoramalizeValueToSi()<= point.AdditionalPhysicalQuantity.GetNoramalizeValueToSi());

                var end = range.FirstOrDefault(q => q.Stop.MainPhysicalQuantity.GetNoramalizeValueToSi() >=
                                                    point.MainPhysicalQuantity.GetNoramalizeValueToSi()
                                                    && q.Stop.AdditionalPhysicalQuantity.GetNoramalizeValueToSi() >= point.AdditionalPhysicalQuantity.GetNoramalizeValueToSi());


                return start != null && end != null;

            

        }


        //public Tuple<MeasPoint<T>, MeasPoint<T>> FullRange()
        //{
        //    return new Tuple<MeasPoint<T>, MeasPoint<T>>(Ranges.Min(q => q.Start), Ranges.Max(q => q.Stop));
        //}
        /// <summary>
        /// Позволяет получить характеристику точности
        /// </summary>
        public AccuracyChatacteristic AccuracyChatacteristic { get; set; }

        #region Methods

        /// <summary>
        /// Запрос перечня физических величин в диапазоне.
        /// </summary>
        /// <returns>Возвращает список физических величин, содержащихся в диапазоне.</returns>
        public List<MeasureUnits> GetPhysicalQuantity()
        {
            var result = new List<MeasureUnits>();

            foreach (var range in RealRangeStor)
                result.Add(range.Unit);
            // удаляем дубликаты, если такое возможно!
            return new HashSet<MeasureUnits>(result).ToList();
        }

        #endregion

        /// <inheritdoc />
        public IEnumerator GetEnumerator()
        {
            return RealRangeStor.GetEnumerator();
        }
    }

   
}