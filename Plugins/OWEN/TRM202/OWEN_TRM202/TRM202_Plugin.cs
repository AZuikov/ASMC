using AP.Math;
using ASMC.Core.Model;
using ASMC.Data.Model;
using ASMC.Data.Model.Interface;
using ASMC.Data.Model.PhysicalQuantity;
using ASMC.Devices.IEEE;
using ASMC.Devices.OWEN;
using ASMC.Devices.Port.IZ_Tech;
using ASMC.Devices.Port.ZipNu4Pribor;
using ASMC.Devices.UInterface.TRM.ViewModel;
using DevExpress.Mvvm;
using NLog;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ASMC.Devices.IEEE.Fluke.Calibrator;

namespace OWEN_TRM202
{
    public class TRM202_Plugin : Program<Operation>
    {
        public TRM202_Plugin(ServicePack service) : base(service)
        {
            Grsi = "32478-06";
            Type = "ТРМ202";
        }
    }

    public class Operation : OperationMetrControlBase
    {
        //определяет какие типы проверок доступны для СИ: поверка первичная/переодическая, калибровка, adjustment.
        public Operation(ServicePack servicePac)
        {
            //это операция первичной поверки
            UserItemOperationPrimaryVerf = new OpertionFirsVerf(servicePac);
            //здесь периодическая поверка, но набор операций такой же
            UserItemOperationPeriodicVerf = UserItemOperationPrimaryVerf;
        }
    }

    public class OpertionFirsVerf : ASMC.Core.Model.Operation
    {
        public OpertionFirsVerf(ServicePack servicePack) : base(servicePack)
        {
            //Необходимые устройства
            ControlDevices = new IDeviceUi[]
            {
                new Device
                {
                    Devices = new IDeviceBase[] {new Calib5522A()}, Description = "Многофунциональный калибратор"
                },
                new Device
                {
                    Devices = new IDeviceBase[] {new MIT_8()}, Description = "измеритель температуры прецизионный"
                }
            };
            TestDevices = new IDeviceUi[]
            {
                new Device
                {
                    Devices = new IDeviceBase[] {new TRM202DeviceUI()},
                    Description = "измеритель температуры прецизионный"
                }
            };
            
            DocumentName = "TRM202_protocol";

            UserItemOperation = new IUserItemOperationBase[]
            {
               // new Oprobovanie735Chanel2(this),
                new Oprobovanie_7_3_5_3_Chanel2(this)
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

        #endregion Methods
    }

    public class Oper1VisualTest : ParagraphBase<bool>
    {
        public Oper1VisualTest(IUserItemOperation userItemOperation) : base(userItemOperation)
        {
            Name = "Внешний осмотр";
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

        protected override string GetReportTableName()
        {
            throw new NotImplementedException();
        }

        #endregion Methods
    }

    public class Oprobovanie735Chanel1 : Oprobovanie_7_3_5_1
    {
        public Oprobovanie735Chanel1(IUserItemOperation userItemOperation) : base(userItemOperation, 1)
        {
        }
    }

    public class Oprobovanie735Chanel2 : Oprobovanie_7_3_5_1
    {
        public Oprobovanie735Chanel2(IUserItemOperation userItemOperation) : base(userItemOperation, 2)
        {
        }
    }

    public abstract class Oprobovanie_7_3_5_1 : ParagraphBase<MeasPoint<Temperature>>
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        #region Fields

        private readonly ushort _chanelNumber;

        #endregion Fields

        #region Property

        protected TRM202DeviceUI trm202 { get; set; }

        #endregion Property

        /// <summary>
        /// Объект операции опробования.
        /// </summary>
        /// <param name = "userItemOperation"></param>
        /// <param name = "chanelNumb">Номер канала прибора 1 или 2.</param>
        public Oprobovanie_7_3_5_1(IUserItemOperation userItemOperation, ushort chanelNumb) : base(userItemOperation)
        {
            _chanelNumber = chanelNumb;
            Name = $"Опробование: 7.3.5.1 проверка исправности измерительных входов канала {_chanelNumber}";
            DataRow = new List<IBasicOperation<MeasPoint<Temperature>>>();
            Sheme = new ShemeImage
            {
                Number = _chanelNumber,
                FileName = $"TRM202_Oprobovanie7351_chanel{_chanelNumber}.jpg",
                Description = "Соберите схему, как показано на рисунке."
            };
        }

        #region Methods

        protected override DataTable FillData()
        {
            var dataTable = base.FillData();
            foreach (var row in DataRow)
            {
                var dataRow = dataTable.NewRow();
                var dds = DataRow[0] as BasicOperationVerefication<MeasPoint<Temperature>>;
                if (dds == null) continue;
                dataRow[0] = dds.Getting?.Description;
                dataRow[1] = dds.LowerTolerance?.Description;
                dataRow[2] = dds.UpperTolerance?.Description;
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
            return new[] { "Показания прибора при величине сопротивления 51,07 Ом", "Нижний предел", "Верхний предел", "Результат" };
        }

        /// <inheritdoc />
        protected override string GetReportTableName()
        { //AP>REPORT>UTILS есть перечисление с именем таблицы, от типа имени закладки зависит механизм заполнения
            return $"ITBmOprobovanie7351{_chanelNumber}";
        }

        protected override void InitWork(CancellationTokenSource token)
        {
            base.InitWork(token);

            if (UserItemOperation != null)
            {
                trm202 = (UserItemOperation.TestDevices.FirstOrDefault(q => q.SelectedDevice as TRM202DeviceUI != null)
                                           .SelectedDevice as IControlPannelDevice).Device as TRM202DeviceUI;
                trm202.StringConnection = GetStringConnect(trm202);

                var operation = new BasicOperationVerefication<MeasPoint<Temperature>>();
                operation.InitWork = async () =>
                {
                    try
                    {
                        await Task.Run(() =>
                        {
                            //делаем предварительные настройка канала прибора

                            //1. задаем на каналах нужную характеристику НСХ 50М (W100 = 1,4280)
                            byte[] typeTermoCouple = BitConverter.GetBytes((int)TRM202Device.in_t.r428).Where(a => a != 0).ToArray();
                            trm202.WriteParametrToTRM(TRM202Device.Parametr.InT, typeTermoCouple, _chanelNumber);
                            //2. ставим сдвиги и наклоны характеристик
                            trm202.WriteFloat24Parametr(TRM202Device.Parametr.SH, 0, _chanelNumber);
                            trm202.WriteFloat24Parametr(TRM202Device.Parametr.KU, 1, _chanelNumber);
                            //3. ставим полосы фильтров и постоянную времени фильтра
                            trm202.WriteFloat24Parametr(TRM202Device.Parametr.Fb, 0, _chanelNumber);
                            trm202.WriteFloat24Parametr(TRM202Device.Parametr.InF, 0, _chanelNumber);
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
                    operation.Expected = new MeasPoint<Temperature>(5M);
                    operation.ErrorCalculation = (inA, inB) => new MeasPoint<Temperature>(0.9M);
                    operation.LowerTolerance = operation.Expected - operation.Error;
                    operation.UpperTolerance = operation.Expected + operation.Error;
                    decimal measPoint = trm202.GetMeasValChanel(_chanelNumber);
                    int mantisa = MathStatistics.GetMantissa(operation.Error.MainPhysicalQuantity.Value);
                    MathStatistics.Round(ref measPoint, mantisa);
                    operation.Getting = new MeasPoint<Temperature>(measPoint);
                };

                operation.IsGood = () => operation.Getting >= operation.LowerTolerance && operation.Getting <= operation.UpperTolerance;

                operation.CompliteWork = () =>
                {
                    if (!operation.IsGood())
                    {
                        var answer =
                            UserItemOperation.ServicePack.MessageBox().Show(operation +
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
                                : (BasicOperationVerefication<MeasPoint<Temperature>>)operation.Clone());
            }
        }

        #endregion Methods
    }

    public class Oprobovanie_7_3_5_3_Chanel1 : Oprobovanie_7_3_5_3
    {
        public Oprobovanie_7_3_5_3_Chanel1(IUserItemOperation userItemOperation) : base(userItemOperation, 1)
        {
        }
    }

    public class Oprobovanie_7_3_5_3_Chanel2 : Oprobovanie_7_3_5_3
    {
        public Oprobovanie_7_3_5_3_Chanel2(IUserItemOperation userItemOperation) : base(userItemOperation, 2)
        {
        }
    }

    public abstract class Oprobovanie_7_3_5_3 : ParagraphBase<MeasPoint<Percent>>
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        #region Fields

        private readonly ushort _chanelNumber;

        #endregion Fields

        #region Property
        
        protected TRM202DeviceUI trm202 { get; set; }
        
        protected Calib5522A flkCalib5522A { get; set; }

        #endregion Property

        protected Oprobovanie_7_3_5_3(IUserItemOperation userItemOperation, ushort chanelNumb) : base(userItemOperation)
        {
            _chanelNumber = chanelNumb;
            Name = $"Опробование: 7.3.5.3 Проверка исправности входов, работающих с унифицированным сигналом постоянного напряжения и термопарами (канал {_chanelNumber})";
            DataRow = new List<IBasicOperation<MeasPoint<Percent>>>();
            Sheme = new ShemeImage
            {
                Number = _chanelNumber,
                FileName = $"TRM202_Oprobovanie7353_chanel{_chanelNumber}.jpg",
                Description = "Соберите схему, как показано на рисунке."
            };

            flkCalib5522A = new Calib5522A();
        }

        /// <inheritdoc />
        protected override string[] GenerateDataColumnTypeObject()
        {
            return new[] { "Показания прибора при напряжении 250 мВ", "Нижний предел", "Верхний предел", "Результат" };
        }

        protected override string GetReportTableName()
        { //AP>REPORT>UTILS есть перечисление с именем таблицы, от типа имени закладки зависит механизм заполнения
            return $"ITBmOprobovanie7353{_chanelNumber}";
        }

        protected override void InitWork(CancellationTokenSource token)
        {
            base.InitWork(token);

            trm202 = (UserItemOperation.TestDevices.FirstOrDefault(q => q.SelectedDevice as TRM202DeviceUI != null)
                                       .SelectedDevice as IControlPannelDevice).Device as TRM202DeviceUI;
            trm202.StringConnection = GetStringConnect(trm202);

            flkCalib5522A.StringConnection ??= GetStringConnect(flkCalib5522A);

            if (trm202 == null || flkCalib5522A == null) return;

            if (UserItemOperation != null)
            {
                

                var operation = new BasicOperationVerefication<MeasPoint<Percent>>();
                operation.InitWork = async () =>
                {
                    try
                    {
                       
                        await Task.Run(() => { flkCalib5522A.Out.SetOutput(CalibrMain.COut.State.Off); });

                        await Task.Run(() =>
                        {
                            //делаем предварительные настройка канала прибора

                            //1. задаем на каналах нужную характеристику работы с унифицированным сигналом 0 … 1,0 В
                            byte[] typeTermoCouple = BitConverter.GetBytes((int)TRM202Device.in_t.U0_1).Where(a => a != 0).ToArray();
                            trm202.WriteParametrToTRM(TRM202Device.Parametr.InT, typeTermoCouple, _chanelNumber);
                            //2. ставим сдвиги и наклоны характеристик
                            trm202.WriteFloat24Parametr(TRM202Device.Parametr.SH, 0, _chanelNumber);
                            trm202.WriteFloat24Parametr(TRM202Device.Parametr.KU, 1, _chanelNumber);
                            //3. ставим полосы фильтров и постоянную времени фильтра
                            trm202.WriteFloat24Parametr(TRM202Device.Parametr.Fb, 0, _chanelNumber);
                            trm202.WriteFloat24Parametr(TRM202Device.Parametr.InF, 0, _chanelNumber);
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
                    operation.Expected = new MeasPoint<Percent>(25M);
                    operation.ErrorCalculation = (inA, inB) => new MeasPoint<Percent>(0.5M);
                    operation.LowerTolerance = operation.Expected - operation.Error;
                    operation.UpperTolerance = operation.Expected + operation.Error;
                    MeasPoint<Voltage> setPoint = new MeasPoint<Voltage>(operation
                                                                        .Expected.MainPhysicalQuantity
                                                                        .GetNoramalizeValueToSi() / 100);
                    flkCalib5522A.Out.Set.Voltage.Dc.SetValue(setPoint);
                    flkCalib5522A.Out.SetOutput(CalibrMain.COut.State.On);
                    Thread.Sleep(2000);
                    decimal measPoint = trm202.GetMeasValChanel(_chanelNumber);
                    flkCalib5522A.Out.SetOutput(CalibrMain.COut.State.Off);
                    int mantisa = MathStatistics.GetMantissa(operation.Error.MainPhysicalQuantity.Value);
                    MathStatistics.Round(ref measPoint, mantisa);
                    operation.Getting = new MeasPoint<Percent>(measPoint);
                };
                operation.IsGood = () => operation.Getting >= operation.LowerTolerance && operation.Getting <= operation.UpperTolerance;
                operation.CompliteWork = () =>
                {
                    if (!operation.IsGood())
                    {
                        var answer =
                            UserItemOperation.ServicePack.MessageBox().Show($"Текущая точка {operation.Expected.Description} не проходит по допуску:\n" +
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

                    return Task.FromResult(operation.IsGood());
                };

                DataRow.Add(DataRow.IndexOf(operation) == -1
                                ? operation
                                : (BasicOperationVerefication<MeasPoint<Percent>>)operation.Clone());
            }
        }
    }
}