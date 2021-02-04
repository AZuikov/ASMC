// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

using System.Collections.Generic;
using ASMC.Data.Model;
using ASMC.Data.Model.PhysicalQuantity;
using ASMC.Devices.Interface;
using ASMC.Devices.Interface.SourceAndMeter;

namespace ASMC.Devices.IEEE.Fluke.Calibrator
{
    public class Calib5522A : CalibrMain 
    {
        #region Property

        /// <summary>
        /// Позволяет получить последнюю ошибку из очереди калибратора.
        /// </summary>
        public ErrorCode GetLastErrorCode
        {
            get
            {
                var answer = QueryLine("err?").Split(',');
                if (answer.Length == 2)
                {
                    int.TryParse(answer[0], out var result);
                    return (ErrorCode) result;
                }

                return 0;
            }
        }

        #endregion

        public Calib5522A()
        {
            UserType = "Fluke 5522A";
            
        }

        #region Methods

        /// <summary>
        /// Возвращает массив очереди ошибок калибратора. Порядок массива обратный - первый элемент это последняя ошибка.
        /// </summary>
        /// <returns></returns>
        public ErrorCode[] GetErrorStack()
        {
            var list = new List<ErrorCode>();
            ErrorCode err;
            do
            {
                err = GetLastErrorCode;
                list.Add(err);
            } while (err != ErrorCode.NoError);

            return list.ToArray();
        }

        #endregion

        public void SetVoltageDc(MeasPoint<Voltage> setPoint)
        {
            Out.Set.Voltage.Dc.SetValue(setPoint);
            
        }

        public void SetVoltageAc(MeasPoint<Voltage, Frequency> setPoint)
        {
            Out.Set.Voltage.Ac.SetValue(setPoint);
        }

        public void SetResistance2W(MeasPoint<Resistance> setPoint)
        {
            Out.Set.Resistance.SetValue(setPoint);
            Out.Set.Resistance.SetCompensation(COut.CSet.CResistance.Zcomp.Wire2);

        }

        public void SetResistance4W(MeasPoint<Resistance> setPoint)
        {
            Out.Set.Resistance.SetValue(setPoint);
            Out.Set.Resistance.SetCompensation(COut.CSet.CResistance.Zcomp.Wire4);
        }

        public void SetCurrentDc(MeasPoint<Current> setPoint)
        {
            Out.Set.Current.Dc.SetValue(setPoint);
        }

        public void SetCurrentAc(MeasPoint<Current, Frequency> setPoint)
        {
            Out.Set.Current.Ac.SetValue(setPoint);
        }

        public void SetTemperature(MeasPoint<Temperature> setPoint, COut.CSet.СTemperature.TypeTermocouple typeTermocouple, string unitDegreas)
        {
            Out.Set.Temperature.SetTermoCoupleType(typeTermocouple);
            Out.Set.Temperature.SetValue(setPoint);
        }

        public void SetOutputOn()
        {
            Out.SetOutput(COut.State.On);
        }

        public void SetOutputOff()
        {
            Out.SetOutput(COut.State.Off);
        }

        public void Reset()
        {
            WriteLine(IeeeBase.Reset);
            
        }


        
    }


}