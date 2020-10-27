// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

using System.Collections.Generic;
using ASMC.Data.Model;
using ASMC.Data.Model.PhysicalQuantity;

namespace ASMC.Devices.IEEE.Fluke.Calibrator
{
    public class Calib5522A : CalibrMain
    {
        public Calib5522A()
        {
            UserType = "5522A";
            Out.Set.Voltage.Dc.Ranges.RealRangeStor = new[]
            {
                new PhysicalRange<Voltage>(new MeasPoint<Voltage>(329.9999M, UnitMultiplier.Mili)),
                new PhysicalRange<Voltage>(new MeasPoint<Voltage>(330M, UnitMultiplier.Mili), new MeasPoint<Voltage>(3.299999M)),
                new PhysicalRange<Voltage>(new MeasPoint<Voltage>(3.3M), new MeasPoint<Voltage>(32.99999M)),
                new PhysicalRange<Voltage>(new MeasPoint<Voltage>(33M), new MeasPoint<Voltage>(329.99999M)),
                new PhysicalRange<Voltage>(new MeasPoint<Voltage>(330M), new MeasPoint<Voltage>(1020M))
            };
            
            Out.Set.Voltage.Ac.Ranges.RealRangeStor = new[]
            {
                CreateAcPoint(1M, UnitMultiplier.Mili,32.999M, UnitMultiplier.Mili,10M, UnitMultiplier.None, 45, UnitMultiplier.None) , 
                CreateAcPoint(1M, UnitMultiplier.Mili,32.999M, UnitMultiplier.Mili,45, UnitMultiplier.None, 10,  UnitMultiplier.Kilo) , 
                CreateAcPoint(1M, UnitMultiplier.Mili,32.999M, UnitMultiplier.Mili,10M, UnitMultiplier.Kilo, 20, UnitMultiplier.Kilo) , 
                CreateAcPoint(1M, UnitMultiplier.Mili,32.999M, UnitMultiplier.Mili,20, UnitMultiplier.Kilo, 50, UnitMultiplier.Kilo) , 
                CreateAcPoint(1M, UnitMultiplier.Mili,32.999M, UnitMultiplier.Mili,50, UnitMultiplier.Kilo, 100, UnitMultiplier.Kilo) , 
                CreateAcPoint(1M, UnitMultiplier.Mili,32.999M, UnitMultiplier.Mili,100, UnitMultiplier.Kilo, 500, UnitMultiplier.Kilo) ,

                CreateAcPoint(33M, UnitMultiplier.Mili,329.99M, UnitMultiplier.Mili,10M, UnitMultiplier.None, 45, UnitMultiplier.None) ,
                CreateAcPoint(33M, UnitMultiplier.Mili,329.99M, UnitMultiplier.Mili,45, UnitMultiplier.None, 10,  UnitMultiplier.Kilo) ,
                CreateAcPoint(33M, UnitMultiplier.Mili,329.99M, UnitMultiplier.Mili,10M, UnitMultiplier.Kilo, 20, UnitMultiplier.Kilo) ,
                CreateAcPoint(33M, UnitMultiplier.Mili,329.99M, UnitMultiplier.Mili,20, UnitMultiplier.Kilo, 50, UnitMultiplier.Kilo) ,
                CreateAcPoint(33M, UnitMultiplier.Mili,329.99M, UnitMultiplier.Mili,50, UnitMultiplier.Kilo, 100, UnitMultiplier.Kilo) ,
                CreateAcPoint(33M, UnitMultiplier.Mili,329.99M, UnitMultiplier.Mili,100, UnitMultiplier.Kilo, 500, UnitMultiplier.Kilo) ,

                CreateAcPoint(330M, UnitMultiplier.Mili,3.29999M, UnitMultiplier.None,10M, UnitMultiplier.None, 45, UnitMultiplier.None) ,
                CreateAcPoint(330M, UnitMultiplier.Mili,3.29999M, UnitMultiplier.None,45, UnitMultiplier.None, 10,  UnitMultiplier.Kilo) ,
                CreateAcPoint(330M, UnitMultiplier.Mili,3.29999M, UnitMultiplier.None,10M, UnitMultiplier.Kilo, 20, UnitMultiplier.Kilo) ,
                CreateAcPoint(330M, UnitMultiplier.Mili,3.29999M, UnitMultiplier.None,20, UnitMultiplier.Kilo, 50, UnitMultiplier.Kilo) ,
                CreateAcPoint(330M, UnitMultiplier.Mili,3.29999M, UnitMultiplier.None,50, UnitMultiplier.Kilo, 100, UnitMultiplier.Kilo) ,
                CreateAcPoint(330M, UnitMultiplier.Mili,3.29999M, UnitMultiplier.None,100, UnitMultiplier.Kilo, 500, UnitMultiplier.Kilo) ,

                CreateAcPoint(330M, UnitMultiplier.Mili,3.29999M, UnitMultiplier.None,10M, UnitMultiplier.None, 45, UnitMultiplier.None) ,
                CreateAcPoint(330M, UnitMultiplier.Mili,3.29999M, UnitMultiplier.None,45, UnitMultiplier.None, 10,  UnitMultiplier.Kilo) ,
                CreateAcPoint(330M, UnitMultiplier.Mili,3.29999M, UnitMultiplier.None,10M, UnitMultiplier.Kilo, 20, UnitMultiplier.Kilo) ,
                CreateAcPoint(330M, UnitMultiplier.Mili,3.29999M, UnitMultiplier.None,20, UnitMultiplier.Kilo, 50, UnitMultiplier.Kilo) ,
                CreateAcPoint(330M, UnitMultiplier.Mili,3.29999M, UnitMultiplier.None,50, UnitMultiplier.Kilo, 100, UnitMultiplier.Kilo) ,
                CreateAcPoint(330M, UnitMultiplier.Mili,3.29999M, UnitMultiplier.None,100, UnitMultiplier.Kilo, 500, UnitMultiplier.Kilo) ,

            };

            PhysicalRange<Voltage, Frequency> CreateAcPoint(decimal startVolt, UnitMultiplier multForStartVolt, decimal stopVolt, UnitMultiplier multForStopVolt , decimal freqValStart, UnitMultiplier multFreqStart , decimal freqStopVal, UnitMultiplier multFreqStop)
            {
                return new
                    PhysicalRange<Voltage, Frequency>(new MeasPoint<Voltage, Frequency>(startVolt, multForStartVolt, freqValStart, multFreqStart),
                                                      new MeasPoint<Voltage, Frequency>(stopVolt, multForStopVolt,
                                                                                        freqStopVal, multFreqStop));
            }

            


        }

         

        /// <summary>
        /// Позволяет получить последнюю ошибку из очереди калибратора.
        /// </summary>
        public ErrorCode GetLastErrorCode
        {
            get
            {
                string[] answer = this.QueryLine("err?").Split(',');
                if (answer.Length == 2)
                {
                    int.TryParse(answer[0], out var result);
                    return (ErrorCode)result;
                }

                return 0;
            }
        }

        /// <summary>
        /// Возвращает массив очереди ошибок калибратора. Порядок массива обратный - первый элемент это последняя ошибка.
        /// </summary>
        /// <returns></returns>
        public ErrorCode[] GetErrorStack()
        {
            List<ErrorCode> list = new List<ErrorCode>();
            ErrorCode err;
            do
            {
                err = GetLastErrorCode;
                list.Add(err);
            } while (err != ErrorCode.NoError);

            return list.ToArray();
        }
    }
}