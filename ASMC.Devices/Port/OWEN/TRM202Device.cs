using AP.Utils.Data;
using ASMC.Devices.Port.OWEN;
using NLog;
using OwenioNet.Types;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace ASMC.Devices.OWEN
{
    public class TRM202Device : OwenProtocol
    {
        public int DeviceAddres { get; set; }

        /// <summary>
        /// Тип входного датчика или сигнала для входа 1 (2)
        /// </summary>
        public enum in_t
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

        public enum Parametr
        {
            /// <summary>
            /// Тип входного датчика или сигнала для входа 1 (2).
            /// </summary>
            [ReadOnly(false)] [StringValue("in.t")] InT,

            /// <summary>
            /// Точность вывода температуры на входе 1 (2).
            /// </summary>
            [ReadOnly(false)] [StringValue("dPt")] dPt,

            /// <summary>
            /// Положение десятичной точки для входа 1 (2).
            /// </summary>
            [ReadOnly(false)] [StringValue("dP")] dP,

            /// <summary>
            /// Нижняя граница диапазона измерения для входа 1 (2).
            /// </summary>
            [StringValue("in.L")] InL,

            /// <summary>
            /// Верхняя граница диапазона измерения для входа 1 (2).
            /// </summary>
            [ReadOnly(false)] [StringValue("in.H")] InH,

            /// <summary>
            /// Вычислитель квадратного корня для аналогового входа 1.
            /// </summary>
            [ReadOnly(false)] [StringValue("Sqr")] Sqr,

            /// <summary>
            /// Входная величина для ЛУ1 (2).
            /// </summary>
            [ReadOnly(false)] [StringValue("iLU")] I_LU,

            /// <summary>
            /// Сдвиг характеристики для входа 1 (2).
            /// </summary>
            [ReadOnly(false)] [StringValue("SH")] SH,

            /// <summary>
            /// Наклон характеристики для входа 1 (2).
            /// </summary>
            [ReadOnly(false)] [StringValue("KU")] KU,

            /// <summary>
            /// Полоса фильтра для входа 1 (2).
            /// </summary>
            [ReadOnly(false)] [StringValue("Fb")] Fb,

            /// <summary>
            /// Постоянная времени цифрового фильтра для входа 1 (2).
            /// </summary>
            [ReadOnly(false)] [StringValue("inF")] InF,

            /// <summary>
            /// Нижняя граница задания уставки ЛУ1 (2).
            /// </summary>
            [ReadOnly(true)] [StringValue("SL.L")] SL_L,

            /// <summary>
            /// Верхняя граница задания уставки ЛУ1(2).
            /// </summary>
            [ReadOnly(true)] [StringValue("SL.H")] SL_H,

            /// <summary>
            /// Тип логики компаратора 1 (2).
            /// </summary>
            [ReadOnly(false)] [StringValue("CmP")] CmP,

            /// <summary>
            /// Гистерезис для компаратора 1 (2).
            /// </summary>
            [ReadOnly(true)] [StringValue("HYS")] HYS,

            /// <summary>
            /// Задержка включения компаратора 1 (2).
            /// </summary>
            [ReadOnly(false)] [StringValue("don")] Don,

            /// <summary>
            /// Задержка выключения компаратора 1 (2).
            /// </summary>
            [ReadOnly(false)] [StringValue("doF")] DoF,

            /// <summary>
            /// Минимальное время удерживания компаратора 1 (2) во вкл. состоянии.
            /// </summary>
            [ReadOnly(false)] [StringValue("ton")] Ton,

            /// <summary>
            /// Минимальное время удерживания компаратора 1 (2) в выкл.состоянии.
            /// </summary>
            [ReadOnly(false)] [StringValue("toF")] ToF,

            /// <summary>
            /// Режим работы ЦАП 1.
            /// </summary>
            [ReadOnly(false)] [StringValue("dAC")] DAC,

            /// <summary>
            /// Способ управления для выхода 1 (2).
            /// </summary>
            [ReadOnly(false)] [StringValue("CtL")] CtL,

            /// <summary>
            /// Полоса пропорциональности для выхода 1 (2).
            /// </summary>
            [ReadOnly(true)] [StringValue("XP")] XP,

            /// <summary>
            /// Нижняя граница выходного диапазона регистрации ЦАП 1 (2).
            /// </summary>
            [ReadOnly(true)] [StringValue("An.L")] An_L,

            /// <summary>
            /// Верхняя граница выходного диапазона регистрации ЦАП 1 (2).
            /// </summary>
            [ReadOnly(true)] [StringValue("An.H")] An_H,

            /// <summary>
            /// Состояние выхода 1 (2) в режиме «ошибка».
            /// </summary>
            [ReadOnly(true)] [StringValue("oEr")] O_Er,

            /// <summary>
            /// Измеренное значение входной величины.
            /// </summary>
            [ReadOnly(true)] [StringValue("PV")] PV,

            /// <summary>
            /// Значение, посчитанное вычислителем или код ошибки (аналогичный Pv).
            /// </summary>
            [ReadOnly(true)] [StringValue("LuPV")] LuPV,

            /// <summary>
            /// Уставка
            /// </summary>
            [ReadOnly(true)] [StringValue("SP")] SP,

            /// <summary>
            /// Режим индикации.
            /// </summary>
            [ReadOnly(false)] [StringValue("diSP")] DiSP,

            /// <summary>
            /// Время выхода из режима программирования.
            /// </summary>
            [ReadOnly(false)] [StringValue("rESt")] RESt,

            /// <summary>
            /// Протокол обмена.
            /// </summary>
            [ReadOnly(true)] [StringValue("PROT")] PROT,

            /// <summary>
            /// Скорость обмена в сети.
            /// </summary>
            [ReadOnly(true)] [StringValue("bPS")] BPS,

            /// <summary>
            /// Длина сетевого адреса.
            /// </summary>
            [ReadOnly(true)] [StringValue("A.LEn")] A_LEn,

            /// <summary>
            /// Базовый адрес прибора в сети.
            /// </summary>
            [ReadOnly(true)] [StringValue("Addr")] Addr,

            /// <summary>
            /// Задержка при ответе по RS485.
            /// </summary>
            [ReadOnly(true)] [StringValue("rSdL")] RSdL,

            /// <summary>
            /// Длина слова данных.
            /// </summary>
            [ReadOnly(true)] [StringValue("LEn")] LEn,

            /// <summary>
            /// Состояние бита четности в посылке.
            /// </summary>
            [ReadOnly(true)] [StringValue("PrtY")] PrtY,

            /// <summary>
            /// Количество стоп-бит в посылке.
            /// </summary>
            [ReadOnly(true)] [StringValue("Sbit")] Sbit,

            /// <summary>
            /// Версия программы.
            /// </summary>
            [ReadOnly(true)] [StringValue("VER")] VER,

            /// <summary>
            /// Название прибора.
            /// </summary>
            [ReadOnly(true)] [StringValue("Dev")] Dev,

            /// <summary>
            /// Команда смены протокола обмена.
            /// </summary>
            [ReadOnly(true)] [StringValue("PRTL")] PRTL,

            /// <summary>
            /// Команда перехода на новые сетевые настройки.
            /// </summary>
            [ReadOnly(true)] [StringValue("APLY")] APLY,

            /// <summary>
            /// Команда перезагрузки прибора (эквивалент выкл/вкл питания).
            /// </summary>
            [ReadOnly(true)] [StringValue("INIT")] INIT,

            /// <summary>
            /// Код сетевой ошибки при последнем обращении.
            /// </summary>
            [ReadOnly(true)] [StringValue("N.err")] N_Err,

            /// <summary>
            /// Для чтения/записи атрибута «редактирования».
            /// </summary>
            [ReadOnly(true)] [StringValue("Attr")] Attr,

            /// <summary>
            /// Перевод канала на внешнее управление.
            /// </summary>
            [ReadOnly(true)] [StringValue("r-L")] R_L,

            /// <summary>
            /// Значение выходного сигнала или код ошибки.
            /// </summary>
            [ReadOnly(true)] [StringValue("r.oUt")] RoUt,

            // Параметры секретности (группа скрыта под паролем PASS = 100).

            /// <summary>
            /// Защита параметров от просмотра.
            /// </summary>
            [ReadOnly(true)] [StringValue("oAPt")] O_APt,

            /// <summary>
            /// Защита параметров от изменения.
            /// </summary>
            [ReadOnly(true)] [StringValue("wtPt")] WtPt,

            /// <summary>
            /// Защита отдельных параметров от просмотра и изменений (включение или отключение действия атрибутов).
            /// </summary>
            [ReadOnly(true)] [StringValue("EdPt")] EdPt
        }

        public enum TrmError
        {
            [Description("ошибка на входе")] Input = 0Xfd,

            [Description("отсутствие связи с АЦП")]
            NoAcpConnect = 0Xfe,

            [Description("вычисленное значение заведомо не верно")]
            Value = 0Xf0,

            [Description("Запись недопустимого значения в r.oUt(выдается при попытке записи значения отличного от 0 или 1 при ВУ ключевого типа)")]
            InvalidWriteValue = 0Xf1,

            [Description("Значение мантиссы превышает ограничения дескриптора")]
            Mantissa = 0X06,

            [Description("Не найден дескриптор")] Descriptor = 0X28,

            [Description("Размер поля данных не соответствует ожидаемому")]
            FieldValue = 0X31,

            [Description("Значение бита запроса не соответствует ожидаемому")]
            BitValue = 0X32,

            [Description("Редактирование параметра запрещено индивидуальным атрибутом")]
            Edit = 0X33,

            [Description("Недопустимо большой линейный индекс")]
            Index = 0X34,

            [Description("Ошибка при чтении EEPROM (ответ при наличии Er.64)")]
            Read = 0X48,

            [Description("Недопустимое сочетание значений параметров (редактирование параметра заблокировано значением другого или значениями нескольких других)")]
            Unecpected = 0X47
        }

        /// <summary>
        /// Виды протоколов передачи данных, которые поддерживает ТРМ
        /// </summary>
        public enum TrmProtocol
        {
            Owen, //работаем только по протоколу ОВЕН
            ModBusRtu,
            ModBusASCII
        }

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        #region Property

        public AddressLengthType AddressLength { get; set; }

        public TrmProtocol Protocol { get; set; }

        #endregion Property

        public TRM202Device()
        {
            UserType = "ТРМ202";
        }

        #region Methods

        private static void GenericException(TrmError err)
        {
            switch (err)
            {
                case TrmError.BitValue:
                    throw new TrmException { Code = err };
                case TrmError.Descriptor:
                    throw new TrmException { Code = err };
                case TrmError.Edit:
                    throw new TrmException { Code = err };
                case TrmError.FieldValue:
                    throw new TrmException { Code = err };
                case TrmError.Index:
                    throw new TrmException { Code = err };
                case TrmError.Input:
                    throw new TrmException { Code = err };
                case TrmError.InvalidWriteValue:
                    throw new TrmException { Code = err };
                case TrmError.Mantissa:
                    throw new TrmException { Code = err };
                case TrmError.NoAcpConnect:
                    throw new TrmException { Code = err };
                case TrmError.Read:
                    throw new TrmException { Code = err };
                case TrmError.Unecpected:
                    throw new TrmException { Code = err };
                case TrmError.Value:
                    throw new TrmException { Code = err };
            }
        }

        /// <summary>
        /// Входная величина для регулятора/регистратора.
        /// </summary>
        /// <param name = "chanel">Номер канала (регистр, 0 или 1). Номер канала нужно задавать начиная с 1.</param>
        /// <returns></returns>
        public decimal GetLuPvValChanel(ushort chanel)
        {
            return (decimal)ReadFloatParam(DeviceAddres + (chanel - 1), AddressLength, Parametr.LuPV.GetStringValue(),
                                            3);
        }

        /// <summary>
        /// Запрос измеренного значения, по протоколу ОВЕН.
        /// </summary>
        /// <param name = "chanel">Номер канала (регистр, 0 или 1). Номер канала нужно задавать начиная с 1.</param>
        /// <returns></returns>
        public decimal GetMeasValChanel(ushort chanel)
        {
            // Для второго канала нужно увеличить адрес прибора на 1.
            return (decimal)ReadFloatParam(DeviceAddres + (chanel - 1), AddressLength, Parametr.PV.GetStringValue(),
                                            3);
        }

        public decimal GetNumericParam(Parametr parName,
            int size, ushort? Register = null)
        {
            return (decimal)ReadFloatParam(DeviceAddres, AddressLength, parName.GetStringValue(), size, --Register);
        }

        public int GetShortIntPar(Parametr parName, int size, ushort? Register = null)
        {
            return ReadShortIntParam(DeviceAddres, AddressLength, parName.GetStringValue(), size, --Register);
        }

        public override byte[] OwenReadParam(string ParametrName, int addres, ushort? Register = null)
        {
            var answerDevice = base.OwenReadParam(ParametrName, addres, --Register);
            //отловим ошибку
            if (answerDevice.Length == 1)
                if (Enum.IsDefined(typeof(TrmError), (int)answerDevice[0]))
                    GenericException((TrmError)answerDevice[0]);

            return answerDevice;
        }

        public void WriteParametrToTRM(Parametr parName, byte[] writeDataBytes, ushort? Register = null)
        {
            ushort? localRegister = null;
            if (Register != null)
            {
                localRegister = (ushort?)(Register - 1);//адрес канала начинается с нуля
            }

            OwenWriteParam(DeviceAddres, AddressLength, parName.GetStringValue(), writeDataBytes, localRegister);
        }

        public void WriteFloat24Parametr(Parametr parName, float writeValue, ushort? Register = null)
        {
            int maxArrSize = 3;//3 байта максимум для этой модели устройства
            List<byte> list = new List<byte>(BitConverter.GetBytes(writeValue));
            list.Reverse();
            if (list.Count < maxArrSize)
            {
                do
                {
                    list.Add(0x00);
                } while (list.Count != maxArrSize);
            }
            if (list.Count > maxArrSize)
                list.RemoveRange(maxArrSize, list.Count - maxArrSize);

            WriteParametrToTRM(parName, list.ToArray(), Register);
        }

        #endregion Methods

        /// <summary>
        /// Хранит настройки прибора
        /// </summary>
        public class ParametrBackup
        {
        }
    }
}