using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using ASMC.Core.Model;
using ASMC.Data.Model;
using ASMC.Data.Model.PhysicalQuantity;
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
                                                                                   propertyClass.Type,
                                                                                   attr?.MeasPointType)).ToArray();
                    //accessor[obj, propertyClass.Name] = Activator.CreateInstance(propertyClass.Type, arr);
                }
            }

            object GenerateMeasurePointFromString(string str,  Type inType, Type attMeasPointType)
            {
                str = str.Replace(".", Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator);
                var strArr = str.Split(' ');
                if (strArr.Length !=4) 
                    throw new ArgumentException($"Неверное число параметров в строке: {strArr.Length}"); 
                
                var mainVal = decimal.TryParse(strArr[0], out _) ? decimal.Parse(strArr[0]) : (decimal?)null;
                UnitMultiplier mainUnitMultiplier = UnitMultiplierExtension.ParseUnitMultiplier(strArr[1], CultureInfo.GetCultureInfo("en-US"));
              
                var additionalVal = decimal.TryParse(strArr[2], out _)? decimal.Parse(strArr[2]) : (decimal?) null;
                UnitMultiplier additionalUnitMultiplier = UnitMultiplierExtension.ParseUnitMultiplier(strArr[3], CultureInfo.GetCultureInfo("en-US"));

                var generit = inType.GetGenericArguments();
                
                var pointAccessor = TypeAccessor.Create(attMeasPointType);
                
                if (generit.Length == 2)
                {
                   var pointComplexObj  = Activator.CreateInstance(attMeasPointType, (decimal) mainVal,mainUnitMultiplier,  additionalVal,additionalUnitMultiplier);
                   
                    //pointAccessor[pointComplexObj, nameof(MeasPoint<Voltage, Voltage>.MainPhysicalQuantity.Value)] = mainVal;
                    //pointAccessor[pointComplexObj, nameof(MeasPoint<Voltage, Voltage>.MainPhysicalQuantity.Multiplier)] = mainUnitMultiplier;
                    //pointAccessor[pointComplexObj, nameof(MeasPoint<Voltage, Voltage>.AdditionalPhysicalQuantity.Value)] = additionalVal;
                    //pointAccessor[pointComplexObj, nameof(MeasPoint<Voltage, Voltage>.AdditionalPhysicalQuantity.Multiplier)] = additionalUnitMultiplier;
                    return pointComplexObj;
                }

                var pointSimpleObj = Activator.CreateInstance(attMeasPointType, (decimal)mainVal, mainUnitMultiplier);
                
                return pointSimpleObj;

            }


        }

        #endregion
    }
}