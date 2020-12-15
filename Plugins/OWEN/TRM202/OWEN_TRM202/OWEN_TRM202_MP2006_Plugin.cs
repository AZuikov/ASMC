using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AP.Math;
using ASMC.Core.Model;
using ASMC.Data.Model;
using ASMC.Data.Model.Interface;
using ASMC.Data.Model.PhysicalQuantity;
using ASMC.Devices.IEEE;
using ASMC.Devices.IEEE.Fluke.Calibrator;
using ASMC.Devices.OWEN;
using ASMC.Devices.Port.IZ_Tech;
using ASMC.Devices.UInterface.TRM.ViewModel;
using DevExpress.Mvvm;
using NLog;

//Тут должен быть код для методики 2006 года, сделано только опробование

//namespace OWEN_TRM202
//{
//    public class OWEN_TRM202_MP2006_Plugin : Program<Operation2006>
//    {
//        public OWEN_TRM202_MP2006_Plugin(ServicePack service) : base(service)
//        {
//            Grsi = "32478-06 (МП2006) Не ";
//            Type = "ТРМ202";
//        }
//    }

//    public class Operation2006 : OperationMetrControlBase
//    {
//        //определяет какие типы проверок доступны для СИ: поверка первичная/переодическая, калибровка, adjustment.
//        public Operation2006(ServicePack servicePac)
//        {
//            //это операция первичной поверки
//            UserItemOperationPrimaryVerf = new OpertionFirsVerf2006(servicePac);
//            //здесь периодическая поверка, но набор операций такой же
//            UserItemOperationPeriodicVerf = UserItemOperationPrimaryVerf;
//        }
//    }

//    public class OpertionFirsVerf2006 : ASMC.Core.Model.Operation
//    {
//        public OpertionFirsVerf2006(ServicePack servicePack) : base(servicePack)
//        {
//            //Необходимые устройства
//            ControlDevices = new IDeviceUi[]
//            {
//                new Device
//                {
//                    Devices = new IDeviceBase[] {new Calib5522A()}, Description = "Многофунциональный калибратор"
//                },
//                new Device
//                {
//                    Devices = new IDeviceBase[] {new MIT_8()}, Description = "измеритель температуры прецизионный"
//                }
//            };
//            TestDevices = new IDeviceUi[]
//            {
//                new Device
//                {
//                    Devices = new IDeviceBase[] {new TRM202DeviceUI()},
//                    Description = "измеритель температуры прецизионный"
//                }
//            };

//            DocumentName = "TRM202_protocol";

//            var oprobovanieCh1 = new Oprobovanie_7_3_5_1(this, 1);
//            oprobovanieCh1.Name = "Опрбование канала 1";
//            oprobovanieCh1.Nodes.Add(new Oprobovanie_7_3_5_1_Chanel1(this));
//            oprobovanieCh1.Nodes.Add(new Oprobovanie_7_3_5_2_Chanel1(this));
//            oprobovanieCh1.Nodes.Add(new Oprobovanie_7_3_5_3_Chanel1(this));

//            var oprobovanieCh2 = new Oprobovanie_7_3_5_1(this, 1);
//            oprobovanieCh2.Name = "Опрбование канала 2";
//            oprobovanieCh2.Nodes.Add(new Oprobovanie_7_3_5_1_Chanel2(this));
//            oprobovanieCh2.Nodes.Add(new Oprobovanie_7_3_5_2_Chanel2(this));
//            oprobovanieCh2.Nodes.Add(new Oprobovanie_7_3_5_3_Chanel2(this));

//            UserItemOperation = new IUserItemOperationBase[]
//            {
//                oprobovanieCh1,
//                oprobovanieCh2
//            };
//        }

//        #region Methods

//        public override void FindDevice()
//        {
//            throw null;
//        }

//        public override void RefreshDevice()
//        {
//            AddresDevice = IeeeBase.AllStringConnect;
//        }

//        #endregion
//    }

//    public class OperationForChanel
//    {
//        public OperationForChanel()
//        {
//            var operationArr = new IUserItemOperationBase[] { };
//        }
//    }

//    public class Oper1VisualTest : ParagraphBase<bool>
//    {
//        public Oper1VisualTest(IUserItemOperation userItemOperation) : base(userItemOperation)
//        {
//            Name = "Внешний осмотр";
//            DataRow = new List<IBasicOperation<bool>>();
//        }

//        #region Methods

//        protected override DataTable FillData()
//        {
//            var data = base.FillData();
//            var dataRow = data.NewRow();
//            if (DataRow.Count == 1)
//            {
//                var dds = DataRow[0] as BasicOperation<bool>;
//                dataRow[0] = dds.Getting ? "Соответствует" : dds.Comment;
//                data.Rows.Add(dataRow);
//            }

//            return data;
//        }

//        protected override string GetReportTableName()
//        {
//            throw null;
//        }

//        #endregion
//    }

//    public class Oprobovanie_7_3_5_1 : ParagraphBase<MeasPoint<Temperature>>
//    {
//        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

//        #region Fields

//        private readonly ushort _chanelNumber;

//        #endregion

//        #region Property

//        protected Calib5522A flkCalib5522A { get; set; }

//        protected TRM202DeviceUI trm202 { get; set; }

//        #endregion

//        /// <summary>
//        /// Объект операции опробования.
//        /// </summary>
//        /// <param name = "userItemOperation"></param>
//        /// <param name = "chanelNumb">Номер канала прибора 1 или 2.</param>
//        public Oprobovanie_7_3_5_1(IUserItemOperation userItemOperation, ushort chanelNumb) : base(userItemOperation)
//        {
//            _chanelNumber = chanelNumb;
//            Name = $"Опробование: 7.3.5.1 проверка исправности измерительных входов канала {_chanelNumber}";
//            DataRow = new List<IBasicOperation<MeasPoint<Temperature>>>();
//            Sheme = new ShemeImage
//            {
//                Number = 1,
//                FileName = $"TRM202_5522A_OprobovanieResistanceTermocouple_chanel{_chanelNumber}.jpg",
//                Description = "Соберите схему, как показано на рисунке."
//            };
//        }

//        #region Methods

//        protected override DataTable FillData()
//        {
//            var dataTable = base.FillData();
//            foreach (var row in DataRow)
//            {
//                var dataRow = dataTable.NewRow();
//                var dds = DataRow[0] as BasicOperationVerefication<MeasPoint<Temperature>>;
//                if (dds == null) continue;
//                dataRow[0] = dds.Getting?.Description;
//                dataRow[1] = dds.LowerTolerance?.Description;
//                dataRow[2] = dds.UpperTolerance?.Description;
//                if (dds.IsGood == null)
//                    dataRow[3] = "не выполнено";
//                else
//                    dataRow[3] = dds.IsGood() ? "Годен" : "Брак";
//                dataTable.Rows.Add(dataRow);
//            }

//            return dataTable;
//        }

//        /// <inheritdoc />
//        protected override string[] GenerateDataColumnTypeObject()
//        {
//            return new[]
//            {
//                "Показания прибора при величине сопротивления 51,07 Ом", "Нижний предел", "Верхний предел", "Результат"
//            };
//        }

//        /// <inheritdoc />
//        protected override string GetReportTableName()
//        {
//            //AP>REPORT>UTILS есть перечисление с именем таблицы, от типа имени закладки зависит механизм заполнения
//            return $"ITBmOprobovanie7351{_chanelNumber}";
//        }

//        protected override void InitWork(CancellationTokenSource token)
//        {
//            base.InitWork(token);

//            trm202 = (UserItemOperation.TestDevices.FirstOrDefault(q => q.SelectedDevice as TRM202DeviceUI != null)
//                                       .SelectedDevice as IControlPannelDevice).Device as TRM202DeviceUI;

//            if (trm202 == null || flkCalib5522A == null) return;

//            trm202.StringConnection = GetStringConnect(trm202);
//            flkCalib5522A.StringConnection ??= GetStringConnect(flkCalib5522A);

//            if (UserItemOperation != null)
//            {
//                var operation = new BasicOperationVerefication<MeasPoint<Temperature>>();
//                operation.InitWork = async () =>
//                {
//                    try
//                    {
//                        await Task.Run(() =>
//                        {
//                            //делаем предварительные настройка канала прибора

//                            //1. задаем на каналах нужную характеристику НСХ 50М (W100 = 1,4280)
//                            var typeTermoCouple = BitConverter
//                                                 .GetBytes((int) TRM202Device.in_t.r428).Where(a => a != 0).ToArray();
//                            trm202.WriteParametrToTRM(TRM202Device.Parametr.InT, typeTermoCouple, _chanelNumber);
//                            //2. ставим сдвиги и наклоны характеристик
//                            trm202.WriteFloat24Parametr(TRM202Device.Parametr.SH, 0, _chanelNumber);
//                            trm202.WriteFloat24Parametr(TRM202Device.Parametr.KU, 1, _chanelNumber);
//                            //3. ставим полосы фильтров и постоянную времени фильтра
//                            trm202.WriteFloat24Parametr(TRM202Device.Parametr.Fb, 0, _chanelNumber);
//                            trm202.WriteFloat24Parametr(TRM202Device.Parametr.InF, 0, _chanelNumber);
//                        });
//                    }
//                    catch (Exception e)
//                    {
//                        Logger.Error(e);
//                        throw;
//                    }
//                };
//                operation.BodyWorkAsync = () =>
//                {
//                    try
//                    {
//                        operation.Expected = new MeasPoint<Temperature>(5M);
//                        operation.ErrorCalculation = (inA, inB) => new MeasPoint<Temperature>(0.9M);
//                        operation.LowerTolerance = operation.Expected - operation.Error;
//                        operation.UpperTolerance = operation.Expected + operation.Error;
//                        var setPoint = new MeasPoint<Resistance>(51.07M);
//                        flkCalib5522A.Out.Set.Resistance.SetValue(setPoint);
//                        flkCalib5522A.Out.SetOutput(CalibrMain.COut.State.On);
//                        Thread.Sleep(2000);
//                        var measPoint = trm202.GetMeasValChanel(_chanelNumber);
//                        flkCalib5522A.Out.SetOutput(CalibrMain.COut.State.Off);
//                        var mantisa = MathStatistics.GetMantissa(operation.Error.MainPhysicalQuantity.Value);
//                        MathStatistics.Round(ref measPoint, mantisa);
//                        operation.Getting = new MeasPoint<Temperature>(measPoint);
//                    }
//                    catch (Exception e)
//                    {
//                        Logger.Error(e);
//                        throw;
//                    }
//                };

//                operation.IsGood = () =>
//                    operation.Getting >= operation.LowerTolerance && operation.Getting <= operation.UpperTolerance;

//                operation.CompliteWork = () =>
//                {
//                    if (!operation.IsGood())
//                    {
//                        var answer =
//                            UserItemOperation.ServicePack.MessageBox().Show(operation +
//                                                                            $"\nФАКТИЧЕСКАЯ погрешность {operation.Expected - operation.Getting}\n\n" +
//                                                                            "Повторить измерение этой точки?",
//                                                                            "Информация по текущему измерению",
//                                                                            MessageButton.YesNo,
//                                                                            MessageIcon.Question,
//                                                                            MessageResult.Yes);

//                        if (answer == MessageResult.No) return Task.FromResult(true);
//                    }

//                    return Task.FromResult(operation.IsGood());
//                };

//                DataRow.Add(DataRow.IndexOf(operation) == -1
//                                ? operation
//                                : (BasicOperationVerefication<MeasPoint<Temperature>>) operation.Clone());
//            }
//        }

//        #endregion
//    }

//    public class Oprobovanie_7_3_5_1_Chanel1 : Oprobovanie_7_3_5_1
//    {
//        public Oprobovanie_7_3_5_1_Chanel1(IUserItemOperation userItemOperation) : base(userItemOperation, 1)
//        {
//            flkCalib5522A = new Calib5522A();
//        }
//    }

//    public class Oprobovanie_7_3_5_1_Chanel2 : Oprobovanie_7_3_5_1
//    {
//        public Oprobovanie_7_3_5_1_Chanel2(IUserItemOperation userItemOperation) : base(userItemOperation, 2)
//        {
//            flkCalib5522A = new Calib5522A();
//        }
//    }

//    public class Oprobovanie_7_3_5_2 : ParagraphBase<MeasPoint<Percent>>
//    {
//        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

//        #region Fields

//        private readonly ushort _chanelNumber;

//        #endregion

//        #region Property

//        protected Calib5522A flkCalib5522A { get; set; }

//        protected TRM202DeviceUI trm202 { get; set; }

//        #endregion

//        protected Oprobovanie_7_3_5_2(IUserItemOperation userItemOperation, ushort chanelNumb) : base(userItemOperation)
//        {
//            _chanelNumber = chanelNumb;
//            Name =
//                $"Опробование: 7.3.5.2 Проверка исправности входов, работающих с унифицированным сигналом постоянного тока (канал {_chanelNumber})";
//            DataRow = new List<IBasicOperation<MeasPoint<Percent>>>();
//            Sheme = new ShemeImage
//            {
//                Number = 2,
//                FileName = $"TRM202_Oprobovanie7353_chanel{_chanelNumber}.jpg",
//                Description = "Соберите схему, как показано на рисунке."
//            };
//        }

//        #region Methods

//        /// <inheritdoc />
//        protected override string[] GenerateDataColumnTypeObject()
//        {
//            return new[] {"Показания прибора при токе 10 мА", "Нижний предел", "Верхний предел", "Результат"};
//        }

//        protected override string GetReportTableName()
//        {
//            //AP>REPORT>UTILS есть перечисление с именем таблицы, от типа имени закладки зависит механизм заполнения
//            return $"ITBmOprobovanie7353{_chanelNumber}";
//        }

//        protected override void InitWork(CancellationTokenSource token)
//        {
//            base.InitWork(token);

//            trm202 = (UserItemOperation.TestDevices.FirstOrDefault(q => q.SelectedDevice as TRM202DeviceUI != null)
//                                       .SelectedDevice as IControlPannelDevice).Device as TRM202DeviceUI;

//            if (trm202 == null || flkCalib5522A == null) return;

//            trm202.StringConnection = GetStringConnect(trm202);
//            flkCalib5522A.StringConnection ??= GetStringConnect(flkCalib5522A);

//            if (UserItemOperation != null)
//            {
//                var operation = new BasicOperationVerefication<MeasPoint<Percent>>();
//                operation.InitWork = async () =>
//                {
//                    try
//                    {
//                        await Task.Run(() => { flkCalib5522A.Out.SetOutput(CalibrMain.COut.State.Off); });

//                        await Task.Run(() =>
//                        {
//                            //делаем предварительные настройка канала прибора

//                            //1. задаем на каналах нужную характеристику работы с унифицированным сигналом 0 … 1,0 В
//                            var typeTermoCouple = BitConverter
//                                                 .GetBytes((int) TRM202Device.in_t.i0_20).Where(a => a != 0).ToArray();
//                            trm202.WriteParametrToTRM(TRM202Device.Parametr.InT, typeTermoCouple, _chanelNumber);
//                            //2. ставим сдвиги и наклоны характеристик
//                            trm202.WriteFloat24Parametr(TRM202Device.Parametr.SH, 0, _chanelNumber);
//                            trm202.WriteFloat24Parametr(TRM202Device.Parametr.KU, 1, _chanelNumber);
//                            //3. ставим полосы фильтров и постоянную времени фильтра
//                            trm202.WriteFloat24Parametr(TRM202Device.Parametr.Fb, 0, _chanelNumber);
//                            trm202.WriteFloat24Parametr(TRM202Device.Parametr.InF, 0, _chanelNumber);
//                            //4. установить для поверяемого канала в программируемом параметре «Нижняя граница
//                            //диапазона измерения» значение «000.0», а в параметре «Верхняя граница диапазона измерения» – значение «100.0»
//                            trm202.WriteFloat24Parametr(TRM202Device.Parametr.InL, 0, _chanelNumber);
//                            trm202.WriteFloat24Parametr(TRM202Device.Parametr.InH, 100, _chanelNumber);
//                        });
//                    }
//                    catch (Exception e)
//                    {
//                        Logger.Error(e);
//                        throw;
//                    }
//                };
//                operation.BodyWorkAsync = () =>
//                {
//                    try
//                    {
//                        operation.Expected = new MeasPoint<Percent>(50M);
//                        operation.ErrorCalculation = (inA, inB) => new MeasPoint<Percent>(0.5M);
//                        operation.LowerTolerance = operation.Expected - operation.Error;
//                        operation.UpperTolerance = operation.Expected + operation.Error;
//                        var setPoint = new MeasPoint<Voltage>(1);
//                        flkCalib5522A.Out.Set.Voltage.Dc.SetValue(setPoint);
//                        flkCalib5522A.Out.SetOutput(CalibrMain.COut.State.On);
//                        Thread.Sleep(2000);
//                        var measPoint = trm202.GetMeasValChanel(_chanelNumber);
//                        flkCalib5522A.Out.SetOutput(CalibrMain.COut.State.Off);
//                        var mantisa = MathStatistics.GetMantissa(operation.Error.MainPhysicalQuantity.Value);
//                        MathStatistics.Round(ref measPoint, mantisa);
//                        operation.Getting = new MeasPoint<Percent>(measPoint);
//                    }
//                    catch (Exception e)
//                    {
//                        Logger.Error(e);
//                        throw;
//                    }
//                };
//                operation.IsGood = () =>
//                    operation.Getting >= operation.LowerTolerance && operation.Getting <= operation.UpperTolerance;
//                operation.CompliteWork = () =>
//                {
//                    if (!operation.IsGood())
//                    {
//                        var answer =
//                            UserItemOperation.ServicePack.MessageBox()
//                                             .Show($"Текущая точка {operation.Expected.Description} не проходит по допуску:\n" +
//                                                   $"Минимально допустимое значение {operation.LowerTolerance.Description}\n" +
//                                                   $"Максимально допустимое значение {operation.UpperTolerance.Description}\n" +
//                                                   $"Допустимое значение погрешности {operation.Error.Description}\n" +
//                                                   $"ИЗМЕРЕННОЕ значение {operation.Getting.Description}\n\n" +
//                                                   $"\nФАКТИЧЕСКАЯ погрешность {(operation.Expected - operation.Getting).Description}\n\n" +
//                                                   "Повторить измерение этой точки?",
//                                                   "Информация по текущему измерению",
//                                                   MessageButton.YesNo, MessageIcon.Question,
//                                                   MessageResult.Yes);

//                        if (answer == MessageResult.No) return Task.FromResult(true);
//                    }

//                    return Task.FromResult(operation.IsGood());
//                };

//                DataRow.Add(DataRow.IndexOf(operation) == -1
//                                ? operation
//                                : (BasicOperationVerefication<MeasPoint<Percent>>) operation.Clone());
//            }
//        }

//        #endregion
//    }

//    public class Oprobovanie_7_3_5_2_Chanel1 : Oprobovanie_7_3_5_2
//    {
//        public Oprobovanie_7_3_5_2_Chanel1(IUserItemOperation userItemOperation) : base(userItemOperation, 1)
//        {
//            flkCalib5522A = new Calib5522A();
//        }
//    }

//    public class Oprobovanie_7_3_5_2_Chanel2 : Oprobovanie_7_3_5_2
//    {
//        public Oprobovanie_7_3_5_2_Chanel2(IUserItemOperation userItemOperation) : base(userItemOperation, 2)
//        {
//            flkCalib5522A = new Calib5522A();
//        }
//    }

//    public class Oprobovanie_7_3_5_3 : ParagraphBase<MeasPoint<Percent>>
//    {
//        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

//        #region Fields

//        private readonly ushort _chanelNumber;

//        #endregion

//        #region Property

//        protected Calib5522A flkCalib5522A { get; set; }

//        protected TRM202DeviceUI trm202 { get; set; }

//        #endregion

//        protected Oprobovanie_7_3_5_3(IUserItemOperation userItemOperation, ushort chanelNumb) : base(userItemOperation)
//        {
//            _chanelNumber = chanelNumb;
//            Name =
//                $"Опробование: 7.3.5.3 Проверка исправности входов, работающих с унифицированным сигналом постоянного напряжения и термопарами (канал {_chanelNumber})";
//            DataRow = new List<IBasicOperation<MeasPoint<Percent>>>();
//            Sheme = new ShemeImage
//            {
//                Number = 2,
//                FileName = $"TRM202_Oprobovanie7353_chanel{_chanelNumber}.jpg",
//                Description = "Соберите схему, как показано на рисунке."
//            };
//        }

//        #region Methods

//        /// <inheritdoc />
//        protected override string[] GenerateDataColumnTypeObject()
//        {
//            return new[] {"Показания прибора при напряжении 250 мВ", "Нижний предел", "Верхний предел", "Результат"};
//        }

//        protected override string GetReportTableName()
//        {
//            //AP>REPORT>UTILS есть перечисление с именем таблицы, от типа имени закладки зависит механизм заполнения
//            return $"ITBmOprobovanie7353{_chanelNumber}";
//        }

//        protected override void InitWork(CancellationTokenSource token)
//        {
//            base.InitWork(token);
//            trm202 = (UserItemOperation.TestDevices.FirstOrDefault(q => q.SelectedDevice as TRM202DeviceUI != null)
//                                       .SelectedDevice as IControlPannelDevice).Device as TRM202DeviceUI;

//            if (trm202 == null || flkCalib5522A == null) return;

//            trm202.StringConnection = GetStringConnect(trm202);
//            flkCalib5522A.StringConnection ??= GetStringConnect(flkCalib5522A);

//            if (UserItemOperation != null)
//            {
//                var operation = new BasicOperationVerefication<MeasPoint<Percent>>();
//                operation.InitWork = async () =>
//                {
//                    try
//                    {
//                        await Task.Run(() => { flkCalib5522A.Out.SetOutput(CalibrMain.COut.State.Off); });

//                        await Task.Run(() =>
//                        {
//                            //делаем предварительные настройка канала прибора

//                            //1. задаем на каналах нужную характеристику работы с унифицированным сигналом 0 … 1,0 В
//                            var typeTermoCouple = BitConverter
//                                                 .GetBytes((int) TRM202Device.in_t.U0_1).Where(a => a != 0).ToArray();
//                            trm202.WriteParametrToTRM(TRM202Device.Parametr.InT, typeTermoCouple, _chanelNumber);
//                            //2. ставим сдвиги и наклоны характеристик
//                            trm202.WriteFloat24Parametr(TRM202Device.Parametr.SH, 0, _chanelNumber);
//                            trm202.WriteFloat24Parametr(TRM202Device.Parametr.KU, 1, _chanelNumber);
//                            //3. ставим полосы фильтров и постоянную времени фильтра
//                            trm202.WriteFloat24Parametr(TRM202Device.Parametr.Fb, 0, _chanelNumber);
//                            trm202.WriteFloat24Parametr(TRM202Device.Parametr.InF, 0, _chanelNumber);
//                        });
//                    }
//                    catch (Exception e)
//                    {
//                        Logger.Error(e);
//                        throw;
//                    }
//                };
//                operation.BodyWorkAsync = () =>
//                {
//                    try
//                    {
//                        operation.Expected = new MeasPoint<Percent>(25M);
//                        operation.ErrorCalculation = (inA, inB) => new MeasPoint<Percent>(0.5M);
//                        operation.LowerTolerance = operation.Expected - operation.Error;
//                        operation.UpperTolerance = operation.Expected + operation.Error;
//                        var setPoint = new MeasPoint<Voltage>(operation
//                                                             .Expected.MainPhysicalQuantity
//                                                             .GetNoramalizeValueToSi() / 100);
//                        flkCalib5522A.Out.Set.Voltage.Dc.SetValue(setPoint);
//                        flkCalib5522A.Out.SetOutput(CalibrMain.COut.State.On);
//                        Thread.Sleep(2000);
//                        var measPoint = trm202.GetMeasValChanel(_chanelNumber);
//                        flkCalib5522A.Out.SetOutput(CalibrMain.COut.State.Off);
//                        var mantisa = MathStatistics.GetMantissa(operation.Error.MainPhysicalQuantity.Value);
//                        MathStatistics.Round(ref measPoint, mantisa);
//                        operation.Getting = new MeasPoint<Percent>(measPoint);
//                    }
//                    catch (Exception e)
//                    {
//                        Logger.Error(e);
//                        throw;
//                    }
//                };
//                operation.IsGood = () =>
//                    operation.Getting >= operation.LowerTolerance && operation.Getting <= operation.UpperTolerance;
//                operation.CompliteWork = () =>
//                {
//                    if (!operation.IsGood())
//                    {
//                        var answer =
//                            UserItemOperation.ServicePack.MessageBox()
//                                             .Show($"Текущая точка {operation.Expected.Description} не проходит по допуску:\n" +
//                                                   $"Минимально допустимое значение {operation.LowerTolerance.Description}\n" +
//                                                   $"Максимально допустимое значение {operation.UpperTolerance.Description}\n" +
//                                                   $"Допустимое значение погрешности {operation.Error.Description}\n" +
//                                                   $"ИЗМЕРЕННОЕ значение {operation.Getting.Description}\n\n" +
//                                                   $"\nФАКТИЧЕСКАЯ погрешность {(operation.Expected - operation.Getting).Description}\n\n" +
//                                                   "Повторить измерение этой точки?",
//                                                   "Информация по текущему измерению",
//                                                   MessageButton.YesNo, MessageIcon.Question,
//                                                   MessageResult.Yes);

//                        if (answer == MessageResult.No) return Task.FromResult(true);
//                    }

//                    return Task.FromResult(operation.IsGood());
//                };

//                DataRow.Add(DataRow.IndexOf(operation) == -1
//                                ? operation
//                                : (BasicOperationVerefication<MeasPoint<Percent>>) operation.Clone());
//            }
//        }

//        #endregion
//    }

//    public class Oprobovanie_7_3_5_3_Chanel1 : Oprobovanie_7_3_5_3
//    {
//        public Oprobovanie_7_3_5_3_Chanel1(IUserItemOperation userItemOperation) : base(userItemOperation, 1)
//        {
//            flkCalib5522A = new Calib5522A();
//        }
//    }

//    public class Oprobovanie_7_3_5_3_Chanel2 : Oprobovanie_7_3_5_3
//    {
//        public Oprobovanie_7_3_5_3_Chanel2(IUserItemOperation userItemOperation) : base(userItemOperation, 2)
//        {
//            flkCalib5522A = new Calib5522A();
//        }
//    }
//}