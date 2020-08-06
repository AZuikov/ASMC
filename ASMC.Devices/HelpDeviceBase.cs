﻿using System;
using System.Globalization;
using System.Linq;
using AP.Utils.Data;
using MathNet.Numerics.Statistics;

namespace ASMC.Devices
{
    public abstract class HelpDeviceBase
    {
        public ICommand[] Multipliers
        {
            get; protected set;
        }
        /// <summary>
        /// Преобразут данные в нужных единицах
        /// </summary>
        /// <param name = "date">The date.</param>
        /// <param name = "mult">The mult.</param>
        /// <returns></returns>
        public double DataStrToDoubleMind(string date, Multipliers mult = Devices.Multipliers.None)
        {

            var value = date.Split(',');
            var a = new double[value.Length];
            for(var i = 0; i < value.Length; i++)
                a[i] = StrToDoubleMindMind(value[i], GetMultiplier(mult));
            return a.Mean() < 0 ? a.RootMeanSquare() * -1 : a.RootMeanSquare();
        }

        protected Multipliers GetMultiplier(ICommand mult)
        {
            var res = Enum.GetValues(typeof(Multipliers)).Cast<Multipliers>()
                          .FirstOrDefault(q => Equals(q.GetDoubleValue(), mult.Value));
            return res;
        }
        protected ICommand GetMultiplier(Multipliers mult)
        {
            var res = Multipliers.FirstOrDefault(q => Equals(q.Value, mult.GetDoubleValue()));
            if(res == null)
                throw new ArgumentOutOfRangeException($@"Даппозон {mult} не найден.");
            return res;
        }

        private static double StrToDoubleMindMind(string date, ICommand mult = null)
        {
            var dDate = new double[2];
            var value = date.Replace(".", ",").Split('E');
            dDate[0] = Convert.ToDouble(value[0]);
            dDate[1] = Convert.ToDouble(value[1]);
            return math(dDate[0], dDate[1], mult);
            // ReSharper disable once InconsistentNaming
            double math(double val, double exponent, ICommand m)
            {
                return val * Math.Pow(10, exponent) / m?.Value ?? 1.0;
            }
        }
        public string JoinValueMult(double value, Multipliers mult)
        {
            return JoinValueMult((decimal)value, mult);
        }
        public string JoinValueMult(decimal value, Multipliers mult)
        {
            return value.ToString("G", CultureInfo.GetCultureInfo("en-US")) +
                   GetMultiplier(mult).StrCommand;
        }
    }
}