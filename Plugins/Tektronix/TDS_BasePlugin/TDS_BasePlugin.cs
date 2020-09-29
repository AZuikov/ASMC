using AP.Math;
using AP.Utils.Data;
using AP.Utils.Helps;
using ASMC.Core.Model;
using ASMC.Data.Model;
using ASMC.Data.Model.Interface;
using ASMC.Devices.IEEE;
using ASMC.Devices.IEEE.Fluke.CalibtatorOscilloscope;
using ASMC.Devices.IEEE.Tektronix.Oscilloscope;
using DevExpress.Mvvm;
using NLog;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace TDS_BasePlugin
{
    public static class Hepls
    {
        #region Methods

        public static Task<bool> HelpsCompliteWork(BasicOperationVerefication<MeasPoint> operation,
            IUserItemOperation UserItemOperation)
        {
            if (!operation.IsGood())
            {
                var answer =
                    UserItemOperation.ServicePack.MessageBox
                                     .Show($"Текущая точка {operation.Expected.Description} не проходит по допуску:\n" +
                                           $"Минимально допустимое значение {operation.LowerTolerance?.Description}\n" +
                                           $"Максимально допустимое значение {operation.UpperTolerance.Description}\n" +
                                           $"Допустимое значение погрешности {operation.Error.Description}\n" +
                                           $"ИЗМЕРЕННОЕ значение {operation.Getting.Description}\n\n" +
                                           $"\nФАКТИЧЕСКАЯ погрешность {operation.Expected.Value - operation.Getting.Value}\n\n" +
                                           "Повторить измерение этой точки?",
                                           "Информация по текущему измерению",
                                           MessageButton.YesNo, MessageIcon.Question,
                                           MessageResult.Yes);

                if (answer == MessageResult.No) return Task.FromResult(true);
            }

            return Task.FromResult(operation.IsGood());
        }

        #endregion Methods
    }

    public class TDS_BasePlugin<T> : Program<T> where T : OperationMetrControlBase

    {
        public TDS_BasePlugin(ServicePack service) : base(service)
        {
            Grsi = "32618-06";
        }
    }

    public class Operation : OperationMetrControlBase
    {
        public Operation()
        {
            UserItemOperationPeriodicVerf = UserItemOperationPrimaryVerf;
        }
    }

    public abstract class OpertionFirsVerf : ASMC.Core.Model.Operation
    {
        public OpertionFirsVerf(ServicePack servicePack) : base(servicePack)
        {
            ControlDevices = new IDeviceUi[]
            {
                new Device {Devices = new IDeviceBase[] {new Calibr9500B()}, Description = "Калибратор осциллографов"}
            };
            Accessories = new[]
            {
                "Интерфейсный кабель для клибратора (GPIB)"
            };

            DocumentName = "32618-06 TDS";
        }

        #region Methods

        public override void RefreshDevice()
        {
            AddresDevice = IeeeBase.AllStringConnect;
        }

        #endregion Methods
    }

    public class Oper1VisualTest : ParagraphBase, IUserItemOperation<bool>
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
            var data = new DataTable { TableName = "ITBmVisualTest" };

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
                service.Entity = new Tuple<string, Assembly>("TDS_VisualTest", null);
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

        #endregion Methods

        public List<IBasicOperation<bool>> DataRow { get; set; }
    }

    public class Oper2Oprobovanie : ParagraphBase, IUserItemOperation<bool>
    {
        public Oper2Oprobovanie(IUserItemOperation userItemOperation) : base(userItemOperation)
        {
            Name = "Опробование";
            DataRow = new List<IBasicOperation<bool>>();
        }

        #region Methods

        protected override DataTable FillData()
        {
            var data = new DataTable { TableName = "ITBmOprobovanie" };

            data.Columns.Add("Результат опробования");
            var dataRow = data.NewRow();
            if (DataRow.Count == 1)
            {
                var dds = DataRow[0] as BasicOperation<bool>;
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
                service.Title = "Опробование";
                service.Entity = new Tuple<string, Assembly>("TDS_Oprobovanie", null);
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

        #endregion Methods

        public List<IBasicOperation<bool>> DataRow { get; set; }
    }

    /*Определение погрешности коэффициентов отклонения.*/

    public abstract class Oper3KoefOtkl : ParagraphBase, IUserItemOperation<MeasPoint>
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        #region Fields

        /// <summary>
        /// тестируемый канал.
        /// </summary>
        private readonly TDS_Oscilloscope.ChanelSet TestingChanel;

        protected Calibr9500B calibr9500B;

        /// <summary>
        /// Веритикальная развертка на которой производится поверка.
        /// </summary>
        protected MeasPoint ChanelVerticalRange;

        protected List<TDS_Oscilloscope.VerticalScale> verticalScalesList = new List<TDS_Oscilloscope.VerticalScale>();

        protected TDS_Oscilloscope someTdsOscilloscope;

        #endregion Fields

        protected Oper3KoefOtkl(IUserItemOperation userItemOperation, TDS_Oscilloscope.ChanelSet inTestingChanel) :
            base(userItemOperation)
        {
            Name = "Определение погрешности коэффициентов отклонения";
            DataRow = new List<IBasicOperation<MeasPoint>>();
            TestingChanel = inTestingChanel;
            //подключение к каналу это наверное можно сделать через сообщение
            // ли для каждого канала картинку нарисовать TemplateSheme
        }

        #region Methods

        protected override DataTable FillData()
        {
            var dataTable = new DataTable { TableName = $"FillTabBmOper3KoefOtkl{TestingChanel}" };
            dataTable.Columns.Add("Коэффициент развёртки");
            dataTable.Columns.Add("Амплитуда подаваемого сигнала");
            dataTable.Columns.Add("Измеренное значение амплитуды");
            dataTable.Columns.Add("Минимальное допустимое значение");
            dataTable.Columns.Add("Максимальное допустимое значение");
            dataTable.Columns.Add("Результат");

            foreach (var row in DataRow)
            {
                var dataRow = dataTable.NewRow();
                var dds = row as BasicOperationVerefication<MeasPoint>;
                if (dds == null) continue;
                dataRow[0] = new MeasPoint(dds.Expected.Units, dds.Expected.UnitMultipliersUnit, dds.Expected.Value / 3)
                   .Description + "/Дел";
                dataRow[1] = dds.Expected?.Description;
                dataRow[2] = dds.Getting?.Description;
                dataRow[3] = dds.LowerTolerance?.Description;
                dataRow[4] = dds.UpperTolerance?.Description;

                if (dds.IsGood == null)
                    dataRow[5] = "не выполнено";
                else
                    dataRow[5] = dds.IsGood() ? "Годен" : "Брак";
                dataTable.Rows.Add(dataRow);
            }

            return dataTable;
        }

        protected override void InitWork()
        {
            if (calibr9500B == null || someTdsOscilloscope == null) return;

            DataRow.Clear();
            foreach (TDS_Oscilloscope.VerticalScale currScale in verticalScalesList)
            {
                var operation = new BasicOperationVerefication<MeasPoint>();
                operation.InitWork = async () =>
                {
                    try
                    {
                        await Task.Run(() =>
                        {
                            calibr9500B.StringConnection = GetStringConnect(calibr9500B);
                            someTdsOscilloscope.StringConnection = GetStringConnect(someTdsOscilloscope);
                        });
                    }
                    catch (Exception e)
                    {
                        Logger.Error(e);
                        throw;
                    }
                };

                operation.BodyWork = () =>
                {
                    try
                    {
                        //1.нужно знать канал
                        someTdsOscilloscope.Chanel.SetChanelState(TestingChanel, TDS_Oscilloscope.State.ON);
                        //теперь нужно понять с каким каналом мы будем работать на калибраторе
                        var chnael = calibr9500B.FindActiveHeadOnChanel(new ActiveHead9510()).FirstOrDefault();
                        calibr9500B.Route.Chanel.SetChanel(chnael);
                        calibr9500B.Route.Chanel.SetImpedans(Calibr9500B.Impedans.Res_1M);
                        //2.установить развертку по вертикали
                        someTdsOscilloscope.Chanel.SetProbe(TestingChanel, TDS_Oscilloscope.Probe.Att_1);
                        someTdsOscilloscope.Chanel.Vertical.SetSCAle(TestingChanel, currScale);
                        //смещение для номального отображения
                        someTdsOscilloscope.Chanel.Vertical.SetPosition(TestingChanel, -2);
                        //триггер
                        someTdsOscilloscope.Trigger.SetTriggerMode(TDS_Oscilloscope.CTrigger.Mode.AUTO);
                        someTdsOscilloscope.Trigger.SetTriggerType(TDS_Oscilloscope.CTrigger.Type.EDGE);
                        someTdsOscilloscope.Trigger.SetTriggerEdgeSource(TestingChanel);
                        someTdsOscilloscope.Trigger.SetTriggerEdgeSlope(TDS_Oscilloscope.CTrigger.Slope.RIS);

                        //3.установить развертку по времени
                        someTdsOscilloscope.Horizontal.SetHorizontalScale(TDS_Oscilloscope
                                                                         .HorizontalSCAle.Scal_500mkSec);

                        //4.установить усреднение
                        someTdsOscilloscope.Acquire.SetDataCollection(TDS_Oscilloscope
                                                                     .MiscellaneousMode.SAMple); //так быстрее будет
                        //5.подать сигнал: меандр 1 кГц
                        //это разверктка
                        ChanelVerticalRange = new MeasPoint(MeasureUnits.V, currScale.GetUnitMultipliersValue(),
                                                            (decimal)currScale.GetDoubleValue());
                        // это подаваемая амплитуда
                        operation.Expected = new MeasPoint(MeasureUnits.V, currScale.GetUnitMultipliersValue(),
                                                           3 * (decimal)currScale.GetDoubleValue());
                        calibr9500B.Source.SetFunc(Calibr9500B.Shap.SQU).Source
                                   .SetVoltage((double)operation.Expected.Value,
                                               operation.Expected.UnitMultipliersUnit).Source
                                   .SetFreq(1, UnitMultipliers.Kilo);
                        calibr9500B.Source.Output(Calibr9500B.State.On);
                        //6.снять показания с осциллографа

                        someTdsOscilloscope.Measurement.SetMeas(TestingChanel, TDS_Oscilloscope.TypeMeas.PK2);
                        someTdsOscilloscope.Acquire.SetDataCollection(TDS_Oscilloscope.MiscellaneousMode.AVErage);
                        someTdsOscilloscope.Trigger.SetTriggerLevelOn50Percent();
                        Thread.Sleep(2500);
                        someTdsOscilloscope.Trigger.SetTriggerLevelOn50Percent();
                        var measResult = someTdsOscilloscope.Measurement.MeasureValue() /
                                         (decimal)currScale.GetUnitMultipliersValue().GetDoubleValue();
                        MathStatistics.Round(ref measResult, 2);

                        someTdsOscilloscope.Acquire.SetDataCollection(TDS_Oscilloscope.MiscellaneousMode.SAMple);

                        operation.Getting =
                            new MeasPoint(MeasureUnits.V, currScale.GetUnitMultipliersValue(), measResult);
                        operation.ErrorCalculation = (point, measPoint) =>
                        {
                            var nominalPoint = ChanelVerticalRange.Value *
                                               (decimal)ChanelVerticalRange.UnitMultipliersUnit.GetDoubleValue();

                            if (nominalPoint >= (decimal)0.01 && nominalPoint <= 5)
                                return new MeasPoint(MeasureUnits.V, currScale.GetUnitMultipliersValue(),
                                                     operation.Expected.Value * 3 / 100);

                            return new MeasPoint(MeasureUnits.V, currScale.GetUnitMultipliersValue(),
                                                 operation.Expected.Value * 4 / 100);
                        };
                        operation.UpperTolerance = new MeasPoint(MeasureUnits.V, currScale.GetUnitMultipliersValue(),
                                                                 operation.Expected.Value + operation.Error.Value);
                        operation.LowerTolerance = new MeasPoint(MeasureUnits.V, currScale.GetUnitMultipliersValue(),
                                                                 operation.Expected.Value - operation.Error.Value);

                        operation.IsGood = () =>
                        {
                            if (operation.Getting == null || operation.Expected == null ||
                                operation.UpperTolerance == null || operation.LowerTolerance == null) return false;
                            return (operation.Getting.Value < operation.UpperTolerance.Value) &
                                   (operation.Getting.Value > operation.LowerTolerance.Value);
                        };
                    }
                    catch (Exception e)
                    {
                        Logger.Error(e);
                        throw;
                    }
                    finally
                    {
                        calibr9500B.Source.Output(Calibr9500B.State.Off);
                        someTdsOscilloscope.Chanel.SetChanelState(TestingChanel, TDS_Oscilloscope.State.OFF);
                    }
                };
                operation.CompliteWork = () => Hepls.HelpsCompliteWork(operation, UserItemOperation);
                DataRow.Add(DataRow.IndexOf(operation) == -1
                                ? operation
                                : (BasicOperationVerefication<MeasPoint>)operation.Clone());
            }
        }

        #endregion Methods

        public List<IBasicOperation<MeasPoint>> DataRow { get; set; }
    }

    /*Определение погрешности измерения временных интервалов.*/

    public abstract class Oper4MeasureTimeIntervals : ParagraphBase, IUserItemOperation<MeasPoint>
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        #region Fields

        /// <summary>
        /// тестируемый канал.
        /// </summary>
        private readonly TDS_Oscilloscope.ChanelSet _testingChanel;

        protected Calibr9500B calibr9500B;

        /// <summary>
        /// Развертка по времени на которой производится поверка.
        /// </summary>
        protected MeasPoint ChanelHorizontalRange;

        /// <summary>
        /// Набор разверток, в зависимости от модели устройства. Как в методике поверки.
        /// </summary>
        protected Dictionary<TDS_Oscilloscope.HorizontalSCAle, MeasPoint> ScaleTolDict =
            new Dictionary<TDS_Oscilloscope.HorizontalSCAle, MeasPoint>();

        protected TDS_Oscilloscope someTdsOscilloscope;

        #endregion Fields

        public Oper4MeasureTimeIntervals(IUserItemOperation userItemOperation,
            TDS_Oscilloscope.ChanelSet chanel) : base(userItemOperation)
        {
            _testingChanel = chanel;
            Name = "Определение погрешности измерения временных интервалов";
            DataRow = new List<IBasicOperation<MeasPoint>>();
        }

        #region Methods

        protected override DataTable FillData()
        {
            var dataTable = new DataTable { TableName = $"FillTabBmOper4MeasureTimeIntervalsl{_testingChanel}" };
            dataTable.Columns.Add("Развертка по времени");
            dataTable.Columns.Add("Измеренное значение периода");
            dataTable.Columns.Add("Минимально допустимое значение");
            dataTable.Columns.Add("Максимально допустимое значение");
            dataTable.Columns.Add("Результат");

            foreach (var row in DataRow)
            {
                var dataRow = dataTable.NewRow();
                var dds = row as BasicOperationVerefication<MeasPoint>;
                if (dds == null) continue;

                dataRow[0] = dds.Expected?.Description;
                dataRow[1] = dds.Getting?.Description;
                dataRow[2] = dds.LowerTolerance?.Description;
                dataRow[3] = dds.UpperTolerance?.Description;
                

                if (dds.IsGood == null)
                    dataRow[4] = "не выполнено";
                else
                    dataRow[4] = dds.IsGood() ? "Годен" : "Брак";
                dataTable.Rows.Add(dataRow);
            }

            return dataTable;
        }

        protected override void InitWork()
        {
            if (calibr9500B == null || someTdsOscilloscope == null) return;

            DataRow.Clear();
            foreach (var currScale in ScaleTolDict.Keys)
            {
                var operation = new BasicOperationVerefication<MeasPoint>();
                operation.InitWork = async () =>
                {
                    try
                    {
                        await Task.Run(() =>
                        {
                            calibr9500B.StringConnection = GetStringConnect(calibr9500B);
                            someTdsOscilloscope.StringConnection = GetStringConnect(someTdsOscilloscope);
                        });
                    }
                    catch (Exception e)
                    {
                        Logger.Error(e);
                        throw;
                    }
                };

                operation.BodyWork = () =>
                {
                    try
                    {
                        //1.нужно знать канал
                        someTdsOscilloscope.Chanel.SetChanelState(_testingChanel, TDS_Oscilloscope.State.ON);
                        //теперь нужно понять с каким каналом мы будем работать на калибраторе
                        var chnael = calibr9500B.FindActiveHeadOnChanel(new ActiveHead9510()).FirstOrDefault();
                        calibr9500B.Route.Chanel.SetChanel(chnael);
                        calibr9500B.Route.Chanel.SetImpedans(Calibr9500B.Impedans.Res_1M);
                        //2.установить развертку по вертикали
                        someTdsOscilloscope.Chanel.SetProbe(_testingChanel, TDS_Oscilloscope.Probe.Att_1);
                        someTdsOscilloscope.Chanel.Vertical.SetSCAle(_testingChanel,
                                                                     TDS_Oscilloscope.VerticalScale.Scale_200mV);
                        someTdsOscilloscope.Chanel.Vertical.SetPosition(_testingChanel, 0);

                        someTdsOscilloscope.Horizontal.SetHorizontalScale(currScale);
                        calibr9500B.Source.SetFunc(Calibr9500B.Shap.MARK);
                        calibr9500B.Source.Parametr.MARKER.SetWaveForm(Calibr9500B.MarkerWaveForm.SQU);
                        calibr9500B.Source.Parametr.MARKER.SetAmplitude(Calibr9500B.MarkerAmplitude.ampl1V);
                        ChanelHorizontalRange = new MeasPoint(MeasureUnits.sec, currScale.GetUnitMultipliersValue(),
                                                              (decimal)currScale.GetDoubleValue());
                        var ExpectedPoin = new MeasPoint(MeasureUnits.sec, currScale.GetUnitMultipliersValue(),
                                                         (decimal)(currScale.GetDoubleValue() * 2));
                        calibr9500B.Source.Parametr.MARKER.SetPeriod(ExpectedPoin);
                        operation.Expected = ExpectedPoin;

                        calibr9500B.Source.Output(Calibr9500B.State.On);

                        //триггер
                        someTdsOscilloscope.Acquire.SetDataCollection(TDS_Oscilloscope.MiscellaneousMode.SAMple);
                        someTdsOscilloscope.Trigger.SetTriggerMode(TDS_Oscilloscope.CTrigger.Mode.AUTO);
                        someTdsOscilloscope.Trigger.SetTriggerType(TDS_Oscilloscope.CTrigger.Type.EDGE);
                        someTdsOscilloscope.Trigger.SetTriggerEdgeSource(_testingChanel);
                        someTdsOscilloscope.Trigger.SetTriggerEdgeSlope(TDS_Oscilloscope.CTrigger.Slope.RIS);
                        someTdsOscilloscope.Trigger.SetTriggerLevelOn50Percent();
                        someTdsOscilloscope.Measurement.SetMeas(_testingChanel, TDS_Oscilloscope.TypeMeas.PERI,
                                                                2);
                        Thread.Sleep(1000);
                        var measResult = someTdsOscilloscope.Measurement.MeasureValue(2) /
                                         (decimal)currScale.GetUnitMultipliersValue().GetDoubleValue();
                        MathStatistics.Round(ref measResult, 2);

                        operation.Getting =
                            new MeasPoint(MeasureUnits.sec, currScale.GetUnitMultipliersValue(), measResult);

                        operation.ErrorCalculation = (point, measPoint) => ScaleTolDict[currScale];

                        //!!!!!! нужно привести погрешность к размерности измеряемого значениея
                        operation.UpperTolerance = new MeasPoint(MeasureUnits.sec, currScale.GetUnitMultipliersValue(),Math.Round(operation.Expected.Value + operation.Error.Value *
                                                                                                                                  (decimal)(operation.Error.UnitMultipliersUnit
                                                                                                                                                     .GetDoubleValue() /
                                                                                                                                            operation.Expected.UnitMultipliersUnit
                                                                                                                                                     .GetDoubleValue()), 2));
                        operation.LowerTolerance = new MeasPoint(MeasureUnits.sec, currScale.GetUnitMultipliersValue(),Math.Round(operation.Expected.Value - operation.Error.Value *
                                                                                                                                  (decimal)(operation.Error.UnitMultipliersUnit
                                                                                                                                                     .GetDoubleValue() /
                                                                                                                                            operation.Expected.UnitMultipliersUnit
                                                                                                                                                     .GetDoubleValue()), 2)
                                                                 );
                        operation.Expected = new MeasPoint(MeasureUnits.sec, currScale.GetUnitMultipliersValue(), (decimal)currScale.GetDoubleValue());

                        operation.IsGood = () =>
                        {
                            if (operation.Getting == null || operation.Expected == null ||
                                operation.UpperTolerance == null || operation.LowerTolerance == null) return false;
                            return (operation.Getting.Value < operation.UpperTolerance.Value) &
                                   (operation.Getting.Value > operation.LowerTolerance.Value);
                        };
                    }
                    catch (Exception e)
                    {
                        Logger.Error(e);
                        throw;
                    }
                    finally
                    {
                        calibr9500B.Source.Output(Calibr9500B.State.Off);
                        someTdsOscilloscope.Chanel.SetChanelState(_testingChanel, TDS_Oscilloscope.State.OFF);
                    }
                };

                operation.CompliteWork = () => Hepls.HelpsCompliteWork(operation, UserItemOperation);
                DataRow.Add(DataRow.IndexOf(operation) == -1
                                ? operation
                                : (BasicOperationVerefication<MeasPoint>)operation.Clone());
            }
        }

        #endregion Methods

        public List<IBasicOperation<MeasPoint>> DataRow { get; set; }
    }

    /// <summary>
    /// Класс нужен для того, что бы выделить диапазоны верменных разверток в зависимости от модели, по МП.
    /// </summary>
    public class TDS20XXBOper4MeasureTimeIntervals : Oper4MeasureTimeIntervals
    {
        public TDS20XXBOper4MeasureTimeIntervals(IUserItemOperation userItemOperation,
            TDS_Oscilloscope.ChanelSet chanel) : base(userItemOperation, chanel)
        {
            ScaleTolDict.Add(TDS_Oscilloscope.HorizontalSCAle.Scal_2_5nSec,
                             new MeasPoint(MeasureUnits.sec, UnitMultipliers.Nano, (decimal)0.61));
            ScaleTolDict.Add(TDS_Oscilloscope.HorizontalSCAle.Scal_50nSec,
                             new MeasPoint(MeasureUnits.sec, UnitMultipliers.Nano, (decimal)0.81));
            ScaleTolDict.Add(TDS_Oscilloscope.HorizontalSCAle.Scal_250nSec,
                             new MeasPoint(MeasureUnits.sec, UnitMultipliers.Nano, (decimal)1.63));
            ScaleTolDict.Add(TDS_Oscilloscope.HorizontalSCAle.Scal_500mkSec,
                             new MeasPoint(MeasureUnits.sec, UnitMultipliers.Micro, (decimal)2.05));
            ScaleTolDict.Add(TDS_Oscilloscope.HorizontalSCAle.Scal_2_5mSec,
                             new MeasPoint(MeasureUnits.sec, UnitMultipliers.Micro, (decimal)10.25));
        }
    }

    /// <summary>
    /// Класс нужен для того, что бы выделить диапазоны верменных разверток в зависимости от модели, по МП.
    /// </summary>
    public class TDS10XXBOper4MeasureTimeIntervals : Oper4MeasureTimeIntervals
    {
        public TDS10XXBOper4MeasureTimeIntervals(IUserItemOperation userItemOperation,
            TDS_Oscilloscope.ChanelSet chanel) : base(userItemOperation, chanel)
        {
            ScaleTolDict.Add(TDS_Oscilloscope.HorizontalSCAle.Scal_5nSec,
                             new MeasPoint(MeasureUnits.sec, UnitMultipliers.Nano, (decimal)0.62));
            ScaleTolDict.Add(TDS_Oscilloscope.HorizontalSCAle.Scal_50nSec,
                             new MeasPoint(MeasureUnits.sec, UnitMultipliers.Nano, (decimal)0.81));
            ScaleTolDict.Add(TDS_Oscilloscope.HorizontalSCAle.Scal_250nSec,
                             new MeasPoint(MeasureUnits.sec, UnitMultipliers.Nano, (decimal)1.63));
            ScaleTolDict.Add(TDS_Oscilloscope.HorizontalSCAle.Scal_500mkSec,
                             new MeasPoint(MeasureUnits.sec, UnitMultipliers.Micro, (decimal)2.05));
            ScaleTolDict.Add(TDS_Oscilloscope.HorizontalSCAle.Scal_2_5mSec,
                             new MeasPoint(MeasureUnits.sec, UnitMultipliers.Micro, (decimal)10.25));
        }
    }

    /*Определение Времени нарастания переходной характеристики.*/

    public abstract class Oper5MeasureRiseTime : ParagraphBase, IUserItemOperation<MeasPoint>
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        #region Fields

        /// <summary>
        /// тестируемый канал.
        /// </summary>
        private readonly TDS_Oscilloscope.ChanelSet _testingChanel;

        protected Calibr9500B calibr9500B;

        /// <summary>
        /// Значение развертки по времени, для отображения фронта.
        /// </summary>
        protected TDS_Oscilloscope.HorizontalSCAle horizontalScAleForTest;

        protected MeasPoint RiseTimeTol;
        protected TDS_Oscilloscope someTdsOscilloscope;

        /// <summary>
        /// Набор разверток, в зависимости от модели устройства. Как в методике поверки.
        /// </summary>
        protected List<TDS_Oscilloscope.VerticalScale> verticalScalesList = new List<TDS_Oscilloscope.VerticalScale>();

        #endregion Fields

        protected Oper5MeasureRiseTime(IUserItemOperation userItemOperation, TDS_Oscilloscope.ChanelSet chanel) :
            base(userItemOperation)
        {
            _testingChanel = chanel;
            Name = "Определение Времени нарастания переходной характеристики";
            DataRow = new List<IBasicOperation<MeasPoint>>();

            verticalScalesList.Add(TDS_Oscilloscope.VerticalScale.Scale_5mV);
            verticalScalesList.Add(TDS_Oscilloscope.VerticalScale.Scale_10mV);
            verticalScalesList.Add(TDS_Oscilloscope.VerticalScale.Scale_20mV);
            verticalScalesList.Add(TDS_Oscilloscope.VerticalScale.Scale_50mV);
            verticalScalesList.Add(TDS_Oscilloscope.VerticalScale.Scale_100mV);
            verticalScalesList.Add(TDS_Oscilloscope.VerticalScale.Scale_200mV);
            verticalScalesList.Add(TDS_Oscilloscope.VerticalScale.Scale_500mV);
            verticalScalesList.Add(TDS_Oscilloscope.VerticalScale.Scale_1V);
            verticalScalesList.Add(TDS_Oscilloscope.VerticalScale.Scale_2V);
            verticalScalesList.Add(TDS_Oscilloscope.VerticalScale.Scale_5V);
        }

        #region Methods

        protected override DataTable FillData()
        {
            var dataTable = new DataTable { TableName = $"FillTabBmOper5MeasureRiseTime{_testingChanel}" };
            dataTable.Columns.Add("Горизонтальная развёртка");
            dataTable.Columns.Add("Коэффициент отклонения");
            dataTable.Columns.Add("Измеренное значение длительности фронта");
            dataTable.Columns.Add("Минимально допустимое значение длительности фронта");
            dataTable.Columns.Add("Результат");

            foreach (var row in DataRow)
            {
                var dataRow = dataTable.NewRow();
                var dds = row as BasicOperationVerefication<MeasPoint>;
                if (dds == null) continue;

                dataRow[0] = new MeasPoint(MeasureUnits.sec, UnitMultipliers.Nano, (decimal)2.5).Description + "/Дел";
                dataRow[1] = new MeasPoint(dds.Expected.Units, dds.Expected.UnitMultipliersUnit, dds.Expected.Value / 3).Description + "/Дел";
                dataRow[2] = dds?.Getting.Description;
                dataRow[3] = dds?.UpperTolerance.Description;

                if (dds.IsGood == null)
                    dataRow[4] = "не выполнено";
                else
                    dataRow[4] = dds.IsGood() ? "Годен" : "Брак";
                dataTable.Rows.Add(dataRow);
            }

            return dataTable;
        }

        protected override void InitWork()
        {
            if (calibr9500B == null || someTdsOscilloscope == null) return;

            DataRow.Clear();

            foreach (TDS_Oscilloscope.VerticalScale verticalScale in verticalScalesList)
            {
                //первый коэффициент отклонения пропускаем
                if (verticalScale == TDS_Oscilloscope.VerticalScale.Scale_2mV || verticalScale == TDS_Oscilloscope.VerticalScale.Scale_20mV) continue;

                var operation = new BasicOperationVerefication<MeasPoint>();
                operation.InitWork = async () =>
                {
                    try
                    {
                        await Task.Run(() =>
                        {
                            calibr9500B.StringConnection = GetStringConnect(calibr9500B);
                            someTdsOscilloscope.StringConnection = GetStringConnect(someTdsOscilloscope);
                            someTdsOscilloscope.ResetDevice();
                        });
                    }
                    catch (Exception e)
                    {
                        Logger.Error(e);
                        throw;
                    }
                };

                operation.BodyWork = () =>
                {
                    try
                    {
                        //1.нужно знать канал
                        someTdsOscilloscope.Chanel.SetChanelState(_testingChanel, TDS_Oscilloscope.State.ON);
                        //теперь нужно понять с каким каналом мы будем работать на калибраторе
                        var chnael = calibr9500B.FindActiveHeadOnChanel(new ActiveHead9510()).FirstOrDefault();
                        calibr9500B.Route.Chanel.SetChanel(chnael);
                        calibr9500B.Route.Chanel.SetImpedans(Calibr9500B.Impedans.Res_1M);
                        //2.установить развертку по вертикали
                        someTdsOscilloscope.Chanel.SetProbe(_testingChanel, TDS_Oscilloscope.Probe.Att_1);
                        someTdsOscilloscope.Chanel.Vertical.SetSCAle(_testingChanel, verticalScale);
                        someTdsOscilloscope.Chanel.Vertical.SetPosition(_testingChanel, 1);
                        someTdsOscilloscope.Horizontal.SetHorizontalScale(horizontalScAleForTest);

                        calibr9500B.Source.SetFunc(Calibr9500B.Shap.EDG);
                        calibr9500B.Source.Parametr.EDGE.SetEdgeDirection(Calibr9500B.Direction.RIS);
                        calibr9500B.Source.Parametr.EDGE.SetEdgeSpeed(Calibr9500B.SpeedEdge.Mid_500p);
                        operation.Expected = new MeasPoint(MeasureUnits.V, verticalScale.GetUnitMultipliersValue(),
                                                           (decimal)(3 * verticalScale.GetDoubleValue()));
                        calibr9500B.Source.SetVoltage((double)operation.Expected.Value,
                                                      operation.Expected.UnitMultipliersUnit);

                        calibr9500B.Source.Output(Calibr9500B.State.On);

                        //триггер
                        someTdsOscilloscope.Acquire.SetDataCollection(TDS_Oscilloscope.MiscellaneousMode.SAMple);
                        someTdsOscilloscope.Trigger.SetTriggerMode(TDS_Oscilloscope.CTrigger.Mode.AUTO);
                        someTdsOscilloscope.Trigger.SetTriggerType(TDS_Oscilloscope.CTrigger.Type.EDGE);
                        someTdsOscilloscope.Trigger.SetTriggerEdgeSource(_testingChanel);
                        someTdsOscilloscope.Trigger.SetTriggerEdgeSlope(TDS_Oscilloscope.CTrigger.Slope.RIS);
                        someTdsOscilloscope.Trigger.SetTriggerLevelOn50Percent();

                        if (verticalScale == TDS_Oscilloscope.VerticalScale.Scale_5V)
                        {
                            someTdsOscilloscope.Acquire.SetSingleOrRunStopMode(TDS_Oscilloscope.AcquireMode.RUNSTOP);
                            someTdsOscilloscope.Acquire.StartAcquire();
                            someTdsOscilloscope.WriteLine("acq:state off");
                        }

                        someTdsOscilloscope.Measurement.SetMeas(_testingChanel, TDS_Oscilloscope.TypeMeas.RIS, 3);
                        Thread.Sleep(1000);
                        var measResult = someTdsOscilloscope.Measurement.MeasureValue(3) /
                                         (decimal)horizontalScAleForTest.GetUnitMultipliersValue().GetDoubleValue();
                        MathStatistics.Round(ref measResult, 2);

                        operation.Getting =
                            new MeasPoint(MeasureUnits.sec, verticalScale.GetUnitMultipliersValue(), measResult);
                        operation.ErrorCalculation = (point, measPoint) => RiseTimeTol;
                        operation.UpperTolerance = RiseTimeTol;

                        operation.IsGood = () =>
                        {
                            if (operation.Getting == null || operation.Expected == null ||
                                operation.UpperTolerance == null) return false;
                            return operation.Getting.Value < operation.UpperTolerance.Value;
                        };
                    }
                    catch (Exception e)
                    {
                        Logger.Error(e);
                        throw;
                    }
                    finally
                    {
                        calibr9500B.Source.Output(Calibr9500B.State.Off);
                        someTdsOscilloscope.Chanel.SetChanelState(_testingChanel, TDS_Oscilloscope.State.OFF).ResetDevice();
                    }
                };

                operation.CompliteWork = () => Hepls.HelpsCompliteWork(operation, UserItemOperation);
                DataRow.Add(DataRow.IndexOf(operation) == -1
                                ? operation
                                : (BasicOperationVerefication<MeasPoint>)operation.Clone());
            }
        }

        #endregion Methods

        public List<IBasicOperation<MeasPoint>> DataRow { get; set; }
    }
}