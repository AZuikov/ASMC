using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using AP.Math;
using AP.Utils.Data;
using AP.Utils.Helps;
using ASMC.Core.Model;
using ASMC.Data.Model;
using ASMC.Data.Model.Interface;
using ASMC.Devices.IEEE;
using ASMC.Devices.IEEE.Fluke.Calibrator;
using ASMC.Devices.Port.APPA;
using DevExpress.Mvvm;
using NLog;

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

    public abstract class OpertionFirsVerf : ASMC.Core.Model.Operation
    {
        public OpertionFirsVerf(ServicePack servicePack) : base(servicePack)
        {
            //Необходимые устройства
            ControlDevices = new IDeviceUi[]
                {new Device {Name = new[] {"5522A"}, Description = "Многофунциональный калибратор"}};
            TestDevices = new IDeviceUi[]
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
            var data = new DataTable {TableName = "ITBmVisualTest"};
            ;
            data.Columns.Add("Результат внешнего осмотра");
            var dataRow = data.NewRow();
            if (DataRow.Count == 1)
            {
                var dds = DataRow[0] as BasicOperation<bool>;
                // ReSharper disable once PossibleNullReferenceException
                dataRow[0] = dds.Getting ? "Соответствует" : dds.Comment;
                data.Rows.Add(dataRow);
            }

            return data;
        }

        protected override void InitWork()
        {
            DataRow.Clear();
            var operation = new BasicOperation<bool>();
            operation.Expected = true;
            operation.IsGood = () => Equals(operation.Getting, operation.Expected);
            operation.InitWork = () =>
            {
                var service = UserItemOperation.ServicePack.QuestionText;
                service.Title = "Внешний осмотр";
                service.Entity = new Tuple<string, Assembly>("VisualTestText", null);
                service.Show();
                var res = service.Entity as Tuple<string, bool>;
                operation.Getting = res.Item2;
                operation.Comment = res.Item1;
                operation.IsGood = () => operation.Getting;

                return Task.CompletedTask;
            };

            operation.CompliteWork = () => { return Task.FromResult(true); };
            DataRow.Add(operation);
        }

        #endregion

        public List<IBasicOperation<bool>> DataRow { get; set; }
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
        /// Процент от измеряемой точки для расчета погрешности (уже переведе в абсолютные единицы).
        /// </summary>
        public decimal BaseTolCoeff = (decimal) 0.0006;

        /// <summary>
        /// довесок к формуле погрешности- число единиц младшего разряда
        /// </summary>
        public int EdMlRaz = 10; //значение для пределов свыше 200 мВ

        /// <summary>
        /// Разарешение пределеа измерения (последний значащий разряд)
        /// </summary>
        protected AcVariablePoint RangeResolution;

        /// <summary>
        /// Массив поверяемых точек напряжения.
        /// </summary>
        protected AcVariablePoint[] VoltPoint;

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
                                             .Show($"Установите режим измерения: {OperMeasureMode.GetStringValue()} {OperMeasureMode}",
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
                        flkCalib5522A.Out.Set.Voltage.Dc.SetValue(currPoint.VariableBaseValueMeasPoint.NominalVal);
                        flkCalib5522A.Out.SetOutput(CalibrMain.COut.State.On);
                        Thread.Sleep(2000);
                        //измеряем
                        var measurePoint = (decimal) appa107N.GetValue();

                        flkCalib5522A.Out.SetOutput(CalibrMain.COut.State.Off);

                        operation.Getting = measurePoint;
                        operation.Expected = currPoint.VariableBaseValueMeasPoint.NominalVal /
                                             (decimal) currPoint
                                                      .VariableBaseValueMeasPoint.MultipliersUnit.GetDoubleValue();
                        //расчет погрешности для конкретной точки предела измерения
                        operation.ErrorCalculation = (inA, inB) =>
                        {
                            var result = BaseTolCoeff * operation.Expected + EdMlRaz *
                                RangeResolution.VariableBaseValueMeasPoint.NominalVal *
                                (decimal) (RangeResolution
                                          .VariableBaseValueMeasPoint.MultipliersUnit.GetDoubleValue() /
                                           currPoint.VariableBaseValueMeasPoint.MultipliersUnit
                                                    .GetDoubleValue()
                                );
                            var mantisa =
                                MathStatistics.GetMantissa((decimal) (RangeResolution
                                                                     .VariableBaseValueMeasPoint.MultipliersUnit
                                                                     .GetDoubleValue() /
                                                                      currPoint.VariableBaseValueMeasPoint
                                                                               .MultipliersUnit
                                                                               .GetDoubleValue()));
                            MathStatistics.Round(ref result, mantisa);
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
    }

    public class Oper3_1DC_2V_Measure : Oper3DcvMeasureBase
    {
        public Oper3_1DC_2V_Measure(Mult107_109N.RangeNominal inRangeNominal, IUserItemOperation userItemOperation) :
            base(userItemOperation)
        {
            OperationDcRangeCode = Mult107_109N.RangeCode.Range1Manual;
            OperationDcRangeNominal = inRangeNominal;
            RangeResolution = new AcVariablePoint(100, Multipliers.Micro);
            Name = OperationDcRangeNominal.GetStringValue();
            OpMultipliers = Multipliers.None;

            BaseMultipliers = 100;
            VoltPoint = new AcVariablePoint[6];
            VoltPoint[0] = new AcVariablePoint((decimal) 0.004 * BaseMultipliers, OpMultipliers);
            VoltPoint[1] = new AcVariablePoint((decimal) 0.008 * BaseMultipliers, OpMultipliers);
            VoltPoint[2] = new AcVariablePoint((decimal) 0.012 * BaseMultipliers, OpMultipliers);
            VoltPoint[3] = new AcVariablePoint((decimal) 0.016 * BaseMultipliers, OpMultipliers);
            VoltPoint[4] = new AcVariablePoint((decimal) 0.018 * BaseMultipliers, OpMultipliers);
            VoltPoint[5] = new AcVariablePoint((decimal) -0.018 * BaseMultipliers, OpMultipliers);
        }
    }

    public class Oper3_1DC_20V_Measure : Oper3DcvMeasureBase
    {
        public Oper3_1DC_20V_Measure(Mult107_109N.RangeNominal inRangeNominal, IUserItemOperation userItemOperation) :
            base(userItemOperation)
        {
            OperationDcRangeCode = Mult107_109N.RangeCode.Range2Manual;
            OperationDcRangeNominal = inRangeNominal;
            RangeResolution = new AcVariablePoint(1, Multipliers.Mili);
            Name = OperationDcRangeNominal.GetStringValue();
            OpMultipliers = Multipliers.None;

            BaseMultipliers = 1000;
            VoltPoint = new AcVariablePoint[6];
            VoltPoint[0] = new AcVariablePoint((decimal) 0.004 * BaseMultipliers, OpMultipliers);
            VoltPoint[1] = new AcVariablePoint((decimal) 0.008 * BaseMultipliers, OpMultipliers);
            VoltPoint[2] = new AcVariablePoint((decimal) 0.012 * BaseMultipliers, OpMultipliers);
            VoltPoint[3] = new AcVariablePoint((decimal) 0.016 * BaseMultipliers, OpMultipliers);
            VoltPoint[4] = new AcVariablePoint((decimal) 0.018 * BaseMultipliers, OpMultipliers);
            VoltPoint[5] = new AcVariablePoint((decimal) -0.018 * BaseMultipliers, OpMultipliers);
        }
    }

    public class Oper3_1DC_200V_Measure : Oper3DcvMeasureBase
    {
        public Oper3_1DC_200V_Measure(Mult107_109N.RangeNominal inRangeNominal, IUserItemOperation userItemOperation) :
            base(userItemOperation)
        {
            OperationDcRangeCode = Mult107_109N.RangeCode.Range3Manual;
            OperationDcRangeNominal = inRangeNominal;
            RangeResolution = new AcVariablePoint(10, Multipliers.Mili);
            Name = OperationDcRangeNominal.GetStringValue();
            OpMultipliers = Multipliers.None;

            BaseMultipliers = 10000;
            VoltPoint = new AcVariablePoint[6];
            VoltPoint[0] = new AcVariablePoint((decimal) 0.004 * BaseMultipliers, OpMultipliers);
            VoltPoint[1] = new AcVariablePoint((decimal) 0.008 * BaseMultipliers, OpMultipliers);
            VoltPoint[2] = new AcVariablePoint((decimal) 0.012 * BaseMultipliers, OpMultipliers);
            VoltPoint[3] = new AcVariablePoint((decimal) 0.016 * BaseMultipliers, OpMultipliers);
            VoltPoint[4] = new AcVariablePoint((decimal) 0.018 * BaseMultipliers, OpMultipliers);
            VoltPoint[5] = new AcVariablePoint((decimal) -0.018 * BaseMultipliers, OpMultipliers);
        }
    }

    public class Oper3_1DC_1000V_Measure : Oper3DcvMeasureBase
    {
        public Oper3_1DC_1000V_Measure(Mult107_109N.RangeNominal inRangeNominal, IUserItemOperation userItemOperation) :
            base(userItemOperation)
        {
            OperationDcRangeCode = Mult107_109N.RangeCode.Range4Manual;
            OperationDcRangeNominal = inRangeNominal;
            RangeResolution = new AcVariablePoint(100, Multipliers.Mili);
            Name = OperationDcRangeNominal.GetStringValue();
            OpMultipliers = Multipliers.None;

            BaseMultipliers = 1;
            VoltPoint = new AcVariablePoint[6];
            VoltPoint[0] = new AcVariablePoint(100 * BaseMultipliers, OpMultipliers);
            VoltPoint[1] = new AcVariablePoint(200 * BaseMultipliers, OpMultipliers);
            VoltPoint[2] = new AcVariablePoint(400 * BaseMultipliers, OpMultipliers);
            VoltPoint[3] = new AcVariablePoint(700 * BaseMultipliers, OpMultipliers);
            VoltPoint[4] = new AcVariablePoint(900 * BaseMultipliers, OpMultipliers);
            VoltPoint[5] = new AcVariablePoint(-900 * BaseMultipliers, OpMultipliers);
        }
    }

    public class Oper3_1DC_20mV_Measure : Oper3DcvMeasureBase
    {
        public Oper3_1DC_20mV_Measure(Mult107_109N.RangeNominal inRangeNominal, IUserItemOperation userItemOperation) :
            base(userItemOperation)
        {
            OperationDcRangeCode = Mult107_109N.RangeCode.Range1Manual;
            OperationDcRangeNominal = inRangeNominal;
            RangeResolution = new AcVariablePoint(1, Multipliers.Micro);
            Name = OperationDcRangeNominal.GetStringValue();
            OpMultipliers = Multipliers.Mili;

            RangeResolution = new AcVariablePoint(1, Multipliers.Micro);

            BaseMultipliers = 1;
            VoltPoint = new AcVariablePoint[6];
            VoltPoint[0] = new AcVariablePoint((decimal) 0.004 * BaseMultipliers, OpMultipliers);
            VoltPoint[1] = new AcVariablePoint((decimal) 0.008 * BaseMultipliers, OpMultipliers);
            VoltPoint[2] = new AcVariablePoint((decimal) 0.012 * BaseMultipliers, OpMultipliers);
            VoltPoint[3] = new AcVariablePoint((decimal) 0.016 * BaseMultipliers, OpMultipliers);
            VoltPoint[4] = new AcVariablePoint((decimal) 0.018 * BaseMultipliers, OpMultipliers);
            VoltPoint[5] = new AcVariablePoint((decimal) -0.018 * BaseMultipliers, OpMultipliers);
        }
    }

    public class Oper3_1DC_200mV_Measure : Oper3DcvMeasureBase
    {
        public Oper3_1DC_200mV_Measure(Mult107_109N.RangeNominal inRangeNominal, IUserItemOperation userItemOperation) :
            base(userItemOperation)
        {
            OperationDcRangeCode = Mult107_109N.RangeCode.Range1Manual;
            OperationDcRangeNominal = inRangeNominal;
            RangeResolution = new AcVariablePoint(10, Multipliers.Micro);
            Name = OperationDcRangeNominal.GetStringValue();
            OpMultipliers = Multipliers.Mili;

            RangeResolution = new AcVariablePoint(10, Multipliers.Micro);

            BaseMultipliers = 10;
            VoltPoint = new AcVariablePoint[6];
            VoltPoint[0] = new AcVariablePoint((decimal) 0.004 * BaseMultipliers, OpMultipliers);
            VoltPoint[1] = new AcVariablePoint((decimal) 0.008 * BaseMultipliers, OpMultipliers);
            VoltPoint[2] = new AcVariablePoint((decimal) 0.012 * BaseMultipliers, OpMultipliers);
            VoltPoint[3] = new AcVariablePoint((decimal) 0.016 * BaseMultipliers, OpMultipliers);
            VoltPoint[4] = new AcVariablePoint((decimal) 0.018 * BaseMultipliers, OpMultipliers);
            VoltPoint[5] = new AcVariablePoint((decimal) -0.018 * BaseMultipliers, OpMultipliers);
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

    public class Oper4AcvMeasureBase : ParagraphBase, IUserItemOperation<decimal>
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        #region Fields

        /// <summary>
        /// Набор частот, характерный для данного предела измерения
        /// </summary>
        protected MeasPoint[] HerzVPoint;

        /// <summary>
        /// Множитель для поверяемых точек. (Если точки можно посчитать простым умножением).
        /// </summary>
        protected decimal VoltMultipliers;

        /// <summary>
        /// Итоговый массив поверяемых точек. У каждого номинала напряжения вложены номиналы частот для текущей точки.
        /// </summary>
        public AcVariablePoint[] VoltPoint;

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
        /// частот.
        /// </summary>
        /// <returns>Результат вычисления.</returns>
        protected void ConstructTooleranceFormula(MeasPoint inFreq)
        {
            //разрешение предела измерения должно быть проинициализировано в коснтсрукторе соответсвующего класса

            if ((OperationAcRangeNominal == Mult107_109N.RangeNominal.Range200mV ||
                 OperationAcRangeNominal == Mult107_109N.RangeNominal.Range20mV) &&
                inFreq.MultipliersUnit == Multipliers.None)
            {
                EdMlRaz = 80;
                if (inFreq.NominalVal >= 40 && inFreq.NominalVal < 100) BaseTolCoeff = (decimal) 0.007;
                if (inFreq.NominalVal >= 100 && inFreq.NominalVal < 1000) BaseTolCoeff = (decimal) 0.01;
            }

            if (OperationAcRangeNominal == Mult107_109N.RangeNominal.Range2V ||
                OperationAcRangeNominal == Mult107_109N.RangeNominal.Range20V ||
                OperationAcRangeNominal == Mult107_109N.RangeNominal.Range200V)
            {
                if (inFreq.MultipliersUnit == Multipliers.None)
                {
                    EdMlRaz = 50;
                    if (inFreq.NominalVal >= 40 && inFreq.NominalVal < 100) BaseTolCoeff = (decimal) 0.007;
                    if (inFreq.NominalVal >= 100 && inFreq.NominalVal < 1000) BaseTolCoeff = (decimal) 0.01;
                }

                if (inFreq.MultipliersUnit == Multipliers.Kilo)
                {
                    if (inFreq.NominalVal >= 1 && inFreq.NominalVal < 10)
                    {
                        BaseTolCoeff = (decimal) 0.02;
                        EdMlRaz = 60;
                    }

                    if (inFreq.NominalVal >= 10 && inFreq.NominalVal < 20)
                    {
                        BaseTolCoeff = (decimal) 0.03;
                        EdMlRaz = 70;
                    }

                    if (inFreq.NominalVal >= 20 && inFreq.NominalVal < 50)
                    {
                        BaseTolCoeff = (decimal) 0.05;
                        EdMlRaz = 80;
                    }

                    if (inFreq.NominalVal >= 50 && inFreq.NominalVal <= 100)
                    {
                        BaseTolCoeff = (decimal) 0.1;
                        EdMlRaz = 100;
                    }
                }
            }

            if (OperationAcRangeNominal == Mult107_109N.RangeNominal.Range750V)
            {
                EdMlRaz = 50;
                if (inFreq.NominalVal >= 40 && inFreq.NominalVal < 100) BaseTolCoeff = (decimal) 0.007;
                if (inFreq.NominalVal >= 100 && inFreq.NominalVal < 1000) BaseTolCoeff = (decimal) 0.01;
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
            foreach (var freqPoint in volPoint.Herz)
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
                                             .Show($"Установите режим измерения: {OperMeasureMode.GetStringValue()} {OperMeasureMode}",
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
                    flkCalib5522A.Out.Set.Voltage.Ac.SetValue(volPoint.VariableBaseValueMeasPoint.NominalVal,
                                                              freqPoint.NominalVal,
                                                              volPoint.VariableBaseValueMeasPoint.MultipliersUnit,
                                                              freqPoint.MultipliersUnit);
                    flkCalib5522A.Out.SetOutput(CalibrMain.COut.State.On);
                    Thread.Sleep(2000);
                    //измеряем
                    var measurePoint = (decimal) appa107N.GetValue();
                    operation.Getting = measurePoint;

                    operation.Expected = volPoint.VariableBaseValueMeasPoint.NominalVal /
                                         (decimal) volPoint.VariableBaseValueMeasPoint.MultipliersUnit.GetDoubleValue();

                    //расчет погрешности для конкретной точки предела измерения
                    ConstructTooleranceFormula(freqPoint); // функция подбирает коэффициенты для формулы погрешности
                    operation.ErrorCalculation = (inA, inB) =>
                    {
                        var result = BaseTolCoeff * operation.Expected + EdMlRaz *
                            RangeResolution.VariableBaseValueMeasPoint.NominalVal *
                            (decimal) (RangeResolution
                                      .VariableBaseValueMeasPoint.MultipliersUnit.GetDoubleValue() /
                                       volPoint.VariableBaseValueMeasPoint.MultipliersUnit.GetDoubleValue());
                        var mantisa =
                            MathStatistics.GetMantissa((decimal) (RangeResolution
                                                                 .VariableBaseValueMeasPoint.MultipliersUnit
                                                                 .GetDoubleValue() /
                                                                  volPoint.VariableBaseValueMeasPoint.MultipliersUnit
                                                                          .GetDoubleValue()));
                        MathStatistics.Round(ref result, mantisa);
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
                DataRow.Add(DataRow.IndexOf(operation) == -1
                                ? operation
                                : (BasicOperationVerefication<decimal>) operation.Clone());
            }
        }

        #endregion

        public List<IBasicOperation<decimal>> DataRow { get; set; }

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
        protected AcVariablePoint RangeResolution;

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

            RangeResolution = new AcVariablePoint(1, Multipliers.Micro);

            HerzVPoint = new MeasPoint[2];
            HerzVPoint[0] = new MeasPoint(MeasureUnits.V, Multipliers.Mili, 40);

            HerzVPoint[1] = new MeasPoint(MeasureUnits.V, Multipliers.None, 1000);

            VoltPoint = new AcVariablePoint[3];
            VoltPoint[0] = new AcVariablePoint(4 * VoltMultipliers, OpMultipliers, HerzVPoint);
            VoltPoint[1] = new AcVariablePoint(10 * VoltMultipliers, OpMultipliers, HerzVPoint);
            VoltPoint[2] = new AcVariablePoint(18 * VoltMultipliers, OpMultipliers, HerzVPoint);
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

            RangeResolution = new AcVariablePoint(10, Multipliers.Micro);

            HerzVPoint = new MeasPoint[2];
            HerzVPoint[0] = new MeasPoint(MeasureUnits.V, Multipliers.Mili, 40);
            HerzVPoint[1] = new MeasPoint(MeasureUnits.V, Multipliers.None, 1000);

            VoltPoint = new AcVariablePoint[3];
            VoltPoint[0] = new AcVariablePoint(4 * VoltMultipliers, OpMultipliers, HerzVPoint);
            VoltPoint[1] = new AcVariablePoint(10 * VoltMultipliers, OpMultipliers, HerzVPoint);
            VoltPoint[2] = new AcVariablePoint(18 * VoltMultipliers, OpMultipliers, HerzVPoint);
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

            RangeResolution = new AcVariablePoint((decimal) 0.1, Multipliers.Mili);

            HerzVPoint = new MeasPoint[6];
            HerzVPoint[0] = new MeasPoint(MeasureUnits.V, Multipliers.None, 40 * VoltMultipliers);
            HerzVPoint[1] = new MeasPoint(MeasureUnits.V, Multipliers.None, 1000 * VoltMultipliers);
            HerzVPoint[2] = new MeasPoint(MeasureUnits.V, Multipliers.Kilo, 10 * VoltMultipliers);
            HerzVPoint[3] = new MeasPoint(MeasureUnits.V, Multipliers.Kilo, 20 * VoltMultipliers);
            HerzVPoint[4] = new MeasPoint(MeasureUnits.V, Multipliers.Kilo, 50 * VoltMultipliers);
            HerzVPoint[5] = new MeasPoint(MeasureUnits.V, Multipliers.Kilo, 100 * VoltMultipliers);

            VoltPoint = new AcVariablePoint[3];
            //конкретно для первой точки 0.2 нужны не все частоты, поэтому вырежем только необходимые
            var trimHerzArr = new MeasPoint[4];
            Array.Copy(HerzVPoint, trimHerzArr, 4);
            VoltPoint[0] = new AcVariablePoint((decimal) 0.2, OpMultipliers, trimHerzArr);
            VoltPoint[1] = new AcVariablePoint(1, OpMultipliers, HerzVPoint);
            VoltPoint[2] = new AcVariablePoint((decimal) 1.8, OpMultipliers, HerzVPoint);
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

            RangeResolution = new AcVariablePoint(1, Multipliers.Mili);

            HerzVPoint = new MeasPoint[6];
            HerzVPoint[0] = new MeasPoint(MeasureUnits.V, Multipliers.None, 40 * VoltMultipliers);
            HerzVPoint[1] = new MeasPoint(MeasureUnits.V, Multipliers.None, 1000 * VoltMultipliers);
            HerzVPoint[2] = new MeasPoint(MeasureUnits.V, Multipliers.Kilo, 10 * VoltMultipliers);
            HerzVPoint[3] = new MeasPoint(MeasureUnits.V, Multipliers.Kilo, 20 * VoltMultipliers);
            HerzVPoint[4] = new MeasPoint(MeasureUnits.V, Multipliers.Kilo, 50 * VoltMultipliers);
            HerzVPoint[5] = new MeasPoint(MeasureUnits.V, Multipliers.Kilo, 100 * VoltMultipliers);

            VoltPoint = new AcVariablePoint[3];
            //конкретно для первой точки 0.2 нужны не все частоты, поэтому вырежем только необходимые
            var trimHerzArr = new MeasPoint[4];
            Array.Copy(HerzVPoint, trimHerzArr, 4);
            VoltPoint[0] = new AcVariablePoint((decimal) 0.2 * VoltMultipliers, OpMultipliers, trimHerzArr);
            VoltPoint[1] = new AcVariablePoint(1 * VoltMultipliers, OpMultipliers, HerzVPoint);
            VoltPoint[2] = new AcVariablePoint((decimal) 1.8 * VoltMultipliers, OpMultipliers, HerzVPoint);
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

            RangeResolution = new AcVariablePoint(10, Multipliers.Mili);

            HerzVPoint = new MeasPoint[6];
            HerzVPoint[0] = new MeasPoint(MeasureUnits.V, Multipliers.None, 40 * VoltMultipliers);
            HerzVPoint[1] = new MeasPoint(MeasureUnits.V, Multipliers.None, 1000 * VoltMultipliers);
            HerzVPoint[2] = new MeasPoint(MeasureUnits.V, Multipliers.Kilo, 10 * VoltMultipliers);
            HerzVPoint[3] = new MeasPoint(MeasureUnits.V, Multipliers.Kilo, 20 * VoltMultipliers);
            HerzVPoint[4] = new MeasPoint(MeasureUnits.V, Multipliers.Kilo, 50 * VoltMultipliers);
            HerzVPoint[5] = new MeasPoint(MeasureUnits.V, Multipliers.Kilo, 100 * VoltMultipliers);

            VoltPoint = new AcVariablePoint[3];
            //конкретно для первой точки 0.2 нужны не все частоты, поэтому вырежем только необходимые
            var trimHerzArr = new MeasPoint[4];
            Array.Copy(HerzVPoint, trimHerzArr, 4);
            VoltPoint[0] = new AcVariablePoint((decimal) 0.2 * VoltMultipliers, OpMultipliers, trimHerzArr);
            VoltPoint[1] = new AcVariablePoint(1 * VoltMultipliers, OpMultipliers, HerzVPoint);
            VoltPoint[2] = new AcVariablePoint((decimal) 1.8 * VoltMultipliers, OpMultipliers, HerzVPoint);
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

            RangeResolution = new AcVariablePoint(100, Multipliers.Mili);

            HerzVPoint = new MeasPoint[2];
            HerzVPoint[0] = new MeasPoint(MeasureUnits.V, OpMultipliers, 40);
            HerzVPoint[1] = new MeasPoint(MeasureUnits.V, OpMultipliers, 1000);

            VoltPoint = new AcVariablePoint[3];
            VoltPoint[0] = new AcVariablePoint(100 * VoltMultipliers, OpMultipliers, HerzVPoint);
            VoltPoint[1] = new AcVariablePoint(400 * VoltMultipliers, OpMultipliers, HerzVPoint);
            VoltPoint[2] = new AcVariablePoint(700 * VoltMultipliers, OpMultipliers, HerzVPoint);
        }
    }

    #endregion ACV

    //////////////////////////////******DCI*******///////////////////////////////

    #region DCI

    public class Oper5DciMeasureBase : ParagraphBase, IUserItemOperation<decimal>
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        #region Fields

        //множители для пределов.
        public decimal BaseMultipliers;

        /// <summary>
        /// Процент от измеряемой точки для расчета погрешности (уже переведе в абсолютные единицы).
        /// </summary>
        public decimal BaseTolCoeff;

        /// <summary>
        /// Массив поверяемых точек напряжения.
        /// </summary>
        protected MeasPoint[] CurrentDciPoint;

        /// <summary>
        /// довесок к формуле погрешности- число единиц младшего разряда
        /// </summary>
        public int EdMlRaz; //значение для пределов свыше 200 мВ

        /// <summary>
        /// Разарешение пределеа измерения (последний значащий разряд)
        /// </summary>
        protected AcVariablePoint RangeResolution;

        #endregion

        #region Property

        /// <summary>
        /// Код предела измерения на приборе
        /// </summary>
        public Mult107_109N.RangeCode OperationRangeCode { get; protected set; }

        /// <summary>
        /// Предел измерения поверяемого прибора, необходимый для работы
        /// </summary>
        public Mult107_109N.RangeNominal OperationRangeNominal { get; protected set; }

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

        public Oper5DciMeasureBase(IUserItemOperation userItemOperation) : base(userItemOperation)
        {
            Name = "Определение погрешности измерения постоянного тока";
            OperMeasureMode = Mult107_109N.MeasureMode.DCmA;
            OpMultipliers = Multipliers.None;
            OperationRangeCode = Mult107_109N.RangeCode.Range1Manual;
            OperationRangeNominal = Mult107_109N.RangeNominal.RangeNone;
            DataRow = new List<IBasicOperation<decimal>>();
            Sheme = new ShemeImage
            {
                Description = "Измерительная схема",
                Number = 2,
                FileName = "appa_10XN_ma_5522A.jpg",
                ExtendedDescription = "Соберите измерительную схему, согласно рисунку"
            };
        }

        #region Methods

        protected override DataTable FillData()
        {
            throw new NotImplementedException();
        }

        protected override void InitWork()
        {
            if (appa107N == null || flkCalib5522A == null) return;

            DataRow.Clear();

            foreach (var currPoint in CurrentDciPoint)
            {
                var operation = new BasicOperationVerefication<decimal>();
                DataRow.Clear();
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
                                             .Show($"Установите режим измерения: {OperMeasureMode.GetStringValue()} {OperMeasureMode}",
                                                   "Указание оператору", MessageButton.OK,
                                                   MessageIcon.Information,
                                                   MessageResult.OK);

                        var testRangeNominal = appa107N.GetRangeNominal;
                        while (OperationRangeNominal != appa107N.GetRangeNominal)
                        {
                            var countPushRangeButton =
                                appa107N.GetRangeSwitchMode != Mult107_109N.RangeSwitchMode.Auto ? 1 : 0;

                            if (OpMultipliers == Multipliers.Mili)
                            {
                                UserItemOperation.ServicePack.MessageBox
                                                 .Show($"Текущий предел измерения прибора {appa107N.GetRangeNominal.GetStringValue()}\n Необходимо установить предел {OperationRangeNominal.GetStringValue()} " +
                                                       $"Нажмите на приборе клавишу Range {countPushRangeButton + 1} раз.",
                                                       "Указание оператору", MessageButton.OK, MessageIcon.Information,
                                                       MessageResult.OK);
                            }
                            else
                            {
                                var curRange = (int) appa107N.GetRangeCode & 128;
                                var targetRange = (int) OperationRangeCode & 128;
                                countPushRangeButton = countPushRangeButton + 4 - curRange +
                                                       (targetRange < curRange ? curRange : 0);

                                UserItemOperation.ServicePack.MessageBox
                                                 .Show($"Текущий предел измерения прибора {appa107N.GetRangeNominal.GetStringValue()}\n Необходимо установить предел {OperationRangeNominal.GetStringValue()} " +
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
                    flkCalib5522A.Out.Set.Current.Dc.SetValue(currPoint.NominalVal);
                    flkCalib5522A.Out.SetOutput(CalibrMain.COut.State.On);
                    Thread.Sleep(2000);
                    //измеряем
                    var measurePoint = (decimal) appa107N.GetValue();

                    flkCalib5522A.Out.SetOutput(CalibrMain.COut.State.Off);

                    operation.Getting = measurePoint;
                    operation.Expected = currPoint.NominalVal /
                                         (decimal) currPoint
                                                  .MultipliersUnit.GetDoubleValue();
                    //расчет погрешности для конкретной точки предела измерения
                    operation.ErrorCalculation = (inA, inB) =>
                    {
                        var result = BaseTolCoeff * operation.Expected + EdMlRaz *
                            RangeResolution.VariableBaseValueMeasPoint.NominalVal *
                            (decimal) (RangeResolution
                                      .VariableBaseValueMeasPoint.MultipliersUnit.GetDoubleValue() /
                                       currPoint.MultipliersUnit.GetDoubleValue()
                            );
                        var mantisa =
                            MathStatistics.GetMantissa((decimal) (RangeResolution
                                                                 .VariableBaseValueMeasPoint.MultipliersUnit
                                                                 .GetDoubleValue() /
                                                                  currPoint.MultipliersUnit
                                                                           .GetDoubleValue()));
                        MathStatistics.Round(ref result, mantisa);
                        return result;
                    };
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
    }

    public class Oper5_1Dci_20mA_Measure : Oper5DciMeasureBase
    {
        public Oper5_1Dci_20mA_Measure(Mult107_109N.RangeNominal inRangeNominal, IUserItemOperation userItemOperation) :
            base(userItemOperation)
        {
            OperationRangeCode = Mult107_109N.RangeCode.Range1Manual;
            OperationRangeNominal = inRangeNominal;
            RangeResolution = new AcVariablePoint(1, Multipliers.Micro);
            Name = OperationRangeNominal.GetStringValue();
            OpMultipliers = Multipliers.Mili;

            BaseMultipliers = 1;
            CurrentDciPoint = new MeasPoint[5];
            CurrentDciPoint[0] = new MeasPoint(MeasureUnits.I, OpMultipliers, 4 * BaseMultipliers);
            CurrentDciPoint[1] = new MeasPoint(MeasureUnits.I, OpMultipliers, 8 * BaseMultipliers);
            CurrentDciPoint[2] = new MeasPoint(MeasureUnits.I, OpMultipliers, 12 * BaseMultipliers);
            CurrentDciPoint[3] = new MeasPoint(MeasureUnits.I, OpMultipliers, 18 * BaseMultipliers);
            CurrentDciPoint[4] = new MeasPoint(MeasureUnits.I, OpMultipliers, -18 * BaseMultipliers);
        }
    }

    public class Oper5_1Dci_200mA_Measure : Oper5DciMeasureBase
    {
        public Oper5_1Dci_200mA_Measure(Mult107_109N.RangeNominal inRangeNominal, IUserItemOperation userItemOperation)
            : base(userItemOperation)
        {
            OperationRangeCode = Mult107_109N.RangeCode.Range2Manual;
            OperationRangeNominal = inRangeNominal;
            RangeResolution = new AcVariablePoint(10, Multipliers.Micro);
            Name = OperationRangeNominal.GetStringValue();
            OpMultipliers = Multipliers.Mili;

            BaseMultipliers = 10;
            CurrentDciPoint = new MeasPoint[5];
            CurrentDciPoint[0] = new MeasPoint(MeasureUnits.I, OpMultipliers, 4 * BaseMultipliers);
            CurrentDciPoint[1] = new MeasPoint(MeasureUnits.I, OpMultipliers, 8 * BaseMultipliers);
            CurrentDciPoint[2] = new MeasPoint(MeasureUnits.I, OpMultipliers, 12 * BaseMultipliers);
            CurrentDciPoint[3] = new MeasPoint(MeasureUnits.I, OpMultipliers, 18 * BaseMultipliers);
            CurrentDciPoint[4] = new MeasPoint(MeasureUnits.I, OpMultipliers, -18 * BaseMultipliers);
        }
    }

    public class Oper5_1Dci_2A_Measure : Oper5DciMeasureBase
    {
        public Oper5_1Dci_2A_Measure(Mult107_109N.RangeNominal inRangeNominal, IUserItemOperation userItemOperation) :
            base(userItemOperation)
        {
            OperationRangeCode = Mult107_109N.RangeCode.Range1Manual;
            OperationRangeNominal = inRangeNominal;
            RangeResolution = new AcVariablePoint(100, Multipliers.Micro);
            Name = OperationRangeNominal.GetStringValue();
            OpMultipliers = Multipliers.None;

            BaseMultipliers = (decimal) 0.1;
            CurrentDciPoint = new MeasPoint[5];
            CurrentDciPoint[0] = new MeasPoint(MeasureUnits.I, OpMultipliers, 4 * BaseMultipliers);
            CurrentDciPoint[1] = new MeasPoint(MeasureUnits.I, OpMultipliers, 8 * BaseMultipliers);
            CurrentDciPoint[2] = new MeasPoint(MeasureUnits.I, OpMultipliers, 12 * BaseMultipliers);
            CurrentDciPoint[3] = new MeasPoint(MeasureUnits.I, OpMultipliers, 18 * BaseMultipliers);
            CurrentDciPoint[4] = new MeasPoint(MeasureUnits.I, OpMultipliers, -18 * BaseMultipliers);

            Sheme = new ShemeImage
            {
                Description = "Измерительная схема",
                Number = 3,
                FileName = "appa_10XN_A_Aux_5522A.jpg",
                ExtendedDescription = "Соберите измерительную схему, согласно рисунку"
            };
        }
    }

    public class Oper5_2_1Dci_10A_Measure : Oper5DciMeasureBase
    {
        public Oper5_2_1Dci_10A_Measure(Mult107_109N.RangeNominal inRangeNominal, IUserItemOperation userItemOperation)
            : base(userItemOperation)
        {
            OperationRangeCode = Mult107_109N.RangeCode.Range1Manual;
            OperationRangeNominal = inRangeNominal;
            RangeResolution = new AcVariablePoint(1, Multipliers.Mili);

            OpMultipliers = Multipliers.None;

            CurrentDciPoint = new MeasPoint[1];
            CurrentDciPoint[0] = new MeasPoint(MeasureUnits.I, OpMultipliers, 1);

            Name = OperationRangeNominal.GetStringValue() + $" точка {CurrentDciPoint[0].Description}";

            Sheme = new ShemeImage
            {
                Description = "Измерительная схема",
                Number = 3,
                FileName = "appa_10XN_A_Aux_5522A.jpg",
                ExtendedDescription = "Соберите измерительную схему, согласно рисунку"
            };
        }
    }

    public class Oper5_2_2Dci_10A_Measure : Oper5DciMeasureBase
    {
        public Oper5_2_2Dci_10A_Measure(Mult107_109N.RangeNominal inRangeNominal, IUserItemOperation userItemOperation)
            : base(userItemOperation)
        {
            OperationRangeCode = Mult107_109N.RangeCode.Range1Manual;
            OperationRangeNominal = inRangeNominal;
            RangeResolution = new AcVariablePoint(1, Multipliers.Mili);
            Name = OperationRangeNominal.GetStringValue();
            OpMultipliers = Multipliers.None;

            CurrentDciPoint = new MeasPoint[4];
            CurrentDciPoint[0] = new MeasPoint(MeasureUnits.I, OpMultipliers, 5);
            CurrentDciPoint[1] = new MeasPoint(MeasureUnits.I, OpMultipliers, 9);
            CurrentDciPoint[2] = new MeasPoint(MeasureUnits.I, OpMultipliers, -9);

            Sheme = new ShemeImage
            {
                Description = "Измерительная схема",
                Number = 4,
                FileName = "appa_10XN_20A_5522A.jpg",
                ExtendedDescription = "Соберите измерительную схему, согласно рисунку"
            };
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
    }

    #endregion FREQ

    //////////////////////////////******OHM*******///////////////////////////////

    #region OHM

    public class Oper8_1Resistance_200Ohm_Measure : Oper8ResistanceMeasureBase
    {
        public Oper8_1Resistance_200Ohm_Measure(Mult107_109N.RangeNominal inRangeNominal,
            IUserItemOperation userItemOperation) : base(userItemOperation)
        {
            OperationOhmRangeCode = Mult107_109N.RangeCode.Range1Manual;
            OperationOhmRangeNominal = inRangeNominal;
            RangeResolution = new AcVariablePoint(10, Multipliers.Mili);
            Name = OperationOhmRangeNominal.GetStringValue();
            OpMultipliers = Multipliers.None;

            BaseMultipliers = 1;
            OhmPoint = new MeasPoint[3];
            OhmPoint[0] = new MeasPoint(MeasureUnits.Ohm, OpMultipliers, 50 * BaseMultipliers);
            OhmPoint[1] = new MeasPoint(MeasureUnits.Ohm, OpMultipliers, 100 * BaseMultipliers);
            OhmPoint[2] = new MeasPoint(MeasureUnits.Ohm, OpMultipliers, 200 * BaseMultipliers);
        }
    }

    public class Oper8_1Resistance_2kOhm_Measure : Oper8ResistanceMeasureBase
    {
        public Oper8_1Resistance_2kOhm_Measure(Mult107_109N.RangeNominal inRangeNominal,
            IUserItemOperation userItemOperation) : base(userItemOperation)
        {
            OperationOhmRangeCode = Mult107_109N.RangeCode.Range2Manual;
            OperationOhmRangeNominal = inRangeNominal;
            RangeResolution = new AcVariablePoint(100, Multipliers.Mili);
            Name = OperationOhmRangeNominal.GetStringValue();
            OpMultipliers = Multipliers.Kilo;

            BaseMultipliers = 1;
            OhmPoint = new MeasPoint[5];
            OhmPoint[0] = new MeasPoint(MeasureUnits.Ohm, OpMultipliers, (decimal) 0.4 * BaseMultipliers);
            OhmPoint[1] = new MeasPoint(MeasureUnits.Ohm, OpMultipliers, (decimal) 0.8 * BaseMultipliers);
            OhmPoint[2] = new MeasPoint(MeasureUnits.Ohm, OpMultipliers, 1 * BaseMultipliers);
            OhmPoint[3] = new MeasPoint(MeasureUnits.Ohm, OpMultipliers, (decimal) 1.5 * BaseMultipliers);
            OhmPoint[4] = new MeasPoint(MeasureUnits.Ohm, OpMultipliers, (decimal) 1.8 * BaseMultipliers);
        }
    }

    public class Oper8_1Resistance_20kOhm_Measure : Oper8ResistanceMeasureBase
    {
        public Oper8_1Resistance_20kOhm_Measure(Mult107_109N.RangeNominal inRangeNominal,
            IUserItemOperation userItemOperation) : base(userItemOperation)
        {
            OperationOhmRangeCode = Mult107_109N.RangeCode.Range3Manual;
            OperationOhmRangeNominal = inRangeNominal;
            RangeResolution = new AcVariablePoint(1, Multipliers.None);
            Name = OperationOhmRangeNominal.GetStringValue();
            OpMultipliers = Multipliers.Kilo;

            BaseMultipliers = 10;
            OhmPoint = new MeasPoint[5];
            OhmPoint[0] = new MeasPoint(MeasureUnits.Ohm, OpMultipliers, (decimal) 0.4 * BaseMultipliers);
            OhmPoint[1] = new MeasPoint(MeasureUnits.Ohm, OpMultipliers, (decimal) 0.8 * BaseMultipliers);
            OhmPoint[2] = new MeasPoint(MeasureUnits.Ohm, OpMultipliers, 1 * BaseMultipliers);
            OhmPoint[3] = new MeasPoint(MeasureUnits.Ohm, OpMultipliers, (decimal) 1.5 * BaseMultipliers);
            OhmPoint[4] = new MeasPoint(MeasureUnits.Ohm, OpMultipliers, (decimal) 1.8 * BaseMultipliers);
        }
    }

    public class Oper8_1Resistance_200kOhm_Measure : Oper8ResistanceMeasureBase
    {
        public Oper8_1Resistance_200kOhm_Measure(Mult107_109N.RangeNominal inRangeNominal,
            IUserItemOperation userItemOperation) : base(userItemOperation)
        {
            OperationOhmRangeCode = Mult107_109N.RangeCode.Range4Manual;
            OperationOhmRangeNominal = inRangeNominal;
            RangeResolution = new AcVariablePoint(10, Multipliers.None);
            Name = OperationOhmRangeNominal.GetStringValue();
            OpMultipliers = Multipliers.Kilo;

            BaseMultipliers = 100;
            OhmPoint = new MeasPoint[5];
            OhmPoint[0] = new MeasPoint(MeasureUnits.Ohm, OpMultipliers, (decimal) 0.4 * BaseMultipliers);
            OhmPoint[1] = new MeasPoint(MeasureUnits.Ohm, OpMultipliers, (decimal) 0.8 * BaseMultipliers);
            OhmPoint[2] = new MeasPoint(MeasureUnits.Ohm, OpMultipliers, 1 * BaseMultipliers);
            OhmPoint[3] = new MeasPoint(MeasureUnits.Ohm, OpMultipliers, (decimal) 1.5 * BaseMultipliers);
            OhmPoint[4] = new MeasPoint(MeasureUnits.Ohm, OpMultipliers, (decimal) 1.8 * BaseMultipliers);
        }
    }

    public class Oper8_1Resistance_2MOhm_Measure : Oper8ResistanceMeasureBase
    {
        public Oper8_1Resistance_2MOhm_Measure(Mult107_109N.RangeNominal inRangeNominal,
            IUserItemOperation userItemOperation) : base(userItemOperation)
        {
            OperationOhmRangeCode = Mult107_109N.RangeCode.Range5Manual;
            OperationOhmRangeNominal = inRangeNominal;
            RangeResolution = new AcVariablePoint(10, Multipliers.None);
            Name = OperationOhmRangeNominal.GetStringValue();
            OpMultipliers = Multipliers.Mega;

            BaseMultipliers = 1000;
            OhmPoint = new MeasPoint[5];
            OhmPoint[0] = new MeasPoint(MeasureUnits.Ohm, OpMultipliers, (decimal) 0.4 * BaseMultipliers);
            OhmPoint[1] = new MeasPoint(MeasureUnits.Ohm, OpMultipliers, (decimal) 0.8 * BaseMultipliers);
            OhmPoint[2] = new MeasPoint(MeasureUnits.Ohm, OpMultipliers, 1 * BaseMultipliers);
            OhmPoint[3] = new MeasPoint(MeasureUnits.Ohm, OpMultipliers, (decimal) 1.5 * BaseMultipliers);
            OhmPoint[4] = new MeasPoint(MeasureUnits.Ohm, OpMultipliers, (decimal) 1.8 * BaseMultipliers);
        }
    }

    public class Oper8_1Resistance_20MOhm_Measure : Oper8ResistanceMeasureBase
    {
        public Oper8_1Resistance_20MOhm_Measure(Mult107_109N.RangeNominal inRangeNominal,
            IUserItemOperation userItemOperation) : base(userItemOperation)
        {
            OperationOhmRangeCode = Mult107_109N.RangeCode.Range6Manual;
            OperationOhmRangeNominal = inRangeNominal;
            RangeResolution = new AcVariablePoint(10, Multipliers.Mili);
            Name = OperationOhmRangeNominal.GetStringValue();
            OpMultipliers = Multipliers.Mega;

            BaseMultipliers = 1;
            OhmPoint = new MeasPoint[3];
            OhmPoint[0] = new MeasPoint(MeasureUnits.Ohm, OpMultipliers, 5 * BaseMultipliers);
            OhmPoint[1] = new MeasPoint(MeasureUnits.Ohm, OpMultipliers, 10 * BaseMultipliers);
            OhmPoint[2] = new MeasPoint(MeasureUnits.Ohm, OpMultipliers, 20 * BaseMultipliers);
        }
    }

    public class Oper8_1Resistance_200MOhm_Measure : Oper8ResistanceMeasureBase
    {
        public Oper8_1Resistance_200MOhm_Measure(Mult107_109N.RangeNominal inRangeNominal,
            IUserItemOperation userItemOperation) : base(userItemOperation)
        {
            OperationOhmRangeCode = Mult107_109N.RangeCode.Range7Manual;
            OperationOhmRangeNominal = inRangeNominal;
            RangeResolution = new AcVariablePoint(10, Multipliers.Mili);
            Name = OperationOhmRangeNominal.GetStringValue();
            OpMultipliers = Multipliers.Mega;

            BaseMultipliers = 10;
            OhmPoint = new MeasPoint[3];
            OhmPoint[0] = new MeasPoint(MeasureUnits.Ohm, OpMultipliers, 5 * BaseMultipliers);
            OhmPoint[1] = new MeasPoint(MeasureUnits.Ohm, OpMultipliers, 10 * BaseMultipliers);
            OhmPoint[2] = new MeasPoint(MeasureUnits.Ohm, OpMultipliers, 20 * BaseMultipliers);
        }
    }

    public class Oper8_1Resistance_2GOhm_Measure : Oper8ResistanceMeasureBase
    {
        public Oper8_1Resistance_2GOhm_Measure(Mult107_109N.RangeNominal inRangeNominal,
            IUserItemOperation userItemOperation) : base(userItemOperation)
        {
            OperationOhmRangeCode = Mult107_109N.RangeCode.Range8Manual;
            OperationOhmRangeNominal = inRangeNominal;
            RangeResolution = new AcVariablePoint(10, Multipliers.Mili);
            Name = OperationOhmRangeNominal.GetStringValue();
            OpMultipliers = Multipliers.Giga;

            OhmPoint = new MeasPoint[1];
            OhmPoint[0] = new MeasPoint(MeasureUnits.Ohm, OpMultipliers, (decimal) 0.9);
        }
    }

    public class Oper8ResistanceMeasureBase : ParagraphBase, IUserItemOperationBase
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        #region Fields

        //множители для пределов.
        public decimal BaseMultipliers;

        public decimal BaseTolCoeff;

        /// <summary>
        /// довесок к формуле погрешности- единица младшего разряда
        /// </summary>
        public int EdMlRaz = 10; //значение для пределов свыше 200 мВ

        /// <summary>
        /// Массив поверяемых точек напряжения.
        /// </summary>
        protected MeasPoint[] OhmPoint;

        protected AcVariablePoint RangeResolution;

        #endregion

        #region Property

        public List<IBasicOperation<decimal>> DataRow { get; set; }

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

        public Oper8ResistanceMeasureBase(IUserItemOperation userItemOperation) : base(userItemOperation)
        {
            Name = "Определение погрешности измерения электрического сопротивления";
            OperMeasureMode = Mult107_109N.MeasureMode.Ohm;
            OpMultipliers = Multipliers.None;
            OperationOhmRangeCode = Mult107_109N.RangeCode.Range1Manual;
            OperationOhmRangeNominal = Mult107_109N.RangeNominal.RangeNone;

            DataRow = new List<IBasicOperation<decimal>>();
            Sheme = ShemeTemplateDefault.TemplateSheme;
            OpMultipliers = Multipliers.None;
        }

        #region Methods

        protected override DataTable FillData()
        {
            throw new NotImplementedException();
        }

        protected override void InitWork()
        {
            if (appa107N == null || flkCalib5522A == null) return;

            DataRow.Clear();
            foreach (var currPoint in OhmPoint)
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
                                             .Show($"Установите режим измерения: {OperMeasureMode.GetStringValue()} {OperMeasureMode}",
                                                   "Указание оператору", MessageButton.OK,
                                                   MessageIcon.Information,
                                                   MessageResult.OK);

                        var testRangeNominal = appa107N.GetRangeNominal;
                        while (OperationOhmRangeNominal != appa107N.GetRangeNominal)
                        {
                            var countPushRangeButton =
                                appa107N.GetRangeSwitchMode != Mult107_109N.RangeSwitchMode.Auto ? 1 : 0;

                            if (OpMultipliers == Multipliers.Mili)
                            {
                                UserItemOperation.ServicePack.MessageBox
                                                 .Show($"Текущий предел измерения прибора {appa107N.GetRangeNominal.GetStringValue()}\n Необходимо установить предел {OperationOhmRangeNominal.GetStringValue()} " +
                                                       $"Нажмите на приборе клавишу Range {countPushRangeButton + 1} раз.",
                                                       "Указание оператору", MessageButton.OK, MessageIcon.Information,
                                                       MessageResult.OK);
                            }
                            else
                            {
                                var curRange = (int) appa107N.GetRangeCode & 128;
                                var targetRange = (int) OperationOhmRangeCode & 128;
                                countPushRangeButton = countPushRangeButton + 4 - curRange +
                                                       (targetRange < curRange ? curRange : 0);

                                UserItemOperation.ServicePack.MessageBox
                                                 .Show($"Текущий предел измерения прибора {appa107N.GetRangeNominal.GetStringValue()}\n Необходимо установить предел {OperationOhmRangeNominal.GetStringValue()} " +
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
                        flkCalib5522A.Out.Set.Resistance.SetValue(currPoint.NominalVal *
                                                                  (decimal) currPoint.MultipliersUnit.GetDoubleValue());
                        flkCalib5522A.Out.SetOutput(CalibrMain.COut.State.On);
                        Thread.Sleep(2000);
                        //измеряем
                        var measurePoint = (decimal) appa107N.GetValue();

                        flkCalib5522A.Out.SetOutput(CalibrMain.COut.State.Off);

                        operation.Getting = measurePoint;
                        operation.Expected = currPoint.NominalVal /
                                             (decimal) currPoint
                                                      .MultipliersUnit.GetDoubleValue();
                        //расчет погрешности для конкретной точки предела измерения
                        operation.ErrorCalculation = (inA, inB) =>
                        {
                            var result = BaseTolCoeff * operation.Expected + EdMlRaz *
                                RangeResolution.VariableBaseValueMeasPoint.NominalVal *
                                (decimal) (RangeResolution
                                          .VariableBaseValueMeasPoint.MultipliersUnit.GetDoubleValue() /
                                           currPoint.MultipliersUnit.GetDoubleValue()
                                );
                            var mantisa =
                                MathStatistics.GetMantissa((decimal) (RangeResolution
                                                                     .VariableBaseValueMeasPoint.MultipliersUnit
                                                                     .GetDoubleValue() /
                                                                      currPoint.MultipliersUnit.GetDoubleValue()));
                            MathStatistics.Round(ref result, mantisa);
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
    }

    #endregion OHM

    //////////////////////////////******FAR*******///////////////////////////////

    #region FAR

    public class Oper9FarMeasureBase : ParagraphBase, IUserItemOperationBase
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        #region Fields

        //множители для пределов.
        public decimal BaseMultipliers;

        public decimal BaseTolCoeff;

        /// <summary>
        /// довесок к формуле погрешности- единица младшего разряда
        /// </summary>
        public int EdMlRaz; //значение для пределов свыше 200 мВ

        /// <summary>
        /// Массив поверяемых точек напряжения.
        /// </summary>
        protected MeasPoint[] FarMeasPoints;

        protected AcVariablePoint RangeResolution;

        #endregion

        #region Property

        public List<IBasicOperation<decimal>> DataRow { get; set; }

        /// <summary>
        /// Код предела измерения на приборе
        /// </summary>
        public Mult107_109N.RangeCode OperationRangeCode { get; protected set; }

        /// <summary>
        /// Предел измерения поверяемого прибора, необходимый для работы
        /// </summary>
        public Mult107_109N.RangeNominal OperationRangeNominal { get; protected set; }

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

        public Oper9FarMeasureBase(IUserItemOperation userItemOperation) : base(userItemOperation)
        {
            Name = "Определение погрешности измерения электрической ёмкости";
            OperMeasureMode = Mult107_109N.MeasureMode.Cap;
            OpMultipliers = Multipliers.None;
            OperationRangeCode = Mult107_109N.RangeCode.Range1Manual;
            OperationRangeNominal = Mult107_109N.RangeNominal.RangeNone;

            DataRow = new List<IBasicOperation<decimal>>();
            Sheme = ShemeTemplateDefault.TemplateSheme;
            OpMultipliers = Multipliers.None;
        }

        #region Methods

        protected override DataTable FillData()
        {
            throw new NotImplementedException();
        }

        protected override void InitWork()
        {
            if (appa107N == null || flkCalib5522A == null) return;

            DataRow.Clear();
            foreach (var currPoint in FarMeasPoints)
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
                                             .Show($"Установите режим измерения: {OperMeasureMode.GetStringValue()} {OperMeasureMode}",
                                                   "Указание оператору", MessageButton.OK,
                                                   MessageIcon.Information,
                                                   MessageResult.OK);

                        var testRangeNominal = appa107N.GetRangeNominal;
                        while (OperationRangeNominal != appa107N.GetRangeNominal)
                        {
                            var countPushRangeButton =
                                appa107N.GetRangeSwitchMode != Mult107_109N.RangeSwitchMode.Auto ? 1 : 0;

                            if (OpMultipliers == Multipliers.Mili)
                            {
                                UserItemOperation.ServicePack.MessageBox
                                                 .Show($"Текущий предел измерения прибора {appa107N.GetRangeNominal.GetStringValue()}\n Необходимо установить предел {OperationRangeNominal.GetStringValue()} " +
                                                       $"Нажмите на приборе клавишу Range {countPushRangeButton + 1} раз.",
                                                       "Указание оператору", MessageButton.OK, MessageIcon.Information,
                                                       MessageResult.OK);
                            }
                            else
                            {
                                var curRange = (int) appa107N.GetRangeCode & 128;
                                var targetRange = (int) OperationRangeCode & 128;
                                countPushRangeButton = countPushRangeButton + 4 - curRange +
                                                       (targetRange < curRange ? curRange : 0);

                                UserItemOperation.ServicePack.MessageBox
                                                 .Show($"Текущий предел измерения прибора {appa107N.GetRangeNominal.GetStringValue()}\n Необходимо установить предел {OperationRangeNominal.GetStringValue()} " +
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
                        flkCalib5522A.Out.Set.Capacitance.SetValue(currPoint.NominalVal *
                                                                   (decimal) currPoint
                                                                            .MultipliersUnit.GetDoubleValue());
                        flkCalib5522A.Out.SetOutput(CalibrMain.COut.State.On);
                        Thread.Sleep(2000);
                        //измеряем
                        var measurePoint = (decimal) appa107N.GetValue();
                        flkCalib5522A.Out.SetOutput(CalibrMain.COut.State.Off);

                        operation.Getting = measurePoint;
                        operation.Expected = currPoint.NominalVal /
                                             (decimal) currPoint
                                                      .MultipliersUnit.GetDoubleValue();
                        //расчет погрешности для конкретной точки предела измерения
                        operation.ErrorCalculation = (inA, inB) =>
                        {
                            var result = BaseTolCoeff * operation.Expected + EdMlRaz *
                                RangeResolution.VariableBaseValueMeasPoint.NominalVal *
                                (decimal) (RangeResolution
                                          .VariableBaseValueMeasPoint.MultipliersUnit.GetDoubleValue() /
                                           currPoint.MultipliersUnit.GetDoubleValue()
                                );
                            var mantisa =
                                MathStatistics.GetMantissa((decimal) (RangeResolution
                                                                     .VariableBaseValueMeasPoint.MultipliersUnit
                                                                     .GetDoubleValue() /
                                                                      currPoint.MultipliersUnit.GetDoubleValue()));
                            MathStatistics.Round(ref result, mantisa);
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
    }

    public class Oper9_1Far_4nF_Measure : Oper9FarMeasureBase
    {
        public Oper9_1Far_4nF_Measure(Mult107_109N.RangeNominal inRangeNominal, IUserItemOperation userItemOperation) :
            base(userItemOperation)
        {
            OperationRangeCode = Mult107_109N.RangeCode.Range1Manual;
            OperationRangeNominal = inRangeNominal;
            RangeResolution = new AcVariablePoint(1, Multipliers.Pico);
            Name = OperationRangeNominal.GetStringValue();
            OpMultipliers = Multipliers.Nano;

            FarMeasPoints = new MeasPoint[1];
            FarMeasPoints[0] = new MeasPoint(MeasureUnits.Far, OpMultipliers, 3);
        }
    }

    public class Oper9_1Far_40nF_Measure : Oper9FarMeasureBase
    {
        public Oper9_1Far_40nF_Measure(Mult107_109N.RangeNominal inRangeNominal, IUserItemOperation userItemOperation) :
            base(userItemOperation)
        {
            OperationRangeCode = Mult107_109N.RangeCode.Range2Manual;
            OperationRangeNominal = inRangeNominal;
            RangeResolution = new AcVariablePoint(10, Multipliers.Pico);
            Name = OperationRangeNominal.GetStringValue();
            OpMultipliers = Multipliers.Nano;

            FarMeasPoints = new MeasPoint[1];
            FarMeasPoints[0] = new MeasPoint(MeasureUnits.Far, OpMultipliers, 30);
        }
    }

    public class Oper9_1Far_400nF_Measure : Oper9FarMeasureBase
    {
        public Oper9_1Far_400nF_Measure(Mult107_109N.RangeNominal inRangeNominal, IUserItemOperation userItemOperation)
            : base(userItemOperation)
        {
            OperationRangeCode = Mult107_109N.RangeCode.Range3Manual;
            OperationRangeNominal = inRangeNominal;
            RangeResolution = new AcVariablePoint(100, Multipliers.Pico);
            Name = OperationRangeNominal.GetStringValue();
            OpMultipliers = Multipliers.Nano;

            FarMeasPoints = new MeasPoint[1];
            FarMeasPoints[0] = new MeasPoint(MeasureUnits.Far, OpMultipliers, 300);
        }
    }

    public class Oper9_1Far_4uF_Measure : Oper9FarMeasureBase
    {
        public Oper9_1Far_4uF_Measure(Mult107_109N.RangeNominal inRangeNominal, IUserItemOperation userItemOperation) :
            base(userItemOperation)
        {
            OperationRangeCode = Mult107_109N.RangeCode.Range4Manual;
            OperationRangeNominal = inRangeNominal;
            RangeResolution = new AcVariablePoint(1, Multipliers.Nano);
            Name = OperationRangeNominal.GetStringValue();
            OpMultipliers = Multipliers.Micro;

            FarMeasPoints = new MeasPoint[1];
            FarMeasPoints[0] = new MeasPoint(MeasureUnits.Far, OpMultipliers, 3);
        }
    }

    public class Oper9_1Far_40uF_Measure : Oper9FarMeasureBase
    {
        public Oper9_1Far_40uF_Measure(Mult107_109N.RangeNominal inRangeNominal, IUserItemOperation userItemOperation) :
            base(userItemOperation)
        {
            OperationRangeCode = Mult107_109N.RangeCode.Range5Manual;
            OperationRangeNominal = inRangeNominal;
            RangeResolution = new AcVariablePoint(10, Multipliers.Nano);
            Name = OperationRangeNominal.GetStringValue();
            OpMultipliers = Multipliers.Micro;

            FarMeasPoints = new MeasPoint[1];
            FarMeasPoints[0] = new MeasPoint(MeasureUnits.Far, OpMultipliers, 30);
        }
    }

    public class Oper9_1Far_400uF_Measure : Oper9FarMeasureBase
    {
        public Oper9_1Far_400uF_Measure(Mult107_109N.RangeNominal inRangeNominal, IUserItemOperation userItemOperation)
            : base(userItemOperation)
        {
            OperationRangeCode = Mult107_109N.RangeCode.Range6Manual;
            OperationRangeNominal = inRangeNominal;
            RangeResolution = new AcVariablePoint(100, Multipliers.Nano);
            Name = OperationRangeNominal.GetStringValue();
            OpMultipliers = Multipliers.Micro;

            FarMeasPoints = new MeasPoint[1];
            FarMeasPoints[0] = new MeasPoint(MeasureUnits.Far, OpMultipliers, 300);
        }
    }

    public class Oper9_1Far_4mF_Measure : Oper9FarMeasureBase
    {
        public Oper9_1Far_4mF_Measure(Mult107_109N.RangeNominal inRangeNominal, IUserItemOperation userItemOperation) :
            base(userItemOperation)
        {
            OperationRangeCode = Mult107_109N.RangeCode.Range7Manual;
            OperationRangeNominal = inRangeNominal;
            RangeResolution = new AcVariablePoint(1, Multipliers.Micro);
            Name = OperationRangeNominal.GetStringValue();
            OpMultipliers = Multipliers.Mili;

            FarMeasPoints = new MeasPoint[1];
            FarMeasPoints[0] = new MeasPoint(MeasureUnits.Far, OpMultipliers, 3);
        }
    }

    public class Oper9_1Far_40mF_Measure : Oper9FarMeasureBase
    {
        public Oper9_1Far_40mF_Measure(Mult107_109N.RangeNominal inRangeNominal, IUserItemOperation userItemOperation) :
            base(userItemOperation)
        {
            OperationRangeCode = Mult107_109N.RangeCode.Range8Manual;
            OperationRangeNominal = inRangeNominal;
            RangeResolution = new AcVariablePoint(10, Multipliers.Micro);
            Name = OperationRangeNominal.GetStringValue();
            OpMultipliers = Multipliers.Mili;

            FarMeasPoints = new MeasPoint[1];
            FarMeasPoints[0] = new MeasPoint(MeasureUnits.Far, OpMultipliers, 30);
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
    }

    #endregion TEMP
}