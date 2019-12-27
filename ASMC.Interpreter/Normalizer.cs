using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ASMC.Interpreter
{
    public static class Normalizer
    {
       

    }
    public struct NormalizerRegular
    {
        /// <summary>
        /// Ищет более 1 пробела подряд и знаки табуляции
        /// </summary>
        public static readonly Regex Spaces = new Regex(@"((  +)|\t)", RegexOptions.Compiled|RegexOptions.IgnoreCase);
        /// <summary>
        /// Ищет пробелы в начале текста и перед новой строкой
        /// </summary>
        public static readonly Regex SpacesTrim = new Regex(@"(\s\n)|(^\s)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    }
    public struct KeywordsRegular
    {
        /// <summary>
        /// Коментарий
        /// </summary>
        public static readonly Regex Comment = new Regex(@"^#(?=\s)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        /// <summary>
        /// объевление переменной
        /// </summary>
        public static readonly Regex Variable = new Regex(@"^var(?=\s)", RegexOptions.Compiled|RegexOptions.IgnoreCase);
        /// <summary>
        /// создание функции
        /// </summary>
        public static readonly Regex Function = new Regex(@"^Function(?=\s)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        /// <summary>
        /// Вызов функции
        /// </summary>
        public static readonly Regex Call = new Regex(@"^Call(?=\s)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        /// <summary>
        /// Поиск выражения скобоках в строке
        /// </summary>
        public static readonly Regex Parantheses = new Regex(@"(?<=\()\S*(?=\))", RegexOptions.Compiled | RegexOptions.IgnoreCase);  
        /// <summary>
        /// Операции
        /// </summary>
        public static readonly Regex MathOperations = new Regex(@"[\\^]|[+]|[-]|[*]|[/]|([=]+)|(!=)|[<]|[>]|[!]", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        /// <summary>
        /// Аргументы
        /// </summary>     
        public static readonly Regex ValuesAurguments = new Regex(@"(\"".*?\"")|((((?<=\-)|(?<=\+)|(?<=,)))(\-{1}[0-9.,]+)|(([\w.])+(?=[=+-/*^()]|)))", RegexOptions.Compiled | RegexOptions.IgnoreCase);
         /// <summary>
         /// Текстовое значение
         /// </summary>
        public static readonly Regex Strings = new Regex(@"((?<=\"").*?(?=\""))", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    }
}
