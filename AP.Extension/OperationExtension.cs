using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ASMC.Core.Model;
using ASMC.Data.Model;
using FastMember;

namespace AP.Extension
{
    public static class OperationExtension
    {
        public static void FillTestPoint(this Operation operation, string path)
        {
            //foreach (var oper in operation.UserItemOperation)
            //{
            //    GetMember(oper, oper.GetType());
            //}

            //void GetMember(object obj, Type type)
            //{
            //    var accessor = TypeAccessor.Create(type);
            //    var propertyClass = accessor.GetMembers().Where(q => q.Name== nameof(IUserItemOperation<object>.TestMeasPoints)).ToArray();
            //    foreach (var cl in propertyClass)
            //    {

            //        if (cl.Type.IsInterface && obj != null)
            //        {
            //            object value = accessor[obj, cl.Name];
            //            if (value != null) GetMember(value, value.GetType());
            //        }
            //        /*Ищем атрибут для диапазона*/
            //        if (cl.GetAttribute(typeof(TestMeasPointAttribute), true) != null)
            //        {
            //            var att = (TestMeasPointAttribute)cl.GetAttribute(typeof(TestMeasPointAttribute), true);
            //            using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read))
            //            {
            //                var file = File.ReadAllLines(path).ToList();
            //                var str = file.FindIndex(s => s.Equals(att.Mode));
            //                //todo Не кооретно находит конец и начало некоторых диапазонов. также в диапазон можеет попасть текст
            //                var end = file.Skip(str + 1).ToList().FindIndex(s => s.StartsWith("Mode:"));
            //                if (end == -1)
            //                {
            //                    end = file.FindLastIndex(q => !string.IsNullOrWhiteSpace(q));
            //                }
            //                var date = file.Skip(str + 1).Take(end - 1).ToArray();
            //                var reg = new Regex(@"\s\s+");
            //                var res = date.Select(q => reg.Replace(q, " ").Replace("\t", "")).Where(q => !q.StartsWith("#")).ToArray();
            //                /*заполняемое хранилище диапазонов*/
            //                accessor[obj, cl.Name] = Activator.CreateInstance(cl.Type, res.Where(q => !string.IsNullOrWhiteSpace(q)).Select(q => (IPhysicalRange)GenerateRange(q, accessor[obj, cl.Name], cl.Type, att.MeasPointType)).ToArray());
            //            }
            //        }
            //        if (obj == null) continue;

            //        GetMember(accessor[obj, cl.Name], cl.Type);
            //    }
            //}
        }
    }
}
