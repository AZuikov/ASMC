using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using ASMC.Core.Model;
using ASMC.Data.Model;
using FastMember;

namespace AP.Extension
{
    public static class OperationExtension
    {
        #region Methods

        /// <summary>
        /// Загружает контрольные измерительные точки из внешнего файла.
        /// </summary>
        /// <param name = "operation"></param>
        /// <param name = "path">Путь к  текстовому файлу с точками.</param>
        public static void FillTestPoint(this Operation operation, string path)
        {
            //((IUserItemOperation<object>)operation.UserItemOperation[0]).TestMeasPoints

            foreach (var oper in operation.UserItemOperation)
                if (oper.GetType().GetCustomAttributes(true).Any(q => q.GetType() == typeof(TestMeasPointAttribute)))
                    GetMember(oper, oper.GetType());

            void GetMember(object obj, Type type)
            {
                var attr = (TestMeasPointAttribute) type
                                                   .GetCustomAttributes(true)
                                                   .FirstOrDefault(q => q.GetType() == typeof(TestMeasPointAttribute));
                var attrType = attr.MeasPointType;
                var mode = attr.Mode;

                var accessor = TypeAccessor.Create(type);
                var propertyClass = accessor.GetMembers()
                                            .First(q => q.Name == nameof(IUserItemOperation<object>.TestMeasPoints));

                if (propertyClass.Type.IsInterface && obj != null)
                {
                    var value = accessor[obj, propertyClass.Name];
                    if (value != null) GetMember(value, value.GetType());
                }
                /*Ищем атрибут для диапазона*/

                using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read))
                {
                    var file = File.ReadAllLines(path).ToList();
                    var startIndexBlock = file.FindIndex(s => s.Equals(mode));

                    var endIndexBlock = file.Skip(startIndexBlock + 1).ToList()
                                            .FindIndex(s => s.StartsWith("Operation"));
                    if (endIndexBlock == -1) endIndexBlock = file.FindLastIndex(q => !string.IsNullOrWhiteSpace(q));
                    var date = file.Skip(startIndexBlock + 1).Take(endIndexBlock - 1).ToArray();
                    var reg = new Regex(@"\s\s+");
                    var resultData = date.Select(q => reg.Replace(q, " ").Replace("\t", ""))
                                         .Where(q => !q.StartsWith("#")).ToArray();
                    /*заполняемое хранилище диапазонов*/

                    var arr = resultData.Where(q => !string.IsNullOrWhiteSpace(q))
                                        .Select(q =>
                                                    GenerateMeasurePointFromString(q,
                                                                                   accessor
                                                                                       [obj, propertyClass.Name],
                                                                                   propertyClass.Type,
                                                                                   attr?.MeasPointType)).ToArray();
                    //accessor[obj, propertyClass.Name] = Activator.CreateInstance(propertyClass.Type, ).ToArray());
                }
            }

            object[] GenerateMeasurePointFromString(string str, object inObj, Type inType, Type attMeasPointType)
            {
                str = str.Replace(".", Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator);
                var strarr = str.Split(' ');

                

                var date = str.Split(' ').Where(q => !string.IsNullOrWhiteSpace(q))
                              .Select(q => q.Contains("NA") ? null : (decimal?) double.Parse(q)).ToArray();
                
                var generit = inType.GetGenericArguments().First();
                if (generit.GetGenericTypeDefinition().GetInterfaces()
                           .FirstOrDefault(q => Equals(q.Name, typeof(IMeasPoint<,>).Name)) != null)
                {
                    var at = TypeAccessor.Create(generit);
                    var gta = generit.GenericTypeArguments;
                    var MainVal = Activator.CreateInstance(gta[0], (decimal) date[0]);
                    var AdditionalVal = Activator.CreateInstance(gta[2], (decimal) date[0]);
                }

                return new object[] {0.1M, 0.852M};
            }


        }

        #endregion
    }
}