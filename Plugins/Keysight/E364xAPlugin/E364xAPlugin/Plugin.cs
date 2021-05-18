using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using AP.Extension;
using ASMC.Common.ViewModel;
using ASMC.Core.Model;
using ASMC.Core.UI;
using ASMC.Data.Model;
using ASMC.Data.Model.Interface;
using ASMC.Data.Model.PhysicalQuantity;
using ASMC.Devices.IEEE;
using ASMC.Devices.IEEE.Keysight.ElectronicLoad;
using ASMC.Devices.IEEE.Keysight.Multimeter;
using ASMC.Devices.IEEE.Keysight.PowerSupplyes;
using DevExpress.Mvvm;
using DevExpress.Mvvm.UI;
using NLog;
using Current = ASMC.Data.Model.PhysicalQuantity.Current;

namespace E364xAPlugin
{
    public class Operation<T> : OperationMetrControlBase where T : E36xxA_DeviceBasicFunction
    {

        
        //определяет какие типы проверок доступны для СИ: поверка первичная/переодическая, калибровка, adjustment.
        public Operation(ServicePack servicePack)
        {
            //это операция первичной поверки
            UserItemOperationPrimaryVerf = new OpertionFirsVerf(servicePack, Activator.CreateInstance<T>());
            //здесь периодическая поверка, но набор операций такой же
            UserItemOperationPeriodicVerf = UserItemOperationPrimaryVerf;
            SpeedUserItemOperationPrimaryVerf = new SpeedOpertionFirsVerf(servicePack, Activator.CreateInstance<T>());
            SpeedUserItemOperationPeriodicVerf = new SpeedOpertionFirsVerf(servicePack, Activator.CreateInstance<T>());
            
        }
    }

    public class OpertionFirsVerf : Operation
    {
        public OpertionFirsVerf(ServicePack servicePack, E36xxA_DeviceBasicFunction inPower) : base(servicePack)
        {
            ControlDevices = new IDeviceUi[]
            {
                new Device
                {
                    Devices = new IDeviceRemote[] {new N3303A(), new N3306A()}, Description = "Электронная нагрузка"
                },
                new Device
                {
                    Devices = new IDeviceRemote[] {new Keysight34401A()}, Description = "Цифровой мультиметр"
                }
            };

            TestDevices = new IDeviceUi[]
            {
                new Device
                {
                    Devices = new IDeviceRemote[]
                    {
                        inPower
                    },
                    Description = "Мера напряжения и тока"
                }
            };
            DocumentName = "E364xA_protocol";

            UserItemOperation = new IUserItemOperationBase[]
            {
                new Oper1VisualTest(this),
                new Oper2Oprobovanie(this),

                #region outp1

                new UnstableVoltToLoadChange(this, E36xxChanels.OUTP1),
                new AcVoltChange(this, E36xxChanels.OUTP1),
                new UnstableVoltageOnTime(this, E36xxChanels.OUTP1),
                new OutputVoltageSetting(this, E36xxChanels.OUTP1),
                new OutputCurrentMeasure(this, E36xxChanels.OUTP1),
                new UnstableCurrentLoadChange(this, E36xxChanels.OUTP1),
                new UnstableCurrentToAcChange(this, E36xxChanels.OUTP1),
                new UnstableCurrentOnTime(this, E36xxChanels.OUTP1),
                new OutputCurrentSetup(this, E36xxChanels.OUTP1),
                new OutputVoltageMeasure(this, E36xxChanels.OUTP1),
                new VoltageTransientDuration(this, E36xxChanels.OUTP1),

                #endregion outp1

                #region outp2

                new UnstableVoltToLoadChange(this, E36xxChanels.OUTP2),
                new AcVoltChange(this, E36xxChanels.OUTP2),
                new UnstableVoltageOnTime(this, E36xxChanels.OUTP2),
                new OutputVoltageSetting(this, E36xxChanels.OUTP2),
                new OutputCurrentMeasure(this, E36xxChanels.OUTP2),
                new UnstableCurrentLoadChange(this, E36xxChanels.OUTP2),
                new UnstableCurrentToAcChange(this, E36xxChanels.OUTP2),
                new UnstableCurrentOnTime(this, E36xxChanels.OUTP2),
                new OutputCurrentSetup(this, E36xxChanels.OUTP2),
                new OutputVoltageMeasure(this, E36xxChanels.OUTP2),
                new VoltageTransientDuration(this, E36xxChanels.OUTP2),

                #endregion outp2
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

    public class SpeedOpertionFirsVerf : Operation
    {
        public SpeedOpertionFirsVerf(ServicePack servicePack, E36xxA_DeviceBasicFunction inPower) : base(servicePack)
        {
            ControlDevices = new IDeviceUi[]
            {
                new Device
                {
                    Devices = new IDeviceRemote[] {new N3303A(), new N3306A()}, Description = "Электронная нагрузка"
                },
                new Device
                {
                    Devices = new IDeviceRemote[] {new Keysight34401A()}, Description = "Цифровой мультиметр"
                }
            };

            TestDevices = new IDeviceUi[]
            {
                new Device
                {
                    Devices = new IDeviceRemote[]
                    {
                        inPower
                    },
                    Description = "Мера напряжения и тока"
                }
            };
            DocumentName = "E364xA_protocol";

            UserItemOperation = new IUserItemOperationBase[]
            {
                new Oper1VisualTest(this),
                new Oper2Oprobovanie(this),

                #region outp1

                new UnstableVoltToLoadChange(this, E36xxChanels.OUTP1),
                new AcVoltChange(this, E36xxChanels.OUTP1),
                new UnstableVoltageOnTime(this, E36xxChanels.OUTP1, true),
                new OutputVoltageSetting(this, E36xxChanels.OUTP1),
                new OutputCurrentMeasure(this, E36xxChanels.OUTP1),
                new UnstableCurrentLoadChange(this, E36xxChanels.OUTP1),
                new UnstableCurrentToAcChange(this, E36xxChanels.OUTP1),
                new UnstableCurrentOnTime(this, E36xxChanels.OUTP1, true),
                new OutputCurrentSetup(this, E36xxChanels.OUTP1),
                new OutputVoltageMeasure(this, E36xxChanels.OUTP1),
                new VoltageTransientDuration(this, E36xxChanels.OUTP1),

                #endregion outp1

                #region outp2

                new UnstableVoltToLoadChange(this, E36xxChanels.OUTP2),
                new AcVoltChange(this, E36xxChanels.OUTP2),
                new UnstableVoltageOnTime(this, E36xxChanels.OUTP2, true),
                new OutputVoltageSetting(this, E36xxChanels.OUTP2),
                new OutputCurrentMeasure(this, E36xxChanels.OUTP2),
                new UnstableCurrentLoadChange(this, E36xxChanels.OUTP2),
                new UnstableCurrentToAcChange(this, E36xxChanels.OUTP2),
                new UnstableCurrentOnTime(this, E36xxChanels.OUTP2, true),
                new OutputCurrentSetup(this, E36xxChanels.OUTP2),
                new OutputVoltageMeasure(this, E36xxChanels.OUTP2),
                new VoltageTransientDuration(this, E36xxChanels.OUTP2),

                #endregion outp2
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
                dataRow[0] = dds.Getting ? "Соответствует" : dds?.Comment;
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
            operation.InitWorkAsync = () =>
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

            operation.CompliteWorkAsync = () => { return Task.FromResult(true); };
            DataRow.Add(operation);
        }

        #endregion
    }

    public class Oper2Oprobovanie : ParagraphBase<bool>
    {
        #region Property

        protected E36xxA_DeviceBasicFunction powerSupply { get; set; }

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
                dataRow[0] = dds.Getting ? "Соответствует" : dds?.Comment;
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
            powerSupply = UserItemOperation.TestDevices.FirstOrDefault(q => q.SelectedDevice as E36xxA_DeviceBasicFunction != null)
                                           .SelectedDevice as E36xxA_DeviceBasicFunction;
            if (powerSupply == null) return;
            powerSupply.StringConnection = GetStringConnect(powerSupply);
            base.InitWork(token);

            var operation = new BasicOperation<bool>();
            operation.InitWorkAsync = async () =>
            {
                operation.Expected = true;
                operation.Getting = powerSupply.SelfTest();
            };

            operation.IsGood = () => operation.Getting;
            operation.CompliteWorkAsync = () => { return Task.FromResult(operation.IsGood()); };
            DataRow.Add(operation);
        }

        #endregion
    }

    public class UnstableVoltToLoadChange : BasePowerSupplyWithDigitMult<Voltage>
    {
        protected static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public UnstableVoltToLoadChange(IUserItemOperation userItemOperation, E36xxChanels inChanel) :
            base(userItemOperation, inChanel)
        {
            Name =
                $"Определение нестабильности выходного напряжения от изменения нагрузки (канал {_chanel})";
        }

        #region Methods

        public override void DefaultFillingRowTable(DataRow dataRow, BasicOperationVerefication<MeasPoint<Voltage>> dds)
        {
            dataRow[0] = dds?.Comment;
            dataRow[1] = dds?.Expected?.Description;
            dataRow[2] = dds?.Getting?.Description;
            var mp = dds?.Expected - dds?.Getting;
            dataRow[3] = mp.Description;
            dataRow[4] = dds?.LowerTolerance?.Description;
            dataRow[5] = dds?.UpperTolerance?.Description;
        }

        protected override MeasPoint<Voltage> ErrorCalc(MeasPoint<Voltage> inVal)
        {
            var resultError = new MeasPoint<Voltage>(0.0001M * inVal
                                                              .MainPhysicalQuantity
                                                              .GetNoramalizeValueToSi() +
                                                     0.003M);
            resultError.Round(4);
            return resultError;
        }

        protected override string[] GenerateDataColumnTypeObject()
        {
            return new[]
            {
                "Предел канала",
                "Измеренное напряжение U1",
                "Измеренное напряжение U2",
                "Разность U1 - U2",
                "Минимально допустимое значение",
                "Максимально допустимое значение"
            }.Concat(base.GenerateDataColumnTypeObject()).ToArray();
        }

        protected override void InitWork(CancellationTokenSource token)
        {
            base.InitWork(token);
            ConnectionToDevice();
            if (_chanel == E36xxChanels.OUTP2 && powerSupply.outputs.Length < 2) return;

            if (powerSupply == null || ElectonicLoad == null) return;

            foreach (E36xxA_Ranges rangePowerSupply in Enum.GetValues(typeof(E36xxA_Ranges)))
            {
                var operation = new BasicOperationVerefication<MeasPoint<Voltage>>();
                operation.InitWorkAsync = async () =>
                {
                    try
                    {
                        SetDevicesForVoltageMode(rangePowerSupply);
                    }
                    catch (Exception e)
                    {
                        Logger.Error(e);
                        throw;
                    }

                    operation.Comment = powerSupply.GetVoltageRange().Description;
                };
                operation.BodyWorkAsync = (cancellationToken) =>
                {
                    return Task.Factory.StartNew(() =>
                    {
                        try
                        {
                            powerSupply.OutputOn();

                            ElectonicLoad.OutputOn();
                            Thread.Sleep(3000);
                            digitalMult.DcVoltage.RangeStorage.IsAutoRange = true;
                            digitalMult.DcVoltage.Setting();
                            var U1 = digitalMult.DcVoltage.GetValue();
                            operation.Expected = U1;
                            operation.Expected.Round(4);
                            //разрываем цепь
                            ElectonicLoad.OutputOff();
                            Thread.Sleep(3000);
                            digitalMult.DcVoltage.RangeStorage.IsAutoRange = true;
                            digitalMult.DcVoltage.Setting();
                            var U2 = digitalMult.DcVoltage.GetValue();

                            powerSupply.OutputOff();
                            ElectonicLoad.OutputOff();

                            operation.Getting = U2;
                            operation.Getting.Round(4);

                            SetErrorCalculationUpperLowerCalcAndIsGood(operation);
                        }
                        catch (Exception e)
                        {
                            Logger.Error(e);
                            throw;
                        }
                        finally
                        {
                            powerSupply.OutputOff();
                            ElectonicLoad.OutputOff();
                        }
                    }, cancellationToken);
                
                };
                operation.CompliteWorkAsync = () =>
                {
                    if (operation.IsGood != null && !operation.IsGood())
                    {
                        var answer =
                            UserItemOperation.ServicePack.MessageBox()
                                             .Show($"Нестабильность напряжени не проходит по допуску на пределе {operation.Comment}:\n" +
                                                   $"{operation.Expected.Description} - {operation.Getting.Description} = {(operation.Getting - operation.Expected).Description}\n" +
                                                   $"Допустимое значение дрейфа: {operation.Error.Description}\n\n" +
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
                DataRow.Add(operation);
            }
        }

        #endregion
    }

    public class AcVoltChange : BasePowerSupplyWithDigitMult<Voltage>
    {
        protected static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public AcVoltChange(IUserItemOperation userItemOperation, E36xxChanels inChanel) :
            base(userItemOperation, inChanel)
        {
            Name =
                $"Определение нестабильности выходного напряжения от изменения напряжения питания  (канал {_chanel})";
        }

        #region Methods

        public override void DefaultFillingRowTable(DataRow dataRow, BasicOperationVerefication<MeasPoint<Voltage>> dds)
        {
            dataRow[0] = dds?.Comment;
            dataRow[1] = dds?.Expected?.Description;
            dataRow[2] = dds?.Getting?.Description;
            var mp = dds?.Expected - dds?.Getting;
            dataRow[3] = mp.Description;
            dataRow[4] = dds?.LowerTolerance?.Description;
            dataRow[5] = dds?.UpperTolerance?.Description;
        }

        protected override MeasPoint<Voltage> ErrorCalc(MeasPoint<Voltage> inVal)
        {
            var resultError =
                new MeasPoint<Voltage>(0.0001M * inVal.MainPhysicalQuantity.GetNoramalizeValueToSi() +
                                       0.003M);
            resultError.Round(4);
            return resultError;
        }

        protected override string[] GenerateDataColumnTypeObject()
        {
            return new[]
            {
                "Предел канала",
                "Измеренное напряжение U1",
                "Измеренное напряжение U2",
                "Разность U1 - U2",
                "Минимально допустимое значение",
                "Максимально допустимое значение"
            }.Concat(base.GenerateDataColumnTypeObject()).ToArray();
        }

        protected override void InitWork(CancellationTokenSource token)
        {
            base.InitWork(token);
            ConnectionToDevice();
            if (_chanel == E36xxChanels.OUTP2 && powerSupply.outputs.Length < 2) return;
            if (powerSupply == null || ElectonicLoad == null) return;

            foreach (E36xxA_Ranges rangePowerSupply in Enum.GetValues(typeof(E36xxA_Ranges)))
            {
                var operation = new BasicOperationVerefication<MeasPoint<Voltage>>();
                operation.InitWorkAsync = async () =>
                {
                    MeasPoint<Voltage> U1 = new MeasPoint<Voltage>();
                    try
                    {
                       
                        SetDevicesForVoltageMode(rangePowerSupply);
                        await Task.Run(() =>
                        {
                            powerSupply.OutputOn();
                            ElectonicLoad.OutputOn();
                        });

                        UserItemOperation.ServicePack.MessageBox()
                                         .Show("Установите напряжение питающей сети 220 В + 10 % (242 В).", "Внимание!", MessageBoxButton.OK);
                        Thread.Sleep(1000);
                        await Task.Run(() =>
                        {
                            
                            digitalMult.DcVoltage.RangeStorage.IsAutoRange = true;
                            digitalMult.DcVoltage.Setting();
                             U1 = digitalMult.DcVoltage.GetValue();
                        });
                        
                        UserItemOperation.ServicePack.MessageBox()
                                         .Show("Установите напряжение питающей сети 220 В - 10 % (198 В).", "Внимание!", MessageBoxButton.OK);
                        Thread.Sleep(1000);
                        await Task.Run(() =>
                        {
                            
                            digitalMult.DcVoltage.RangeStorage.IsAutoRange = true;
                            digitalMult.DcVoltage.Setting();
                            var U2 = digitalMult.DcVoltage.GetValue();

                            powerSupply.OutputOff();
                            ElectonicLoad.OutputOff();

                            operation.Expected = U1;
                            operation.Expected.Round(4);

                            operation.Getting = U2;
                            operation.Getting.Round(4);

                            

                        });
                        SetErrorCalculationUpperLowerCalcAndIsGood(operation);
                    }
                    catch (Exception e)
                    {
                        Logger.Error(e);
                        throw;
                    }
                    finally
                    {
                        powerSupply.OutputOff();
                        ElectonicLoad.OutputOff();
                    }

                    operation.Comment = powerSupply.GetVoltageRange().Description;
                };
                
                operation.CompliteWorkAsync = () =>
                {
                    if (operation.IsGood != null && !operation.IsGood())
                    {
                        var answer =
                            UserItemOperation.ServicePack.MessageBox()
                                             .Show($"Нестабильность выходного напряжения не проходит по допускуна пределе {operation.Comment}:\n" +
                                                   $"{operation.Expected.Description} - {operation.Getting.Description} = {(operation.Getting - operation.Expected).Description}\n" +
                                                   $"Допустимое значение нестабильности: {operation.Error.Description}\n\n" +
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
                DataRow.Add(operation);
            }
        }

        #endregion
    }

    public class VoltageTransientDuration : BasePowerSupplyProcedure<Time>
    {
        #region Property

        /// <summary>
        /// Предоставляет сервис окна ввода данных.
        /// </summary>
        private SelectionService Service { get; set; }

        #endregion

        public VoltageTransientDuration(IUserItemOperation userItemOperation, E36xxChanels inChanel) :
            base(userItemOperation, inChanel)
        {
            Name =
                $"Определение времени переходного процесса при изменении нагрузки (канал {_chanel})";
            Sheme = new SchemeImage
            {
                AssemblyLocalName = Assembly.GetExecutingAssembly().GetName().Name,
                Description = "Измерительная схема",
                Number = 5,
                FileName = "E364xA_DefaultSheme.jpg",
                FileNameDescription = "VoltageTransientDurationText.rtf"
            };
        }

        #region Methods

        public override void DefaultFillingRowTable(DataRow dataRow, BasicOperationVerefication<MeasPoint<Time>> dds)
        {
            dataRow["Номер измерения канала"] = dds?.Comment;
            dataRow["Значение длительности переходного процесса"] = dds.Getting?.Description;
            dataRow["Среднее значение времени переходного процесса"] = dds.Expected?.Description;
            dataRow["Максимально допустимое значение"] = dds?.Error?.Description;
        }

        protected override MeasPoint<Time> ErrorCalc(MeasPoint<Time> inVal)
        {
            throw new NotImplementedException();
        }

        protected override string[] GenerateDataColumnTypeObject()
        {
            return new[]
            {
                "Номер измерения канала",
                "Значение длительности переходного процесса",
                "Среднее значение времени переходного процесса",
                "Максимально допустимое значение"
            }.Concat(base.GenerateDataColumnTypeObject()).ToArray();
        }

        protected override void InitWork(CancellationTokenSource token)
        {
            base.InitWork(token);

            try
            {
                /*Получаем измерения*/

                var operation = new BasicOperationVerefication<MeasPoint<Time>>();
                var arrPoints = new[] {"1", "2", "3", "4", "5"};
                MeasPoint<Time>[] arrGetting = null;
                operation.InitWorkAsync = async () =>
                {
                    var setting = new TableViewModel.SettingTableViewModel {Breaking = 1, CellFormat = "мкс"};
                    var vm = new OneTableViewModel();

                    vm.Data = TableViewModel.CreateTable("Значения длительности переходного процесса", arrPoints,
                                                         setting);
                    Service = UserItemOperation.ServicePack.FreeWindow() as SelectionService;
                    Service.Title = Name;
                    Service.ViewLocator = new ViewLocator(vm.GetType().Assembly);
                    Service.ViewModel = vm;
                    Service.DocumentType = "OneTableView";
                    Service.Show();

                    arrGetting =
                        vm.Data.Cells.Select(cell => new MeasPoint<Time>(ObjectToDecimal(cell.Value),
                                                                         UnitMultiplier.Micro)).ToArray();
                };
                operation.BodyWorkAsync = (cancellationToken) =>
                {
                    return Task.Factory.StartNew(() =>
                    {
                        var averTime = new MeasPoint<Time>(0);
                        foreach (var point in arrGetting)
                        {
                            averTime = averTime + point;
                            averTime.MainPhysicalQuantity.ChangeMultiplier(point.MainPhysicalQuantity.Multiplier);
                        }

                        averTime = averTime / arrGetting.Length;
                        averTime.Round(2);

                        for (var i = 0; i < arrPoints.Length; i++)
                        {
                            if (i > 0) operation = (BasicOperationVerefication<MeasPoint<Time>>)operation.Clone();
                            operation.Comment = arrPoints[i];
                            operation.Expected = averTime;
                            operation.Getting = arrGetting[i];

                            if (i > 0) DataRow.Add(operation);
                        }
                    }, cancellationToken);
                   
                };
                operation.ErrorCalculation = (point, measPoint) => new MeasPoint<Time>(50, UnitMultiplier.Micro);
                operation.IsGood = () => operation.Expected < operation.Error;

                operation.CompliteWorkAsync = () => Task.FromResult(operation.IsGood());

                DataRow.Add(operation);
            }
            catch
            {
                token.Cancel();
            }
        }

        /// <summary>
        /// Приобразует строку в децимал.
        /// </summary>
        /// <param name = "obj"></param>
        /// <returns></returns>
        protected decimal ObjectToDecimal(object obj)
        {
            if (string.IsNullOrEmpty(obj.ToString())) return 0;

            return decimal.Parse(obj.ToString().Trim()
                                    .Replace(".",
                                             Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator));
        }

        #endregion
    }

    public class UnstableVoltageOnTime : BasePowerSupplyWithDigitMult<Voltage>
    {
        protected static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public UnstableVoltageOnTime(IUserItemOperation userItemOperation, E36xxChanels inChanel,
            bool isSpeedOperation = false) :
            base(userItemOperation, inChanel)
        {
            this.isSpeedOperation = isSpeedOperation;
            Name = $"Определение величины дрейфа выходного напряжения (канал {_chanel})";
        }

        #region Methods

        public override void DefaultFillingRowTable(DataRow dataRow, BasicOperationVerefication<MeasPoint<Voltage>> dds)
        {
            dataRow["Предел напряжения канала"] = dds?.Comment;
            dataRow["Измеренное значение напряжения"] = dds?.Expected?.Description;
            dataRow["Абсолютное отклонение напряжения"] = dds?.Getting?.Description;
            dataRow["Минимально допустимое значение"] = dds?.LowerTolerance?.Description;
            dataRow["Максимально допустимое значение"] = dds?.UpperTolerance?.Description;
        }

        protected override MeasPoint<Voltage> ErrorCalc(MeasPoint<Voltage> inVal)
        {
            var resultError = new MeasPoint<Voltage>(0.0002M * inVal
                                                              .MainPhysicalQuantity
                                                              .GetNoramalizeValueToSi() +
                                                     0.003M);
            resultError.Round(4);
            return resultError;
        }

        protected override string[] GenerateDataColumnTypeObject()
        {
            return new[]
            {
                "Предел напряжения канала",
                "Измеренное значение напряжения",
                "Абсолютное отклонение напряжения",
                "Минимально допустимое значение",
                "Максимально допустимое значение"
            }.Concat(base.GenerateDataColumnTypeObject()).ToArray();
        }

        protected override void InitWork(CancellationTokenSource token)
        {
            base.InitWork(token);
            ConnectionToDevice();
            if (_chanel == E36xxChanels.OUTP2 && powerSupply.outputs.Length < 2) return;
            if (powerSupply == null || ElectonicLoad == null) return;

            foreach (E36xxA_Ranges rangePowerSupply in Enum.GetValues(typeof(E36xxA_Ranges)))
            {
                MeasPoint<Voltage> U1 = null;
                //нужно произвести 17 измерений, 1-е измерение опорное, относительно него
                //дальше будут оцениваться последующие.
                for (var i = 0; i < 17; i++)
                {
                    var operation = new BasicOperationVerefication<MeasPoint<Voltage>>();
                    operation.InitWorkAsync = async () =>
                    {
                        try
                        {
                            SetDevicesForVoltageMode(rangePowerSupply);
                        }
                        catch (Exception e)
                        {
                            Logger.Error(e);
                            throw;
                        }

                        if (DataRow.IndexOf(operation) == 0 || DataRow.IndexOf(operation) == 17) Thread.Sleep(3000);

                        operation.Comment = powerSupply.GetVoltageRange().Description;
                    };
                    operation.BodyWorkAsync = (cancellationToken) =>
                    {
                        return Task.Factory.StartNew(() =>
                        {
                            try
                            {
                                powerSupply.OutputOn();
                                ElectonicLoad.OutputOn();

                                digitalMult.DcVoltage.RangeStorage.IsAutoRange = true;
                                digitalMult.DcVoltage.Setting();

                                if (DataRow.IndexOf(operation) == 0 || DataRow.IndexOf(operation) == 17)
                                {
                                    U1 = digitalMult.DcVoltage.GetValue();
                                    operation.Expected = U1;
                                    operation.Expected.Round(4);
                                    operation.Getting = U1 - U1;
                                }
                                else
                                {
                                    if (isSpeedOperation) //если выбран режим ПОВЕРКА - ускоренная поверка
                                        Thread.Sleep(15000);
                                    else
                                        Thread.Sleep(108000); //30 минут в нормальном режиме поверки
                                    digitalMult.DcVoltage.RangeStorage.IsAutoRange = true;
                                    digitalMult.DcVoltage.Setting();
                                    var Un = digitalMult.DcVoltage.GetValue();
                                    operation.Expected = Un;
                                    operation.Expected.Round(4);

                                    operation.Getting = U1 - Un;
                                    operation.Getting.Round(4);
                                }

                                operation.ErrorCalculation = (point, measPoint) =>
                                {
                                    var result = point - measPoint;
                                    result.Round(4);
                                    return result;
                                };
                                operation.UpperCalculation = expected => { return ErrorCalc(expected); };
                                operation.LowerCalculation = expected => ErrorCalc(expected) * -1;

                                powerSupply.OutputOff();
                                ElectonicLoad.OutputOff();
                            }
                            catch (Exception e)
                            {
                                Logger.Error(e);
                                throw;
                            }
                            finally
                            {
                                powerSupply.OutputOff();
                                ElectonicLoad.OutputOff();
                            }

                        }, cancellationToken);
                   
                    };
                    operation.IsGood = () =>
                    {
                        if (operation.Getting == null || operation.Expected == null ||
                            operation.UpperTolerance == null || operation.LowerTolerance == null) return false;
                        // в Expected записано измеренное напряжение, а погрешность посчитана для первого напряжения
                        // и края посчитаны тоже для первого напряжения.
                        // Фактически первое измеренное напряжение определяет границы допуска.
                        //поэтому ниже сравнивается Getting с краями допуска
                        return (operation.Getting < operation.UpperTolerance) &
                               (operation.Getting > operation.LowerTolerance);
                    };
                    operation.CompliteWorkAsync = () =>
                    {
                        if (operation.IsGood != null && !operation.IsGood())
                        {
                            var answer =
                                UserItemOperation.ServicePack.MessageBox()
                                                 .Show("Величина дрейфа напряжения не проходит по допуску:\n" +
                                                       $"Для точки {operation.Expected.Description} дрейф составил {operation.Getting.Description}\n" +
                                                       $"Допустимое значение погрешности {operation.Error.Description}\n" +
                                                       $"Допустимым значение напряжения в данном случае считается от {operation.LowerTolerance.Description} до {operation.UpperTolerance.Description}\n" +
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
                    DataRow.Add(operation);
                }
            }
        }

        #endregion
    }

    public class OutputVoltageSetting : BasePowerSupplyWithDigitMult<Voltage>
    {
        protected static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public OutputVoltageSetting(IUserItemOperation userItemOperation, E36xxChanels inChanel) :
            base(userItemOperation, inChanel)
        {
            Name =
                $"Определение погрешности установки выходного напряжения в режиме постоянного напряжения (канал {_chanel})";
        }

        #region Methods

        public override void DefaultFillingRowTable(DataRow dataRow, BasicOperationVerefication<MeasPoint<Voltage>> dds)
        {
            dataRow["Предел напряжения канала"] = dds?.Comment;
            dataRow["Поверяемая точка"] = dds?.Expected?.Description;
            dataRow["Измеренное значение"] = dds.Getting?.Description;
            dataRow["Минимально допустимое значение"] = dds?.LowerTolerance?.Description;
            dataRow["Максимально допустимое значение"] = dds?.UpperTolerance?.Description;
        }

        protected override MeasPoint<Voltage> ErrorCalc(MeasPoint<Voltage> inVal)
        {
            MeasPoint<Voltage> error;
            if (_chanel == E36xxChanels.OUTP2)
                error = new MeasPoint<Voltage>(inVal.MainPhysicalQuantity.Value *
                                               0.001M +
                                               0.025M);
            else
                error =
                    new MeasPoint<Voltage>(inVal.MainPhysicalQuantity.Value * 0.005M +
                                           0.010M);

            error.Round(4);
            return error;
        }

        protected override string[] GenerateDataColumnTypeObject()
        {
            return new[]
            {
                "Предел напряжения канала",
                "Поверяемая точка",
                "Измеренное значение",
                "Минимально допустимое значение",
                "Максимально допустимое значение"
            }.Concat(base.GenerateDataColumnTypeObject()).ToArray();
        }

        protected override void InitWork(CancellationTokenSource token)
        {
            base.InitWork(token);
            ConnectionToDevice();
            if (_chanel == E36xxChanels.OUTP2 && powerSupply.outputs.Length < 2) return;

            if (powerSupply == null || ElectonicLoad == null || digitalMult == null) return;
            if (_chanel == E36xxChanels.OUTP2 && powerSupply.outputs.Length == 1) return;

            foreach (E36xxA_Ranges rangePowerSupply in Enum.GetValues(typeof(E36xxA_Ranges)))
            {
                var _voltRange = powerSupply.Ranges[(int) rangePowerSupply];
                var VoltSteps =
                    _voltRange.GetArayMeasPointsInParcent(new MeasPoint<Voltage>(0), 0, 20, 40, 60, 80, 100);

                foreach (MeasPoint<Voltage> setPoint in VoltSteps)
                {
                    var operation = new BasicOperationVerefication<MeasPoint<Voltage>>();
                    operation.Expected = setPoint;
                    operation.InitWorkAsync = async () =>
                    {
                        try
                        {
                            SetDevicesForVoltageMode(rangePowerSupply);
                            powerSupply.VOLT.SetValue(setPoint);
                        }
                        catch (Exception e)
                        {
                            Logger.Error(e);
                            throw;
                        }

                        operation.Comment = powerSupply.GetVoltageRange().Description;
                    };
                    operation.BodyWorkAsync = (cancellationToken) =>
                    {
                        return Task.Factory.StartNew(() =>
                        {
                            try
                            {
                                powerSupply.OutputOn();
                                ElectonicLoad.OutputOn();
                                Thread.Sleep(1000);

                                digitalMult.DcVoltage.RangeStorage.IsAutoRange = true;
                                digitalMult.DcVoltage.Setting();
                                var MeasVolts = digitalMult.DcVoltage.GetValue();
                                operation.Getting = MeasVolts;
                                operation.Getting.Round(4);

                                powerSupply.OutputOff();
                                ElectonicLoad.OutputOff();

                                SetDefaultErrorCalculationUpperLowerCalcAndIsGood(operation);
                            }
                            catch (Exception e)
                            {
                                Logger.Error(e);
                                throw;
                            }
                            finally
                            {
                                powerSupply.OutputOff();
                                ElectonicLoad.OutputOff();
                            }
                        }, cancellationToken);
                  
                    };
                    operation.CompliteWorkAsync = () =>
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
                    DataRow.Add(operation);
                }
            }
        }

        #endregion
    }

    public class OutputCurrentMeasure : BasePowerSupplyProcedure<Current>
    {
        protected static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public OutputCurrentMeasure(IUserItemOperation userItemOperation, E36xxChanels inChanel) :
            base(userItemOperation, inChanel)
        {
            Name =
                $"Определение погрешности измерения выходного тока (канал {_chanel})";
        }

        #region Methods

        public override void DefaultFillingRowTable(DataRow dataRow, BasicOperationVerefication<MeasPoint<Current>> dds)
        {
            dataRow["Предел напряжения канала"] = dds?.Comment;
            dataRow["Поверяемая точка"] = dds?.Expected?.Description;
            dataRow["Измеренное значение"] = dds.Getting?.Description;
            dataRow["Минимально допустимое значение"] = dds?.LowerTolerance?.Description;
            dataRow["Максимально допустимое значение"] = dds?.UpperTolerance?.Description;
        }

        protected override MeasPoint<Current> ErrorCalc(MeasPoint<Current> inVal)
        {
            MeasPoint<Current> error;
            if (_chanel == E36xxChanels.OUTP2)
                error = new MeasPoint<Current>(inVal.MainPhysicalQuantity.Value *
                                               0.0015M +
                                               0.010M);
            else
                error =
                    new MeasPoint<Current>(inVal.MainPhysicalQuantity.Value * 0.005M +
                                           0.005M);

            error.Round(4);
            return error;
        }

        protected override string[] GenerateDataColumnTypeObject()
        {
            return new[]
            {
                "Предел напряжения канала",
                "Поверяемая точка",
                "Измеренное значение",
                "Минимально допустимое значение",
                "Максимально допустимое значение"
            }.Concat(base.GenerateDataColumnTypeObject()).ToArray();
        }

        protected override void InitWork(CancellationTokenSource token)
        {
            base.InitWork(token);
            ConnectionToDevice();
            if (_chanel == E36xxChanels.OUTP2 && powerSupply.outputs.Length < 2) return;

            if (powerSupply == null || ElectonicLoad == null) return;
            if (_chanel == E36xxChanels.OUTP2 && powerSupply.outputs.Length == 1) return;

            foreach (E36xxA_Ranges rangePowerSupply in Enum.GetValues(typeof(E36xxA_Ranges)))
            {
                var _voltRange = powerSupply.Ranges[(int) rangePowerSupply];
                var VoltSteps =
                    _voltRange.GetArayMeasPointsInParcent(new MeasPoint<Voltage>(0), 100, 80, 60, 40, 20, 0);

                foreach (MeasPoint<Voltage> setPoint in VoltSteps)
                {
                    var operation = new BasicOperationVerefication<MeasPoint<Current>>();
                    operation.InitWorkAsync = async () =>
                    {
                        try
                        {
                            SetDevicesForVoltageMode(rangePowerSupply);
                        }
                        catch (Exception e)
                        {
                            Logger.Error(e);
                            throw;
                        }

                        operation.Comment =
                            $"Предел {powerSupply.GetVoltageRange().Description}, напряжение выхода {setPoint.Description}";
                    };
                    operation.BodyWorkAsync = (cancellationToken) =>
                    {
                        return Task.Factory.StartNew(() =>
                        {
                            try
                            {
                                powerSupply.OutputOn();
                                ElectonicLoad.OutputOn();
                                Thread.Sleep(1000);

                                var StdCurr = ElectonicLoad.GetMeasureCurrent();
                                operation.Expected = StdCurr;
                                operation.Expected.Round(4);
                                var MeasPowerSupplyCurr = powerSupply.MEAS.GetMeasureCurrent();
                                operation.Getting = MeasPowerSupplyCurr;
                                operation.Getting.Round(4);

                                powerSupply.OutputOff();
                                ElectonicLoad.OutputOff();

                                SetDefaultErrorCalculationUpperLowerCalcAndIsGood(operation);
                            }
                            catch (Exception e)
                            {
                                Logger.Error(e);
                                throw;
                            }
                            finally
                            {
                                powerSupply.OutputOff();
                                ElectonicLoad.OutputOff();
                            }
                        }, cancellationToken);
                       
                    };
                    operation.CompliteWorkAsync = () =>
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
                    DataRow.Add(operation);
                }
            }
        }

        #endregion
    }

    public class UnstableCurrentLoadChange : BasePowerSupplyProcedure<Current>
    {
        protected static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public UnstableCurrentLoadChange(IUserItemOperation userItemOperation, E36xxChanels inChanel) :
            base(userItemOperation, inChanel)
        {
            Name =
                $"Определение нестабильности выходного тока от изменения нагрузки (канал {_chanel})";
        }

        #region Methods

        public override void DefaultFillingRowTable(DataRow dataRow, BasicOperationVerefication<MeasPoint<Current>> dds)
        {
            dataRow["Предел воспроизведения напряжения"] = dds?.Comment;
            dataRow["Измеренный ток I1"] = dds?.Expected?.Description;
            dataRow["Измеренный ток I2"] = dds?.Getting?.Description;
            var point = dds?.Expected - dds?.Getting;
            dataRow["Разность I1 - I2 (отклонение)"] = point.Description;
            dataRow["Минимально допустимое значение"] = dds?.LowerTolerance?.Description;
            dataRow["Максимально допустимое значение"] = dds?.UpperTolerance?.Description;
        }

        protected override MeasPoint<Current> ErrorCalc(MeasPoint<Current> inVal)
        {
            var resultError = new MeasPoint<Current>(0.0001M * inVal
                                                              .MainPhysicalQuantity
                                                              .GetNoramalizeValueToSi() +
                                                     0.000250M);
            resultError.Round(4);

            return resultError;
        }

        protected override string[] GenerateDataColumnTypeObject()
        {
            return new[]
            {
                "Предел воспроизведения напряжения",
                "Измеренный ток I1",
                "Измеренный ток I2",
                "Разность I1 - I2 (отклонение)",
                "Минимально допустимое значение",
                "Максимально допустимое значение"
            }.Concat(base.GenerateDataColumnTypeObject()).ToArray();
        }

        protected override void InitWork(CancellationTokenSource token)
        {
            base.InitWork(token);
            ConnectionToDevice();
            if (_chanel == E36xxChanels.OUTP2 && powerSupply.outputs.Length < 2) return;

            if (powerSupply == null || ElectonicLoad == null) return;

            foreach (E36xxA_Ranges rangePowerSupply in Enum.GetValues(typeof(E36xxA_Ranges)))
            {
                var operation = new BasicOperationVerefication<MeasPoint<Current>>();
                operation.InitWorkAsync = async () =>
                {
                    try
                    {
                        SetDevicesForCurrentMode(operation, rangePowerSupply);
                    }
                    catch (Exception e)
                    {
                        Logger.Error(e);
                        throw;
                    }

                    operation.Comment = powerSupply.Ranges[(int) rangePowerSupply].Description;
                };
                operation.BodyWorkAsync = (cancellationToken) =>
                {
                    return Task.Factory.StartNew(() =>
                    {
                        try
                        {
                            powerSupply.OutputOn();

                            ElectonicLoad.OutputOn();
                            Thread.Sleep(3000);

                            var i1 = ElectonicLoad.GetMeasureCurrent();
                            operation.Expected = i1;
                            operation.Expected.Round(4);
                            //подключаем второе сопротивление т.е. уменьшаем текущее в 2 раза
                            var resistance = ElectonicLoad.GetResistnceLevel();
                            ElectonicLoad.SetResistanceLevel(resistance / 2);
                            Thread.Sleep(1000);
                            var i2 = ElectonicLoad.GetMeasureCurrent();
                            operation.Getting = i2;
                            operation.Getting.Round(4);

                            powerSupply.OutputOff();
                            ElectonicLoad.OutputOff();

                            SetErrorCalculationUpperLowerCalcAndIsGood(operation);
                        }
                        catch (Exception e)
                        {
                            Logger.Error(e);
                            throw;
                        }
                        finally
                        {
                            powerSupply.OutputOff();
                            ElectonicLoad.OutputOff();
                        }
                    }, cancellationToken);
                
                };
                operation.CompliteWorkAsync = () =>
                {
                    if (operation.IsGood != null && !operation.IsGood())
                    {
                        var answer =
                            UserItemOperation.ServicePack.MessageBox()
                                             .Show("Нестабильности выходного тока не проходит по допуску:\n" +
                                                   $"{operation.Expected.Description} - {operation.Getting.Description} = {(operation.Getting - operation.Expected).Description}\n" +
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
                DataRow.Add(operation);
            }
        }

        #endregion
    }

    public class UnstableCurrentToAcChange : BasePowerSupplyProcedure<Current>
    {
        protected static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public UnstableCurrentToAcChange(IUserItemOperation userItemOperation, E36xxChanels inChanel) :
            base(userItemOperation, inChanel)
        {
            Name =
                $"Определение нестабильности выходного тока от изменения напряжения питания (канал {_chanel})";
        }

        #region Methods

        public override void DefaultFillingRowTable(DataRow dataRow, BasicOperationVerefication<MeasPoint<Current>> dds)
        {
            dataRow["Предел воспроизведения напряжения"] = dds?.Comment;
            dataRow["Измеренный ток I1"] = dds?.Expected?.Description;
            dataRow["Измеренный ток I2"] = dds?.Getting?.Description;
            var point = dds?.Expected - dds?.Getting;
            dataRow["Разность I1 - I2 (отклонение)"] = point.Description;
            dataRow["Минимально допустимое значение"] = dds?.LowerTolerance?.Description;
            dataRow["Максимально допустимое значение"] = dds?.UpperTolerance?.Description;
        }

        protected override MeasPoint<Current> ErrorCalc(MeasPoint<Current> inVal)
        {
            var resultError = new MeasPoint<Current>(0.0001M * inVal
                                                              .MainPhysicalQuantity
                                                              .GetNoramalizeValueToSi() +
                                                     0.000250M);
            resultError.Round(4);

            return resultError;
        }

        protected override string[] GenerateDataColumnTypeObject()
        {
            return new[]
            {
                "Предел воспроизведения напряжения",
                "Измеренный ток I1",
                "Измеренный ток I2",
                "Разность I1 - I2 (отклонение)",
                "Минимально допустимое значение",
                "Максимально допустимое значение"
            }.Concat(base.GenerateDataColumnTypeObject()).ToArray();
        }

        protected override void InitWork(CancellationTokenSource token)
        {
            base.InitWork(token);
            ConnectionToDevice();
            if (_chanel == E36xxChanels.OUTP2 && powerSupply.outputs.Length < 2) return;

            if (powerSupply == null || ElectonicLoad == null) return;

            foreach (E36xxA_Ranges rangePowerSupply in Enum.GetValues(typeof(E36xxA_Ranges)))
            {
                var operation = new BasicOperationVerefication<MeasPoint<Current>>();
                operation.InitWorkAsync = async () =>
                {
                    MeasPoint<Current> i1 = new MeasPoint<Current>();
                    try
                    {
                        SetDevicesForCurrentMode(operation, rangePowerSupply);
                        await Task.Run(() =>
                        {
                            powerSupply.OutputOn();
                            ElectonicLoad.OutputOn();
                            });
                        Thread.Sleep(3000);
                        UserItemOperation.ServicePack.MessageBox()
                                         .Show("Установите напряжение питающей сети 220 В + 10 % (242 В).", "Внимание!", MessageBoxButton.OK);

                        await Task.Run(() =>
                        {
                            i1 = ElectonicLoad.GetMeasureCurrent();
                            operation.Expected = i1;
                            operation.Expected.Round(4);
                        });

                        UserItemOperation.ServicePack.MessageBox()
                                         .Show("Установите напряжение питающей сети 220 В - 10 % (198 В).", "Внимание!", MessageBoxButton.OK);
                        Thread.Sleep(1000);

                        await Task.Run(() =>
                        {
                            var i2 = ElectonicLoad.GetMeasureCurrent();
                            powerSupply.OutputOff();
                            ElectonicLoad.OutputOff();

                            i2.Round(4);
                            operation.Getting = i2;
                            operation.Getting.Round(4);

                            
                        });
                        SetErrorCalculationUpperLowerCalcAndIsGood(operation);
                    }
                    catch (Exception e)
                    {
                        Logger.Error(e);
                        throw;
                    }
                    finally
                    {
                        powerSupply.OutputOff();
                        ElectonicLoad.OutputOff();
                    }
                };
                
                operation.CompliteWorkAsync = () =>
                {
                    if (operation.IsGood != null && !operation.IsGood())
                    {
                        var answer =
                            UserItemOperation.ServicePack.MessageBox()
                                             .Show("Нестабильности выходного тока не проходит по допуску:\n" +
                                                   $"{operation.Expected.Description} - {operation.Getting.Description} = {(operation.Getting - operation.Expected).Description}\n" +
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
                DataRow.Add(operation);
            }
        }

        #endregion
    }

    public class UnstableCurrentOnTime : BasePowerSupplyProcedure<Current>
    {
        protected static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public UnstableCurrentOnTime(IUserItemOperation userItemOperation, E36xxChanels inChanel,
            bool isSpeedOperation = false) :
            base(userItemOperation, inChanel)
        {
            this.isSpeedOperation = isSpeedOperation;
            Name = $"Определение величины дрейфа выходного тока (канал {_chanel})";
        }

        #region Methods

        public override void DefaultFillingRowTable(DataRow dataRow, BasicOperationVerefication<MeasPoint<Current>> dds)
        {
            dataRow["Предел напряжения канала"] = dds?.Comment;
            dataRow["Измеренное значение тока"] = dds?.Expected?.Description;
            dataRow["Абсолютное отклонение тока"] = dds?.Getting?.Description;
            dataRow["Минимально допустимое значение"] = dds?.LowerTolerance?.Description;
            dataRow["Максимально допустимое значение"] = dds?.UpperTolerance?.Description;
        }

        protected override MeasPoint<Current> ErrorCalc(MeasPoint<Current> inVal)
        {
            var resultError = new MeasPoint<Current>(0.001M * inVal
                                                             .MainPhysicalQuantity
                                                             .GetNoramalizeValueToSi() +
                                                     0.001M);
            resultError.Round(4);
            return resultError;
        }

        protected override string[] GenerateDataColumnTypeObject()
        {
            return new[]
            {
                "Предел напряжения канала",
                "Измеренное значение тока",
                "Абсолютное отклонение тока",
                "Минимально допустимое значение",
                "Максимально допустимое значение"
            }.Concat(base.GenerateDataColumnTypeObject()).ToArray();
        }

        protected override void InitWork(CancellationTokenSource token)
        {
            base.InitWork(token);
            ConnectionToDevice();
            if (_chanel == E36xxChanels.OUTP2 && powerSupply.outputs.Length < 2) return;
            if (powerSupply == null || ElectonicLoad == null) return;

            foreach (E36xxA_Ranges rangePowerSupply in Enum.GetValues(typeof(E36xxA_Ranges)))
            {
                MeasPoint<Current> I1 = null;
                for (var i = 0; i < 17; i++)
                {
                    var operation = new BasicOperationVerefication<MeasPoint<Current>>();
                    operation.InitWorkAsync = async () =>
                    {
                        try
                        {
                            SetDevicesForCurrentMode(operation, rangePowerSupply);
                        }
                        catch (Exception e)
                        {
                            Logger.Error(e);
                            throw;
                        }
                    };
                    operation.BodyWorkAsync = (cancellationToken) =>
                    {
                        return Task.Factory.StartNew(() =>
                        {
                            try
                            {
                                powerSupply.OutputOn();
                                ElectonicLoad.OutputOn();

                                if (isSpeedOperation) //если выбран режим ПОВЕРКА - ускоренная поверка
                                    Thread.Sleep(15000);
                                else
                                    Thread.Sleep(108000); // нормальная поверка по МП, между измерениями 30 минут

                                if (DataRow.IndexOf(operation) == 0 || DataRow.IndexOf(operation) == 17)
                                {
                                    I1 = ElectonicLoad.GetMeasureCurrent();
                                    operation.Expected = I1;
                                    operation.Expected.Round(4);
                                    operation.Getting = I1 - I1;
                                }
                                else
                                {
                                    var In = ElectonicLoad.GetMeasureCurrent();
                                    operation.Expected = In;
                                    operation.Expected.Round(4);

                                    operation.Getting = I1 - In;
                                    operation.Getting.Round(4);
                                }

                                operation.ErrorCalculation = (point, measPoint) =>
                                {
                                    var result = point - measPoint;
                                    result.Round(4);
                                    return result;
                                };
                            }
                            catch (Exception e)
                            {
                                Logger.Error(e);
                                throw;
                            }
                            finally
                            {
                                powerSupply.OutputOff();
                                ElectonicLoad.OutputOff();
                            }

                            operation.UpperCalculation = expected => { return ErrorCalc(expected); };
                            operation.LowerCalculation = expected => ErrorCalc(expected) * -1;

                            operation.IsGood = () =>
                            {
                                if (operation.Getting == null || operation.Expected == null ||
                                    operation.UpperTolerance == null || operation.LowerTolerance == null) return false;
                                // в Expected записан измеренный ток,а погрешность посчитана для первого тока
                                // и края посчитаны тоже для первого тока.
                                // Фактически первый измеренный ток определяет границы допуска.
                                // В Getting записана разность первого тока и текущего измеренного.
                                //поэтому ниже сравнивается Getting с краями допуска
                                return (operation.Getting < operation.UpperTolerance) &
                                       (operation.Getting > operation.LowerTolerance);
                            };
                        }, cancellationToken);
                     
                    };
                    operation.CompliteWorkAsync = () =>
                    {
                        if (operation.IsGood != null && !operation.IsGood())
                        {
                            var answer =
                                UserItemOperation.ServicePack.MessageBox()
                                                 .Show("Величины дрейфа выходного тока не проходит по допуску:\n" +
                                                       $"Допустимое значение погрешности {operation.Error.Description}\n" +
                                                       $"ИЗМЕРЕННОЕ значение {operation.Getting.Description}\n\n" +
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
                    DataRow.Add(operation);
                }
            }
        }

        #endregion
    }

    public class OutputCurrentSetup : BasePowerSupplyProcedure<Current>
    {
        protected static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public OutputCurrentSetup(IUserItemOperation userItemOperation, E36xxChanels inChanel) :
            base(userItemOperation, inChanel)
        {
            Name =
                $"Определение погрешности установки тока в режиме постоянного тока (канал {_chanel})";
        }

        #region Methods

        public override void DefaultFillingRowTable(DataRow dataRow, BasicOperationVerefication<MeasPoint<Current>> dds)
        {
            dataRow["Предел воспроизведения напряжения"] = dds?.Comment;
            dataRow["Поверяемая точка"] = dds?.Expected?.Description;
            dataRow["Измеренное значение"] = dds.Getting?.Description;
            dataRow["Минимально допустимое значение"] = dds?.LowerTolerance?.Description;
            dataRow["Максимально допустимое значение"] = dds?.UpperTolerance?.Description;
        }

        protected override MeasPoint<Current> ErrorCalc(MeasPoint<Current> inVal)
        {
            var error =
                new MeasPoint<Current>(inVal.MainPhysicalQuantity.Value * 0.002M +
                                       0.010M);

            error.Round(4);
            return error;
        }

        protected override string[] GenerateDataColumnTypeObject()
        {
            return new[]
            {
                "Предел воспроизведения напряжения",
                "Поверяемая точка",
                "Измеренное значение",
                "Минимально допустимое значение",
                "Максимально допустимое значение"
            }.Concat(base.GenerateDataColumnTypeObject()).ToArray();
        }

        protected override void InitWork(CancellationTokenSource token)
        {
            base.InitWork(token);
            ConnectionToDevice();
            if (_chanel == E36xxChanels.OUTP2 && powerSupply.outputs.Length < 2) return;

            if (powerSupply == null || ElectonicLoad == null) return;
            if (_chanel == E36xxChanels.OUTP2 && powerSupply.outputs.Length == 1) return;

            foreach (E36xxA_Ranges rangePowerSupply in Enum.GetValues(typeof(E36xxA_Ranges)))
            {
                var _voltRange = powerSupply.Ranges[(int) rangePowerSupply];
                var CurrentLimit = new MeasPoint<Current>(_voltRange.AdditionalPhysicalQuantity);
                var CurrSteps =
                    CurrentLimit.GetArayMeasPointsInParcent(new MeasPoint<Current>(0), 0, 20, 40, 60, 80, 100);

                foreach (var measPoint1 in CurrSteps)
                {
                    var setPoint = (MeasPoint<Current>) measPoint1;

                    var operation = new BasicOperationVerefication<MeasPoint<Current>>();

                    operation.InitWorkAsync = async () =>
                    {
                        try
                        {
                            SetDevicesForCurrentMode(operation, rangePowerSupply);
                            powerSupply.CURR.SetValue((MeasPoint<Current>)measPoint1);
                        }
                        catch (Exception e)
                        {
                            Logger.Error(e);
                            throw;
                        }
                    };
                    operation.BodyWorkAsync = (cancellationToken) =>
                    {
                        return Task.Factory.StartNew(() =>
                        {
                            try
                            {
                                operation.Expected = (MeasPoint<Current>)measPoint1;
                                powerSupply.OutputOn();
                                ElectonicLoad.OutputOn();
                                Thread.Sleep(1000);
                                var MeasStdCurr = ElectonicLoad.GetMeasureCurrent();
                                operation.Getting = MeasStdCurr;
                                operation.Getting.Round(4);

                                powerSupply.OutputOff();
                                ElectonicLoad.OutputOff();

                                SetDefaultErrorCalculationUpperLowerCalcAndIsGood(operation);
                            }
                            catch (Exception e)
                            {
                                Logger.Error(e);
                                throw;
                            }
                            finally
                            {
                                powerSupply.OutputOff();
                                ElectonicLoad.OutputOff();
                            }
                        }, cancellationToken);
                       
                    };
                    operation.CompliteWorkAsync = () =>
                    {
                        if (operation.IsGood != null && !operation.IsGood())
                        {
                            var answer =
                                UserItemOperation.ServicePack.MessageBox()
                                                 .Show($"Текущая точка {operation.Getting.Description} не проходит по допуску:\n" +
                                                       $"Минимально допустимое значение {operation.LowerTolerance.Description}\n" +
                                                       $"Максимально допустимое значение {operation.UpperTolerance.Description}\n" +
                                                       
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
                    DataRow.Add(operation);
                }
            }
        }

        #endregion
    }

    public class OutputVoltageMeasure : BasePowerSupplyWithDigitMult<Voltage>
    {
        /*
         *Здесь производится измерение напряжения в режиме стабилизации ТОКА
         *изменяется уставка по току, а контролируется напряжения внешним вольтметром и всттроенным вольтметром источника питания
         */

        protected static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public OutputVoltageMeasure(IUserItemOperation userItemOperation, E36xxChanels inChanel) :
            base(userItemOperation, inChanel)
        {
            Name =
                $"Определение погрешности измерения выходного напряжения (канал {_chanel})";
        }

        #region Methods

        public override void DefaultFillingRowTable(DataRow dataRow, BasicOperationVerefication<MeasPoint<Voltage>> dds)
        {
            dataRow["Предел воспроизведения напряжения"] = dds?.Comment;
            dataRow["Поверяемая точка"] = dds?.Expected?.Description;
            dataRow["Измеренное значение"] = dds.Getting?.Description;
            dataRow["Минимально допустимое значение"] = dds?.LowerTolerance?.Description;
            dataRow["Максимально допустимое значение"] = dds?.UpperTolerance?.Description;
        }

        protected override MeasPoint<Voltage> ErrorCalc(MeasPoint<Voltage> inVal)
        {
            MeasPoint<Voltage> error;
            if (_chanel == E36xxChanels.OUTP2)
                error = new MeasPoint<Voltage>(inVal.MainPhysicalQuantity.Value *
                                               0.001M +
                                               0.025M);
            else
                error =
                    new MeasPoint<Voltage>(inVal.MainPhysicalQuantity.Value * 0.005M +
                                           0.010M);

            error.Round(4);
            return error;
        }

        protected override string[] GenerateDataColumnTypeObject()
        {
            return new[]
            {
                "Предел воспроизведения напряжения",
                "Поверяемая точка",
                "Измеренное значение",
                "Минимально допустимое значение",
                "Максимально допустимое значение"
            }.Concat(base.GenerateDataColumnTypeObject()).ToArray();
        }

        protected override void InitWork(CancellationTokenSource token)
        {
            base.InitWork(token);
            ConnectionToDevice();
            if (_chanel == E36xxChanels.OUTP2 && powerSupply.outputs.Length < 2) return;

            if (powerSupply == null || ElectonicLoad == null || digitalMult == null) return;
            if (_chanel == E36xxChanels.OUTP2 && powerSupply.outputs.Length == 1) return;

            foreach (E36xxA_Ranges rangePowerSupply in Enum.GetValues(typeof(E36xxA_Ranges)))
            {
                var _voltRange = powerSupply.Ranges[(int) rangePowerSupply];
                var VoltSteps =
                    _voltRange.GetArayMeasPointsInParcent(new MeasPoint<Voltage>(0), 0, 20, 40, 60, 80, 100);

                foreach (MeasPoint<Voltage> setPoint in VoltSteps)
                {
                    var operation = new BasicOperationVerefication<MeasPoint<Voltage>>();
                    operation.Expected = setPoint;
                    operation.InitWorkAsync = async () =>
                    {
                        try
                        {
                            SetDevicesForCurrentMode(operation, rangePowerSupply);
                            powerSupply.VOLT.SetValue(setPoint);
                            var current = powerSupply.CURR.GetValue();
                            var resist = new MeasPoint<Resistance>(0.85M* setPoint.MainPhysicalQuantity.GetNoramalizeValueToSi()/
                                                                   current.MainPhysicalQuantity.GetNoramalizeValueToSi());
                            ElectonicLoad.SetResistanceLevel(resist);
                            
                        }
                        catch (Exception e)
                        {
                            Logger.Error(e);
                            throw;
                        }
                        
                    };
                    operation.BodyWorkAsync = (cancellationToken) =>
                    {
                        return Task.Factory.StartNew(() =>
                        {
                            try
                            {
                                powerSupply.OutputOn();
                                ElectonicLoad.OutputOn();
                                Thread.Sleep(2000);

                                digitalMult.DcVoltage.RangeStorage.IsAutoRange = true;
                                digitalMult.DcVoltage.Setting();
                                var stdVolt = digitalMult.DcVoltage.GetValue();
                                operation.Expected = stdVolt;
                                operation.Expected.Round(4);

                                var measVolt = powerSupply.MEAS.GetMeasureVoltage();
                                operation.Getting = measVolt;
                                operation.Getting.Round(4);


                                powerSupply.OutputOff();
                                ElectonicLoad.OutputOff();

                                SetDefaultErrorCalculationUpperLowerCalcAndIsGood(operation);
                            }
                            catch (Exception e)
                            {
                                Logger.Error(e);
                                throw;
                            }
                            finally
                            {
                                powerSupply.OutputOff();
                                ElectonicLoad.OutputOff();
                            }
                        }, cancellationToken);
                      
                    };
                    operation.CompliteWorkAsync = () =>
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
                    DataRow.Add(operation);
                }
            }
        }

        #endregion
    }
}