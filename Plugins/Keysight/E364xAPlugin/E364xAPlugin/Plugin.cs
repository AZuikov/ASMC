using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AP.Extension;
using AP.Utils.Data;
using ASMC.Core.Model;
using ASMC.Data.Model;
using ASMC.Data.Model.Interface;
using ASMC.Data.Model.PhysicalQuantity;
using ASMC.Devices.IEEE;
using ASMC.Devices.IEEE.Keysight.ElectronicLoad;
using ASMC.Devices.IEEE.Keysight.Multimeter;
using ASMC.Devices.IEEE.Keysight.PowerSupplies;
using ASMC.Devices.IEEE.Keysight.PowerSupplyes;
using DevExpress.Mvvm;
using NLog;
using Current = ASMC.Data.Model.PhysicalQuantity.Current;
using Resistance = ASMC.Data.Model.PhysicalQuantity.Resistance;
using Voltage = ASMC.Data.Model.PhysicalQuantity.Voltage;


namespace E364xAPlugin
{
    public class Plugin  : Program<Operation> 
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
                    Devices =   new IDeviceBase[]{new N3303A(), new N3306A()}, Description = "Электронная нагрузка"
                },
                new Device
                {
                    Devices = new IDeviceBase[] {new Mult_34401A() }, Description = "Цифровой мультиметр"
                }, 
            };

            TestDevices = new IDeviceUi[]
            {
                new Device
                {
                    Devices = new IDeviceBase[]{new E3648A() }, Description = "Мера напряжения и тока"
                } 
            };
            DocumentName = "E364xA_protocol";

            UserItemOperation = new IUserItemOperationBase[]
            {
                new Oper1VisualTest(this),
                new Oper2Oprobovanie(this),
                new Oper3UnstableVoltageLoadChange(this, E364xA.Chanel.OUTP1), 
            };
        }

        public override void RefreshDevice()
        {
            AddresDevice = IeeeBase.AllStringConnect;
        }

        public override void FindDevice()
        {
            throw new NotImplementedException();
        }
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
        protected E36XX_IPowerSupply powerSupply { get; set; }

        public Oper2Oprobovanie(IUserItemOperation userItemOperation) : base(userItemOperation)
        {
            Name = "Опробование";
            DataRow = new List<IBasicOperation<bool>>();
        }

        protected override string[] GenerateDataColumnTypeObject()
        {
            return new[] { "Результат опробования" };
        }

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

        protected override string GetReportTableName()
        {
           return "ITBmOprobovanie";
        }

        protected override void InitWork(CancellationTokenSource token)
        {
           
            powerSupply = UserItemOperation.TestDevices.FirstOrDefault(q => q.SelectedDevice as E364xA != null)
                                            .SelectedDevice as E36XX_IPowerSupply ;
            if (powerSupply == null) return;
            ((IeeeBase)powerSupply).StringConnection = GetStringConnect((IProtocolStringLine)powerSupply);
            base.InitWork(token);

            var operation = new BasicOperation<bool>();
            operation.InitWork = async () =>
            {
                operation.Expected = true;
                operation.Getting = ((IeeeBase)powerSupply).SelfTest("+0");
            };
            
            operation.IsGood = () => operation.Getting;
            operation.CompliteWork = () => { return Task.FromResult(operation.IsGood()); };
            DataRow.Add(operation);

        }
    }

    public class Oper3UnstableVoltageLoadChange : ParagraphBase<MeasPoint<Voltage>>
    {
        private E364xA.Chanel _chanelNumber;
        protected E36XX_IPowerSupply powerSupply { get; set; }
        protected IElectronicLoad ElectonicLoad { get; set; }
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public Oper3UnstableVoltageLoadChange(IUserItemOperation userItemOperation, E364xA.Chanel inChanel) : base(userItemOperation)
        {
            _chanelNumber = inChanel;
            Name = $"Определение нестабильности выходного напряжения \nот изменения нагрузки (канал {_chanelNumber})";
        }

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
            return $"FillTabBm_Oper3_ch{_chanelNumber}";
        }

        protected override void InitWork(CancellationTokenSource token)
        {
            base.InitWork(token);
            ElectonicLoad = UserItemOperation.ControlDevices.FirstOrDefault(q => q.SelectedDevice as IElectronicLoad != null)
                                             .SelectedDevice as IElectronicLoad;

            powerSupply = UserItemOperation.TestDevices.FirstOrDefault(q => q.SelectedDevice as E364xA != null)
                                           .SelectedDevice as E36XX_IPowerSupply;
            if (powerSupply == null || ElectonicLoad == null) return;
            ((IeeeBase)powerSupply).StringConnection = GetStringConnect((IProtocolStringLine)powerSupply);
            ((IeeeBase)ElectonicLoad).StringConnection = GetStringConnect((IProtocolStringLine)ElectonicLoad);
            
            foreach (var rangePowerSupply in powerSupply.GetRangeS())
            {
                var operation = new BasicOperationVerefication<MeasPoint<Voltage>>();;
                operation.InitWork = async () =>
                {
                    powerSupply.SetRange(rangePowerSupply);

                    operation.Comment = powerSupply.GetVoltageRange().Description;
                   
                    powerSupply.SetVoltageLevel(new MeasPoint<Voltage>(rangePowerSupply.MainPhysicalQuantity));
                    powerSupply.SetCurrentLevel(new MeasPoint<Current>(rangePowerSupply.AdditionalPhysicalQuantity));
                    // расчитаем идеальное значение для электронной нагрузки
                    MeasPoint<Resistance> resistToLoad =
                        new MeasPoint<Resistance>(rangePowerSupply.MainPhysicalQuantity.Value/rangePowerSupply.AdditionalPhysicalQuantity.Value);

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
                        Thread.Sleep(1000);
                        MeasPoint<Voltage> U1 = ElectonicLoad.GetMeasureVoltage();
                        //разрываем цепь
                        ElectonicLoad.OutputOff();
                        Thread.Sleep(1000);
                        MeasPoint<Voltage> U2 = ElectonicLoad.GetMeasureVoltage();

                        powerSupply.OutputOff();
                        ElectonicLoad.OutputOff();

                        operation.Expected = new MeasPoint<Voltage>(0);
                        var measResult = U1 - U2;
                        measResult.Round(3);
                        operation.Getting = measResult;
                        
                        operation.ErrorCalculation = (point, measPoint) =>
                        {
                            return new MeasPoint<Voltage> (0.0001M * U1.MainPhysicalQuantity.GetNoramalizeValueToSi() + 0.003M);
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
                                : (BasicOperationVerefication<MeasPoint<Voltage>>)operation.Clone());

            }

            
            
        }
    }


    public class Oper4AcVoltChange : ParagraphBase<MeasPoint<Voltage>>
    {
        private E364xA.Chanel _chanelNumber;
        protected E36XX_IPowerSupply powerSupply { get; set; }
        protected IElectronicLoad ElectonicLoad { get; set; }
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public Oper4AcVoltChange(IUserItemOperation userItemOperation, E364xA.Chanel inChanel) : base(userItemOperation)
        {
            _chanelNumber = inChanel;
            Name = $"Определение нестабильности выходного напряжения \nот изменения напряжения питания (канал {_chanelNumber})";
        }

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
            return $"FillTabBm_Oper3_ch{_chanelNumber}";
        }

        protected override void InitWork(CancellationTokenSource token)
        {
            base.InitWork(token);
            ElectonicLoad = UserItemOperation.ControlDevices.FirstOrDefault(q => q.SelectedDevice as IElectronicLoad != null)
                                             .SelectedDevice as IElectronicLoad;

            powerSupply = UserItemOperation.TestDevices.FirstOrDefault(q => q.SelectedDevice as E364xA != null)
                                           .SelectedDevice as E36XX_IPowerSupply;
            if (powerSupply == null || ElectonicLoad == null) return;
            ((IeeeBase)powerSupply).StringConnection = GetStringConnect((IProtocolStringLine)powerSupply);
            ((IeeeBase)ElectonicLoad).StringConnection = GetStringConnect((IProtocolStringLine)ElectonicLoad);

            foreach (var rangePowerSupply in powerSupply.GetRangeS())
            {
                var operation = new BasicOperationVerefication<MeasPoint<Voltage>>(); ;
                operation.InitWork = async () =>
                {
                    powerSupply.SetRange(rangePowerSupply);

                    operation.Comment = powerSupply.GetVoltageRange().Description;
                    powerSupply.SetVoltageLevel(new MeasPoint<Voltage>(rangePowerSupply.MainPhysicalQuantity));
                    powerSupply.SetCurrentLevel(new MeasPoint<Current>(rangePowerSupply.AdditionalPhysicalQuantity));
                    // расчитаем идеальное значение для электронной нагрузки
                    MeasPoint<Resistance> resistToLoad =
                        new MeasPoint<Resistance>(rangePowerSupply.MainPhysicalQuantity.Value / rangePowerSupply.AdditionalPhysicalQuantity.Value);


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
                        Thread.Sleep(1000);
                        MeasPoint<Voltage> U1 = ElectonicLoad.GetMeasureVoltage();
                        MeasPoint<Voltage> U2 = ElectonicLoad.GetMeasureVoltage();

                        powerSupply.OutputOff();
                        ElectonicLoad.OutputOff();

                        operation.Expected = new MeasPoint<Voltage>(0);
                        decimal measResult = U1.MainPhysicalQuantity.GetNoramalizeValueToSi() - U2.MainPhysicalQuantity.GetNoramalizeValueToSi();
                        measResult = Math.Round(measResult, 3);
                        operation.Getting = new MeasPoint<Voltage>(measResult);

                        operation.ErrorCalculation = (point, measPoint) =>
                        {
                            return new MeasPoint<Voltage>(0.0001M * U1.MainPhysicalQuantity.GetNoramalizeValueToSi() + 0.003M);
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
                                : (BasicOperationVerefication<MeasPoint<Voltage>>)operation.Clone());

            }



        }
    }

    public class Oper6UnstableVoltageOnTime : ParagraphBase<MeasPoint<Voltage>>
    {
        private E364xA.Chanel _chanelNumber;
        protected E36XX_IPowerSupply powerSupply { get; set; }
        protected IElectronicLoad ElectonicLoad { get; set; }
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public Oper6UnstableVoltageOnTime(IUserItemOperation userItemOperation, E364xA.Chanel inChanel) : base(userItemOperation)
        {
            _chanelNumber = inChanel;
            Name = $"Определение величины дрейфа выходного напряжения (канал {_chanelNumber})";
        }

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
            return $"FillTabBm_Oper3_ch{_chanelNumber}";
        }

        protected override void InitWork(CancellationTokenSource token)
        {
            base.InitWork(token);
            ElectonicLoad = UserItemOperation.ControlDevices.FirstOrDefault(q => q.SelectedDevice as IElectronicLoad != null)
                                             .SelectedDevice as IElectronicLoad;

            powerSupply = UserItemOperation.TestDevices.FirstOrDefault(q => q.SelectedDevice as E364xA != null)
                                           .SelectedDevice as E36XX_IPowerSupply;
            if (powerSupply == null || ElectonicLoad == null) return;
            ((IeeeBase)powerSupply).StringConnection = GetStringConnect((IProtocolStringLine)powerSupply);
            ((IeeeBase)ElectonicLoad).StringConnection = GetStringConnect((IProtocolStringLine)ElectonicLoad);

            
                var operation = new BasicOperationVerefication<MeasPoint<Voltage>>(); ;
                operation.InitWork = async () =>
                {
                    
                        powerSupply.SetHighVoltageRange();
                    
                    operation.Comment = powerSupply.GetVoltageRange().Description;
                    powerSupply.SetMaxVoltageLevel();
                    powerSupply.SetMaxCurrentLevel();
                    //узнаем максимумы тока и напряжения
                    MeasPoint<Voltage> MaxVolt = powerSupply.GetVoltageLevel();
                    MeasPoint<Current> MaxCurr = powerSupply.GetCurrentLevel();
                    //источник выставит не круглые значения. Округлим их до идеала (если значения выше нормированного порога,
                    //то стабильность характеристик прибора не гарантируется, эти значения не нормируются документацией).
                    MaxVolt = new MeasPoint<Voltage>(Math.Round(MaxVolt.MainPhysicalQuantity.GetNoramalizeValueToSi(), 0));
                    MaxCurr = new MeasPoint<Current>(Math.Round(MaxCurr.MainPhysicalQuantity.GetNoramalizeValueToSi(), 0));
                    // расчитаем идеальное значение для электронной нагрузки
                    MeasPoint<Resistance> resistToLoad =
                        new MeasPoint<Resistance>(MaxVolt.MainPhysicalQuantity.GetNoramalizeValueToSi() /
                                                  MaxCurr.MainPhysicalQuantity.GetNoramalizeValueToSi());

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
                        //массив из 17 измерений
                        MeasPoint<Voltage>[]measPoints = new MeasPoint<Voltage>[16];
                        Thread.Sleep(5000);
                        MeasPoint<Voltage> U1 = ElectonicLoad.GetMeasureVoltage();
                        for (int i = 1; i < measPoints.Length; i++)
                        {
                            Thread.Sleep(15000);//пауза между измерениями
                            MeasPoint<Voltage> Un = ElectonicLoad.GetMeasureVoltage();
                            //сразу посчитаем разности
                            measPoints[i] = measPoints[0] - Un;
                        }
                        
                        

                        powerSupply.OutputOff();
                        ElectonicLoad.OutputOff();

                        operation.Expected = new MeasPoint<Voltage>(0);
                        
                        operation.Getting = measPoints.Max();//верно ли он ищет максимум?

                        operation.ErrorCalculation = (point, measPoint) =>
                        {
                            return new MeasPoint<Voltage>(0.0002M * U1.MainPhysicalQuantity.GetNoramalizeValueToSi() + 0.003M);
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
                                : (BasicOperationVerefication<MeasPoint<Voltage>>)operation.Clone());

            



        }
    }
}
