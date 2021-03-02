using System;
using System.Collections.Generic;
using System.Globalization;
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
        /// <param name = "path">Путь к  текстовому файлу с измерительными точками.</param>
        public static void FillTestPoint(this Operation operation, string path)
        {
            //файл лучше весь считать заранее один раз.
            var file = new List<string>();
            using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                file = File.ReadAllLines(path).ToList();
            }

            foreach (var oper in operation.UserItemOperation)
                if (oper.GetType().GetCustomAttributes(true).Any(q => q.GetType() == typeof(TestMeasPointAttribute)))
                    GetMember(oper, oper.GetType(), file);

            void GetMember(object obj, Type typeOperation, List<string> inFile)
            {
                var attr = (TestMeasPointAttribute) typeOperation
                                                   .GetCustomAttributes(true)
                                                   .FirstOrDefault(q => q.GetType() == typeof(TestMeasPointAttribute));
                var attrType = attr.MeasPointType;
                var mode = attr.Mode;

                var accessor = TypeAccessor.Create(typeOperation);
                var propertyClass = accessor.GetMembers()
                                            .First(q => q.Name == nameof(IUserItemOperation<object>.TestMeasPoints));

                if (propertyClass.Type.IsInterface && obj != null)
                {
                    var value = accessor[obj, propertyClass.Name];
                    if (value != null) GetMember(value, value.GetType(), inFile);
                }

                /*выбираем из данных тольку ту часть, которая нужна для этого вида измерения*/
                var startIndexBlock = inFile.FindIndex(s => s.Equals(mode));

                var endIndexBlock = inFile.Skip(startIndexBlock + 1).ToList()
                                          .FindIndex(s => s.StartsWith("Operation"));
                if (endIndexBlock == -1) endIndexBlock = inFile.FindLastIndex(q => !string.IsNullOrWhiteSpace(q));
                var date = inFile.Skip(startIndexBlock + 1).Take(endIndexBlock - 1).ToArray();
                var reg = new Regex(@"\s\s+");
                var resultData = date.Select(q => reg.Replace(q, " ").Replace("\t", ""))
                                     .Where(q => !q.StartsWith("#")).ToArray();
                /*создаем из текстовых данных массив измерительных точек MeasPoint*/

                var arr = resultData.Where(q => !string.IsNullOrWhiteSpace(q))
                                    .Select(q =>
                                                GenerateMeasurePointFromString(q,
                                                                               propertyClass.Type,
                                                                               attr?.MeasPointType)).ToArray();
                //теперь массив точек нужно присвоить списку точек, поверяемых (проверяемых) в данной операции

                accessor[obj, propertyClass.Name] = Array.CreateInstance(attr.MeasPointType, arr.Length, 2);
                
                //заполняем TestMeasPoint - точки, которые нужно поверять, заполняются для каждой операции поверки
                int rows = arr.GetUpperBound(0) + 1;
                int columns = arr.Length/rows;

                for (var i = 0; i < arr.Length; i++)
                {
                    ((Array) accessor[obj, propertyClass.Name]).SetValue(arr[i][0], i,0);
                    ((Array) accessor[obj, propertyClass.Name]).SetValue(arr[i][1], i,1);
                }
            }

            object[] GenerateMeasurePointFromString(string str, Type inType, Type attMeasPointType)
            {
                str = str.Trim();
                str = str.Replace(".", Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator);
                var strArr = str.Split(' ');
                if (strArr.Length != 5 && strArr.Length != 4)
                    throw new ArgumentException($"Неверное число параметров в строке: {strArr.Length}");
                decimal? RangeVal=null;
                decimal? MainVal=null;
                object rangeSimpleObj = null;
                object testingPoint = null;
                UnitMultiplier mainUnitMultiplier = UnitMultiplier.None;
                decimal? additionalVal = null;
                UnitMultiplier additionalUnitMultiplier = UnitMultiplier.None;

                // от длины строки будет зависеть как она парсится
                if (strArr.Length == 5)
                {
                    RangeVal = decimal.TryParse(strArr[0], out _) ? decimal.Parse(strArr[0]) : (decimal?)null;
                     MainVal = decimal.TryParse(strArr[1], out _) ? decimal.Parse(strArr[1]) : (decimal?)null;
                     mainUnitMultiplier =
                        UnitMultiplierExtension.ParseUnitMultiplier(strArr[2], CultureInfo.GetCultureInfo("en-US"));

                    additionalVal = decimal.TryParse(strArr[3], out _) ? decimal.Parse(strArr[2]) : (decimal?)null;
                    additionalUnitMultiplier =
                        UnitMultiplierExtension.ParseUnitMultiplier(strArr[4], CultureInfo.GetCultureInfo("en-US"));

                    rangeSimpleObj = Activator.CreateInstance(attMeasPointType, (decimal)RangeVal, mainUnitMultiplier);
                }
                //если предел не указан, то вместо него будет null
                else if (strArr.Length == 4)
                {
                    MainVal = decimal.TryParse(strArr[0], out _) ? decimal.Parse(strArr[0]) : (decimal?)null;
                    mainUnitMultiplier =
                        UnitMultiplierExtension.ParseUnitMultiplier(strArr[1], CultureInfo.GetCultureInfo("en-US"));

                    additionalVal = decimal.TryParse(strArr[2], out _) ? decimal.Parse(strArr[2]) : (decimal?)null;
                    additionalUnitMultiplier =
                        UnitMultiplierExtension.ParseUnitMultiplier(strArr[3], CultureInfo.GetCultureInfo("en-US"));
                }

                var generit = inType.GetGenericArguments();
                
                //var pointAccessor = TypeAccessor.Create(attMeasPointType);
                
                //либо это простая точка, без вложенной физ. величины или сложная.
                 testingPoint = generit.Length == 2? 
                    Activator.CreateInstance(attMeasPointType, (decimal)MainVal, mainUnitMultiplier, 
                                                                            additionalVal, additionalUnitMultiplier):
                    Activator.CreateInstance(attMeasPointType, (decimal) MainVal, mainUnitMultiplier);
                
                return new[] { rangeSimpleObj, testingPoint };
            }
        }

        #endregion
    }
}