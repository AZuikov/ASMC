// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using MathNet.Numerics.Statistics;

namespace MathStatistick
{
    /// <summary>
    /// Реализует матиматическую обработку по ГОСТ 8.417 - по единицам величин, ГОСТ Р 8.736-2011, СТ СЭВ 543-77 - по округлению
    /// </summary>
    public static class MathStatistic
    {
        /// <summary>
        /// Округляет входное значение до знака соответствующему образовому
        /// </summary>
        /// <param name="value">Значение подлежащее округению</param>
        /// <param name="referens">Знаечние в соответствии с которым необходимо округлить</param>
        /// <returns>Округленное значение, с добовлением 0 при необходимости</returns>
        public static string RoundingReference(double value, double referens)
        {
            var returnValue = "";

            var length = MantissaLength(referens);

            var _value = Round(value, length);
            var mantissaValue = MantissaLength(_value);
            if (mantissaValue == 0)
            {
                if (length > 0)
                {
                    returnValue = _value.ToString("0.#######################", new CultureInfo("en-US")) + ".0";
                    mantissaValue = 1;
                }
                else
                {
                    mantissaValue = length;
                    returnValue = _value.ToString("0.#######################", new CultureInfo("en-US"));
                }
            }
            else
            {
                returnValue = _value.ToString("0.#######################", new CultureInfo("en-US"));
            }
            for (int i = 0; i < length - mantissaValue; i++)
            {
                returnValue = returnValue + "0";
            }
            return returnValue;
        }
        /// <summary>
        /// Округляет входное значение до знака соответствующему образовому
        /// </summary>
        /// <param name="value">Значение подлежащее округению</param>
        /// <param name="referens">Знаечние в соответствии с которым необходимо округлить</param>
        /// <returns>Округленное значение, с добовлением 0 при необходимости</returns>
        public static double RoundingReferenceDoble(double value, double referens)
        {            
            double _value;
            int Length;

            Length = MantissaLength(referens);

            _value = Round(value, Length);
           
            return _value;
        }
        /// <summary>
        /// Определяеть длинну мантиссы
        /// </summary>
        /// <param name="value">Входное значение</param>
        /// <returns>длинна мантиссы</returns>
        public static int MantissaLength(double value)
        {
            string[] array;
            array = value.ToString("0.#######################", new CultureInfo("en-US")).Split('.');
            try
            {
                return array[1].Length;
            }
            catch (Exception)
            {
                return 0;
            }

        }
        /// <summary>
        /// Определяеть длинну мантиссы в строке
        /// </summary>
        /// <param name="value">Входное значение</param>
        /// <returns>длинна мантиссы</returns>
        public static int MantissaLength(string value)
        {
            string[] array;
            array = value.Split('.');
            try
            {
                return array[1].Length;
            }
            catch (Exception)
            {
                return 0;
            }

        }
        /// <summary>
        /// Округление значения в соответствии с СЭВ СТ СЭВ 543-77
        /// </summary>
        /// <param name="value">Округляемое значение</param>
        /// <param name="decimals">Знаков после зяпятой</param>
        /// <returns>Округлеееное значение</returns>
        public static double Round(Double value, int decimals)
        {
            var Local = Thread.CurrentThread.CurrentCulture;
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
            double _value;
            string str;
            int point;
            List<char> array = new List<char>();
            if (MantissaLength(value) <= decimals)
            {
                return value;
            }
            if (value > 0)
            {
                _value = value;
            }
            else
            {
                _value = value * -1;
            }
            str = _value.ToString("0.#######################", new CultureInfo("en-US"));
            point = str.IndexOf('.');

            array.AddRange(str.ToCharArray());
            if (Convert.ToInt32(array[point + 1 + decimals].ToString()) < 5)
            {
                if (decimals == 0)
                {
                    decimals--;
                }
                array.RemoveRange(point + decimals + 1, array.Count - decimals - point - 1);
            }
            else
            {
                int test, test1;
                test = point + decimals;
                try
                {
                    while(array[test] == '9')
                    {
                        array[test] = '0';
                        test--;
                    }
                    test1 = Convert.ToInt32(array[test].ToString()) + 1;
                }
                catch (Exception)
                {
                    test--;
                    do
                    {
                        if (array[test].ToString() == "9")
                        {
                            array[test] = '0';
                            try
                            {
                                test1 = Convert.ToInt32(array[test - 1].ToString()) + 1;
                                test--;
                            }
                            catch (Exception)
                            {
                                array.Insert(0, '1');
                                test1 = Convert.ToInt32(array[test].ToString());
                            }

                        }
                        else
                        {
                            test1 = Convert.ToInt32(array[test].ToString()) + 1;
                            decimals--;
                        }
                    } while (array[test].ToString() == "9");
                }

                char cha = Convert.ToChar(test1.ToString());
                array[test] = cha;
                array.RemoveRange(point + decimals + 1, array.Count - decimals - point - 1);
            }

            if (value < 0)
            {
                _value = Convert.ToDouble(new string(array.ToArray())) * -1;
            }
            else
            {
                _value = Convert.ToDouble(new string(array.ToArray()));
            }

            Thread.CurrentThread.CurrentCulture = Local;
            return _value;
        }
        /// <summary>
        /// Исключает грубые погрешности при доверительной вероятности 95%. Можно использовать если измерений больше 2, в другом случае не имеет смысла 
        /// </summary>
        /// <param name="array">Массив с измереными значениями</param>
        /// <returns>Срезнеквадратичное значение</returns>
        public static double ManyExclusionGrossError(List<double> array)
        {
            const int LengthArray = 34;
            int count, InCount;
            List<double> _array;
            _array = new List<double>();
            count = array.Count;
            InCount = 0;

            count = count / LengthArray;
            for (int i = 0; i < count; i++)
            {
                _array.Add(ExclusionGrossError(array.GetRange(InCount, LengthArray)));
                InCount = InCount + LengthArray;
            }
            _array.Add(ExclusionGrossError(array.GetRange(count * LengthArray, array.Count - count * LengthArray)));
            if (_array.Count > 1)
            {
                return ManyExclusionGrossError(_array);
            }

            return _array[0];
        }
        /// <summary>
        /// Исключает грубые погрешности при доверительной вероятности 95%. Можн использщовать если измерений больше 2 и меньше 35
        /// </summary>
        /// <param name="array">Массив с измереными значениями</param>
        /// <returns>Срезнеквадратичное значение</returns>
        private static double ExclusionGrossError(List<double> array)
        {
            double max, min, Mean, Devia, Greb;
            bool error;
            do
            {
                error = false;
                max = array.Max();
                min = array.Min();
                Mean = array.Mean();
                Devia = array.StandardDeviation();
                double emax = Math.Abs(max - Mean) / Devia;
                double emin = Math.Abs(min - Mean) / Devia;
                Greb = Grabbs(array.Count);
                if (emax > Greb)
                {
                    error = true;
                    array.RemoveAt(array.IndexOf(max));
                }
                if (emin > Greb)
                {
                    error = true;
                    array.RemoveAt(array.IndexOf(min));
                }
            } while (Greb != 0 && error);
            return array.Mean() < 0 ? array.RootMeanSquare() * -1 : array.RootMeanSquare();
            
        }
        /// <summary>
        /// Мтод возвращающий значение критерия Граббса для указаного количества измерений. 
        /// </summary>
        /// <param name="n">Количество измерений. Допускаеться значие больше 2 и меньше 35</param>
        /// <returns>Критерий Граббса</returns>
        private static double Grabbs(int n)
        {
            double value;
            if (n < 2 && n < 35)
            {
                return 0;
            }
            switch (n)
            {
                case 3:
                    value = 1.555;
                    break;
                case 4:
                    value = 1.481;
                    break;
                case 5:
                    value = 1.715;
                    break;
                case 6:
                    value = 1.887;
                    break;
                case 7:
                    value = 2.02;
                    break;
                case 8:
                    value = 2.126;
                    break;
                case 9:
                    value = 2.215;
                    break;
                case 10:
                    value = 2.290;
                    break;
                case 11:
                    value = 2.355;
                    break;
                case 12:
                    value = 2.412;
                    break;
                case 13:
                    value = 2.462;
                    break;
                case 14:
                    value = 2.507;
                    break;
                case 15:
                    value = 2.549;
                    break;
                case 16:
                    value = 2.585;
                    break;
                case 17:
                    value = 2.620;
                    break;
                case 18:
                    value = 2.651;
                    break;
                case 19:
                    value = 2.681;
                    break;
                case 20:
                    value = 2.709;
                    break;
                case 21:
                    value = 2.733;
                    break;
                case 22:
                    value = 2.758;
                    break;
                case 23:
                    value = 2.781;
                    break;
                case 24:
                    value = 2.802;
                    break;
                case 25:
                    value = 2.822;
                    break;
                case 26:
                    value = 2.841;
                    break;
                case 27:
                    value = 2.859;
                    break;
                case 28:
                    value = 2.876;
                    break;
                case 29:
                    value = 2.893;
                    break;
                case 30:
                    value = 2.908;
                    break;
                case 31:
                    value = 2.924;
                    break;
                case 32:
                    value = 2.938;
                    break;
                case 33:
                    value = 2.952;
                    break;
                case 34:
                    value = 2.965;
                    break;
                default:
                    value = 0;
                    break;
            }
            return value;
        }
        /// <summary>
        /// Максимальное абсолютное значений
        /// </summary>
        /// <param name="array">>Массив значений</param>
        /// <returns>Максимальное значение</returns>
        public static double MaxListAbs(List<double> array)
        {
            List<double> Array = array;
            for (int i = 0; i < Array.Count; i++)
            {
                if (Array[i] < 0)
                {
                    Array[i] = Array[i] * -1;
                }
            }
            return Array.Max();
        }
        /// <summary>
        /// Минимальное абсолютное значений
        /// </summary>
        /// <param name="array">Массив значений</param>
        /// <returns>Минимальное занчение</returns>
        public static double MinListAbs(List<double> array)
        {
            List<double> Array = array;
            for (int i = 0; i < Array.Count; i++)
            {
                if (Array[i] < 0)
                {
                    Array[i] = Array[i] * -1;
                }
            }
            return Array.Min();
        }
        /// <summary>
        /// Случайное знаечние в диапазоне
        /// </summary>
        /// <param name="from">От</param>
        /// <param name="to">До</param>
        /// <returns>Случайно знаечние</returns>
        public static double RandomToRange(double from, double to)
        {
            Random rand = new Random((int)Stopwatch.GetTimestamp());
            if (from < 0&& to>0)
            {
                var positive = rand.NextDouble() * (to - 0) + 0;
                var negative = (rand.NextDouble() * (0 - from * -1) + from * -1) * -1;
                return rand.Next(1) == 1 ? positive : negative;
            }

            if(from < 0 && to < 0)
            {               
                return (rand.NextDouble() * (Math.Abs(from)  - Math.Abs(to)) + Math.Abs(to))*-1;
            }

            return rand.NextDouble() * (to - from) + from;


        }
        public static double ValueToRange(double StartRenge, double EndRange, double min, double max, double value)
        {
          return  (EndRange-StartRenge)/(max- min) *value + ((StartRenge * max - EndRange * min) / (max - min));
        }
    }
}
