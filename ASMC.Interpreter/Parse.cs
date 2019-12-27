using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;

namespace ASMC.Interpreter
{
    public class Parse
    {
        /// <summary>
        ///     Математические знаки(опарации).
        /// </summary>
        public enum Signs
        {
            /// <summary>
            ///     Знак равенства.
            /// </summary>
            [Description("=")]
            Equally,

            /// <summary>
            ///     Знак степени.
            /// </summary>
            [Description("^")]
            Power,

            /// <summary>
            ///     Знак сложения
            /// </summary>
            [Description("+")]
            Addition,

            /// <summary>
            ///     Знак вычитания
            /// </summary>
            [Description("-")]
            Subtraction,

            /// <summary>
            ///     Знак деления
            /// </summary>
            [Description("/")]
            Division,

            /// <summary>
            ///     Знак умножения
            /// </summary>
            [Description("*")]
            Multiplication,
            /// <summary>
            ///     Знак проверки на равенство
            /// </summary>
            [Description("==")]
            Equality,

            /// <summary>
            ///     Знак больше чем
            /// </summary>
            [Description(">")]
            More,
            /// <summary>
            ///     Знак меньше чем
            /// </summary>
            [Description("<")]
            Less,
            /// <summary>
            ///     Знак меньше чем
            /// </summary>
            [Description("!=")]
            NotEqual,    
            /// <summary>
            ///     Инверсия
            /// </summary>
            [Description("!")]
            Inversion
        }

        public Context context = new Context();

        public void Pars(string code)
        {
            code = NormalizerRegular.Spaces.Replace(code, " "); /*Чистим от лисшних символов*/
            code = NormalizerRegular.SpacesTrim.Replace(code, "".TrimEnd());    /*чистим от лишних символов в конце*/
                var arrayRows = code.Split('\n');
                foreach (var row in arrayRows)
                {
                    if(KeywordsRegular.Comment.IsMatch(row))  /*Если # комментарий, пропускаем*/
                    {
                       continue;
                    }
                    if (KeywordsRegular.Variable.IsMatch(row))  /**Если объевление переменной*/
                    {
                        var str = row.Replace("var", "");
                        if (str.IndexOf("=", StringComparison.Ordinal) >= 0)   /*Если есть присвоение значения*/
                        {
                            var values = KeywordsRegular.ValuesAurguments.Matches(str);
                            str = KeywordsRegular.ValuesAurguments.Replace(str, "");
                            var operators = new List<Signs>(Enum.GetValues(typeof(Signs)).Length);
                            foreach (var op in KeywordsRegular.MathOperations.Matches(str))
                                operators.Add(Enum.GetValues(typeof(Signs)).Cast<Signs>().First(x =>
                                    (x.GetType().GetField(x.ToString())
                                             .GetCustomAttributes(typeof(DescriptionAttribute), false) as
                                         DescriptionAttribute[] ??
                                     throw new InvalidOperationException(x.ToString() + @" операция без аттрибута"))
                                    .First()
                                    .Description.ToString()
                                    .Equals(op.ToString(), StringComparison.CurrentCultureIgnoreCase)));

                            if (values.Count > 2)          /*Если содержит какую либо операцию*/
                            {
                                var content1 = new Context();
                                dynamic result = null; /*содержит конеченое расчетное значение*/
                                string a, b; /*Левый и правый аргумент*/
                                foreach (var op in operators)
                                {
                                    if (op == Signs.Equally) continue;
                                    if (result == null)
                                    {
                                        a = Map(values[operators.IndexOf(op)].Value, ref content1, ref context);
                                    }
                                    else
                                    {
                                        a = result.GetHashCode().ToString();
                                        content1.SetVariabel(a, result);
                                    }

                                    b = Map(values[operators.IndexOf(op) + 1].Value, ref content1, ref context);

                                    switch (op)
                                    {
                                        case Signs.Power:
                                            throw new NotImplementedException();
                                            break;
                                        case Signs.Addition:
                                            result = new AddNotTerminalExpression(new TerminalExpression(a),
                                                new TerminalExpression(b)).Interpret(content1);
                                            break;
                                        case Signs.Subtraction:
                                            result = new SubNotTerminalExpression(new TerminalExpression(a),
                                                new TerminalExpression(b)).Interpret(content1);
                                            break;
                                        case Signs.Division:
                                            throw new NotImplementedException();
                                            break;
                                        case Signs.Multiplication:
                                            throw new NotImplementedException();
                                            break;
                                        case Signs.Equally:
                                            throw new ArgumentException("Неожиданный знак =");
                                        case Signs.Equality:
                                            throw new NotImplementedException();
                                            break;
                                        case Signs.More:
                                            throw new NotImplementedException();
                                            break;
                                        case Signs.Less:
                                            throw new NotImplementedException();
                                            break;
                                        case Signs.NotEqual:
                                            throw new NotImplementedException();
                                            break;
                                        case Signs.Inversion:
                                            throw new NotImplementedException();
                                            break;
                                        default:
                                            throw new ArgumentException("Неизвестная операция");
                                    }
                                }

                                context.SetVariabel(values[0].Value, result);/*создаем переменную и присваеваем ей значение*/
                            }
                            else  /*выполняем когда только присвоение и не требуется дополнительных оппераций*/
                            {
                                object val22;
                                if (decimal.TryParse(values[1].Value, out var res))
                                    val22 = res;
                                else if (KeywordsRegular.Strings.IsMatch(values[1].Value))
                                    val22 = values[1].Value;
                                else
                                    val22 = context.GetVariable(values[1].Value);
                                context.SetVariabel(values[0].Value, val22);
                            }
                        }
                        else
                        {
                            context.SetVariabel(str.Substring(0, str.Length), null);
                        }
                    }
                   
                }

            string Map(string value,ref Context body,ref Context parentcontent)
            {
                string result;

                
                if(decimal.TryParse(value, out var res))    /*Пытаемся преобразоавть в число*/
                {
                    result = res.GetHashCode().ToString();
                    body.SetVariabel(result, res);
                }
                else if(KeywordsRegular.Strings.IsMatch(value))    /*Проверяем является ли значение строкой*/
                {
                    result = value.GetHashCode().ToString();
                    body.SetVariabel(result, value);
                }
                else  /*Берем знаечние из глабального контента*/
                {
                    result = value;
                    body.SetVariabel(result, parentcontent.GetVariable(result));
                }
                return result;
            }
        }
        public void Function(string[] arr)
        {

        }
    }
}