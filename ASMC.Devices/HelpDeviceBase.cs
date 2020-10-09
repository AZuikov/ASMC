using System;
using System.Globalization;
using System.Linq;
using AP.Utils.Data;
using AP.Utils.Helps;
using ASMC.Data.Model;
using MathNet.Numerics.Statistics;

namespace ASMC.Devices
{
    public abstract class HelpDeviceBase
    {
        private ICommand[] _multipliers;

        public ICommand[] Multipliers
        {
            get { 
                if(_multipliers==null) throw  new  NullReferenceException("Multipliers не инициализирован");
                return _multipliers;

            }
            protected set =>  _multipliers = value;
        }

        /// <summary>
        /// Преобразут строку в double, может принимать строку с перечисленными значениями через запятую.
        /// </summary>
        /// <param name = "date">Одно значение или перечисление значений через запятую. Разделитель целой и дробной части точка.</param>
        /// <param name = "mult">Множитель единицы измерения, в которую нужно преобразовать входные данные (милли, кило и т.д.). </param>
        /// <returns></returns>
        public double DataStrToDoubleMind(string date, UnitMultipliers mult = AP.Utils.Helps.UnitMultipliers.None)
        {
            
            var value = date.Split(',');
            var a = new double[value.Length];
            for(var i = 0; i < value.Length; i++)
                a[i] = StrToDoubleMindMind(value[i], GetMultiplier(mult));
            return a.Mean() < 0 ? a.RootMeanSquare() * -1 : a.RootMeanSquare();
        }

        /// <summary>
        /// Принимает число inDouble и переводит его согласно множителю едииц mult.
        /// </summary>
        /// <param name="inDouble">Числовое значение для перевода</param>
        /// <param name="mult">множитель единицы измерения (милли, кило и т.д.).</param>
        /// <returns></returns>
        public double DoubleToDoubleMind(double inDouble, UnitMultipliers mult = AP.Utils.Helps.UnitMultipliers.None)
        {
            return mult == AP.Utils.Helps.UnitMultipliers.None ? inDouble : inDouble / mult.GetDoubleValue();
        }

       

        /// <summary>
        /// Возвращает множитель единицы измерения (милли, кило и т.д.).
        /// </summary>
        /// <param name="mult"></param>
        /// <returns></returns>
        protected UnitMultipliers GetMultiplier(ICommand mult)
        {
            var res = Enum.GetValues(typeof(UnitMultipliers)).Cast<UnitMultipliers>()
                          .FirstOrDefault(q => Equals(q.GetDoubleValue(), mult.Value));
            return res;
        }
        protected ICommand GetMultiplier(UnitMultipliers mult)
        {
            var res = Multipliers.FirstOrDefault(q => Equals(q.Value, mult.GetDoubleValue()));
            if(res == null)
                throw new ArgumentOutOfRangeException($@"Даппозон {mult} не найден.");
            return res;

        }


        /// <summary>
        /// Преобразует строкове значение в double. Принимает так же числа в виде "2.345E-5".
        /// </summary>
        /// <param name="date">Число для преобразования в виде строки.</param>
        /// <param name="mult">Множитель единицы измерения (милли, кило и т.д.).</param>
        /// <returns></returns>
        public static double StrToDoubleMindMind(string date, ICommand mult = null)
        {
            var dDate = new double[2];
            var value = date.Replace(".", ",").Split('E');
            dDate[0] = Convert.ToDouble(value[0]);
            dDate[1] = Convert.ToDouble(value[1]);
            return math(dDate[0], dDate[1], mult);
            // ReSharper disable once InconsistentNaming
            double math(double val, double exponent, ICommand m)
            {
                if (exponent == 0)   return val  /( m?.Value ?? 1.0);
                return val * Math.Pow(10, exponent) /( m?.Value ?? 1.0);
            }
        }

        public string JoinValueMult(double value, UnitMultipliers mult)
        {
            return JoinValueMult((decimal)value, mult);
        }
        /// <summary>
        /// Преобразует числовое значени в строку с указанными единицами измерения.
        /// </summary>
        /// <param name="value">Числовое значение которео нужно преобразовать.</param>
        /// <param name="mult"> Множитель единицы измерения (млии, кило и т.д.).</param>
        /// <returns></returns>
        public string JoinValueMult(decimal value, UnitMultipliers mult)
        {
            return value.ToString("G", CultureInfo.GetCultureInfo("en-US")) +
                   GetMultiplier(mult).StrCommand;
        }
    }
}