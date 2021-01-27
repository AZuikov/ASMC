using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
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


namespace E363xAPlugin
{
    public class Operation<T> : OperationMetrControlBase where T : E36xxA_Device
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
        public OpertionFirsVerf(ServicePack servicePack, E36xxA_Device inPower) : base(servicePack)
        {
            ControlDevices = new IDeviceUi[]
            {
                new Device
                {
                    Devices = new IDeviceRemote[] {new N3306A(), new N3303A()}, Description = "Электронная нагрузка"
                },
                new Device
                {
                    Devices = new IDeviceRemote[] {new Keysight34401A_Mult()}, Description = "Цифровой мультиметр"
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
            DocumentName = "E363xA_protocol";

            UserItemOperation = new IUserItemOperationBase[]
            {
                new Oper1VisualTest(this),
                new Oper2Oprobovanie(this),
                new UnstableVoltToLoadChange(this),
                new AcVoltChange(this), 
                new VoltageTransientDuration(this), 



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
        public SpeedOpertionFirsVerf(ServicePack servicePack, E36xxA_Device inPower) : base(servicePack)
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
            return new[] { "Результат внешнего осмотра" };
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

        protected E36xxA_Device powerSupply { get; set; }

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
            return new[] { "Результат опробования" };
        }

        protected override string GetReportTableName()
        {
            return "ITBmOprobovanie";
        }

        protected override void InitWork(CancellationTokenSource token)
        {
            powerSupply = UserItemOperation.TestDevices.FirstOrDefault(q => q.SelectedDevice as E36xxA_Device != null)
                                           .SelectedDevice as E36xxA_Device;
            if (powerSupply == null) return;
            powerSupply.StringConnection = GetStringConnect(powerSupply);
            base.InitWork(token);

            var operation = new BasicOperation<bool>();
            operation.InitWork = async () =>
            {
                operation.Expected = true;
                operation.Getting = powerSupply.SelfTest("+0");
            };

            operation.IsGood = () => operation.Getting;
            operation.CompliteWork = () => { return Task.FromResult(operation.IsGood()); };
            DataRow.Add(operation);
        }

        #endregion
    }

    public class UnstableVoltToLoadChange : BasePowerSupplyWithDigitMult<Voltage>
    {

        protected static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public UnstableVoltToLoadChange(IUserItemOperation userItemOperation) :
            base(userItemOperation)
        {
            Name =
                $"Определение нестабильности выходного напряжения от изменения нагрузки";
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
            

            if (powerSupply == null || ElectonicLoad == null) return;

            foreach (E36xxA_Ranges rangePowerSupply in Enum.GetValues(typeof(E36xxA_Ranges)))
            {
                var operation = new BasicOperationVerefication<MeasPoint<Voltage>>();
                operation.InitWork = async () =>
                {
                    try
                    {
                        
                        powerSupply.SetRange(rangePowerSupply);
                        operation.Comment = powerSupply.GetVoltageRange().Description;
                        var _voltRange = powerSupply.Ranges[(int)rangePowerSupply];
                        powerSupply.SetVoltageLevel(new MeasPoint<Voltage>(_voltRange.MainPhysicalQuantity));
                        powerSupply.SetCurrentLevel(new MeasPoint<Current>(_voltRange
                                                                              .AdditionalPhysicalQuantity));
                        // расчитаем идеальное значение для электронной нагрузки
                        var resistToLoad =
                            new MeasPoint<Resistance>(_voltRange.MainPhysicalQuantity.GetNoramalizeValueToSi() /
                                                      _voltRange
                                                         .AdditionalPhysicalQuantity.GetNoramalizeValueToSi());
                        resistToLoad.Round(4);

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
                        digitalMult.DcVoltage.AutoRange = true;
                        digitalMult.DcVoltage.Setting();
                        var U1 = digitalMult.DcVoltage.GetActiveMeasuredValue();
                        operation.Expected = U1;
                        operation.Expected.Round(4);
                        //разрываем цепь
                        ElectonicLoad.OutputOff();
                        Thread.Sleep(3000);
                        digitalMult.DcVoltage.AutoRange = true;
                        digitalMult.DcVoltage.Setting();
                        var U2 = digitalMult.DcVoltage.GetActiveMeasuredValue();

                        powerSupply.OutputOff();
                        ElectonicLoad.OutputOff();

                        operation.Getting = U2;
                        operation.Getting.Round(4);

                        operation.ErrorCalculation = (point, measPoint) => point - measPoint;
                        SetUpperLowerCalcAndIsGood(operation);
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
                operation.CompliteWork = () =>
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

        protected override MeasPoint<Voltage> ErrorCalc(MeasPoint<Voltage> inVal)
        {
            var resultError = new MeasPoint<Voltage>(0.0001M * inVal
                                                              .MainPhysicalQuantity
                                                              .GetNoramalizeValueToSi() +
                                                     0.002M);
            resultError.Round(4);
            return resultError;
        }

        #endregion
    }

    public class AcVoltChange : BasePowerSupplyWithDigitMult<Voltage>
    {
        protected static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        public AcVoltChange(IUserItemOperation userItemOperation) :
            base(userItemOperation)
        {
            Name =
                $"Определение нестабильности выходного напряжения от изменения напряжения питания";
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
                                       0.002M);
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
            
            if (powerSupply == null || ElectonicLoad == null) return;

            foreach (E36xxA_Ranges rangePowerSupply in Enum.GetValues(typeof(E36xxA_Ranges)))
            {
                var operation = new BasicOperationVerefication<MeasPoint<Voltage>>();
                operation.InitWork = async () =>
                {
                    try
                    {
                        powerSupply.SetRange(rangePowerSupply);
                        operation.Comment = powerSupply.GetVoltageRange().Description;
                        var _voltRange = powerSupply.Ranges[(int)rangePowerSupply];
                        powerSupply.SetVoltageLevel(new MeasPoint<Voltage>(_voltRange.MainPhysicalQuantity));
                        powerSupply.SetCurrentLevel(new MeasPoint<Current>(_voltRange.AdditionalPhysicalQuantity));
                        // расчитаем идеальное значение для электронной нагрузки
                        var resistToLoad =
                            new MeasPoint<Resistance>(_voltRange.MainPhysicalQuantity.GetNoramalizeValueToSi() /
                                                      _voltRange
                                                         .AdditionalPhysicalQuantity.GetNoramalizeValueToSi());
                        resistToLoad.Round(4);

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
                        digitalMult.DcVoltage.AutoRange = true;
                        digitalMult.DcVoltage.Setting();
                        var U1 = digitalMult.DcVoltage.GetActiveMeasuredValue();
                        Thread.Sleep(1000);
                        digitalMult.DcVoltage.AutoRange = true;
                        digitalMult.DcVoltage.Setting();
                        var U2 = digitalMult.DcVoltage.GetActiveMeasuredValue();

                        powerSupply.OutputOff();
                        ElectonicLoad.OutputOff();

                        operation.Expected = U1;
                        operation.Expected.Round(4);

                        operation.Getting = U2;
                        operation.Getting.Round(4);

                        operation.ErrorCalculation = (point, measPoint) => point - measPoint;

                       SetUpperLowerCalcAndIsGood(operation);
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
                operation.CompliteWork = () =>
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

        public VoltageTransientDuration(IUserItemOperation userItemOperation) :
            base(userItemOperation)
        {
            Name =
                $"Определение времени переходного процесса при изменении нагрузки";
            Sheme = new ShemeImage
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
            return new MeasPoint<Time>(50,UnitMultiplier.Micro);
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
                var arrPoints = new[] { "1", "2", "3", "4", "5" };
                MeasPoint<Time>[] arrGetting = null;
                operation.InitWork = async () =>
                {
                    var setting = new TableViewModel.SettingTableViewModel { Breaking = 1, CellFormat = "мкс" };
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
                operation.BodyWorkAsync = () =>
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
                };
                operation.ErrorCalculation = (point, measPoint) => new MeasPoint<Time>(50, UnitMultiplier.Micro);
                operation.IsGood = () => operation.Expected < operation.Error;

                operation.CompliteWork = () => Task.FromResult(operation.IsGood());

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

}
