﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AP.Utils.Data;
using ASMC.Data.Model.Interface;
using DevExpress.Xpf.Core.HandleDecorator;

namespace ASMC.Data.Model.PhysicalQuantity
{


    public interface IPhysicalQuantity : IComparable, ICloneable
    {
        ///  <summary>
        /// Изменяет множитель на заданный
        ///  </summary>
        ///  <param name="multiplier"></param>
        ///  <returns></returns>
        IPhysicalQuantity ChangeMultiplier(UnitMultiplier multiplier);
        /// <summary>
        /// Возвращает единицы измерения с множителем.
        /// </summary>
        /// <returns></returns>
        string GetMultiplierUnit();
        /// <summary>
        /// Возвращает численное значение в системе СИ.
        /// </summary>
        /// <returns></returns>
        decimal GetNoramalizeValueToSi();
        #region Property

        /// <summary>
        /// Позволяет задать или получить множитель фезической величины
        /// </summary>
        UnitMultiplier Multiplier { get; set; }

        /// <summary>
        /// Позволяет задать или получить еденицу езмерения данной физической величины
        /// </summary>
        MeasureUnits Unit { get; set; }

        /// <summary>
        /// Предоставляет перечень допустимый единиц измерений. Например Давение может быть в Па и в м.рт.ст
        /// </summary>
        MeasureUnits[] Units { get; }

        /// <summary>
        /// Позволяет задать или получить знаенчие физической величины
        /// </summary>
        decimal Value { get; set; }

        #endregion
    }


    public interface IPhysicalQuantity<T> : IEquatable<T>,IComparable<T>, IPhysicalQuantity where T : class, IPhysicalQuantity
    {

    }
    /// <summary>
    /// Предоставляет базовую реализацию физической величины
    /// </summary>
    public abstract class PhysicalQuantity<T> : IPhysicalQuantity<T>
        where T : class, IPhysicalQuantity
    {
        protected PhysicalQuantity(decimal value):this()
        {
            Value = value;
        }
        protected PhysicalQuantity(decimal value, UnitMultiplier multiplier ) :this(value)
        {
            Multiplier = multiplier;
        }
        #region Fields

        private MeasureUnits _unit;

        public PhysicalQuantity()
        {

        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public override string ToString()
        {
            return $@"{Value} {GetMultiplierUnit()}";
        }

        /// <summary>
        /// Возвращает результат проверки пренадлишности едениц измерения к физической величине.
        /// </summary>
        /// <param name = "units"></param>
        /// <returns></returns>
        protected bool CheckedAttachmentUnits(MeasureUnits units)
        {
            return Array.FindIndex(Units, item => item == units) != -1;
        }

        protected IPhysicalQuantity ThisConvetToSi()
        {
            var pq = (IPhysicalQuantity)Activator.CreateInstance(GetType());
            pq.Value = Value * (decimal)Multiplier.GetDoubleValue();
            pq.Multiplier = UnitMultiplier.None;
            pq.Unit = Unit;
            return pq;
        }

        #endregion

        public int CompareTo(object obj)
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj));
            return string.Compare(GetType().Name, obj.GetType().Name, StringComparison.Ordinal);
        }

        public int CompareTo(T other)
        {
            if (Unit != other.Unit) throw new ArgumentException();
            return (Value * (decimal)Multiplier.GetDoubleValue()).CompareTo(other.Value *
                                                                              (decimal)other
                                                                                       .Multiplier.GetDoubleValue());
        }

        public virtual bool Equals(T other)
        {
            return Unit == other?.Unit && Value * (decimal)Multiplier.GetDoubleValue() ==
                other.Value * (decimal)other.Multiplier.GetDoubleValue();
        }

        /// <inheritdoc />
        public MeasureUnits[] Units { get; protected set; }

        /// <inheritdoc />
        public MeasureUnits Unit
        {
            get => _unit;
            set
            {
                if (!CheckedAttachmentUnits(value))
                    throw new ArgumentOutOfRangeException($@"{value} не входит в допустимый список едениц измиериний");

                _unit = value;
            }
        }

        /// <inheritdoc />
        public IPhysicalQuantity ChangeMultiplier(UnitMultiplier multiplier)
        {
            this.Value = this.GetNoramalizeValueToSi() / (decimal) multiplier.GetDoubleValue();
            this.Multiplier = multiplier;
            return this;
        }

        /// <summary>
        /// Автоматически выбирает оптимальный множитель физической величины, для удобства отображения. Без выполнения окргуления.
        /// </summary>
        public void AutoSelectMultiplier()
        {
            UnitMultiplier baseMultiplier = Multiplier;

            if (Value == 0) return;
            
            else if (Value > 1)
            {
                foreach (UnitMultiplier newMultiplier in Enum.GetValues(typeof(UnitMultiplier)))
                {
                    decimal checkValue = GetNoramalizeValueToSi() / (decimal)newMultiplier.GetDoubleValue();
                    if (checkValue < 1000)//сравниваем их порядковые номера в перечислении
                    {
                        ChangeMultiplier(newMultiplier);
                        return;
                    }
                }
            }
            else //if (Value < 1)
            {
                foreach (UnitMultiplier newMultiplier in Enum.GetValues(typeof(UnitMultiplier)))
                {
                    decimal checkValue = GetNoramalizeValueToSi() / (decimal) newMultiplier.GetDoubleValue();
                    if (checkValue < 1000 )
                    {
                        ChangeMultiplier(newMultiplier);
                         return;

                    }
                }
            }
        }

        /// <inheritdoc />
        public string GetMultiplierUnit()
        {
            //todo нужно выцеплять верный множитель с учетом культуры
            
            return Multiplier.GetStringValue() + Unit.GetStringValue();
        }

        /// <inheritdoc />
        public decimal GetNoramalizeValueToSi()
        {
            return Value * (decimal)Multiplier.GetDoubleValue();
        }

        /// <inheritdoc />
        public UnitMultiplier Multiplier { get; set; }

        /// <inheritdoc />
        public decimal Value { get; set; }

        #region Operator

        public static bool operator >(PhysicalQuantity<T> a, PhysicalQuantity<T> b)
        {
            if (a.Unit == b.Unit &&
                a.Value * (decimal)a.Multiplier.GetDoubleValue() >
                b.Value * (decimal)b.Multiplier.GetDoubleValue())
                return true;

            return false;
        }

        public static bool operator <(PhysicalQuantity<T> a, PhysicalQuantity<T> b)
        {
            if (a.Unit == b.Unit &&
                a.Value * (decimal)a.Multiplier.GetDoubleValue() <
                b.Value * (decimal)b.Multiplier.GetDoubleValue())
                return true;

            return false;
        }

        public static bool operator ==(PhysicalQuantity<T> a, PhysicalQuantity<T> b)
        {
            if (a?.Unit == b?.Unit &&
                a?.Value * (decimal)a?.Multiplier.GetDoubleValue() ==
                b?.Value * (decimal)b?.Multiplier.GetDoubleValue())
                return true;

            return false;
        }

        public static bool operator !=(PhysicalQuantity<T> a, PhysicalQuantity<T> b)
        {
            if (a?.Unit == b?.Unit &&
                a?.Value * (decimal)a?.Multiplier.GetDoubleValue() !=
                b?.Value * (decimal)b?.Multiplier.GetDoubleValue())
                return true;

            return false;
        }

        public static PhysicalQuantity<T> operator +(PhysicalQuantity<T> a, PhysicalQuantity<T> b)
        {
            if (a.Unit == b.Unit)
            {
                var obj = Activator.CreateInstance(a.GetType()) as PhysicalQuantity<T>;
                obj.Value = a.Value * (decimal)a.Multiplier.GetDoubleValue() +
                            b.Value * (decimal)b.Multiplier.GetDoubleValue();
                obj.Unit = a.Unit;
                return obj;
            }

            throw new ArgumentException("Не соответствуют единицы измерения операндов");
        }

        public static PhysicalQuantity<T> operator -(PhysicalQuantity<T> a, PhysicalQuantity<T> b)
        {
            if (a.Unit == b.Unit)
            {
                var obj = Activator.CreateInstance(a.GetType()) as PhysicalQuantity<T>;
                obj.Value = a.Value * (decimal)a.Multiplier.GetDoubleValue() -
                            b.Value * (decimal)b.Multiplier.GetDoubleValue();
                obj.Unit = a.Unit;
                return obj;
            }

            throw new ArgumentException("Не соответствуют единицы измерения операндов");
        }

        public static PhysicalQuantity<T> operator *(PhysicalQuantity<T> a, PhysicalQuantity<T> b)
        {
            if (a.Unit == b.Unit)
            {
                var obj = Activator.CreateInstance(a.GetType()) as PhysicalQuantity<T>;
                obj.Value = a.Value * (decimal)a.Multiplier.GetDoubleValue() *
                            b.Value * (decimal)b.Multiplier.GetDoubleValue();
                obj.Unit = a.Unit;
                return obj;
            }

            throw new ArgumentException("Не соответствуют единицы измерения операндов");
        }

        public static PhysicalQuantity<T> operator /(PhysicalQuantity<T> a, PhysicalQuantity<T> b)
        {
            if (a.Unit == b.Unit)
            {
                var obj = Activator.CreateInstance(a.GetType()) as PhysicalQuantity<T>;
                obj.Value = a.Value * (decimal)a.Multiplier.GetDoubleValue() /
                            (b.Value * (decimal)b.Multiplier.GetDoubleValue());
                obj.Unit = a.Unit;
                return obj;
            }

            throw new ArgumentException("Не соответствуют единицы измерения операндов");
        }

        public static PhysicalQuantity<T> operator /(PhysicalQuantity<T> a, decimal b)
        {
            var obj = Activator.CreateInstance(a.GetType()) as PhysicalQuantity<T>;
            obj.Value = a.Value * (decimal)a.Multiplier.GetDoubleValue() / b;
            obj.Unit = a.Unit;
            return obj;
        }

        public static PhysicalQuantity<T> operator *(PhysicalQuantity<T> a, decimal b)
        {
            var obj = Activator.CreateInstance(a.GetType()) as PhysicalQuantity<T>;
            obj.Value = a.Value * (decimal)a.Multiplier.GetDoubleValue() * b;
            obj.Unit = a.Unit;
            return obj;
        }

        public static PhysicalQuantity<T> operator +(PhysicalQuantity<T> a, decimal b)
        {
            var obj = Activator.CreateInstance(a.GetType()) as PhysicalQuantity<T>;
            obj.Value = a.Value * (decimal)a.Multiplier.GetDoubleValue() + b;
            obj.Unit = a.Unit;
            return obj;
        }

        public static PhysicalQuantity<T> operator -(PhysicalQuantity<T> a, decimal b)
        {
            var obj = Activator.CreateInstance(a.GetType()) as PhysicalQuantity<T>;
            obj.Value = a.Value * (decimal)a.Multiplier.GetDoubleValue() - b;
            obj.Unit = a.Unit;
            return obj;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as T);
        }

       

        /// <inheritdoc />
        public object Clone()
        {
            var obj = Activator.CreateInstance(GetType()) as PhysicalQuantity<T>;
            obj.Value = Value;
            obj.Multiplier = Multiplier;
            obj.Unit = Unit;
            return obj;
        }

        #endregion
    }
    /// <summary>
    /// Реализует физическую величину давление
    /// </summary>
    public sealed class Pressure : PhysicalQuantity<Pressure>, IConvertPhysicalQuantity<Pressure>
    {
        /// <inheritdoc />
        public Pressure(decimal value) : base(value)
        {
        }

        /// <inheritdoc />
        public Pressure(decimal value, UnitMultiplier multiplier) : base(value, multiplier)
        {
        }

        public Pressure()
        {
            Units = new[] { MeasureUnits.Pressure, MeasureUnits.MercuryPressure };
        }

        public Pressure Convert(MeasureUnits u)
        {
            if (CheckedAttachmentUnits(u))
                throw new InvalidCastException("Невозможно преобразовать в другой тип физической величины");
            if (Unit == u) return this;
          var pq=  ThisConvetToSi();
            switch (u)
            {
                case MeasureUnits.MercuryPressure:
                    return new Pressure { Unit = u, Value = decimal.Parse(pq.Value.ToString()) / 0.0000075018754688672M};
                case MeasureUnits.Pressure:
                    return new Pressure { Unit = u, Value = decimal.Parse(pq.Value.ToString()) * 0.0000075018754688672M };
            }

            return null;
        }
    }


    /// <summary>
    /// Реализует физическую величину масса.
    /// </summary>
    public sealed class Weight : PhysicalQuantity<Weight>
    {
        public Weight()
        {
            Units = new[] { MeasureUnits.Weight };
            Unit = MeasureUnits.Weight;
        }

        public Force ConvertToForce()
        {
            return new Force{Value=Value/100};
        }
    }
    /// <summary>
    /// Реализует физическую величину сила.
    /// </summary>
    public sealed class Force : PhysicalQuantity<Force>
    {
        /// <inheritdoc />
        public Force(decimal value) : base(value)
        {
            Units = new[] { MeasureUnits.N };
            Unit = MeasureUnits.N;
        }

        /// <inheritdoc />
        public Force(decimal value, UnitMultiplier multiplier) : base(value, multiplier)
        {
            Units = new[] { MeasureUnits.N };
            Unit = MeasureUnits.N;
        }

        public Force()
        {
            Units = new[] { MeasureUnits.N };
            Unit = MeasureUnits.N;
        }
    }

    /// <summary>
    /// Реализует физическую величину мощности.
    /// </summary>
    public sealed class Power : PhysicalQuantity<Power>, IConvertPhysicalQuantity<Power>
    {
        /// <inheritdoc />
        public Power(decimal value) : base(value)
        {
            Units = new[] { MeasureUnits.Watt, MeasureUnits.dBm };
            Unit = MeasureUnits.Watt;
        }

        /// <inheritdoc />
        public Power(decimal value, UnitMultiplier multiplier) : base(value, multiplier)
        {
            Units = new[] { MeasureUnits.Watt, MeasureUnits.dBm };
            Unit = MeasureUnits.Watt;
        }

        public Power()
        {
            Units = new[] { MeasureUnits.Watt, MeasureUnits.dBm, MeasureUnits.V };
            Unit = MeasureUnits.Watt;
        }

        public Power Convert(MeasureUnits u)
        {
            if (CheckedAttachmentUnits(u))
                throw new InvalidCastException("Невозможно преобразовать в другой тип физической величины");
            if (Unit == u) return this;
            var currentPhysicalQuantity = ThisConvetToSi();
            //варианты конвертации
            
            if (Unit == MeasureUnits.Watt && u == MeasureUnits.dBm)
            {
                //формула для тракта с нагрузкой 50 Ом
                return  new Power{Unit = u, Value = 10 *(decimal)Math.Log10((double)currentPhysicalQuantity.Value / 0.001) };
            }
            if (Unit == MeasureUnits.dBm && u == MeasureUnits.Watt)
            {
                return new Power { Unit = u, Value = (decimal)Math.Pow(10, (double)currentPhysicalQuantity.Value / 10.0) * 0.001M };
            }
            
            //Ваты  в вольты
            if (Unit == MeasureUnits.Watt && u == MeasureUnits.V)
            {
                return new Power { Unit = u, Value = (decimal)Math.Sqrt((double)currentPhysicalQuantity.Value*50) };
            }
            //вольты в Ваты 
            if (Unit == MeasureUnits.V && u == MeasureUnits.Watt)
            {
                return new Power { Unit = u, Value = (decimal)Math.Pow((double)currentPhysicalQuantity.Value,2)/50 };
            }

            // dBm в Вольты
            if (Unit == MeasureUnits.dBm && u == MeasureUnits.V)
            {
                return new Power { Unit = u, Value = (decimal)Math.Sqrt(0.001*50*Math.Pow(10,(double)currentPhysicalQuantity.Value/10)) };
            }
            // Вольты в dBm
            if (Unit == MeasureUnits.V && u == MeasureUnits.dBm)
            {
                return new Power { Unit = u, Value = 10* (decimal)Math.Log10(Math.Pow((double)currentPhysicalQuantity.Value,2) / (0.001*50))};
            }

            

            return null;
        }
    }

    /// <summary>
    /// Реализует безразмерную физическую величину (без единиц измерения).
    /// </summary>
    public sealed class NoUnits : PhysicalQuantity<NoUnits>
    {
        /// <inheritdoc />
        public NoUnits(decimal value) : base(value)
        {
            Units = new[] { MeasureUnits.NONE,MeasureUnits.dB};
            Unit = MeasureUnits.NONE;
        }

        /// <inheritdoc />
        public NoUnits(decimal value, UnitMultiplier multiplier) : base(value, multiplier)
        {
            Units = new[] { MeasureUnits.NONE, MeasureUnits.dB };
            Unit = MeasureUnits.NONE;
        }

        public NoUnits()
        {
            Units = new[] { MeasureUnits.NONE, MeasureUnits.dB };
            Unit = MeasureUnits.NONE;
        }
    }

    /// <summary>
    /// Реализует безразмерную физическую величину (без единиц измерения).
    /// </summary>
    public sealed class Degreas : PhysicalQuantity<Degreas>
    {
        /// <inheritdoc />
        public Degreas(decimal value) : base(value)
        {
            Units = new[] {MeasureUnits.degreas};
            Unit = MeasureUnits.degreas;
        }

        /// <inheritdoc />
        public Degreas(decimal value, UnitMultiplier multiplier) : base(value, multiplier)
        {
            Units = new[] { MeasureUnits.degreas };
            Unit = MeasureUnits.degreas;
        }

        public Degreas()
        {
            Units = new[] { MeasureUnits.degreas };
            Unit = MeasureUnits.degreas;
        }
    }

    /// <summary>
    /// Реализует физическую величину длинны.
    /// </summary>
    public sealed class Length : PhysicalQuantity<Length>
    {
        /// <inheritdoc />
        public Length(decimal value) : base(value)
        {
            Units = new[] { MeasureUnits.Length };
            Unit = MeasureUnits.Length;
        }

        /// <inheritdoc />
        public Length(decimal value, UnitMultiplier multiplier) : base(value, multiplier)
        {
            Units = new[] { MeasureUnits.Length };
            Unit = MeasureUnits.Length;
        }

        public Length()
        {
            Units = new[] { MeasureUnits.Length };
            Unit = MeasureUnits.Length;
        }
    }
    /// <summary>
    /// Реализует физическую величину частоты.
    /// </summary>
    public sealed class Frequency : PhysicalQuantity<Frequency>
    {
        public Frequency(decimal value) : base(value)
        {
            Units = new[] { MeasureUnits.Frequency };
            Unit = MeasureUnits.Frequency;
        }

        public Frequency(decimal value, UnitMultiplier multiplier) : base(value, multiplier)
        {
            Units = new[] { MeasureUnits.Frequency };
            Unit = MeasureUnits.Frequency;
        }

        public Frequency()
        {
            Units = new[] { MeasureUnits.Frequency };
            Unit = MeasureUnits.Frequency;
        }
    }

    /// <summary>
    /// Реализует физическую величину сопротивления.
    /// </summary>
    public sealed class Resistance : PhysicalQuantity<Resistance>
    {
        public Resistance()
        {
            Units = new[] { MeasureUnits.Resistance };
            Unit = MeasureUnits.Resistance;
        }

        public Resistance(decimal value): base(value)
        {
            Units = new[] { MeasureUnits.Resistance };
            Unit = MeasureUnits.Resistance;
        }

        public Resistance(decimal value, UnitMultiplier multiplier) : base(value, multiplier)
        {
            Units = new[] { MeasureUnits.Resistance };
            Unit = MeasureUnits.Resistance;
        }
    }


    /// <summary>
    /// Реализует физическую величину времени.
    /// </summary>
    public sealed class Time : PhysicalQuantity<Time>
    {
        public Time()
        {
            Units = new[] { MeasureUnits.Time };
            Unit = MeasureUnits.Time;
        }

        public Time(decimal value) : base(value)
        {
            Units = new[] { MeasureUnits.Time };
            Unit = MeasureUnits.Time;
        }

        public Time(decimal value, UnitMultiplier multiplier) : base(value, multiplier)
        {
            Units = new[] { MeasureUnits.Time };
            Unit = MeasureUnits.Time;
        }
    }
    /// <summary>
    /// Реализует физическую величину электрического напряжения.
    /// </summary>
    public sealed class Voltage : PhysicalQuantity<Voltage>
    {
        public Voltage(decimal value) : base(value)
        {
            Units = new[] { MeasureUnits.V };
            Unit = MeasureUnits.V;
        }

        public Voltage(decimal value, UnitMultiplier multiplier) : base(value, multiplier)
        {
            Units = new[] { MeasureUnits.V };
            Unit = MeasureUnits.V;
        }

        public Voltage()
        {
            Units = new[] { MeasureUnits.V };
            Unit = MeasureUnits.V;
        }
    }
    /// <summary>
    /// Реализует физическую величину электрического тока.
    /// </summary>
    public sealed class Current : PhysicalQuantity<Current>
    {
        public Current()
        {
            Units = new[] { MeasureUnits.I };
            Unit = MeasureUnits.I;
        }

        public Current(decimal value, UnitMultiplier multiplier) : base(value, multiplier)
        {
            Units = new[] { MeasureUnits.I };
            Unit = MeasureUnits.I;
        }

        public Current(decimal value) : base(value)
        {
            Units = new[] { MeasureUnits.I };
            Unit = MeasureUnits.I;
        }
    }

    /// <summary>
    /// Реализует физическую величину электрического ёмкости.
    /// </summary>
    public sealed class Capacity : PhysicalQuantity<Capacity>
    {
        public Capacity()
        {
            Units = new[] { MeasureUnits.Far };
            Unit = MeasureUnits.Far;
        }

        public Capacity(decimal value, UnitMultiplier multiplier) : base(value, multiplier)
        {
            Units = new[] { MeasureUnits.Far };
            Unit = MeasureUnits.Far;
        }

        public Capacity(decimal value) : base(value)
        {
            Units = new[] { MeasureUnits.Far };
            Unit = MeasureUnits.Far;
        }
    }

    /// <summary>
    /// Реализует физическую величину градусы Цельсия.
    /// </summary>
    public sealed class Temperature : PhysicalQuantity<Temperature>
    {
        public Temperature()
        {
            Units = new[] { MeasureUnits.degC };
            Unit = MeasureUnits.degC;
        }

        public Temperature(decimal value, UnitMultiplier multiplier) : base(value, multiplier)
        {
            Units = new[] { MeasureUnits.degC, MeasureUnits.DegF };
            Unit = MeasureUnits.degC;
        }

        public Temperature(decimal value) : base(value)
        {
            Units = new[] { MeasureUnits.degC };
            Unit = MeasureUnits.degC;
        }
    }

    public sealed class Percent : PhysicalQuantity<Percent>
    {
        public Percent()
        {
            Units = new[] {MeasureUnits.Percent};
            Unit = MeasureUnits.Percent;
        }

        public Percent(decimal value,UnitMultiplier multiplier) : base(value, multiplier)
        {
            Units = new[] { MeasureUnits.Percent };
            Unit = MeasureUnits.Percent;
        }

        public Percent(decimal value): base(value)
        {
            Units = new[] { MeasureUnits.Percent };
            Unit = MeasureUnits.Percent;
        }
    }





}
