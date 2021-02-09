using ASMC.Data.Model;
using ASMC.Data.Model.PhysicalQuantity;
using FastMember;
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;

namespace AP.Extension
{
    
    public static class DeviceBaseExtension
    {
        /// <summary>
        /// Заполняет характеристики метрологические характеристикик устройства, которые загружаются из внешнего файла точности.
        /// </summary>
        /// <param name="Devise">Устройство, реализующее интерфейс IDeviceBase.</param>
        /// <param name="path">Полный путь к файлу с характеристиками, включая имя и разрешение.</param>
        public static void FillRangesDevice(this IDeviceRemote Devise, string path)
        {
            GetMember(Devise, Devise.GetType());

            void GetMember(object obj, Type type)
            {
                var accessor = TypeAccessor.Create(type);
                var propertyClass = accessor.GetMembers().Where(q => q.Type.IsClass || q.Type.IsInterface).ToArray();
                foreach (var cl in propertyClass)
                {
                    
                    if (cl.Type.IsInterface&& obj!=null)
                    {
                        object value = accessor[obj, cl.Name];
                        if (value != null) GetMember(value, value.GetType());
                    }
                  
                    /*Ищем атрибут для диапазона*/
                    if (cl.GetAttribute(typeof(AccRangeAttribute), true) != null)
                    {
                        var att = (AccRangeAttribute)cl.GetAttribute(typeof(AccRangeAttribute), true);
                        using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read))
                        {
                            var file = File.ReadAllLines(path).ToList();
                            var str = file.FindIndex(s => s.Equals(att.Mode));
                            //todo Не кооретно находит конец и начало некоторых диапазонов. также в диапазон можеет попасть текст
                            var end = file.Skip(str + 1).ToList().FindIndex(s => s.StartsWith("Mode:"));

                            var date = file.Skip(str + 1).Take(end - 1).ToArray();
                            var reg = new Regex(@"\s\s+");
                            var res = date.Select(q => reg.Replace(q, " ").Replace("\t", "")).Where(q => !q.StartsWith("#")).ToArray();
                            /*заполняемое хранилище диапазонов*/
                            accessor[obj, cl.Name] = Activator.CreateInstance(cl.Type, res.Where(q => !string.IsNullOrWhiteSpace(q)).Select(q => (IPhysicalRange)GenerateRange(q, accessor[obj, cl.Name], cl.Type, att.MeasPointType)).ToArray());
                           // return;
                        }
                    }
                    if (obj == null) continue;

                    GetMember(accessor[obj, cl.Name], cl.Type);
                }
            }

            object GenerateRange(string str, object obj, Type type, Type attMeasPointType)
            {
                str = str.Replace(".", Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator);
                var date = str.Split(' ').Where(q => !string.IsNullOrWhiteSpace(q)).Select(q => q.Contains("NA") ? null : (decimal?)double.Parse(q)).ToArray();

                var generit = type.GetGenericArguments().First();
                if (generit.GetGenericTypeDefinition().GetInterfaces()
                 .FirstOrDefault(q => Equals(q.Name, typeof(IPhysicalRange<,>).Name)) != null)
                {
                    var at = TypeAccessor.Create(generit);

                    var gta = generit.GenericTypeArguments;
                    // ReSharper disable once PossibleInvalidOperationException
                    var StartFirst = Activator.CreateInstance(gta[0], (decimal)date[0]);
                    // ReSharper disable once PossibleInvalidOperationException
                    var StartSecond = Activator.CreateInstance(gta[1], (decimal)date[2]);
                    // ReSharper disable once PossibleInvalidOperationException
                    var EndFirst = Activator.CreateInstance(gta[0], (decimal)date[1]);
                    // ReSharper disable once PossibleInvalidOperationException
                    var EndSecond = Activator.CreateInstance(gta[1], (decimal)date[3]);
                    var rageAccessor = TypeAccessor.Create(generit);

                    var star = Activator.CreateInstance(attMeasPointType);
                    var atstart = TypeAccessor.Create(attMeasPointType);
                    atstart[star, nameof(MeasPoint<Voltage, Voltage>.MainPhysicalQuantity)] = StartFirst;
                    atstart[star, nameof(MeasPoint<Voltage, Voltage>.AdditionalPhysicalQuantity)] = StartSecond;

                    var end = Activator.CreateInstance(attMeasPointType);

                    atstart[end, nameof(MeasPoint<Voltage, Voltage>.MainPhysicalQuantity)] = EndFirst;
                    atstart[end, nameof(MeasPoint<Voltage, Voltage>.AdditionalPhysicalQuantity)] = EndSecond;

                    var acc = new AccuracyChatacteristic(date[6], date[5], date[4]);
                    /*Диапазон*/
                    var range = Activator.CreateInstance(generit);

                    at[range, nameof(PhysicalRange<Voltage, Voltage>.Start)] = star;
                    at[range, nameof(PhysicalRange<Voltage, Voltage>.End)] = end;
                    at[range, nameof(PhysicalRange<Voltage, Voltage>.AccuracyChatacteristic)] = acc;
                    return range;
                }
                else
                {
                    var at = TypeAccessor.Create(generit);

                    var gta = generit.GenericTypeArguments;
                    // ReSharper disable once PossibleInvalidOperationException
                    var StartFirst = Activator.CreateInstance(gta[0], (decimal)date[0]);
                    // ReSharper disable once PossibleInvalidOperationException
                    var EndFirst = Activator.CreateInstance(gta[0], (decimal)date[1]);
                    var rageAccessor = TypeAccessor.Create(generit);

                    var star = Activator.CreateInstance(attMeasPointType);
                    var atstart = TypeAccessor.Create(attMeasPointType);
                    atstart[star, nameof(MeasPoint<Voltage>.MainPhysicalQuantity)] = StartFirst;

                    var end = Activator.CreateInstance(attMeasPointType);

                    atstart[end, nameof(MeasPoint<Voltage>.MainPhysicalQuantity)] = EndFirst;

                    var acc = new AccuracyChatacteristic(date[6], date[5], date[4]);
                    /*Диапазон*/
                    var range = Activator.CreateInstance(generit);

                    at[range, nameof(PhysicalRange<Voltage>.Start)] = star;
                    at[range, nameof(PhysicalRange<Voltage>.End)] = end;
                    at[range, nameof(PhysicalRange<Voltage>.AccuracyChatacteristic)] = acc;
                    return range;
                }

                //var volteStart = new Voltage((decimal)double.Parse(date[0]));
                //var freqStart = new Frequency((decimal)double.Parse(date[2]));
                //var volteEnd = new Voltage((decimal)double.Parse(date[1]));
                //var freqEnd = new Frequency((decimal)double.Parse(date[3]));

                //var acc = new AccuracyChatacteristic(date[6].Contains("NA") ? (decimal?)null : (decimal?)double.Parse(date[6]),
                //    date[5].Contains("NA") ? (decimal?)null : (decimal?)double.Parse(date[5]),
                //    date[4].Contains("NA") ? (decimal?)null : (decimal?)double.Parse(date[4]));

                //return new PhysicalRange<Voltage, Frequency>(new MeasPoint<Voltage, Frequency>(volteStart, freqStart), new MeasPoint<Voltage, Frequency>(volteEnd, freqEnd), acc);
                return null;
            }
        }
    }
}