using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
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
using DevExpress.Mvvm.POCO;
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

        public override void FindDevice()
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
                        
                        if (appa107N.StringConnection.Equals("COM1")) appa107N.StringConnection = GetStringConnect(appa107N);
                        flkCalib5522A.StringConnection ??= GetStringConnect(flkCalib5522A);

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
                                                       $"Нажмите на приборе клавишу Range {countPushRangeButton} раз.",
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
                        flkCalib5522A.Out.Set.Voltage.Dc.SetValue(currPoint.VariableBaseValueMeasPoint.NominalVal * (decimal)currPoint.VariableBaseValueMeasPoint.MultipliersUnit.GetDoubleValue());
                        flkCalib5522A.Out.ClearMemoryRegister();
                        flkCalib5522A.Out.SetOutput(CalibrMain.COut.State.On);
                        Thread.Sleep(2000);
                        //измеряем
                        var measurePoint = (decimal) appa107N.GetValue();

                        flkCalib5522A.Out.SetOutput(CalibrMain.COut.State.Off);

                        operation.Getting = measurePoint;
                        operation.Expected = currPoint.VariableBaseValueMeasPoint.NominalVal;
                        //расчет погрешности для конкретной точки предела измерения
                        operation.ErrorCalculation = (inA, inB) =>
                        {
                            var result = BaseTolCoeff * Math.Abs(operation.Expected)  + EdMlRaz *
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
                                                                               .GetDoubleValue()), CultureInfo.CurrentCulture);
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

        public async override Task StartWork(CancellationToken token)
        {
            await base.StartWork(token);
            appa107N?.Dispose();
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
            
            Name = OperationDcRangeNominal.GetStringValue();
            OpMultipliers = Multipliers.None;

            BaseTolCoeff = (decimal)0.0006;
            EdMlRaz = 10;
            RangeResolution = new AcVariablePoint(100, MeasureUnits.V, Multipliers.Micro);

            BaseMultipliers = 100;
            VoltPoint = new AcVariablePoint[6];
            VoltPoint[0] = new AcVariablePoint((decimal) 0.4 , MeasureUnits.V,OpMultipliers);
            VoltPoint[1] = new AcVariablePoint((decimal) 0.8 , MeasureUnits.V,OpMultipliers);
            VoltPoint[2] = new AcVariablePoint((decimal) 1.2 , MeasureUnits.V,OpMultipliers);
            VoltPoint[3] = new AcVariablePoint((decimal) 1.6 , MeasureUnits.V,OpMultipliers);
            VoltPoint[4] = new AcVariablePoint((decimal) 1.8 , MeasureUnits.V,OpMultipliers);
            VoltPoint[5] = new AcVariablePoint((decimal) -1.8, MeasureUnits.V, OpMultipliers);
        }
    }

    public class Oper3_1DC_20V_Measure : Oper3DcvMeasureBase
    {
        public Oper3_1DC_20V_Measure(Mult107_109N.RangeNominal inRangeNominal, IUserItemOperation userItemOperation) :
            base(userItemOperation)
        {
            OperationDcRangeCode = Mult107_109N.RangeCode.Range2Manual;
            OperationDcRangeNominal = inRangeNominal;
            RangeResolution = new AcVariablePoint(1, MeasureUnits.V, Multipliers.Mili);
            Name = OperationDcRangeNominal.GetStringValue();
            OpMultipliers = Multipliers.None;

            BaseMultipliers = 1000;
            VoltPoint = new AcVariablePoint[6];
            VoltPoint[0] = new AcVariablePoint( 4 , MeasureUnits.V,OpMultipliers);
            VoltPoint[1] = new AcVariablePoint( 8 , MeasureUnits.V,OpMultipliers);
            VoltPoint[2] = new AcVariablePoint( 12, MeasureUnits.V,OpMultipliers);
            VoltPoint[3] = new AcVariablePoint( 16, MeasureUnits.V,OpMultipliers);
            VoltPoint[4] = new AcVariablePoint( 18, MeasureUnits.V,OpMultipliers);
            VoltPoint[5] = new AcVariablePoint( -18 , MeasureUnits.V, OpMultipliers);
        }
    }

    public class Oper3_1DC_200V_Measure : Oper3DcvMeasureBase
    {
        public Oper3_1DC_200V_Measure(Mult107_109N.RangeNominal inRangeNominal, IUserItemOperation userItemOperation) :
            base(userItemOperation)
        {
            OperationDcRangeCode = Mult107_109N.RangeCode.Range3Manual;
            OperationDcRangeNominal = inRangeNominal;
           
            Name = OperationDcRangeNominal.GetStringValue();
            OpMultipliers = Multipliers.None;

            BaseTolCoeff = (decimal)0.0006;
            EdMlRaz = 10;
            RangeResolution = new AcVariablePoint(10, MeasureUnits.V, Multipliers.Mili);

            BaseMultipliers = 10000;
            VoltPoint = new AcVariablePoint[6];
            VoltPoint[0] = new AcVariablePoint((decimal) 40 , MeasureUnits.V,OpMultipliers);
            VoltPoint[1] = new AcVariablePoint((decimal) 80 , MeasureUnits.V,OpMultipliers);
            VoltPoint[2] = new AcVariablePoint((decimal) 120 , MeasureUnits.V,OpMultipliers);
            VoltPoint[3] = new AcVariablePoint((decimal) 160 , MeasureUnits.V,OpMultipliers);
            VoltPoint[4] = new AcVariablePoint((decimal) 180 , MeasureUnits.V,OpMultipliers);
            VoltPoint[5] = new AcVariablePoint((decimal) -180, MeasureUnits.V, OpMultipliers);
        }
    }

    public class Oper3_1DC_1000V_Measure : Oper3DcvMeasureBase
    {
        public Oper3_1DC_1000V_Measure(Mult107_109N.RangeNominal inRangeNominal, IUserItemOperation userItemOperation) :
            base(userItemOperation)
        {
            OperationDcRangeCode = Mult107_109N.RangeCode.Range4Manual;
            OperationDcRangeNominal = inRangeNominal;
            
            Name = OperationDcRangeNominal.GetStringValue();
            OpMultipliers = Multipliers.None;

            BaseTolCoeff = (decimal)0.0006;
            EdMlRaz = 10;
            RangeResolution = new AcVariablePoint(100, MeasureUnits.V, Multipliers.Mili);

            BaseMultipliers = 1;
            VoltPoint = new AcVariablePoint[6];
            VoltPoint[0] = new AcVariablePoint(100 , MeasureUnits.V,OpMultipliers);
            VoltPoint[1] = new AcVariablePoint(200 , MeasureUnits.V,OpMultipliers);
            VoltPoint[2] = new AcVariablePoint(400 , MeasureUnits.V,OpMultipliers);
            VoltPoint[3] = new AcVariablePoint(700 , MeasureUnits.V,OpMultipliers);
            VoltPoint[4] = new AcVariablePoint(900 , MeasureUnits.V,OpMultipliers);
            VoltPoint[5] = new AcVariablePoint(-900, MeasureUnits.V, OpMultipliers);
        }
    }

    public class Oper3_1DC_20mV_Measure : Oper3DcvMeasureBase
    {
        public Oper3_1DC_20mV_Measure(Mult107_109N.RangeNominal inRangeNominal, IUserItemOperation userItemOperation) :
            base(userItemOperation)
        {
            OperationDcRangeCode = Mult107_109N.RangeCode.Range1Manual;
            OperationDcRangeNominal = inRangeNominal;
            RangeResolution = new AcVariablePoint(1, MeasureUnits.V, Multipliers.Micro);
            Name = OperationDcRangeNominal.GetStringValue();
            OpMultipliers = Multipliers.Mili;

            BaseTolCoeff = (decimal)0.0006;
            EdMlRaz = 60;
            RangeResolution = new AcVariablePoint(1, MeasureUnits.V, Multipliers.Micro);

            BaseMultipliers = 1;
            VoltPoint = new AcVariablePoint[6];
            VoltPoint[0] = new AcVariablePoint((decimal) 4   , MeasureUnits.V,OpMultipliers);
            VoltPoint[1] = new AcVariablePoint((decimal) 8   , MeasureUnits.V,OpMultipliers);
            VoltPoint[2] = new AcVariablePoint((decimal) 12  , MeasureUnits.V,OpMultipliers);
            VoltPoint[3] = new AcVariablePoint((decimal) 16  , MeasureUnits.V,OpMultipliers);
            VoltPoint[4] = new AcVariablePoint((decimal) 18  , MeasureUnits.V,OpMultipliers);
            VoltPoint[5] = new AcVariablePoint((decimal) -18 , MeasureUnits.V, OpMultipliers);
        }
    }

    public class Oper3_1DC_200mV_Measure : Oper3DcvMeasureBase
    {
        public Oper3_1DC_200mV_Measure(Mult107_109N.RangeNominal inRangeNominal, IUserItemOperation userItemOperation) :
            base(userItemOperation)
        {
            OperationDcRangeCode = Mult107_109N.RangeCode.Range1Manual;
            OperationDcRangeNominal = inRangeNominal;
            RangeResolution = new AcVariablePoint(10, MeasureUnits.V, Multipliers.Micro);
            Name = OperationDcRangeNominal.GetStringValue();
            OpMultipliers = Multipliers.Mili;

            BaseTolCoeff = (decimal)0.0006;
            EdMlRaz = 20;
            RangeResolution = new AcVariablePoint(10, MeasureUnits.V, Multipliers.Micro);

            BaseMultipliers = 10;
            VoltPoint = new AcVariablePoint[6];
            VoltPoint[0] = new AcVariablePoint((decimal)40, MeasureUnits.V, OpMultipliers);
            VoltPoint[1] = new AcVariablePoint((decimal)80, MeasureUnits.V, OpMultipliers);
            VoltPoint[2] = new AcVariablePoint((decimal)120, MeasureUnits.V, OpMultipliers);
            VoltPoint[3] = new AcVariablePoint((decimal)160, MeasureUnits.V, OpMultipliers);
            VoltPoint[4] = new AcVariablePoint((decimal)180, MeasureUnits.V, OpMultipliers);
            VoltPoint[5] = new AcVariablePoint((decimal)-180, MeasureUnits.V, OpMultipliers);
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

            RangeResolution = new AcVariablePoint(1, MeasureUnits.V, Multipliers.Micro);

            HerzVPoint = new MeasPoint[2];
            HerzVPoint[0] = new MeasPoint(MeasureUnits.Herz, Multipliers.None, 40);

            HerzVPoint[1] = new MeasPoint(MeasureUnits.Herz, Multipliers.None, 1000);

            VoltPoint = new AcVariablePoint[3];
            VoltPoint[0] = new AcVariablePoint(4 * VoltMultipliers, MeasureUnits.V, OpMultipliers, HerzVPoint);
            VoltPoint[1] = new AcVariablePoint(10 * VoltMultipliers, MeasureUnits.V, OpMultipliers, HerzVPoint);
            VoltPoint[2] = new AcVariablePoint(18 * VoltMultipliers, MeasureUnits.V, OpMultipliers, HerzVPoint);
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

            RangeResolution = new AcVariablePoint(10, MeasureUnits.V, Multipliers.Micro);

            HerzVPoint = new MeasPoint[2];
            HerzVPoint[0] = new MeasPoint(MeasureUnits.Herz, Multipliers.Mili, 40);
            HerzVPoint[1] = new MeasPoint(MeasureUnits.Herz, Multipliers.None, 1000);

            VoltPoint = new AcVariablePoint[3];
            VoltPoint[0] = new AcVariablePoint(4 * VoltMultipliers, MeasureUnits.V, OpMultipliers, HerzVPoint);
            VoltPoint[1] = new AcVariablePoint(10 * VoltMultipliers, MeasureUnits.V, OpMultipliers, HerzVPoint);
            VoltPoint[2] = new AcVariablePoint(18 * VoltMultipliers, MeasureUnits.V, OpMultipliers, HerzVPoint);
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

            RangeResolution = new AcVariablePoint((decimal) 0.1, MeasureUnits.V, Multipliers.Mili);

            HerzVPoint = new MeasPoint[6];
            HerzVPoint[0] = new MeasPoint(MeasureUnits.Herz, Multipliers.None, 40 * VoltMultipliers);
            HerzVPoint[1] = new MeasPoint(MeasureUnits.Herz, Multipliers.None, 1000 * VoltMultipliers);
            HerzVPoint[2] = new MeasPoint(MeasureUnits.Herz, Multipliers.Kilo, 10 * VoltMultipliers);
            HerzVPoint[3] = new MeasPoint(MeasureUnits.Herz, Multipliers.Kilo, 20 * VoltMultipliers);
            HerzVPoint[4] = new MeasPoint(MeasureUnits.Herz, Multipliers.Kilo, 50 * VoltMultipliers);
            HerzVPoint[5] = new MeasPoint(MeasureUnits.Herz, Multipliers.Kilo, 100 * VoltMultipliers);

            VoltPoint = new AcVariablePoint[3];
            //конкретно для первой точки 0.2 нужны не все частоты, поэтому вырежем только необходимые
            var trimHerzArr = new MeasPoint[4];
            Array.Copy(HerzVPoint, trimHerzArr, 4);
            VoltPoint[0] = new AcVariablePoint((decimal) 0.2, MeasureUnits.V, OpMultipliers, trimHerzArr);
            VoltPoint[1] = new AcVariablePoint(1, MeasureUnits.V, OpMultipliers, HerzVPoint);
            VoltPoint[2] = new AcVariablePoint((decimal) 1.8, MeasureUnits.V, OpMultipliers, HerzVPoint);
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

            RangeResolution = new AcVariablePoint(1, MeasureUnits.V, Multipliers.Mili);

            HerzVPoint = new MeasPoint[6];
            HerzVPoint[0] = new MeasPoint(MeasureUnits.Herz, Multipliers.None, 40 * VoltMultipliers);
            HerzVPoint[1] = new MeasPoint(MeasureUnits.Herz, Multipliers.None, 1000 * VoltMultipliers);
            HerzVPoint[2] = new MeasPoint(MeasureUnits.Herz, Multipliers.Kilo, 10 * VoltMultipliers);
            HerzVPoint[3] = new MeasPoint(MeasureUnits.Herz, Multipliers.Kilo, 20 * VoltMultipliers);
            HerzVPoint[4] = new MeasPoint(MeasureUnits.Herz, Multipliers.Kilo, 50 * VoltMultipliers);
            HerzVPoint[5] = new MeasPoint(MeasureUnits.Herz, Multipliers.Kilo, 100 * VoltMultipliers);

            VoltPoint = new AcVariablePoint[3];
            //конкретно для первой точки 0.2 нужны не все частоты, поэтому вырежем только необходимые
            var trimHerzArr = new MeasPoint[4];
            Array.Copy(HerzVPoint, trimHerzArr, 4);
            VoltPoint[0] = new AcVariablePoint((decimal) 0.2 * VoltMultipliers, MeasureUnits.V, OpMultipliers,
                                               trimHerzArr);
            VoltPoint[1] = new AcVariablePoint(1 * VoltMultipliers, MeasureUnits.V, OpMultipliers, HerzVPoint);
            VoltPoint[2] =
                new AcVariablePoint((decimal) 1.8 * VoltMultipliers, MeasureUnits.V, OpMultipliers, HerzVPoint);
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

            RangeResolution = new AcVariablePoint(10, MeasureUnits.V, Multipliers.Mili);

            HerzVPoint = new MeasPoint[6];
            HerzVPoint[0] = new MeasPoint(MeasureUnits.Herz, Multipliers.None, 40 * VoltMultipliers);
            HerzVPoint[1] = new MeasPoint(MeasureUnits.Herz, Multipliers.None, 1000 * VoltMultipliers);
            HerzVPoint[2] = new MeasPoint(MeasureUnits.Herz, Multipliers.Kilo, 10 * VoltMultipliers);
            HerzVPoint[3] = new MeasPoint(MeasureUnits.Herz, Multipliers.Kilo, 20 * VoltMultipliers);
            HerzVPoint[4] = new MeasPoint(MeasureUnits.Herz, Multipliers.Kilo, 50 * VoltMultipliers);
            HerzVPoint[5] = new MeasPoint(MeasureUnits.Herz, Multipliers.Kilo, 100 * VoltMultipliers);

            VoltPoint = new AcVariablePoint[3];
            //конкретно для первой точки 0.2 нужны не все частоты, поэтому вырежем только необходимые
            var trimHerzArr = new MeasPoint[4];
            Array.Copy(HerzVPoint, trimHerzArr, 4);
            VoltPoint[0] = new AcVariablePoint((decimal) 0.2 * VoltMultipliers, MeasureUnits.V, OpMultipliers,
                                               trimHerzArr);
            VoltPoint[1] = new AcVariablePoint(1 * VoltMultipliers, MeasureUnits.V, OpMultipliers, HerzVPoint);
            VoltPoint[2] =
                new AcVariablePoint((decimal) 1.8 * VoltMultipliers, MeasureUnits.V, OpMultipliers, HerzVPoint);
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

            RangeResolution = new AcVariablePoint(100, MeasureUnits.V, Multipliers.Mili);

            HerzVPoint = new MeasPoint[2];
            HerzVPoint[0] = new MeasPoint(MeasureUnits.Herz, OpMultipliers, 40);
            HerzVPoint[1] = new MeasPoint(MeasureUnits.Herz, OpMultipliers, 1000);

            VoltPoint = new AcVariablePoint[3];
            VoltPoint[0] = new AcVariablePoint(100 * VoltMultipliers, MeasureUnits.V, OpMultipliers, HerzVPoint);
            VoltPoint[1] = new AcVariablePoint(400 * VoltMultipliers, MeasureUnits.V, OpMultipliers, HerzVPoint);
            VoltPoint[2] = new AcVariablePoint(700 * VoltMultipliers, MeasureUnits.V, OpMultipliers, HerzVPoint);
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
            
            Name = OperationRangeNominal.GetStringValue();
            OpMultipliers = Multipliers.Mili;

            BaseTolCoeff = (decimal)0.002;
            EdMlRaz = 40;                
            RangeResolution = new AcVariablePoint(1, MeasureUnits.I, Multipliers.Micro);

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
            
            Name = OperationRangeNominal.GetStringValue();
            OpMultipliers = Multipliers.Mili;

            BaseTolCoeff = (decimal)0.002;
            EdMlRaz = 40;
            RangeResolution = new AcVariablePoint(10, MeasureUnits.I, Multipliers.Micro);

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
            
            Name = OperationRangeNominal.GetStringValue();
            OpMultipliers = Multipliers.None;

            BaseTolCoeff = (decimal)0.002;
            EdMlRaz = 40;
            RangeResolution = new AcVariablePoint(100, MeasureUnits.I, Multipliers.Micro);

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
            OperationRangeCode = Mult107_109N.RangeCode.Range2Manual;
            OperationRangeNominal = inRangeNominal;

            BaseTolCoeff = (decimal)0.002;
            EdMlRaz = 40;
            RangeResolution = new AcVariablePoint(1, MeasureUnits.I, Multipliers.Mili);

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
            OperationRangeCode = Mult107_109N.RangeCode.Range2Manual;
            OperationRangeNominal = inRangeNominal;
            Name = OperationRangeNominal.GetStringValue();
            OpMultipliers = Multipliers.None;

            BaseTolCoeff = (decimal)0.002;
            EdMlRaz = 40;
            RangeResolution = new AcVariablePoint(1, MeasureUnits.I, Multipliers.Mili);

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

    public class Oper6AciMeasureBase : ParagraphBase, IUserItemOperation<decimal>
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        #region Fields

        /// <summary>
        /// Итоговый массив поверяемых точек. У каждого номинала напряжения вложены номиналы частот для текущей точки.
        /// </summary>
        public AcVariablePoint[] AciPoint;

        /// <summary>
        /// Множитель для поверяемых точек. (Если точки можно посчитать простым умножением).
        /// </summary>
        protected decimal CurrentMultipliers;

        /// <summary>
        /// Набор частот, характерный для данного предела измерения
        /// </summary>
        protected MeasPoint[] HerzPoint;

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

        public Oper6AciMeasureBase(IUserItemOperation userItemOperation) : base(userItemOperation)
        {
            Name = "Определение погрешности измерения переменного напряжения";
            OperMeasureMode = Mult107_109N.MeasureMode.ACV;
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

        /// <summary>
        /// Метод выбирает необходимый значения коэффициентов для формулы погрешности, исходя из предела измерения и диапазона
        /// частот.
        /// </summary>
        /// <returns>Результат вычисления.</returns>
        protected void ConstructTooleranceFormula(MeasPoint inFreq)
        {
            if (OperationRangeNominal == Mult107_109N.RangeNominal.Range20mA &&
                inFreq.MultipliersUnit == Multipliers.None)
            {
                if (inFreq.NominalVal >= 40 && inFreq.NominalVal < 500)
                {
                    BaseTolCoeff = (decimal) 0.008;
                    EdMlRaz = 50;
                }

                if (inFreq.NominalVal >= 500 && inFreq.NominalVal < 1000)
                {
                    BaseTolCoeff = (decimal) 0.012;
                    EdMlRaz = 80;
                }
            }
            else if (OperationRangeNominal == Mult107_109N.RangeNominal.Range200mA ||
                     OperationRangeNominal == Mult107_109N.RangeNominal.Range2A ||
                     OperationRangeNominal == Mult107_109N.RangeNominal.Range10A)
            {
                if (inFreq.NominalVal >= 40 && inFreq.NominalVal < 500)
                {
                    BaseTolCoeff = (decimal) 0.008;
                    EdMlRaz = 50;
                }
                else if (inFreq.NominalVal >= 500 && inFreq.NominalVal < 1000)
                {
                    BaseTolCoeff = (decimal) 0.012;
                    EdMlRaz = 80;
                }
                else if (inFreq.NominalVal >= 1000 && inFreq.NominalVal <= 3000)
                {
                    BaseTolCoeff = (decimal) 0.02;
                    EdMlRaz = 80;
                }
            }
        }

        protected override DataTable FillData()
        {
            throw new NotImplementedException();
        }

        protected override void InitWork()
        {
            if (flkCalib5522A == null || appa107N == null) return;
            DataRow.Clear();

            foreach (var dciCurr in AciPoint)
            foreach (var freqPoint in dciCurr.Herz)
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
                                                       "Указание оператору", MessageButton.OK,
                                                       MessageIcon.Information,
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
                                                       "Указание оператору", MessageButton.OK,
                                                       MessageIcon.Information,
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
                    flkCalib5522A.Out.Set.Current.Ac.SetValue(dciCurr.VariableBaseValueMeasPoint.NominalVal,
                                                              freqPoint.NominalVal,
                                                              dciCurr.VariableBaseValueMeasPoint.MultipliersUnit,
                                                              freqPoint.MultipliersUnit);
                    flkCalib5522A.Out.SetOutput(CalibrMain.COut.State.On);
                    Thread.Sleep(2000);
                    //измеряем
                    var measurePoint = (decimal) appa107N.GetValue();
                    operation.Getting = measurePoint;

                    operation.Expected = dciCurr.VariableBaseValueMeasPoint.NominalVal /
                                         (decimal) dciCurr
                                                  .VariableBaseValueMeasPoint.MultipliersUnit.GetDoubleValue();

                    //расчет погрешности для конкретной точки предела измерения
                    ConstructTooleranceFormula(freqPoint); // функция подбирает коэффициенты для формулы погрешности
                    operation.ErrorCalculation = (inA, inB) =>
                    {
                        var result = BaseTolCoeff * operation.Expected + EdMlRaz *
                            RangeResolution.VariableBaseValueMeasPoint.NominalVal *
                            (decimal) (RangeResolution
                                      .VariableBaseValueMeasPoint.MultipliersUnit.GetDoubleValue() /
                                       dciCurr.VariableBaseValueMeasPoint.MultipliersUnit
                                              .GetDoubleValue());
                        var mantisa =
                            MathStatistics.GetMantissa((decimal) (RangeResolution
                                                                 .VariableBaseValueMeasPoint.MultipliersUnit
                                                                 .GetDoubleValue() /
                                                                  dciCurr.VariableBaseValueMeasPoint.MultipliersUnit
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
                                                                          MessageButton.YesNo,
                                                                          MessageIcon.Question,
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

    public class Oper6_1Aci_20mA_Measure : Oper6AciMeasureBase
    {
        public Oper6_1Aci_20mA_Measure(Mult107_109N.RangeNominal inRangeNominal,
            IUserItemOperation userItemOperation) : base(userItemOperation)
        {
            OperationRangeCode = Mult107_109N.RangeCode.Range1Manual;
            OperationRangeNominal = inRangeNominal;
            RangeResolution = new AcVariablePoint(1, MeasureUnits.I, Multipliers.Micro);
            Name = OperationRangeNominal.GetStringValue();
            OpMultipliers = Multipliers.Mili;

            CurrentMultipliers = 1;

            HerzPoint = new MeasPoint[2];
            HerzPoint[0] = new MeasPoint(MeasureUnits.Herz, Multipliers.None, 40);
            HerzPoint[1] = new MeasPoint(MeasureUnits.Herz, Multipliers.None, 1000);

            AciPoint = new AcVariablePoint[3];
            AciPoint[0] = new AcVariablePoint(4 * CurrentMultipliers, MeasureUnits.I, OpMultipliers, HerzPoint);
            AciPoint[1] = new AcVariablePoint(10 * CurrentMultipliers, MeasureUnits.I, OpMultipliers, HerzPoint);
            AciPoint[2] = new AcVariablePoint(18 * CurrentMultipliers, MeasureUnits.I, OpMultipliers, HerzPoint);
        }
    }

    public class Oper6_1Aci_200mA_Measure : Oper6AciMeasureBase
    {
        public Oper6_1Aci_200mA_Measure(Mult107_109N.RangeNominal inRangeNominal,
            IUserItemOperation userItemOperation) : base(userItemOperation)
        {
            OperationRangeCode = Mult107_109N.RangeCode.Range2Manual;
            OperationRangeNominal = inRangeNominal;
            RangeResolution = new AcVariablePoint(10, MeasureUnits.I, Multipliers.Micro);
            Name = OperationRangeNominal.GetStringValue();
            OpMultipliers = Multipliers.Mili;

            CurrentMultipliers = 10;

            HerzPoint = new MeasPoint[3];
            HerzPoint[0] = new MeasPoint(MeasureUnits.Herz, Multipliers.None, 40);
            HerzPoint[1] = new MeasPoint(MeasureUnits.Herz, Multipliers.None, 1000);
            HerzPoint[2] = new MeasPoint(MeasureUnits.Herz, Multipliers.Kilo, 3);

            AciPoint = new AcVariablePoint[3];
            AciPoint[0] = new AcVariablePoint(4 * CurrentMultipliers, MeasureUnits.I, OpMultipliers, HerzPoint);
            AciPoint[1] = new AcVariablePoint(10 * CurrentMultipliers, MeasureUnits.I, OpMultipliers, HerzPoint);
            AciPoint[2] = new AcVariablePoint(18 * CurrentMultipliers, MeasureUnits.I, OpMultipliers, HerzPoint);
        }
    }

    public class Oper6_1Aci_2A_Measure : Oper6AciMeasureBase
    {
        public Oper6_1Aci_2A_Measure(Mult107_109N.RangeNominal inRangeNominal,
            IUserItemOperation userItemOperation) : base(userItemOperation)
        {
            OperationRangeCode = Mult107_109N.RangeCode.Range1Manual;
            OperationRangeNominal = inRangeNominal;
            RangeResolution = new AcVariablePoint(100, MeasureUnits.I, Multipliers.Micro);
            Name = OperationRangeNominal.GetStringValue();
            OpMultipliers = Multipliers.None;

            CurrentMultipliers = (decimal) 0.1;

            HerzPoint = new MeasPoint[3];
            HerzPoint[0] = new MeasPoint(MeasureUnits.Herz, Multipliers.None, 40);
            HerzPoint[1] = new MeasPoint(MeasureUnits.Herz, Multipliers.None, 1000);
            HerzPoint[2] = new MeasPoint(MeasureUnits.Herz, Multipliers.Kilo, 3);

            AciPoint = new AcVariablePoint[3];
            AciPoint[0] = new AcVariablePoint(4 * CurrentMultipliers, MeasureUnits.I, OpMultipliers, HerzPoint);
            AciPoint[1] = new AcVariablePoint(10 * CurrentMultipliers, MeasureUnits.I, OpMultipliers, HerzPoint);
            AciPoint[2] = new AcVariablePoint(18 * CurrentMultipliers, MeasureUnits.I, OpMultipliers, HerzPoint);

            Sheme = new ShemeImage
            {
                Description = "Измерительная схема",
                Number = 3,
                FileName = "appa_10XN_A_Aux_5522A.jpg",
                ExtendedDescription = "Соберите измерительную схему, согласно рисунку"
            };
        }
    }

    public class Oper6_2_1Aci_10A_Measure : Oper6AciMeasureBase
    {
        public Oper6_2_1Aci_10A_Measure(Mult107_109N.RangeNominal inRangeNominal,
            IUserItemOperation userItemOperation) : base(userItemOperation)
        {
            OperationRangeCode = Mult107_109N.RangeCode.Range2Manual;
            OperationRangeNominal = inRangeNominal;
            RangeResolution = new AcVariablePoint(1, MeasureUnits.I, Multipliers.Mili);
            Name = OperationRangeNominal.GetStringValue();
            OpMultipliers = Multipliers.None;

            HerzPoint = new MeasPoint[3];
            HerzPoint[0] = new MeasPoint(MeasureUnits.Herz, Multipliers.None, 40);
            HerzPoint[1] = new MeasPoint(MeasureUnits.Herz, Multipliers.None, 1000);
            HerzPoint[2] = new MeasPoint(MeasureUnits.Herz, Multipliers.Kilo, 3);

            AciPoint = new AcVariablePoint[1];
            AciPoint[0] = new AcVariablePoint(2, MeasureUnits.I, OpMultipliers, HerzPoint);

            Sheme = new ShemeImage
            {
                Description = "Измерительная схема",
                Number = 3,
                FileName = "appa_10XN_A_Aux_5522A.jpg",
                ExtendedDescription = "Соберите измерительную схему, согласно рисунку"
            };
        }
    }

    public class Oper6_2_2Aci_10A_Measure : Oper6AciMeasureBase
    {
        public Oper6_2_2Aci_10A_Measure(Mult107_109N.RangeNominal inRangeNominal,
            IUserItemOperation userItemOperation) : base(userItemOperation)
        {
            OperationRangeCode = Mult107_109N.RangeCode.Range2Manual;
            OperationRangeNominal = inRangeNominal;
            RangeResolution = new AcVariablePoint(1, MeasureUnits.I, Multipliers.Mili);
            Name = OperationRangeNominal.GetStringValue();
            OpMultipliers = Multipliers.None;

            HerzPoint = new MeasPoint[3];
            HerzPoint[0] = new MeasPoint(MeasureUnits.Herz, Multipliers.None, 40);
            HerzPoint[1] = new MeasPoint(MeasureUnits.Herz, Multipliers.None, 1000);
            HerzPoint[2] = new MeasPoint(MeasureUnits.Herz, Multipliers.Kilo, 3);

            AciPoint = new AcVariablePoint[2];
            AciPoint[0] = new AcVariablePoint(5, MeasureUnits.I, OpMultipliers, HerzPoint);
            AciPoint[0] = new AcVariablePoint(9, MeasureUnits.I, OpMultipliers, HerzPoint);

            Sheme = new ShemeImage
            {
                Description = "Измерительная схема",
                Number = 4,
                FileName = "appa_10XN_20A_5522A.jpg",
                ExtendedDescription = "Соберите измерительную схему, согласно рисунку"
            };
        }
    }

    #endregion ACI

    //////////////////////////////******FREQ*******///////////////////////////////

    #region FREQ

    public class Oper7FreqMeasureBase : ParagraphBase, IUserItemOperation<decimal>
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        #region Fields

        //множители для пределов.
        public decimal BaseMultipliers;

        public decimal BaseTolCoeff;

        /// <summary>
        /// довесок к формуле погрешности- единица младшего разряда
        /// </summary>
        public int EdMlRaz;

        protected MeasPoint[] HerzPoint;

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

        public Oper7FreqMeasureBase(IUserItemOperation userItemOperation) : base(userItemOperation)
        {
            Name = "Определение погрешности измерения частоты переменного напряжения";
            OperMeasureMode = Mult107_109N.MeasureMode.Herz;
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
            foreach (var voltPoint in VoltPoint)
            foreach (var freqPoint in voltPoint.Herz)
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
                        flkCalib5522A.Out.Set.Voltage.Ac.SetValue(voltPoint.VariableBaseValueMeasPoint.NominalVal,
                                                                  freqPoint.NominalVal,
                                                                  voltPoint.VariableBaseValueMeasPoint.MultipliersUnit,
                                                                  freqPoint.MultipliersUnit);
                        flkCalib5522A.Out.SetOutput(CalibrMain.COut.State.On);
                        Thread.Sleep(2000);
                        //измеряем
                        var measurePoint = (decimal) appa107N.GetValue();

                        flkCalib5522A.Out.SetOutput(CalibrMain.COut.State.Off);

                        operation.Getting = measurePoint;
                        operation.Expected = freqPoint.NominalVal /
                                             (decimal) freqPoint
                                                      .MultipliersUnit.GetDoubleValue();
                        //расчет погрешности для конкретной точки предела измерения
                        operation.ErrorCalculation = (inA, inB) =>
                        {
                            var result = BaseTolCoeff * operation.Expected + EdMlRaz *
                                RangeResolution.VariableBaseValueMeasPoint.NominalVal *
                                (decimal) (RangeResolution
                                          .VariableBaseValueMeasPoint.MultipliersUnit.GetDoubleValue() /
                                           freqPoint.MultipliersUnit
                                                    .GetDoubleValue()
                                );
                            var mantisa =
                                MathStatistics.GetMantissa((decimal) (RangeResolution
                                                                     .VariableBaseValueMeasPoint.MultipliersUnit
                                                                     .GetDoubleValue() /
                                                                      freqPoint.MultipliersUnit
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

    public class Oper71Freq20HzMeasureBase : Oper7FreqMeasureBase
    {
        public Oper71Freq20HzMeasureBase(Mult107_109N.RangeNominal inRangeNominal,
            IUserItemOperation userItemOperation) :
            base(userItemOperation)
        {
            OperationRangeCode = Mult107_109N.RangeCode.Range1Manual;
            
            OperMeasureMode = Mult107_109N.MeasureMode.Herz;
            OpMultipliers = Multipliers.None;
            OperationRangeNominal = inRangeNominal;
            Name = OperationRangeNominal.GetStringValue();
            Sheme = ShemeTemplateDefault.TemplateSheme;

            BaseTolCoeff = (decimal)0.0001;
            EdMlRaz = 50;
            RangeResolution = new AcVariablePoint(1, MeasureUnits.Herz, Multipliers.Mili);

            HerzPoint = new MeasPoint[1];
            HerzPoint[0] = new MeasPoint(MeasureUnits.Herz, OpMultipliers, 10);

            VoltPoint = new AcVariablePoint[1];
            VoltPoint[0] = new AcVariablePoint((decimal) 0.5, MeasureUnits.V, Multipliers.None, HerzPoint);
        }
    }

    public class Oper71Freq200HzMeasureBase : Oper7FreqMeasureBase
    {
        public Oper71Freq200HzMeasureBase(Mult107_109N.RangeNominal inRangeNominal,
            IUserItemOperation userItemOperation) :
            base(userItemOperation)
        {
            OperationRangeCode = Mult107_109N.RangeCode.Range2Manual;
            OperMeasureMode = Mult107_109N.MeasureMode.Herz;
            OpMultipliers = Multipliers.None;
            OperationRangeNominal = inRangeNominal;
            Name = OperationRangeNominal.GetStringValue();
            Sheme = ShemeTemplateDefault.TemplateSheme;

            BaseTolCoeff = (decimal)0.0001;
            EdMlRaz = 10;
            RangeResolution = new AcVariablePoint(10, MeasureUnits.Herz, Multipliers.Mili);

            HerzPoint = new MeasPoint[1];
            HerzPoint[0] = new MeasPoint(MeasureUnits.Herz, OpMultipliers, 100);

            VoltPoint = new AcVariablePoint[1];
            VoltPoint[0] = new AcVariablePoint((decimal) 0.5, MeasureUnits.V, Multipliers.None, HerzPoint);
        }
    }

    public class Oper71Freq2kHzMeasureBase : Oper7FreqMeasureBase
    {
        public Oper71Freq2kHzMeasureBase(Mult107_109N.RangeNominal inRangeNominal,
            IUserItemOperation userItemOperation) :
            base(userItemOperation)
        {
            OperationRangeCode = Mult107_109N.RangeCode.Range3Manual;
            OperMeasureMode = Mult107_109N.MeasureMode.Herz;
            OpMultipliers = Multipliers.Kilo;
            OperationRangeNominal = inRangeNominal;
            Name = OperationRangeNominal.GetStringValue();
            Sheme = ShemeTemplateDefault.TemplateSheme;

            BaseTolCoeff = (decimal)0.0001;
            EdMlRaz = 10;
            RangeResolution = new AcVariablePoint(100, MeasureUnits.Herz, Multipliers.Mili);

            HerzPoint = new MeasPoint[1];
            HerzPoint[0] = new MeasPoint(MeasureUnits.Herz, OpMultipliers, 1);

            VoltPoint = new AcVariablePoint[1];
            VoltPoint[0] = new AcVariablePoint((decimal) 0.5, MeasureUnits.V, Multipliers.None, HerzPoint);
        }
    }

    public class Oper71Freq20kHzMeasureBase : Oper7FreqMeasureBase
    {
        public Oper71Freq20kHzMeasureBase(Mult107_109N.RangeNominal inRangeNominal,
            IUserItemOperation userItemOperation) :
            base(userItemOperation)
        {
            OperationRangeCode = Mult107_109N.RangeCode.Range4Manual;
            OperMeasureMode = Mult107_109N.MeasureMode.Herz;
            OpMultipliers = Multipliers.Kilo;
            OperationRangeNominal = inRangeNominal;
            Name = OperationRangeNominal.GetStringValue();
            Sheme = ShemeTemplateDefault.TemplateSheme;

            BaseTolCoeff = (decimal)0.0001;
            EdMlRaz = 10;
            RangeResolution = new AcVariablePoint(1, MeasureUnits.Herz, Multipliers.None);

            HerzPoint = new MeasPoint[1];
            HerzPoint[0] = new MeasPoint(MeasureUnits.Herz, OpMultipliers, 10);

            VoltPoint = new AcVariablePoint[1];
            VoltPoint[0] = new AcVariablePoint((decimal) 0.5, MeasureUnits.V, Multipliers.None, HerzPoint);
        }
    }

    public class Oper71Freq200kHzMeasureBase : Oper7FreqMeasureBase
    {
        public Oper71Freq200kHzMeasureBase(Mult107_109N.RangeNominal inRangeNominal,
            IUserItemOperation userItemOperation) :
            base(userItemOperation)
        {
            OperationRangeCode = Mult107_109N.RangeCode.Range5Manual;
            OperMeasureMode = Mult107_109N.MeasureMode.Herz;
            OpMultipliers = Multipliers.Kilo;
            OperationRangeNominal = inRangeNominal;
            Name = OperationRangeNominal.GetStringValue();
            Sheme = ShemeTemplateDefault.TemplateSheme;

            BaseTolCoeff = (decimal)0.0001;
            EdMlRaz = 10;
            RangeResolution = new AcVariablePoint(10, MeasureUnits.Herz, Multipliers.None);

            HerzPoint = new MeasPoint[1];
            HerzPoint[0] = new MeasPoint(MeasureUnits.Herz, OpMultipliers, 100);

            VoltPoint = new AcVariablePoint[1];
            VoltPoint[0] = new AcVariablePoint((decimal) 0.5, MeasureUnits.V, Multipliers.None, HerzPoint);
        }
    }

    public class Oper71Freq1MHzMeasureBase : Oper7FreqMeasureBase
    {
        public Oper71Freq1MHzMeasureBase(Mult107_109N.RangeNominal inRangeNominal,
            IUserItemOperation userItemOperation) :
            base(userItemOperation)
        {
            OperationRangeCode = Mult107_109N.RangeCode.Range6Manual;
            OperMeasureMode = Mult107_109N.MeasureMode.Herz;
            OpMultipliers = Multipliers.Mega;
            OperationRangeNominal = inRangeNominal;
            Name = OperationRangeNominal.GetStringValue();
            Sheme = ShemeTemplateDefault.TemplateSheme;

            BaseTolCoeff = (decimal)0.0001;
            EdMlRaz = 10;
            RangeResolution = new AcVariablePoint(100, MeasureUnits.Herz, Multipliers.None);

            HerzPoint = new MeasPoint[1];
            HerzPoint[0] = new MeasPoint(MeasureUnits.Herz, OpMultipliers, 1);

            VoltPoint = new AcVariablePoint[1];
            VoltPoint[0] = new AcVariablePoint((decimal) 0.5, MeasureUnits.V, Multipliers.None, HerzPoint);
        }
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
            
            Name = OperationOhmRangeNominal.GetStringValue();
            OpMultipliers = Multipliers.None;

            BaseTolCoeff = (decimal)0.003;
            EdMlRaz = 30;
            RangeResolution = new AcVariablePoint(10, MeasureUnits.Ohm, Multipliers.Mili);

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
            Name = OperationOhmRangeNominal.GetStringValue();
            OpMultipliers = Multipliers.Kilo;

            BaseTolCoeff = (decimal)0.003;
            EdMlRaz = 30;
            RangeResolution = new AcVariablePoint(100, MeasureUnits.Ohm, Multipliers.Mili);

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
            
            Name = OperationOhmRangeNominal.GetStringValue();
            OpMultipliers = Multipliers.Kilo;

            BaseTolCoeff = (decimal)0.003;
            EdMlRaz = 30;
            RangeResolution = new AcVariablePoint(1, MeasureUnits.Ohm, Multipliers.None);

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
            Name = OperationOhmRangeNominal.GetStringValue();
            OpMultipliers = Multipliers.Kilo;

            BaseTolCoeff = (decimal)0.003;
            EdMlRaz = 30;
            RangeResolution = new AcVariablePoint(10, MeasureUnits.Ohm, Multipliers.None);

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
            Name = OperationOhmRangeNominal.GetStringValue();
            OpMultipliers = Multipliers.Mega;

            BaseTolCoeff = (decimal)0.003;
            EdMlRaz = 50;
            RangeResolution = new AcVariablePoint(10, MeasureUnits.Ohm, Multipliers.None);

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
            Name = OperationOhmRangeNominal.GetStringValue();
            OpMultipliers = Multipliers.Mega;

            BaseTolCoeff = (decimal)0.05;
            EdMlRaz = 50;
            RangeResolution = new AcVariablePoint(10, MeasureUnits.Ohm, Multipliers.Mili);

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
            Name = OperationOhmRangeNominal.GetStringValue();
            OpMultipliers = Multipliers.Mega;

            BaseTolCoeff = (decimal)0.05;
            EdMlRaz = 20;
            RangeResolution = new AcVariablePoint(10, MeasureUnits.Ohm, Multipliers.Mili);

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
            Name = OperationOhmRangeNominal.GetStringValue();
            OpMultipliers = Multipliers.Giga;

            BaseTolCoeff = (decimal)0.05;
            EdMlRaz = 8;
            RangeResolution = new AcVariablePoint(10, MeasureUnits.Ohm, Multipliers.Mili);

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
                                                       "Указание оператору", MessageButton.OK,
                                                       MessageIcon.Information,
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
                                                       "Указание оператору", MessageButton.OK,
                                                       MessageIcon.Information,
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
                                                       "Указание оператору", MessageButton.OK,
                                                       MessageIcon.Information,
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
                                                       "Указание оператору", MessageButton.OK,
                                                       MessageIcon.Information,
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
        public Oper9_1Far_4nF_Measure(Mult107_109N.RangeNominal inRangeNominal,
            IUserItemOperation userItemOperation) :
            base(userItemOperation)
        {
            OperationRangeCode = Mult107_109N.RangeCode.Range1Manual;
            OperationRangeNominal = inRangeNominal;
            RangeResolution = new AcVariablePoint(1, MeasureUnits.Far, Multipliers.Pico);
            Name = OperationRangeNominal.GetStringValue();
            OpMultipliers = Multipliers.Nano;

            FarMeasPoints = new MeasPoint[1];
            FarMeasPoints[0] = new MeasPoint(MeasureUnits.Far, OpMultipliers, 3);
        }
    }

    public class Oper9_1Far_40nF_Measure : Oper9FarMeasureBase
    {
        public Oper9_1Far_40nF_Measure(Mult107_109N.RangeNominal inRangeNominal,
            IUserItemOperation userItemOperation) :
            base(userItemOperation)
        {
            OperationRangeCode = Mult107_109N.RangeCode.Range2Manual;
            OperationRangeNominal = inRangeNominal;
            RangeResolution = new AcVariablePoint(10, MeasureUnits.Far, Multipliers.Pico);
            Name = OperationRangeNominal.GetStringValue();
            OpMultipliers = Multipliers.Nano;

            FarMeasPoints = new MeasPoint[1];
            FarMeasPoints[0] = new MeasPoint(MeasureUnits.Far, OpMultipliers, 30);
        }
    }

    public class Oper9_1Far_400nF_Measure : Oper9FarMeasureBase
    {
        public Oper9_1Far_400nF_Measure(Mult107_109N.RangeNominal inRangeNominal,
            IUserItemOperation userItemOperation)
            : base(userItemOperation)
        {
            OperationRangeCode = Mult107_109N.RangeCode.Range3Manual;
            OperationRangeNominal = inRangeNominal;
            RangeResolution = new AcVariablePoint(100, MeasureUnits.Far, Multipliers.Pico);
            Name = OperationRangeNominal.GetStringValue();
            OpMultipliers = Multipliers.Nano;

            BaseTolCoeff = (decimal)0.009;
            EdMlRaz = 5;

            FarMeasPoints = new MeasPoint[1];
            FarMeasPoints[0] = new MeasPoint(MeasureUnits.Far, OpMultipliers, 300);
        }
    }

    public class Oper9_1Far_4uF_Measure : Oper9FarMeasureBase
    {
        public Oper9_1Far_4uF_Measure(Mult107_109N.RangeNominal inRangeNominal,
            IUserItemOperation userItemOperation) :
            base(userItemOperation)
        {
            OperationRangeCode = Mult107_109N.RangeCode.Range4Manual;
            OperationRangeNominal = inRangeNominal;
            Name = OperationRangeNominal.GetStringValue();
            OpMultipliers = Multipliers.Micro;

            BaseTolCoeff = (decimal)0.009;
            EdMlRaz = 5;
            RangeResolution = new AcVariablePoint(1, MeasureUnits.Far, Multipliers.Nano);

            FarMeasPoints = new MeasPoint[1];
            FarMeasPoints[0] = new MeasPoint(MeasureUnits.Far, OpMultipliers, 3);
        }
    }

    public class Oper9_1Far_40uF_Measure : Oper9FarMeasureBase
    {
        public Oper9_1Far_40uF_Measure(Mult107_109N.RangeNominal inRangeNominal,
            IUserItemOperation userItemOperation) :
            base(userItemOperation)
        {
            OperationRangeCode = Mult107_109N.RangeCode.Range5Manual;
            OperationRangeNominal = inRangeNominal;
            Name = OperationRangeNominal.GetStringValue();
            OpMultipliers = Multipliers.Micro;

            BaseTolCoeff = (decimal)0.012;
            EdMlRaz = 5;
            RangeResolution = new AcVariablePoint(10, MeasureUnits.Far, Multipliers.Nano);

            FarMeasPoints = new MeasPoint[1];
            FarMeasPoints[0] = new MeasPoint(MeasureUnits.Far, OpMultipliers, 30);
        }
    }

    public class Oper9_1Far_400uF_Measure : Oper9FarMeasureBase
    {
        public Oper9_1Far_400uF_Measure(Mult107_109N.RangeNominal inRangeNominal,
            IUserItemOperation userItemOperation)
            : base(userItemOperation)
        {
            OperationRangeCode = Mult107_109N.RangeCode.Range6Manual;
            OperationRangeNominal = inRangeNominal;
            Name = OperationRangeNominal.GetStringValue();
            OpMultipliers = Multipliers.Micro;

            BaseTolCoeff = (decimal)0.012;
            EdMlRaz = 5;
            RangeResolution = new AcVariablePoint(100, MeasureUnits.Far, Multipliers.Nano);

            FarMeasPoints = new MeasPoint[1];
            FarMeasPoints[0] = new MeasPoint(MeasureUnits.Far, OpMultipliers, 300);
        }
    }

    public class Oper9_1Far_4mF_Measure : Oper9FarMeasureBase
    {
        public Oper9_1Far_4mF_Measure(Mult107_109N.RangeNominal inRangeNominal,
            IUserItemOperation userItemOperation) :
            base(userItemOperation)
        {
            OperationRangeCode = Mult107_109N.RangeCode.Range7Manual;
            OperationRangeNominal = inRangeNominal;
            Name = OperationRangeNominal.GetStringValue();
            OpMultipliers = Multipliers.Mili;

            BaseTolCoeff = (decimal)0.015;
            EdMlRaz = 5;
            RangeResolution = new AcVariablePoint(1, MeasureUnits.Far, Multipliers.Micro);

            FarMeasPoints = new MeasPoint[1];
            FarMeasPoints[0] = new MeasPoint(MeasureUnits.Far, OpMultipliers, 3);
        }
    }

    public class Oper9_1Far_40mF_Measure : Oper9FarMeasureBase
    {
        public Oper9_1Far_40mF_Measure(Mult107_109N.RangeNominal inRangeNominal,
            IUserItemOperation userItemOperation) :
            base(userItemOperation)
        {
            OperationRangeCode = Mult107_109N.RangeCode.Range8Manual;
            OperationRangeNominal = inRangeNominal;
            Name = OperationRangeNominal.GetStringValue();
            OpMultipliers = Multipliers.Mili;

            BaseTolCoeff = (decimal)0.015;
            EdMlRaz = 5;
            RangeResolution = new AcVariablePoint(10, MeasureUnits.Far, Multipliers.Micro);

            FarMeasPoints = new MeasPoint[1];
            FarMeasPoints[0] = new MeasPoint(MeasureUnits.Far, OpMultipliers, 30);
        }
    }

    #endregion FAR

    //////////////////////////////******TEMP*******///////////////////////////////

    #region TEMP

    public  class Oper10TemperatureMeasureBase : ParagraphBase, IUserItemOperation<decimal>
    {
        public List<IBasicOperation<decimal>> DataRow { get; set; }
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        #region Fields

        //множители для пределов.
        public decimal BaseMultipliers;

        /// <summary>
        /// Процент от измеряемой точки для расчета погрешности (уже переведе в абсолютные единицы).
        /// </summary>
        public decimal BaseTolCoeff = (decimal)0.0006;

        /// <summary>
        /// довесок к формуле погрешности- число единиц младшего разряда
        /// </summary>
        public int EdMlRaz; //значение для пределов свыше 200 мВ

        /// <summary>
        /// Разарешение пределеа измерения (последний значащий разряд)
        /// </summary>
        protected AcVariablePoint RangeResolution;

        /// <summary>
        /// Массив поверяемых точек напряжения.
        /// </summary>
        protected AcVariablePoint[] DegC_Point;

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


        public Oper10TemperatureMeasureBase(IUserItemOperation userItemOperation) : base(userItemOperation)
        {
            Name = "Определение погрешности измерения температуры, градусы Цельсия";
            OperMeasureMode = Mult107_109N.MeasureMode.degC;
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

            foreach (var currPoint in DegC_Point)
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
                                var curRange = (int)appa107N.GetRangeCode & 128;
                                var targetRange = (int)OperationRangeCode & 128;
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
                        flkCalib5522A.Out.Set.Temperature.SetTermocouple(CalibrMain.COut.CSet.СTemperature
                                                                                   .TypeTermocouple.K);
                        flkCalib5522A.Out.Set.Temperature.SetValue((double)currPoint.VariableBaseValueMeasPoint.NominalVal);
                        //flkCalib5522A.WriteLine(Calib5522A.COut.CSet.Temperature.SetValue());
                                      //Voltage.Dc.SetValue(currPoint.VariableBaseValueMeasPoint.NominalVal);
                        flkCalib5522A.Out.SetOutput(CalibrMain.COut.State.On);
                        Thread.Sleep(2000);
                        //измеряем
                        var measurePoint = (decimal)appa107N.GetValue();

                        flkCalib5522A.Out.SetOutput(CalibrMain.COut.State.Off);

                        operation.Getting = measurePoint;
                        operation.Expected = currPoint.VariableBaseValueMeasPoint.NominalVal /
                                             (decimal)currPoint
                                                      .VariableBaseValueMeasPoint.MultipliersUnit.GetDoubleValue();
                        //расчет погрешности для конкретной точки предела измерения
                        operation.ErrorCalculation = (inA, inB) =>
                        {
                            var result = BaseTolCoeff * operation.Expected + EdMlRaz *
                                RangeResolution.VariableBaseValueMeasPoint.NominalVal *
                                (decimal)(RangeResolution
                                          .VariableBaseValueMeasPoint.MultipliersUnit.GetDoubleValue() /
                                           currPoint.VariableBaseValueMeasPoint.MultipliersUnit
                                                    .GetDoubleValue()
                                );
                            var mantisa =
                                MathStatistics.GetMantissa((decimal)(RangeResolution
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
                                : (BasicOperationVerefication<decimal>)operation.Clone());
            }
        }

        #endregion

        
    }

    public class Oper10_1Temperature_Minus200_Minus100_Measure : Oper10TemperatureMeasureBase
    {

        public Oper10_1Temperature_Minus200_Minus100_Measure(Mult107_109N.RangeNominal inRangeNominal, IUserItemOperation userItemOperation) :
            base(userItemOperation)
        {
            OperationRangeCode = Mult107_109N.RangeCode.Range1Manual;
            OperationRangeNominal = inRangeNominal;
            RangeResolution = new AcVariablePoint(100, MeasureUnits.degC , Multipliers.Mili);
            Name = OperationRangeNominal.GetStringValue();
            OpMultipliers = Multipliers.None;

            BaseTolCoeff = (decimal)0.001;
            EdMlRaz = 60;

            DegC_Point = new AcVariablePoint[]{ new AcVariablePoint((decimal)-200 , MeasureUnits.degC ,OpMultipliers) };
           
        }
    }

    public class Oper10_1Temperature_Minus100_400_Measure : Oper10TemperatureMeasureBase
    {

        public Oper10_1Temperature_Minus100_400_Measure(Mult107_109N.RangeNominal inRangeNominal, IUserItemOperation userItemOperation) :
            base(userItemOperation)
        {
            OperationRangeCode = Mult107_109N.RangeCode.Range1Manual;
            OperationRangeNominal = inRangeNominal;
            RangeResolution = new AcVariablePoint(100, MeasureUnits.degC, Multipliers.Mili);
            Name = OperationRangeNominal.GetStringValue();
            OpMultipliers = Multipliers.None;

            BaseTolCoeff = (decimal)0.001;
            EdMlRaz = 30;

            DegC_Point = new AcVariablePoint[]
            {
                new AcVariablePoint((decimal)-100, MeasureUnits.degC, OpMultipliers),
                new AcVariablePoint((decimal)0, MeasureUnits.degC, OpMultipliers),
                new AcVariablePoint((decimal)100, MeasureUnits.degC, OpMultipliers),
            };

        }
    }

    public class Oper10_1Temperature_400_1200_Measure : Oper10TemperatureMeasureBase
    {

        public Oper10_1Temperature_400_1200_Measure(Mult107_109N.RangeNominal inRangeNominal, IUserItemOperation userItemOperation) :
            base(userItemOperation)
        {
            OperationRangeCode = Mult107_109N.RangeCode.Range2Manual;
            OperationRangeNominal = inRangeNominal;
            RangeResolution = new AcVariablePoint(1, MeasureUnits.degC, Multipliers.None);
            Name = OperationRangeNominal.GetStringValue();
            OpMultipliers = Multipliers.None;

            BaseTolCoeff = (decimal)0.001;
            EdMlRaz = 3;

            DegC_Point = new AcVariablePoint[]
            {
                new AcVariablePoint((decimal)500, MeasureUnits.degC, OpMultipliers),
                new AcVariablePoint((decimal)800, MeasureUnits.degC, OpMultipliers),
                new AcVariablePoint((decimal)1200, MeasureUnits.degC, OpMultipliers),
            };

        }
    }

    #endregion TEMP
}