// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

using System.Collections.Generic;

namespace ASMC.Devices.IEEE.Fluke.Calibrator
{
    public class Calib5522A : CalibrMain
    {
        public Calib5522A()
        {
            UserType = "5522A";
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