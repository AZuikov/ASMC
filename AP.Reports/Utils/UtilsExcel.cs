using System;
using System.Linq;

namespace AP.Reports.Utils
{
    public static class UtilsExcel
    {
        /// <summary>
        /// Возвращает буквенный символ столбца Microsoft Excel, соответствующий заданному порядковому номеру.
        /// </summary>
        /// <param name="number">Порядковый номер столбца.</param>
        /// <returns></returns>
        public static string NumberToLetters(int number)
        {
            string result;
            if (number > 0)
            {
                var alphabets = (number - 1) / 26;
                var remainder = (number - 1) % 26;
                result = ((char)('A' + remainder)).ToString();
                if (alphabets > 0)
                    result = NumberToLetters(alphabets) + result;
            }
            else
                result = null;
            return result;
        }
        /// <summary>
        /// Возвращает порядковый номер столбца Microsoft Excel, соответствующий заданному буквенному символу.
        /// </summary>
        /// <param name="letters">Буквенный символ столбца.</param>
        /// <returns></returns>
        public static int LettersToNumber(string letters)
        {
            var result = 0;
            if (letters.Length > 0 && letters.All(a => (a >= 'A' && a <= 'Z')))
                try
                {
                    for (var i = letters.Length; i > 0; i--)
                        result += (int)checked(Math.Pow(26, i - 1) + (letters[i - 1] - 'A') * Math.Pow(26, letters.Length - i));
                }
                catch (OverflowException)
                {
                    result = -1;
                }
            else
                result = -1;
            return result;
        }
    }
}
