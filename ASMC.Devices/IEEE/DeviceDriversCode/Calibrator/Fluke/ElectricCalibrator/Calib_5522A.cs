﻿// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

using System;
using System.Collections.Generic;
using System.Linq;
using AP.Utils.Data;
using ASMC.Data.Model;
using ASMC.Data.Model.PhysicalQuantity;
using ASMC.Devices.Interface.SourceAndMeter;
using ASMC.Devices.Model;

namespace ASMC.Devices.IEEE.Fluke.Calibrator
{
    public class Calib_5522A : CalibrMain, IResistance4W
    {
        public Calib_5522A()
        {
            UserType = "Fluke 5522A";
            Resistance4W = new Resist4W(Device,this);

        }

        public ISourcePhysicalQuantity<Resistance> Resistance4W { get; protected set; }

        protected override string GetCommandForErrorQuery()
        {
            return "err?";
        }

        protected override int GetLastErrorCode()
        {
            var answer = Device.QueryLine(GetCommandForErrorQuery()).Split(',');
            if (answer.Length == 2)
            {
                int.TryParse(answer[0], out var result);
                if (Enum.IsDefined(typeof(ErrorCode5522A), result))
                    return result;
            }
            return (int)ErrorCode5522A.NoError;

       
    }

        protected internal override void CheckErrors()
        {
            List<int> errCodeList = new List<int>();
            List<string> errStrList = new List<string>();
            int errCode;
            do
            {
                errCode = GetLastErrorCode();
                if (Enum.IsDefined(typeof(ErrorCode5522A), errCode) && (errCode != (int)ErrorCode5522A.NoError))
                {
                    errCodeList.Add(errCode);
                    errStrList.Add(((ErrorCode5522A)errCode).GetStringValue());
                }

            } while (errCode != 0);

            var errorStr = "";
            for (var i = 0; i < errStrList.Count; i++)
                errorStr = errorStr + $"{i + 1}) {errCodeList[i]}: {errStrList[i]}\n";
            if (errCodeList.Count > 0)
                throw new Exception($"{Device.StringConnection}: Очередь ошибок: \n{errorStr}");

        }

        
        public class Resist4W : Resist
        {


            public Resist4W(IeeeBase deviceCalibr,CalibrMain calibrMain) : base(deviceCalibr,calibrMain)
            {
                RangeStorage = new RangeDevice();
                CompensationMode = new ICommand[]
                        {
                            new Command("ZCOMP WIRE4", "4х проводная компенсация", 4)
                        };

            }


            #region Methods

            protected override string GetUnit()
            {
                return "OHM";
            }

            public override void SetValue(MeasPoint<Resistance> value)
            {
                    base.SetValue(value);
                calibrator.WriteLine(CompensationMode.First().StrCommand);
                //todo проверка на ошибки
            }


            #endregion

            public class RangeDevice : RangeDeviceBase<Resistance>
            {
                [AccRange("Mode: Ohms 4W", typeof(MeasPoint<Resistance>))]
                public override RangeStorage<PhysicalRange<Resistance>> Ranges { get; set; }

            }
      
        }


        #region ErrosCatch



        #endregion ErrosCatch

        public enum ErrorCode5522A
        {
            [StringValue("Ошибки отсутствуют.")] NoError = 0,

            [StringValue("(DDE:FR)   Переполнение очереди ошибок")]
            Err1 = 1,

            [StringValue("(DDE:FR D) Внутренний процессор не отвечает (отправка)")]
            Err100 = 100,

            [StringValue("(DDE:FR D) Внутренний процессор не отвечает (прием)")]
            Err101 = 101,

            [StringValue("(DDE:FR D) Нет синхронизации с внутренним процессором")]
            Err102 = 102,

            [StringValue("(DDE:FR)   Неверная команда защиты \"guard xing\"")]
            Err103 = 103,

            [StringValue("(DDE:FR D) Сработало аппаратное реле")]
            Err104 = 104,

            [StringValue("(DDE:FR D) Внутренний процессор в состоянии ожидания")]
            Err105 = 105,

            [StringValue("(DDE:FR D )АЦП в спящем режиме")]
            Err106 = 106,

            [StringValue("(DDE:FR D) Внутренний процессор в состоянии ожидания")]
            Err107 = 107,

            [StringValue("(DDE:FR)   Внутренний процессор устарел")]
            Err108 = 108,

            [StringValue("(DDE:FR D) Ошибка четности внутреннего процессора")]
            Err109 = 109,

            [StringValue("(DDE:FR D) Ошибка переполнения внутреннего процессора")]
            Err110 = 110,

            [StringValue("(DDE:FR D) Ошибка кадрирования внутреннего процессора")]
            Err111 = 111,

            [StringValue("(DDE:FR D) Отказ внутреннего процессора")]
            Err112 = 112,

            [StringValue("(DDE:FR D) Отказ ввода внутреннего процессора")]
            Err113 = 113,

            [StringValue("(DDE:FR D) Ошибка обнаружения отказа внутреннего процессора")]
            Err114 = 114,

            [StringValue("(DDE:FR D) Ошибка чтения/записи внутреннего процессора")]
            Err115 = 115,

            [StringValue("(DDE:FR D) Получены неожидаемые данные (IG)")]
            Err116 = 116,

            [StringValue("(DDE:FR D) Невозможно загрузить форму сигнала")]
            Err200 = 200,

            [StringValue("(DDE: )    Неверный номер процедуры")]
            Err300 = 300,

            [StringValue("(DDE: )    Пункт процедуры не существует")]
            Err301 = 301,

            [StringValue("(DDE: )    Загрузка в состоянии занятости невозможна")]
            Err302 = 302,

            [StringValue("(DDE: )    Невозможно начать/продолжить калибровку")]
            Err303 = 303,

            [StringValue("(DDE: )    Неверные единицы эталона")]
            Err304 = 304,

            [StringValue("(DDE: )    Введено значение вне диапазона")]
            Err305 = 305,

            [StringValue("(DDE: )    Эталонное значение не ожидается")]
            Err306 = 306,

            [StringValue("(DDE: )    Завершение команды проигнорировано")]
            Err307 = 307,

            [StringValue("(DDE:FR)   Калибровочная постоянная вне диапазона")]
            Err308 = 308,

            [StringValue("(DDE:FR)   Калибровка нуля закончилась неудачно")]
            Err309 = 309,

            [StringValue("(DDE:FR D) Отказ последовательности при калибровке")]
            Err310 = 310,

            [StringValue("(DDE:FR D) Отказ измерения АЦП")]
            Err311 = 311,

            [StringValue("(DDE:FR)   Неверный параметр этапа калибровки")]
            Err312 = 312,

            [StringValue("(DDE: )    Переключатель калибровки должен находится в положении ENABLED")]
            Err313 = 313,

            [StringValue("(DDE:FR)   Деление на ноль")]
            Err314 = 314,

            [StringValue("(DDE:FR)   Калибратор должен находится в рабочем режиме")]
            Err315 = 315,

            [StringValue("(DDE:FR)   Открыть термопару для калибровки через интерфейс RJ")]
            Err316 = 316,

            [StringValue("(DDE:FR)   Неверный эталон Z или ввод")]
            Err317 = 317,

            [StringValue("(DDE:FR)   При калибровке превышен верхний предел ЦАП")]
            Err318 = 318,

            [StringValue("(DDE: R)   Калибровка необходима каждые 7 дней")]
            Err319 = 319,

            [StringValue("(DDE: R)   Калибровка нулевого сопротивления необходима каждые 12 часов")]
            Err320 = 320,

            [StringValue("(QYE:F)    Необычная ошибка калибровки %d")]
            Err398 = 398,

            [StringValue("(QYE:F)    Ошибка во время %s")]
            Err399 = 399,

            [StringValue("(DDE:FR D) Кодировщик не отвечает VERS")]
            Err400 = 400,

            [StringValue("(DDE:FR D) Кодировщик не отвечает COMM")]
            Err401 = 401,

            [StringValue("(DDE:FR D) Кодировщик не отвечает STAT")]
            Err402 = 402,

            [StringValue("(DDE:FR)   Отказ самотестирования кодировщика")]
            Err403 = 403,

            [StringValue("(DDE:FR)   Правое переполнение дисплея")]
            Err405 = 405,

            [StringValue("(DDE:FR)   Недопустимый символ #%d")]
            Err406 = 406,

            [StringValue("(DDE:FR)   Нет сброса кодировщика")]
            Err407 = 407,

            [StringValue("(DDE:FR)   Неверная команда кодировщика")]
            Err408 = 408,

            [StringValue("(DDE:FR D) Неожидаемый сброс кодировщика")]
            Err409 = 409,

            [StringValue("(DDE:FR D) Ошибка внутреннего состояния")]
            Err500 = 500,

            [StringValue("(DDE:)     Неверное ключевое слово или пункт меню")]
            Err501 = 501,

            [StringValue("(DDE:)     Гармоника должна быть в пределах 1-50")]
            Err502 = 502,

            [StringValue("(DDE:)     Частота должна быть больше нуля")]
            Err503 = 503,

            [StringValue("(DDE:)     Амплитуда переменного тока должна быть больше нуля")]
            Err504 = 504,

            [StringValue("(DDE:)     Полное сопротивление должно быть больше или равно нулю")]
            Err505 = 505,

            [StringValue("(DDE:)     Функция не доступна")]
            Err506 = 506,

            [StringValue("(DDE:)     Значение не доступно")]
            Err507 = 507,

            [StringValue("(DDE:)     Невозможно автоматически ввести ватты")]
            Err508 = 508,

            [StringValue("(DDE:)     Выходное значение вне диапазона пользователя")]
            Err509 = 509,

            [StringValue("(DDE:)     Коэффициент заполнения должен быть 1.0-99.0")]
            Err510 = 510,

            [StringValue("(DDE:)     Коэффициент мощности должен быть от 0,0 до 1,0")]
            Err511 = 511,

            [StringValue("(DDE:)     Невозможно сейчас выбрать это поле")]
            Err512 = 512,

            [StringValue("(DDE:)     Изменение цифры за пределами диапазона")]
            Err513 = 513,

            [StringValue("(DDE:)     Невозможно сейчас выбрать изменение этого поля")]
            Err514 = 514,

            [StringValue("(DDE:)     Невозможно сейчас изменить выходное значение")]
            Err515 = 515,

            [StringValue("(DDE:)     дБм только для синусоидального переменного напряжения")]
            Err516 = 516,

            [StringValue("(DDE:)     Слишком высокая частота для несинусоидального сигнала")]
            Err517 = 517,

            [StringValue("(DDE:)     Значение вне фиксированного диапазона")]
            Err518 = 518,

            [StringValue("(DDE:)     Необходимо указать единицы выходного значения")]
            Err519 = 519,

            [StringValue("(DDE:)     Невозможно установить одновременно две частоты")]
            Err520 = 520,

            [StringValue("(DDE:)     Невозможно воспроизвести одновременно 3 значения")]
            Err521 = 521,

            [StringValue("(DDE:)     Температура должна быть в градусах C или F")]
            Err522 = 522,

            [StringValue("(DDE:)     Операция в настоящее время невозможна")]
            Err523 = 523,

            [StringValue("(DDE:)     Предел слишком мал или велик")]
            Err526 = 526,

            [StringValue("(DDE:)     Изменения сейчас невозможны, за исключением СБРОСА")]
            Err527 = 527,

            [StringValue("(DDE:)     Смещение вне диапазона")]
            Err528 = 528,

            [StringValue("(DDE:)     Невозможно изменить на или с 0 Гц")]
            Err529 = 529,

            [StringValue("(DDE:)     Неверный образ состояния, загрузка невозможна")]
            Err530 = 530,

            [StringValue("(DDE:)     Смещение термопары ограничено значениями +/-500 C")]
            Err531 = 531,

            [StringValue("(DDE:)     Невозможно перейти в режим STBY при измерении посредством термопары")]
            Err532 = 532,

            [StringValue("(DDE:)     Невозможно сейчас установить смещение")]
            Err533 = 533,

            [StringValue("(DDE:)     Невозможно зафиксировать этот диапазон")]
            Err534 = 534,

            [StringValue("(DDE:)     Невозможно сейчас установить фазу или коэффициент мощности")]
            Err535 = 535,

            [StringValue("(DDE:)     Невозможно сейчас установить форму сигнала")]
            Err536 = 536,

            [StringValue("(DDE:)     Невозможно сейчас задать гармоники")]
            Err537 = 537,

            [StringValue("(DDE:)     Невозможно сейчас изменить коэффициент заполнения")]
            Err538 = 538,

            [StringValue("(DDE:)     Невозможно сейчас изменить компенсацию")]
            Err539 = 539,

            [StringValue("(DDE:FR)   OUTPUT (Выход) по току переведен на 5725 А")]
            Err540 = 540,

            [StringValue("(DDE:)     Величина TC ref должна быть действительной температурой термопары")]
            Err541 = 541,

            [StringValue("(DDE:)     Невозможно сейчас включить ЗЕМЛЮ")]
            Err542 = 542,

            [StringValue("(DDE:R)    STA не может обновить OTD")]
            Err543 = 543,

            [StringValue("(DDE:)     Невозможно ввести мощность несинусоидального сигнала")]
            Err544 = 544,

            [StringValue("(DDE:)     Изменение сейчас невозможно")]
            Err545 = 545,

            [StringValue("(DDE:)     Невозможно сейчас установить в мультивибратор это значение")]
            Err546 = 546,

            [StringValue("(DDE:)     Невозможно сейчас установить выходное сопротивление")]
            Err547 = 547,

            [StringValue("(DDE:FR)   Компенсация сейчас ВЫКЛЮЧЕНА")]
            Err548 = 548,

            [StringValue("(DDE:)     Период должен быть больше или равен нулю")]
            Err549 = 549,

            [StringValue("(DDE:)     Отчет уже напечатан")]
            Err550 = 550,

            [StringValue("(DDE:)     Модуль калибровки осциллографа не установлен")]
            Err551 = 551,

            [StringValue("(DDE:)     Не функция калибровки осциллографа")]
            Err552 = 552,

            [StringValue("(DDE:)     Невозможно сейчас установить форму маркера")]
            Err553 = 553,

            [StringValue("(DDE:)     Невозможно сейчас установить параметр видео")]
            Err554 = 554,

            [StringValue("(DDE:)     Положение маркера за пределами диапазона")]
            Err555 = 555,

            [StringValue("(DDE:)     Ширина импульса должна быть 1-255")]
            Err556 = 556,

            [StringValue("(DDE:)     Невозможно сейчас установить диапазон")]
            Err557 = 557,

            [StringValue("(DDE:)     Не диапазон для этой функции")]
            Err558 = 558,

            [StringValue("(DDE:)     Невозможно сейчас задать импульс TD")]
            Err559 = 559,

            [StringValue("(DDE:)     ZERO_MEAS только для измерения C или PRES")]
            Err560 = 560,

            [StringValue("(DDE:FR)   Это требует опции -SC")]
            Err561 = 561,

            [StringValue("(DDE:FR)   Это требует опции -SC600")]
            Err562 = 562,

            [StringValue("(DDE:)     Временной предел должен быть в диапазоне 1с - 60 с")]
            Err563 = 563,

            [StringValue("(DDE:)     Невозможно задать опорную фазу в настоящий момент")]
            Err564 = 564,

            [StringValue("(DDE:)     Измерение ZERO_MEAS недействительно")]
            Err565 = 565,

            [StringValue("(DDE:)     Невозможно сейчас установить демпфирование")]
            Err566 = 566,

            [StringValue("(DDE:)     Невозможно сейчас включить EXGRD")]
            Err567 = 567,

            [StringValue("(DDE:)     Ведомое устройство не может отправить SYNCOUT")]
            Err568 = 568,

            [StringValue("(DDE:FR)   Это требует опции -SC1100")]
            Err569 = 569,

            [StringValue("(DDE: )    Неверный номер гармоники")]
            Err570 = 570,

            [StringValue("(DDE: )    Неверная амплитуда гармоники")]
            Err571 = 571,

            [StringValue("(DDE: )    Дубликат номера гармоники")]
            Err572 = 572,

            [StringValue("(DDE: )    Мультивибратор работает только в РАБОЧЕМ режиме и установленными значениями")]
            Err573 = 573,

            [StringValue("(DDE: )    Не более 15 гармоник в __многоканальной волне")]
            Err574 = 574,

            [StringValue("(DDE:)     Фликер только для прямоугольного или синусоидального сигнала")]
            Err575 = 575,

            [StringValue("(DDE: )    Опция -PQ не установлена")]
            Err576 = 576,

            [StringValue("(DDE: )    Должен быть для этого в режиме PQ")]
            Err577 = 577,

            [StringValue("(DDE:)     Установить это в настоящее время невозможно")]
            Err578 = 578,

            [StringValue("(EXE: R)   Слишком большой параметр")]
            Err579 = 579,

            [StringValue("(DDE:FR D) Внешний процессор в состоянии ожидания")]
            Err600 = 600,

            [StringValue("(DDE:FR )  Сбой проверки оперативной памяти при включении")]
            Err601 = 601,

            [StringValue("(DDE:FR )  Сбой проверки GPIB (универсальной интерфейсной шины) при включении")]
            Err602 = 602,

            [StringValue("(DDE: R )  Сбой записи в энергонезависимую память")]
            Err700 = 700,

            [StringValue("(DDE: R )  Отказ энергонезависимой памяти")]
            Err701 = 701,

            [StringValue("(DDE: FR ) Отказ энергонезависимой памяти и загрузка стандартных значений")]
            Err702 = 702,

            [StringValue("(DDE: FR ) Энергонезависимая память устарела. Загрузка стандартных значений")]
            Err703 = 703,

            [StringValue("(DDE:FR D) Ошибка четности последовательного сигнала")]
            Err800 = 800,

            [StringValue("(DDE:FR )  Ошибка кадрирования последовательного сигнала %s")]
            Err801 = 801,

            [StringValue("(DDE:FR D) Ошибка переполнения последовательного сигнала %s")]
            Err802 = 802,

            [StringValue("(DDE:FR D) Выпадание символов при последовательной передаче %s")]
            Err803 = 803,

            [StringValue("(DDE:FR D) Время ожидания отчета - прервано")]
            Err900 = 900,

            [StringValue("(DDE:FR )  Отказ последовательности во время диагностики")]
            Err1000 = 1000,

            [StringValue("(DDE:FR )  Слишком длинное имя последовательности")]
            Err1200 = 1200,

            [StringValue("(DDE:FR )  Таблица последовательностей ОЗУ заполнена")]
            Err1201 = 1201,

            [StringValue("(DDE:FR )  Таблица имен последовательностей заполнена")]
            Err1202 = 1202,

            [StringValue("(CME: R )  Неверный синтаксис")]
            Err1300 = 1300,

            [StringValue("(CME: R )  Неизвестная команда")]
            Err1301 = 1301,

            [StringValue("(CME: R )  Неверное число параметров")]
            Err1302 = 1302,

            [StringValue("(CME: R )  Неверное ключевое слово")]
            Err1303 = 1303,

            [StringValue("(CME: R )  Неверный тип параметра")]
            Err1304 = 1304,

            [StringValue("(CME: R )  Неверные единицы параметра")]
            Err1305 = 1305,

            [StringValue("(EXE: R )  Неверное значение параметра")]
            Err1306 = 1306,

            [StringValue("(QYE: R )  Зависание ввода/вывода 488.2")]
            Err1307 = 1307,

            [StringValue("(QYE: R )  Прерывание запроса 488.2")]
            Err1308 = 1308,

            [StringValue("(QYE: R )  Незавершенная команда 488.2")]
            Err1309 = 1309,

            [StringValue("(QYE: R )  Запрос после неопределенного ответа 488.2")]
            Err1310 = 1310,

            [StringValue("(DDE: R )  Отказ по интерфейсу GPIB универсальной интерфейсной шины")]
            Err1311 = 1311,

            [StringValue("(DDE: R )  Отказ по последовательному интерфейсу")]
            Err1312 = 1312,

            [StringValue("(DDE: R )  Только для сервисного обслуживания")]
            Err1313 = 1313,

            [StringValue("(EXE: R )  Слишком длинный параметр")]
            Err1314 = 1314,

            [StringValue("(CME: R )  Отказ мультивибратора устройства")]
            Err1315 = 1315,

            [StringValue("(EXE: R )  Рекурсия мультивибратора устройства")]
            Err1316 = 1316,

            [StringValue("(CME: R )  Переполнение буфера последовательного интерфейса")]
            Err1317 = 1317,

            [StringValue("(CME: R )  Неверное число")]
            Err1318 = 1318,

            [StringValue("(EXE: R )  Отказ команды обслуживания")]
            Err1319 = 1319,

            [StringValue("(CME: R )  Неверное двоичное число")]
            Err1320 = 1320,

            [StringValue("(CME: R )  Неверный двоичный блок")]
            Err1321 = 1321,

            [StringValue("(CME: R )  Неверное ключевое слово")]
            Err1322 = 1322,

            [StringValue("(CME: R )  Неверное десятичное число")]
            Err1323 = 1323,

            [StringValue("(CME: R )  Множитель экспоненты слишком большой")]
            Err1324 = 1324,

            [StringValue("(CME: R )  Неверный шестнадцатеричный блок")]
            Err1325 = 1325,

            [StringValue("(CME: R )  Неверное шестнадцатеричное число")]
            Err1326 = 1326,

            [StringValue("(CME: R )  Неверное восьмеричное число")]
            Err1328 = 1328,

            [StringValue("(CME: R )  Слишком много символов")]
            Err1329 = 1329,

            [StringValue("(CME: R )  Неверная строка")]
            Err1330 = 1330,

            [StringValue("(DDE: R )  Рабочий режим запрещен при отложенной ошибке")]
            Err1331 = 1331,

            [StringValue("(CME:FR )  Невозможно сейчас изменить настройки испытываемого устройства")]
            Err1332 = 1332,

            [StringValue("(DDE:FRS)  Чрезмерное напряжение источника питания")]
            Err1500 = 1500,

            [StringValue("(DDE:FRS)  Шунт амперметра перегружен или недогружен")]
            Err1501 = 1501,

            [StringValue("(DDE:FRS)  Превышение теплового предела по току (А)")]
            Err1502 = 1502,

            [StringValue("(DDE:FRS)  Превышение предела по току на выходе")]
            Err1503 = 1503,

            [StringValue("(DDE:FRS)  Превышение предела по напряжению или току на входе")]
            Err1504 = 1504,

            [StringValue("(DDE:FRS)  Счетчик VDAC вне диапазона")]
            Err1505 = 1505,

            [StringValue("(DDE:FRS)  Счетчик IDAC вне диапазона")]
            Err1506 = 1506,

            [StringValue("(DDE:FRS)  Счетчик ЦАП шкалы переменного тока вне диапазона")]
            Err1507 = 1507,

            [StringValue("(DDE:FRS)  Счетчик ЦАП шкалы постоянного тока вне диапазона")]
            Err1508 = 1508,

            [StringValue("(DDE:FRS)  Счетчик ЦАП частоты вне диапазона")]
            Err1509 = 1509,

            [StringValue("(DDE:FRS)  Счетчик IDAC (DC OFFSET) вне диапазона")]
            Err1510 = 1510,

            [StringValue("(DDE:FRS)  Счетчик ZDAC вне диапазона")]
            Err1511 = 1511,

            [StringValue("(DDE:FRS)  Не удается считать регистр внешнего генератора")]
            Err1512 = 1512,

            [StringValue("(DDE:FRS)  Слишком высокая частота внешнего генератора")]
            Err1513 = 1513,

            [StringValue("(DDE:FRS)  Слишком низкая частота внешнего генератора")]
            Err1514 = 1514,

            [StringValue("(DDE:FR D) 	Не удается загрузить форму волны для режима осциллографа")]
            Err1515 = 1515,

            [StringValue("(DDE:FRS ) 	Пиковые или средние амплитуды слишком велики")]
            Err1516 = 1516,

            [StringValue("(DDE:FR D) 	Ошибка перехода OPM")]
            Err1600 = 1600,

            [StringValue("(DDE:FR D) 	Ошибка измерения термопары")]
            Err1601 = 1601,

            [StringValue("(DDE:FR D) 	Ошибка измерения Z")]
            Err1602 = 1602,

            [StringValue("(DDE:FR)  	Неизвестная ошибка %d")]
            Err65535 = 65535
        }
    }
}