using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AP.Math;
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
                        var measurePoint = (decimal) appa107N.GetSingleValue();

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
                                           currPoint.VariableBaseValueMeasPoint.MultipliersUnit.GetDoubleValue()
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
                    var measurePoint = (decimal) appa107N.GetSingleValue();
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
            HerzVPoint[0] = new MeasPoint();
            HerzVPoint[0].MultipliersUnit = Multipliers.Mili;
            HerzVPoint[0].NominalVal = 40;

            HerzVPoint[1] = new MeasPoint();
            HerzVPoint[1].MultipliersUnit = Multipliers.None;
            HerzVPoint[1].NominalVal = 1000;

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
            HerzVPoint[0] = new MeasPoint();
            HerzVPoint[0].MultipliersUnit = Multipliers.None;
            HerzVPoint[0].NominalVal = 40;

            HerzVPoint[1] = new MeasPoint();
            HerzVPoint[1].MultipliersUnit = Multipliers.None;
            HerzVPoint[1].NominalVal = 1000;

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
            HerzVPoint[0] = new MeasPoint();
            HerzVPoint[1] = new MeasPoint();
            HerzVPoint[2] = new MeasPoint();
            HerzVPoint[3] = new MeasPoint();
            HerzVPoint[4] = new MeasPoint();
            HerzVPoint[5] = new MeasPoint();

            HerzVPoint[0].NominalVal = 40 * VoltMultipliers;
            HerzVPoint[0].MultipliersUnit = Multipliers.None;
            HerzVPoint[1].NominalVal = 1000 * VoltMultipliers;
            HerzVPoint[1].MultipliersUnit = Multipliers.None;
            HerzVPoint[2].NominalVal = 10 * VoltMultipliers;
            HerzVPoint[2].MultipliersUnit = Multipliers.Kilo;
            HerzVPoint[3].NominalVal = 20 * VoltMultipliers;
            HerzVPoint[3].MultipliersUnit = Multipliers.Kilo;
            HerzVPoint[4].NominalVal = 50 * VoltMultipliers;
            HerzVPoint[4].MultipliersUnit = Multipliers.Kilo;
            HerzVPoint[5].NominalVal = 100 * VoltMultipliers;
            HerzVPoint[5].MultipliersUnit = Multipliers.Kilo;

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
            HerzVPoint[0] = new MeasPoint();
            HerzVPoint[1] = new MeasPoint();
            HerzVPoint[2] = new MeasPoint();
            HerzVPoint[3] = new MeasPoint();
            HerzVPoint[4] = new MeasPoint();
            HerzVPoint[5] = new MeasPoint();

            HerzVPoint[0].NominalVal = 40;
            HerzVPoint[0].MultipliersUnit = Multipliers.None;
            HerzVPoint[1].NominalVal = 1000;
            HerzVPoint[1].MultipliersUnit = Multipliers.None;
            HerzVPoint[2].NominalVal = 10;
            HerzVPoint[2].MultipliersUnit = Multipliers.Kilo;
            HerzVPoint[3].NominalVal = 20;
            HerzVPoint[3].MultipliersUnit = Multipliers.Kilo;
            HerzVPoint[4].NominalVal = 50;
            HerzVPoint[4].MultipliersUnit = Multipliers.Kilo;
            HerzVPoint[5].NominalVal = 100;
            HerzVPoint[5].MultipliersUnit = Multipliers.Kilo;

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
            HerzVPoint[0] = new MeasPoint();
            HerzVPoint[1] = new MeasPoint();
            HerzVPoint[2] = new MeasPoint();
            HerzVPoint[3] = new MeasPoint();
            HerzVPoint[4] = new MeasPoint();
            HerzVPoint[5] = new MeasPoint();

            HerzVPoint[0].NominalVal = 40;
            HerzVPoint[0].MultipliersUnit = Multipliers.None;
            HerzVPoint[1].NominalVal = 1000;
            HerzVPoint[1].MultipliersUnit = Multipliers.None;
            HerzVPoint[2].NominalVal = 10;
            HerzVPoint[2].MultipliersUnit = Multipliers.Kilo;
            HerzVPoint[3].NominalVal = 20;
            HerzVPoint[3].MultipliersUnit = Multipliers.Kilo;
            HerzVPoint[4].NominalVal = 50;
            HerzVPoint[4].MultipliersUnit = Multipliers.Kilo;
            HerzVPoint[5].NominalVal = 100;
            HerzVPoint[5].MultipliersUnit = Multipliers.Kilo;

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
            HerzVPoint[0] = new MeasPoint();
            HerzVPoint[0].NominalVal = 40;
            HerzVPoint[0].MultipliersUnit = Multipliers.None;
            HerzVPoint[1] = new MeasPoint();
            HerzVPoint[1].NominalVal = 1000;
            HerzVPoint[1].MultipliersUnit = Multipliers.None;

            VoltPoint = new AcVariablePoint[3];
            VoltPoint[0] = new AcVariablePoint(100 * VoltMultipliers, OpMultipliers, HerzVPoint);
            VoltPoint[1] = new AcVariablePoint(400 * VoltMultipliers, OpMultipliers, HerzVPoint);
            VoltPoint[2] = new AcVariablePoint(700 * VoltMultipliers, OpMultipliers, HerzVPoint);
        }
    }

    #endregion ACV

    //////////////////////////////******DCI*******///////////////////////////////

    #region DCI

    public abstract class Oper5DciMeasureBase : ParagraphBase, IUserItemOperation<decimal>
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
        protected AcVariablePoint[] CurrentDciPoint;

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
        public Mult107_109N.RangeCode OperationDciRangeCode { get; protected set; }

        /// <summary>
        /// Предел измерения поверяемого прибора, необходимый для работы
        /// </summary>
        public Mult107_109N.RangeNominal OperationDciRangeNominal { get; protected set; }

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
            OperationDciRangeCode = Mult107_109N.RangeCode.Range1Manual;
            OperationDciRangeNominal = Mult107_109N.RangeNominal.RangeNone;
            DataRow = new List<IBasicOperation<decimal>>();
            Sheme = new ShemeImage
            {
                Description = "Измерительная схема",
                Number = 1,
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
                        while (OperationDciRangeNominal != appa107N.GetRangeNominal)
                        {
                            var countPushRangeButton =
                                appa107N.GetRangeSwitchMode != Mult107_109N.RangeSwitchMode.Auto ? 1 : 0;

                            if (OpMultipliers == Multipliers.Mili)
                            {
                                UserItemOperation.ServicePack.MessageBox
                                                 .Show($"Текущий предел измерения прибора {appa107N.GetRangeNominal.GetStringValue()}\n Необходимо установить предел {OperationDciRangeNominal.GetStringValue()} " +
                                                       $"Нажмите на приборе клавишу Range {countPushRangeButton + 1} раз.",
                                                       "Указание оператору", MessageButton.OK, MessageIcon.Information,
                                                       MessageResult.OK);
                            }
                            else
                            {
                                var curRange = (int) appa107N.GetRangeCode & 128;
                                var targetRange = (int) OperationDciRangeCode & 128;
                                countPushRangeButton = countPushRangeButton + 4 - curRange +
                                                       (targetRange < curRange ? curRange : 0);

                                UserItemOperation.ServicePack.MessageBox
                                                 .Show($"Текущий предел измерения прибора {appa107N.GetRangeNominal.GetStringValue()}\n Необходимо установить предел {OperationDciRangeNominal.GetStringValue()} " +
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
                    flkCalib5522A.Out.Set.Current.Dc.SetValue(currPoint.VariableBaseValueMeasPoint.NominalVal);
                    flkCalib5522A.Out.SetOutput(CalibrMain.COut.State.On);
                    Thread.Sleep(2000);
                    //измеряем
                    var measurePoint = (decimal) appa107N.GetSingleValue();

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
                                       currPoint.VariableBaseValueMeasPoint.MultipliersUnit.GetDoubleValue()
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
                                : (BasicOperationVerefication<decimal>)operation.Clone());
            }
        }

        #endregion

       

        public List<IBasicOperation<decimal>> DataRow { get; set; }
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

    public abstract class Oper8OhmMeasureBase : ParagraphBase, IUserItemOperationBase
    {
        #region Fields

        //множители для пределов.
        public decimal _baseMultipliers;

        /// <summary>
        /// Массив поверяемых точек напряжения.
        /// </summary>
        protected AcVariablePoint[] _ohmPoint;

        protected AcVariablePoint _rangeResolution;

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