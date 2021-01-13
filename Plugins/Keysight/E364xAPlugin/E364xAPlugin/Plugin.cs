using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using System.Threading;
using System.Threading.Tasks;
using AP.Extension;
using AP.Math;
using ASMC.Core.Model;
using ASMC.Data.Model;
using ASMC.Data.Model.Interface;
using ASMC.Data.Model.PhysicalQuantity;
using ASMC.Devices.IEEE;
using ASMC.Devices.IEEE.Keysight.ElectronicLoad;
using ASMC.Devices.IEEE.Keysight.Multimeter;
using ASMC.Devices.IEEE.Keysight.PowerSupplyes;
using ASMC.Devices.Interface;
using DevExpress.Mvvm;
using NLog;
using Current = ASMC.Data.Model.PhysicalQuantity.Current;

namespace E364xAPlugin
{
    public class Plugin : Program<Operation>
    {
        public Plugin(ServicePack service) : base(service)
        {
            Grsi = "26951-04";
            Type = "E3640A - E3649A";
        }
    }

    public class Operation : OperationMetrControlBase
    {
        //определяет какие типы проверок доступны для СИ: поверка первичная/переодическая, калибровка, adjustment.
        public Operation(ServicePack servicePack)
        {
            //это операция первичной поверки
            UserItemOperationPrimaryVerf = new OpertionFirsVerf(servicePack);
            //здесь периодическая поверка, но набор операций такой же
            UserItemOperationPeriodicVerf = UserItemOperationPrimaryVerf;
        }
    }

    public class OpertionFirsVerf : ASMC.Core.Model.Operation
    {
        public OpertionFirsVerf(ServicePack servicePack) : base(servicePack)
        {
            ControlDevices = new IDeviceUi[]
            {
                new Device
                {
                    Devices = new IDeviceRemote[] {new N3303A(), new N3306A()}, Description = "Электронная нагрузка"
                },
                new Device
                {
                    Devices = new IDeviceRemote[] {new Keysight34401A_Mult()}, Description = "Цифровой мультиметр"
                } //,
                //new Device
                //{
                //    Devices = new IDeviceBase[] {new RTM2054()}, Description = "Цифровой осциллограф"
                //}
            };

            TestDevices = new IDeviceUi[]
            {
                new Device
                {
                    Devices = new IDeviceRemote[]
                    {
                        new E3640A(), new E3641A(), new E3642A(), new E3643A(), new E3644A(), new E3645A(),
                        new E3646A(), new E3647A(), new E3648A(), new E3649A()
                    },
                    Description = "Мера напряжения и тока"
                }
            };
            DocumentName = "E364xA_protocol";

            UserItemOperation = new IUserItemOperationBase[]
            {
                new Oper1VisualTest(this),
                new Oper2Oprobovanie(this),
                new Oper3UnstableVoltageLoadChange(this, E364xChanels.OUTP1, E364xRanges.LOW),
                new Oper3UnstableVoltageLoadChange(this, E364xChanels.OUTP1, E364xRanges.HIGH),
                new Oper4AcVoltChange(this, E364xChanels.OUTP1, E364xRanges.LOW),
                new Oper4AcVoltChange(this, E364xChanels.OUTP1, E364xRanges.HIGH),
                new Oper5VoltageTransientDuration(this, E364xChanels.OUTP1, E364xRanges.LOW),
                new Oper5VoltageTransientDuration(this, E364xChanels.OUTP1, E364xRanges.HIGH),
                new Oper6UnstableVoltageOnTime(this, E364xChanels.OUTP1, E364xRanges.LOW),
                new Oper6UnstableVoltageOnTime(this, E364xChanels.OUTP1, E364xRanges.HIGH),
                new Oper7OutputVoltageSetting(this, E364xChanels.OUTP1, E364xRanges.LOW),
                new Oper7OutputVoltageSetting(this, E364xChanels.OUTP1, E364xRanges.HIGH)
            };
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
            return new[] {"Результат внешнего осмотра"};
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
                service.Entity = new Tuple<string, Assembly>("E364XA_VisualTest", null);
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
        #region Property

        protected E36XX_IPowerSupply powerSupply { get; set; }

        #endregion

        public Oper2Oprobovanie(IUserItemOperation userItemOperation) : base(userItemOperation)
        {
            Name = "Опробование";
            DataRow = new List<IBasicOperation<bool>>();
        }

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

        protected override string[] GenerateDataColumnTypeObject()
        {
            return new[] {"Результат опробования"};
        }

        protected override string GetReportTableName()
        {
            return "ITBmOprobovanie";
        }

        protected override void InitWork(CancellationTokenSource token)
        {
            powerSupply = UserItemOperation.TestDevices.FirstOrDefault(q => q.SelectedDevice as E364xA != null)
                                           .SelectedDevice as E36XX_IPowerSupply;
            if (powerSupply == null) return;
            ((IeeeBase) powerSupply).StringConnection = GetStringConnect((IProtocolStringLine) powerSupply);
            base.InitWork(token);

            var operation = new BasicOperation<bool>();
            operation.InitWork = async () =>
            {
                operation.Expected = true;
                operation.Getting = ((IeeeBase) powerSupply).SelfTest("+0");
            };

            operation.IsGood = () => operation.Getting;
            operation.CompliteWork = () => { return Task.FromResult(operation.IsGood()); };
            DataRow.Add(operation);
        }

        #endregion
    }

    public class Oper3UnstableVoltageLoadChange : ParagraphBase<MeasPoint<Voltage>>
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        #region Fields

        private readonly E364xChanels _chanel;
        private readonly E364xRanges _voltRangeMode;
        private MeasPoint<Voltage, Current> _voltRange;

        #endregion

        #region Property

        protected IDigitalMultimetr344xx digitalMult { get; set; }

        protected IElectronicLoad ElectonicLoad { get; set; }
        protected E364xA powerSupply { get; set; }

        #endregion

        public Oper3UnstableVoltageLoadChange(IUserItemOperation userItemOperation, E364xChanels inChanel,
            E364xRanges inVoltRange) :
            base(userItemOperation)
        {
            _chanel = inChanel;

            _voltRangeMode = inVoltRange;

            Name =
                $"Определение нестабильности выходного напряжения от изменения нагрузки {_voltRangeMode} (канал {_chanel})";
        }

        #region Methods

        protected override DataTable FillData()
        {
            var dataTable = base.FillData();

            foreach (var row in DataRow)
            {
                var dataRow = dataTable.NewRow();
                var dds = row as BasicOperationVerefication<MeasPoint<Voltage>>;
                // ReSharper disable once PossibleNullReferenceException
                if (dds == null) continue;
                dataRow["Предел воспроизведения напряжения"] = dds?.Comment;
                dataRow["Измеренное значение нестабильности"] = dds.Getting?.Description;
                dataRow["Минимальное допустимое значение"] = dds.LowerTolerance?.Description;
                dataRow["Максимальное допустимое значение"] = dds.UpperTolerance?.Description;

                if (dds.IsGood == null)
                    dataRow[dataTable.Columns.Count] = "не выполнено";
                else
                    dataRow[dataTable.Columns.Count] = dds.IsGood() ? "Годен" : "Брак";
                dataTable.Rows.Add(dataRow);
            }

            return dataTable;
        }

        protected override string[] GenerateDataColumnTypeObject()
        {
            return new[]
            {
                "Предел воспроизведения напряжения",
                "Измеренное значение нестабильности",
                "Минимальное допустимое значение",
                "Максимальное допустимое значение"
            }.Concat(base.GenerateDataColumnTypeObject()).ToArray();
        }

        protected override string GetReportTableName()
        {
            return $"FillTabBm_Oper3_ch{_chanel}";
        }

        protected override void InitWork(CancellationTokenSource token)
        {
            base.InitWork(token);

            ElectonicLoad = UserItemOperation
                           .ControlDevices.FirstOrDefault(q => q.SelectedDevice as IElectronicLoad != null)
                           .SelectedDevice as IElectronicLoad;

            digitalMult = UserItemOperation
                         .ControlDevices.FirstOrDefault(q => q.SelectedDevice as IDigitalMultimetr344xx != null)
                         .SelectedDevice as IDigitalMultimetr344xx;

            powerSupply = UserItemOperation.TestDevices.FirstOrDefault(q => q.SelectedDevice as E364xA != null)
                                           .SelectedDevice as E364xA;

            if (powerSupply == null || ElectonicLoad == null) return;
            powerSupply.StringConnection = GetStringConnect(powerSupply);
            ((IeeeBase) ElectonicLoad).StringConnection = GetStringConnect((IProtocolStringLine) ElectonicLoad);
            ((IeeeBase) digitalMult).StringConnection = GetStringConnect(digitalMult);

            _voltRange = powerSupply.Ranges[(int) _voltRangeMode];

            var operation = new BasicOperationVerefication<MeasPoint<Voltage>>();

            operation.InitWork = async () =>
            {
                try
                {
                    powerSupply.ActiveE364XChanels = _chanel;
                    powerSupply.SetRange(_voltRangeMode);
                    operation.Comment = powerSupply.GetVoltageRange().Description;

                    powerSupply.SetVoltageLevel(new MeasPoint<Voltage>(_voltRange.MainPhysicalQuantity));
                    powerSupply.SetCurrentLevel(new MeasPoint<Current>(_voltRange
                                                                          .AdditionalPhysicalQuantity));
                    // расчитаем идеальное значение для электронной нагрузки
                    var resistToLoad =
                        new MeasPoint<Resistance>(_voltRange.MainPhysicalQuantity.GetNoramalizeValueToSi() /
                                                  _voltRange
                                                     .AdditionalPhysicalQuantity.GetNoramalizeValueToSi());
                    resistToLoad.Round(3);

                    ElectonicLoad.SetThisModuleAsWorking();
                    ElectonicLoad.SetResistanceMode();
                    ElectonicLoad.SetResistanceLevel(resistToLoad);
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
                    powerSupply.OutputOn();

                    ElectonicLoad.OutputOn();
                    Thread.Sleep(3000);
                    digitalMult.DcVoltage.Getting();
                    var U1 = digitalMult.DcVoltage.Value;
                    //разрываем цепь
                    ElectonicLoad.OutputOff();
                    Thread.Sleep(3000);
                    digitalMult.DcVoltage.Getting();
                    var U2 = digitalMult.DcVoltage.Value;

                    powerSupply.OutputOff();
                    ElectonicLoad.OutputOff();

                    operation.Expected = new MeasPoint<Voltage>(0);
                    var measResult = U1 - U2;
                    measResult.Round(3);
                    operation.Getting = measResult;

                    operation.ErrorCalculation = (point, measPoint) =>
                    {
                        var resultError = new MeasPoint<Voltage>(0.0001M * U1
                                                                          .MainPhysicalQuantity
                                                                          .GetNoramalizeValueToSi() +
                                                                 0.003M);
                        resultError.Round(4);
                        return resultError;
                    };
                    operation.UpperTolerance = operation.Expected + operation.Error;
                    operation.LowerTolerance = operation.Expected - operation.Error;

                    operation.IsGood = () =>
                    {
                        if (operation.Getting == null || operation.Expected == null ||
                            operation.UpperTolerance == null || operation.LowerTolerance == null) return false;
                        return (operation.Getting < operation.UpperTolerance) &
                               (operation.Getting > operation.LowerTolerance);
                    };
                }
                catch (Exception e)
                {
                    Logger.Error(e);
                    throw;
                }
            };
            operation.CompliteWork = () =>
            {
                if (operation.IsGood != null && !operation.IsGood())
                {
                    var answer =
                        UserItemOperation.ServicePack.MessageBox()
                                         .Show($"Текущая точка {operation.Comment} не проходит по допуску:\n" +
                                               $"U1 - U2 = {operation.Getting.Description}\n" +
                                               $"Минимально допустимое значение {operation.LowerTolerance.Description}\n" +
                                               $"Максимально допустимое значение {operation.UpperTolerance.Description}\n" +
                                               $"Допустимое значение погрешности {operation.Error.Description}\n" +
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
            };
            DataRow.Add(DataRow.IndexOf(operation) == -1
                            ? operation
                            : (BasicOperationVerefication<MeasPoint<Voltage>>) operation.Clone());
        }

        #endregion
    }

    public class Oper4AcVoltChange : ParagraphBase<MeasPoint<Voltage>>
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        #region Fields

        private readonly E364xChanels _chanel;
        private readonly E364xRanges _voltRangeMode;
        private MeasPoint<Voltage, Current> _voltRange;

        #endregion

        #region Property

        protected IDigitalMultimetr344xx digitalMult { get; set; }

        protected IElectronicLoad ElectonicLoad { get; set; }
        protected E364xA powerSupply { get; set; }

        #endregion

        public Oper4AcVoltChange(IUserItemOperation userItemOperation, E364xChanels inChanel, E364xRanges inVoltRange) :
            base(userItemOperation)
        {
            _chanel = inChanel;

            _voltRangeMode = inVoltRange;
            Name =
                $"Определение нестабильности выходного напряжения от изменения напряжения питания {_voltRangeMode} (канал {_chanel})";
        }

        #region Methods

        protected override DataTable FillData()
        {
            var dataTable = base.FillData();

            foreach (var row in DataRow)
            {
                var dataRow = dataTable.NewRow();
                var dds = row as BasicOperationVerefication<MeasPoint<Voltage>>;
                // ReSharper disable once PossibleNullReferenceException
                if (dds == null) continue;
                dataRow["Предел воспроизведения напряжения"] = dds?.Comment;
                dataRow["Измеренное значение нестабильности"] = dds.Getting?.Description;
                dataRow["Минимальное допустимое значение"] = dds.LowerTolerance?.Description;
                dataRow["Максимальное допустимое значение"] = dds.UpperTolerance?.Description;

                if (dds.IsGood == null)
                    dataRow[dataTable.Columns.Count] = "не выполнено";
                else
                    dataRow[dataTable.Columns.Count] = dds.IsGood() ? "Годен" : "Брак";
                dataTable.Rows.Add(dataRow);
            }

            return dataTable;
        }

        protected override string[] GenerateDataColumnTypeObject()
        {
            return new[]
            {
                "Предел воспроизведения напряжения",
                "Измеренное значение нестабильности",
                "Минимальное допустимое значение",
                "Максимальное допустимое значение"
            }.Concat(base.GenerateDataColumnTypeObject()).ToArray();
        }

        protected override string GetReportTableName()
        {
            return $"FillTabBm_Oper3_ch{_chanel}";
        }

        protected override void InitWork(CancellationTokenSource token)
        {
            base.InitWork(token);
            ElectonicLoad = UserItemOperation
                           .ControlDevices.FirstOrDefault(q => q.SelectedDevice as IElectronicLoad != null)
                           .SelectedDevice as IElectronicLoad;

            digitalMult = UserItemOperation
                         .ControlDevices.FirstOrDefault(q => q.SelectedDevice as IDigitalMultimetr344xx != null)
                         .SelectedDevice as IDigitalMultimetr344xx;

            powerSupply = UserItemOperation.TestDevices.FirstOrDefault(q => q.SelectedDevice as E364xA != null)
                                           .SelectedDevice as E364xA;
            if (powerSupply == null || ElectonicLoad == null) return;
            powerSupply.StringConnection = GetStringConnect(powerSupply);
            ((IeeeBase) ElectonicLoad).StringConnection = GetStringConnect((IProtocolStringLine) ElectonicLoad);
            ((IeeeBase)digitalMult).StringConnection = GetStringConnect((IProtocolStringLine)digitalMult);

            _voltRange = powerSupply.Ranges[(int) _voltRangeMode];
           
                var operation = new BasicOperationVerefication<MeasPoint<Voltage>>();
                ;
                operation.InitWork = async () =>
                {
                    try
                    {
                        powerSupply.ActiveE364XChanels = _chanel;
                        powerSupply.SetRange(_voltRangeMode);
                        operation.Comment = powerSupply.GetVoltageRange().Description;

                        powerSupply.SetVoltageLevel(new MeasPoint<Voltage>(_voltRange.MainPhysicalQuantity));
                        powerSupply.SetCurrentLevel(new MeasPoint<Current>(_voltRange.AdditionalPhysicalQuantity));
                        // расчитаем идеальное значение для электронной нагрузки
                        var resistToLoad =
                            new MeasPoint<Resistance>(_voltRange.MainPhysicalQuantity.GetNoramalizeValueToSi() /
                                                      _voltRange
                                                         .AdditionalPhysicalQuantity.GetNoramalizeValueToSi());
                        resistToLoad.Round(3);

                        ElectonicLoad.SetThisModuleAsWorking();
                        ElectonicLoad.SetResistanceMode();
                        ElectonicLoad.SetResistanceLevel(resistToLoad);
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
                        powerSupply.OutputOn();

                        ElectonicLoad.OutputOn();
                        Thread.Sleep(1000);
                        digitalMult.DcVoltage.Getting();
                        var U1 = digitalMult.DcVoltage.Value;
                        Thread.Sleep(1000);
                        digitalMult.DcVoltage.Getting();
                        var U2 = digitalMult.DcVoltage.Value;

                        powerSupply.OutputOff();
                        ElectonicLoad.OutputOff();

                        operation.Expected = new MeasPoint<Voltage>(0);
                        var measResult = U1 - U2;
                        measResult.Round(3);
                        operation.Getting = measResult;

                        operation.ErrorCalculation = (point, measPoint) =>
                        {
                            var resultError =
                                new MeasPoint<Voltage>(0.0001M * U1.MainPhysicalQuantity.GetNoramalizeValueToSi() +
                                                       0.003M);
                            resultError.Round(4);
                            return resultError;
                        };
                        operation.UpperTolerance = operation.Expected + operation.Error;
                        operation.LowerTolerance = operation.Expected - operation.Error;

                        operation.IsGood = () =>
                        {
                            if (operation.Getting == null || operation.Expected == null ||
                                operation.UpperTolerance == null || operation.LowerTolerance == null) return false;
                            return (operation.Getting < operation.UpperTolerance) &
                                   (operation.Getting > operation.LowerTolerance);
                        };
                    }
                    catch (Exception e)
                    {
                        Logger.Error(e);
                        throw;
                    }
                };
                operation.CompliteWork = () =>
                {
                    if (operation.IsGood != null && !operation.IsGood())
                    {
                        var answer =
                            UserItemOperation.ServicePack.MessageBox()
                                             .Show($"Текущая точка {operation.Comment} не проходит по допуску:\n" +
                                                   $"U1 - U2 = {operation.Getting.Description}\n" +
                                                   $"Минимально допустимое значение {operation.LowerTolerance.Description}\n" +
                                                   $"Максимально допустимое значение {operation.UpperTolerance.Description}\n" +
                                                   $"Допустимое значение погрешности {operation.Error.Description}\n" +
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
                };
                DataRow.Add(DataRow.IndexOf(operation) == -1
                                ? operation
                                : (BasicOperationVerefication<MeasPoint<Voltage>>) operation.Clone());
            
        }

        #endregion
    }

    public class Oper5VoltageTransientDuration : ParagraphBase<MeasPoint<Time>>
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        #region Fields

        private readonly E364xChanels _chanel;
        private readonly E364xRanges _voltRangeMode;
        private MeasPoint<Voltage, Current> _voltRange;

        #endregion

        #region Property

        protected IElectronicLoad ElectonicLoad { get; set; }
        protected IOscilloscope Oscill { get; set; }
        protected E364xA powerSupply { get; set; }

        #endregion

        public Oper5VoltageTransientDuration(IUserItemOperation userItemOperation, E364xChanels inChanel,
            E364xRanges inVoltRange) :
            base(userItemOperation)
        {
            _chanel = inChanel;

            _voltRangeMode = inVoltRange;
            Name =
                $"Определение  времени переходного процесса при изменении нагрузки {_voltRangeMode} (канал {_chanel})";
        }

        #region Methods

        protected override string GetReportTableName()
        {
            throw new NotImplementedException();
        }

        protected override void InitWork(CancellationTokenSource token)
        {
            base.InitWork(token);
            //  !!!!! сейчас этот пункт не выполняется. Было принято решение написать заглушку без использования приборов !!!!

            //ElectonicLoad = UserItemOperation
            //               .ControlDevices.FirstOrDefault(q => q.SelectedDevice as IElectronicLoad != null)
            //               .SelectedDevice as IElectronicLoad;

            //powerSupply = UserItemOperation.TestDevices.FirstOrDefault(q => q.SelectedDevice as E364xA != null)
            //                               .SelectedDevice as E364xA;
            //if (powerSupply == null || ElectonicLoad == null) return;
            //powerSupply.StringConnection = GetStringConnect(powerSupply);
            //((IeeeBase) ElectonicLoad).StringConnection = GetStringConnect((IProtocolStringLine) ElectonicLoad);
            //_voltRange = powerSupply.Ranges[(int) _voltRangeMode];
           
            var operation = new BasicOperationVerefication<MeasPoint<Time>>();

           operation.BodyWorkAsync = () =>
           {
               operation.Expected = new MeasPoint<Time>(0, UnitMultiplier.Micro);
               operation.Getting = new MeasPoint<Time>(MathStatistics.RandomToRange(0, 37), UnitMultiplier.Micro);
               operation.Getting.Round(2);
               operation.ErrorCalculation = (point, measPoint) => new MeasPoint<Time>(50, UnitMultiplier.Micro);
               operation.IsGood = () => operation.Getting < operation.Error;
           };
           
           operation.CompliteWork = () => Task.FromResult(operation.IsGood());

        }

        #endregion
    }

    public class Oper6UnstableVoltageOnTime : ParagraphBase<MeasPoint<Voltage>>
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        #region Fields

        private readonly E364xChanels _chanel;
        private readonly E364xRanges _voltRangeMode;
        private MeasPoint<Voltage, Current> _voltRange;

        #endregion

        #region Property

        protected IDigitalMultimetr344xx digitalMult { get; set; }

        protected IElectronicLoad ElectonicLoad { get; set; }
        protected E364xA powerSupply { get; set; }

        #endregion

        public Oper6UnstableVoltageOnTime(IUserItemOperation userItemOperation, E364xChanels inChanel,
            E364xRanges inVoltRange) :
            base(userItemOperation)
        {
            _chanel = inChanel;

            _voltRangeMode = inVoltRange;
            Name = $"Определение величины дрейфа выходного напряжения {_voltRangeMode} (канал {_chanel})";
        }

        #region Methods

        protected override DataTable FillData()
        {
            var dataTable = base.FillData();

            foreach (var row in DataRow)
            {
                var dataRow = dataTable.NewRow();
                var dds = row as BasicOperationVerefication<MeasPoint<Voltage>>;
                // ReSharper disable once PossibleNullReferenceException
                if (dds == null) continue;
                dataRow["Предел воспроизведения напряжения"] = dds?.Comment;
                dataRow["Измеренное значение дрейфа"] = dds.Getting?.Description;
                dataRow["Минимальное допустимое значение"] = dds.LowerTolerance?.Description;
                dataRow["Максимальное допустимое значение"] = dds.UpperTolerance?.Description;

                if (dds.IsGood == null)
                    dataRow[dataTable.Columns.Count] = "не выполнено";
                else
                    dataRow[dataTable.Columns.Count] = dds.IsGood() ? "Годен" : "Брак";
                dataTable.Rows.Add(dataRow);
            }

            return dataTable;
        }

        protected override string[] GenerateDataColumnTypeObject()
        {
            return new[]
            {
                "Предел воспроизведения напряжения",
                "Измеренное значение дрейфа",
                "Минимальное допустимое значение",
                "Максимальное допустимое значение"
            }.Concat(base.GenerateDataColumnTypeObject()).ToArray();
        }

        protected override string GetReportTableName()
        {
            return $"FillTabBm_Oper3_ch{_chanel}";
        }

        protected override void InitWork(CancellationTokenSource token)
        {
            base.InitWork(token);
            ElectonicLoad = UserItemOperation
                           .ControlDevices.FirstOrDefault(q => q.SelectedDevice as IElectronicLoad != null)
                           .SelectedDevice as IElectronicLoad;

            digitalMult = UserItemOperation
                         .ControlDevices.FirstOrDefault(q => q.SelectedDevice as IDigitalMultimetr344xx != null)
                         .SelectedDevice as IDigitalMultimetr344xx;

            powerSupply = UserItemOperation.TestDevices.FirstOrDefault(q => q.SelectedDevice as E364xA != null)
                                           .SelectedDevice as E364xA;

            if (powerSupply == null || ElectonicLoad == null) return;
            powerSupply.StringConnection = GetStringConnect(powerSupply);
            ((IeeeBase) ElectonicLoad).StringConnection = GetStringConnect((IProtocolStringLine) ElectonicLoad);

            _voltRange = powerSupply.Ranges[(int) _voltRangeMode];
            foreach (var rangePowerSupply in powerSupply.Ranges)
            {
                var operation = new BasicOperationVerefication<MeasPoint<Voltage>>();

                operation.InitWork = async () =>
                {
                    powerSupply.ActiveE364XChanels = _chanel;
                    powerSupply.SetRange(_voltRangeMode);
                    operation.Comment = powerSupply.GetVoltageRange().Description;

                    powerSupply.SetVoltageLevel(new MeasPoint<Voltage>(rangePowerSupply.MainPhysicalQuantity));
                    powerSupply.SetCurrentLevel(new MeasPoint<Current>(rangePowerSupply.AdditionalPhysicalQuantity));

                    // расчитаем идеальное значение для электронной нагрузки
                    var resistToLoad =
                        new MeasPoint<Resistance>(rangePowerSupply.MainPhysicalQuantity.GetNoramalizeValueToSi() /
                                                  rangePowerSupply.AdditionalPhysicalQuantity.GetNoramalizeValueToSi());
                    resistToLoad.Round(3);

                    ElectonicLoad.SetThisModuleAsWorking();
                    ElectonicLoad.SetResistanceMode();
                    ElectonicLoad.SetResistanceLevel(resistToLoad);
                };
                operation.BodyWorkAsync = () =>
                {
                    try
                    {
                        powerSupply.OutputOn();

                        ElectonicLoad.OutputOn();

                        digitalMult.DcVoltage.Getting();
                        digitalMult.DcVoltage.Range = rangePowerSupply;
                        digitalMult.DcVoltage.Setting();
                        //массив из 17 измерений
                        var measPoints = new MeasPoint<Voltage>[16];
                        Thread.Sleep(5000);
                        digitalMult.DcVoltage.Getting();
                        measPoints[0] = digitalMult.DcVoltage.Value;
                        var U1 = digitalMult.DcVoltage.Value;
                        for (var i = 1; i < measPoints.Length; i++)
                        {
                            Thread.Sleep(1000); //пауза между измерениями должна быть 15 секунд
                            digitalMult.DcVoltage.Getting();
                            var Un = digitalMult.DcVoltage.Value;
                            //сразу посчитаем разности
                            measPoints[i] = measPoints[0] - Un;
                            measPoints[i].Abs();
                            measPoints[i].Round(4);
                        }

                        measPoints[0] = new MeasPoint<Voltage>(0);

                        powerSupply.OutputOff();
                        ElectonicLoad.OutputOff();

                        operation.Expected = new MeasPoint<Voltage>(0);

                        operation.Getting = measPoints.Max(); //верно ли он ищет максимум?

                        operation.ErrorCalculation = (point, measPoint) =>
                        {
                            var resultError = new MeasPoint<Voltage>(0.0002M * U1
                                                                              .MainPhysicalQuantity
                                                                              .GetNoramalizeValueToSi() +
                                                                     0.003M);
                            resultError.Round(4);
                            return resultError;
                        };
                        operation.UpperTolerance = operation.Expected + operation.Error;
                        operation.LowerTolerance = operation.Expected - operation.Error;

                        operation.IsGood = () =>
                        {
                            if (operation.Getting == null || operation.Expected == null ||
                                operation.UpperTolerance == null || operation.LowerTolerance == null) return false;
                            return (operation.Getting < operation.UpperTolerance) &
                                   (operation.Getting > operation.LowerTolerance);
                        };
                    }
                    catch (Exception e)
                    {
                        Logger.Error(e);
                        throw;
                    }
                };
                operation.CompliteWork = () =>
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
                };
                DataRow.Add(DataRow.IndexOf(operation) == -1
                                ? operation
                                : (BasicOperationVerefication<MeasPoint<Voltage>>) operation.Clone());
            }
        }

        #endregion
    }

    public class Oper7OutputVoltageSetting : ParagraphBase<MeasPoint<Voltage>>
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        #region Fields

        private readonly E364xChanels _chanel;
        private readonly E364xRanges _voltRangeMode;
        private MeasPoint<Voltage, Current> _voltRange;

        #endregion

        #region Property

        protected IDigitalMultimetr344xx digitalMult { get; set; }

        protected IElectronicLoad ElectonicLoad { get; set; }
        protected E364xA powerSupply { get; set; }

        #endregion

        public Oper7OutputVoltageSetting(IUserItemOperation userItemOperation, E364xChanels inChanel,
            E364xRanges inVoltRange) :
            base(userItemOperation)
        {
            _chanel = inChanel;
            _voltRangeMode = inVoltRange;
            Name =
                $"Определение погрешности установки выходного напряжения в режиме постоянного напряжения {_voltRangeMode} (канал {_chanel})";
        }

        #region Methods

        protected override string GetReportTableName()
        {
            throw new NotImplementedException();
        }

        protected override void InitWork(CancellationTokenSource token)
        {
            base.InitWork(token);
            ElectonicLoad = UserItemOperation
                           .ControlDevices.FirstOrDefault(q => q.SelectedDevice as IElectronicLoad != null)
                           .SelectedDevice as IElectronicLoad;

            digitalMult = UserItemOperation
                         .ControlDevices.FirstOrDefault(q => q.SelectedDevice as IDigitalMultimetr344xx != null)
                         .SelectedDevice as IDigitalMultimetr344xx;

            powerSupply = UserItemOperation.TestDevices.FirstOrDefault(q => q.SelectedDevice as E364xA != null)
                                           .SelectedDevice as E364xA;

            if (powerSupply == null || ElectonicLoad == null || digitalMult == null) return;
            if (_chanel == E364xChanels.OUTP2 && powerSupply.outputs.Length == 1) return;

            //шаг в 20% от номинала напряжения
            _voltRange = powerSupply.Ranges[(int) _voltRangeMode];

            var VoltSteps =
                _voltRange.GetArayMeasPointsInParcent(new MeasPoint<Voltage>(0), 0, 20, 40, 60, 80, 100);

            foreach (MeasPoint<Voltage> setPoint in VoltSteps)
            {
                ((IeeeBase) digitalMult).StringConnection = GetStringConnect(digitalMult);
                powerSupply.StringConnection = GetStringConnect(powerSupply);
                ((IeeeBase) ElectonicLoad).StringConnection = GetStringConnect((IProtocolStringLine) ElectonicLoad);

                var operation = new BasicOperationVerefication<MeasPoint<Voltage>>();
                operation.Expected = setPoint;
                operation.InitWork = async () =>
                {
                    try
                    {
                        powerSupply.ActiveE364XChanels = _chanel;
                        powerSupply.SetRange(_voltRangeMode);
                        operation.Comment = powerSupply.GetVoltageRange().Description;

                        operation.ErrorCalculation = (point, measPoint) =>
                        {
                            MeasPoint<Voltage> error;
                            if (_chanel == E364xChanels.OUTP2)
                                error = new MeasPoint<Voltage>(operation.Expected.MainPhysicalQuantity.Value * 0.001M +
                                                               0.025M);
                            else
                                error =
                                    new MeasPoint<Voltage>(operation.Expected.MainPhysicalQuantity.Value * 0.005M +
                                                           0.010M);

                            error.Round(4);
                            return error;
                        };

                        operation.LowerTolerance = operation.Expected - operation.Error;
                        operation.UpperTolerance = operation.Expected + operation.Error;

                        powerSupply.SetVoltageLevel(setPoint);
                        powerSupply.SetCurrentLevel(new MeasPoint<Current>(_voltRange.AdditionalPhysicalQuantity));

                        var resistToLoad =
                            new MeasPoint<Resistance>(_voltRange.MainPhysicalQuantity.GetNoramalizeValueToSi() /
                                                      _voltRange.AdditionalPhysicalQuantity.GetNoramalizeValueToSi());
                        resistToLoad.Round(3);

                        ElectonicLoad.SetThisModuleAsWorking();
                        ElectonicLoad.SetResistanceMode();
                        ElectonicLoad.SetResistanceLevel(resistToLoad);
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
                        powerSupply.OutputOn();
                        ElectonicLoad.OutputOn();
                        Thread.Sleep(1000);

                        digitalMult.DcVoltage.Range = operation.Expected;
                        digitalMult.DcVoltage.Setting();
                        digitalMult.DcVoltage.Getting();

                        var MeasVolts = digitalMult.DcVoltage.Value;
                        operation.Getting = MeasVolts;

                        powerSupply.OutputOff();
                        ElectonicLoad.OutputOff();

                        operation.IsGood = () =>
                        {
                            if (operation.Getting == null || operation.Expected == null ||
                                operation.UpperTolerance == null || operation.LowerTolerance == null) return false;
                            return (operation.Getting < operation.UpperTolerance) &
                                   (operation.Getting > operation.LowerTolerance);
                        };
                    }
                    catch (Exception e)
                    {
                        Logger.Error(e);
                        throw;
                    }
                };
                operation.CompliteWork = () =>
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
                };
                DataRow.Add(DataRow.IndexOf(operation) == -1
                                ? operation
                                : (BasicOperationVerefication<MeasPoint<Voltage>>) operation.Clone());
            }
        }

        #endregion
    }
}