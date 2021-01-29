using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using AP.Math;
using AP.Utils.Data;
using ASMC.Core.Model;
using ASMC.Data.Model;
using ASMC.Data.Model.Interface;
using ASMC.Data.Model.PhysicalQuantity;
using ASMC.Devices.IEEE;
using ASMC.Devices.IEEE.Fluke.CalibtatorOscilloscope;
using ASMC.Devices.IEEE.Tektronix.Oscilloscope;
using DevExpress.Mvvm;
using NLog;

namespace TDS_BasePlugin
{
    public static class HelpsTds
    {
        #region Methods

        public static Task<bool> HelpsCompliteWork<T>(BasicOperationVerefication<MeasPoint<T>> operation,
            IUserItemOperation UserItemOperation) where T : class, IPhysicalQuantity<T>, new()
        {
            if (operation.IsGood != null && !operation.IsGood())
            {
                var answer =
                    UserItemOperation.ServicePack.MessageBox()
                                     .Show($"Текущая точка {operation.Expected.Description} не проходит по допуску:\n" +
                                           $"Минимально допустимое значение {operation.LowerTolerance.Description}\n" +
                                           $"Максимально допустимое значение {operation.UpperTolerance.Description}\n" +
                                           $"Допустимое значение погрешности {operation.Error.Description}\n" +
                                           $"ИЗМЕРЕННОЕ значение {operation.Getting.Description}\n\n" +
                                           $"\nФАКТИЧЕСКАЯ погрешность {(operation.Expected - operation.Getting).Description}\n\n" +
                                           "Повторить измерение этой точки?",
                                           "Информация по текущему измерению",
                                           MessageButton.YesNo, MessageIcon.Question,
                                           MessageResult.Yes);

                if (answer == MessageResult.No) return Task.FromResult(true);
            }

            if (operation.IsGood == null)
                return Task.FromResult(true);
            return Task.FromResult(operation.IsGood());
        }

        #endregion
    }

    public abstract class TDS_BasePlugin<T> : Program<T> where T : OperationMetrControlBase

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
                new Device {Devices = new IDeviceRemote[] {new Calibr9500B()}, Description = "Калибратор осциллографов"}
            };
            TestDevices = new IDeviceUi[]
                {new Device {Devices = new IDeviceRemote[] {new TDS_Oscilloscope()}, Description = "Цифровой осциллограф."}};
            Accessories = new[]
            {
                "Интерфейсный кабель для калибратора (GPIB)"
            };

            DocumentName = "32618-06 TDS";
        }

        #region Methods

        public override void RefreshDevice()
        {
            AddresDevice = IeeeBase.AllStringConnect;
        }

        #endregion
    }

    public class Oper1VisualTest : ParagraphBase<bool>
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
            var data = base.FillData();
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

        /// <inheritdoc />
        protected override string[] GenerateDataColumnTypeObject()
        {
            return new[]
            {
                "Результат внешнего осмотра"
            };
        }

        /// <inheritdoc />
        protected override string GetReportTableName()
        {
            return "ITBmVisualTest";
        }

        protected override void InitWork(CancellationTokenSource token)
        {
            base.InitWork(token);
            var operation = new BasicOperation<bool>();
            operation.Expected = true;
            operation.IsGood = () => Equals(operation.Getting, operation.Expected);
            operation.InitWork = () =>
            {
                var service = UserItemOperation.ServicePack.QuestionText();
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

        #endregion
    }

    public class Oper2Oprobovanie : ParagraphBase<bool>
    {
        public Oper2Oprobovanie(IUserItemOperation userItemOperation) : base(userItemOperation)
        {
            Name = "Опробование";
        }
        protected TDS_Oscilloscope someTdsOscilloscope;
        #region Methods

        protected override DataTable FillData()
        {
            var data = base.FillData();
            var dataRow = data.NewRow();
            if (DataRow.Count == 1)
            {
                var dds = DataRow[0] as BasicOperation<bool>;
                dataRow[0] = dds.Getting ? "Соответствует" : dds.Comment;
                data.Rows.Add(dataRow);
            }

            return data;
        }

        /// <inheritdoc />
        protected override string[] GenerateDataColumnTypeObject()
        {
            return new[]
            {
                "Результат опробования"
            };
        }

        /// <inheritdoc />
        protected override string GetReportTableName()
        {
            return "ITBmOprobovanie";
        }

        protected override void InitWork(CancellationTokenSource token)
        {
            base.InitWork(token);
            var operation = new BasicOperation<bool>();
            operation.Expected = true;
            operation.IsGood = () => Equals(operation.Getting, operation.Expected);
            operation.InitWork = () =>
            {
                var service = UserItemOperation.ServicePack.QuestionText();
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

        #endregion
    }

    /*Определение погрешности коэффициентов отклонения.*/

    public abstract class Oper3KoefOtkl : ParagraphBase<MeasPoint<Voltage>>
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        #region Fields

        /// <summary>
        /// тестируемый канал.
        /// </summary>
        private readonly TDS_Oscilloscope.ChanelSet _testingChanel;

        protected Calibr9500B calibr9500B;

        /// <summary>
        /// Веритикальная развертка на которой производится поверка.
        /// </summary>
        protected MeasPoint<Voltage> ChanelVerticalRange;

        protected TDS_Oscilloscope someTdsOscilloscope;

        protected List<TDS_Oscilloscope.VerticalScale> verticalScalesList = new List<TDS_Oscilloscope.VerticalScale>();

        #endregion

        protected Oper3KoefOtkl(IUserItemOperation userItemOperation, TDS_Oscilloscope.ChanelSet inTestingChanel,
            string inResourceDir) :
            base(userItemOperation)
        {
            _testingChanel = inTestingChanel;
            Name = $"{_testingChanel}: Определение погрешности коэффициентов отклонения";

            Sheme = new ShemeImage
            {
                AssemblyLocalName = inResourceDir,
                Description = "Измерительная схема",
                Number = (int)_testingChanel,
                FileName = $"9500B_to_TDS_{_testingChanel}.jpg",
                ExtendedDescription = "Соберите измерительную схему, согласно рисунку"
            };

            verticalScalesList.Add(TDS_Oscilloscope.VerticalScale.Scale_2mV);
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
            var dataTable = base.FillData();

            foreach (var row in DataRow)
            {
                var dataRow = dataTable.NewRow();
                var dds = row as BasicOperationVerefication<MeasPoint<Voltage>>;
                if (dds == null) continue;

                if (dds.Expected != null && dds.Getting != null)
                {
                    dataRow["Коэффициент развёртки"] = new MeasPoint<Voltage>(dds.Expected.MainPhysicalQuantity.Value / 6,
                                                                              dds.Expected.MainPhysicalQuantity.Multiplier)
                       .Description + "/Дел";
                    var result = dds.Expected - dds.Getting;
                    result.MainPhysicalQuantity.ChangeMultiplier(dds.Expected.MainPhysicalQuantity.Multiplier);
                    result.MainPhysicalQuantity.Value = Math.Round(result.MainPhysicalQuantity.Value, 2);
                    dataRow["Абсолютная погрешность коэффициента отклонения"] = result.Description;
                }
                else
                {
                    dataRow["Абсолютная погрешность коэффициента отклонения"] = "нет данных";
                    dataRow["Коэффициент развёртки"] = "нет данных";
                }

                dataRow["Допустимое значение погрешности"] = "± " + dds.Error.Description;


                if (dds.IsGood == null)
                    dataRow[3] = "не выполнено";
                else
                    dataRow[3] = dds.IsGood() ? "Годен" : "Брак";
                dataTable.Rows.Add(dataRow);
            }

            return dataTable;
        }

        /// <inheritdoc />
        protected override string[] GenerateDataColumnTypeObject()
        {
            return new[]
            {
                "Коэффициент развёртки",
                "Абсолютная погрешность коэффициента отклонения",
                "Допустимое значение погрешности",

            }.Concat(base.GenerateDataColumnTypeObject()).ToArray();
            ;
        }

        /// <inheritdoc />
        protected override string GetReportTableName()
        {
            return $"FillTabBmOper3KoefOtkl{_testingChanel}";
        }

        protected override void InitWork(CancellationTokenSource token)
        {
            base.InitWork(token);
            if (calibr9500B == null || someTdsOscilloscope == null) return;

            foreach (var currScale in verticalScalesList)
            {
                var operation = new BasicOperationVerefication<MeasPoint<Voltage>>();
                operation.InitWork = async () =>
                {
                    try
                    {
                        await Task.Run(() =>
                        {
                            calibr9500B.StringConnection = GetStringConnect(calibr9500B);
                            someTdsOscilloscope.StringConnection = GetStringConnect(someTdsOscilloscope);
                            someTdsOscilloscope.ResetDevice();
                            Thread.Sleep(300);
                        });
                    }
                    catch (Exception e)
                    {
                        Logger.Error(e);
                        throw;
                    }
                };

                operation.BodyWorkAsync = () =>
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
                        someTdsOscilloscope.Chanel.Vertical.SetSCAle(_testingChanel, currScale);
                        //смещение для номального отображения
                        someTdsOscilloscope.Chanel.Vertical.SetPosition(_testingChanel, -3);
                        //триггер
                        someTdsOscilloscope.Trigger.SetTriggerMode(TDS_Oscilloscope.CTrigger.Mode.AUTO);
                        someTdsOscilloscope.Trigger.SetTriggerType(TDS_Oscilloscope.CTrigger.Type.EDGE);
                        someTdsOscilloscope.Trigger.SetTriggerEdgeSource(_testingChanel);
                        someTdsOscilloscope.Trigger.SetTriggerEdgeSlope(TDS_Oscilloscope.CTrigger.Slope.RIS);

                        //3.установить развертку по времени
                        someTdsOscilloscope.Horizontal.SetHorizontalScale(TDS_Oscilloscope
                                                                         .HorizontalSCAle.Scal_500mkSec);

                        //4.установить усреднение
                        someTdsOscilloscope.Acquire.SetDataCollection(TDS_Oscilloscope
                                                                     .MiscellaneousMode.SAMple); //так быстрее будет
                        //5.подать сигнал: меандр 1 кГц
                        //это разверктка
                        ChanelVerticalRange =
                            new MeasPoint<Voltage>((decimal)currScale.GetDoubleValue(),
                                                   currScale.GetUnitMultipliersValue());
                        // это подаваемая амплитуда
                        operation.Expected =
                            new MeasPoint<Voltage>(6 * (decimal)currScale.GetDoubleValue(),
                                                   currScale.GetUnitMultipliersValue());
                        calibr9500B.Source.SetFunc(Calibr9500B.Shap.SQU).Source
                                   .SetVoltage(operation.Expected).Source
                                   .SetFreq(1, UnitMultiplier.Kilo);
                        calibr9500B.Source.Output(Calibr9500B.State.On);
                        //6.снять показания с осциллографа

                        someTdsOscilloscope.Measurement.SetMeas(_testingChanel, TDS_Oscilloscope.TypeMeas.PK2);
                        someTdsOscilloscope.Acquire.SetDataCollection(TDS_Oscilloscope.MiscellaneousMode.AVErage);
                        someTdsOscilloscope.Trigger.SetTriggerLevelOn50Percent();
                        Thread.Sleep(2500);
                        someTdsOscilloscope.Trigger.SetTriggerLevelOn50Percent();
                        var measResult = someTdsOscilloscope.Measurement.MeasureValue() /
                                         (decimal)currScale.GetUnitMultipliersValue().GetDoubleValue();
                        MathStatistics.Round(ref measResult, 2);

                        someTdsOscilloscope.Acquire.SetDataCollection(TDS_Oscilloscope.MiscellaneousMode.SAMple);

                        operation.Getting =
                            new MeasPoint<Voltage>(measResult, currScale.GetUnitMultipliersValue());
                        operation.ErrorCalculation = (point, measPoint) =>
                        {
                            var nominalPoint = ChanelVerticalRange.MainPhysicalQuantity.Value *
                                               (decimal)ChanelVerticalRange
                                                        .MainPhysicalQuantity.Multiplier.GetDoubleValue();
                            MeasPoint<Voltage> resultError;
                            if (nominalPoint >= (decimal)0.01 && nominalPoint <= 5)
                            {
                                resultError = new MeasPoint<Voltage>(operation.Expected.MainPhysicalQuantity.Value * 3 / 100,
                                                                 currScale.GetUnitMultipliersValue());

                            }
                            else
                            {
                                resultError = new MeasPoint<Voltage>(operation.Expected.MainPhysicalQuantity.Value * 4 / 100,
                                                                     currScale.GetUnitMultipliersValue());
                            }

                            resultError.MainPhysicalQuantity.ChangeMultiplier(currScale.GetUnitMultipliersValue());
                            return resultError;

                        };

                        operation.UpperCalculation = (expected) => { return expected + operation.Error; };
                        operation.LowerCalculation = (expected) => { return expected - operation.Error; };
                        operation.UpperTolerance.MainPhysicalQuantity.ChangeMultiplier(currScale.GetUnitMultipliersValue());
                        operation.LowerTolerance.MainPhysicalQuantity.ChangeMultiplier(currScale.GetUnitMultipliersValue());

                        operation.IsGood = () =>
                        {
                            if (operation.Getting == null || operation.Expected == null ||
                                operation.UpperTolerance == null || operation.LowerTolerance == null) return false;
                            return (operation.Getting <= operation.UpperTolerance) &
                                   (operation.Getting >= operation.LowerTolerance);
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
                operation.CompliteWork = () => HelpsTds.HelpsCompliteWork(operation, UserItemOperation);
                DataRow.Add(DataRow.IndexOf(operation) == -1
                                ? operation
                                : (BasicOperationVerefication<MeasPoint<Voltage>>)operation.Clone());
            }
        }



        #endregion
    }

    /*Определение погрешности измерения временных интервалов.*/

    public class Oper4MeasureTimeIntervals : ParagraphBase<MeasPoint<Time>>
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        #region Fields

        /// <summary>
        /// тестируемый канал.
        /// </summary>
        private readonly TDS_Oscilloscope.ChanelSet _testingOscillosocopeChanel;

        protected Calibr9500B calibr9500B;

        /// <summary>
        /// Развертка по времени на которой производится поверка.
        /// </summary>
        protected MeasPoint<Time> ChanelHorizontalRange;

        /// <summary>
        /// Набор разверток, в зависимости от модели устройства. Как в методике поверки.
        /// </summary>
        protected Dictionary<TDS_Oscilloscope.HorizontalSCAle, MeasPoint<Time>> ScaleTolDict =
            new Dictionary<TDS_Oscilloscope.HorizontalSCAle, MeasPoint<Time>>();

        protected TDS_Oscilloscope someTdsOscilloscope;

        #endregion

        public Oper4MeasureTimeIntervals(IUserItemOperation userItemOperation,
            TDS_Oscilloscope.ChanelSet oscillosocopeChanel, string inResourceDir) : base(userItemOperation)
        {
            _testingOscillosocopeChanel = oscillosocopeChanel;
            Name = $"{_testingOscillosocopeChanel}: Определение погрешности измерения временных интервалов";
            Sheme = new ShemeImage
            {
                AssemblyLocalName = inResourceDir,
                Description = "Измерительная схема",
                Number = (int)_testingOscillosocopeChanel,
                FileName = $"9500B_to_TDS_{_testingOscillosocopeChanel}.jpg",
                ExtendedDescription = "Соберите измерительную схему, согласно рисунку"
            };
        }

        #region Methods

        protected override DataTable FillData()
        {
            var dataTable = base.FillData();

            foreach (var row in DataRow)
            {
                var dataRow = dataTable.NewRow();
                var dds = row as BasicOperationVerefication<MeasPoint<Time>>;
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

        /// <inheritdoc />
        protected override string[] GenerateDataColumnTypeObject()
        {
            return new[]
            {
                "Развертка по времени",
                "Измеренное значение периода", "Минимально допустимое значение", "Максимально допустимое значение",
                "Максимальное допустимое значение"
            }.Concat(base.GenerateDataColumnTypeObject()).ToArray();
            ;
        }

        /// <inheritdoc />
        protected override string GetReportTableName()
        {
            return $"FillTabBmOper4MeasureTimeIntervalsl{_testingOscillosocopeChanel}";
        }

        protected override void InitWork(CancellationTokenSource token)
        {
            base.InitWork(token);
            if (calibr9500B == null || someTdsOscilloscope == null) return;

            foreach (var currScale in ScaleTolDict.Keys)
            {
                var operation = new BasicOperationVerefication<MeasPoint<Time>>();
                operation.InitWork = async () =>
                {
                    try
                    {
                        await Task.Run(() =>
                        {
                            calibr9500B.StringConnection = GetStringConnect(calibr9500B);
                            someTdsOscilloscope.StringConnection = GetStringConnect(someTdsOscilloscope);
                            someTdsOscilloscope.ResetDevice();
                            Thread.Sleep(300);
                        });
                    }
                    catch (Exception e)
                    {
                        Logger.Error(e);
                        throw;
                    }
                };

                operation.BodyWorkAsync = () =>
                {
                    try
                    {
                        //1.нужно знать канал
                        someTdsOscilloscope.Chanel.SetChanelState(_testingOscillosocopeChanel, TDS_Oscilloscope.State.ON);
                        //теперь нужно понять с каким каналом мы будем работать на калибраторе
                        var calibratorHeadChnael = calibr9500B.FindActiveHeadOnChanel(new ActiveHead9510()).FirstOrDefault();
                        calibr9500B.Route.Chanel.SetChanel(calibratorHeadChnael);
                        calibr9500B.Route.Chanel.SetImpedans(Calibr9500B.Impedans.Res_1M);
                        //2.установить развертку по вертикали
                        someTdsOscilloscope.Chanel.SetProbe(_testingOscillosocopeChanel, TDS_Oscilloscope.Probe.Att_1);
                        someTdsOscilloscope.Chanel.Vertical.SetSCAle(_testingOscillosocopeChanel,
                                                                     TDS_Oscilloscope.VerticalScale.Scale_200mV);
                        someTdsOscilloscope.Chanel.Vertical.SetPosition(_testingOscillosocopeChanel, 0);

                        someTdsOscilloscope.Horizontal.SetHorizontalScale(currScale);
                        calibr9500B.Source.SetFunc(Calibr9500B.Shap.MARK);
                        calibr9500B.Source.Parametr.MARKER.SetWaveForm(Calibr9500B.MarkerWaveForm.SQU);
                        calibr9500B.Source.Parametr.MARKER.SetAmplitude(Calibr9500B.MarkerAmplitude.ampl1V);
                        ChanelHorizontalRange =
                            new MeasPoint<Time>((decimal)currScale.GetDoubleValue(),
                                                currScale.GetUnitMultipliersValue());
                        var ExpectedPoin = new MeasPoint<Time>((decimal)(currScale.GetDoubleValue() * 2),
                                                               currScale.GetUnitMultipliersValue());
                        calibr9500B.Source.Parametr.MARKER.SetPeriod(ExpectedPoin);
                        operation.Expected = ExpectedPoin;

                        calibr9500B.Source.Output(Calibr9500B.State.On);

                        //триггер
                        someTdsOscilloscope.Acquire.SetDataCollection(TDS_Oscilloscope.MiscellaneousMode.SAMple);
                        someTdsOscilloscope.Trigger.SetTriggerMode(TDS_Oscilloscope.CTrigger.Mode.AUTO);
                        someTdsOscilloscope.Trigger.SetTriggerType(TDS_Oscilloscope.CTrigger.Type.EDGE);
                        someTdsOscilloscope.Trigger.SetTriggerEdgeSource(_testingOscillosocopeChanel);
                        someTdsOscilloscope.Trigger.SetTriggerEdgeSlope(TDS_Oscilloscope.CTrigger.Slope.RIS);
                        someTdsOscilloscope.Trigger.SetTriggerLevelOn50Percent();
                        someTdsOscilloscope.Measurement.SetMeas(_testingOscillosocopeChanel, TDS_Oscilloscope.TypeMeas.PERI,
                                                                2);
                        Thread.Sleep(1000);
                        var measResult = someTdsOscilloscope.Measurement.MeasureValue(2) /
                                         (decimal)currScale.GetUnitMultipliersValue().GetDoubleValue();
                        MathStatistics.Round(ref measResult, 2);

                        operation.Getting =
                            new MeasPoint<Time>(measResult, currScale.GetUnitMultipliersValue());

                        operation.ErrorCalculation = (point, measPoint) => ScaleTolDict[currScale];


                        operation.Expected =
                            new MeasPoint<Time>((decimal)currScale.GetDoubleValue() * 2,
                                                currScale.GetUnitMultipliersValue());
                        operation.UpperCalculation = (expected) => { return expected + operation.Error; };
                        operation.LowerCalculation = (expected) => { return expected - operation.Error; };
                        operation.UpperTolerance.MainPhysicalQuantity.ChangeMultiplier(currScale.GetUnitMultipliersValue());
                        operation.LowerTolerance.MainPhysicalQuantity.ChangeMultiplier(currScale.GetUnitMultipliersValue());

                        operation.IsGood = () =>
                        {
                            if (operation.Getting == null || operation.Expected == null ||
                                operation.UpperTolerance == null || operation.LowerTolerance == null) return false;
                            return (operation.Getting <= operation.UpperTolerance) &
                                   (operation.Getting >= operation.LowerTolerance);
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
                        someTdsOscilloscope.Chanel.SetChanelState(_testingOscillosocopeChanel, TDS_Oscilloscope.State.OFF);
                    }
                };

                operation.CompliteWork = () => HelpsTds.HelpsCompliteWork(operation, UserItemOperation);
                DataRow.Add(DataRow.IndexOf(operation) == -1
                                ? operation
                                : (BasicOperationVerefication<MeasPoint<Time>>)operation.Clone());
            }
        }

        #endregion
    }

    /// <summary>
    /// Класс нужен для того, что бы выделить диапазоны верменных разверток в зависимости от модели, по МП.
    /// </summary>
    public class TDS20XXBOper4MeasureTimeIntervals : Oper4MeasureTimeIntervals
    {
        public TDS20XXBOper4MeasureTimeIntervals(IUserItemOperation userItemOperation,
            TDS_Oscilloscope.ChanelSet oscillosocopeChanel, string inResourceDir) : base(userItemOperation, oscillosocopeChanel, inResourceDir)
        {
            ScaleTolDict.Add(TDS_Oscilloscope.HorizontalSCAle.Scal_2_5nSec,
                             new MeasPoint<Time>(0.61M, UnitMultiplier.Nano));
            ScaleTolDict.Add(TDS_Oscilloscope.HorizontalSCAle.Scal_50nSec,
                             new MeasPoint<Time>(0.81M, UnitMultiplier.Nano));
            ScaleTolDict.Add(TDS_Oscilloscope.HorizontalSCAle.Scal_250nSec,
                             new MeasPoint<Time>(1.63m, UnitMultiplier.Nano));
            ScaleTolDict.Add(TDS_Oscilloscope.HorizontalSCAle.Scal_500mkSec,
                             new MeasPoint<Time>(2.05m, UnitMultiplier.Micro));
            ScaleTolDict.Add(TDS_Oscilloscope.HorizontalSCAle.Scal_2_5mSec,
                             new MeasPoint<Time>(10.25m, UnitMultiplier.Micro));
        }
    }

    /// <summary>
    /// Класс нужен для того, что бы выделить диапазоны верменных разверток в зависимости от модели, по МП.
    /// </summary>
    public class TDS10XXBOper4MeasureTimeIntervals : Oper4MeasureTimeIntervals
    {
        public TDS10XXBOper4MeasureTimeIntervals(IUserItemOperation userItemOperation,
            TDS_Oscilloscope.ChanelSet oscillosocopeChanel, string inResourceDir) : base(userItemOperation, oscillosocopeChanel, inResourceDir)
        {
            ScaleTolDict.Add(TDS_Oscilloscope.HorizontalSCAle.Scal_5nSec,
                             new MeasPoint<Time>(0.62M, UnitMultiplier.Nano));
            ScaleTolDict.Add(TDS_Oscilloscope.HorizontalSCAle.Scal_50nSec,
                             new MeasPoint<Time>(0.81m, UnitMultiplier.Nano));
            ScaleTolDict.Add(TDS_Oscilloscope.HorizontalSCAle.Scal_250nSec,
                             new MeasPoint<Time>(1.63m, UnitMultiplier.Nano));
            ScaleTolDict.Add(TDS_Oscilloscope.HorizontalSCAle.Scal_500mkSec,
                             new MeasPoint<Time>(2.05M, UnitMultiplier.Micro));
            ScaleTolDict.Add(TDS_Oscilloscope.HorizontalSCAle.Scal_2_5mSec,
                             new MeasPoint<Time>(10.25M, UnitMultiplier.Micro));
        }
    }

    /*Определение Времени нарастания переходной характеристики.*/

    public class Oper5MeasureRiseTime : ParagraphBase<object>
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

        protected MeasPoint<Time> RiseTimeTol;
        protected TDS_Oscilloscope someTdsOscilloscope;

        /// <summary>
        /// Набор разверток, в зависимости от модели устройства. Как в методике поверки.
        /// </summary>
        protected List<TDS_Oscilloscope.VerticalScale> verticalScalesList = new List<TDS_Oscilloscope.VerticalScale>();

        #endregion

        protected Oper5MeasureRiseTime(IUserItemOperation userItemOperation, TDS_Oscilloscope.ChanelSet chanel,
            string inResourceDir) :
            base(userItemOperation)
        {
            _testingChanel = chanel;
            Name = $"{_testingChanel}: Определение Времени нарастания переходной характеристики";

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

            Sheme = new ShemeImage
            {
                AssemblyLocalName = inResourceDir,
                Description = "Измерительная схема",
                Number = (int)_testingChanel,
                FileName = $"9500B_to_TDS_{_testingChanel}.jpg",
                ExtendedDescription = "Соберите измерительную схему, согласно рисунку"
            };
        }

        #region Methods

        protected override DataTable FillData()
        {
            var dataTable = base.FillData();

            foreach (var row in DataRow)
            {
                var dataRow = dataTable.NewRow();
                var dds = row as BasicOperationVerefication<object>;
                if (dds == null) continue;

                dataRow[0] = new MeasPoint<Time>(2.5M, UnitMultiplier.Nano).Description + "/Дел";
                dataRow[1] = new MeasPoint<Voltage>(((MeasPoint<Voltage>)dds.Expected).MainPhysicalQuantity.Value / 3,
                                                    ((MeasPoint<Voltage>)dds.Expected).MainPhysicalQuantity.Multiplier)
                   .Description + "/Дел";
                dataRow[2] = ((MeasPoint<Time>)dds?.Getting).Description;
                dataRow[3] = ((MeasPoint<Time>)dds?.Error).Description;

                if (dds.IsGood == null)
                    dataRow[4] = "не выполнено";
                else
                    dataRow[4] = dds.IsGood() ? "Годен" : "Брак";
                dataTable.Rows.Add(dataRow);
            }

            return dataTable;
        }

        /// <inheritdoc />
        protected override string[] GenerateDataColumnTypeObject()
        {
            return new[]
            {
                "Горизонтальная развёртка",
                "Коэффициент отклонения",
                "Измеренное значение длительности фронта",
                "Минимально допустимое значение длительности фронта"
            }.Concat(base.GenerateDataColumnTypeObject()).ToArray();
            ;
        }

        /// <inheritdoc />
        protected override string GetReportTableName()
        {
            return $"FillTabBmOper5MeasureRiseTime{_testingChanel}";
        }

        protected override void InitWork(CancellationTokenSource token)
        {
            base.InitWork(token);
            if (calibr9500B == null || someTdsOscilloscope == null) return;

            foreach (var verticalScale in verticalScalesList)
            {
                //первый коэффициент отклонения пропускаем
                if (verticalScale == TDS_Oscilloscope.VerticalScale.Scale_2mV ||
                    verticalScale == TDS_Oscilloscope.VerticalScale.Scale_20mV) continue;

                var operation = new BasicOperationVerefication<object>();
                operation.InitWork = async () =>
                {
                    try
                    {
                        await Task.Run(() =>
                        {
                            calibr9500B.StringConnection = GetStringConnect(calibr9500B);
                            someTdsOscilloscope.StringConnection = GetStringConnect(someTdsOscilloscope);
                            someTdsOscilloscope.ResetDevice();
                            Thread.Sleep(300);
                        });
                    }
                    catch (Exception e)
                    {
                        Logger.Error(e);
                        throw;
                    }
                };

                operation.BodyWorkAsync = () =>
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
                        operation.Expected = new MeasPoint<Voltage>((decimal)(3 * verticalScale.GetDoubleValue()),
                                                                    verticalScale.GetUnitMultipliersValue());
                        calibr9500B.Source.SetVoltage((MeasPoint<Voltage>)operation.Expected);

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
                            new MeasPoint<Time>(measResult, horizontalScAleForTest.GetUnitMultipliersValue());
                        operation.ErrorCalculation = (point, measPoint) => RiseTimeTol;


                        operation.IsGood = () =>
                        {
                            if (operation.Getting == null || operation.Expected == null ||
                                operation.UpperTolerance == null) return false;
                            return (MeasPoint<Time>)operation.Getting <= (MeasPoint<Time>)operation.UpperTolerance;
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
                        someTdsOscilloscope.Chanel.SetChanelState(_testingChanel, TDS_Oscilloscope.State.OFF)
                                           .ResetDevice();
                    }
                };

                operation.CompliteWork = () => Task.FromResult(true);
                DataRow.Add(DataRow.IndexOf(operation) == -1
                                ? operation
                                : (BasicOperationVerefication<object>)operation.Clone());
            }
        }

        #endregion
    }
}