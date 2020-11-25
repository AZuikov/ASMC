using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using AP.Extension;
using AP.Utils.Data;
using ASMC.Common.ViewModel;
using ASMC.Core.Model;
using ASMC.Core.UI;
using ASMC.Data.Model;
using ASMC.Data.Model.Interface;
using ASMC.Data.Model.PhysicalQuantity;
using ASMC.Devices.IEEE;
using ASMC.Devices.IEEE.Fluke.Calibrator;
using ASMC.Devices.OWEN;
using ASMC.Devices.Port.IZ_Tech;
using ASMC.Devices.UInterface.TRM.ViewModel;
using DevExpress.Mvvm.UI;
using NLog;

namespace OWEN_TRM202
{
    internal class OWEN_TRM202_MP2007_Plugin : Program<Operation2007>
    {
        public OWEN_TRM202_MP2007_Plugin(ServicePack service) : base(service)
        {
            Grsi = "32478-06 (МП2007)";
            Type = "ТРМ202";
        }
    }

    public class Operation2007 : OperationMetrControlBase
    {
        //определяет какие типы проверок доступны для СИ: поверка первичная/переодическая, калибровка, adjustment.
        public Operation2007(ServicePack servicePac)
        {
            //это операция первичной поверки
            UserItemOperationPrimaryVerf = new OpertionFirsVerf2007(servicePac);
            //здесь периодическая поверка, но набор операций такой же
            UserItemOperationPeriodicVerf = UserItemOperationPrimaryVerf;
        }
    }

    public class OpertionFirsVerf2007 : Operation
    {
        public OpertionFirsVerf2007(ServicePack servicePack) : base(servicePack)
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
                new Oprobovanie2007(this, 1),
                new Oprobovanie2007(this, 2),
                new Operation8_4(this)
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

        public class Oprobovanie2007 : ParagraphBase<bool>
        {
            private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

            #region Fields

            private readonly ushort _chanelNumber;

            #endregion

            #region Property

            protected Calib5522A flkCalib5522A { get; set; }

            protected TRM202DeviceUI trm202 { get; set; }

            #endregion

            public Oprobovanie2007(IUserItemOperation userItemOperation, ushort chanelNumb) : base(userItemOperation)
            {
                _chanelNumber = chanelNumb;
                Name = $"Опробование канала {_chanelNumber}";
                DataRow = new List<IBasicOperation<bool>>();
                flkCalib5522A = new Calib5522A();
            }

            #region Methods

            protected override DataTable FillData()
            {
                return base.FillData();
            }

            protected override string[] GenerateDataColumnTypeObject()
            {
                return new[] {"Результат"};
            }

            protected override string GetReportTableName()
            {
                throw new NotImplementedException();
            }

            protected override void InitWork(CancellationTokenSource token)
            {
                base.InitWork(token);
                trm202 = (UserItemOperation.TestDevices.FirstOrDefault(q => q.SelectedDevice as TRM202DeviceUI != null)
                                           .SelectedDevice as IControlPannelDevice).Device as TRM202DeviceUI;

                if (trm202 == null || flkCalib5522A == null) return;

                trm202.StringConnection = GetStringConnect(trm202);
                if (UserItemOperation != null)
                {
                    var operation = new BasicOperationVerefication<bool>();
                    operation.InitWork = async () =>
                    {
                        try
                        {
                            await Task.Run(() =>
                            {
                                //делаем предварительные настройка канала прибора

                                //1. задаем на каналах нужную характеристику НСХ 50М (W100 = 1,4280)
                                var typeTermoCouple = BitConverter
                                                     .GetBytes((int) TRM202Device.in_t.r428).Where(a => a != 0)
                                                     .ToArray();
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
                    operation.IsGood = () => true;
                    operation.CompliteWork = () => { return Task.FromResult(operation.IsGood()); };
                    DataRow.Add(DataRow.IndexOf(operation) == -1
                                    ? operation
                                    : (BasicOperationVerefication<bool>) operation.Clone());
                }
            }

            #endregion
        }

        public class Operation8_4 : ParagraphBase<MeasPoint<Temperature>>
        {
            #region Fields

            private string _coupleType;

            #endregion

            public Operation8_4(IUserItemOperation userItemOperation) : base(userItemOperation)
            {
                Name = "Тестирование термопары типа ХЗ";
            }

            #region Methods

            protected override string GetReportTableName()
            {
                throw new NotImplementedException();
            }

            protected override void InitWork(CancellationTokenSource token)
            {
                base.InitWork(token);
                var operation = new BasicOperationVerefication<MeasPoint<Temperature>>();
                var vm = new TermocoupleViewModel();

                var tableFormat1 = new TableViewModel.SettingTableViewModel();
                tableFormat1.IsHorizontal = true;
                tableFormat1.CellFormat = MeasureUnits.degC.GetStringValue();

                var tableFormat2 = new TableViewModel.SettingTableViewModel();
                tableFormat2.IsHorizontal = true;
                tableFormat2.CellFormat = MeasureUnits.Resistance.GetStringValue();

                var table1 = TableViewModel.CreateTable("Диапазон измерения температур",
                                                        new[]
                                                        {
                                                            "Начало диапазона (0 %)", "Конец диапазона (100 %)"
                                                            
                                                        }, tableFormat1);
                var table2 = TableViewModel.CreateTable("Диапазон сопротивлений датчика",
                                                        new[]
                                                        {
                                                            "Начало диапазона (0 %)", "Конец диапазона (100 %)",
                                                            
                                                        }, tableFormat2);

                vm.Content.Add(table1);
                vm.Content.Add(table2);
                
                var window = UserItemOperation.ServicePack.FreeWindow() as SelectionService;
                window.DocumentType = "TermocupleView";
                window.ViewLocator = new ViewLocator(Assembly.GetExecutingAssembly());
                window.ViewModel = vm;
                window.Show();

                var returnCellsTable1 = vm.Content[0].Cells;
                
                decimal numericParseValue;
                decimal.TryParse((string) returnCellsTable1[0].Value, out numericParseValue);
                var StartDegreasRange = new MeasPoint<Temperature>(numericParseValue);
                decimal.TryParse((string) returnCellsTable1[1].Value, out numericParseValue);
                var EndDegreasRange = new MeasPoint<Temperature>(numericParseValue);
                var pointDegreasArr = EndDegreasRange.GetArayMeasPointsInParcent(StartDegreasRange, 5, 25, 50, 75, 95).ToArray();

                var returnCellsTable2 = vm.Content[1].Cells;
                decimal.TryParse((string)returnCellsTable2[0].Value, out numericParseValue);
                var StartResistenceRange = new MeasPoint<Temperature>(numericParseValue);
                decimal.TryParse((string)returnCellsTable2[1].Value, out numericParseValue);
                var EndResistanceRange = new MeasPoint<Temperature>(numericParseValue);
                var pointResistanceArr = EndResistanceRange.GetArayMeasPointsInParcent(StartResistenceRange, 5, 25, 50, 75, 95).ToArray();
                
                DataRow.Add(DataRow.IndexOf(operation) == -1
                                ? operation
                                : (BasicOperationVerefication<MeasPoint<Temperature>>) operation.Clone());
            }

            #endregion
        }
    }
}