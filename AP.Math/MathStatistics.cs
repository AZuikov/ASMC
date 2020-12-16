using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using MathNet.Numerics.Random;

namespace AP.Math
{
    public static class MathStatistics
    {
        /// <summary>
        /// Случайное знаечние в диапазоне
        /// </summary>
        /// <param name="from">От</param>
        /// <param name="to">До</param>
        /// <returns>Случайно знаечние</returns>
        public static decimal RandomToRange(decimal from, decimal to)
        {
            if (from >= to) throw new ArgumentOutOfRangeException($"Значение аргумента from({from}) больше значения to({to}).");
            Random rand = new Random((int)Stopwatch.GetTimestamp());
            if (from < 0 && to > 0)
            {
                var positive = rand.NextDecimal() * (to - 0) + 0;
                var negative = (rand.NextDecimal() * (0 - from * -1) + from * -1) * -1;
                return rand.Next(1) == 1 ? positive : negative;
            }

            if (from < 0 && to < 0)
            {
                return (rand.NextDecimal() * (System.Math.Abs(from) - System.Math.Abs(to)) + System.Math.Abs(to)) * -1;
            }

            return rand.NextDecimal() * (to - from) + from;


        }


        public static double ValueToRange(double StartRange, double EndRange, double min, double max, double value)
        {
            return (EndRange - StartRange) / (max - min) * value +
                   ((StartRange * max - EndRange * min) / (max - min));
        }

        public enum GrubbsLevel
        {
            MoreThen1Persent,
            MoreThen5Persent
        }

        private static double[] _grubbsValuesFor1Persent;
        private static double[] _grubbsValuesFor5Persent;

        /// <summary>
        ///     Возвращает среднее арифметическое
        /// </summary>
        /// <param name="values">Перечисление значений</param>
        /// <returns></returns>
        public static decimal GetArithmeticalMean(decimal[] values)
        {
            if (!values.Any())
                return 0; //Избегаем деления на 0
            decimal sum = 0;
            foreach (var value in values) sum += value / values.Length;

            return sum;
        }

        /// <summary>
        ///     Возвращает критическое значение для критерия Граббса по ГОСТ Р8.736-2011
        /// </summary>
        /// <param name="n">Количество измерений</param>
        /// <param name="grubbsLevel">Уровень значимости</param>
        /// <returns></returns>
        public static double GetCriticalGrubbsValue(int n, GrubbsLevel grubbsLevel)
        {
            if (n < 3) throw new ArgumentException();

            if (n > 40)
                n = 40;
            switch (grubbsLevel)
            {
                case GrubbsLevel.MoreThen1Persent:
                    if (_grubbsValuesFor1Persent == null)
                        _grubbsValuesFor1Persent = new[]
                        {
                            1.155, 1.496, 1.764, 1.973, 2.139, 2.274, 2.387, 2.482, 2.564, 2.636,
                            2.699, 2.755, 2.806, 2.852, 2.894, 2.932, 2.968, 3.001, 3.031, 3.060,
                            3.087, 3.112, 3.135, 3.157, 3.178, 3.199, 3.218, 3.236, 3.253, 3.270,
                            3.286, 3.301, 3.301, 3.330, 3.330, 3.356, 3.356, 3.381
                        };

                    return _grubbsValuesFor1Persent[n - 3];
                case GrubbsLevel.MoreThen5Persent:
                    if (_grubbsValuesFor5Persent == null)
                        _grubbsValuesFor5Persent = new[]
                        {
                            1.155, 1.481, 1.715, 1.887, 2.020, 2.126, 2.215, 2.290, 2.355, 2.412,
                            2.462, 2.507, 2.549, 2.585, 2.620, 2.651, 2.681, 2.709, 2.733, 2.758,
                            2.781, 2.802, 2.822, 2.841, 2.859, 2.876, 2.893, 2.908, 2.924, 2.938,
                            2.952, 2.965, 2.965, 2.991, 2.991, 3.014, 3.014, 3.036
                        };

                    return _grubbsValuesFor5Persent[n - 3];
            }   
            return 0;
        }


    


        /// <summary>
        /// Возвращает среднеквадратичное отклонение.
        /// </summary>
        /// <param name="values">Массив значений для обраотки.</param>
        /// <returns>Возвращает расчитанной значение СКО.</returns>
        public static decimal GetStandartDeviation(decimal[] values)
        {
            if (!values.Any())
                return 0; //Избегаем деления на 0
            decimal sum = 0;
            var arithmeticalMean = GetArithmeticalMean(values);
            
            foreach (var value in values)  sum += (value - arithmeticalMean) * (value - arithmeticalMean) / values.Length;

            return (decimal)System.Math.Sqrt((double)sum);
        }

        /// <summary>
        ///     Исключает грубые погрешности используя критерий Граббса. Если исключение невозможно - возврящает null
        /// </summary>
        /// <param name="values">Массив измерений</param>
        /// <param name="grubbsLevel">Уровень значимости</param>
        /// <returns>Был ли массив изменён?</returns>
        public static bool Grubbs(ref decimal[] values, GrubbsLevel grubbsLevel = GrubbsLevel.MoreThen1Persent)
        {
            var valueList = new List<decimal>(values);
            var inProcess = true;
            var numberOfDeleted = 0;

            while (inProcess)
            {
                if (valueList.Count < 3)
                    break;
                inProcess = false;
                var stdDeviation = GetStandartDeviation(valueList.ToArray());
                if (stdDeviation == 0)
                    break; //Escape dividing by zero
                //Grubbs for max value
                var grubbs1 = System.Math.Abs(valueList.Max() - GetArithmeticalMean(valueList.ToArray())) /
                              stdDeviation;
                if (grubbs1 > (decimal) GetCriticalGrubbsValue(valueList.Count, grubbsLevel))
                {
                    inProcess = true;
                    valueList.Remove(valueList.Max());
                    numberOfDeleted++;
                }

                //Grubbs for min value
                var grubbs2 = System.Math.Abs(valueList.Min() - GetArithmeticalMean(valueList.ToArray())) /
                              stdDeviation;
                if (grubbs2 <= (decimal) GetCriticalGrubbsValue(valueList.Count + (inProcess ? 1 : 0), grubbsLevel))
                    continue;
                inProcess = true;
                valueList.Remove(valueList.Min());
                numberOfDeleted++;
            }

            //values = valueList.Count < 3 ? null : valueList.ToArray();
            values =  valueList.ToArray();
            return numberOfDeleted > 0;
        }

        /// <summary>
        ///     Входят ли все значения из массива в диапазон 6 сигм
        /// </summary>
        /// <param name="values">Перечисление значений</param>
        /// <returns></returns>
        public static bool IntoTreeSigma(decimal[] values)
        {
            var arithmeticalMean = GetArithmeticalMean(values);
            var standartDeviation = GetStandartDeviation(values);
            var low = arithmeticalMean - 3 * standartDeviation;
            var high = arithmeticalMean + 3 * standartDeviation;
            var result = true;
            foreach (var val in values)
                if (val > high || val < low)
                    result = false;

            return result;
        }

        /// <summary>
        ///     Определяет, лежит ли точка в окрестностях графика [-min, +max]
        /// </summary>
        /// <param name="graph">График, заданный координатами Y точек</param>
        /// <param name="pointY">Координата Y точки</param>
        /// <param name="pointX">Координата X точки</param>
        /// <param name="min">Допустимое отрицательное смещение вниз от графика</param>
        /// <param name="max">Допустимое положительное смещение вверх от графика</param>
        /// <returns></returns>
        public static bool IsPointNearGraph(decimal[] graph, decimal pointY, int pointX, decimal min, decimal max)
        {
            if (min < 0 || max > 0) throw new ArgumentException("min and max parametrs must be more than zero");

            if (graph.Length - 1 < pointX) throw new ArgumentException("pointX must be less then graph max index");

            if (pointX < 0) throw new ArgumentException("pointX must be more then zero");

            if (pointY > graph[pointX] + max || pointY < graph[pointX] - min)
                return false;
            return true;
        }

        /// <summary>
        ///     Возвращает true если последние 2/3 измерений показывают одно и то же значение +/- погрешность измерений прибора
        /// </summary>
        /// <param name="signal">Измеренные значения</param>
        /// <param name="error">Погрешность измерений прибора</param>
        /// <returns></returns>
        public static bool IsSignalFades(decimal[] signal, decimal error)
        {
            var twoThird = signal.Length / 3;
            var lastTwoThirds = new decimal[signal.Length - twoThird - 1];
            for (var i = lastTwoThirds.Length - 1; i >= 0; i--) lastTwoThirds[i] = signal[i + twoThird];

            var arithmeticalMean = GetArithmeticalMean(lastTwoThirds);

            var result = true;
            for (var i = twoThird; i < signal.Length; i++)
                if (signal[i] > arithmeticalMean + error || signal[i] < arithmeticalMean - error)
                    result = false;

            return result;
        }

        /// <summary>
        ///     Округляет до number знаков после запятой
        /// </summary>
        /// <param name="value">Округляемое</param>
        /// <param name="reference">Опорное значение</param>
        public static string Round(decimal value, string reference)
        {
           return Round( value, GetMantissa(reference));
        }

        /// <summary>
        ///     Округляет до number знаков после запятой
        /// </summary>
        /// <param name="value">Округляемое</param>
        /// <param name="reference">Опорное значение</param>
        public static string Round(double value, string reference)
        {
            return Round( value, GetMantissa(reference));
        }
        /// <summary>
        ///     Возврящает количество знаков после запятой
        /// </summary>
        /// <param name="value">Число</param>
        /// <param name="reduceZeros">Отбросить незначимые нули</param>
        /// <returns></returns>
        public static int GetMantissa<T>(T value, bool reduceZeros = false)
        {
            return GetMantissa(Convert.ToString(value, CultureInfo.CurrentCulture), CultureInfo.CurrentCulture, reduceZeros);
        }

        /// <summary>
        ///     Возврящает количество знаков после запятой
        /// </summary>
        /// <param name="value">Число</param>
        /// <param name="cultureInfo"></param>
        /// <param name="reduceZeros">Отбросить незначимые нули</param>
        /// <returns></returns>
        public static int GetMantissa<T>(T value, CultureInfo cultureInfo, bool reduceZeros = false)
        {
            return GetMantissa(Convert.ToString(value, cultureInfo), cultureInfo, reduceZeros);
        }
        /// <summary>
        ///     Возврящает количество знаков после запятой
        /// </summary>
        /// <param name="value">Число</param>
        /// <param name="cultureInfo"></param>
        /// <param name="reduceZeros">Отбросить незначимые нули</param>
        /// <returns></returns>
        private static int GetMantissa(string value, CultureInfo cultureInfo, bool reduceZeros = false)
        {
            if (string.IsNullOrEmpty(value)) return 0;
            var numeric = 0m;
            if (!decimal.TryParse(value, NumberStyles.Any, cultureInfo, out numeric))
                throw new InvalidCastException();
            var splitted = numeric.ToString(cultureInfo).Split(Convert.ToChar(cultureInfo.NumberFormat.CurrencyDecimalSeparator));
            if (splitted.Length != 2)
                return 0;
            if (!reduceZeros) return splitted[1].Length;

            int i;
            for (i = splitted[1].Length - 1; i >= 0; i--)
                if (splitted[1][i] != '0')
                    break; 
            return i + 1;
        }


        /// <summary>
        ///     Округляет до number знаков после запятой
        /// </summary>
        /// <param name="value">Округляемое</param>
        /// <param name="number">Число знаков</param>
        /// <param name="reduceZeros">Отбросить незначимые нули</param>
        public static string Round(double value, int number, bool reduceZeros = false)
        {
            value = System.Math.Round(value, number, MidpointRounding.AwayFromZero);
            return reduceZeros ? value.ToString(@"G17", CultureInfo.CurrentCulture) : AddInsignificantZeros(value, number);
        }

        /// <summary>
        ///     Округляет до number знаков после запятой
        /// </summary>
        /// <param name="value">Округляемое</param>
        /// <param name="number">Число знаков</param>
        /// <param name="reduceZeros">Отбросить незначимые нули</param>
        public static string Round( decimal value, int number, bool reduceZeros = false)
        {
            value = System.Math.Round(value, number, MidpointRounding.AwayFromZero);
            return reduceZeros? value.ToString(@"G17", CultureInfo.CurrentCulture) : AddInsignificantZeros(value, number);
        }
        /// <summary>
        /// Добавляет незначищие нули
        /// </summary>
        /// <param name="value">Численное знаечние</param>
        /// <param name="number"></param>
        /// <returns></returns>
        private static string AddInsignificantZeros(object value, int number)
        {
            var val = Convert.ToDecimal(value);
            var mantissa = GetMantissa(val, CultureInfo.InvariantCulture);
            var result = new StringBuilder(val.ToString(@"G17", CultureInfo.CurrentCulture));
            while(mantissa < number)
            {
                if(mantissa == 0)
                {
                    result.Append(NumberFormatInfo.CurrentInfo.CurrencyDecimalSeparator);
                }
                result.Append('0');
                mantissa = GetMantissa(result.ToString(), CultureInfo.CurrentCulture);
            }
            return result.ToString();
        }
        /// <summary>
        /// Производит масштабирование значения
        /// </summary>
        /// <param name="input">Входное занчение</param>
        /// <param name="minInput">Минимальное входное значение </param>
        /// <param name="maxInput">максимальное входное знаечние</param>
        /// <param name="minOutput">Минимальное значение выходного диапазона</param>
        /// <param name="maxOutput">Максимальное знаечние выходного диапазона</param>
        /// <returns>Возвращет смасштабирование значение в указаном выходном диапазоне</returns>
        public static decimal Mapping(decimal input,decimal minInput, decimal maxInput, decimal minOutput, decimal maxOutput)
        {
            if (minOutput >= maxOutput || minInput >= maxInput || (input < minInput || input > maxInput))  throw new ArgumentOutOfRangeException();
                return ((maxOutput - minOutput) / (maxInput - minInput)) * input +
                       (minOutput * maxInput - maxOutput * minInput) / (maxInput - minInput);
        }
        /// <summary>
        /// Производит масштабирование значения
        /// </summary>
        /// <param name="input">Входное занчение</param>
        /// <param name="minInput">Минимальное входное значение </param>
        /// <param name="maxInput">максимальное входное знаечние</param>
        /// <param name="minOutput">Минимальное значение выходного диапазона</param>
        /// <param name="maxOutput">Максимальное знаечние выходного диапазона</param>
        /// <returns>Возвращет смасштабирование значение в указаном выходном диапазоне</returns>
        public static double Mapping(double input, double minInput, double maxInput, double minOutput, double maxOutput)
        {
            return Mapping(input, minInput, maxInput, minOutput, maxOutput);
        }

    }
}