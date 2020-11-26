﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using AP.Extension;
using AP.Math;
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
using DevExpress.Mvvm;
using DevExpress.Mvvm.UI;
using NLog;

namespace OWEN_TRM202


{
    public static class Helps
    {
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
    }

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

            DocumentName = "TRM202_protocolMP2007";

            var Chanel1Operation = new Operation8_4_ResistanceTermocoupleGost6651(this,1){Name = "Канал 1"};
            Chanel1Operation.Nodes.Add(new Operation8_4_ResistanceTermocoupleGost6651(this,1));
            Chanel1Operation.Nodes[0].Nodes.Add(new Operation8_4_Cu100_426_Poverka(this,1));
            Chanel1Operation.Nodes[0].Nodes.Add(new Operation8_4_Cu50_426_Poverka(this,1));
            Chanel1Operation.Nodes[0].Nodes.Add(new Operation8_4_Pt100_385_Poverka(this,1));
            Chanel1Operation.Nodes[0].Nodes.Add(new Operation8_4_Pt50_385_Poverka(this,1));
            Chanel1Operation.Nodes[0].Nodes.Add(new Operation8_4_TSM50M_428_Poverka(this,1));
            Chanel1Operation.Nodes[0].Nodes.Add(new Operation8_4_TSM100M_428_Poverka(this,1));
            Chanel1Operation.Nodes[0].Nodes.Add(new Operation8_4_TSP50P_391_Poverka(this,1));
            Chanel1Operation.Nodes[0].Nodes.Add(new Operation8_4_TSP100P_391_Poverka(this,1));

            var Chanel2Operation = new Operation8_4_ResistanceTermocoupleGost6651(this,2){Name = "Канал 2"};
            Chanel2Operation.Nodes.Add(new Operation8_4_ResistanceTermocoupleGost6651(this,2));
            Chanel2Operation.Nodes[0].Nodes.Add(new Operation8_4_Cu100_426_Poverka(this,  2));
            Chanel2Operation.Nodes[0].Nodes.Add(new Operation8_4_Cu50_426_Poverka(this,  2));
            Chanel2Operation.Nodes[0].Nodes.Add(new Operation8_4_Pt100_385_Poverka(this,  2));
            Chanel2Operation.Nodes[0].Nodes.Add(new Operation8_4_Pt50_385_Poverka(this,  2));
            Chanel2Operation.Nodes[0].Nodes.Add(new Operation8_4_TSM50M_428_Poverka(this,  2));
            Chanel2Operation.Nodes[0].Nodes.Add(new Operation8_4_TSM100M_428_Poverka(this,  2));
            Chanel2Operation.Nodes[0].Nodes.Add(new Operation8_4_TSP50P_391_Poverka(this,  2));
            Chanel2Operation.Nodes[0].Nodes.Add(new Operation8_4_TSP100P_391_Poverka(this,  2));

            UserItemOperation = new IUserItemOperationBase[]
            {
                new Oper1VisualTest(this), 
                //new Oprobovanie2007(this, 1),
                //new Oprobovanie2007(this, 2),
                //Chanel1Operation,
                Chanel2Operation
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
                    service.Entity = new Tuple<string, Assembly>("TRM_Visual_test", null);
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
                return null;
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

        public class Operation8_4_ResistanceTermocoupleGost6651 : ParagraphBase<MeasPoint<Temperature>>
        {
            #region Fields
            private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

            protected TRM202Device.in_t _coupleType;
            protected RangeStorage<PhysicalRange<Temperature>> MeasureRanges;
            protected MeasPoint<Temperature, Resistance>[] measPoints;

            protected TRM202DeviceUI trm202;
            protected  ushort _chanelNumber { get;  set; }
            protected Calib5522A flkCalib5522A;

            #endregion

            public Operation8_4_ResistanceTermocoupleGost6651(IUserItemOperation userItemOperation, ushort inChanelNumber) : base(userItemOperation)
            {
                _chanelNumber = inChanelNumber;
                Name = "Определение основной приведенной погрешности прибора со специализированным входом";
                Sheme = new ShemeImage
                {
                    Number = _chanelNumber,
                    FileName = $"TRM202_5522A_OprobovanieResistanceTermocouple_chanel{_chanelNumber}.jpg",
                    ExtendedDescription = "Соберите схему, как показано на рисунке."
                };
               
            }

            #region Methods

            protected override string GetReportTableName()
            {
                return null;
            }

            protected override DataTable FillData()
            {
                var dataTable = base.FillData();
                foreach (var row in DataRow)
                {
                    var dataRow = dataTable.NewRow();
                    var dds = row as BasicOperationVerefication<MeasPoint<Temperature>>;
                    if (dds == null) continue;
                    dataRow["Поверяемая точка"] = dds.Expected?.Description;
                    dataRow["Измеренное значение"] = dds.Getting?.Description;
                    dataRow["Минимально допустимое значение"] = dds.LowerTolerance?.Description;
                    dataRow["Максимально допустимое значение"] = dds.UpperTolerance?.Description;

                    if (dds.IsGood == null)
                        dataRow[4] = "не выполнено";
                    else
                        dataRow[4] = dds.IsGood() ? "Годен" : "Брак";
                    dataTable.Rows.Add(dataRow);
                }
                return dataTable;
            }

            protected override string[] GenerateDataColumnTypeObject()
            {
                return new string[]{"Поверяемая точка", "Измеренное значение", "Минимально допустимое значение","Максимально допустимое значение"};
            }

            protected override void InitWork(CancellationTokenSource token)
            {

                trm202 = (UserItemOperation.TestDevices.FirstOrDefault(q => q.SelectedDevice as TRM202DeviceUI != null)
                                           .SelectedDevice as IControlPannelDevice).Device as TRM202DeviceUI;

                if (trm202 == null || flkCalib5522A == null) return;

                trm202.StringConnection = GetStringConnect(trm202);
                flkCalib5522A.StringConnection ??= GetStringConnect(flkCalib5522A);

                base.InitWork(token);


                foreach (var point in measPoints)
                {
                    var operation = new BasicOperationVerefication<MeasPoint<Temperature>>();
                    operation.InitWork = async () =>
                    {
                        try
                        {
                            await Task.Run(() =>
                            {
                                //делаем предварительные настройка канала прибора

                                //1. задаем на каналах нужную характеристику 
                                var typeTermoCouple = BitConverter
                                                     .GetBytes((int)_coupleType).Where(a => a != 0).ToArray(); //выкидываем из массива лишние нули
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
                       
                        try
                        {
                            operation.Expected = new MeasPoint<Temperature>(point.MainPhysicalQuantity);
                            operation.ErrorCalculation = (measPoint, point1) =>
                            {
                                //считаем приведенную погрешность
                                return MeasureRanges.GetReducerTolMeasPoint(measPoint);
                            };

                            operation.UpperTolerance = operation.Expected + operation.Error;
                            operation.UpperTolerance.MainPhysicalQuantity.ChangeMultiplier(point.MainPhysicalQuantity
                                                                                                .Multiplier);

                            operation.LowerTolerance = operation.Expected - operation.Error;
                            operation.LowerTolerance.MainPhysicalQuantity.ChangeMultiplier(point.MainPhysicalQuantity
                                                                                                .Multiplier);
                            MeasPoint<Resistance> setPoint = new MeasPoint<Resistance>(point.AdditionalPhysicalQuantity);
                            flkCalib5522A.Out.Set.Resistance.SetValue(setPoint);
                            flkCalib5522A.Out.SetOutput(CalibrMain.COut.State.On);
                            Thread.Sleep(1500);
                            var measPoint = trm202.GetMeasValChanel(_chanelNumber);
                            flkCalib5522A.Out.SetOutput(CalibrMain.COut.State.Off);
                            MathStatistics.Round(ref measPoint, 1);
                            operation.Getting = new MeasPoint<Temperature>(measPoint);
                        }
                        catch (Exception e)
                        {
                            Logger.Error(e);
                            throw;
                        }

                        operation.IsGood = () => operation.Getting >= operation.LowerTolerance && operation.Getting <= operation.UpperTolerance; ;
                    };
                    operation.CompliteWork = () => Helps.HelpsCompliteWork(operation, UserItemOperation) ;


                    DataRow.Add(DataRow.IndexOf(operation) == -1
                                    ? operation
                                    : (BasicOperationVerefication<MeasPoint<Temperature>>)operation.Clone());
                }
               
            }

            #endregion
        }

        public class Operation8_4_Cu50_426_Poverka : Operation8_4_ResistanceTermocoupleGost6651
        {
            public Operation8_4_Cu50_426_Poverka(IUserItemOperation userItemOperation, ushort inChanel) : base(userItemOperation, inChanel)
            {
                trm202 = new TRM202DeviceUI();
               
                MeasureRanges = trm202.GetCu50_426RangeStorage;
                _coupleType = TRM202Device.in_t.r426;
                Name = _coupleType.GetStringValue();
                flkCalib5522A = new Calib5522A();
                
                measPoints = new MeasPoint<Temperature, Resistance>[]
                {
                    new MeasPoint<Temperature, Resistance>(-37,42.120M), 
                    new MeasPoint<Temperature, Resistance>(12.5M,52.662M),
                    new MeasPoint<Temperature, Resistance>(75M, 65.980M),
                    new MeasPoint<Temperature, Resistance>(137.5M,79.297M),
                    new MeasPoint<Temperature, Resistance>(187.5M, 89.952M)
                };
                
            }

            protected override string GetReportTableName()
            {
                return "FillTabBm8_4_Cu50_426";
            }
        }

        public class Operation8_4_Cu100_426_Poverka : Operation8_4_ResistanceTermocoupleGost6651
        {
            public Operation8_4_Cu100_426_Poverka(IUserItemOperation userItemOperation, ushort inChanel) : base(userItemOperation,inChanel)
            {
                trm202 = new TRM202DeviceUI();
                
                MeasureRanges = trm202.GetCu100_426RangeStorage;
                _coupleType = TRM202Device.in_t.r_426;
                Name = _coupleType.GetStringValue();
                flkCalib5522A = new Calib5522A();

                measPoints = new MeasPoint<Temperature, Resistance>[]
                {
                    new MeasPoint<Temperature, Resistance>(-37, 84.230M),
                    new MeasPoint<Temperature, Resistance>(12.5M, 105.325M),
                    new MeasPoint<Temperature, Resistance>(75, 131.960M), 
                    new MeasPoint<Temperature, Resistance>(137.5M, 158.595M), 
                    new MeasPoint<Temperature, Resistance>(187.5M, 179.905M)
                };
            }

            protected override string GetReportTableName()
            {
                return "FillTabBm8_4_Cu100_426";
            }
        }

        public class Operation8_4_Pt100_385_Poverka : Operation8_4_ResistanceTermocoupleGost6651
        {
            public Operation8_4_Pt100_385_Poverka(IUserItemOperation userItemOperation, ushort inChanel) : base(userItemOperation, inChanel)
            {
                trm202 = new TRM202DeviceUI();
                
                MeasureRanges = trm202.GetPt100_385RangeStorage;
                _coupleType = TRM202Device.in_t.r_385;
                Name = _coupleType.GetStringValue();
                flkCalib5522A = new Calib5522A();

                measPoints = new MeasPoint<Temperature, Resistance>[]
                {
                    new MeasPoint<Temperature, Resistance>(-152, 38.890M),
                    new MeasPoint<Temperature, Resistance>(37.5M, 114.575M),
                    new MeasPoint<Temperature, Resistance>(275,203.110M),
                    new MeasPoint<Temperature, Resistance>(512.5M,285.135M),
                    new MeasPoint<Temperature, Resistance>(702.5M, 346.055M)
                };
            }

            protected override string GetReportTableName()
            {
                return "FillTabBm8_4_Pt100_385";
            }
        }

        public class Operation8_4_Pt50_385_Poverka : Operation8_4_ResistanceTermocoupleGost6651
        {
            public Operation8_4_Pt50_385_Poverka(IUserItemOperation userItemOperation, ushort inChanel) : base(userItemOperation, inChanel)
            {
                trm202 = new TRM202DeviceUI();
                
                MeasureRanges = trm202.GetPt50_385RangeStorage;
                _coupleType = TRM202Device.in_t.r385;
                Name = _coupleType.GetStringValue();
                flkCalib5522A = new Calib5522A();

                measPoints = new MeasPoint<Temperature, Resistance>[]
                {
                    new MeasPoint<Temperature, Resistance>(-152, 19.445M),
                    new MeasPoint<Temperature, Resistance>(37.5M, 57.288M),
                    new MeasPoint<Temperature, Resistance>(275,101.555M),
                    new MeasPoint<Temperature, Resistance>(512.5M, 142.567M),
                    new MeasPoint<Temperature, Resistance>(702.5M, 173.027M)
                };
            }

            protected override string GetReportTableName()
            {
                return "FillTabBmOper8_4_Pt50_385";
            }
        }

        public class Operation8_4_TSM50M_428_Poverka : Operation8_4_ResistanceTermocoupleGost6651
        {
            public Operation8_4_TSM50M_428_Poverka(IUserItemOperation userItemOperation, ushort inChanel) : base(userItemOperation, inChanel)
            {
                trm202 = new TRM202DeviceUI();
                
                MeasureRanges = trm202.GetTSP50M_428RangeStorage;
                _coupleType = TRM202Device.in_t.r428;
                Name = _coupleType.GetStringValue();
                flkCalib5522A = new Calib5522A();

                measPoints = new MeasPoint<Temperature, Resistance>[]
                {
                    new MeasPoint<Temperature, Resistance>(-170, 12.570M),
                    new MeasPoint<Temperature, Resistance>(-92.5M, 29.960M),
                    new MeasPoint<Temperature, Resistance>(5,51.070M),
                    new MeasPoint<Temperature, Resistance>(102.5M,71.923M),
                    new MeasPoint<Temperature, Resistance>(180.5M, 88.605M), 
                };
            }

            protected override string GetReportTableName()
            {
                return "FillTabBmOper8_4_TSM50M_428";
            }
        }

        public class Operation8_4_TSM100M_428_Poverka : Operation8_4_ResistanceTermocoupleGost6651
        {
            public Operation8_4_TSM100M_428_Poverka(IUserItemOperation userItemOperation, ushort inChanel) : base(userItemOperation, inChanel)
            {
                trm202 = new TRM202DeviceUI();
                
                MeasureRanges = trm202.GetTSP100M_428RangeStorage;
                _coupleType = TRM202Device.in_t.r_428;
                Name = _coupleType.GetStringValue();
                flkCalib5522A = new Calib5522A();

                measPoints = new MeasPoint<Temperature, Resistance>[]
                {
                    new MeasPoint<Temperature, Resistance>(-170,25.140M),
                    new MeasPoint<Temperature, Resistance>(-92.5M,59.920M),
                    new MeasPoint<Temperature, Resistance>(5,102.140M), 
                    new MeasPoint<Temperature, Resistance>(102.5M, 143.845M),
                    new MeasPoint<Temperature, Resistance>(180.5M, 177.210M)
                };
            }

            protected override string GetReportTableName()
            {
                return "FillTabBmOper8_4_TSM100M_428";
            }
        }

        public class Operation8_4_TSP50P_391_Poverka : Operation8_4_ResistanceTermocoupleGost6651
        {
            public Operation8_4_TSP50P_391_Poverka(IUserItemOperation userItemOperation, ushort inChanel) : base(userItemOperation, inChanel)
            {
                trm202 = new TRM202DeviceUI();
                
                MeasureRanges = trm202.GetTSP50P_391RangeStorage;
                _coupleType = TRM202Device.in_t.r391;
                Name = _coupleType.GetStringValue();
                flkCalib5522A = new Calib5522A();

                measPoints = new MeasPoint<Temperature, Resistance>[]
                {
                    new MeasPoint<Temperature, Resistance>(-152, 18.970M),
                    new MeasPoint<Temperature, Resistance>(37.5M, 57.403M), 
                    new MeasPoint<Temperature, Resistance>(275, 102.375M), 
                    new MeasPoint<Temperature, Resistance>(512.5M, 144.055M), 
                    new MeasPoint<Temperature, Resistance>(702.5M, 174.955M)
                };
            }

            protected override string GetReportTableName()
            {
                return "FillTabBmOper8_4_TSP50P_391";
            }
        }

        public class Operation8_4_TSP100P_391_Poverka : Operation8_4_ResistanceTermocoupleGost6651
        {
            public Operation8_4_TSP100P_391_Poverka(IUserItemOperation userItemOperation, ushort inChanel) : base(userItemOperation, inChanel)
            {
                trm202 = new TRM202DeviceUI();
                
                MeasureRanges = trm202.GetTSP100P_391RangeStorage;
                _coupleType = TRM202Device.in_t.r_391;
                Name = _coupleType.GetStringValue();
                flkCalib5522A = new Calib5522A();

                measPoints = new MeasPoint<Temperature, Resistance>[]
                {
                    new MeasPoint<Temperature, Resistance>(-152, 37.940M), 
                    new MeasPoint<Temperature, Resistance>(37.5M, 114.805M), 
                    new MeasPoint<Temperature, Resistance>(275, 204.750M ), 
                    new MeasPoint<Temperature, Resistance>(512.5M, 288.110M), 
                    new MeasPoint<Temperature, Resistance>(702.5M, 349.910M)
                };
            }

            protected override string GetReportTableName()
            {
                return "FillTabBmOper8_4_TSP100P_391";
            }
        }
    }
}