﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AP.Utils.Helps;

namespace ASMC.Data.Model.PhysicalQuantity
{

    /// <summary>
    /// Реализует физическую величину давление
    /// </summary>
    public sealed class Pressure : PhysicalQuantity<Pressure>, IConvertPhysicalQuantity<Pressure>
    {
        public Pressure()
        {
            Units = new[] { MeasureUnits.Pressure, MeasureUnits.MercuryPressure };
        }

        public Pressure Convert(MeasureUnits u)
        {
            if (CheckedAttachmentUnits(u))
                throw new InvalidCastException("Нвозможно преобразовать в другой тип физической величины");
            if (Unit == u) return this;
          var pq=  ThisConvetToSi();
            switch (u)
            {
                case MeasureUnits.MercuryPressure:
                    return new Pressure { Unit = u, Value = decimal.Parse(pq.Value.ToString()) / (decimal)0.0000075018754688672 };
                case MeasureUnits.Pressure:
                    return new Pressure { Unit = u, Value = decimal.Parse(pq.Value.ToString()) * (decimal)0.0000075018754688672 };
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
        public Force()
        {
            Units = new[] { MeasureUnits.N };
            Unit = MeasureUnits.N;
        }
    }
    /// <summary>
    /// Реализует физическую величину длинны.
    /// </summary>
    public sealed class Length : PhysicalQuantity<Length>
    {
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
        public Frequency()
        {
            Units = new[] { MeasureUnits.Frequency };
            Unit = MeasureUnits.Frequency;
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
    }
    /// <summary>
    /// Реализует физическую величину электрического напряжения.
    /// </summary>
    public sealed class Voltage : PhysicalQuantity<Voltage>
    {
        public Voltage()
        {
            Units = new[] { MeasureUnits.V };
            Unit = MeasureUnits.V;
        }
    }

    public sealed class Amper : PhysicalQuantity<Amper>
    {
        public Amper()
        {
            Units = new[] { MeasureUnits.I };
            Unit = MeasureUnits.I;
        }
    }

}
