using System;
using System.Collections.Generic;
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
using ASMC.Devices.IEEE.Fluke.Calibrator;
using ASMC.Devices.OWEN;
using ASMC.Devices.Port.OWEN;
using ASMC.Devices.UInterface.TRM.ViewModel;
using DevExpress.Mvvm;
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
                    Devices = new IDeviceRemote[] { new Calibr_9100(), new Calib5522A() },
                    Description = "Многофунциональный калибратор"

                }
            };
            TestDevices = new IDeviceUi[]
            {
                new Device
                {
                    Devices = new IDeviceRemote[] {new TRM202DeviceUI()},
                    Description = "измеритель температуры прецизионный"
                }
            };

            DocumentName = "TRM202_protocolMP2007";

            var Chanel1Operation = new Operation8_4_ResistanceTermocoupleGost6651(this, 1) { Name = "Канал 1" };
            Chanel1Operation.Nodes.Add(new Operation8_4_Cu100_426_Poverka(this, 1));
            Chanel1Operation.Nodes.Add(new Operation8_4_Cu50_426_Poverka(this, 1));
            Chanel1Operation.Nodes.Add(new Operation8_4_Pt100_385_Poverka(this, 1));
            Chanel1Operation.Nodes.Add(new Operation8_4_Pt50_385_Poverka(this, 1));
            Chanel1Operation.Nodes.Add(new Operation8_4_TSM50M_428_Poverka(this, 1));
            Chanel1Operation.Nodes.Add(new Operation8_4_TSM100M_428_Poverka(this, 1));
            Chanel1Operation.Nodes.Add(new Operation8_4_TSP50P_391_Poverka(this, 1));
            Chanel1Operation.Nodes.Add(new Operation8_4_TSP100P_391_Poverka(this, 1));
            //пункт меьтодики 2007 года. 8.5.1.1
            //Chanel1Operation.Nodes.Add(new Operation8_4_TSM50M_428_Poverka_8_5_1_1(this, 1));
            Chanel1Operation.Nodes.Add(new Operation8_4_UnificSignal_0_1_Poverka(this, 1));
            //пункт меьтодики 2007 года. 8.5.1.3
            //Chanel1Operation.Nodes.Add(new Operation8_4_UnificSignal_0_1_Poverka_8_5_1_3(this, 1));
            Chanel1Operation.Nodes.Add(new Operation8_4_UnificSignal_50_50_Poverka(this, 1));
            Chanel1Operation.Nodes.Add(new Operation8_4_UnificSignal_0_5mA_Poverka(this, 1));
            Chanel1Operation.Nodes.Add(new Operation8_4_UnificSignal_0_20mA_Poverka(this, 1));
            //пункт меьтодики 2007 года. 8.5.1.2
            //Chanel1Operation.Nodes.Add(new Operation8_4_UnificSignal_0_20mA_Poverka_8_5_1_2(this, 1));
            Chanel1Operation.Nodes.Add(new Operation8_4_UnificSignal_4_20mA_Poverka(this, 1));
            Chanel1Operation.Nodes.Add(new Operation8_4_Type_A1_Poverka(this, 1));
            Chanel1Operation.Nodes.Add(new Operation8_4_Type_A2_Poverka(this, 1));
            Chanel1Operation.Nodes.Add(new Operation8_4_Type_A3_Poverka(this, 1));
            Chanel1Operation.Nodes.Add(new Operation8_4_Type_L_Poverka(this, 1));
            Chanel1Operation.Nodes.Add(new Operation8_4_Type_J_Poverka(this, 1));
            Chanel1Operation.Nodes.Add(new Operation8_4_Type_N_Poverka(this, 1));
            Chanel1Operation.Nodes.Add(new Operation8_4_Type_K_Poverka(this, 1));
            // //пункт меьтодики 2007 года. 8.5.1.4
            //Chanel1Operation.Nodes.Add(new Operation8_4_Type_K_Poverka_8_5_1_4(this, 1));
            Chanel1Operation.Nodes.Add(new Operation8_4_Type_S_Poverka(this, 1));
            Chanel1Operation.Nodes.Add(new Operation8_4_Type_R_Poverka(this, 1));
            Chanel1Operation.Nodes.Add(new Operation8_4_Type_B_Poverka(this, 1));
            Chanel1Operation.Nodes.Add(new Operation8_4_Type_T_Poverka(this, 1));


            var Chanel2Operation = new Operation8_4_ResistanceTermocoupleGost6651(this, 2) { Name = "Канал 2" };
            Chanel2Operation.Nodes.Add(new Operation8_4_Cu100_426_Poverka(this, 2));
            Chanel2Operation.Nodes.Add(new Operation8_4_Cu50_426_Poverka(this, 2));
            Chanel2Operation.Nodes.Add(new Operation8_4_Pt100_385_Poverka(this, 2));
            Chanel2Operation.Nodes.Add(new Operation8_4_Pt50_385_Poverka(this, 2));
            Chanel2Operation.Nodes.Add(new Operation8_4_TSM50M_428_Poverka(this, 2));
            Chanel2Operation.Nodes.Add(new Operation8_4_TSM100M_428_Poverka(this, 2));
            Chanel2Operation.Nodes.Add(new Operation8_4_TSP50P_391_Poverka(this, 2));
            Chanel2Operation.Nodes.Add(new Operation8_4_TSP100P_391_Poverka(this, 2));
            //пункт меьтодики 2007 года. 8.5.1.1
            //Chanel2Operation.Nodes.Add(new Operation8_4_TSM50M_428_Poverka_8_5_1_1(this, 2));
            Chanel2Operation.Nodes.Add(new Operation8_4_UnificSignal_0_1_Poverka(this, 2));
            //пункт меьтодики 2007 года. 8.5.1.3
            //Chanel2Operation.Nodes.Add(new Operation8_4_UnificSignal_0_1_Poverka_8_5_1_3(this, 2));
            Chanel2Operation.Nodes.Add(new Operation8_4_UnificSignal_50_50_Poverka(this, 2));
            Chanel2Operation.Nodes.Add(new Operation8_4_UnificSignal_0_5mA_Poverka(this, 2));
            Chanel2Operation.Nodes.Add(new Operation8_4_UnificSignal_0_20mA_Poverka(this, 2));
            //пункт меьтодики 2007 года. 8.5.1.2
            //Chanel2Operation.Nodes.Add(new Operation8_4_UnificSignal_0_20mA_Poverka_8_5_1_2(this, 2));
            Chanel2Operation.Nodes.Add(new Operation8_4_UnificSignal_4_20mA_Poverka(this, 2));
            Chanel2Operation.Nodes.Add(new Operation8_4_Type_A1_Poverka(this, 2));
            Chanel2Operation.Nodes.Add(new Operation8_4_Type_A2_Poverka(this, 2));
            Chanel2Operation.Nodes.Add(new Operation8_4_Type_A3_Poverka(this, 2));
            Chanel2Operation.Nodes.Add(new Operation8_4_Type_L_Poverka(this, 2));
            Chanel2Operation.Nodes.Add(new Operation8_4_Type_J_Poverka(this, 2));
            Chanel2Operation.Nodes.Add(new Operation8_4_Type_N_Poverka(this, 2));
            Chanel2Operation.Nodes.Add(new Operation8_4_Type_K_Poverka(this, 2));
            // //пункт меьтодики 2007 года. 8.5.1.4
            //Chanel2Operation.Nodes.Add(new Operation8_4_Type_K_Poverka_8_5_1_4(this, 2));
            Chanel2Operation.Nodes.Add(new Operation8_4_Type_S_Poverka(this, 2));
            Chanel2Operation.Nodes.Add(new Operation8_4_Type_R_Poverka(this, 2));
            Chanel2Operation.Nodes.Add(new Operation8_4_Type_B_Poverka(this, 2));
            Chanel2Operation.Nodes.Add(new Operation8_4_Type_T_Poverka(this, 2));


            UserItemOperation = new IUserItemOperationBase[]
            {
                new Oper1VisualTest(this),
                new Oper2IsolationTest(this),
                new Oprobovanie2007(this, 1),
                new Oprobovanie2007(this, 2),
                Chanel1Operation,
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

        #endregion Methods

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
                    if (dds != null)
                    {
                        dataRow[0] = dds.Getting ? "Соответствует" : dds?.Comment;
                        data.Rows.Add(dataRow);
                    }

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
                //сообщение о необходимости предварительно сохранить настройки прибора
                Helps.AttentionWindow(UserItemOperation);

                base.InitWork(token);
                var operation = new BasicOperation<bool>();
                operation.Expected = true;
                operation.IsGood = () => Equals(operation.Getting, operation.Expected);
                operation.InitWorkAsync = () =>
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

                operation.CompliteWorkAsync = () => { return Task.FromResult(true); };
                DataRow.Add(operation);
            }

            #endregion Methods
        }

        public class Oper2IsolationTest : ParagraphBase<bool>
        {
            public Oper2IsolationTest(IUserItemOperation userItemOperation) : base(userItemOperation)
            {
                Name = "Проверка электрического сопротивления изоляции";
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
                    if (dds != null)
                    {
                        dataRow[0] = dds.Getting ? "соответствует" : $"не соответствует: {dds?.Comment}";
                        data.Rows.Add(dataRow);
                    }

                }

                return data;
            }

            /// <inheritdoc />
            protected override string[] GenerateDataColumnTypeObject()
            {
                return new[] { "Результат теста изоляции" };
            }

            /// <inheritdoc />
            protected override string GetReportTableName()
            {
                return "ITBmIsolationTest";
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
                    service.Entity = new Tuple<string, Assembly>("TRM_IsolationTest", null);
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

            #endregion Methods
        }


        public class Oprobovanie2007 : ParagraphBase<bool>
        {
            private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

            #region Fields

            private readonly ushort _chanelNumber;

            #endregion Fields

            #region Property

            protected TRM202DeviceUI trm202 { get; set; }

            #endregion Property

            public Oprobovanie2007(IUserItemOperation userItemOperation, ushort chanelNumb) : base(userItemOperation)
            {
                _chanelNumber = chanelNumb;
                Name = $"Опробование канала {_chanelNumber}";
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
                    if (dds != null)
                    {
                        dataRow[0] = dds.Getting ? "проведено успешно." : dds?.Comment;
                        data.Rows.Add(dataRow);
                    }

                }

                return data;
            }

            protected override string[] GenerateDataColumnTypeObject()
            {
                return new[] { "Результат" };
            }

            protected override string GetReportTableName()
            {
                return $"ITBmOprobovanie_ch{_chanelNumber}";
            }

            protected override void InitWork(CancellationTokenSource token)
            {
                base.InitWork(token);
                trm202 = (UserItemOperation.TestDevices.FirstOrDefault(q => q.SelectedDevice as TRM202DeviceUI != null)
                                           .SelectedDevice as IControlPannelDevice).Device as TRM202DeviceUI;

                if (trm202 == null) return;

                trm202.StringConnection = GetStringConnect(trm202);
                if (UserItemOperation != null)
                {
                    var operation = new BasicOperationVerefication<bool>();
                    operation.InitWorkAsync = async () =>
                    {
                        try
                        {
                            await Task.Run(() =>
                            {
                                //делаем предварительные настройка канала прибора

                                //1. задаем на каналах нужную характеристику НСХ 50М (W100 = 1,4280)
                                var typeTermoCouple = BitConverter
                                                     .GetBytes((int)TRM202Device.in_t.r428).Where(a => a != 0)
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
                    operation.CompliteWorkAsync = () => { return Task.FromResult(operation.IsGood()); };
                    DataRow.Add(operation);
                }
            }

            #endregion Methods
        }

        public class Operation8_4_HCX_TermocoupleGost8_585 : BaseMeasureOperation<Temperature>
        {
            private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
            
            public Operation8_4_HCX_TermocoupleGost8_585(IUserItemOperation userItemOperation, ushort inChanelNumber) :
                base(userItemOperation)
            {
                _chanelNumber = inChanelNumber;
                Name = "Определение основной приведенной погрешности прибора со специализированным входом";
                Sheme = new SchemeImage
                {
                    Number = _chanelNumber + 10,
                    FileName = $"TRM202_TC_out_chanel{_chanelNumber}.jpg",
                    ExtendedDescription = "Соберите схему, как показано на рисунке."
                };
            }

            #region Methods



            protected override string[] GenerateDataColumnTypeObject()
            {
                return new[]
                {
                    "Поверяемая точка", "Измеренное значение",
                    "Основная приведенная погрешность",
                    "Допустимое значение приведенной погрешности"
                }.Concat(base.GenerateDataColumnTypeObject()).ToArray();
            }

            protected override string GetReportTableName()
            {
                return null;
            }

            protected override void InitWork(CancellationTokenSource token)
            {
                ConnectionToDevice();
                DataRow.Clear();
                
                if (trm202 == null || Calibrator == null || measPoints == null) return;
                
                foreach (var point in measPoints)
                {
                    var operation = new BasicOperationVerefication<MeasPoint<Temperature>>();
                    operation.InitWorkAsync = async () =>
                    {
                        try
                        {
                            await Task.Run(() =>
                            {
                                //делаем предварительные настройка канала прибора

                                //1. задаем на каналах нужную характеристику
                                var typeTermoCouple = BitConverter
                                                     .GetBytes((int)CoupleTypeTrm).Where(a => a != 0)
                                                     .ToArray(); //выкидываем из массива лишние нули
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
                        operation.Expected = new MeasPoint<Temperature>(point.MainPhysicalQuantity);


                        operation.UpperCalculation = (expected) => expected + MeasureRanges.GetReducerTolMeasPoint(expected);
                        operation.UpperTolerance.MainPhysicalQuantity.ChangeMultiplier(point.MainPhysicalQuantity
                                                                                            .Multiplier);

                        operation.LowerCalculation = (expected) => expected - MeasureRanges.GetReducerTolMeasPoint(expected);
                        operation.LowerTolerance.MainPhysicalQuantity.ChangeMultiplier(point.MainPhysicalQuantity
                                                                                            .Multiplier);

                        var nullDegPoint = new MeasPoint<Temperature>(0);

                        try
                        {
                            //todo Исключить подключение термопар через отдельную вилку. Все подключения через клеммы для воспроизведения напряжения!!!
                            Calibrator.Temperature.SetTermoCoupleType(CalibrTypeTermocouple);
                            //Calibrator.Temperature.SetValue(nullDegPoint);
                            //Calibrator.Temperature.OutputOn();
                            //Thread.Sleep(1900);
                            // у ТРМ должна быть включена схема компенсации  температуры окр среды
                            //var measDelta = trm202.GetMeasValChanel(_chanelNumber);
                            Calibrator.Temperature.SetValue(point);
                            Calibrator.Temperature.OutputOn();
                            Thread.Sleep(3000);
                            var measPoint = trm202.GetMeasValChanel(_chanelNumber);
                            //if (CoupleTypeTrm != TRM202Device.in_t.E__b) measPoint = measPoint - measDelta;

                            Calibrator.Temperature.OutputOff();

                            MathStatistics.Round(ref measPoint, 1);
                            operation.Getting = new MeasPoint<Temperature>(measPoint);
                        }
                        catch (TrmException e)
                        {
                            string err = $"ТРМ-202 не произвел измерение. Код ошибки: {e}";
                            operation.Comment = err;
                            Logger.Error(err);
                        }
                        catch (Exception e)
                        {
                            Logger.Error(e);
                            throw;
                        }
                        finally
                        {
                            Calibrator.Temperature.OutputOff();
                        }

                        operation.IsGood = () =>
                            operation.Getting >= operation.LowerTolerance &&
                            operation.Getting <= operation.UpperTolerance;
                        ;
                    };
                    operation.CompliteWorkAsync = () => Helps.HelpsCompliteWork(operation, UserItemOperation);

                    DataRow.Add(operation);
                }
            }

            #endregion Methods
        }

        public class Operation8_4_Type_L_Poverka : Operation8_4_HCX_TermocoupleGost8_585
        {
            public Operation8_4_Type_L_Poverka(IUserItemOperation userItemOperation, ushort inChanel) :
                base(userItemOperation, inChanel)
            {
                trm202 = new TRM202DeviceUI();

                MeasureRanges = trm202.GetType_L_TermocoupleRangeStorage;
                CoupleTypeTrm = TRM202Device.in_t.E__L;
                Name = CoupleTypeTrm.GetStringValue();
                //todo Исключить подключение термопар через отдельную вилку. Все подключения через клеммы для воспроизведения напряжения!!!
                CalibrTypeTermocouple = TypeTermocouple.L;

                measPoints = new[]
                {
                    new MeasPoint<Temperature>(-150),
                    new MeasPoint<Temperature>(50M),
                    new MeasPoint<Temperature>(300M),
                    new MeasPoint<Temperature>(550M),
                    new MeasPoint<Temperature>(750M)
                };
            }

            #region Methods

            protected override string GetReportTableName()
            {
                return $"FillTabBm8_4_Type_L_ch{_chanelNumber}";
            }

            #endregion Methods
        }

        public class Operation8_4_Type_J_Poverka : Operation8_4_HCX_TermocoupleGost8_585
        {
            public Operation8_4_Type_J_Poverka(IUserItemOperation userItemOperation, ushort inChanel) :
                base(userItemOperation, inChanel)
            {
                trm202 = new TRM202DeviceUI();

                MeasureRanges = trm202.GetType_J_TermocoupleRangeStorage;
                CoupleTypeTrm = TRM202Device.in_t.E__j;
                Name = CoupleTypeTrm.GetStringValue();
                //todo Исключить подключение термопар через отдельную вилку. Все подключения через клеммы для воспроизведения напряжения!!!
                CalibrTypeTermocouple = TypeTermocouple.J;

                measPoints = new[]
                {
                    new MeasPoint<Temperature>(-130),
                    new MeasPoint<Temperature>(150M),
                    new MeasPoint<Temperature>(500M),
                    new MeasPoint<Temperature>(850M),
                    new MeasPoint<Temperature>(1130M)
                };
            }

            #region Methods

            protected override string GetReportTableName()
            {
                return $"FillTabBm8_4_Type_J_ch{_chanelNumber}";
            }

            #endregion Methods
        }

        public class Operation8_4_Type_N_Poverka : Operation8_4_HCX_TermocoupleGost8_585
        {
            public Operation8_4_Type_N_Poverka(IUserItemOperation userItemOperation, ushort inChanel) :
                base(userItemOperation, inChanel)
            {
                trm202 = new TRM202DeviceUI();

                MeasureRanges = trm202.GetType_N_TermocoupleRangeStorage;
                CoupleTypeTrm = TRM202Device.in_t.E__n;
                Name = CoupleTypeTrm.GetStringValue();
                //todo Исключить подключение термопар через отдельную вилку. Все подключения через клеммы для воспроизведения напряжения!!!
                CalibrTypeTermocouple = TypeTermocouple.N;

                measPoints = new[]
                {
                    new MeasPoint<Temperature>(-125),
                    new MeasPoint<Temperature>(175M),
                    new MeasPoint<Temperature>(550M),
                    new MeasPoint<Temperature>(925M),
                    new MeasPoint<Temperature>(1225M)
                };
            }

            #region Methods

            protected override string GetReportTableName()
            {
                return $"FillTabBm8_4_Type_N_ch{_chanelNumber}";
            }

            #endregion Methods
        }

        public class Operation8_4_Type_K_Poverka : Operation8_4_HCX_TermocoupleGost8_585
        {
            public Operation8_4_Type_K_Poverka(IUserItemOperation userItemOperation, ushort inChanel) :
                base(userItemOperation, inChanel)
            {
                trm202 = new TRM202DeviceUI();

                MeasureRanges = trm202.GetType_K_TermocoupleRangeStorage;
                CoupleTypeTrm = TRM202Device.in_t.E__k;
                Name = CoupleTypeTrm.GetStringValue();
                //todo Исключить подключение термопар через отдельную вилку. Все подключения через клеммы для воспроизведения напряжения!!!
                CalibrTypeTermocouple = TypeTermocouple.K;

                measPoints = new[]
                {
                    new MeasPoint<Temperature>(-125),
                    new MeasPoint<Temperature>(175M),
                    new MeasPoint<Temperature>(550M),
                    new MeasPoint<Temperature>(925M),
                    new MeasPoint<Temperature>(1225M)
                };
            }

            #region Methods

            protected override string GetReportTableName()
            {
                return $"FillTabBm8_4_Type_K_ch{_chanelNumber}";
            }

            #endregion Methods
        }

        public class Operation8_4_Type_K_Poverka_8_5_1_4 : Operation8_4_Type_K_Poverka
        {
            public Operation8_4_Type_K_Poverka_8_5_1_4(IUserItemOperation userItemOperation, ushort inChanel) : base(userItemOperation, inChanel)
            {
                Name = $"8.5.1.4 {this.CoupleTypeTrm.GetStringValue()}";
            }
            protected override string GetReportTableName()
            {
                return $"FillTabBm8_4_Type_K_8514ch{_chanelNumber}";
            }
        }

        public class Operation8_4_Type_S_Poverka : Operation8_4_HCX_TermocoupleGost8_585
        {
            public Operation8_4_Type_S_Poverka(IUserItemOperation userItemOperation, ushort inChanel) :
                base(userItemOperation, inChanel)
            {
                trm202 = new TRM202DeviceUI();

                MeasureRanges = trm202.GetType_S_TermocoupleRangeStorage;
                CoupleTypeTrm = TRM202Device.in_t.E__s;
                Name = CoupleTypeTrm.GetStringValue();
                //todo Исключить подключение термопар через отдельную вилку. Все подключения через клеммы для воспроизведения напряжения!!!
                CalibrTypeTermocouple = TypeTermocouple.S;

                measPoints = new[]
                {
                    new MeasPoint<Temperature>(87),
                    new MeasPoint<Temperature>(437M),
                    new MeasPoint<Temperature>(875M),
                    new MeasPoint<Temperature>(1312M),
                    new MeasPoint<Temperature>(1602M)
                };
            }

            #region Methods

            protected override string GetReportTableName()
            {
                return $"FillTabBm8_4_Type_S_ch{_chanelNumber}";
            }

            #endregion Methods
        }

        public class Operation8_4_Type_R_Poverka : Operation8_4_HCX_TermocoupleGost8_585
        {
            public Operation8_4_Type_R_Poverka(IUserItemOperation userItemOperation, ushort inChanel) :
                base(userItemOperation, inChanel)
            {
                trm202 = new TRM202DeviceUI();

                MeasureRanges = trm202.GetType_R_TermocoupleRangeStorage;
                CoupleTypeTrm = TRM202Device.in_t.E__r;
                Name = CoupleTypeTrm.GetStringValue();
                //todo Исключить подключение термопар через отдельную вилку. Все подключения через клеммы для воспроизведения напряжения!!!
                CalibrTypeTermocouple = TypeTermocouple.R;

                measPoints = new[]
                {
                    new MeasPoint<Temperature>(87),
                    new MeasPoint<Temperature>(437M),
                    new MeasPoint<Temperature>(875M),
                    new MeasPoint<Temperature>(1312M),
                    new MeasPoint<Temperature>(1662M)
                };
            }

            #region Methods

            protected override string GetReportTableName()
            {
                return $"FillTabBm8_4_Type_R_ch{_chanelNumber}";
            }

            #endregion Methods
        }

        public class Operation8_4_Type_B_Poverka : Operation8_4_HCX_TermocoupleGost8_585
        {
            public Operation8_4_Type_B_Poverka(IUserItemOperation userItemOperation, ushort inChanel) :
                base(userItemOperation, inChanel)
            {
                trm202 = new TRM202DeviceUI();

                MeasureRanges = trm202.GetType_B_TermocoupleRangeStorage;
                CoupleTypeTrm = TRM202Device.in_t.E__b;
                Name = CoupleTypeTrm.GetStringValue();
                //todo Исключить подключение термопар через отдельную вилку. Все подключения через клеммы для воспроизведения напряжения!!!
                CalibrTypeTermocouple = TypeTermocouple.B;

                measPoints = new[]
                {
                    new MeasPoint<Temperature>(280),
                    new MeasPoint<Temperature>(600M),
                    new MeasPoint<Temperature>(1000M),
                    new MeasPoint<Temperature>(1400M),
                    new MeasPoint<Temperature>(1720M)
                };
            }

            #region Methods

            protected override string GetReportTableName()
            {
                return $"FillTabBm8_4_Type_B_ch{_chanelNumber}";
            }

            #endregion Methods
        }

        public class Operation8_4_Type_T_Poverka : Operation8_4_HCX_TermocoupleGost8_585
        {
            public Operation8_4_Type_T_Poverka(IUserItemOperation userItemOperation, ushort inChanel) :
                base(userItemOperation, inChanel)
            {
                trm202 = new TRM202DeviceUI();

                MeasureRanges = trm202.GetType_T_TermocoupleRangeStorage;
                CoupleTypeTrm = TRM202Device.in_t.E__t;
                Name = CoupleTypeTrm.GetStringValue();
                //todo Исключить подключение термопар через отдельную вилку. Все подключения через клеммы для воспроизведения напряжения!!!
                CalibrTypeTermocouple = TypeTermocouple.T;

                measPoints = new[]
                {
                    new MeasPoint<Temperature>(-170),
                    new MeasPoint<Temperature>(-50M),
                    new MeasPoint<Temperature>(100M),
                    new MeasPoint<Temperature>(250M),
                    new MeasPoint<Temperature>(370M)
                };
            }

            #region Methods

            protected override string GetReportTableName()
            {
                return $"FillTabBm8_4_Type_T_ch{_chanelNumber}";
            }

            #endregion Methods
        }

        public class Operation8_4_Type_A1_Poverka : Operation8_4_A1_A3_TermocouoleGost8_585
        {
            public Operation8_4_Type_A1_Poverka(IUserItemOperation userItemOperation, ushort inChanel) :
                base(userItemOperation, inChanel)
            {
                trm202 = new TRM202DeviceUI();

                MeasureRanges = trm202.GetType_A1_TermocoupleRangeStorage;
                CoupleTypeTrm = TRM202Device.in_t.E_A1;
                Name = CoupleTypeTrm.GetStringValue();

                measPoints = new[]
                {
                    new MeasPoint<Temperature, Voltage>(125, 1.706M),
                    new MeasPoint<Temperature, Voltage>(625M, 10.028M),
                    new MeasPoint<Temperature, Voltage>(1250M, 19.876M),
                    new MeasPoint<Temperature, Voltage>(1875M, 27.844M),
                    new MeasPoint<Temperature, Voltage>(2375M, 32.654M)
                };
            }

            #region Methods

            protected override string GetReportTableName()
            {
                return $"FillTabBm8_4_Type_A1_ch{_chanelNumber}";
            }

            #endregion Methods
        }

        public class Operation8_4_Type_A2_Poverka : Operation8_4_A1_A3_TermocouoleGost8_585
        {
            public Operation8_4_Type_A2_Poverka(IUserItemOperation userItemOperation, ushort inChanel) :
                base(userItemOperation, inChanel)
            {
                trm202 = new TRM202DeviceUI();

                MeasureRanges = trm202.GetType_A2_TermocoupleRangeStorage;
                CoupleTypeTrm = TRM202Device.in_t.E_A2;
                Name = CoupleTypeTrm.GetStringValue();

                measPoints = new[]
                {
                    new MeasPoint<Temperature, Voltage>(90, 1.191M),
                    new MeasPoint<Temperature, Voltage>(450M, 7.139M),
                    new MeasPoint<Temperature, Voltage>(900M, 14.696M),
                    new MeasPoint<Temperature, Voltage>(1350M, 21.478M),
                    new MeasPoint<Temperature, Voltage>(1710M, 26.180M)
                };
            }

            #region Methods

            protected override string GetReportTableName()
            {
                return $"FillTabBm8_4_Type_A2_ch{_chanelNumber}";
            }

            #endregion Methods
        }

        public class Operation8_4_Type_A3_Poverka : Operation8_4_A1_A3_TermocouoleGost8_585
        {
            public Operation8_4_Type_A3_Poverka(IUserItemOperation userItemOperation, ushort inChanel) :
                base(userItemOperation, inChanel)
            {
                trm202 = new TRM202DeviceUI();

                MeasureRanges = trm202.GetType_A3_TermocoupleRangeStorage;
                CoupleTypeTrm = TRM202Device.in_t.E_A3;
                Name = CoupleTypeTrm.GetStringValue();

                measPoints = new[]
                {
                    new MeasPoint<Temperature, Voltage>(90, 1.176M),
                    new MeasPoint<Temperature, Voltage>(450M, 6.985M),
                    new MeasPoint<Temperature, Voltage>(900M, 14.411M),
                    new MeasPoint<Temperature, Voltage>(1350M, 21.100M),
                    new MeasPoint<Temperature, Voltage>(1710M, 25.728M)
                };
            }

            #region Methods

            protected override string GetReportTableName()
            {
                return $"FillTabBm8_4_Type_A3_ch{_chanelNumber}";
            }

            #endregion Methods
        }

        public class Operation8_4_A1_A3_TermocouoleGost8_585 : BaseMeasureOperation<Temperature>
        {
            private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

            #region Fields

            
           

            #endregion Fields

            #region Property

            protected ushort _chanelNumber { get; set; }

            #endregion Property

            public Operation8_4_A1_A3_TermocouoleGost8_585(IUserItemOperation userItemOperation, ushort inChanelNumber) :
                base(userItemOperation)
            {
                _chanelNumber = inChanelNumber;
                Name = "Определение основной приведенной погрешности прибора со специализированным входом";
                Sheme = new SchemeImage
                {
                    Number = _chanelNumber + 5,
                    FileName = $"TRM202_VoltageTermocouple_chanel{_chanelNumber}.jpg",
                    ExtendedDescription = "Соберите схему, как показано на рисунке."
                };
            }

            #region Methods



            protected override string[] GenerateDataColumnTypeObject()
            {
                return new[]
                {
                    "Поверяемая точка", "Измеренное значение",
                    "Основная приведенная погрешность",
                    "Допустимое значение приведенной погрешности"
                }.Concat(base.GenerateDataColumnTypeObject()).ToArray();
            }

            protected override string GetReportTableName()
            {
                return null;
            }

            protected override void InitWork(CancellationTokenSource token)
            {
                ConnectionToDevice();
                DataRow.Clear();

                if (trm202 == null || Calibrator == null || measPoints == null) return;

               

                base.InitWork(token);

                foreach (var point in measPoints)
                {
                    var operation = new BasicOperationVerefication<MeasPoint<Temperature>>();
                    operation.InitWorkAsync = async () =>
                    {
                        try
                        {
                            await Task.Run(() =>
                            {
                                //делаем предварительные настройка канала прибора

                                //1. задаем на каналах нужную характеристику
                                var typeTermoCouple = BitConverter
                                                     .GetBytes((int)CoupleTypeTrm).Where(a => a != 0)
                                                     .ToArray(); //выкидываем из массива лишние нули
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
                        operation.Expected = new MeasPoint<Temperature>(point.MainPhysicalQuantity);


                        operation.UpperCalculation = (expected) => expected + MeasureRanges.GetReducerTolMeasPoint(expected);
                        operation.UpperTolerance.MainPhysicalQuantity.ChangeMultiplier(point.MainPhysicalQuantity
                                                                                            .Multiplier);

                        operation.LowerCalculation = (expected) => expected - MeasureRanges.GetReducerTolMeasPoint(expected);
                        operation.LowerTolerance.MainPhysicalQuantity.ChangeMultiplier(point.MainPhysicalQuantity
                                                                                            .Multiplier);
                        MeasPoint<Voltage> nullPoint = new MeasPoint<Voltage>(0);

                        try
                        {
                            //Calibrator.DcVoltage.SetValue(nullPoint);
                            //Calibrator.DcVoltage.OutputOn();
                            //Thread.Sleep(1900);
                            //decimal delta = trm202.GetMeasValChanel(_chanelNumber);
                            MeasPoint<Voltage> setPoint =
                                new MeasPoint<Voltage>(point.MainPhysicalQuantity.Value, UnitMultiplier.Mili);
                            Calibrator.DcVoltage.SetValue(setPoint);
                            Calibrator.DcVoltage.OutputOn();
                            Thread.Sleep(1900);
                            decimal measPoint = trm202.GetMeasValChanel(_chanelNumber);
                            //measPoint = measPoint - delta;
                            Calibrator.DcVoltage.OutputOff();
                            MathStatistics.Round(ref measPoint, 1);
                            operation.Getting = new MeasPoint<Temperature>(measPoint);
                        }
                        catch (TrmException e)
                        {
                            string err = $"ТРМ-202 не произвел измерение. Код ошибки: {e}";
                            operation.Comment = err;
                            Logger.Error(err);
                        }
                        catch (Exception e)
                        {
                            Logger.Error(e);
                            throw;
                        }
                        finally
                        {
                            Calibrator.DcVoltage.OutputOff();
                        }

                        operation.IsGood = () =>
                            operation.Getting >= operation.LowerTolerance &&
                            operation.Getting <= operation.UpperTolerance;
                        ;
                    };
                    operation.CompliteWorkAsync = () => Helps.HelpsCompliteWork(operation, UserItemOperation);

                    DataRow.Add(operation);
                }
            }

            #endregion Methods
        }

        public class Operation8_4_UnicSignal : BaseMeasureOperation<Percent>
        {
            private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

            #region Fields

            protected TRM202Device.in_t _coupleType;
            //protected ICalibratorMultimeterFlukeBase Calibrator;
            protected MeasPoint<Percent, Voltage>[] measPointsPercentVolt;
            

            //protected TRM202DeviceUI trm202;

            #endregion Fields



            public Operation8_4_UnicSignal(IUserItemOperation userItemOperation, ushort inChanelNumber) :
                base(userItemOperation)
            {
                _chanelNumber = inChanelNumber;
                Name = "Определение основной приведенной погрешности прибора со специализированным входом";
                Sheme = new SchemeImage
                {
                    Number = _chanelNumber + 5,
                    FileName = $"TRM202_VoltageTermocouple_chanel{_chanelNumber}.jpg",
                    ExtendedDescription = "Соберите схему, как показано на рисунке."
                };
                
            }

            #region Methods

           

            protected override string[] GenerateDataColumnTypeObject()
            {
                return new[]
                {
                    "Поверяемая точка", "Измеренное значение",
                    "Основная приведенная погрешность",
                    "Допустимое значение приведенной погрешности"
                }.Concat(base.GenerateDataColumnTypeObject()).ToArray();
            }

            protected override string GetReportTableName()
            {
                return null;
            }

            protected override void InitWork(CancellationTokenSource token)
            {
                ConnectionToDevice();
                DataRow.Clear();
                
                if (trm202 == null || Calibrator == null || measPointsPercentVolt == null) return;
                
                foreach (var point in measPointsPercentVolt)
                {
                    var operation = new BasicOperationVerefication<MeasPoint<Percent>>();
                    operation.InitWorkAsync = async () =>
                    {
                        try
                        {
                            await Task.Run(() =>
                            {
                                //делаем предварительные настройка канала прибора

                                //1. задаем на каналах нужную характеристику
                                var typeTermoCouple = BitConverter
                                                     .GetBytes((int)_coupleType).Where(a => a != 0)
                                                     .ToArray(); //выкидываем из массива лишние нули
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
                            operation.Expected = new MeasPoint<Percent>(point.MainPhysicalQuantity);
                            operation.UpperCalculation = (expected) => expected + MeasureRanges.GetReducerTolMeasPoint(expected);
                            operation.UpperTolerance.MainPhysicalQuantity.ChangeMultiplier(point.MainPhysicalQuantity
                                                                                                .Multiplier);

                            operation.LowerCalculation = (expected) => expected - MeasureRanges.GetReducerTolMeasPoint(expected);
                            operation.LowerTolerance.MainPhysicalQuantity.ChangeMultiplier(point.MainPhysicalQuantity
                                                                                                .Multiplier);
                            var setPoint =
                                new MeasPoint<Voltage>(point.AdditionalPhysicalQuantity.Value, UnitMultiplier.Mili);
                            Calibrator.DcVoltage.SetValue(setPoint);
                            Calibrator.DcVoltage.OutputOn();
                            Thread.Sleep(1900);
                            var measPoint = trm202.GetMeasValChanel(_chanelNumber);
                            Calibrator.DcVoltage.OutputOff();
                            MathStatistics.Round(ref measPoint, 1);
                            operation.Getting = new MeasPoint<Percent>(measPoint);
                        }
                        catch (Exception e)
                        {
                            Logger.Error(e);
                            throw;
                        }
                        finally
                        {
                            Calibrator.DcVoltage.OutputOff();
                        }

                        operation.IsGood = () =>
                            operation.Getting >= operation.LowerTolerance &&
                            operation.Getting <= operation.UpperTolerance;
                        ;
                    };
                    operation.CompliteWorkAsync = () => Helps.HelpsCompliteWork(operation, UserItemOperation);

                    DataRow.Add(operation);
                }
            }

            #endregion Methods
        }

        public class Operation8_4_UnificSignal_0_1_Poverka : Operation8_4_UnicSignal
        {
            public Operation8_4_UnificSignal_0_1_Poverka(IUserItemOperation userItemOperation, ushort inChanel) :
                base(userItemOperation, inChanel)
            {
                trm202 = new TRM202DeviceUI();
                _coupleType = TRM202Device.in_t.U0_1;
                Name = _coupleType.GetStringValue();
                MeasureRanges = trm202.GetUnificSignalRangeStorage;

                measPointsPercentVolt = new[]
                {
                    new MeasPoint<Percent, Voltage>(5, 50M),
                    new MeasPoint<Percent, Voltage>(25M, 250M),
                    new MeasPoint<Percent, Voltage>(50M, 500M),
                    new MeasPoint<Percent, Voltage>(75M, 750M),
                    new MeasPoint<Percent, Voltage>(95M, 950M)
                };
            }

            #region Methods

            protected override string GetReportTableName()
            {
                return $"FillTabBm8_4_UnificSignal_0_1_ch{_chanelNumber}";
            }

            #endregion Methods
        }

        public class Operation8_4_UnificSignal_0_1_Poverka_8_5_1_3 : Operation8_4_UnicSignal
        {
            public Operation8_4_UnificSignal_0_1_Poverka_8_5_1_3(IUserItemOperation userItemOperation, ushort inChanel) :
                base(userItemOperation, inChanel)
            {
                trm202 = new TRM202DeviceUI();
                _coupleType = TRM202Device.in_t.U0_1;
                Name = $"8.5.1.3 {_coupleType.GetStringValue()}";
                MeasureRanges = trm202.GetUnificSignalRangeStorage;

                measPointsPercentVolt = new[]
                {

                    new MeasPoint<Percent, Voltage>(50M, 500M)
                };
            }

            #region Methods

            protected override string GetReportTableName()
            {
                return $"FillTabBm8_4_UnificSignal_0_1V__8513ch{_chanelNumber}";
            }

            #endregion Methods
        }

        public class Operation8_4_UnificSignal_50_50_Poverka : Operation8_4_UnicSignal
        {
            public Operation8_4_UnificSignal_50_50_Poverka(IUserItemOperation userItemOperation, ushort inChanel) :
                base(userItemOperation, inChanel)
            {
                trm202 = new TRM202DeviceUI();
                _coupleType = TRM202Device.in_t.U_50;
                Name = _coupleType.GetStringValue();
                MeasureRanges = trm202.GetUnificSignalRangeStorage;

                measPointsPercentVolt = new[]
                {
                    new MeasPoint<Percent, Voltage>(5, -45M),
                    new MeasPoint<Percent, Voltage>(25M, -25M),
                    new MeasPoint<Percent, Voltage>(50M, 0M),
                    new MeasPoint<Percent, Voltage>(75M, 25M),
                    new MeasPoint<Percent, Voltage>(95M, 45M)
                };
            }

            #region Methods

            protected override string GetReportTableName()
            {
                return $"FillTabBm8_4_UnificSignal_50_50_ch{_chanelNumber}";
            }

            #endregion Methods
        }

        public class Operation8_4_UnificSignal_0_5mA_Poverka : Operation8_4_UnicSignal
        {
            public Operation8_4_UnificSignal_0_5mA_Poverka(IUserItemOperation userItemOperation, ushort inChanel) :
                base(userItemOperation, inChanel)
            {
                trm202 = new TRM202DeviceUI();
                _coupleType = TRM202Device.in_t.i0_5;
                Name = _coupleType.GetStringValue();
                MeasureRanges = trm202.GetUnificSignalRangeStorage;

                //в этом пункте прибор работает с сигналом 0-5мА,
                //но измеренное значние получает с шунта,
                //поэтому вместо тока будем сразу подавать напряжение, пропорциональное шунту 100 Ом по закону Ома.
                measPointsPercentVolt = new[]
                {
                    new MeasPoint<Percent, Voltage>(5, 25),
                    new MeasPoint<Percent, Voltage>(25M, 125M),
                    new MeasPoint<Percent, Voltage>(50M, 250M),
                    new MeasPoint<Percent, Voltage>(75M, 375M),
                    new MeasPoint<Percent, Voltage>(95M, 475M)
                };
            }

            #region Methods

            protected override string GetReportTableName()
            {
                return $"FillTabBm8_4_UnificSignal_0_50mA_ch{_chanelNumber}";
            }

            #endregion Methods
        }

        public class Operation8_4_UnificSignal_0_20mA_Poverka : Operation8_4_UnicSignal
        {
            public Operation8_4_UnificSignal_0_20mA_Poverka(IUserItemOperation userItemOperation, ushort inChanel) :
                base(userItemOperation, inChanel)
            {
                trm202 = new TRM202DeviceUI();
                _coupleType = TRM202Device.in_t.i0_20;
                Name = _coupleType.GetStringValue();
                MeasureRanges = trm202.GetUnificSignalRangeStorage;

                //в этом пункте прибор работает с сигналом 0-20мА,
                //но измеренное значние получает с шунта,
                //поэтому вместо тока будем сразу подавать напряжение, пропорциональное шунту 100 Ом по закону Ома.
                measPointsPercentVolt = new[]
                {
                    new MeasPoint<Percent, Voltage>(5, 100),
                    new MeasPoint<Percent, Voltage>(25M, 500M),
                    new MeasPoint<Percent, Voltage>(50M, 1000M),
                    new MeasPoint<Percent, Voltage>(75M, 1500M),
                    new MeasPoint<Percent, Voltage>(95M, 1900M)
                };
            }

            #region Methods

            protected override string GetReportTableName()
            {
                return $"FillTabBm8_4_UnificSignal_0_20mA_ch{_chanelNumber}";
            }

            #endregion Methods
        }

        public class Operation8_4_UnificSignal_0_20mA_Poverka_8_5_1_2 : Operation8_4_UnicSignal
        {
            public Operation8_4_UnificSignal_0_20mA_Poverka_8_5_1_2(IUserItemOperation userItemOperation, ushort inChanel) :
                base(userItemOperation, inChanel)
            {
                trm202 = new TRM202DeviceUI();
                _coupleType = TRM202Device.in_t.i0_20;
                Name = $"8.5.1.2 {_coupleType.GetStringValue()}";
                MeasureRanges = trm202.GetUnificSignalRangeStorage;

                //в этом пункте прибор работает с сигналом 0-20мА,
                //но измеренное значние получает с шунта,
                //поэтому вместо тока будем сразу подавать напряжение, пропорциональное шунту 100 Ом по закону Ома.
                measPointsPercentVolt = new[]
                {

                    new MeasPoint<Percent, Voltage>(50M, 1000M)
                };
            }

            #region Methods

            protected override string GetReportTableName()
            {
                return $"FillTabBm8_4_UnificSignal_0_20mA_8512ch{_chanelNumber}";
            }

            #endregion Methods
        }

        public class Operation8_4_UnificSignal_4_20mA_Poverka : Operation8_4_UnicSignal
        {
            public Operation8_4_UnificSignal_4_20mA_Poverka(IUserItemOperation userItemOperation, ushort inChanel) :
                base(userItemOperation, inChanel)
            {
                trm202 = new TRM202DeviceUI();
                _coupleType = TRM202Device.in_t.i4_20;
                Name = _coupleType.GetStringValue();
                MeasureRanges = trm202.GetUnificSignalRangeStorage;

                //в этом пункте прибор работает с сигналом 4-20мА,
                //но измеренное значние получает с шунта,
                //поэтому вместо тока будем сразу подавать напряжение, пропорциональное шунту 100 Ом по закону Ома.
                measPointsPercentVolt = new[]
                {
                    new MeasPoint<Percent, Voltage>(5, 480),
                    new MeasPoint<Percent, Voltage>(25M, 800M),
                    new MeasPoint<Percent, Voltage>(50M, 1200M),
                    new MeasPoint<Percent, Voltage>(75M, 1600M),
                    new MeasPoint<Percent, Voltage>(95M, 1920M)
                };
            }

            #region Methods

            protected override string GetReportTableName()
            {
                return $"FillTabBm8_4_UnificSignal_4_20mA_ch{_chanelNumber}";
            }

            #endregion Methods
        }

        #region Gosr6651

        public class Operation8_4_ResistanceTermocoupleGost6651 : BaseMeasureOperation<Temperature>
        {
            private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

            #region Fields

            protected TRM202Device.in_t _coupleType;

            protected MeasPoint<Temperature, Resistance>[] measPointsTempRes;

            #endregion Fields

            public Operation8_4_ResistanceTermocoupleGost6651(IUserItemOperation userItemOperation,
                ushort inChanelNumber) : base(userItemOperation)
            {
                _chanelNumber = inChanelNumber;
                Name = "Определение основной приведенной погрешности прибора со специализированным входом";
                Sheme = new SchemeImage
                {
                    Number = _chanelNumber,
                    FileName = $"TRM202_ResistanceTermocouple_chanel{_chanelNumber}.jpg",
                    ExtendedDescription = "Соберите схему, как показано на рисунке."
                };
            }

            #region Methods

           

            protected override string[] GenerateDataColumnTypeObject()
            {
                return new[]
                {
                    "Поверяемая точка", "Измеренное значение",
                    "Основная приведенная погрешность",
                    "Допустимое значение приведенной погрешности"
                }.Concat(base.GenerateDataColumnTypeObject()).ToArray();
            }

            protected override string GetReportTableName()
            {
                return null;
            }

            protected override void InitWork(CancellationTokenSource token)
            {
               ConnectionToDevice();
               DataRow.Clear();

                if (trm202 == null || Calibrator == null || measPointsTempRes == null) return;

                

                foreach (var point in measPointsTempRes)
                {
                    var operation = new BasicOperationVerefication<MeasPoint<Temperature>>();
                    operation.InitWorkAsync = async () =>
                    {
                        try
                        {
                            await Task.Run(() =>
                            {
                                //делаем предварительные настройка канала прибора

                                //1. задаем на каналах нужную характеристику
                                var typeTermoCouple = BitConverter
                                                     .GetBytes((int)_coupleType).Where(a => a != 0)
                                                     .ToArray(); //выкидываем из массива лишние нули
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

                        operation.Expected = new MeasPoint<Temperature>(point.MainPhysicalQuantity);

                        operation.UpperCalculation = (expected) => expected + MeasureRanges.GetReducerTolMeasPoint(expected);
                        operation.UpperTolerance.MainPhysicalQuantity.ChangeMultiplier(point.MainPhysicalQuantity
                                                                                            .Multiplier);

                        operation.LowerCalculation = (expected) => expected - MeasureRanges.GetReducerTolMeasPoint(expected);
                        operation.LowerTolerance.MainPhysicalQuantity.ChangeMultiplier(point.MainPhysicalQuantity
                                                                                            .Multiplier);

                        var setPoint = new MeasPoint<Resistance>(point.AdditionalPhysicalQuantity);
                        try
                        {

                            Calibrator.Resistance2W.SetValue(setPoint);
                            Calibrator.Resistance2W.SetCompensation(Compensation.CompNone);
                            Calibrator.Resistance2W.OutputOn();
                            Thread.Sleep(1900);
                            var measPoint = trm202.GetMeasValChanel(_chanelNumber);
                            Calibrator.Resistance2W.OutputOff();
                            MathStatistics.Round(ref measPoint, 1);
                            operation.Getting = new MeasPoint<Temperature>(measPoint);
                        }
                        catch (TrmException e)
                        {
                            string err = $"ТРМ-202 не произвел измерение. Код ошибки: {e}";
                            operation.Comment = err;
                            Logger.Error(err);
                        }
                        catch (Exception e)
                        {
                            Logger.Error(e);
                            throw;
                        }
                        finally
                        {
                            Calibrator.Resistance2W.OutputOff();
                        }

                        operation.IsGood = () =>
                            operation.Getting >= operation.LowerTolerance &&
                            operation.Getting <= operation.UpperTolerance;
                        ;
                    };
                    operation.CompliteWorkAsync = () => Helps.HelpsCompliteWork(operation, UserItemOperation);

                    DataRow.Add(operation);
                }
            }

            #endregion Methods
        }

        public class Operation8_4_Cu50_426_Poverka : Operation8_4_ResistanceTermocoupleGost6651
        {
            public Operation8_4_Cu50_426_Poverka(IUserItemOperation userItemOperation, ushort inChanel) :
                base(userItemOperation, inChanel)
            {
                trm202 = new TRM202DeviceUI();

                MeasureRanges = trm202.GetCu50_426RangeStorage;
                _coupleType = TRM202Device.in_t.r426;
                Name = _coupleType.GetStringValue();

                measPointsTempRes = new[]
                {
                    new MeasPoint<Temperature, Resistance>(-37, 42.120M),
                    new MeasPoint<Temperature, Resistance>(12.5M, 52.662M),
                    new MeasPoint<Temperature, Resistance>(75M, 65.980M),
                    new MeasPoint<Temperature, Resistance>(137.5M, 79.297M),
                    new MeasPoint<Temperature, Resistance>(187.5M, 89.952M)
                };
            }

            #region Methods

            protected override string GetReportTableName()
            {
                return $"FillTabBm8_4_Cu50_426_ch{_chanelNumber}";
            }

            #endregion Methods
        }

        public class Operation8_4_Cu100_426_Poverka : Operation8_4_ResistanceTermocoupleGost6651
        {
            public Operation8_4_Cu100_426_Poverka(IUserItemOperation userItemOperation, ushort inChanel) :
                base(userItemOperation, inChanel)
            {
                trm202 = new TRM202DeviceUI();

                MeasureRanges = trm202.GetCu100_426RangeStorage;
                _coupleType = TRM202Device.in_t.r_426;
                Name = _coupleType.GetStringValue();

                measPointsTempRes = new[]
                {
                    new MeasPoint<Temperature, Resistance>(-37, 84.230M),
                    new MeasPoint<Temperature, Resistance>(12.5M, 105.325M),
                    new MeasPoint<Temperature, Resistance>(75, 131.960M),
                    new MeasPoint<Temperature, Resistance>(137.5M, 158.595M),
                    new MeasPoint<Temperature, Resistance>(187.5M, 179.905M)
                };
            }

            #region Methods

            protected override string GetReportTableName()
            {
                return $"FillTabBm8_4_Cu100_426_ch{_chanelNumber}";
            }

            #endregion Methods
        }

        public class Operation8_4_Pt100_385_Poverka : Operation8_4_ResistanceTermocoupleGost6651
        {
            public Operation8_4_Pt100_385_Poverka(IUserItemOperation userItemOperation, ushort inChanel) :
                base(userItemOperation, inChanel)
            {
                trm202 = new TRM202DeviceUI();

                MeasureRanges = trm202.GetPt100_385RangeStorage;
                _coupleType = TRM202Device.in_t.r_385;
                Name = _coupleType.GetStringValue();

                measPointsTempRes = new[]
                {
                    new MeasPoint<Temperature, Resistance>(-152, 38.890M),
                    new MeasPoint<Temperature, Resistance>(37.5M, 114.575M),
                    new MeasPoint<Temperature, Resistance>(275, 203.110M),
                    new MeasPoint<Temperature, Resistance>(512.5M, 285.135M),
                    new MeasPoint<Temperature, Resistance>(702.5M, 346.055M)
                };
            }

            #region Methods

            protected override string GetReportTableName()
            {
                return $"FillTabBm8_4_Pt100_385_ch{_chanelNumber}";
            }

            #endregion Methods
        }

        public class Operation8_4_Pt50_385_Poverka : Operation8_4_ResistanceTermocoupleGost6651
        {
            public Operation8_4_Pt50_385_Poverka(IUserItemOperation userItemOperation, ushort inChanel) :
                base(userItemOperation, inChanel)
            {
                trm202 = new TRM202DeviceUI();

                MeasureRanges = trm202.GetPt50_385RangeStorage;
                _coupleType = TRM202Device.in_t.r385;
                Name = _coupleType.GetStringValue();

                measPointsTempRes = new[]
                {
                    new MeasPoint<Temperature, Resistance>(-152, 19.445M),
                    new MeasPoint<Temperature, Resistance>(37.5M, 57.288M),
                    new MeasPoint<Temperature, Resistance>(275, 101.555M),
                    new MeasPoint<Temperature, Resistance>(512.5M, 142.567M),
                    new MeasPoint<Temperature, Resistance>(702.5M, 173.027M)
                };
            }

            #region Methods

            protected override string GetReportTableName()
            {
                return $"FillTabBmOper8_4_Pt50_385_ch{_chanelNumber}";
            }

            #endregion Methods
        }

        public class Operation8_4_TSM50M_428_Poverka : Operation8_4_ResistanceTermocoupleGost6651
        {
            public Operation8_4_TSM50M_428_Poverka(IUserItemOperation userItemOperation, ushort inChanel) :
                base(userItemOperation, inChanel)
            {
                trm202 = new TRM202DeviceUI();

                MeasureRanges = trm202.GetTSP50M_428RangeStorage;
                _coupleType = TRM202Device.in_t.r428;
                Name = _coupleType.GetStringValue();

                measPointsTempRes = new[]
                {
                    new MeasPoint<Temperature, Resistance>(-170, 12.570M),
                    new MeasPoint<Temperature, Resistance>(-92.5M, 29.960M),
                    new MeasPoint<Temperature, Resistance>(5, 51.070M),
                    new MeasPoint<Temperature, Resistance>(102.5M, 71.923M),
                    new MeasPoint<Temperature, Resistance>(180.5M, 88.605M)
                };
            }

            #region Methods

            protected override string GetReportTableName()
            {
                return $"FillTabBmOper8_4_TSM50M_428_ch{_chanelNumber}";
            }

            #endregion Methods
        }

        public class Operation8_4_TSM50M_428_Poverka_8_5_1_1 : Operation8_4_ResistanceTermocoupleGost6651
        {
            public Operation8_4_TSM50M_428_Poverka_8_5_1_1(IUserItemOperation userItemOperation, ushort inChanel) :
                base(userItemOperation, inChanel)
            {
                trm202 = new TRM202DeviceUI();

                MeasureRanges = trm202.GetTSP50M_428RangeStorage;
                _coupleType = TRM202Device.in_t.r428;
                Name = $"8.5.1.1 {_coupleType.GetStringValue()}";

                measPointsTempRes = new[]
                {

                    new MeasPoint<Temperature, Resistance>(5, 51.070M)

                };
            }

            #region Methods

            protected override string GetReportTableName()
            {
                return $"FillTabBmOper8_4_TSM50M_428_8_5_1_1_ch{_chanelNumber}";
            }

            #endregion Methods
        }

        public class Operation8_4_TSM100M_428_Poverka : Operation8_4_ResistanceTermocoupleGost6651
        {
            public Operation8_4_TSM100M_428_Poverka(IUserItemOperation userItemOperation, ushort inChanel) :
                base(userItemOperation, inChanel)
            {
                trm202 = new TRM202DeviceUI();

                MeasureRanges = trm202.GetTSP100M_428RangeStorage;
                _coupleType = TRM202Device.in_t.r_428;
                Name = _coupleType.GetStringValue();

                measPointsTempRes = new[]
                {
                    new MeasPoint<Temperature, Resistance>(-170, 25.140M),
                    new MeasPoint<Temperature, Resistance>(-92.5M, 59.920M),
                    new MeasPoint<Temperature, Resistance>(5, 102.140M),
                    new MeasPoint<Temperature, Resistance>(102.5M, 143.845M),
                    new MeasPoint<Temperature, Resistance>(180.5M, 177.210M)
                };
            }

            #region Methods

            protected override string GetReportTableName()
            {
                return $"FillTabBmOper8_4_TSM100M_428_ch{_chanelNumber}";
            }

            #endregion Methods
        }

        public class Operation8_4_TSP50P_391_Poverka : Operation8_4_ResistanceTermocoupleGost6651
        {
            public Operation8_4_TSP50P_391_Poverka(IUserItemOperation userItemOperation, ushort inChanel) :
                base(userItemOperation, inChanel)
            {
                trm202 = new TRM202DeviceUI();

                MeasureRanges = trm202.GetTSP50P_391RangeStorage;
                _coupleType = TRM202Device.in_t.r391;
                Name = _coupleType.GetStringValue();

                measPointsTempRes = new[]
                {
                    new MeasPoint<Temperature, Resistance>(-152, 18.970M),
                    new MeasPoint<Temperature, Resistance>(37.5M, 57.403M),
                    new MeasPoint<Temperature, Resistance>(275, 102.375M),
                    new MeasPoint<Temperature, Resistance>(512.5M, 144.055M),
                    new MeasPoint<Temperature, Resistance>(702.5M, 174.955M)
                };
            }

            #region Methods

            protected override string GetReportTableName()
            {
                return $"FillTabBmOper8_4_TSP50P_391_ch{_chanelNumber}";
            }

            #endregion Methods
        }

        public class Operation8_4_TSP100P_391_Poverka : Operation8_4_ResistanceTermocoupleGost6651
        {
            public Operation8_4_TSP100P_391_Poverka(IUserItemOperation userItemOperation, ushort inChanel) :
                base(userItemOperation, inChanel)
            {
                trm202 = new TRM202DeviceUI();

                MeasureRanges = trm202.GetTSP100P_391RangeStorage;
                _coupleType = TRM202Device.in_t.r_391;
                Name = _coupleType.GetStringValue();

                measPointsTempRes = new[]
                {
                    new MeasPoint<Temperature, Resistance>(-152, 37.940M),
                    new MeasPoint<Temperature, Resistance>(37.5M, 114.805M),
                    new MeasPoint<Temperature, Resistance>(275, 204.750M),
                    new MeasPoint<Temperature, Resistance>(512.5M, 288.110M),
                    new MeasPoint<Temperature, Resistance>(702.5M, 349.910M)
                };
            }

            #region Methods

            protected override string GetReportTableName()
            {
                return $"FillTabBmOper8_4_TSP100P_391_ch{_chanelNumber}";
            }

            #endregion Methods
        }

        #endregion Gosr6651
    }
}