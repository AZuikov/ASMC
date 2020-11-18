using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
                    Devices = new IDeviceBase[] {new Km300P()}, Description = "компаратор-калибратор универсальный"
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
                new Oprobovanie735Chanel1(this),
                new Oprobovanie735Chanel2(this)
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

        #endregion
    }

    public class Oprobovanie735Chanel1 : Oprobovanie_7_3_5
    {
        public Oprobovanie735Chanel1(IUserItemOperation userItemOperation) : base(userItemOperation, 1)
        {
        }
    }

    public class Oprobovanie735Chanel2 : Oprobovanie_7_3_5
    {
        public Oprobovanie735Chanel2(IUserItemOperation userItemOperation) : base(userItemOperation, 2)
        {
        }
    }

    public abstract class Oprobovanie_7_3_5 : ParagraphBase<MeasPoint<CelsiumGrad>>
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        #region Fields

        private readonly ushort _chanelNumber;

        #endregion

        #region Property

        protected TRM202DeviceUI trm202 { get; set; }

        #endregion

        /// <summary>
        /// Объект операции опробования.
        /// </summary>
        /// <param name = "userItemOperation"></param>
        /// <param name = "chanelNumb">Номер канала прибора 1 или 2.</param>
        public Oprobovanie_7_3_5(IUserItemOperation userItemOperation, ushort chanelNumb) : base(userItemOperation)
        {
            _chanelNumber = chanelNumb;
            Name = $"Опробование: 7.3.5 проверка исправности измерительных входов канала {_chanelNumber}";
            DataRow = new List<IBasicOperation<MeasPoint<CelsiumGrad>>>();
            Sheme = new ShemeImage
            {
                Number = _chanelNumber,
                FileName = $"TRM202_Oprobovanie735_chanel{_chanelNumber}.jpg",
                Description = "Соберите схему, как показано на рисунке."
            };
        }

        #region Methods

        protected override DataTable FillData()
        {
            var data = base.FillData();
            var dataRow = data.NewRow();
            if (DataRow.Count == 1)
            {
                var dds = DataRow[0] as BasicOperationVerefication<bool>;
                if (dds == null)
                    dataRow[0] = "";
                //ReSharper disable once PossibleNullReferenceException
                else if (dds.IsGood == null)
                    dataRow[0] = "не выполнено";
                else
                    dataRow[0] = dds.IsGood() ? "Соответствует" : dds.Comment;
                data.Rows.Add(dataRow);
            }

            return data;
        }

        /// <inheritdoc />
        protected override string[] GenerateDataColumnTypeObject()
        {
            return new[] {"Результат опробования"};
        }

        /// <inheritdoc />
        protected override string GetReportTableName()
        {
            return $"ITBmOprobovanie735{_chanelNumber}";
        }

        protected override void InitWork(CancellationTokenSource token)
        {
            base.InitWork(token);
            //нужно проверсти инициализацию
            if (UserItemOperation != null)
            {
                trm202 = (UserItemOperation.TestDevices.FirstOrDefault(q => q.SelectedDevice as TRM202DeviceUI != null)
                                           .SelectedDevice as IControlPannelDevice).Device as TRM202DeviceUI;
                trm202.StringConnection = GetStringConnect(trm202);
                
                var operation = new BasicOperationVerefication<MeasPoint<CelsiumGrad>>();
                operation.InitWork = async () =>
                {
                    try
                    {
                        await Task.Run(() =>
                        {
                            //делаем предварительные настройка канала прибора

                            //1. задаем на каналах нужную характеристику НСХ 50М (W100 = 1,4280)
                            var typeTermoCouple = BitConverter.GetBytes((int) TRM202Device.in_t.r428);
                            ((TRM202Device)trm202.Device).WriteParametrToTRM(TRM202Device.Parametr.InT, new byte[]{9}, _chanelNumber);
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
                    operation.Expected = new MeasPoint<CelsiumGrad>(5M);
                    operation.ErrorCalculation = (inA, inB) => new MeasPoint<CelsiumGrad>(0.9M);
                    operation.LowerTolerance = operation.Expected - operation.Error;
                    operation.UpperTolerance = operation.Expected + operation.Error;
                    operation.Getting = new MeasPoint<CelsiumGrad>(trm202.GetMeasValChanel(_chanelNumber));
                };

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
                                : (BasicOperationVerefication<MeasPoint<CelsiumGrad>>) operation.Clone());
            }
        }

        #endregion
    }
}