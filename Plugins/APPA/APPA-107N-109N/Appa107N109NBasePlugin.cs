using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AP.Utils.Data;
using ASMC.Data.Model;
using ASMC.Data.Model.Interface;
using ASMC.Devices;
using ASMC.Devices.IEEE;
using ASMC.Devices.IEEE.Fluke.Calibrator;
using ASMC.Devices.Port.APPA;
using DevExpress.Mvvm;
using NLog;
using IDevice = ASMC.Data.Model.IDevice;

namespace APPA_107N_109N
{
    public class Appa107N109NBasePlugin : Program

    {
        public Appa107N109NBasePlugin(ServicePack service) : base(service)
        {
            Grsi = "20085-11";
        }
    }

    public class Operation : OperationMetrControlBase
    {
        //определяет какие типы проверок доступны для СИ: поверка первичная/переодическая, калибровка, adjustment.
        public Operation()
        {
            //это операция первичной поверки
            //UserItemOperationPrimaryVerf = new OpertionFirsVerf();
            //здесь периодическая поверка, но набор операций такой же
            UserItemOperationPeriodicVerf = UserItemOperationPrimaryVerf;
        }
    }

    public abstract class OpertionFirsVerf : ASMC.Data.Model.Operation
    {
        public OpertionFirsVerf(ServicePack servicePack) : base(servicePack)
        {
            //Необходимые устройства
            ControlDevices = new IDevice[]
                {new Device {Name = new[] {"5522A"}, Description = "Многофунциональный калибратор"}};
            TestDevices = new IDevice[]
                {new Device {Name = new[] {"APPA-107N"}, Description = "Цифровой портативный мультиметр"}};

            Accessories = new[]
            {
                "Интерфейсный кабель для клибратора (GPIB или COM порт)",
                "Кабель banana - banana 2 шт.",
                "Интерфейсный кабель для прибора APPA-107N/APPA-109N USB-COM инфракрасный."
            };

            DocumentName = "APPA_107N_109N";
        }

        #region Methods

        public override void FindDivice()
        {
            throw new NotImplementedException();
        }

        public override void RefreshDevice()
        {
            AddresDevice = IeeeBase.AllStringConnect;
        }

        #endregion
    }

    public abstract class Oper1VisualTest : ParagraphBase, IUserItemOperation<bool>
    {
        public Oper1VisualTest(IUserItemOperation userItemOperation) : base(userItemOperation)
        {
            Name = "Внешний осмотр";
            DataRow = new List<IBasicOperation<bool>>();
        }

        #region Methods

        /// <inheritdoc />
        protected override DataTable FillData()
        {
            var data = new DataTable();
            data.Columns.Add("Результат внешнего осмотра");
            var dataRow = data.NewRow();
            var dds = DataRow[0] as BasicOperation<bool>;
            // ReSharper disable once PossibleNullReferenceException
            dataRow[0] = dds.Getting;
            data.Rows.Add(dataRow);
            return data;
        }

        protected override void InitWork()
        {
            var operation = new BasicOperation<bool>();
            operation.Expected = true;
            operation.IsGood = () => operation.Getting == operation.Expected;
            operation.InitWork = () =>
            {
                var service = UserItemOperation.ServicePack.QuestionText;
                service.Title = "Внешний осмотр";
                service.Entity = (Document: "B5VisualTestText", "");
                service.Show();
                operation.Getting = true;
                return Task.CompletedTask;
            };

            operation.CompliteWork = () => Task.FromResult(operation.IsGood());
            DataRow.Add(operation);
        }

        #endregion

        public List<IBasicOperation<bool>> DataRow { get; set; }

        /// <inheritdoc />
        public override async Task StartSinglWork(CancellationToken token, Guid guid)
        {
            var a = DataRow.FirstOrDefault(q => Equals(q.Guid, guid));
            if (a != null)
                await a.WorkAsync(token);
        }

        /// <inheritdoc />
        public override async Task StartWork(CancellationToken token)
        {
            InitWork();
            foreach (var dr in DataRow)
                await dr.WorkAsync(token);
        }
    }

    public abstract class Oper2Oprobovanie : ParagraphBase, IUserItemOperation<bool>
    {
        public Oper2Oprobovanie(IUserItemOperation userItemOperation) : base(userItemOperation)
        {
            Name = "Опробование";
            DataRow = new List<IBasicOperation<bool>>();
        }

        #region Methods

        protected override DataTable FillData()
        {
            var data = new DataTable();
            data.Columns.Add("Результат опробования");
            var dataRow = data.NewRow();
            var dds = DataRow[0] as BasicOperationVerefication<bool>;
            dataRow[0] = dds.Getting;
            data.Rows.Add(dataRow);
            return data;
        }

        #endregion

        public List<IBasicOperation<bool>> DataRow { get; set; }

        public override Task StartSinglWork(CancellationToken token, Guid guid)
        {
            throw new NotImplementedException();
        }

        public override async Task StartWork(CancellationToken cancellationToken)

        {
            var bo = new BasicOperation<bool> {Expected = true};
            //bo.IsGood = s => bo.Getting;

            DataRow.Add(bo);
        }
    }

    //////////////////////////////******DCV*******///////////////////////////////

    #region DCV

    public class Oper3DcvMeasureBase : ParagraphBase, IUserItemOperation<decimal>
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        #region Fields

        //множители для пределов.
        public decimal BaseMultipliers;

        /// <summary>
        /// Разарешение пределеа измерения (последний значащий разряд)
        /// </summary>
        protected AcVPoint RangeResolution;

        /// <summary>
        /// Массив поверяемых точек напряжения.
        /// </summary>
        protected AcVPoint[] VoltPoint;

        /// <summary>
        /// Процент от измеряемой точки для расчета погрешности (уже переведе в абсолютные единицы).
        /// </summary>
        public decimal BaseTolCoeff = (decimal) 0.0006;

        /// <summary>
        /// довесок к формуле погрешности- число единиц младшего разряда
        /// </summary>
        public int EdMlRaz = 10; //значение для пределов свыше 200 мВ

        #endregion

        #region Property

        /// <summary>
        /// Код предела измерения на приборе
        /// </summary>
        public Mult107_109N.RangeCode OperationDcRangeCode { get; protected set; }

        /// <summary>
        /// Предел измерения поверяемого прибора, необходимый для работы
        /// </summary>
        public Mult107_109N.RangeNominal OperationDcRangeNominal { get; protected set; }

        /// <summary>
        /// Режим операции измерения прибора
        /// </summary>
        public Mult107_109N.MeasureMode OperMeasureMode { get; protected set; }

        /// <summary>
        /// Множитель единицы измерения текущей операции
        /// </summary>
        public Multipliers OpMultipliers { get; protected set; }

        //контрлируемый прибор
        protected Mult107_109N appa107N { get; set; }

        //эталон
        protected Calib5522A flkCalib5522A { get; set; }

        #endregion

        public Oper3DcvMeasureBase(IUserItemOperation userItemOperation) : base(userItemOperation)
        {
            Name = "Определение погрешности измерения постоянного напряжения";
            OperMeasureMode = Mult107_109N.MeasureMode.DCV;
            OpMultipliers = Multipliers.None;
            OperationDcRangeCode = Mult107_109N.RangeCode.Range1Manual;
            OperationDcRangeNominal = Mult107_109N.RangeNominal.RangeNone;

            DataRow = new List<IBasicOperation<decimal>>();
            Sheme = ShemeTemplateDefault.TemplateSheme;
            OpMultipliers = Multipliers.None;
        }

        #region Methods

        protected override DataTable FillData()
        {
            return null;
        }

        protected override void InitWork()
        {
            if (appa107N == null || flkCalib5522A == null) return;

            DataRow.Clear();

            foreach (var currPoint in VoltPoint)
            {
                var operation = new BasicOperationVerefication<decimal>();
                operation.InitWork = async () =>
                {
                    try
                    {
                        appa107N.StringConnection = GetStringConnect(appa107N);
                        flkCalib5522A.StringConnection = GetStringConnect(flkCalib5522A);

                        flkCalib5522A.Out.SetOutput(CalibrMain.COut.State.Off);

                        var testMeasureModde = appa107N.GetMeasureMode;
                        while (OperMeasureMode != appa107N.GetMeasureMode)
                            UserItemOperation.ServicePack.MessageBox
                                             .Show($"Установите режим измерения {OperMeasureMode}",
                                                   "Указание оператору", MessageButton.OK,
                                                   MessageIcon.Information,
                                                   MessageResult.OK);

                        var testRangeNominal = appa107N.GetRangeNominal;
                        while (OperationDcRangeNominal != appa107N.GetRangeNominal)
                        {
                            var countPushRangeButton =
                                appa107N.GetRangeSwitchMode != Mult107_109N.RangeSwitchMode.Auto ? 1 : 0;

                            if (OpMultipliers == Multipliers.Mili)
                            {
                                UserItemOperation.ServicePack.MessageBox
                                                 .Show($"Текущий предел измерения прибора {appa107N.GetRangeNominal.GetStringValue()}\n Необходимо установить предел {OperationDcRangeNominal.GetStringValue()} " +
                                                       $"Нажмите на приборе клавишу Range {countPushRangeButton + 1} раз.",
                                                       "Указание оператору", MessageButton.OK, MessageIcon.Information,
                                                       MessageResult.OK);
                            }
                            else
                            {
                                var curRange = (int) appa107N.GetRangeCode & 128;
                                var targetRange = (int) OperationDcRangeCode & 128;
                                countPushRangeButton = countPushRangeButton + 4 - curRange +
                                                       (targetRange < curRange ? curRange : 0);

                                UserItemOperation.ServicePack.MessageBox
                                                 .Show($"Текущий предел измерения прибора {appa107N.GetRangeNominal.GetStringValue()}\n Необходимо установить предел {OperationDcRangeNominal.GetStringValue()} " +
                                                       $"Нажмите на приборе клавишу Range {countPushRangeButton} раз.",
                                                       "Указание оператору", MessageButton.OK, MessageIcon.Information,
                                                       MessageResult.OK);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Logger.Error(e);
                    }
                };
                operation.BodyWork = () =>
                {
                    try
                    {
                        flkCalib5522A.Out.Set.Voltage.Dc.SetValue(currPoint._volt._nominalVal);
                        flkCalib5522A.Out.SetOutput(CalibrMain.COut.State.On);
                        Thread.Sleep(2000);
                        //измеряем
                        var measurePoint = (decimal) appa107N.GetSingleValue();

                        flkCalib5522A.Out.SetOutput(CalibrMain.COut.State.Off);

                        operation.Getting = measurePoint;
                        operation.Expected = currPoint._volt._nominalVal /
                                             (decimal) currPoint._volt._multipliersUnit.GetDoubleValue();
                        //расчет погрешности для конкретной точки предела измерения
                        operation.ErrorCalculation = (inA, inB) =>
                        {
                            decimal result = BaseTolCoeff * operation.Expected + EdMlRaz * RangeResolution._volt._nominalVal *
                            (decimal) (RangeResolution._volt._multipliersUnit.GetDoubleValue() /
                                       currPoint._volt._multipliersUnit.GetDoubleValue());
                            int mantisa =
                                AP.Math.MathStatistics.GetMantissa((decimal)(RangeResolution
                                                                            ._volt._multipliersUnit.GetDoubleValue() /
                                                                             currPoint._volt._multipliersUnit
                                                                                     .GetDoubleValue()));
                            AP.Math.MathStatistics.Round(ref result, mantisa, false);
                            return result;
                        };
                            
                        operation.LowerTolerance = operation.Expected - operation.Error;
                        operation.UpperTolerance = operation.Expected + operation.Error;
                        operation.IsGood = () => (operation.Getting < operation.UpperTolerance) &
                                                 (operation.Getting > operation.LowerTolerance);
                    }
                    catch (Exception e)
                    {
                        Logger.Error(e);
                        throw;
                    }
                };
                operation.CompliteWork = () =>
                {
                    if (!operation.IsGood())
                    {
                        var answer =
                            UserItemOperation.ServicePack.MessageBox.Show(operation +
                                                                          $"\nФАКТИЧЕСКАЯ погрешность {operation.Expected - operation.Getting}\n\n" +
                                                                          "Повторить измерение этой точки?",
                                                                          "Информация по текущему измерению",
                                                                          MessageButton.YesNo, MessageIcon.Question,
                                                                          MessageResult.Yes);

                        if (answer == MessageResult.No) return Task.FromResult(true);
                    }

                    return Task.FromResult(operation.IsGood());
                };
                DataRow.Add(DataRow.IndexOf(operation) == -1
                                ? operation
                                : (BasicOperationVerefication<decimal>) operation.Clone());
            }
        }

        #endregion

        public List<IBasicOperation<decimal>> DataRow { get; set; }

        public override async Task StartSinglWork(CancellationToken token, Guid guid)
        {
            var a = DataRow.FirstOrDefault(q => Equals(q.Guid, guid));
            if (a != null)
                await a.WorkAsync(token);
        }

        public override async Task StartWork(CancellationToken token)
        {
            InitWork();
            foreach (var doThisPoint in DataRow) await doThisPoint.WorkAsync(token);
        }
    }

    public class Oper3_1DC_2V_Measure : Oper3DcvMeasureBase
    {
        public Oper3_1DC_2V_Measure(Mult107_109N.RangeNominal inRangeNominal, IUserItemOperation userItemOperation) :
            base(userItemOperation)
        {
            OperationDcRangeCode = Mult107_109N.RangeCode.Range1Manual;
            OperationDcRangeNominal = inRangeNominal;
            RangeResolution = new AcVPoint(100, Multipliers.Micro);
            Name = OperationDcRangeNominal.GetStringValue();
            OpMultipliers = Multipliers.None;

            BaseMultipliers = 100;
            VoltPoint = new AcVPoint[6];
            VoltPoint[0] = new AcVPoint((decimal) 0.004 * BaseMultipliers, OpMultipliers);
            VoltPoint[1] = new AcVPoint((decimal) 0.008 * BaseMultipliers, OpMultipliers);
            VoltPoint[2] = new AcVPoint((decimal) 0.012 * BaseMultipliers, OpMultipliers);
            VoltPoint[3] = new AcVPoint((decimal) 0.016 * BaseMultipliers, OpMultipliers);
            VoltPoint[4] = new AcVPoint((decimal) 0.018 * BaseMultipliers, OpMultipliers);
            VoltPoint[5] = new AcVPoint((decimal) -0.018 * BaseMultipliers, OpMultipliers);
        }
    }

    public class Oper3_1DC_20V_Measure : Oper3DcvMeasureBase
    {
        public Oper3_1DC_20V_Measure(Mult107_109N.RangeNominal inRangeNominal, IUserItemOperation userItemOperation) :
            base(userItemOperation)
        {
            OperationDcRangeCode = Mult107_109N.RangeCode.Range2Manual;
            OperationDcRangeNominal = inRangeNominal;
            RangeResolution = new AcVPoint(1, Multipliers.Mili);
            Name = OperationDcRangeNominal.GetStringValue();
            OpMultipliers = Multipliers.None;

            BaseMultipliers = 1000;
            VoltPoint = new AcVPoint[6];
            VoltPoint[0] = new AcVPoint((decimal) 0.004 * BaseMultipliers, OpMultipliers);
            VoltPoint[1] = new AcVPoint((decimal) 0.008 * BaseMultipliers, OpMultipliers);
            VoltPoint[2] = new AcVPoint((decimal) 0.012 * BaseMultipliers, OpMultipliers);
            VoltPoint[3] = new AcVPoint((decimal) 0.016 * BaseMultipliers, OpMultipliers);
            VoltPoint[4] = new AcVPoint((decimal) 0.018 * BaseMultipliers, OpMultipliers);
            VoltPoint[5] = new AcVPoint((decimal) -0.018 * BaseMultipliers, OpMultipliers);
        }
    }

    public class Oper3_1DC_200V_Measure : Oper3DcvMeasureBase
    {
        public Oper3_1DC_200V_Measure(Mult107_109N.RangeNominal inRangeNominal, IUserItemOperation userItemOperation) :
            base(userItemOperation)
        {
            OperationDcRangeCode = Mult107_109N.RangeCode.Range3Manual;
            OperationDcRangeNominal = inRangeNominal;
            RangeResolution = new AcVPoint(10, Multipliers.Mili);
            Name = OperationDcRangeNominal.GetStringValue();
            OpMultipliers = Multipliers.None;

            BaseMultipliers = 10000;
            VoltPoint = new AcVPoint[6];
            VoltPoint[0] = new AcVPoint((decimal) 0.004 * BaseMultipliers, OpMultipliers);
            VoltPoint[1] = new AcVPoint((decimal) 0.008 * BaseMultipliers, OpMultipliers);
            VoltPoint[2] = new AcVPoint((decimal) 0.012 * BaseMultipliers, OpMultipliers);
            VoltPoint[3] = new AcVPoint((decimal) 0.016 * BaseMultipliers, OpMultipliers);
            VoltPoint[4] = new AcVPoint((decimal) 0.018 * BaseMultipliers, OpMultipliers);
            VoltPoint[5] = new AcVPoint((decimal) -0.018 * BaseMultipliers, OpMultipliers);
        }
    }

    public class Oper3_1DC_1000V_Measure : Oper3DcvMeasureBase
    {
        public Oper3_1DC_1000V_Measure(Mult107_109N.RangeNominal inRangeNominal, IUserItemOperation userItemOperation) :
            base(userItemOperation)
        {
            OperationDcRangeCode = Mult107_109N.RangeCode.Range4Manual;
            OperationDcRangeNominal = inRangeNominal;
            RangeResolution = new AcVPoint(100, Multipliers.Mili);
            Name = OperationDcRangeNominal.GetStringValue();
            OpMultipliers = Multipliers.None;

            BaseMultipliers = 1;
            VoltPoint = new AcVPoint[6];
            VoltPoint[0] = new AcVPoint(100 * BaseMultipliers, OpMultipliers);
            VoltPoint[1] = new AcVPoint(200 * BaseMultipliers, OpMultipliers);
            VoltPoint[2] = new AcVPoint(400 * BaseMultipliers, OpMultipliers);
            VoltPoint[3] = new AcVPoint(700 * BaseMultipliers, OpMultipliers);
            VoltPoint[4] = new AcVPoint(900 * BaseMultipliers, OpMultipliers);
            VoltPoint[5] = new AcVPoint(-900 * BaseMultipliers, OpMultipliers);
        }
    }

    public class Oper3_1DC_20mV_Measure : Oper3DcvMeasureBase
    {
        public Oper3_1DC_20mV_Measure(Mult107_109N.RangeNominal inRangeNominal, IUserItemOperation userItemOperation) :
            base(userItemOperation)
        {
            OperationDcRangeCode = Mult107_109N.RangeCode.Range1Manual;
            OperationDcRangeNominal = inRangeNominal;
            RangeResolution = new AcVPoint(1, Multipliers.Micro);
            Name = OperationDcRangeNominal.GetStringValue();
            OpMultipliers = Multipliers.Mili;

            RangeResolution = new AcVPoint(1, Multipliers.Micro);

            BaseMultipliers = 1;
            VoltPoint = new AcVPoint[6];
            VoltPoint[0] = new AcVPoint((decimal) 0.004 * BaseMultipliers, OpMultipliers);
            VoltPoint[1] = new AcVPoint((decimal) 0.008 * BaseMultipliers, OpMultipliers);
            VoltPoint[2] = new AcVPoint((decimal) 0.012 * BaseMultipliers, OpMultipliers);
            VoltPoint[3] = new AcVPoint((decimal) 0.016 * BaseMultipliers, OpMultipliers);
            VoltPoint[4] = new AcVPoint((decimal) 0.018 * BaseMultipliers, OpMultipliers);
            VoltPoint[5] = new AcVPoint((decimal) -0.018 * BaseMultipliers, OpMultipliers);
        }
    }

    public class Oper3_1DC_200mV_Measure : Oper3DcvMeasureBase
    {
        public Oper3_1DC_200mV_Measure(Mult107_109N.RangeNominal inRangeNominal, IUserItemOperation userItemOperation) :
            base(userItemOperation)
        {
            OperationDcRangeCode = Mult107_109N.RangeCode.Range1Manual;
            OperationDcRangeNominal = inRangeNominal;
            RangeResolution = new AcVPoint(10, Multipliers.Micro);
            Name = OperationDcRangeNominal.GetStringValue();
            OpMultipliers = Multipliers.Mili;

            RangeResolution = new AcVPoint(10, Multipliers.Micro);

            BaseMultipliers = 10;
            VoltPoint = new AcVPoint[6];
            VoltPoint[0] = new AcVPoint((decimal) 0.004 * BaseMultipliers, OpMultipliers);
            VoltPoint[1] = new AcVPoint((decimal) 0.008 * BaseMultipliers, OpMultipliers);
            VoltPoint[2] = new AcVPoint((decimal) 0.012 * BaseMultipliers, OpMultipliers);
            VoltPoint[3] = new AcVPoint((decimal) 0.016 * BaseMultipliers, OpMultipliers);
            VoltPoint[4] = new AcVPoint((decimal) 0.018 * BaseMultipliers, OpMultipliers);
            VoltPoint[5] = new AcVPoint((decimal) -0.018 * BaseMultipliers, OpMultipliers);
        }
    }

    internal static class ShemeTemplateDefault
    {
        public static readonly ShemeImage TemplateSheme;

        static ShemeTemplateDefault()
        {
            TemplateSheme = new ShemeImage
            {
                Description = "Измерительная схема",
                Number = 1,
                FileName = @"appa_10XN_volt_hz_5522A.jpg",
                ExtendedDescription = "Соберите измерительную схему, согласно рисунку"
            };
        }
    }

    #endregion DCV

    //////////////////////////////******ACV*******///////////////////////////////

    #region ACV

    /// <summary>
    /// Класс точки для переменной величины (например переменное напряжение). К примеру нужно задать напряжение и частоту
    /// </summary>
    public class AcPoint
    {
        #region Property

        //множитель единицы
        public Multipliers _multipliersUnit { get; set; }

        //номинал величины
        public decimal _nominalVal { get; set; }

        #endregion
    }

    /// <summary>
    /// Точка для переменного напряжения
    /// </summary>
    public class AcVPoint
    {
        #region Fields

        internal readonly AcPoint _volt = new AcPoint();

        internal AcPoint[] _herz;

        #endregion

        /// <summary>
        /// Конструктор можно использовать для точек с постоянным напряжением (массива частоты нет).
        /// </summary>
        /// <param name = "inNominal">Предел измерения прибора.</param>
        /// <param name = "inMultipliersUnit">Множитель единицы измерения.</param>
        public AcVPoint(decimal inNominal, Multipliers inMultipliersUnit) : this(inNominal, inMultipliersUnit, null)
        {
        }

        /// <summary>
        /// Конструктор для точек переменного напряжения и тока (массив с частотами вложен).
        /// </summary>
        /// <param name = "inNominal">номинал предела измерения.</param>
        /// <param name = "inMultipliersUnit">Множитель единицы измерения.</param>
        /// <param name = "inHerzArr">Массив частот для данной точки.</param>
        public AcVPoint(decimal inNominal, Multipliers inMultipliersUnit, AcPoint[] inHerzArr)
        {
            //номинал напряжения
            _volt._nominalVal = inNominal;
            _volt._multipliersUnit = inMultipliersUnit;
            //вложенный массив с частотами для данной точки
            _herz = inHerzArr;
        }
    }

    public class Oper4AcvMeasureBase : ParagraphBase, IUserItemOperation<decimal>
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        #region Fields

        /// <summary>
        /// Набор частот, характерный для данного предела измерения
        /// </summary>
        protected AcPoint[] HerzVPoint;

        /// <summary>
        /// Множитель для поверяемых точек. (Если точки можно посчитать простым умножением).
        /// </summary>
        protected decimal VoltMultipliers;

        /// <summary>
        /// Итоговый массив поверяемых точек. У каждого номинала напряжения вложены номиналы частот для текущей точки.
        /// </summary>
        public AcVPoint[] VoltPoint;

        #endregion

        #region Property

        /// <summary>
        /// Код предела измерения на приборе
        /// </summary>
        public Mult107_109N.RangeCode OperationAcRangeCode { get; protected set; }

        /// <summary>
        /// Предел измерения поверяемого прибора, необходимый для работы
        /// </summary>
        public Mult107_109N.RangeNominal OperationAcRangeNominal { get; protected set; }

        /// <summary>
        /// Режим операции измерения прибора
        /// </summary>
        public Mult107_109N.MeasureMode OperMeasureMode { get; protected set; }

        /// <summary>
        /// Множитель единицы измерения текущей операции
        /// </summary>
        public Multipliers OpMultipliers { get; protected set; }

        //контрлируемый прибор
        protected Mult107_109N appa107N { get; set; }

        //эталон
        protected Calib5522A flkCalib5522A { get; set; }

        #endregion

        public Oper4AcvMeasureBase(IUserItemOperation userItemOperation) : base(userItemOperation)
        {
            Name = "Определение погрешности измерения переменного напряжения";
            OperMeasureMode = Mult107_109N.MeasureMode.ACV;
            OpMultipliers = Multipliers.None;
            OperationAcRangeCode = Mult107_109N.RangeCode.Range1Manual;
            OperationAcRangeNominal = Mult107_109N.RangeNominal.RangeNone;
            DataRow = new List<IBasicOperation<decimal>>();
        }

        #region Methods

        /// <summary>
        /// Метод выбирает необходимый значения коэффициентов для формулы погрешности, исходя из предела измерения и диапазона
        /// частот.
        /// </summary>
        /// <returns>Результат вычисления.</returns>
        protected void ConstructTooleranceFormula(AcPoint inFreq)
        {
            //разрешение предела измерения должно быть проинициализировано в коснтсрукторе соответсвующего класса

            if ((OperationAcRangeNominal == Mult107_109N.RangeNominal.Range200mV ||
                 OperationAcRangeNominal == Mult107_109N.RangeNominal.Range20mV) &&
                inFreq._multipliersUnit == Multipliers.None)
            {
                EdMlRaz = 80;
                if (inFreq._nominalVal >= 40 && inFreq._nominalVal < 100) BaseTolCoeff = (decimal) 0.007;
                if (inFreq._nominalVal >= 100 && inFreq._nominalVal < 1000) BaseTolCoeff = (decimal) 0.01;
            }

            if (OperationAcRangeNominal == Mult107_109N.RangeNominal.Range2V ||
                OperationAcRangeNominal == Mult107_109N.RangeNominal.Range20V ||
                OperationAcRangeNominal == Mult107_109N.RangeNominal.Range200V)
            {
                if (inFreq._multipliersUnit == Multipliers.None)
                {
                    EdMlRaz = 50;
                    if (inFreq._nominalVal >= 40 && inFreq._nominalVal < 100) BaseTolCoeff = (decimal) 0.007;
                    if (inFreq._nominalVal >= 100 && inFreq._nominalVal < 1000) BaseTolCoeff = (decimal) 0.01;
                }

                if (inFreq._multipliersUnit == Multipliers.Kilo)
                {
                    if (inFreq._nominalVal >= 1 && inFreq._nominalVal < 10)
                    {
                        BaseTolCoeff = (decimal) 0.02;
                        EdMlRaz = 60;
                    }

                    if (inFreq._nominalVal >= 10 && inFreq._nominalVal < 20)
                    {
                        BaseTolCoeff = (decimal) 0.03;
                        EdMlRaz = 70;
                    }

                    if (inFreq._nominalVal >= 20 && inFreq._nominalVal < 50)
                    {
                        BaseTolCoeff = (decimal) 0.05;
                        EdMlRaz = 80;
                    }

                    if (inFreq._nominalVal >= 50 && inFreq._nominalVal <= 100)
                    {
                        BaseTolCoeff = (decimal) 0.1;
                        EdMlRaz = 100;
                    }
                }
            }

            if (OperationAcRangeNominal == Mult107_109N.RangeNominal.Range750V)
            {
                EdMlRaz = 50;
                if (inFreq._nominalVal >= 40 && inFreq._nominalVal < 100) BaseTolCoeff = (decimal) 0.007;
                if (inFreq._nominalVal >= 100 && inFreq._nominalVal < 1000) BaseTolCoeff = (decimal) 0.01;
            }
        }

        protected override DataTable FillData()
        {
            return null;
        }

        protected override void InitWork()
        {
            if (flkCalib5522A == null || appa107N == null) return;
            DataRow.Clear();

            foreach (var volPoint in VoltPoint)
            foreach (var freqPoint in volPoint._herz)
            {
                var operation = new BasicOperationVerefication<decimal>();
                operation.InitWork = async () =>
                {
                    try
                    {
                        appa107N.StringConnection = GetStringConnect(appa107N);
                        flkCalib5522A.StringConnection = GetStringConnect(flkCalib5522A);

                        flkCalib5522A.Out.SetOutput(CalibrMain.COut.State.Off);

                        var testMeasureModde = appa107N.GetMeasureMode;
                        while (OperMeasureMode != appa107N.GetMeasureMode)
                            UserItemOperation.ServicePack.MessageBox
                                             .Show($"Установите режим измерения {OperMeasureMode}",
                                                   "Указание оператору", MessageButton.OK,
                                                   MessageIcon.Information,
                                                   MessageResult.OK);

                        var testRangeNominal = appa107N.GetRangeNominal;
                        while (OperationAcRangeNominal != appa107N.GetRangeNominal)
                        {
                            var countPushRangeButton =
                                appa107N.GetRangeSwitchMode != Mult107_109N.RangeSwitchMode.Auto ? 1 : 0;

                            if (OpMultipliers == Multipliers.Mili)
                            {
                                UserItemOperation.ServicePack.MessageBox
                                                 .Show($"Текущий предел измерения прибора {appa107N.GetRangeNominal.GetStringValue()}\n Необходимо установить предел {OperationAcRangeNominal.GetStringValue()} " +
                                                       $"Нажмите на приборе клавишу Range {countPushRangeButton + 1} раз.",
                                                       "Указание оператору", MessageButton.OK, MessageIcon.Information,
                                                       MessageResult.OK);
                            }
                            else
                            {
                                var curRange = (int) appa107N.GetRangeCode & 128;
                                var targetRange = (int) OperationAcRangeCode & 128;
                                countPushRangeButton = countPushRangeButton + 4 - curRange +
                                                       (targetRange < curRange ? curRange : 0);

                                UserItemOperation.ServicePack.MessageBox
                                                 .Show($"Текущий предел измерения прибора {appa107N.GetRangeNominal.GetStringValue()}\n Необходимо установить предел {OperationAcRangeNominal.GetStringValue()} " +
                                                       $"Нажмите на приборе клавишу Range {countPushRangeButton} раз.",
                                                       "Указание оператору", MessageButton.OK, MessageIcon.Information,
                                                       MessageResult.OK);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Logger.Error(e);
                    }
                };
                operation.BodyWork = () =>
                {
                    flkCalib5522A.Out.Set.Voltage.Ac.SetValue(volPoint._volt._nominalVal, freqPoint._nominalVal,
                                                              volPoint._volt._multipliersUnit,
                                                              freqPoint._multipliersUnit);
                    flkCalib5522A.Out.SetOutput(CalibrMain.COut.State.On);
                    Thread.Sleep(2000);
                    //измеряем
                    var measurePoint = (decimal) appa107N.GetSingleValue();
                    operation.Getting = measurePoint;

                    operation.Expected = volPoint._volt._nominalVal /
                                         (decimal) volPoint._volt._multipliersUnit.GetDoubleValue();

                    //расчет погрешности для конкретной точки предела измерения
                    ConstructTooleranceFormula(freqPoint); // функция подбирает коэффициенты для формулы погрешности
                    operation.ErrorCalculation = (inA, inB) =>
                    {
                        decimal result = BaseTolCoeff * operation.Expected + EdMlRaz * RangeResolution._volt._nominalVal *
                                         (decimal) (RangeResolution._volt._multipliersUnit.GetDoubleValue() /
                                                    volPoint._volt._multipliersUnit.GetDoubleValue());
                        int mantisa =
                            AP.Math.MathStatistics.GetMantissa((decimal) (RangeResolution
                                                                         ._volt._multipliersUnit.GetDoubleValue() /
                                                                          volPoint._volt._multipliersUnit
                                                                                  .GetDoubleValue()));
                        AP.Math.MathStatistics.Round(ref result, mantisa,false);
                        return result;
                    };
                       
                    operation.LowerTolerance = operation.Expected - operation.Error;
                    operation.UpperTolerance = operation.Expected + operation.Error;
                    operation.IsGood = () => (operation.Getting < operation.UpperTolerance) &
                                             (operation.Getting > operation.LowerTolerance);
                };
                operation.CompliteWork = () =>
                {
                    if (!operation.IsGood())
                    {
                        var answer =
                            UserItemOperation.ServicePack.MessageBox.Show(operation +
                                                                          $"\nФАКТИЧЕСКАЯ погрешность {operation.Expected - operation.Getting}\n\n" +
                                                                          "Повторить измерение этой точки?",
                                                                          "Информация по текущему измерению",
                                                                          MessageButton.YesNo, MessageIcon.Question,
                                                                          MessageResult.Yes);

                        if (answer == MessageResult.No) return Task.FromResult(true);
                    }

                    return Task.FromResult(operation.IsGood());
                };
            }
        }

        #endregion

        public List<IBasicOperation<decimal>> DataRow { get; set; }

        public override Task StartSinglWork(CancellationToken token, Guid guid)
        {
            return null;
        }

        public override async Task StartWork(CancellationToken cancellationToken)

        {
            throw new NotImplementedException();
        }

        #region TolleranceFormula

        /// <summary>
        /// Процент от измеряемой точки для расчета погрешности (уже переведе в абсолютные единицы).
        /// </summary>
        public decimal BaseTolCoeff;

        /// <summary>
        /// довесок к формуле погрешности- число единиц младшего разряда
        /// </summary>
        public int EdMlRaz;

        /// <summary>
        /// Разарешение пределеа измерения (последний значащий разряд)
        /// </summary>
        protected AcVPoint RangeResolution;

        #endregion TolleranceFormula
    }

    public class Ope4_1_AcV_20mV_Measure : Oper4AcvMeasureBase
    {
        public Ope4_1_AcV_20mV_Measure(Mult107_109N.RangeNominal inRangeNominal, IUserItemOperation userItemOperation) :
            base(userItemOperation)
        {
            OpMultipliers = Multipliers.Mili;
            OperMeasureMode = Mult107_109N.MeasureMode.ACmV;
            OperationAcRangeCode = Mult107_109N.RangeCode.Range1Manual;
            OperationAcRangeNominal = inRangeNominal;
            Name = OperationAcRangeNominal.GetStringValue();
            Sheme = ShemeTemplateDefault.TemplateSheme;

            VoltMultipliers = 1;

            RangeResolution = new AcVPoint(1, Multipliers.Micro);

            HerzVPoint = new AcPoint[2];
            HerzVPoint[0] = new AcPoint();
            HerzVPoint[0]._multipliersUnit = Multipliers.Mili;
            HerzVPoint[0]._nominalVal = 40;

            HerzVPoint[1] = new AcPoint();
            HerzVPoint[1]._multipliersUnit = Multipliers.None;
            HerzVPoint[1]._nominalVal = 1000;

            VoltPoint = new AcVPoint[3];
            VoltPoint[0] = new AcVPoint(4 * VoltMultipliers, OpMultipliers, HerzVPoint);
            VoltPoint[1] = new AcVPoint(10 * VoltMultipliers, OpMultipliers, HerzVPoint);
            VoltPoint[2] = new AcVPoint(18 * VoltMultipliers, OpMultipliers, HerzVPoint);
        }
    }

    public class Ope4_1_AcV_200mV_Measure : Oper4AcvMeasureBase
    {
        public Ope4_1_AcV_200mV_Measure(Mult107_109N.RangeNominal inRangeNominal, IUserItemOperation userItemOperation)
            : base(userItemOperation)
        {
            OpMultipliers = Multipliers.Mili;
            OperMeasureMode = Mult107_109N.MeasureMode.ACmV;
            OperationAcRangeCode = Mult107_109N.RangeCode.Range2Manual;
            OperationAcRangeNominal = inRangeNominal;
            Name = OperationAcRangeNominal.GetStringValue();
            Sheme = ShemeTemplateDefault.TemplateSheme;

            VoltMultipliers = 10;

            RangeResolution = new AcVPoint(10, Multipliers.Micro);

            HerzVPoint = new AcPoint[2];
            HerzVPoint[0] = new AcPoint();
            HerzVPoint[0]._multipliersUnit = Multipliers.None;
            HerzVPoint[0]._nominalVal = 40;

            HerzVPoint[1] = new AcPoint();
            HerzVPoint[1]._multipliersUnit = Multipliers.None;
            HerzVPoint[1]._nominalVal = 1000;

            VoltPoint = new AcVPoint[3];
            VoltPoint[0] = new AcVPoint(4 * VoltMultipliers, OpMultipliers, HerzVPoint);
            VoltPoint[1] = new AcVPoint(10 * VoltMultipliers, OpMultipliers, HerzVPoint);
            VoltPoint[2] = new AcVPoint(18 * VoltMultipliers, OpMultipliers, HerzVPoint);
        }
    }

    public class Ope4_1_AcV_2V_Measure : Oper4AcvMeasureBase
    {
        public Ope4_1_AcV_2V_Measure(Mult107_109N.RangeNominal inRangeNominal, IUserItemOperation userItemOperation) :
            base(userItemOperation)
        {
            OpMultipliers = Multipliers.None;
            OperMeasureMode = Mult107_109N.MeasureMode.ACV;
            OperationAcRangeCode = Mult107_109N.RangeCode.Range1Manual;
            OperationAcRangeNominal = inRangeNominal;
            Name = OperationAcRangeNominal.GetStringValue();
            Sheme = ShemeTemplateDefault.TemplateSheme;

            VoltMultipliers = 1;

            RangeResolution = new AcVPoint((decimal) 0.1, Multipliers.Mili);

            HerzVPoint = new AcPoint[6];
            HerzVPoint[0] = new AcPoint();
            HerzVPoint[1] = new AcPoint();
            HerzVPoint[2] = new AcPoint();
            HerzVPoint[3] = new AcPoint();
            HerzVPoint[4] = new AcPoint();
            HerzVPoint[5] = new AcPoint();

            HerzVPoint[0]._nominalVal = 40 * VoltMultipliers;
            HerzVPoint[0]._multipliersUnit = Multipliers.None;
            HerzVPoint[1]._nominalVal = 1000 * VoltMultipliers;
            HerzVPoint[1]._multipliersUnit = Multipliers.None;
            HerzVPoint[2]._nominalVal = 10 * VoltMultipliers;
            HerzVPoint[2]._multipliersUnit = Multipliers.Kilo;
            HerzVPoint[3]._nominalVal = 20 * VoltMultipliers;
            HerzVPoint[3]._multipliersUnit = Multipliers.Kilo;
            HerzVPoint[4]._nominalVal = 50 * VoltMultipliers;
            HerzVPoint[4]._multipliersUnit = Multipliers.Kilo;
            HerzVPoint[5]._nominalVal = 100 * VoltMultipliers;
            HerzVPoint[5]._multipliersUnit = Multipliers.Kilo;

            VoltPoint = new AcVPoint[3];
            //конкретно для первой точки 0.2 нужны не все частоты, поэтому вырежем только необходимые
            var trimHerzArr = new AcPoint[4];
            Array.Copy(HerzVPoint, trimHerzArr, 4);
            VoltPoint[0] = new AcVPoint((decimal) 0.2, OpMultipliers, trimHerzArr);
            VoltPoint[1] = new AcVPoint(1, OpMultipliers, HerzVPoint);
            VoltPoint[2] = new AcVPoint((decimal) 1.8, OpMultipliers, HerzVPoint);
        }
    }

    public class Ope4_1_AcV_20V_Measure : Oper4AcvMeasureBase
    {
        public Ope4_1_AcV_20V_Measure(Mult107_109N.RangeNominal inRangeNominal, IUserItemOperation userItemOperation) :
            base(userItemOperation)
        {
            OpMultipliers = Multipliers.None;
            OperMeasureMode = Mult107_109N.MeasureMode.ACV;
            OperationAcRangeCode = Mult107_109N.RangeCode.Range2Manual;
            OperationAcRangeNominal = inRangeNominal;
            Name = OperationAcRangeNominal.GetStringValue();
            Sheme = ShemeTemplateDefault.TemplateSheme;

            VoltMultipliers = 10;

            RangeResolution = new AcVPoint(1, Multipliers.Mili);

            HerzVPoint = new AcPoint[6];
            HerzVPoint[0] = new AcPoint();
            HerzVPoint[1] = new AcPoint();
            HerzVPoint[2] = new AcPoint();
            HerzVPoint[3] = new AcPoint();
            HerzVPoint[4] = new AcPoint();
            HerzVPoint[5] = new AcPoint();

            HerzVPoint[0]._nominalVal = 40;
            HerzVPoint[0]._multipliersUnit = Multipliers.None;
            HerzVPoint[1]._nominalVal = 1000;
            HerzVPoint[1]._multipliersUnit = Multipliers.None;
            HerzVPoint[2]._nominalVal = 10;
            HerzVPoint[2]._multipliersUnit = Multipliers.Kilo;
            HerzVPoint[3]._nominalVal = 20;
            HerzVPoint[3]._multipliersUnit = Multipliers.Kilo;
            HerzVPoint[4]._nominalVal = 50;
            HerzVPoint[4]._multipliersUnit = Multipliers.Kilo;
            HerzVPoint[5]._nominalVal = 100;
            HerzVPoint[5]._multipliersUnit = Multipliers.Kilo;

            VoltPoint = new AcVPoint[3];
            //конкретно для первой точки 0.2 нужны не все частоты, поэтому вырежем только необходимые
            var trimHerzArr = new AcPoint[4];
            Array.Copy(HerzVPoint, trimHerzArr, 4);
            VoltPoint[0] = new AcVPoint((decimal) 0.2 * VoltMultipliers, OpMultipliers, trimHerzArr);
            VoltPoint[1] = new AcVPoint(1 * VoltMultipliers, OpMultipliers, HerzVPoint);
            VoltPoint[2] = new AcVPoint((decimal) 1.8 * VoltMultipliers, OpMultipliers, HerzVPoint);
        }
    }

    public class Ope4_1_AcV_200V_Measure : Oper4AcvMeasureBase
    {
        public Ope4_1_AcV_200V_Measure(Mult107_109N.RangeNominal inRangeNominal, IUserItemOperation userItemOperation) :
            base(userItemOperation)
        {
            OpMultipliers = Multipliers.None;
            OperMeasureMode = Mult107_109N.MeasureMode.ACV;
            OperationAcRangeCode = Mult107_109N.RangeCode.Range3Manual;
            OperationAcRangeNominal = inRangeNominal;
            Name = OperationAcRangeNominal.GetStringValue();
            Sheme = ShemeTemplateDefault.TemplateSheme;

            VoltMultipliers = 100;

            RangeResolution = new AcVPoint(10, Multipliers.Mili);

            HerzVPoint = new AcPoint[6];
            HerzVPoint[0] = new AcPoint();
            HerzVPoint[1] = new AcPoint();
            HerzVPoint[2] = new AcPoint();
            HerzVPoint[3] = new AcPoint();
            HerzVPoint[4] = new AcPoint();
            HerzVPoint[5] = new AcPoint();

            HerzVPoint[0]._nominalVal = 40;
            HerzVPoint[0]._multipliersUnit = Multipliers.None;
            HerzVPoint[1]._nominalVal = 1000;
            HerzVPoint[1]._multipliersUnit = Multipliers.None;
            HerzVPoint[2]._nominalVal = 10;
            HerzVPoint[2]._multipliersUnit = Multipliers.Kilo;
            HerzVPoint[3]._nominalVal = 20;
            HerzVPoint[3]._multipliersUnit = Multipliers.Kilo;
            HerzVPoint[4]._nominalVal = 50;
            HerzVPoint[4]._multipliersUnit = Multipliers.Kilo;
            HerzVPoint[5]._nominalVal = 100;
            HerzVPoint[5]._multipliersUnit = Multipliers.Kilo;

            VoltPoint = new AcVPoint[3];
            //конкретно для первой точки 0.2 нужны не все частоты, поэтому вырежем только необходимые
            var trimHerzArr = new AcPoint[4];
            Array.Copy(HerzVPoint, trimHerzArr, 4);
            VoltPoint[0] = new AcVPoint((decimal) 0.2 * VoltMultipliers, OpMultipliers, trimHerzArr);
            VoltPoint[1] = new AcVPoint(1 * VoltMultipliers, OpMultipliers, HerzVPoint);
            VoltPoint[2] = new AcVPoint((decimal) 1.8 * VoltMultipliers, OpMultipliers, HerzVPoint);
        }
    }

    public class Ope4_1_AcV_750V_Measure : Oper4AcvMeasureBase
    {
        public Ope4_1_AcV_750V_Measure(Mult107_109N.RangeNominal inRangeNominal, IUserItemOperation userItemOperation)
            : base(userItemOperation)
        {
            OpMultipliers = Multipliers.None;
            OperMeasureMode = Mult107_109N.MeasureMode.ACV;
            OperationAcRangeCode = Mult107_109N.RangeCode.Range4Manual;
            OperationAcRangeNominal = inRangeNominal;
            Name = OperationAcRangeNominal.GetStringValue();
            Sheme = ShemeTemplateDefault.TemplateSheme;

            VoltMultipliers = 1;

            RangeResolution = new AcVPoint(100, Multipliers.Mili);

            HerzVPoint = new AcPoint[2];
            HerzVPoint[0] = new AcPoint();
            HerzVPoint[0]._nominalVal = 40;
            HerzVPoint[0]._multipliersUnit = Multipliers.None;
            HerzVPoint[1] = new AcPoint();
            HerzVPoint[1]._nominalVal = 1000;
            HerzVPoint[1]._multipliersUnit = Multipliers.None;

            VoltPoint = new AcVPoint[3];
            VoltPoint[0] = new AcVPoint(100 * VoltMultipliers, OpMultipliers, HerzVPoint);
            VoltPoint[1] = new AcVPoint(400 * VoltMultipliers, OpMultipliers, HerzVPoint);
            VoltPoint[2] = new AcVPoint(700 * VoltMultipliers, OpMultipliers, HerzVPoint);
        }
    }

    #endregion ACV

    //////////////////////////////******DCI*******///////////////////////////////

    #region DCI

    public abstract class Oper5DcIMeasure : ParagraphBase, IUserItemOperationBase
    {
        public Oper5DcIMeasure(IUserItemOperation userItemOperation) : base(userItemOperation)
        {
        }

        #region Methods

        protected override DataTable FillData()
        {
            throw new NotImplementedException();
        }

        #endregion

        public override Task StartSinglWork(CancellationToken token, Guid guid)
        {
            throw new NotImplementedException();
        }

        public override async Task StartWork(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }

    #endregion DCI

    //////////////////////////////******ACI*******///////////////////////////////

    #region ACI

    public abstract class Oper6AcIMeasure : ParagraphBase, IUserItemOperationBase
    {
        public Oper6AcIMeasure(IUserItemOperation userItemOperation) : base(userItemOperation)
        {
        }

        #region Methods

        protected override DataTable FillData()
        {
            throw new NotImplementedException();
        }

        #endregion

        public override Task StartSinglWork(CancellationToken token, Guid guid)
        {
            throw new NotImplementedException();
        }

        public override async Task StartWork(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }

    #endregion ACI

    //////////////////////////////******FREQ*******///////////////////////////////

    #region FREQ

    public abstract class Oper7FreqMeasure : ParagraphBase, IUserItemOperationBase
    {
        public Oper7FreqMeasure(IUserItemOperation userItemOperation) : base(userItemOperation)
        {
        }

        #region Methods

        protected override DataTable FillData()
        {
            throw new NotImplementedException();
        }

        #endregion

        public override Task StartSinglWork(CancellationToken token, Guid guid)
        {
            throw new NotImplementedException();
        }

        public override async Task StartWork(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }

    #endregion FREQ

    //////////////////////////////******OHM*******///////////////////////////////

    #region OHM

    public abstract class Oper8OhmMeasureBase : ParagraphBase, IUserItemOperationBase
    {
        #region Fields

        //множители для пределов.
        public decimal _baseMultipliers;

        /// <summary>
        /// Массив поверяемых точек напряжения.
        /// </summary>
        protected AcVPoint[] _ohmPoint;

        protected AcVPoint _rangeResolution;

        /// <summary>
        /// довесок к формуле погрешности- единица младшего разряда
        /// </summary>
        public int EdMlRaz = 10; //значение для пределов свыше 200 мВ

        #endregion

        #region Property

        /// <summary>
        /// Код предела измерения на приборе
        /// </summary>
        public Mult107_109N.RangeCode OperationOhmRangeCode { get; protected set; }

        /// <summary>
        /// Предел измерения поверяемого прибора, необходимый для работы
        /// </summary>
        public Mult107_109N.RangeNominal OperationOhmRangeNominal { get; protected set; }

        /// <summary>
        /// Режим операции измерения прибора
        /// </summary>
        public Mult107_109N.MeasureMode OperMeasureMode { get; protected set; }

        /// <summary>
        /// Множитель единицы измерения текущей операции
        /// </summary>
        public Multipliers OpMultipliers { get; protected set; }

        //контрлируемый прибор
        protected Mult107_109N appa107N { get; set; }

        //эталон
        protected Calib5522A flkCalib5522A { get; set; }

        #endregion

        public Oper8OhmMeasureBase(IUserItemOperation userItemOperation) : base(userItemOperation)
        {
        }

        #region Methods

        protected override DataTable FillData()
        {
            throw new NotImplementedException();
        }

        #endregion

        public override Task StartSinglWork(CancellationToken token, Guid guid)
        {
            throw new NotImplementedException();
        }

        public override async Task StartWork(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }

    #endregion OHM

    //////////////////////////////******FAR*******///////////////////////////////

    #region FAR

    public abstract class Oper9FarMeasure : ParagraphBase, IUserItemOperationBase
    {
        public Oper9FarMeasure(IUserItemOperation userItemOperation) : base(userItemOperation)
        {
        }

        #region Methods

        protected override DataTable FillData()
        {
            throw new NotImplementedException();
        }

        #endregion

        public override Task StartSinglWork(CancellationToken token, Guid guid)
        {
            throw new NotImplementedException();
        }

        public override async Task StartWork(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }

    #endregion FAR

    //////////////////////////////******TEMP*******///////////////////////////////

    #region TEMP

    public abstract class Oper10TemperatureMeasure : ParagraphBase, IUserItemOperationBase
    {
        public Oper10TemperatureMeasure(IUserItemOperation userItemOperation) : base(userItemOperation)
        {
        }

        #region Methods

        protected override DataTable FillData()
        {
            throw new NotImplementedException();
        }

        #endregion

        public override Task StartSinglWork(CancellationToken token, Guid guid)
        {
            throw new NotImplementedException();
        }

        public override async Task StartWork(CancellationToken cancellationToken)

        {
            throw new NotImplementedException();
        }
    }

    #endregion TEMP
}