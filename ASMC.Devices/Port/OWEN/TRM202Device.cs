using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASMC.Devices.OWEN
{
    public class TRM202Device : OwenProtocol
    {
        /// <summary>
        /// Тип входного датчика или сигнала для входа 1 (2)
        /// </summary>
        enum in_t
        {
            r385 = 1,
            r_385,
            r391,
            r_391,
            r_21,
            r426,
            r_426,
            r_23,
            r428,
            r_428,
            E_A1,
            E_A2,
            E_A3,
            E__b,
            E__j,
            E__K,
            E__L,
            E__n,
            E__r,
            E__S,
            E__t,
            i0_5,
            i0_20,
            i4_20,
            U_50,
            U0_1
        }




        public enum TrmError
        {
        [Description ("ошибка на входе")]ErroInput = 0Xfd,
        [Description ("отсутствие связи с АЦП")]NoAcpConnect = 0Xfe,
        [Description ("вычисленное значение заведомо не верно")]ValueError = 0Xf0,
        [Description ("Запись недопустимого значения в r.oUt(выдается при попытке записи значения отличного от 0 или 1 при ВУ ключевого типа)")]InvalidWriteValue = 0Xf1,
        [Description("Значение мантиссы превышает ограничения дескриптора")] MantissaError = 0X06,
        [Description("Не найден дескриптор")] DescriptorError = 0X28,
        [Description("Размер поля данных не соответствует ожидаемому")] FieldValueError = 0X31,
        [Description("Значение бита запроса не соответствует ожидаемому")] BitValueErroe = 0X32,
        [Description("Редактирование параметра запрещено индивидуальным атрибутом")] EditError = 0X33,
        [Description("Недопустимо большой линейный индекс")] IndexError = 0X34,
        [Description("Ошибка при чтении EEPROM (ответ при наличии Er.64)")] ReadError = 0X48,
        [Description("Недопустимое сочетание значений параметров (редактирование параметра заблокировано значением другого или значениями нескольких других)")] UnecpectedError = 0X34,

        }
    }

    
}
