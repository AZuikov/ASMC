using System;
using System.Collections.Generic;
using System.Linq;
using AP.Utils.Helps;

namespace ASMC.Data.Model
{
    public interface IMeasPoint<TPhysicalQuantity> : ICloneable, IComparable<IMeasPoint<TPhysicalQuantity>>
        where TPhysicalQuantity : class, IPhysicalQuantity<TPhysicalQuantity>, IEquatable<TPhysicalQuantity>, new()
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
        where TAddPhysicalQuantity : IPhysicalQuantity, IEquatable<TPhysicalQuantity>, new()
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
        public static MeasPoint<TPhysicalQuantity> operator /(MeasPoint<TPhysicalQuantity> a,
            MeasPoint<TPhysicalQuantity> b)
        {
            if (!Equals(a.MainPhysicalQuantity.Unit, b.MainPhysicalQuantity.Unit)) throw new ArgumentException("Не возможно выполнить операцию с разными физическими величинами");

            return new MeasPoint<TPhysicalQuantity>(a.MainPhysicalQuantity.GetNoramalizeValueToSi())
                                                    / (b.MainPhysicalQuantity.GetNoramalizeValueToSi());
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
        /// <param name = "multipliers">Множитель, по умолчению <see cref = "UnitMultipliers.None" /></param>
        public MeasPoint(decimal value, UnitMultipliers multipliers = UnitMultipliers.None) 
        {
            MainPhysicalQuantity.Value = value;
            MainPhysicalQuantity.Multipliers = multipliers;
        }

        public MeasPoint()
        {
            MainPhysicalQuantity = new TPhysicalQuantity();
        }

        #endregion Constructor
    }

    public class MeasPoint<TPhysicalQuantity, TAddPhysicalQuantity> : MeasPoint<TPhysicalQuantity>,
                                                                      IMeasPoint<TPhysicalQuantity, TAddPhysicalQuantity>
        where TPhysicalQuantity : class, IPhysicalQuantity<TPhysicalQuantity>, new()
        where TAddPhysicalQuantity : IPhysicalQuantity, IEquatable<TPhysicalQuantity>, new()
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

        private MeasPoint(decimal value, TAddPhysicalQuantity addPhysicalQuantity)
        {
            MainPhysicalQuantity.Value = value;
            AdditionalPhysicalQuantity = addPhysicalQuantity;
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
        public static MeasPoint<TPhysicalQuantity, TAddPhysicalQuantity> operator /(MeasPoint<TPhysicalQuantity, TAddPhysicalQuantity> a,
            MeasPoint<TPhysicalQuantity, TAddPhysicalQuantity> b)
        {
            if (!Equals(a.MainPhysicalQuantity.Unit, b.MainPhysicalQuantity.Unit) 
                ||
                !Equals(a.AdditionalPhysicalQuantity.Unit, b.AdditionalPhysicalQuantity.Unit)
                ||
                !Equals(a.AdditionalPhysicalQuantity.GetNoramalizeValueToSi(), b.AdditionalPhysicalQuantity.GetNoramalizeValueToSi())) throw new ArgumentException("Не возможно выполнить операцию с разными физическими величинами");

            return new MeasPoint<TPhysicalQuantity, TAddPhysicalQuantity>((a.MainPhysicalQuantity.GetNoramalizeValueToSi())
                                                    / (b.MainPhysicalQuantity.GetNoramalizeValueToSi()),(TAddPhysicalQuantity)a.AdditionalPhysicalQuantity.Clone());
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


        #endregion
    }


    /// <summary>
    /// Предоставляет реализацию допустимых диапазнов (пределов) воспроизведения/измерения физических величин.
    /// </summary>
    public class PhysicalRange<T>: IPhysicalRange<MeasPoint<T>> where T : class, IPhysicalQuantity<T>, new()
    {
        #region Property

        /// <inheritdoc />
        public AccuracyChatacteristic AccuracyChatacteristic { get; }

        /// <inheritdoc />
        public MeasPoint<T> Start { get; protected set; }

        /// <inheritdoc />
        public MeasPoint<T> Stop { get; protected set; }

        public MeasureUnits Unit { get; protected set; }

        #endregion
        public PhysicalRange(MeasPoint<T> stopRange, AccuracyChatacteristic accuracy=null)
        {
            AccuracyChatacteristic = accuracy;
            Start = new MeasPoint<T>();
            Stop = stopRange;
            Unit = stopRange.MainPhysicalQuantity.Unit;
        }
        public PhysicalRange(MeasPoint<T> startRange, MeasPoint<T> stopRange, AccuracyChatacteristic accuracy = null)
        {
            if (!Equals(startRange.MainPhysicalQuantity.Unit, stopRange.MainPhysicalQuantity.Unit))
                throw new ArgumentException("Не возможно сравнить точки с разными физическими величинами");
            AccuracyChatacteristic = accuracy;
            Start = startRange;
            Stop = stopRange;

            Unit = startRange.MainPhysicalQuantity.Unit;
        }
    }
    /// <summary>
    /// Предоставляет реализацию допустимых диапазнов (пределов) воспроизведения/измерения физических величин.
    /// </summary>
    public class PhysicalRange<T,Tadd> : IPhysicalRange<MeasPoint<T,Tadd>> where T : class, IPhysicalQuantity<T>, new() where Tadd : IPhysicalQuantity, IEquatable<T>, new()
    {
        #region Property

        /// <inheritdoc />
        public AccuracyChatacteristic AccuracyChatacteristic { get; }

        /// <inheritdoc />
        public MeasPoint<T, Tadd> Start { get; protected set; }

        /// <inheritdoc />
        public MeasPoint<T, Tadd> Stop { get; protected set; }

        public MeasureUnits Unit { get; protected set; }

        #endregion
        public PhysicalRange(MeasPoint<T, Tadd> stopRange, AccuracyChatacteristic accuracy = null)
        {
            AccuracyChatacteristic = accuracy;
            Start = new MeasPoint<T, Tadd>();
            Stop = stopRange;
            Unit = stopRange.MainPhysicalQuantity.Unit;
        }
        public PhysicalRange(MeasPoint<T, Tadd> startRange, MeasPoint<T, Tadd> stopRange, AccuracyChatacteristic accuracy = null)
        {
            if (!Equals(startRange.MainPhysicalQuantity.Unit, stopRange.MainPhysicalQuantity.Unit))
                throw new ArgumentException("Не возможно сравнить точки с разными физическими величинами");
            AccuracyChatacteristic = accuracy;
            Start = startRange;
            Stop = stopRange;

            Unit = startRange.MainPhysicalQuantity.Unit;
        }
    }






    public interface IPhysicalRange<out T>
    {
        #region Property

        /// <summary>
        /// позвляет получить харатеристику точности диапазона.
        /// </summary>
        AccuracyChatacteristic AccuracyChatacteristic { get;} 
        /// <summary>
        /// Значение величины, описывающее начало диапазона (входит в диапазон).
        /// </summary>
        T Start { get; }

        /// <summary>
        /// Значение величины описывающая верхнюю (граничную) точку диапазона (входит в диапазон).
        /// </summary>
        T Stop { get; }

        MeasureUnits Unit { get; }

        #endregion
    }

    /// <summary>
    /// Предоставляет реализацию хранилища диапазонов (по виду измерения). Фактически перечень пределов СИ.
    /// </summary>
    public class RangeStorage<T> where T : class, IPhysicalQuantity<T>, new()
    {
        #region Property

        /// <summary>
        /// Наименование.
        /// </summary>
        public string Name { get; set; }

        public PhysicalRange<T>[] Ranges { get; set; }

        #endregion

        public RangeStorage(params PhysicalRange<T>[] inPhysicalRange)
        {
            Ranges = inPhysicalRange;
        }

        public Tuple<MeasPoint<T>, MeasPoint<T>> FullRange()
        {
            return new Tuple<MeasPoint<T>, MeasPoint<T>>(Ranges.Min(q => q.Start), Ranges.Max(q => q.Stop));
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

        #endregion

    }
}