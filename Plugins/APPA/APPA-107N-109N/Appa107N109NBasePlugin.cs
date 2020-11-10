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
using ASMC.Devices.Port.APPA;
using DevExpress.Mvvm;
using NLog;

namespace APPA_107N_109N
{
    public class Appa107N109NBasePlugin<T> : Program<T> where T : OperationMetrControlBase

    {
        public Appa107N109NBasePlugin(ServicePack service) : base(service)
        {
            Grsi = "20085-11";
        }
    }

    public class Operation : OperationMetrControlBase
    {
        //определяет какие типы проверок доступны для СИ: поверка первичная/переодическая, калибровка, adjustment.
        public Operation()
        {
            //это операция первичной поверки
            //UserItemOperationPrimaryVerf = new OpertionFirsVerf();
            //здесь периодическая поверка, но набор операций такой же
            UserItemOperationPeriodicVerf = UserItemOperationPrimaryVerf;
        }
    }

    /// <summary>
    /// Класс для вспомогательных функций
    /// </summary>
    public static class Hepls
    {
        #region Methods

        /// <summary>
        /// Позволяет посчитать, сколько раз нужно нажать кнопку переключения пределов, что бы попасть на нужный предел измерения.
        /// </summary>
        /// <param name = "CountOfRange">Общее количество пределов измерения на данном режиме.</param>
        /// <param name = "CurrentRange">Номер текущего установленного предела измерения.</param>
        /// <param name = "TargetRange">Номер предела измерения, на который нужно переключиться.</param>
        /// <returns></returns>
        public static int CountOfPushButton(int CountOfRange, int CurrentRange, int TargetRange)
        {
            if (CurrentRange == TargetRange) return 0;
            if (CurrentRange < TargetRange)
                return TargetRange - CurrentRange;
            return CountOfRange - CurrentRange + TargetRange;
        }

        public static Task<bool> HelpsCompliteWork<T>(BasicOperationVerefication<MeasPoint<T>> operation,
            IUserItemOperation UserItemOperation) where T : class, IPhysicalQuantity<T>, new()
        {
            if (operation.IsGood!= null && !operation.IsGood())
            {
                var answer =
                    UserItemOperation.ServicePack.MessageBox()
                                     .Show($"Текущая точка {operation.Expected.Description} не проходит по допуску:\n" +
                                           $"Минимально допустимое значение {operation.LowerTolerance.Description}\n" +
                                           $"Максимально допустимое значение {operation.UpperTolerance.Description}\n" +
                                           $"Допустимое значение погрешности {operation.Error.Description}\n" +
                                           $"ИЗМЕРЕННОЕ значение {operation.Getting.Description}\n\n" +
                                           $"\nФАКТИЧЕСКАЯ погрешность {operation.Expected - operation.Getting}\n\n" +
                                           "Повторить измерение этой точки?",
                                           "Информация по текущему измерению",
                                           MessageButton.YesNo, MessageIcon.Question,
                                           MessageResult.Yes);

                if (answer == MessageResult.No) return Task.FromResult(true);
            }
            if (operation.IsGood==null)
                return Task.FromResult(true);
            return Task.FromResult(operation.IsGood());
        }

        public static Task<bool> HelpsCompliteWork<T, T1>(BasicOperationVerefication<MeasPoint<T, T1>> operation,
            IUserItemOperation UserItemOperation) where T : class, IPhysicalQuantity<T>, new()
                                                  where T1 : class, IPhysicalQuantity<T1>, new()
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
                                           $"\nФАКТИЧЕСКАЯ погрешность {operation.Expected - operation.Getting}\n\n" +
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

        #endregion
    }

    public abstract class OpertionFirsVerf : ASMC.Core.Model.Operation
    {
        public override void RefreshDevice()
        {
            AddresDevice = IeeeBase.AllStringConnect;
        }

        public override void FindDevice()
        {
            throw new NotImplementedException();
        }

        public OpertionFirsVerf(ServicePack servicePack) : base(servicePack)
        {
            //Необходимые устройства
            ControlDevices = new IDeviceUi[]
            {
                new Device
                {
                    Devices = new IDeviceBase[] {new Calib5522A()}, Description = "Многофунциональный калибратор"
                }
            };

            Accessories = new[]
            {
                "Интерфейсный кабель для клибратора (GPIB или COM порт)",
                "Кабель banana - banana 2 шт.",
                "Интерфейсный каббора APPA-107N/APPA-109N USB-COM инфракрасный."
            };
            DocumentName = "APPA_107N_109N";
        }

        public class Oper1VisualTest : ParagraphBase<bool>
        {
            #region Property

            

            #endregion

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
                    service.Entity = new Tuple<string, Assembly>("VisualTest", null);
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

            /// <inheritdoc />
            protected override string[] GenerateDataColumnTypeObject()
            {
                return new[] {"Результат опробования"};
            }

            /// <inheritdoc />
            protected override string GetReportTableName()
            {
                return "ITBmOprobovanie";
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
                    service.Title = "Опробование";
                    service.Entity = new Tuple<string, Assembly>("Oprobovanie", null);
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

        //////////////////////////////******DCV*******///////////////////////////////

        #region DCV

        public class Oper3DcvMeasureBase : ParagraphBase<MeasPoint<Voltage>>
        {
            private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

            #region Fields

            /// <summary>
            /// Процент от измеряемой точки для расчета погрешности (уже переведе в абсолютные единицы).
            /// </summary>
            public decimal BaseTolCoeff = (decimal) 0.0006;

            /// <summary>
            /// Число пределов для данного режима.
            /// </summary>
            protected int CountOfRanges;

            /// <summary>
            /// довесок к формуле погрешности- число единиц младшего разряда
            /// </summary>
            public int EdMlRaz = 10; //значение для пределов свыше 200 мВ

            /// <summary>
            /// Разарешение пределеа измерения (последний значащий разряд)
            /// </summary>
            protected MeasPoint<Voltage> RangeResolution;

            

          

            /// <summary>
            /// Массив поверяемых точек напряжения.
            /// </summary>
            protected MeasPoint<Voltage>[] VoltPoint;

            #endregion

            #region Property

            /// <summary>
            /// Код предела измерения на поверяемого прибора.
            /// </summary>
            public Mult107_109N.RangeCode OperationDcRangeCode { get; protected set; }

            /// <summary>
            /// Предел измерения поверяемого прибора, необходимый для работы.
            /// </summary>
            public Mult107_109N.RangeNominal OperationDcRangeNominal { get; protected set; }

            /// <summary>
            /// Режим операции измерения прибора.
            /// </summary>
            public Mult107_109N.MeasureMode OperMeasureMode { get; protected set; }

            //контрлируемый прибор
            protected Mult107_109N AppaMult107109N { get; set; }

            //эталон
            protected Calib5522A flkCalib5522A { get; set; }

            #endregion

            public Oper3DcvMeasureBase(IUserItemOperation userItemOperation, string inResourceDir) :
                base(userItemOperation)
            {
               
                Name = "Определение погрешности измерения постоянного напряжения";
                OperMeasureMode = Mult107_109N.MeasureMode.DCV;

                OperationDcRangeCode = Mult107_109N.RangeCode.Range1Manual;
                OperationDcRangeNominal = Mult107_109N.RangeNominal.RangeNone;

                DataRow = new List<IBasicOperation<MeasPoint<Voltage>>>();
                Sheme = ShemeTemplateDefault.TemplateSheme;
                Sheme.AssemblyLocalName = inResourceDir;
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
                    dataRow[0] = OperationDcRangeNominal.GetStringValue();
                    dataRow[1] = dds.Expected?.MainPhysicalQuantity.ToString();
                    dataRow[2] = dds.Getting?.MainPhysicalQuantity.ToString();
                    dataRow[3] = dds.LowerTolerance?.MainPhysicalQuantity.ToString();
                    dataRow[4] = dds.UpperTolerance?.MainPhysicalQuantity.ToString();
                    if (dds.IsGood == null)
                        dataRow[5] = "не выполнено";
                    else
                        dataRow[5] = dds.IsGood() ? "Годен" : "Брак";
                    dataTable.Rows.Add(dataRow);
                }

                return dataTable;
            }

            /// <inheritdoc />
            protected override string[] GenerateDataColumnTypeObject()
            {
                return new[]
                {
                    "Предел измерения",
                    "Поверяемая точка", "Измеренное значение", "Минимальное допустимое значение",
                    "Максимальное допустимое значение"
                }.Concat(base.GenerateDataColumnTypeObject()).ToArray();
            }

            /// <inheritdoc />
            protected override string GetReportTableName()
            {
                return null;
            }

            protected override void InitWork(CancellationTokenSource token)
            {
                base.InitWork(token);
                if (AppaMult107109N == null || flkCalib5522A == null) return;

                foreach (var currPoint in VoltPoint)
                {
                    var operation = new BasicOperationVerefication<MeasPoint<Voltage>>();
                    operation.InitWork = async () =>
                    {
                        try
                        {
                            if (AppaMult107109N.StringConnection.Equals("COM1"))
                                AppaMult107109N.StringConnection = GetStringConnect(AppaMult107109N);
                            flkCalib5522A.StringConnection ??= GetStringConnect(flkCalib5522A);

                            await Task.Run(() => { flkCalib5522A.Out.SetOutput(CalibrMain.COut.State.Off); });

                            while (OperMeasureMode !=
                                   await Task<Mult107_109N.MeasureMode>.Factory.StartNew(() => AppaMult107109N.GetMeasureMode))
                                UserItemOperation.ServicePack.MessageBox()
                                                 .Show($"Установите режим измерения: {OperMeasureMode.GetStringValue()} {OperMeasureMode}",
                                                       "Указание оператору", MessageButton.OK,
                                                       MessageIcon.Information,
                                                       MessageResult.OK);

                            while (await Task<Mult107_109N.RangeSwitchMode>.Factory.StartNew(() => AppaMult107109N
                                                                                                .GetRangeSwitchMode) ==
                                   Mult107_109N.RangeSwitchMode.Auto)
                                UserItemOperation.ServicePack.MessageBox()
                                                 .Show("Установите ручной режим переключения пределов.");

                            while (OperationDcRangeNominal !=
                                   await Task<Mult107_109N.RangeNominal>
                                        .Factory.StartNew(() => AppaMult107109N.GetRangeNominal))
                            {
                                int countPushRangeButton;

                                if (currPoint.MainPhysicalQuantity.Multiplier == UnitMultiplier.Mili)
                                {
                                    CountOfRanges = 2;
                                    UserItemOperation.ServicePack.MessageBox()
                                                     .Show($"Текущий предел измерения прибора {AppaMult107109N.GetRangeNominal.GetStringValue()}\n Необходимо установить предел {OperationDcRangeNominal.GetStringValue()} " +
                                                           $"Нажмите на приборе клавишу Range {countPushRangeButton = 1} раз.",
                                                           "Указание оператору", MessageButton.OK,
                                                           MessageIcon.Information,
                                                           MessageResult.OK);
                                }
                                else
                                {
                                    //работает только для ручного режима переключения пределов
                                    CountOfRanges = 4;
                                    var curRange = (int) AppaMult107109N.GetRangeCode - 127;
                                    var targetRange = (int) OperationDcRangeCode - 127;
                                    countPushRangeButton =
                                        Hepls.CountOfPushButton(CountOfRanges, curRange, targetRange);

                                    UserItemOperation.ServicePack.MessageBox()
                                                     .Show($"Текущий предел измерения прибора {AppaMult107109N.GetRangeNominal.GetStringValue()}\n Необходимо установить предел {OperationDcRangeNominal.GetStringValue()} " +
                                                           $"Нажмите на приборе клавишу Range {countPushRangeButton} раз.",
                                                           "Указание оператору", MessageButton.OK,
                                                           MessageIcon.Information,
                                                           MessageResult.OK);
                                }
                            }
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
                            flkCalib5522A.Out.Set.Voltage.Dc.SetValue(currPoint);
                            flkCalib5522A.Out.ClearMemoryRegister();
                            flkCalib5522A.Out.SetOutput(CalibrMain.COut.State.On);
                            Thread.Sleep(2000);
                            //измеряем
                            var measurePoint = (decimal) AppaMult107109N.GetValue();
                            flkCalib5522A.Out.SetOutput(CalibrMain.COut.State.Off);

                            var mantisa =
                                MathStatistics.GetMantissa(RangeResolution
                                                          .MainPhysicalQuantity.GetNoramalizeValueToSi() /
                                                           currPoint.MainPhysicalQuantity.GetNoramalizeValueToSi());
                            //округляем измерения
                            MathStatistics.Round(ref measurePoint, mantisa);

                            operation.Getting =
                                new MeasPoint<Voltage>(measurePoint, currPoint.MainPhysicalQuantity.Multiplier);
                            
                            operation.Expected = currPoint;
                            //расчет погрешности для конкретной точки предела измерения
                            operation.ErrorCalculation = (inA, inB) =>
                            {
                                var result =
                                    BaseTolCoeff * Math.Abs(operation.Expected.MainPhysicalQuantity.Value) +
                                    EdMlRaz *
                                    RangeResolution.MainPhysicalQuantity.Value *
                                    (decimal) (RangeResolution
                                              .MainPhysicalQuantity.Multiplier.GetDoubleValue() /
                                               currPoint.MainPhysicalQuantity.Multiplier.GetDoubleValue());

                                MathStatistics.Round(ref result, mantisa);
                                return new MeasPoint<Voltage>(result, currPoint.MainPhysicalQuantity.Multiplier);
                            };

                            operation.LowerTolerance = operation.Expected - operation.Error;
                            operation.UpperTolerance = operation.Expected + operation.Error;

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
                    operation.CompliteWork = () => Hepls.HelpsCompliteWork(operation, UserItemOperation);
                    DataRow.Add(DataRow.IndexOf(operation) == -1
                                    ? operation
                                    : (BasicOperationVerefication<MeasPoint<Voltage>>) operation.Clone());
                }
            }

            #endregion

            
        }

        public class Oper3_1DC_2V_Measure : Oper3DcvMeasureBase
        {
            public Oper3_1DC_2V_Measure(Mult107_109N.RangeNominal inRangeNominal, IUserItemOperation userItemOperation,
                string inResourceDir) :
                base(userItemOperation, inResourceDir)
            {
                OperationDcRangeCode = Mult107_109N.RangeCode.Range1Manual;
                OperationDcRangeNominal = inRangeNominal;

                Name = OperationDcRangeNominal.GetStringValue();

                BaseTolCoeff = (decimal) 0.0006;
                EdMlRaz = 10;
                RangeResolution = new MeasPoint<Voltage>(100, UnitMultiplier.Micro);

                VoltPoint = new []
                {
                    new MeasPoint<Voltage>((decimal) 0.4),
                    new MeasPoint<Voltage>((decimal) 0.8),
                    new MeasPoint<Voltage>((decimal) 1.2),
                    new MeasPoint<Voltage>((decimal) 1.6),
                    new MeasPoint<Voltage>((decimal) 1.8),
                    new MeasPoint<Voltage>((decimal) -1.8)
                };
                
                
                
                
                
                
            }

            #region Methods

            /// <inheritdoc />
            protected override string GetReportTableName()
            {
                return "FillTabBmOper3_1DC_2V_Measure";
            }

            #endregion
        }

        public class Oper3_1DC_20V_Measure : Oper3DcvMeasureBase
        {
            public Oper3_1DC_20V_Measure(Mult107_109N.RangeNominal inRangeNominal, IUserItemOperation userItemOperation,
                string inResourceDir) :
                base(userItemOperation, inResourceDir)
            {
                OperationDcRangeCode = Mult107_109N.RangeCode.Range2Manual;
                OperationDcRangeNominal = inRangeNominal;
                RangeResolution = new MeasPoint<Voltage>(1, UnitMultiplier.Mili);
                Name = OperationDcRangeNominal.GetStringValue();

                VoltPoint = new []
                {
                    new MeasPoint<Voltage>(4 ),
                    new MeasPoint<Voltage>(8 ),
                    new MeasPoint<Voltage>(12),
                    new MeasPoint<Voltage>(16),
                    new MeasPoint<Voltage>(18),
                    new MeasPoint<Voltage>(-18)
                };
                 
            }

            #region Methods

            /// <inheritdoc />
            protected override string GetReportTableName()
            {
                return "FillTabBmOper3_1DC_20V_Measure";
                ;
            }

            #endregion
        }

        public class Oper3_1DC_200V_Measure : Oper3DcvMeasureBase
        {
            public Oper3_1DC_200V_Measure(Mult107_109N.RangeNominal inRangeNominal,
                IUserItemOperation userItemOperation, string inResourceDir) :
                base(userItemOperation, inResourceDir)
            {
                OperationDcRangeCode = Mult107_109N.RangeCode.Range3Manual;
                OperationDcRangeNominal = inRangeNominal;

                Name = OperationDcRangeNominal.GetStringValue();

                BaseTolCoeff = (decimal) 0.0006;
                EdMlRaz = 10;
                RangeResolution = new MeasPoint<Voltage>(10, UnitMultiplier.Mili);

                VoltPoint = new []
                {
                    new MeasPoint<Voltage>(40   ),
                    new MeasPoint<Voltage>(80   ),
                    new MeasPoint<Voltage>(120  ),
                    new MeasPoint<Voltage>(160  ),
                    new MeasPoint<Voltage>(180  ),
                    new MeasPoint<Voltage>(-180 )
                };
                 
            }

            #region Methods

            /// <inheritdoc />
            protected override string GetReportTableName()
            {
                return "FillTabBmOper3_1DC_200V_Measure";
            }

            #endregion
        }

        public class Oper3_1DC_1000V_Measure : Oper3DcvMeasureBase
        {
            public Oper3_1DC_1000V_Measure(Mult107_109N.RangeNominal inRangeNominal,
                IUserItemOperation userItemOperation, string inResourceDir) :
                base(userItemOperation, inResourceDir)
            {
                OperationDcRangeCode = Mult107_109N.RangeCode.Range4Manual;
                OperationDcRangeNominal = inRangeNominal;

                Name = OperationDcRangeNominal.GetStringValue();

                BaseTolCoeff = (decimal) 0.0006;
                EdMlRaz = 10;
                RangeResolution = new MeasPoint<Voltage>(100, UnitMultiplier.Mili);

                VoltPoint = new []
                {
                    new MeasPoint<Voltage>(100 ),
                    new MeasPoint<Voltage>(200 ),
                    new MeasPoint<Voltage>(400 ),
                    new MeasPoint<Voltage>(700 ),
                    new MeasPoint<Voltage>(900 ),
                    new MeasPoint<Voltage>(-900)
                };
                
                
                
                
                
                
            }

            #region Methods

            /// <inheritdoc />
            protected override string GetReportTableName()
            {
                return "FillTabBmOper3_1DC_1000V_Measure";
            }

            #endregion
        }

        public class Oper3_1DC_20mV_Measure : Oper3DcvMeasureBase
        {
            public Oper3_1DC_20mV_Measure(Mult107_109N.RangeNominal inRangeNominal,
                IUserItemOperation userItemOperation, string inResourceDir) :
                base(userItemOperation, inResourceDir)
            {
                
                OperationDcRangeCode = Mult107_109N.RangeCode.Range1Manual;
                OperationDcRangeNominal = inRangeNominal;
                RangeResolution = new MeasPoint<Voltage>(1, UnitMultiplier.Micro);
                Name = OperationDcRangeNominal.GetStringValue();

                BaseTolCoeff = (decimal) 0.0006;
                EdMlRaz = 60;
                RangeResolution = new MeasPoint<Voltage>(1, UnitMultiplier.Micro);

                VoltPoint = new []
                {
                    new MeasPoint<Voltage>(4,   UnitMultiplier.Mili),
                    new MeasPoint<Voltage>(8, UnitMultiplier.Mili),
                    new MeasPoint<Voltage>(12, UnitMultiplier.Mili),
                    new MeasPoint<Voltage>(16, UnitMultiplier.Mili),
                    new MeasPoint<Voltage>(18, UnitMultiplier.Mili),
                    new MeasPoint<Voltage>(-18, UnitMultiplier.Mili)
            };
                
                
                
                
                
                
            }

            #region Methods

            /// <inheritdoc />
            protected override string GetReportTableName()
            {
                return "FillTabBmOper3_1DC_20mV_Measure";
            }

            #endregion
        }

        public class Oper3_1DC_200mV_Measure : Oper3DcvMeasureBase
        {
            public Oper3_1DC_200mV_Measure(Mult107_109N.RangeNominal inRangeNominal,
                IUserItemOperation userItemOperation, string inResourceDir) :
                base(userItemOperation, inResourceDir)
            {
                
                OperationDcRangeCode = Mult107_109N.RangeCode.Range1Manual;
                OperationDcRangeNominal = inRangeNominal;
                RangeResolution = new MeasPoint<Voltage>(10, UnitMultiplier.Micro);
                Name = OperationDcRangeNominal.GetStringValue();

                BaseTolCoeff = (decimal) 0.0006;
                EdMlRaz = 20;
                RangeResolution = new MeasPoint<Voltage>(10, UnitMultiplier.Micro);

                VoltPoint = new []
                {
                    new MeasPoint<Voltage>(40,   UnitMultiplier.Mili),
                    new MeasPoint<Voltage>(80,   UnitMultiplier.Mili),
                    new MeasPoint<Voltage>(120,  UnitMultiplier.Mili),
                    new MeasPoint<Voltage>(160,  UnitMultiplier.Mili),
                    new MeasPoint<Voltage>(180,  UnitMultiplier.Mili),
                    new MeasPoint<Voltage>(-180, UnitMultiplier.Mili)
                };
                 
            }

            #region Methods

            /// <inheritdoc />
            protected override string GetReportTableName()
            {
                return "FillTabBmOper3_1DC_200mV_Measure";
            }

            #endregion
        }

        internal static class ShemeTemplateDefault
        {
            public static readonly ShemeImage TemplateSheme;

            static ShemeTemplateDefault()
            {
                TemplateSheme = new ShemeImage
                {
                    AssemblyLocalName = Assembly.GetExecutingAssembly().GetName().Name,
                    Description = "Измерительная схема",
                    Number = 1,
                    FileName = @"appa_10XN_volt_hz_5522A.jpg",
                    ExtendedDescription = "Соберите измерительную схему, согласно рисунку"
                };
            }
        }

        #endregion DCV

        //////////////////////////////******ACV*******///////////////////////////////

        #region ACV

        public  class Oper4AcvMeasureBase : ParagraphBase<MeasPoint<Voltage>>
        {
            private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

            #region Fields

            /// <summary>
            /// Число пределов для данного режима.
            /// </summary>
            protected int CountOfRanges;

            ///// <summary>
            ///// Имя закладки таблички в результирующем протоколе doc (Ms Word).
            ///// </summary>
            //protected string ReportTableName;

            /// <summary>
            /// Это пустая точка, которая содержит только единицы измерения текущего
            /// поверяемого предела и множитель для единицы измерения.
            /// </summary>
            //protected MeasPoint<Voltage> thisRangeUnits;

            /// <summary>
            /// Множитель для поверяемых точек. (Если точки можно посчитать простым умножением).
            /// </summary>
            protected decimal VoltMultiplier;

            /// <summary>
            /// Итоговый массив поверяемых точек. У каждого номинала напряжения вложены номиналы частот для текущей точки.
            /// </summary>
            public List<MeasPoint<Voltage, Frequency>> VoltPoint = new List<MeasPoint<Voltage, Frequency>>();

            #endregion

            #region Property

            /// <summary>
            /// Код предела измерения на приборе
            /// </summary>
            public Mult107_109N.RangeCode OperationAcRangeCode { get; protected set; }

            /// <summary>
            /// Предел измерения поверяемого прибора, необходимый для работы
            /// </summary>
            public Mult107_109N.RangeNominal OperationAcRangeNominal { get; protected set; }

            /// <summary>
            /// Режим операции измерения прибора
            /// </summary>
            public Mult107_109N.MeasureMode OperMeasureMode { get; protected set; }

            //контрлируемый прибор
            protected Mult107_109N appa107N { get; set; }

            //эталон
            protected Calib5522A flkCalib5522A { get; set; }

            #endregion

            public Oper4AcvMeasureBase(IUserItemOperation userItemOperation, string inResourceDir) :
                base(userItemOperation)
            {
                Name = "Определение погрешности измерения переменного напряжения";
                OperMeasureMode = Mult107_109N.MeasureMode.ACV;

                OperationAcRangeCode = Mult107_109N.RangeCode.Range1Manual;
                OperationAcRangeNominal = Mult107_109N.RangeNominal.RangeNone;
                DataRow = new List<IBasicOperation<MeasPoint<Voltage>>>();
                Sheme = ShemeTemplateDefault.TemplateSheme;
                Sheme.AssemblyLocalName = inResourceDir;
            }

            #region Methods

            protected override DataTable FillData()
            {
                var dataTable = base.FillData();

                foreach (var row in DataRow)
                {
                    var dataRow = dataTable.NewRow();
                    var dds = row as BasicOperationVerefication<MeasPoint<Voltage, Frequency>>;
                    // ReSharper disable once PossibleNullReferenceException
                    if (dds == null) continue;
                    dataRow[0] = OperationAcRangeNominal.GetStringValue();
                    dataRow[1] = dds.Expected?.MainPhysicalQuantity.ToString();
                    //Нужно выводить в таблицу напряжение и частоты!!!
                    dataRow[2] = dds.Expected?.AdditionalPhysicalQuantity.ToString();
                    dataRow[3] = dds.Getting?.Description;
                    dataRow[4] = dds.LowerTolerance?.Description;
                    dataRow[5] = dds.UpperTolerance?.Description;
                    if (dds.IsGood == null)
                        dataRow[6] = "не выполнено";
                    else
                        dataRow[6] = dds.IsGood() ? "Годен" : "Брак";
                    dataTable.Rows.Add(dataRow);
                }

                return dataTable;
            }

            /// <inheritdoc />
            protected override string[] GenerateDataColumnTypeObject()
            {
                return new[]
                {
                    "Предел измерения",
                    "Поверяемая точка", "Измеренное значение", "Минимальное допустимое значение",
                    "Максимальное допустимое значение"
                }.Concat(base.GenerateDataColumnTypeObject()).ToArray();
                ;
            }

            /// <inheritdoc />
            protected override string GetReportTableName()
            {
                return null;
            }

            protected override void InitWork(CancellationTokenSource token)
            {
                base.InitWork(token);
                if (flkCalib5522A == null || appa107N == null) return;

                foreach (var point in VoltPoint)
                {
                    var operation = new BasicOperationVerefication<MeasPoint<Voltage>>();
                    operation.InitWork = async () =>
                    {
                        try
                        {
                            if (appa107N.StringConnection.Equals("COM1"))
                                appa107N.StringConnection = GetStringConnect(appa107N);
                            flkCalib5522A.StringConnection ??= GetStringConnect(flkCalib5522A);

                            await Task.Run(() => { flkCalib5522A.Out.SetOutput(CalibrMain.COut.State.Off); });

                            var testMeasureModde = appa107N.GetMeasureMode;
                            while (OperMeasureMode !=
                                   await Task<Mult107_109N.MeasureMode>.Factory.StartNew(() => appa107N.GetMeasureMode))
                                UserItemOperation.ServicePack.MessageBox()
                                                 .Show($"Установите режим измерения: {OperMeasureMode.GetStringValue()} {OperMeasureMode}",
                                                       "Указание оператору", MessageButton.OK,
                                                       MessageIcon.Information,
                                                       MessageResult.OK);

                            while (await Task<Mult107_109N.RangeSwitchMode>.Factory.StartNew(() => appa107N
                                                                                                .GetRangeSwitchMode) ==
                                   Mult107_109N.RangeSwitchMode.Auto)
                                UserItemOperation.ServicePack.MessageBox()
                                                 .Show("Установите ручной режим переключения пределов.");

                            while (OperationAcRangeNominal !=
                                   await Task<Mult107_109N.RangeNominal>
                                        .Factory.StartNew(() => appa107N.GetRangeNominal))
                            {
                                int countPushRangeButton;

                                if (point.MainPhysicalQuantity.Multiplier == UnitMultiplier.Mili)
                                {
                                    UserItemOperation.ServicePack.MessageBox()
                                                     .Show($"Текущий предел измерения прибора {appa107N.GetRangeNominal.GetStringValue()}\n Необходимо установить предел {OperationAcRangeNominal.GetStringValue()} " +
                                                           $"Нажмите на приборе клавишу Range {countPushRangeButton = 1} раз.",
                                                           "Указание оператору", MessageButton.OK,
                                                           MessageIcon.Information,
                                                           MessageResult.OK);
                                }
                                else
                                {
                                    //работает только для ручного режима переключения пределов
                                    CountOfRanges = 4;
                                    var curRange = (int) appa107N.GetRangeCode - 127;
                                    var targetRange = (int) OperationAcRangeCode - 127;
                                    countPushRangeButton =
                                        Hepls.CountOfPushButton(CountOfRanges, curRange, targetRange);

                                    UserItemOperation.ServicePack.MessageBox()
                                                     .Show($"Текущий предел измерения прибора {appa107N.GetRangeNominal.GetStringValue()}\n Необходимо установить предел {OperationAcRangeNominal.GetStringValue()} " +
                                                           $"Нажмите на приборе клавишу Range {countPushRangeButton} раз.",
                                                           "Указание оператору", MessageButton.OK,
                                                           MessageIcon.Information,
                                                           MessageResult.OK);
                                }
                            }
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
                            //вычисляе на сколько знаков округлять
                            var mantisa =
                                MathStatistics.GetMantissa(RangeResolution
                                                          .MainPhysicalQuantity.GetNoramalizeValueToSi() /
                                                           point.MainPhysicalQuantity.GetNoramalizeValueToSi());

                            operation.Expected = point;

                            //расчет погрешности для конкретной точки предела измерения
                            ConstructTooleranceFormula(new MeasPoint<Frequency>(point.AdditionalPhysicalQuantity.Value,
                                                                                point.AdditionalPhysicalQuantity
                                                                                     .Multiplier)); // функция подбирает коэффициенты для формулы погрешности
                            operation.ErrorCalculation = (inA, inB) =>
                            {
                                var result = BaseTolCoeff * Math.Abs(operation.Expected.MainPhysicalQuantity.Value) +
                                             EdMlRaz *
                                             RangeResolution.MainPhysicalQuantity.Value *
                                             (decimal) (RangeResolution
                                                       .MainPhysicalQuantity.Multiplier.GetDoubleValue() /
                                                        point.MainPhysicalQuantity.Multiplier
                                                             .GetDoubleValue()
                                             );

                                MathStatistics.Round(ref result, mantisa);
                                return new MeasPoint<Voltage>(result, point.MainPhysicalQuantity.Multiplier);
                            };

                            operation.LowerTolerance = operation.Expected - operation.Error;

                            operation.UpperTolerance = operation.Expected + operation.Error;

                            operation.IsGood = () =>
                            {
                                if (operation.Getting == null || operation.Expected == null ||
                                    operation.UpperTolerance == null || operation.LowerTolerance == null) return false;
                                return (operation.Getting < operation.UpperTolerance) &
                                       (operation.Getting > operation.LowerTolerance);
                            };

                            decimal measurePoint = 0;

                            //todo тут есть точки которые не может воспроизвести калибратор, это нужно проверять как-то
                            flkCalib5522A.Out.Set.Voltage.Ac.SetValue(point);
                            flkCalib5522A.Out.ClearMemoryRegister();
                            flkCalib5522A.Out.SetOutput(CalibrMain.COut.State.On);
                            Thread.Sleep(2000);
                            //измеряем
                            measurePoint = (decimal) appa107N.GetValue();
                            flkCalib5522A.Out.SetOutput(CalibrMain.COut.State.Off);

                            //округляем измерения
                            MathStatistics.Round(ref measurePoint, mantisa);

                            operation.Getting =
                                new MeasPoint<Voltage>(measurePoint, point.MainPhysicalQuantity.Multiplier);
                        }
                        catch (Exception e)
                        {
                            Logger.Error(e);
                            throw;
                        }
                    };
                    operation.CompliteWork = () => Hepls.HelpsCompliteWork(operation, UserItemOperation);
                    DataRow.Add(DataRow.IndexOf(operation) == -1
                                    ? operation
                                    : (BasicOperationVerefication<MeasPoint<Voltage>>) operation.Clone());
                }
            }

            /// <summary>
            /// Метод выбирает необходимый значения коэффициентов для формулы погрешности, исходя из предела измерения и диапазона
            /// частот.
            /// </summary>
            /// <returns>Результат вычисления.</returns>
            private void ConstructTooleranceFormula(MeasPoint<Frequency> inFreq)
            {
                //разрешение предела измерения должно быть проинициализировано в коснтсрукторе соответсвующего класса

                if ((OperationAcRangeNominal == Mult107_109N.RangeNominal.Range200mV ||
                     OperationAcRangeNominal == Mult107_109N.RangeNominal.Range20mV) &&
                    inFreq.MainPhysicalQuantity.Multiplier == UnitMultiplier.None)
                {
                    EdMlRaz = 80;
                    if (inFreq.MainPhysicalQuantity.Value >= 40 && inFreq.MainPhysicalQuantity.Value <= 100)
                        BaseTolCoeff = (decimal) 0.007;
                    if (inFreq.MainPhysicalQuantity.Value > 100 && inFreq.MainPhysicalQuantity.Value <= 1000)
                        BaseTolCoeff = (decimal) 0.01;
                    return;
                }

                if (OperationAcRangeNominal == Mult107_109N.RangeNominal.Range2V ||
                    OperationAcRangeNominal == Mult107_109N.RangeNominal.Range20V ||
                    OperationAcRangeNominal == Mult107_109N.RangeNominal.Range200V)
                {
                    if (inFreq.MainPhysicalQuantity.Multiplier == UnitMultiplier.None)
                    {
                        EdMlRaz = 50;
                        if (inFreq.MainPhysicalQuantity.Value >= 40 && inFreq.MainPhysicalQuantity.Value <= 100)
                            BaseTolCoeff = (decimal) 0.007;
                        if (inFreq.MainPhysicalQuantity.Value > 100 && inFreq.MainPhysicalQuantity.Value <= 1000)
                            BaseTolCoeff = (decimal) 0.01;
                        return;
                    }

                    if (inFreq.MainPhysicalQuantity.Multiplier == UnitMultiplier.Kilo)
                    {
                        if (inFreq.MainPhysicalQuantity.Value >= 1 && inFreq.MainPhysicalQuantity.Value <= 10)
                        {
                            BaseTolCoeff = (decimal) 0.02;
                            EdMlRaz = 60;
                            return;
                        }

                        if (inFreq.MainPhysicalQuantity.Value > 10 && inFreq.MainPhysicalQuantity.Value <= 20)
                        {
                            BaseTolCoeff = (decimal) 0.03;
                            EdMlRaz = 70;
                            return;
                        }

                        if (inFreq.MainPhysicalQuantity.Value > 20 && inFreq.MainPhysicalQuantity.Value <= 50)
                        {
                            BaseTolCoeff = (decimal) 0.05;
                            EdMlRaz = 80;
                            return;
                        }

                        if (inFreq.MainPhysicalQuantity.Value > 50 && inFreq.MainPhysicalQuantity.Value <= 100)
                        {
                            BaseTolCoeff = (decimal) 0.1;
                            EdMlRaz = 100;
                            return;
                        }
                    }
                }

                if (OperationAcRangeNominal == Mult107_109N.RangeNominal.Range750V)
                {
                    EdMlRaz = 50;
                    if (inFreq.MainPhysicalQuantity.Value >= 40 && inFreq.MainPhysicalQuantity.Value <= 100)
                        BaseTolCoeff = (decimal) 0.007;
                    if (inFreq.MainPhysicalQuantity.Value > 100 && inFreq.MainPhysicalQuantity.Value <= 1000)
                        BaseTolCoeff = (decimal) 0.01;
                }
            }

            #endregion

            //public override async Task StartWork(CancellationToken token)
            //{
            //    await base.StartWork(token);
            //    appa107N?.Dispose();
            //}

            #region TolleranceFormula

            /// <summary>
            /// Процент от измеряемой точки для расчета погрешности (уже переведе в абсолютные единицы).
            /// </summary>
            public decimal BaseTolCoeff;

            /// <summary>
            /// довесок к формуле погрешности- число единиц младшего разряда
            /// </summary>
            public int EdMlRaz;

            /// <summary>
            /// Разарешение пределеа измерения (последний значащий разряд)
            /// </summary>
            protected MeasPoint<Voltage> RangeResolution;

            #endregion TolleranceFormula
        }

        public class Ope4_1_AcV_20mV_Measure : Oper4AcvMeasureBase
        {
            public Ope4_1_AcV_20mV_Measure(Mult107_109N.RangeNominal inRangeNominal,
                IUserItemOperation userItemOperation, string inResourceDir) :
                base(userItemOperation, inResourceDir)
            {
                OperMeasureMode = Mult107_109N.MeasureMode.ACmV;
                OperationAcRangeCode = Mult107_109N.RangeCode.Range1Manual;
                OperationAcRangeNominal = inRangeNominal;
                Name = OperationAcRangeNominal.GetStringValue();
                Sheme = ShemeTemplateDefault.TemplateSheme;

                VoltMultiplier = 1;

                RangeResolution = new MeasPoint<Voltage>(1, UnitMultiplier.Micro);

                VoltPoint.Add(new MeasPoint<Voltage, Frequency>(4 * VoltMultiplier, UnitMultiplier.Mili,
                                                                new Frequency {Value = 40}));
                VoltPoint.Add(new MeasPoint<Voltage, Frequency>(4 * VoltMultiplier, UnitMultiplier.Mili,
                                                                new Frequency {Value = 1000}));
                VoltPoint.Add(new MeasPoint<Voltage, Frequency>(10 * VoltMultiplier, UnitMultiplier.Mili,
                                                                new Frequency {Value = 40}));
                VoltPoint.Add(new MeasPoint<Voltage, Frequency>(10 * VoltMultiplier, UnitMultiplier.Mili,
                                                                new Frequency {Value = 1000}));
                VoltPoint.Add(new MeasPoint<Voltage, Frequency>(18 * VoltMultiplier, UnitMultiplier.Mili,
                                                                new Frequency {Value = 40}));
                VoltPoint.Add(new MeasPoint<Voltage, Frequency>(18 * VoltMultiplier, UnitMultiplier.Mili,
                                                                new Frequency {Value = 1000}));
            }

            #region Methods

            /// <inheritdoc />
            protected override string GetReportTableName()
            {
                return "FillTabBmOpe4_1_AcV_20mV_Measure";
            }

            #endregion
        }

        public class Ope4_1_AcV_200mV_Measure : Oper4AcvMeasureBase
        {
            public Ope4_1_AcV_200mV_Measure(Mult107_109N.RangeNominal inRangeNominal,
                IUserItemOperation userItemOperation, string inResourceDir)
                : base(userItemOperation, inResourceDir)
            {
                OperMeasureMode = Mult107_109N.MeasureMode.ACmV;
                OperationAcRangeCode = Mult107_109N.RangeCode.Range2Manual;
                OperationAcRangeNominal = inRangeNominal;
                Name = OperationAcRangeNominal.GetStringValue();
                Sheme = ShemeTemplateDefault.TemplateSheme;

                VoltMultiplier = 10;

                RangeResolution = new MeasPoint<Voltage>(10, UnitMultiplier.Micro);

                VoltPoint.Add(new MeasPoint<Voltage, Frequency>(4 * VoltMultiplier, UnitMultiplier.Mili,
                                                                new Frequency {Value = 40}));
                VoltPoint.Add(new MeasPoint<Voltage, Frequency>(4 * VoltMultiplier, UnitMultiplier.Mili,
                                                                new Frequency {Value = 1000}));
                VoltPoint.Add(new MeasPoint<Voltage, Frequency>(10 * VoltMultiplier, UnitMultiplier.Mili,
                                                                new Frequency {Value = 40}));
                VoltPoint.Add(new MeasPoint<Voltage, Frequency>(10 * VoltMultiplier, UnitMultiplier.Mili,
                                                                new Frequency {Value = 1000}));
                VoltPoint.Add(new MeasPoint<Voltage, Frequency>(18 * VoltMultiplier, UnitMultiplier.Mili,
                                                                new Frequency {Value = 40}));
                VoltPoint.Add(new MeasPoint<Voltage, Frequency>(18 * VoltMultiplier, UnitMultiplier.Mili,
                                                                new Frequency {Value = 1000}));
            }

            #region Methods

            /// <inheritdoc />
            protected override string GetReportTableName()
            {
                return "FillTabBmOpe4_1_AcV_200mV_Measure";
            }

            #endregion
        }

        public class Ope4_1_AcV_2V_Measure : Oper4AcvMeasureBase
        {
            public Ope4_1_AcV_2V_Measure(Mult107_109N.RangeNominal inRangeNominal, IUserItemOperation userItemOperation,
                string inResourceDir) :
                base(userItemOperation, inResourceDir)
            {
                OperMeasureMode = Mult107_109N.MeasureMode.ACV;
                OperationAcRangeCode = Mult107_109N.RangeCode.Range1Manual;
                OperationAcRangeNominal = inRangeNominal;
                Name = OperationAcRangeNominal.GetStringValue();
                Sheme = ShemeTemplateDefault.TemplateSheme;

                VoltMultiplier = 1;

                RangeResolution = new MeasPoint<Voltage>((decimal) 0.1, UnitMultiplier.Mili);

                //конкретно для первой точки 0.2 нужны не все частоты, поэтому вырежем только необходимые
                VoltPoint.Add(new MeasPoint<Voltage, Frequency>((decimal) 0.2, new Frequency {Value = 40}));
                VoltPoint.Add(new MeasPoint<Voltage, Frequency>((decimal) 0.2, new Frequency {Value = 1000}));
                VoltPoint.Add(new MeasPoint<Voltage, Frequency>((decimal) 0.2,
                                                                new Frequency
                                                                    {Value = 10, Multiplier = UnitMultiplier.Kilo}));
                VoltPoint.Add(new MeasPoint<Voltage, Frequency>((decimal) 0.2,
                                                                new Frequency
                                                                    {Value = 20, Multiplier = UnitMultiplier.Kilo}));

                VoltPoint.Add(new MeasPoint<Voltage, Frequency>(1, new Frequency {Value = 40 * VoltMultiplier}));
                VoltPoint.Add(new MeasPoint<Voltage, Frequency>(1, new Frequency {Value = 1000 * VoltMultiplier}));
                VoltPoint.Add(new MeasPoint<Voltage, Frequency>(1,
                                                                new Frequency
                                                                {
                                                                    Value = 10 * VoltMultiplier,
                                                                    Multiplier = UnitMultiplier.Kilo
                                                                }));
                VoltPoint.Add(new MeasPoint<Voltage, Frequency>(1,
                                                                new Frequency
                                                                {
                                                                    Value = 20 * VoltMultiplier,
                                                                    Multiplier = UnitMultiplier.Kilo
                                                                }));
                VoltPoint.Add(new MeasPoint<Voltage, Frequency>(1,
                                                                new Frequency
                                                                {
                                                                    Value = 50 * VoltMultiplier,
                                                                    Multiplier = UnitMultiplier.Kilo
                                                                }));
                VoltPoint.Add(new MeasPoint<Voltage, Frequency>(1,
                                                                new Frequency
                                                                {
                                                                    Value = 100 * VoltMultiplier,
                                                                    Multiplier = UnitMultiplier.Kilo
                                                                }));

                VoltPoint.Add(new MeasPoint<Voltage, Frequency>((decimal) 1.8, new Frequency {Value = 40}));
                VoltPoint.Add(new MeasPoint<Voltage, Frequency>((decimal) 1.8, new Frequency {Value = 1000}));
                VoltPoint.Add(new MeasPoint<Voltage, Frequency>((decimal) 1.8,
                                                                new Frequency
                                                                    {Value = 10, Multiplier = UnitMultiplier.Kilo}));
                VoltPoint.Add(new MeasPoint<Voltage, Frequency>((decimal) 1.8,
                                                                new Frequency
                                                                    {Value = 20, Multiplier = UnitMultiplier.Kilo}));
                VoltPoint.Add(new MeasPoint<Voltage, Frequency>((decimal) 1.8,
                                                                new Frequency
                                                                    {Value = 50, Multiplier = UnitMultiplier.Kilo}));
                VoltPoint.Add(new MeasPoint<Voltage, Frequency>((decimal) 1.8,
                                                                new Frequency
                                                                    {Value = 100, Multiplier = UnitMultiplier.Kilo}));
            }

            #region Methods

            /// <inheritdoc />
            protected override string GetReportTableName()
            {
                return "FillTabBmOpe4_1_AcV_2V_Measure";
            }

            #endregion
        }

        public class Ope4_1_AcV_20V_Measure : Oper4AcvMeasureBase
        {
            public Ope4_1_AcV_20V_Measure(Mult107_109N.RangeNominal inRangeNominal,
                IUserItemOperation userItemOperation, string inResourceDir) :
                base(userItemOperation, inResourceDir)
            {
                OperMeasureMode = Mult107_109N.MeasureMode.ACV;
                OperationAcRangeCode = Mult107_109N.RangeCode.Range2Manual;
                OperationAcRangeNominal = inRangeNominal;
                Name = OperationAcRangeNominal.GetStringValue();
                Sheme = ShemeTemplateDefault.TemplateSheme;

                VoltMultiplier = 10;

                RangeResolution = new MeasPoint<Voltage>(1, UnitMultiplier.Mili);

                //конкретно для первой точки 2 нужны не все частоты, поэтому вырежем только необходимые

                VoltPoint.Add(new MeasPoint<Voltage, Frequency>((decimal) 0.2 * VoltMultiplier,
                                                                new Frequency {Value = 40}));
                VoltPoint.Add(new MeasPoint<Voltage, Frequency>((decimal) 0.2 * VoltMultiplier,
                                                                new Frequency {Value = 1000}));
                VoltPoint.Add(new MeasPoint<Voltage, Frequency>((decimal) 0.2 * VoltMultiplier,
                                                                new Frequency
                                                                    {Value = 10, Multiplier = UnitMultiplier.Kilo}));
                VoltPoint.Add(new MeasPoint<Voltage, Frequency>((decimal) 0.2 * VoltMultiplier,
                                                                new Frequency
                                                                    {Value = 20, Multiplier = UnitMultiplier.Kilo}));

                VoltPoint.Add(new MeasPoint<Voltage, Frequency>(1 * VoltMultiplier, new Frequency {Value = 40}));
                VoltPoint.Add(new MeasPoint<Voltage, Frequency>(1 * VoltMultiplier, new Frequency {Value = 1000}));
                VoltPoint.Add(new MeasPoint<Voltage, Frequency>(1 * VoltMultiplier,
                                                                new Frequency
                                                                    {Value = 10, Multiplier = UnitMultiplier.Kilo}));
                VoltPoint.Add(new MeasPoint<Voltage, Frequency>(1 * VoltMultiplier,
                                                                new Frequency
                                                                    {Value = 20, Multiplier = UnitMultiplier.Kilo}));
                VoltPoint.Add(new MeasPoint<Voltage, Frequency>(1 * VoltMultiplier,
                                                                new Frequency
                                                                    {Value = 50, Multiplier = UnitMultiplier.Kilo}));
                VoltPoint.Add(new MeasPoint<Voltage, Frequency>(1 * VoltMultiplier,
                                                                new Frequency
                                                                    {Value = 100, Multiplier = UnitMultiplier.Kilo}));

                VoltPoint.Add(new MeasPoint<Voltage, Frequency>((decimal) 1.8 * VoltMultiplier,
                                                                new Frequency {Value = 40}));
                VoltPoint.Add(new MeasPoint<Voltage, Frequency>((decimal) 1.8 * VoltMultiplier,
                                                                new Frequency {Value = 1000}));
                VoltPoint.Add(new MeasPoint<Voltage, Frequency>((decimal) 1.8 * VoltMultiplier,
                                                                new Frequency
                                                                    {Value = 10, Multiplier = UnitMultiplier.Kilo}));
                VoltPoint.Add(new MeasPoint<Voltage, Frequency>((decimal) 1.8 * VoltMultiplier,
                                                                new Frequency
                                                                    {Value = 20, Multiplier = UnitMultiplier.Kilo}));
                VoltPoint.Add(new MeasPoint<Voltage, Frequency>((decimal) 1.8 * VoltMultiplier,
                                                                new Frequency
                                                                    {Value = 50, Multiplier = UnitMultiplier.Kilo}));
                VoltPoint.Add(new MeasPoint<Voltage, Frequency>((decimal) 1.8 * VoltMultiplier,
                                                                new Frequency
                                                                    {Value = 100, Multiplier = UnitMultiplier.Kilo}));
            }

            #region Methods

            /// <inheritdoc />
            protected override string GetReportTableName()
            {
                return "FillTabBmOpe4_1_AcV_20V_Measure";
            }

            #endregion
        }

        public class Ope4_1_AcV_200V_Measure : Oper4AcvMeasureBase
        {
            public Ope4_1_AcV_200V_Measure(Mult107_109N.RangeNominal inRangeNominal,
                IUserItemOperation userItemOperation, string inResourceDir) :
                base(userItemOperation, inResourceDir)
            {
                OperMeasureMode = Mult107_109N.MeasureMode.ACV;
                OperationAcRangeCode = Mult107_109N.RangeCode.Range3Manual;
                OperationAcRangeNominal = inRangeNominal;
                Name = OperationAcRangeNominal.GetStringValue();
                Sheme = ShemeTemplateDefault.TemplateSheme;

                VoltMultiplier = 100;

                RangeResolution = new MeasPoint<Voltage>(10, UnitMultiplier.Mili);

                //RangeStorage storage = new RangeStorage(new PhysicalRange<Voltage>(new MeasPoint<Voltage>(1), new MeasPoint<Voltage>(10)),
                //                                        new PhysicalRange<Frequency>(new MeasPoint<Frequency>(1000), new MeasPoint<Frequency>(5000)));

                Frequency[] HerzVPoint = { };
                new Frequency {Value = 40, Multiplier = UnitMultiplier.None}; //.IsFake = true;
                new Frequency {Value = 1000, Multiplier = UnitMultiplier.None};
                new Frequency {Value = 10, Multiplier = UnitMultiplier.Kilo};
                new Frequency {Value = 20, Multiplier = UnitMultiplier.Kilo};
                new Frequency {Value = 50, Multiplier = UnitMultiplier.Kilo};
                new Frequency {Value = 100, Multiplier = UnitMultiplier.Kilo}; //.IsFake  =true;

                //конкретно для первой точки 0.2 нужны не все частоты, поэтому вырежем только необходимые
                VoltPoint.Add(new MeasPoint<Voltage, Frequency>((decimal) 0.2 * VoltMultiplier,
                                                                new Frequency
                                                                {
                                                                    Value = 40,
                                                                    Multiplier = UnitMultiplier.None
                                                                })); //.IsFake = true;
                VoltPoint.Add(new MeasPoint<Voltage, Frequency>((decimal) 0.2 * VoltMultiplier,
                                                                new Frequency
                                                                    {Value = 1000, Multiplier = UnitMultiplier.None}));
                VoltPoint.Add(new MeasPoint<Voltage, Frequency>((decimal) 0.2 * VoltMultiplier,
                                                                new Frequency
                                                                    {Value = 10, Multiplier = UnitMultiplier.Kilo}));
                VoltPoint.Add(new MeasPoint<Voltage, Frequency>((decimal) 0.2 * VoltMultiplier,
                                                                new Frequency
                                                                    {Value = 20, Multiplier = UnitMultiplier.Kilo}));

                VoltPoint.Add(new MeasPoint<Voltage, Frequency>(1 * VoltMultiplier,
                                                                new Frequency
                                                                {
                                                                    Value = 40,
                                                                    Multiplier = UnitMultiplier.None
                                                                })); //.IsFake = true;
                VoltPoint.Add(new MeasPoint<Voltage, Frequency>(1 * VoltMultiplier,
                                                                new Frequency
                                                                    {Value = 1000, Multiplier = UnitMultiplier.None}));
                VoltPoint.Add(new MeasPoint<Voltage, Frequency>(1 * VoltMultiplier,
                                                                new Frequency
                                                                    {Value = 10, Multiplier = UnitMultiplier.Kilo}));
                VoltPoint.Add(new MeasPoint<Voltage, Frequency>(1 * VoltMultiplier,
                                                                new Frequency
                                                                    {Value = 20, Multiplier = UnitMultiplier.Kilo}));
                VoltPoint.Add(new MeasPoint<Voltage, Frequency>(1 * VoltMultiplier,
                                                                new Frequency
                                                                    {Value = 50, Multiplier = UnitMultiplier.Kilo}));
                VoltPoint.Add(new MeasPoint<Voltage, Frequency>(1 * VoltMultiplier,
                                                                new Frequency
                                                                {
                                                                    Value = 100,
                                                                    Multiplier = UnitMultiplier.Kilo
                                                                })); //.IsFake  =true;

                //VoltPoint[2] = new AcVariablePoint((decimal)1.8 * VoltMultiplier, thisRangeUnits.Units, thisRangeUnits.Multipliers, HerzVPoint, true);
                VoltPoint.Add(new MeasPoint<Voltage, Frequency>((decimal) 1.8 * VoltMultiplier,
                                                                new Frequency
                                                                {
                                                                    Value = 40,
                                                                    Multiplier = UnitMultiplier.None
                                                                })); //.IsFake = true;
                VoltPoint.Add(new MeasPoint<Voltage, Frequency>((decimal) 1.8 * VoltMultiplier,
                                                                new Frequency
                                                                    {Value = 1000, Multiplier = UnitMultiplier.None}));
                VoltPoint.Add(new MeasPoint<Voltage, Frequency>((decimal) 1.8 * VoltMultiplier,
                                                                new Frequency
                                                                    {Value = 10, Multiplier = UnitMultiplier.Kilo}));
                VoltPoint.Add(new MeasPoint<Voltage, Frequency>((decimal) 1.8 * VoltMultiplier,
                                                                new Frequency
                                                                    {Value = 20, Multiplier = UnitMultiplier.Kilo}));
                VoltPoint.Add(new MeasPoint<Voltage, Frequency>((decimal) 1.8 * VoltMultiplier,
                                                                new Frequency
                                                                    {Value = 50, Multiplier = UnitMultiplier.Kilo}));
                VoltPoint.Add(new MeasPoint<Voltage, Frequency>((decimal) 1.8 * VoltMultiplier,
                                                                new Frequency
                                                                {
                                                                    Value = 100,
                                                                    Multiplier = UnitMultiplier.Kilo
                                                                })); //.IsFake  =true;
            }

            #region Methods

            /// <inheritdoc />
            protected override string GetReportTableName()
            {
                return "FillTabBmOpe4_1_AcV_200V_Measure";
            }

            #endregion
        }

        public class Ope4_1_AcV_750V_Measure : Oper4AcvMeasureBase
        {
            public Ope4_1_AcV_750V_Measure(Mult107_109N.RangeNominal inRangeNominal,
                IUserItemOperation userItemOperation, string inResourceDir)
                : base(userItemOperation, inResourceDir)
            {
                OperMeasureMode = Mult107_109N.MeasureMode.ACV;
                OperationAcRangeCode = Mult107_109N.RangeCode.Range4Manual;
                OperationAcRangeNominal = inRangeNominal;
                Name = OperationAcRangeNominal.GetStringValue();

                VoltMultiplier = 1;

                RangeResolution = new MeasPoint<Voltage>(100, UnitMultiplier.Mili);

                VoltPoint.Add(new MeasPoint<Voltage, Frequency>(100 * VoltMultiplier,
                                                                new Frequency {Value = 40})); //fake
                VoltPoint.Add(new MeasPoint<Voltage, Frequency>(100 * VoltMultiplier, new Frequency {Value = 1000}));

                VoltPoint.Add(new MeasPoint<Voltage, Frequency>(400 * VoltMultiplier,
                                                                new Frequency {Value = 40})); //fake
                VoltPoint.Add(new MeasPoint<Voltage, Frequency>(400 * VoltMultiplier, new Frequency {Value = 1000}));

                VoltPoint.Add(new MeasPoint<Voltage, Frequency>(700 * VoltMultiplier,
                                                                new Frequency {Value = 40})); //fake
                VoltPoint.Add(new MeasPoint<Voltage, Frequency>(700 * VoltMultiplier, new Frequency {Value = 1000}));
            }

            #region Methods

            /// <inheritdoc />
            protected override string GetReportTableName()
            {
                return "FillTabBmOpe41AcV750VMeasure";
            }

            #endregion
        }

        #endregion ACV

        //////////////////////////////******DCI*******///////////////////////////////

        #region DCI

        public class Oper5DciMeasureBase : ParagraphBase<MeasPoint<Current>>
        {
            private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

            #region Fields

            protected decimal BaseMultiplier;

            /// <summary>
            /// Процент от измеряемой точки для расчета погрешности (уже переведе в абсолютные единицы).
            /// </summary>
            public decimal BaseTolCoeff;

            /// <summary>
            /// Число пределов измерения
            /// </summary>
            private int CountOfRanges;

            /// <summary>
            /// Массив поверяемых точек напряжения.
            /// </summary>
            protected MeasPoint<Current>[] CurrentDciPoint;

            /// <summary>
            /// довесок к формуле погрешности- число единиц младшего разряда
            /// </summary>
            public int EdMlRaz; //значение для пределов свыше 200 мВ

            /// <summary>
            /// Разарешение пределеа измерения (последний значащий разряд)
            /// </summary>
            protected MeasPoint<Current> RangeResolution;

            #endregion

            #region Property

            /// <summary>
            /// Код предела измерения на приборе
            /// </summary>
            public Mult107_109N.RangeCode OperationRangeCode { get; protected set; }

            /// <summary>
            /// Предел измерения поверяемого прибора, необходимый для работы
            /// </summary>
            public Mult107_109N.RangeNominal OperationRangeNominal { get; protected set; }

            /// <summary>
            /// Режим операции измерения прибора
            /// </summary>
            public Mult107_109N.MeasureMode OperMeasureMode { get; protected set; }

            //контрлируемый прибор
            protected Mult107_109N appa107N { get; set; }

            //эталон
            protected Calib5522A flkCalib5522A { get; set; }

            #endregion

            public Oper5DciMeasureBase(IUserItemOperation userItemOperation, string inResourceDir) :
                base(userItemOperation)
            {
                Name = "Определение погрешности измерения постоянного тока";
                OperMeasureMode = Mult107_109N.MeasureMode.DCmA;

                OperationRangeCode = Mult107_109N.RangeCode.Range1Manual;
                OperationRangeNominal = Mult107_109N.RangeNominal.RangeNone;
                DataRow = new List<IBasicOperation<MeasPoint<Current>>>();

                Sheme = new ShemeImage
                {
                    AssemblyLocalName = inResourceDir,
                    Description = "Измерительная схема",
                    Number = 2,
                    FileName = "appa_10XN_ma_5522A.jpg",
                    ExtendedDescription = "Соберите измерительную схему, согласно рисунку"
                };
            }

            #region Methods

            protected override DataTable FillData()
            {
                var dataTable = base.FillData();

                foreach (var row in DataRow)
                {
                    var dataRow = dataTable.NewRow();
                    var dds = row as BasicOperationVerefication<MeasPoint<Current>>;
                    // ReSharper disable once PossibleNullReferenceException
                    if (dds == null) continue;
                    dataRow[0] = OperationRangeNominal.GetStringValue();
                    dataRow[1] = dds.Expected?.Description;
                    dataRow[2] = dds.Getting?.Description;
                    dataRow[3] = dds.LowerTolerance?.Description;
                    dataRow[4] = dds.UpperTolerance?.Description;
                    if (dds.IsGood == null)
                        dataRow[5] = "не выполнено";
                    else
                        dataRow[5] = dds.IsGood() ? "Годен" : "Брак";
                    dataTable.Rows.Add(dataRow);
                }

                return dataTable;
            }

            /// <inheritdoc />
            protected override string[] GenerateDataColumnTypeObject()
            {
                return new[]
                {
                    "Предел измерения",
                    "Поверяемая точка", "Измеренное значение", "Минимальное допустимое значение",
                    "Максимальное допустимое значение"
                }.Concat(base.GenerateDataColumnTypeObject()).ToArray();
                ;
            }

            /// <inheritdoc />
            protected override string GetReportTableName()
            {
                return null;
            }

            protected override void InitWork(CancellationTokenSource token)
            {
                base.InitWork(token);
                if (appa107N == null || flkCalib5522A == null) return;
                foreach (var currPoint in CurrentDciPoint)
                {
                    var operation = new BasicOperationVerefication<MeasPoint<Current>>();
                    operation.InitWork = async () =>
                    {
                        try
                        {
                            if (appa107N.StringConnection.Equals("COM1"))
                                appa107N.StringConnection = GetStringConnect(appa107N);
                            flkCalib5522A.StringConnection ??= GetStringConnect(flkCalib5522A);

                            await Task.Run(() => { flkCalib5522A.Out.SetOutput(CalibrMain.COut.State.Off); });

                            var testMode = appa107N.GetMeasureMode;
                            while (OperMeasureMode !=
                                   await Task<Mult107_109N.MeasureMode>.Factory.StartNew(() => appa107N.GetMeasureMode))
                                UserItemOperation.ServicePack.MessageBox()
                                                 .Show($"Установите режим измерения: {OperMeasureMode.GetStringValue()} {OperMeasureMode}",
                                                       "Указание оператору", MessageButton.OK,
                                                       MessageIcon.Information,
                                                       MessageResult.OK);

                            while (await Task<Mult107_109N.RangeSwitchMode>.Factory.StartNew(() => appa107N
                                                                                                .GetRangeSwitchMode) ==
                                   Mult107_109N.RangeSwitchMode.Auto)
                                UserItemOperation.ServicePack.MessageBox()
                                                 .Show("Установите ручной режим переключения пределов.");

                            while (OperationRangeNominal !=
                                   await Task<Mult107_109N.RangeNominal>
                                        .Factory.StartNew(() => appa107N.GetRangeNominal))
                            {
                                int countPushRangeButton;

                                if (currPoint.MainPhysicalQuantity.Multiplier == UnitMultiplier.Mili)
                                {
                                    CountOfRanges = 2;
                                    UserItemOperation.ServicePack.MessageBox()
                                                     .Show($"Текущий предел измерения прибора {appa107N.GetRangeNominal.GetStringValue()}\n Необходимо установить предел {OperationRangeNominal.GetStringValue()} " +
                                                           $"Нажмите на приборе клавишу Range {countPushRangeButton = 1} раз.",
                                                           "Указание оператору", MessageButton.OK,
                                                           MessageIcon.Information,
                                                           MessageResult.OK);
                                }
                                else
                                {
                                    //работает только для ручного режима переключения пределов
                                    CountOfRanges = 2;
                                    var curRange = (int) appa107N.GetRangeCode - 127;
                                    var targetRange = (int) OperationRangeCode - 127;
                                    countPushRangeButton =
                                        Hepls.CountOfPushButton(CountOfRanges, curRange, targetRange);

                                    UserItemOperation.ServicePack.MessageBox()
                                                     .Show($"Текущий предел измерения прибора {appa107N.GetRangeNominal.GetStringValue()}\n Необходимо установить предел {OperationRangeNominal.GetStringValue()} " +
                                                           $"Нажмите на приборе клавишу Range {countPushRangeButton} раз.",
                                                           "Указание оператору", MessageButton.OK,
                                                           MessageIcon.Information,
                                                           MessageResult.OK);
                                }
                            }
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
                            flkCalib5522A.Out.Set.Current.Dc.SetValue(currPoint.MainPhysicalQuantity.Value *
                                                                      (decimal) currPoint
                                                                               .MainPhysicalQuantity.Multiplier
                                                                               .GetDoubleValue());
                            flkCalib5522A.Out.SetOutput(CalibrMain.COut.State.On);
                            Thread.Sleep(2000);
                            //измеряем
                            var measurePoint = (decimal) appa107N.GetValue();

                            flkCalib5522A.Out.SetOutput(CalibrMain.COut.State.Off);

                            var mantisa =
                                MathStatistics.GetMantissa(RangeResolution
                                                          .MainPhysicalQuantity.GetNoramalizeValueToSi() /
                                                           currPoint.MainPhysicalQuantity.GetNoramalizeValueToSi());
                            //округляем измерения
                            MathStatistics.Round(ref measurePoint, mantisa);

                            operation.Getting =
                                new MeasPoint<Current>(measurePoint, currPoint.MainPhysicalQuantity.Multiplier);
                            operation.Expected = currPoint;
                            //расчет погрешности для конкретной точки предела измерения
                            operation.ErrorCalculation = (inA, inB) =>
                            {
                                var result = BaseTolCoeff * Math.Abs(operation.Expected.MainPhysicalQuantity.Value) +
                                             EdMlRaz *
                                             RangeResolution.MainPhysicalQuantity.Value *
                                             (decimal) (RangeResolution
                                                       .MainPhysicalQuantity.Multiplier.GetDoubleValue() /
                                                        currPoint.MainPhysicalQuantity.Multiplier
                                                                 .GetDoubleValue());

                                MathStatistics.Round(ref result, mantisa);
                                return new MeasPoint<Current>(result, currPoint.MainPhysicalQuantity.Multiplier);
                            };

                            operation.LowerTolerance = operation.Expected - operation.Error;

                            operation.UpperTolerance = operation.Expected + operation.Error;

                            operation.IsGood = () =>
                            {
                                if (operation.Getting == null || operation.Expected == null ||
                                    operation.UpperTolerance == null || operation.LowerTolerance == null)
                                    return false;

                                return (operation.Getting < operation.UpperTolerance) &
                                       (operation.Getting > operation.LowerTolerance);
                                ;
                            };
                        }
                        catch (Exception e)
                        {
                            Logger.Error(e);
                            throw;
                        }
                    };
                    operation.CompliteWork = () => Hepls.HelpsCompliteWork(operation, UserItemOperation);
                    DataRow.Add(DataRow.IndexOf(operation) == -1
                                    ? operation
                                    : (BasicOperationVerefication<MeasPoint<Current>>) operation.Clone());
                }
            }

            #endregion

            ///// <summary>
            ///// Имя закладки таблички в результирующем протоколе doc (Ms Word).
            ///// </summary>
            //protected string ReportTableName;

           
        }

        public class Oper5_1Dci_20mA_Measure : Oper5DciMeasureBase
        {
            public Oper5_1Dci_20mA_Measure(Mult107_109N.RangeNominal inRangeNominal,
                IUserItemOperation userItemOperation, string inResourceDir) :
                base(userItemOperation, inResourceDir)
            {
                OperMeasureMode = Mult107_109N.MeasureMode.DCmA;
                OperationRangeCode = Mult107_109N.RangeCode.Range1Manual;
                OperationRangeNominal = inRangeNominal;

                Name = OperationRangeNominal.GetStringValue();
                Sheme = new ShemeImage
                {
                    AssemblyLocalName = Assembly.GetExecutingAssembly().GetName().Name,
                    Description = "Измерительная схема",
                    Number = 2,
                    FileName = @"appa_10XN_ma_5522A.jpg",
                    ExtendedDescription = "Соберите измерительную схему, согласно рисунку"
                };
                BaseTolCoeff = (decimal) 0.002;
                EdMlRaz = 40;
                RangeResolution = new MeasPoint<Current>(1, UnitMultiplier.Micro);

                BaseMultiplier = 1;
                CurrentDciPoint = new MeasPoint<Current>[5];
                CurrentDciPoint[0] = new MeasPoint<Current>(4 * BaseMultiplier, UnitMultiplier.Mili);
                CurrentDciPoint[1] = new MeasPoint<Current>(8 * BaseMultiplier, UnitMultiplier.Mili);
                CurrentDciPoint[2] = new MeasPoint<Current>(12 * BaseMultiplier, UnitMultiplier.Mili);
                CurrentDciPoint[3] = new MeasPoint<Current>(18 * BaseMultiplier, UnitMultiplier.Mili);
                CurrentDciPoint[4] = new MeasPoint<Current>(-18 * BaseMultiplier, UnitMultiplier.Mili);
            }

            #region Methods

            /// <inheritdoc />
            protected override string GetReportTableName()
            {
                return "FillTabBmOper5_1Dci_20mA_Measure";
            }

            #endregion
        }

        public class Oper5_1Dci_200mA_Measure : Oper5DciMeasureBase
        {
            public Oper5_1Dci_200mA_Measure(Mult107_109N.RangeNominal inRangeNominal,
                IUserItemOperation userItemOperation, string inResourceDir)
                : base(userItemOperation, inResourceDir)
            {
                OperMeasureMode = Mult107_109N.MeasureMode.DCmA;
                OperationRangeCode = Mult107_109N.RangeCode.Range2Manual;
                OperationRangeNominal = inRangeNominal;

                Name = OperationRangeNominal.GetStringValue();
                Sheme = new ShemeImage
                {
                    AssemblyLocalName = Assembly.GetExecutingAssembly().GetName().Name,
                    Description = "Измерительная схема",
                    Number = 2,
                    FileName = @"appa_10XN_ma_5522A.jpg",
                    ExtendedDescription = "Соберите измерительную схему, согласно рисунку"
                };
                BaseTolCoeff = (decimal) 0.002;
                EdMlRaz = 40;
                RangeResolution = new MeasPoint<Current>(10, UnitMultiplier.Micro);

                BaseMultiplier = 10;
                CurrentDciPoint = new MeasPoint<Current>[5];
                CurrentDciPoint[0] = new MeasPoint<Current>(4 * BaseMultiplier, UnitMultiplier.Mili);
                CurrentDciPoint[1] = new MeasPoint<Current>(8 * BaseMultiplier, UnitMultiplier.Mili);
                CurrentDciPoint[2] = new MeasPoint<Current>(12 * BaseMultiplier, UnitMultiplier.Mili);
                CurrentDciPoint[3] = new MeasPoint<Current>(18 * BaseMultiplier, UnitMultiplier.Mili);
                CurrentDciPoint[4] = new MeasPoint<Current>(-18 * BaseMultiplier, UnitMultiplier.Mili);
            }

            #region Methods

            /// <inheritdoc />
            protected override string GetReportTableName()
            {
                return "FillTabBmOper5_1Dci_200mA_Measure";
            }

            #endregion
        }

        public class Oper5_1Dci_2A_Measure : Oper5DciMeasureBase
        {
            public Oper5_1Dci_2A_Measure(Mult107_109N.RangeNominal inRangeNominal, IUserItemOperation userItemOperation,
                string inResourceDir) :
                base(userItemOperation, inResourceDir)
            {
                OperMeasureMode = Mult107_109N.MeasureMode.DCI;
                OperationRangeCode = Mult107_109N.RangeCode.Range1Manual;
                OperationRangeNominal = inRangeNominal;

                Name = OperationRangeNominal.GetStringValue();
                Sheme = new ShemeImage
                {
                    AssemblyLocalName = Assembly.GetExecutingAssembly().GetName().Name,
                    Description = "Измерительная схема",
                    Number = 1,
                    FileName = @"appa_10XN_A_Aux_5522A.jpg",
                    ExtendedDescription = "Соберите измерительную схему, согласно рисунку"
                };
                BaseTolCoeff = (decimal) 0.002;
                EdMlRaz = 40;
                RangeResolution = new MeasPoint<Current>(100, UnitMultiplier.Micro);

                BaseMultiplier = (decimal) 0.1;
                CurrentDciPoint = new[]
                {
                     new MeasPoint<Current>(4 * BaseMultiplier),
                     new MeasPoint<Current>(8 * BaseMultiplier),
                     new MeasPoint<Current>(12 * BaseMultiplier),
                     new MeasPoint<Current>(18 * BaseMultiplier),
                     new MeasPoint<Current>(-18 * BaseMultiplier)
                };

                Sheme = new ShemeImage
                {
                    AssemblyLocalName = Assembly.GetExecutingAssembly().GetName().Name,
                    Description = "Измерительная схема",
                    Number = 3,
                    FileName = "appa_10XN_A_Aux_5522A.jpg",
                    ExtendedDescription = "Соберите измерительную схему, согласно рисунку"
                };
            }

            #region Methods

            /// <inheritdoc />
            protected override string GetReportTableName()
            {
                return "FillTabBmOper5_1Dci_2A_Measure";
            }

            #endregion
        }

        public class Oper5_2_1Dci_10A_Measure : Oper5DciMeasureBase
        {
            public Oper5_2_1Dci_10A_Measure(Mult107_109N.RangeNominal inRangeNominal,
                IUserItemOperation userItemOperation, string inResourceDir)
                : base(userItemOperation, inResourceDir)
            {
                OperMeasureMode = Mult107_109N.MeasureMode.DCI;
                OperationRangeCode = Mult107_109N.RangeCode.Range2Manual;
                OperationRangeNominal = inRangeNominal;

                BaseTolCoeff = (decimal) 0.002;
                EdMlRaz = 40;
                RangeResolution = new MeasPoint<Current>(1, UnitMultiplier.Mili);

                CurrentDciPoint = new MeasPoint<Current>[1];
                CurrentDciPoint[0] = new MeasPoint<Current>(1);

                Name = OperationRangeNominal.GetStringValue() + $" точка {CurrentDciPoint[0].Description}";

                Sheme = new ShemeImage
                {
                    AssemblyLocalName = Assembly.GetExecutingAssembly().GetName().Name,
                    Description = "Измерительная схема",
                    Number = 3,
                    FileName = "appa_10XN_A_Aux_5522A.jpg",
                    ExtendedDescription = "Соберите измерительную схему, согласно рисунку"
                };
            }

            #region Methods

            /// <inheritdoc />
            protected override string GetReportTableName()
            {
                return "FillTabBmOper5_2_1Dci_10A_Measure";
            }

            #endregion
        }

        public class Oper5_2_2Dci_10A_Measure : Oper5DciMeasureBase
        {
            public Oper5_2_2Dci_10A_Measure(Mult107_109N.RangeNominal inRangeNominal,
                IUserItemOperation userItemOperation, string inResourceDir)
                : base(userItemOperation, inResourceDir)
            {
                OperMeasureMode = Mult107_109N.MeasureMode.DCI;
                OperationRangeCode = Mult107_109N.RangeCode.Range2Manual;
                OperationRangeNominal = inRangeNominal;
                Name = OperationRangeNominal.GetStringValue();

                BaseTolCoeff = (decimal) 0.002;
                EdMlRaz = 40;
                RangeResolution = new MeasPoint<Current>(1, UnitMultiplier.Mili);

                CurrentDciPoint = new MeasPoint<Current>[3];
                CurrentDciPoint[0] = new MeasPoint<Current>(5);
                CurrentDciPoint[1] = new MeasPoint<Current>(9);
                CurrentDciPoint[2] = new MeasPoint<Current>(-9);

                Sheme = new ShemeImage
                {
                    AssemblyLocalName = Assembly.GetExecutingAssembly().GetName().Name,
                    Description = "Измерительная схема",
                    Number = 4,
                    FileName = "appa_10XN_20A_5522A.jpg",
                    ExtendedDescription = "Соберите измерительную схему, согласно рисунку"
                };
            }

            #region Methods

            /// <inheritdoc />
            protected override string GetReportTableName()
            {
                return "FillTabBmOper5_2_2Dci_10A_Measure";
            }

            #endregion
        }

        #endregion DCI

        //////////////////////////////******ACI*******///////////////////////////////

        #region ACI

        public class Oper6AciMeasureBase : ParagraphBase<MeasPoint<Current, Frequency>>
        {
            private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

            #region Fields

            /// <summary>
            /// Итоговый массив поверяемых точек. У каждого номинала напряжения вложены номиналы частот для текущей точки.
            /// </summary>
            public MeasPoint<Current, Frequency>[] AciPoint;

            /// <summary>
            /// Число пределов измерения
            /// </summary>
            private int CountOfRanges;

            /// <summary>
            /// Множитель для поверяемых точек. (Если точки можно посчитать простым умножением).
            /// </summary>
            protected decimal CurrentMultiplier;

            #endregion

            #region Property

            /// <summary>
            /// Код предела измерения на приборе
            /// </summary>
            public Mult107_109N.RangeCode OperationRangeCode { get; protected set; }

            /// <summary>
            /// Предел измерения поверяемого прибора, необходимый для работы
            /// </summary>
            public Mult107_109N.RangeNominal OperationRangeNominal { get; protected set; }

            /// <summary>
            /// Режим операции измерения прибора
            /// </summary>
            public Mult107_109N.MeasureMode OperMeasureMode { get; protected set; }

            //контрлируемый прибор
            protected Mult107_109N appa107N { get; set; }

            //эталон
            protected Calib5522A flkCalib5522A { get; set; }

            #endregion

            public Oper6AciMeasureBase(IUserItemOperation userItemOperation, string inResourceDir) :
                base(userItemOperation)
            {
                Name = "Определение погрешности измерения переменного тока";
                OperMeasureMode = Mult107_109N.MeasureMode.ACV;

                OperationRangeCode = Mult107_109N.RangeCode.Range1Manual;
                OperationRangeNominal = Mult107_109N.RangeNominal.RangeNone;
                DataRow = new List<IBasicOperation<MeasPoint<Current, Frequency>>>();

                Sheme = new ShemeImage
                {
                    AssemblyLocalName = inResourceDir,
                    Description = "Измерительная схема",
                    Number = 2,
                    FileName = "appa_10XN_ma_5522A.jpg",
                    ExtendedDescription = "Соберите измерительную схему, согласно рисунку"
                };
            }

            #region Methods

            /// <summary>
            /// Метод выбирает необходимый значения коэффициентов для формулы погрешности, исходя из предела измерения и диапазона
            /// частот.
            /// </summary>
            /// <returns>Результат вычисления.</returns>
            protected void ConstructTooleranceFormula(MeasPoint<Current, Frequency> inFreq)
            {
                if (OperationRangeNominal == Mult107_109N.RangeNominal.Range20mA &&
                    inFreq.AdditionalPhysicalQuantity.Multiplier == UnitMultiplier.None)
                {
                    if (inFreq.AdditionalPhysicalQuantity.Value >= 40 && inFreq.AdditionalPhysicalQuantity.Value < 500)
                    {
                        BaseTolCoeff = (decimal) 0.008;
                        EdMlRaz = 50;
                    }

                    if (inFreq.AdditionalPhysicalQuantity.Value >= 500 &&
                        inFreq.AdditionalPhysicalQuantity.Value < 1000)
                    {
                        BaseTolCoeff = (decimal) 0.012;
                        EdMlRaz = 80;
                    }
                }
                else if (OperationRangeNominal == Mult107_109N.RangeNominal.Range200mA ||
                         OperationRangeNominal == Mult107_109N.RangeNominal.Range2A ||
                         OperationRangeNominal == Mult107_109N.RangeNominal.Range10A)
                {
                    if (inFreq.AdditionalPhysicalQuantity.Value >= 40 && inFreq.AdditionalPhysicalQuantity.Value < 500)
                    {
                        BaseTolCoeff = (decimal) 0.008;
                        EdMlRaz = 50;
                    }
                    else if (inFreq.AdditionalPhysicalQuantity.Value >= 500 &&
                             inFreq.AdditionalPhysicalQuantity.Value < 1000)
                    {
                        BaseTolCoeff = (decimal) 0.012;
                        EdMlRaz = 80;
                    }
                    else if (inFreq.AdditionalPhysicalQuantity.Value >= 1000 &&
                             inFreq.AdditionalPhysicalQuantity.Value <= 3000)
                    {
                        BaseTolCoeff = (decimal) 0.02;
                        EdMlRaz = 80;
                    }
                }
            }

            protected override DataTable FillData()
            {
                var dataTable = base.FillData();

                foreach (var row in DataRow)
                {
                    var dataRow = dataTable.NewRow();
                    var dds = row as BasicOperationVerefication<MeasPoint<Voltage, Frequency>>;
                    // ReSharper disable once PossibleNullReferenceException
                    if (dds == null) continue;
                    dataRow[0] = OperationRangeNominal.GetStringValue();
                    dataRow[1] = dds.Expected.ToString();
                    //тут может упасть!!!
                    dataRow[2] = dds.Expected?.ToString();
                    dataRow[3] = dds.Getting?.ToString();
                    dataRow[4] = dds.LowerTolerance?.ToString();
                    dataRow[5] = dds.UpperTolerance?.ToString();
                    if (dds.IsGood == null)
                        dataRow[6] = "не выполнено";
                    else
                        dataRow[6] = dds.IsGood() ? "Годен" : "Брак";
                    dataTable.Rows.Add(dataRow);
                }

                return dataTable;
            }

            /// <inheritdoc />
            protected override string[] GenerateDataColumnTypeObject()
            {
                return new[]
                {
                    "Предел измерения",
                    "Поверяемая точка", "Измеренное значение", "Минимальное допустимое значение",
                    "Максимальное допустимое значение"
                }.Concat(base.GenerateDataColumnTypeObject()).ToArray();
                ;
            }

            /// <inheritdoc />
            protected override string GetReportTableName()
            {
                return null;
            }

            protected override void InitWork(CancellationTokenSource token)
            {
                base.InitWork(token);
                if (flkCalib5522A == null || appa107N == null) return;

                foreach (var curr in AciPoint)

                {
                    var operation = new BasicOperationVerefication<MeasPoint<Current, Frequency>>();
                    operation.InitWork = async () =>
                    {
                        try
                        {
                            if (appa107N.StringConnection.Equals("COM1"))
                                appa107N.StringConnection = GetStringConnect(appa107N);
                            flkCalib5522A.StringConnection ??= GetStringConnect(flkCalib5522A);

                            await Task.Run(() => { flkCalib5522A.Out.SetOutput(CalibrMain.COut.State.Off); });

                            var testMode = appa107N.GetMeasureMode;
                            while (OperMeasureMode !=
                                   await Task<Mult107_109N.MeasureMode>.Factory.StartNew(() => appa107N.GetMeasureMode))
                                UserItemOperation.ServicePack.MessageBox()
                                                 .Show($"Установите режим измерения: {OperMeasureMode.GetStringValue()} {OperMeasureMode}",
                                                       "Указание оператору", MessageButton.OK,
                                                       MessageIcon.Information,
                                                       MessageResult.OK);

                            while (await Task<Mult107_109N.RangeSwitchMode>.Factory.StartNew(() => appa107N
                                                                                                .GetRangeSwitchMode) ==
                                   Mult107_109N.RangeSwitchMode.Auto)
                                UserItemOperation.ServicePack.MessageBox()
                                                 .Show("Установите ручной режим переключения пределов.");

                            while (OperationRangeNominal !=
                                   await Task<Mult107_109N.RangeNominal>
                                        .Factory.StartNew(() => appa107N.GetRangeNominal))
                            {
                                int countPushRangeButton;

                                if (curr.MainPhysicalQuantity.Multiplier == UnitMultiplier.Mili)
                                {
                                    CountOfRanges = 2;
                                    UserItemOperation.ServicePack.MessageBox()
                                                     .Show($"Текущий предел измерения прибора {appa107N.GetRangeNominal.GetStringValue()}\n Необходимо установить предел {OperationRangeNominal.GetStringValue()} " +
                                                           $"Нажмите на приборе клавишу Range {countPushRangeButton = 1} раз.",
                                                           "Указание оператору", MessageButton.OK,
                                                           MessageIcon.Information,
                                                           MessageResult.OK);
                                }
                                else
                                {
                                    //работает только для ручного режима переключения пределов
                                    CountOfRanges = 2;
                                    var curRange = (int) appa107N.GetRangeCode - 127;
                                    var targetRange = (int) OperationRangeCode - 127;
                                    countPushRangeButton =
                                        Hepls.CountOfPushButton(CountOfRanges, curRange, targetRange);

                                    UserItemOperation.ServicePack.MessageBox()
                                                     .Show($"Текущий предел измерения прибора {appa107N.GetRangeNominal.GetStringValue()}\n Необходимо установить предел {OperationRangeNominal.GetStringValue()} " +
                                                           $"Нажмите на приборе клавишу Range {countPushRangeButton} раз.",
                                                           "Указание оператору", MessageButton.OK,
                                                           MessageIcon.Information,
                                                           MessageResult.OK);
                                }
                            }
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
                            var mantisa =
                                MathStatistics
                                   .GetMantissa(RangeResolution.MainPhysicalQuantity.GetNoramalizeValueToSi() / curr.MainPhysicalQuantity.GetNoramalizeValueToSi(),
                                                true);

                            operation.Expected = (MeasPoint<Current, Frequency>) curr.Clone();

                            //расчет погрешности для конкретной точки предела измерения
                            ConstructTooleranceFormula(curr); // функция подбирает коэффициенты для формулы погрешности
                            operation.ErrorCalculation = (expected, getting) =>
                            {
                                var result = BaseTolCoeff * Math.Abs(operation.Expected.MainPhysicalQuantity.Value) +
                                             EdMlRaz *
                                             RangeResolution.MainPhysicalQuantity.Value *
                                             (decimal) (RangeResolution
                                                       .MainPhysicalQuantity.Multiplier.GetDoubleValue() /
                                                        curr.MainPhysicalQuantity.Multiplier
                                                            .GetDoubleValue());

                                MathStatistics.Round(ref result, mantisa);
                                return new MeasPoint<Current, Frequency>(result, curr.MainPhysicalQuantity.Multiplier,
                                                                         curr.AdditionalPhysicalQuantity);
                            };

                            operation.LowerTolerance = operation.Expected - operation.Error;
                            ;
                            //operation.Expected - operation.Error;
                            operation.UpperTolerance = operation.Expected + operation.Error;
                            //operation.Expected + operation.Error;
                            operation.IsGood = () =>
                            {
                                if (operation.Expected.MainPhysicalQuantity == null ||
                                    operation.Getting.MainPhysicalQuantity == null ||
                                    operation.LowerTolerance.MainPhysicalQuantity == null ||
                                    operation.UpperTolerance.MainPhysicalQuantity == null) return false;

                                return (operation.Getting < operation.UpperTolerance) &
                                       (operation.Getting > operation.LowerTolerance);
                            };

                            decimal measurePoint = 0;
                            //todo здесь нужно проверять точки на возможность воспроизведения калибратором
                            flkCalib5522A.Out.Set.Current.Ac.SetValue(curr);
                            flkCalib5522A.Out.SetOutput(CalibrMain.COut.State.On);
                            Thread.Sleep(2000);
                            //измеряем
                            measurePoint = (decimal) appa107N.GetValue();
                            flkCalib5522A.Out.SetOutput(CalibrMain.COut.State.Off);

                            //округляем измерения
                            MathStatistics.Round(ref measurePoint, mantisa);

                            operation.Getting =
                                new MeasPoint<Current, Frequency>(measurePoint, curr.AdditionalPhysicalQuantity);
                        }
                        catch (Exception e)
                        {
                            Logger.Error(e);
                            throw;
                        }
                    };

                    operation.CompliteWork = () =>
                        Hepls.HelpsCompliteWork(operation, UserItemOperation);
                    DataRow.Add(DataRow.IndexOf(operation) == -1
                                    ? operation
                                    : (BasicOperationVerefication<MeasPoint<Current, Frequency>>) operation.Clone());
                }
            }

            #endregion

            /// <summary>
            /// Набор частот, характерный для данного предела измерения
            /// </summary>
            //protected MeasPoint[] HerzPoint;

            /// <summary>
            /// Это пустая точка, которая содержит только единицы измерения текущего
            /// поверяемого предела и множитель для единицы измерения.
            /// </summary>
            //protected MeasPoint thisRangeUnits;

            #region TolleranceFormula

            /// <summary>
            /// Процент от измеряемой точки для расчета погрешности (уже переведе в абсолютные единицы).
            /// </summary>
            public decimal BaseTolCoeff;

            /// <summary>
            /// довесок к формуле погрешности- число единиц младшего разряда
            /// </summary>
            public int EdMlRaz;

            /// <summary>
            /// Разарешение пределеа измерения (последний значащий разряд)
            /// </summary>
            protected MeasPoint<Current> RangeResolution;

            #endregion TolleranceFormula
        }

        public class Oper6_1Aci_20mA_Measure : Oper6AciMeasureBase
        {
            public Oper6_1Aci_20mA_Measure(Mult107_109N.RangeNominal inRangeNominal,
                IUserItemOperation userItemOperation, string inResourceDir) : base(userItemOperation, inResourceDir)
            {
                OperMeasureMode = Mult107_109N.MeasureMode.ACmA;
                OperationRangeCode = Mult107_109N.RangeCode.Range1Manual;
                OperationRangeNominal = inRangeNominal;
                RangeResolution = new MeasPoint<Current>(1, UnitMultiplier.Micro);
                Name = OperationRangeNominal.GetStringValue();

                CurrentMultiplier = 1;

                AciPoint = new[]
                {
                    new MeasPoint<Current, Frequency>(4 * CurrentMultiplier, UnitMultiplier.Mili,
                                                      new Frequency {Value = 40}),
                    new MeasPoint<Current, Frequency>(4 * CurrentMultiplier, UnitMultiplier.Mili,
                                                      new Frequency {Value = 1000}),
                    new MeasPoint<Current, Frequency>(10 * CurrentMultiplier, UnitMultiplier.Mili,
                                                      new Frequency {Value = 40}),
                    new MeasPoint<Current, Frequency>(10 * CurrentMultiplier, UnitMultiplier.Mili,
                                                      new Frequency {Value = 1000}),
                    new MeasPoint<Current, Frequency>(18 * CurrentMultiplier, UnitMultiplier.Mili,
                                                      new Frequency {Value = 40}),
                    new MeasPoint<Current, Frequency>(18 * CurrentMultiplier, UnitMultiplier.Mili,
                                                      new Frequency {Value = 1000})
                };
            }

            #region Methods

            /// <inheritdoc />
            protected override string GetReportTableName()
            {
                return "FillTabBmOper6_1Aci_20mA_Measure";
            }

            #endregion
        }

        public class Oper6_1Aci_200mA_Measure : Oper6AciMeasureBase
        {
            public Oper6_1Aci_200mA_Measure(Mult107_109N.RangeNominal inRangeNominal,
                IUserItemOperation userItemOperation, string inResourceDir) : base(userItemOperation, inResourceDir)
            {
                OperMeasureMode = Mult107_109N.MeasureMode.ACmA;
                OperationRangeCode = Mult107_109N.RangeCode.Range2Manual;
                OperationRangeNominal = inRangeNominal;
                RangeResolution = new MeasPoint<Current>(10, UnitMultiplier.Micro);
                Name = OperationRangeNominal.GetStringValue();

                CurrentMultiplier = 10;

                AciPoint = new[]
                {
                    new MeasPoint<Current, Frequency>(4 * CurrentMultiplier, UnitMultiplier.Mili,
                                                      new Frequency {Value = 40}),
                    new MeasPoint<Current, Frequency>(4 * CurrentMultiplier, UnitMultiplier.Mili,
                                                      new Frequency {Value = 1000}),
                    new MeasPoint<Current, Frequency>(4 * CurrentMultiplier, UnitMultiplier.Mili,
                                                      new Frequency {Value = 3, Multiplier = UnitMultiplier.Kilo}),
                    new MeasPoint<Current, Frequency>(10 * CurrentMultiplier, UnitMultiplier.Mili,
                                                      new Frequency {Value = 40}),
                    new MeasPoint<Current, Frequency>(10 * CurrentMultiplier, UnitMultiplier.Mili,
                                                      new Frequency {Value = 1000}),
                    new MeasPoint<Current, Frequency>(10 * CurrentMultiplier, UnitMultiplier.Mili,
                                                      new Frequency {Value = 3, Multiplier = UnitMultiplier.Kilo}),
                    new MeasPoint<Current, Frequency>(18 * CurrentMultiplier, UnitMultiplier.Mili,
                                                      new Frequency {Value = 40}),
                    new MeasPoint<Current, Frequency>(18 * CurrentMultiplier, UnitMultiplier.Mili,
                                                      new Frequency {Value = 1000}),
                    new MeasPoint<Current, Frequency>(18 * CurrentMultiplier, UnitMultiplier.Mili,
                                                      new Frequency {Value = 3, Multiplier = UnitMultiplier.Kilo})
                };
            }

            #region Methods

            /// <inheritdoc />
            protected override string GetReportTableName()
            {
                return "FillTabBmOper6_1Aci_200mA_Measure";
            }

            #endregion
        }

        public class Oper6_1Aci_2A_Measure : Oper6AciMeasureBase
        {
            public Oper6_1Aci_2A_Measure(Mult107_109N.RangeNominal inRangeNominal,
                IUserItemOperation userItemOperation, string inResourceDir) : base(userItemOperation, inResourceDir)
            {
                OperMeasureMode = Mult107_109N.MeasureMode.ACI;
                OperationRangeCode = Mult107_109N.RangeCode.Range1Manual;
                OperationRangeNominal = inRangeNominal;
                RangeResolution = new MeasPoint<Current>(100, UnitMultiplier.Micro);
                Name = OperationRangeNominal.GetStringValue();

                CurrentMultiplier = (decimal) 0.1;

                AciPoint = new[]
                {
                    new MeasPoint<Current, Frequency>(4 * CurrentMultiplier, new Frequency {Value = 40}),
                    new MeasPoint<Current, Frequency>(4 * CurrentMultiplier, new Frequency {Value = 1000}),
                    new MeasPoint<Current, Frequency>(4 * CurrentMultiplier,
                                                      new Frequency {Value = 3, Multiplier = UnitMultiplier.Kilo}),

                    new MeasPoint<Current, Frequency>(10 * CurrentMultiplier, new Frequency {Value = 40}),
                    new MeasPoint<Current, Frequency>(10 * CurrentMultiplier, new Frequency {Value = 1000}),
                    new MeasPoint<Current, Frequency>(10 * CurrentMultiplier,
                                                      new Frequency {Value = 3, Multiplier = UnitMultiplier.Kilo}),

                    new MeasPoint<Current, Frequency>(18 * CurrentMultiplier, new Frequency {Value = 40}),
                    new MeasPoint<Current, Frequency>(18 * CurrentMultiplier, new Frequency {Value = 1000}),
                    new MeasPoint<Current, Frequency>(18 * CurrentMultiplier,
                                                      new Frequency {Value = 3, Multiplier = UnitMultiplier.Kilo})
                };

                Sheme = new ShemeImage
                {
                    AssemblyLocalName = Assembly.GetExecutingAssembly().GetName().Name,
                    Description = "Измерительная схема",
                    Number = 3,
                    FileName = "appa_10XN_A_Aux_5522A.jpg",
                    ExtendedDescription = "Соберите измерительную схему, согласно рисунку"
                };
            }

            #region Methods

            /// <inheritdoc />
            protected override string GetReportTableName()
            {
                return "FillTabBmOper6_1Aci_2A_Measure";
            }

            #endregion
        }

        public class Oper6_2_1Aci_10A_Measure : Oper6AciMeasureBase
        {
            public Oper6_2_1Aci_10A_Measure(Mult107_109N.RangeNominal inRangeNominal,
                IUserItemOperation userItemOperation, string inResourceDir) : base(userItemOperation, inResourceDir)
            {
                OperMeasureMode = Mult107_109N.MeasureMode.ACI;
                OperationRangeCode = Mult107_109N.RangeCode.Range2Manual;
                OperationRangeNominal = inRangeNominal;
                RangeResolution = new MeasPoint<Current>(1, UnitMultiplier.Mili);

                AciPoint = new[]
                {
                    new MeasPoint<Current, Frequency>(2, new Frequency {Value = 40}),
                    new MeasPoint<Current, Frequency>(2, new Frequency {Value = 1000}),
                    new MeasPoint<Current, Frequency>(2, new Frequency {Value = 3, Multiplier = UnitMultiplier.Kilo})
                };

                Name = OperationRangeNominal.GetStringValue() +
                       $" точка {AciPoint[0].Description}";

                Sheme = new ShemeImage
                {
                    AssemblyLocalName = Assembly.GetExecutingAssembly().GetName().Name,
                    Description = "Измерительная схема",
                    Number = 3,
                    FileName = "appa_10XN_A_Aux_5522A.jpg",
                    ExtendedDescription = "Соберите измерительную схему, согласно рисунку"
                };
            }

            #region Methods

            /// <inheritdoc />
            protected override string GetReportTableName()
            {
                return "FillTabBmOper6_2_1Aci_10A_Measure";
            }

            #endregion
        }

        public class Oper6_2_2Aci_10A_Measure : Oper6AciMeasureBase
        {
            public Oper6_2_2Aci_10A_Measure(Mult107_109N.RangeNominal inRangeNominal,
                IUserItemOperation userItemOperation, string inResourceDir) : base(userItemOperation, inResourceDir)
            {
                OperMeasureMode = Mult107_109N.MeasureMode.ACI;
                OperationRangeCode = Mult107_109N.RangeCode.Range2Manual;
                OperationRangeNominal = inRangeNominal;
                RangeResolution = new MeasPoint<Current>(1, UnitMultiplier.Mili);
                Name = OperationRangeNominal.GetStringValue();

                AciPoint = new[]
                {
                    new MeasPoint<Current, Frequency>(5, new Frequency {Value = 40}),
                    new MeasPoint<Current, Frequency>(5, new Frequency {Value = 1, Multiplier = UnitMultiplier.Kilo}),
                    new MeasPoint<Current, Frequency>(5, new Frequency {Value = 3, Multiplier = UnitMultiplier.Kilo}),

                    new MeasPoint<Current, Frequency>(9, new Frequency {Value = 40}),
                    new MeasPoint<Current, Frequency>(9, new Frequency {Value = 1, Multiplier = UnitMultiplier.Kilo}),
                    new MeasPoint<Current, Frequency>(9, new Frequency {Value = 3, Multiplier = UnitMultiplier.Kilo})
                };

                Sheme = new ShemeImage
                {
                    AssemblyLocalName = Assembly.GetExecutingAssembly().GetName().Name,
                    Description = "Измерительная схема",
                    Number = 4,
                    FileName = "appa_10XN_20A_5522A.jpg",
                    ExtendedDescription = "Соберите измерительную схему, согласно рисунку"
                };
            }

            #region Methods

            /// <inheritdoc />
            protected override string GetReportTableName()
            {
                return "FillTabBmOper6_2_2Aci_10A_Measure";
            }

            #endregion
        }

        #endregion ACI

        //////////////////////////////******FREQ*******///////////////////////////////

        #region FREQ

        public class Oper7FreqMeasureBase : ParagraphBase<MeasPoint<Frequency>>
        {
            private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

            #region Fields

            //множители для пределов.
            public decimal BaseMultiplier;

            public decimal BaseTolCoeff;

            

            /// <summary>
            /// довесок к формуле погрешности- единица младшего разряда
            /// </summary>
            public int EdMlRaz;

            protected MeasPoint<Frequency>[] HerzPoint;

            protected MeasPoint<Frequency> RangeResolution;

            #endregion

            #region Property

            /// <summary>
            /// Код предела измерения на приборе
            /// </summary>
            public Mult107_109N.RangeCode OperationRangeCode { get; protected set; }

            /// <summary>
            /// Предел измерения поверяемого прибора, необходимый для работы
            /// </summary>
            public Mult107_109N.RangeNominal OperationRangeNominal { get; protected set; }

            /// <summary>
            /// Режим операции измерения прибора
            /// </summary>
            public Mult107_109N.MeasureMode OperMeasureMode { get; protected set; }

            //контрлируемый прибор
            protected Mult107_109N appa107N { get; set; }

            //эталон
            protected Calib5522A flkCalib5522A { get; set; }

            #endregion

            public Oper7FreqMeasureBase(IUserItemOperation userItemOperation, string inResourceDir) :
                base(userItemOperation)
            {
                Name = "Определение погрешности измерения частоты переменного напряжения";
                OperMeasureMode = Mult107_109N.MeasureMode.Herz;

                OperationRangeCode = Mult107_109N.RangeCode.Range1Manual;
                OperationRangeNominal = Mult107_109N.RangeNominal.RangeNone;

                DataRow = new List<IBasicOperation<MeasPoint<Frequency>>>();
                Sheme = ShemeTemplateDefault.TemplateSheme;
                Sheme.AssemblyLocalName = inResourceDir;
            }

            #region Methods

            protected override DataTable FillData()
            {
                var dataTable = base.FillData();

                foreach (var row in DataRow)
                {
                    var dataRow = dataTable.NewRow();
                    var dds = row as BasicOperationVerefication<MeasPoint<Frequency>>;
                    // ReSharper disable once PossibleNullReferenceException
                    if (dds == null) continue;
                    dataRow[0] = OperationRangeNominal.GetStringValue();
                    dataRow[1] = dds.Expected?.Description;
                    dataRow[2] = dds.Getting?.Description;
                    dataRow[3] = dds.LowerTolerance?.Description;
                    dataRow[4] = dds.UpperTolerance?.Description;
                    if (dds.IsGood == null)
                        dataRow[5] = "не выполнено";
                    else
                        dataRow[5] = dds.IsGood() ? "Годен" : "Брак";
                    dataTable.Rows.Add(dataRow);
                }

                return dataTable;
            }

            /// <inheritdoc />
            protected override string[] GenerateDataColumnTypeObject()
            {
                return new[]
                {
                    "Предел измерения",
                    "Поверяемая точка", "Измеренное значение", "Минимальное допустимое значение",
                    "Максимальное допустимое значение"
                }.Concat(base.GenerateDataColumnTypeObject()).ToArray();
                ;
            }

            /// <inheritdoc />
            protected override string GetReportTableName()
            {
                return null;
            }

            protected override void InitWork(CancellationTokenSource token)
            {
                base.InitWork(token);
                if (appa107N == null || flkCalib5522A == null) return;

                foreach (var freqPoint in HerzPoint)
                {
                    var operation = new BasicOperationVerefication<MeasPoint<Frequency>>();
                    operation.InitWork = async () =>
                    {
                        try
                        {
                            if (appa107N.StringConnection.Equals("COM1"))
                                appa107N.StringConnection = GetStringConnect(appa107N);
                            flkCalib5522A.StringConnection ??= GetStringConnect(flkCalib5522A);

                            await Task.Run(() => { flkCalib5522A.Out.SetOutput(CalibrMain.COut.State.Off); });

                            while (OperMeasureMode !=
                                   await Task<Mult107_109N.MeasureMode>.Factory.StartNew(() => appa107N.GetMeasureMode))
                                UserItemOperation.ServicePack.MessageBox()
                                                 .Show($"Установите режим измерения: {OperMeasureMode.GetStringValue()} {OperMeasureMode}",
                                                       "Указание оператору", MessageButton.OK,
                                                       MessageIcon.Information,
                                                       MessageResult.OK);

                            while (await Task<Mult107_109N.RangeSwitchMode>.Factory.StartNew(() => appa107N
                                                                                                .GetRangeSwitchMode) !=
                                   Mult107_109N.RangeSwitchMode.Auto)
                                UserItemOperation.ServicePack.MessageBox()
                                                 .Show("Установите автоматический режим переключения пределов.");
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
                            var setPoint =
                                new MeasPoint<Voltage, Frequency>(0.5M,
                                                                  new Frequency
                                                                  {
                                                                      Value = freqPoint.MainPhysicalQuantity.Value,
                                                                      Multiplier = freqPoint
                                                                                  .MainPhysicalQuantity.Multiplier
                                                                  });
                            flkCalib5522A.Out.Set.Voltage.Ac.SetValue(setPoint);
                            flkCalib5522A.Out.SetOutput(CalibrMain.COut.State.On);
                            Thread.Sleep(100);
                            //измеряем
                            var measurePoint = (decimal) appa107N.GetValue();

                            flkCalib5522A.Out.SetOutput(CalibrMain.COut.State.Off);

                            operation.Getting =
                                new MeasPoint<Frequency>(measurePoint, freqPoint.MainPhysicalQuantity.Multiplier);
                            operation.Expected = freqPoint;

                            var mantisa =
                                MathStatistics.GetMantissa(RangeResolution
                                                          .MainPhysicalQuantity.GetNoramalizeValueToSi() /
                                                           freqPoint.MainPhysicalQuantity.GetNoramalizeValueToSi());

                            //расчет погрешности для конкретной точки предела измерения
                            operation.ErrorCalculation = (inA, inB) =>
                            {
                                var result = BaseTolCoeff * operation.Expected.MainPhysicalQuantity.Value + EdMlRaz *
                                    RangeResolution.MainPhysicalQuantity.Value *
                                    (decimal) (RangeResolution
                                              .MainPhysicalQuantity.Multiplier.GetDoubleValue() /
                                               freqPoint.MainPhysicalQuantity.Multiplier
                                                        .GetDoubleValue()
                                    );

                                MathStatistics.Round(ref result, mantisa);
                                return new MeasPoint<Frequency>(result, freqPoint.MainPhysicalQuantity.Multiplier);
                            };

                            operation.LowerTolerance = operation.Expected - operation.Error;
                            operation.UpperTolerance = operation.Expected + operation.Error;
                            operation.IsGood = () =>
                            {
                                if (operation.Expected == null || operation.Getting == null ||
                                    operation.LowerTolerance == null || operation.UpperTolerance == null) return false;
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
                    operation.CompliteWork = () => Hepls.HelpsCompliteWork(operation, UserItemOperation);
                    DataRow.Add(DataRow.IndexOf(operation) == -1
                                    ? operation
                                    : (BasicOperationVerefication<MeasPoint<Frequency>>) operation.Clone());
                }
            }

            #endregion

            //public override async Task StartWork(CancellationToken token)
            //{
            //    await base.StartWork(token);
            //    appa107N?.Dispose();
            //}
        }

        public class Oper71Freq20HzMeasureBase : Oper7FreqMeasureBase
        {
            public Oper71Freq20HzMeasureBase(Mult107_109N.RangeNominal inRangeNominal,
                IUserItemOperation userItemOperation, string inResourceDir) :
                base(userItemOperation, inResourceDir)
            {
                OperationRangeCode = Mult107_109N.RangeCode.Range1Manual;

                OperMeasureMode = Mult107_109N.MeasureMode.Herz;

                OperationRangeNominal = inRangeNominal;
                Name = OperationRangeNominal.GetStringValue();
                Sheme = ShemeTemplateDefault.TemplateSheme;

                BaseTolCoeff = (decimal) 0.0001;
                EdMlRaz = 50;
                RangeResolution = new MeasPoint<Frequency>(1, UnitMultiplier.Mili);

                HerzPoint = new[]
                {
                    new MeasPoint<Frequency>(10)
                };
            }

            #region Methods

            /// <inheritdoc />
            protected override string GetReportTableName()
            {
                return "FillTabBmOper71Freq20HzMeasureBase";
            }

            #endregion
        }

        public class Oper71Freq200HzMeasureBase : Oper7FreqMeasureBase
        {
            public Oper71Freq200HzMeasureBase(Mult107_109N.RangeNominal inRangeNominal,
                IUserItemOperation userItemOperation, string inResourceDir) :
                base(userItemOperation, inResourceDir)
            {
                OperationRangeCode = Mult107_109N.RangeCode.Range2Manual;
                OperMeasureMode = Mult107_109N.MeasureMode.Herz;

                OperationRangeNominal = inRangeNominal;
                Name = OperationRangeNominal.GetStringValue();
                Sheme = ShemeTemplateDefault.TemplateSheme;

                BaseTolCoeff = (decimal) 0.0001;
                EdMlRaz = 10;
                RangeResolution = new MeasPoint<Frequency>(10, UnitMultiplier.Mili);

                HerzPoint = new[]
                {
                    new MeasPoint<Frequency>(100)
                };
            }

            #region Methods

            /// <inheritdoc />
            protected override string GetReportTableName()
            {
                return "FillTabBmOper71Freq200HzMeasureBase";
            }

            #endregion
        }

        public class Oper71Freq2kHzMeasureBase : Oper7FreqMeasureBase
        {
            public Oper71Freq2kHzMeasureBase(Mult107_109N.RangeNominal inRangeNominal,
                IUserItemOperation userItemOperation, string inResourceDir) :
                base(userItemOperation, inResourceDir)
            {
                OperationRangeCode = Mult107_109N.RangeCode.Range3Manual;
                OperMeasureMode = Mult107_109N.MeasureMode.Herz;

                OperationRangeNominal = inRangeNominal;
                Name = OperationRangeNominal.GetStringValue();
                Sheme = ShemeTemplateDefault.TemplateSheme;

                BaseTolCoeff = (decimal) 0.0001;
                EdMlRaz = 10;
                RangeResolution = new MeasPoint<Frequency>(100, UnitMultiplier.Mili);

                HerzPoint = new[]
                {
                    new MeasPoint<Frequency>(1, UnitMultiplier.Kilo)
                };
            }

            #region Methods

            /// <inheritdoc />
            protected override string GetReportTableName()
            {
                return "FillTabBmOper71Freq2kHzMeasureBase";
            }

            #endregion
        }

        public class Oper71Freq20kHzMeasureBase : Oper7FreqMeasureBase
        {
            public Oper71Freq20kHzMeasureBase(Mult107_109N.RangeNominal inRangeNominal,
                IUserItemOperation userItemOperation, string inResourceDir) :
                base(userItemOperation, inResourceDir)
            {
                OperationRangeCode = Mult107_109N.RangeCode.Range4Manual;
                OperMeasureMode = Mult107_109N.MeasureMode.Herz;

                OperationRangeNominal = inRangeNominal;
                Name = OperationRangeNominal.GetStringValue();
                Sheme = ShemeTemplateDefault.TemplateSheme;
                BaseTolCoeff = (decimal) 0.0001;
                EdMlRaz = 10;
                RangeResolution = new MeasPoint<Frequency>(1);

                HerzPoint = new[]
                {
                    new MeasPoint<Frequency>(10, UnitMultiplier.Kilo)
                };
            }

            #region Methods

            /// <inheritdoc />
            protected override string GetReportTableName()
            {
                return "FillTabBmOper71Freq20kHzMeasureBase";
            }

            #endregion
        }

        public class Oper71Freq200kHzMeasureBase : Oper7FreqMeasureBase
        {
            public Oper71Freq200kHzMeasureBase(Mult107_109N.RangeNominal inRangeNominal,
                IUserItemOperation userItemOperation, string inResourceDir) :
                base(userItemOperation, inResourceDir)
            {
                OperationRangeCode = Mult107_109N.RangeCode.Range5Manual;
                OperMeasureMode = Mult107_109N.MeasureMode.Herz;

                OperationRangeNominal = inRangeNominal;
                Name = OperationRangeNominal.GetStringValue();
                Sheme = ShemeTemplateDefault.TemplateSheme;

                BaseTolCoeff = (decimal) 0.0001;
                EdMlRaz = 10;
                RangeResolution = new MeasPoint<Frequency>(10);

                HerzPoint = new[]
                {
                    new MeasPoint<Frequency>(100, UnitMultiplier.Kilo)
                };
            }

            #region Methods

            /// <inheritdoc />
            protected override string GetReportTableName()
            {
                return "FillTabBmOper71Freq200kHzMeasureBase";
            }

            #endregion
        }

        public class Oper71Freq1MHzMeasureBase : Oper7FreqMeasureBase
        {
            public Oper71Freq1MHzMeasureBase(Mult107_109N.RangeNominal inRangeNominal,
                IUserItemOperation userItemOperation, string inResourceDir) :
                base(userItemOperation, inResourceDir)
            {
                OperationRangeCode = Mult107_109N.RangeCode.Range6Manual;
                OperMeasureMode = Mult107_109N.MeasureMode.Herz;

                OperationRangeNominal = inRangeNominal;
                Name = OperationRangeNominal.GetStringValue();
                Sheme = ShemeTemplateDefault.TemplateSheme;
                BaseTolCoeff = (decimal) 0.0001;
                EdMlRaz = 10;
                RangeResolution = new MeasPoint<Frequency>(100);

                HerzPoint = new[]
                {
                    new MeasPoint<Frequency>(1, UnitMultiplier.Mega)
                };
            }

            #region Methods

            /// <inheritdoc />
            protected override string GetReportTableName()
            {
                return "FillTabBmOper71Freq1MHzMeasureBase";
            }

            #endregion
        }

        #endregion FREQ

        //////////////////////////////******OHM*******///////////////////////////////

        #region OHM

        public class Oper8_1Resistance_200Ohm_Measure : Oper8ResistanceMeasureBase
        {
            public Oper8_1Resistance_200Ohm_Measure(Mult107_109N.RangeNominal inRangeNominal,
                IUserItemOperation userItemOperation, string inResourceDir) : base(userItemOperation, inResourceDir)
            {
                OperationOhmRangeCode = Mult107_109N.RangeCode.Range1Manual;
                OperationOhmRangeNominal = inRangeNominal;

                Name = OperationOhmRangeNominal.GetStringValue();

                BaseTolCoeff = (decimal) 0.003;
                EdMlRaz = 30;
                RangeResolution = new MeasPoint<Resistance>(10, UnitMultiplier.Mili);

                BaseMultiplier = 1;
                OhmPoint = new[]
                {
                    new MeasPoint<Resistance>(50 * BaseMultiplier),
                    new MeasPoint<Resistance>(100 * BaseMultiplier),
                    new MeasPoint<Resistance>(200 * BaseMultiplier)
                };
            }

            #region Methods

            /// <inheritdoc />
            protected override string GetReportTableName()
            {
                return "FillTabBmOper8_1Resistance_200Ohm_Meas";
            }

            #endregion
        }

        public class Oper8_1Resistance_2kOhm_Measure : Oper8ResistanceMeasureBase
        {
            public Oper8_1Resistance_2kOhm_Measure(Mult107_109N.RangeNominal inRangeNominal,
                IUserItemOperation userItemOperation, string inResourceDir) : base(userItemOperation, inResourceDir)
            {
                OperationOhmRangeCode = Mult107_109N.RangeCode.Range2Manual;
                OperationOhmRangeNominal = inRangeNominal;
                Name = OperationOhmRangeNominal.GetStringValue();

                BaseTolCoeff = (decimal) 0.003;
                EdMlRaz = 30;
                RangeResolution = new MeasPoint<Resistance>(100, UnitMultiplier.Mili);

                BaseMultiplier = 1;
                OhmPoint = new[]
                {
                    new MeasPoint<Resistance>((decimal) 0.4 * BaseMultiplier, UnitMultiplier.Kilo),
                    new MeasPoint<Resistance>((decimal) 0.8 * BaseMultiplier, UnitMultiplier.Kilo),
                    new MeasPoint<Resistance>(1 * BaseMultiplier, UnitMultiplier.Kilo),
                    new MeasPoint<Resistance>((decimal) 1.5 * BaseMultiplier, UnitMultiplier.Kilo),
                    new MeasPoint<Resistance>((decimal) 1.8 * BaseMultiplier, UnitMultiplier.Kilo)
                };
            }

            #region Methods

            /// <inheritdoc />
            protected override string GetReportTableName()
            {
                return "FillTabBmOper8_1Resistance_2kOhm_Meas";
            }

            #endregion
        }

        public class Oper8_1Resistance_20kOhm_Measure : Oper8ResistanceMeasureBase
        {
            public Oper8_1Resistance_20kOhm_Measure(Mult107_109N.RangeNominal inRangeNominal,
                IUserItemOperation userItemOperation, string inResourceDir) : base(userItemOperation, inResourceDir)
            {
                OperationOhmRangeCode = Mult107_109N.RangeCode.Range3Manual;
                OperationOhmRangeNominal = inRangeNominal;

                Name = OperationOhmRangeNominal.GetStringValue();

                BaseTolCoeff = (decimal) 0.003;
                EdMlRaz = 30;
                RangeResolution = new MeasPoint<Resistance>(1);

                BaseMultiplier = 10;
                OhmPoint = new[]
                {
                    new MeasPoint<Resistance>((decimal) 0.4 * BaseMultiplier, UnitMultiplier.Kilo),
                    new MeasPoint<Resistance>((decimal) 0.8 * BaseMultiplier, UnitMultiplier.Kilo),
                    new MeasPoint<Resistance>(1 * BaseMultiplier, UnitMultiplier.Kilo),
                    new MeasPoint<Resistance>((decimal) 1.5 * BaseMultiplier, UnitMultiplier.Kilo),
                    new MeasPoint<Resistance>((decimal) 1.8 * BaseMultiplier, UnitMultiplier.Kilo)
                };
            }

            #region Methods

            /// <inheritdoc />
            protected override string GetReportTableName()
            {
                return "FillTabBmOper8_1Resistance_20kOhm_Meas";
            }

            #endregion
        }

        public class Oper8_1Resistance_200kOhm_Measure : Oper8ResistanceMeasureBase
        {
            public Oper8_1Resistance_200kOhm_Measure(Mult107_109N.RangeNominal inRangeNominal,
                IUserItemOperation userItemOperation, string inResourceDir) : base(userItemOperation, inResourceDir)
            {
                OperationOhmRangeCode = Mult107_109N.RangeCode.Range4Manual;
                OperationOhmRangeNominal = inRangeNominal;
                Name = OperationOhmRangeNominal.GetStringValue();

                BaseTolCoeff = (decimal) 0.003;
                EdMlRaz = 30;
                RangeResolution = new MeasPoint<Resistance>(10);

                BaseMultiplier = 100;
                OhmPoint = new[]
                {
                    new MeasPoint<Resistance>((decimal) 0.4 * BaseMultiplier, UnitMultiplier.Kilo),
                    new MeasPoint<Resistance>((decimal) 0.8 * BaseMultiplier, UnitMultiplier.Kilo),
                    new MeasPoint<Resistance>(1 * BaseMultiplier, UnitMultiplier.Kilo),
                    new MeasPoint<Resistance>((decimal) 1.5 * BaseMultiplier, UnitMultiplier.Kilo),
                    new MeasPoint<Resistance>((decimal) 1.8 * BaseMultiplier, UnitMultiplier.Kilo)
                };
            }

            #region Methods

            /// <inheritdoc />
            protected override string GetReportTableName()
            {
                return "FillTabBmOper8_1Resistance_200kOhm_Meas";
            }

            #endregion
        }

        public class Oper8_1Resistance_2MOhm_Measure : Oper8ResistanceMeasureBase
        {
            public Oper8_1Resistance_2MOhm_Measure(Mult107_109N.RangeNominal inRangeNominal,
                IUserItemOperation userItemOperation, string inResourceDir) : base(userItemOperation, inResourceDir)
            {
                OperationOhmRangeCode = Mult107_109N.RangeCode.Range5Manual;
                OperationOhmRangeNominal = inRangeNominal;
                Name = OperationOhmRangeNominal.GetStringValue();

                BaseTolCoeff = (decimal) 0.003;
                EdMlRaz = 50;
                RangeResolution = new MeasPoint<Resistance>(100);

                BaseMultiplier = 1;
                OhmPoint = new[]
                {
                    new MeasPoint<Resistance>((decimal) 0.4 * BaseMultiplier, UnitMultiplier.Mega),
                    new MeasPoint<Resistance>((decimal) 0.8 * BaseMultiplier, UnitMultiplier.Mega),
                    new MeasPoint<Resistance>(1 * BaseMultiplier, UnitMultiplier.Mega),
                    new MeasPoint<Resistance>((decimal) 1.5 * BaseMultiplier, UnitMultiplier.Mega),
                    new MeasPoint<Resistance>((decimal) 1.8 * BaseMultiplier, UnitMultiplier.Mega)
                };
            }

            #region Methods

            /// <inheritdoc />
            protected override string GetReportTableName()
            {
                return "FillTabBmOper8_1Resistance_2MOhm_Meas";
            }

            #endregion
        }

        public class Oper8_1Resistance_20MOhm_Measure : Oper8ResistanceMeasureBase
        {
            public Oper8_1Resistance_20MOhm_Measure(Mult107_109N.RangeNominal inRangeNominal,
                IUserItemOperation userItemOperation, string inResourceDir) : base(userItemOperation, inResourceDir)
            {
                OperationOhmRangeCode = Mult107_109N.RangeCode.Range6Manual;
                OperationOhmRangeNominal = inRangeNominal;
                Name = OperationOhmRangeNominal.GetStringValue();

                BaseTolCoeff = (decimal) 0.05;
                EdMlRaz = 50;
                RangeResolution = new MeasPoint<Resistance>(1, UnitMultiplier.Kilo);

                BaseMultiplier = 1;
                OhmPoint = new[]
                {
                    new MeasPoint<Resistance>(5 * BaseMultiplier, UnitMultiplier.Mega),
                    new MeasPoint<Resistance>(10 * BaseMultiplier, UnitMultiplier.Mega),
                    new MeasPoint<Resistance>(20 * BaseMultiplier, UnitMultiplier.Mega)
                };
            }

            #region Methods

            /// <inheritdoc />
            protected override string GetReportTableName()
            {
                return "FillTabBmOper8_1Resistance_20MOhm_Meas";
            }

            #endregion
        }

        public class Oper8_1Resistance_200MOhm_Measure : Oper8ResistanceMeasureBase
        {
            public Oper8_1Resistance_200MOhm_Measure(Mult107_109N.RangeNominal inRangeNominal,
                IUserItemOperation userItemOperation, string inResourceDir) : base(userItemOperation, inResourceDir)
            {
                OperationOhmRangeCode = Mult107_109N.RangeCode.Range7Manual;
                OperationOhmRangeNominal = inRangeNominal;
                Name = OperationOhmRangeNominal.GetStringValue();

                BaseTolCoeff = (decimal) 0.05;
                EdMlRaz = 20;
                RangeResolution = new MeasPoint<Resistance>(1, UnitMultiplier.Mega);

                BaseMultiplier = 10;
                OhmPoint = new[]
                {
                    new MeasPoint<Resistance>(5 * BaseMultiplier, UnitMultiplier.Mega),
                    new MeasPoint<Resistance>(10 * BaseMultiplier, UnitMultiplier.Mega),
                    new MeasPoint<Resistance>(20 * BaseMultiplier, UnitMultiplier.Mega)
                };
            }

            #region Methods

            /// <inheritdoc />
            protected override string GetReportTableName()
            {
                return "FillTabBmOper8_1Resistance_200MOhm_Meas";
            }

            #endregion
        }

        public class Oper8_1Resistance_2GOhm_Measure : Oper8ResistanceMeasureBase
        {
            public Oper8_1Resistance_2GOhm_Measure(Mult107_109N.RangeNominal inRangeNominal,
                IUserItemOperation userItemOperation, string inResourceDir) : base(userItemOperation, inResourceDir)
            {
                OperationOhmRangeCode = Mult107_109N.RangeCode.Range8Manual;
                OperationOhmRangeNominal = inRangeNominal;
                Name = OperationOhmRangeNominal.GetStringValue();

                BaseTolCoeff = (decimal) 0.05;
                EdMlRaz = 8;
                RangeResolution = new MeasPoint<Resistance>(100, UnitMultiplier.Mega);

                OhmPoint = new[]
                {
                    new MeasPoint<Resistance>((decimal) 0.9, UnitMultiplier.Giga)
                };
            }

            #region Methods

            /// <inheritdoc />
            protected override string GetReportTableName()
            {
                return "FillTabBmOper8_1Resistance_2GOhm_Meas";
            }

            #endregion
        }

        public class Oper8ResistanceMeasureBase : ParagraphBase<MeasPoint<Resistance>>
        {
            private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

            #region Fields

            //множители для пределов.
            public decimal BaseMultiplier;

            public decimal BaseTolCoeff;

            /// <summary>
            /// Число пределов измерения
            /// </summary>
            private int CountOfRanges;

            /// <summary>
            /// довесок к формуле погрешности- единица младшего разряда
            /// </summary>
            public int EdMlRaz = 10; //значение для пределов свыше 200 мВ

            /// <summary>
            /// Массив поверяемых точек напряжения.
            /// </summary>
            protected MeasPoint<Resistance>[] OhmPoint;

            protected MeasPoint<Resistance> RangeResolution;

            #endregion

            #region Property

            /// <summary>
            /// Код предела измерения на приборе
            /// </summary>
            public Mult107_109N.RangeCode OperationOhmRangeCode { get; protected set; }

            /// <summary>
            /// Предел измерения поверяемого прибора, необходимый для работы
            /// </summary>
            public Mult107_109N.RangeNominal OperationOhmRangeNominal { get; protected set; }

            /// <summary>
            /// Режим операции измерения прибора
            /// </summary>
            public Mult107_109N.MeasureMode OperMeasureMode { get; protected set; }

            //контрлируемый прибор
            protected Mult107_109N appa107N { get; set; }

            //эталон
            protected Calib5522A flkCalib5522A { get; set; }

            #endregion

            public Oper8ResistanceMeasureBase(IUserItemOperation userItemOperation, string inResourceDir) :
                base(userItemOperation)
            {
                Name = "Определение погрешности измерения электрического сопротивления";
                OperMeasureMode = Mult107_109N.MeasureMode.Ohm;

                OperationOhmRangeCode = Mult107_109N.RangeCode.Range1Manual;
                OperationOhmRangeNominal = Mult107_109N.RangeNominal.RangeNone;

                DataRow = new List<IBasicOperation<MeasPoint<Resistance>>>();
                Sheme = ShemeTemplateDefault.TemplateSheme;
                Sheme.AssemblyLocalName = inResourceDir;
            }

            #region Methods

            protected override DataTable FillData()
            {
                var dataTable = base.FillData();

                foreach (var row in DataRow)
                {
                    var dataRow = dataTable.NewRow();
                    var dds = row as BasicOperationVerefication<MeasPoint<Resistance>>;
                    // ReSharper disable once PossibleNullReferenceException
                    if (dds == null) continue;
                    dataRow[0] = OperationOhmRangeNominal.GetStringValue();
                    dataRow[1] = dds.Expected?.Description;
                    dataRow[2] = dds.Getting?.Description;
                    dataRow[3] = dds.LowerTolerance?.Description;
                    dataRow[4] = dds.UpperTolerance?.Description;
                    if (dds.IsGood == null)
                        dataRow[5] = "не выполнено";
                    else
                        dataRow[5] = dds.IsGood() ? "Годен" : "Брак";
                    dataTable.Rows.Add(dataRow);
                }

                return dataTable;
            }

            /// <inheritdoc />
            protected override string[] GenerateDataColumnTypeObject()
            {
                return new[]
                {
                    "Предел измерения",
                    "Поверяемая точка", "Измеренное значение", "Минимальное допустимое значение",
                    "Максимальное допустимое значение"
                }.Concat(base.GenerateDataColumnTypeObject()).ToArray();
                ;
            }

            /// <inheritdoc />
            protected override string GetReportTableName()
            {
                return null;
            }

            protected override void InitWork(CancellationTokenSource token)
            {
                base.InitWork(token);
                if (appa107N == null || flkCalib5522A == null) return;

                foreach (var currPoint in OhmPoint)
                {
                    var operation = new BasicOperationVerefication<MeasPoint<Resistance>>();
                    operation.InitWork = async () =>
                    {
                        try
                        {
                            if (appa107N.StringConnection.Equals("COM1"))
                                appa107N.StringConnection = GetStringConnect(appa107N);
                            flkCalib5522A.StringConnection ??= GetStringConnect(flkCalib5522A);

                            await Task.Run(() => { flkCalib5522A.Out.SetOutput(CalibrMain.COut.State.Off); });

                            while (OperMeasureMode !=
                                   await Task<Mult107_109N.MeasureMode>.Factory.StartNew(() => appa107N.GetMeasureMode))
                                UserItemOperation.ServicePack.MessageBox()
                                                 .Show($"Установите режим измерения: {OperMeasureMode.GetStringValue()} {OperMeasureMode}",
                                                       "Указание оператору", MessageButton.OK,
                                                       MessageIcon.Information,
                                                       MessageResult.OK);

                            while (await Task<Mult107_109N.RangeSwitchMode>.Factory.StartNew(() => appa107N
                                                                                                .GetRangeSwitchMode) ==
                                   Mult107_109N.RangeSwitchMode.Auto)
                                UserItemOperation.ServicePack.MessageBox()
                                                 .Show("Установите ручной режим переключения пределов.");

                            while (OperationOhmRangeNominal !=
                                   await Task<Mult107_109N.RangeNominal>
                                        .Factory.StartNew(() => appa107N.GetRangeNominal))
                            {
                                int countPushRangeButton;

                                if (currPoint.MainPhysicalQuantity.Multiplier == UnitMultiplier.Mili)
                                {
                                    UserItemOperation.ServicePack.MessageBox()
                                                     .Show($"Текущий предел измерения прибора {appa107N.GetRangeNominal.GetStringValue()}\n Необходимо установить предел {OperationOhmRangeNominal.GetStringValue()} " +
                                                           $"Нажмите на приборе клавишу Range {countPushRangeButton = 1} раз.",
                                                           "Указание оператору", MessageButton.OK,
                                                           MessageIcon.Information,
                                                           MessageResult.OK);
                                }
                                else
                                {
                                    //работает только для ручного режима переключения пределов
                                    CountOfRanges = 8;
                                    var curRange = (int) appa107N.GetRangeCode - 127;
                                    var targetRange = (int) OperationOhmRangeCode - 127;
                                    countPushRangeButton =
                                        Hepls.CountOfPushButton(CountOfRanges, curRange, targetRange);

                                    UserItemOperation.ServicePack.MessageBox()
                                                     .Show($"Текущий предел измерения прибора {appa107N.GetRangeNominal.GetStringValue()}\n Необходимо установить предел {OperationOhmRangeNominal.GetStringValue()} " +
                                                           $"Нажмите на приборе клавишу Range {countPushRangeButton} раз.",
                                                           "Указание оператору", MessageButton.OK,
                                                           MessageIcon.Information,
                                                           MessageResult.OK);
                                }
                            }
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
                            // компенсаци проводов на младших пределах
                            decimal refValue = 0;
                            if (currPoint.MainPhysicalQuantity.Multiplier == UnitMultiplier.None)
                            {
                                flkCalib5522A.Out.Set.Resistance.SetValue(0);
                                flkCalib5522A.Out.SetOutput(CalibrMain.COut.State.On);
                                Thread.Sleep(3000);
                                //измеряем
                                refValue = (decimal) appa107N.GetValue();
                            }

                            flkCalib5522A.Out.Set.Resistance.SetValue(currPoint);
                            flkCalib5522A.Out.SetOutput(CalibrMain.COut.State.On);

                            if (currPoint.MainPhysicalQuantity.Multiplier == UnitMultiplier.Mega) Thread.Sleep(9000);
                            else if (currPoint.MainPhysicalQuantity.Multiplier == UnitMultiplier.Giga)
                                Thread.Sleep(12000);
                            else
                                Thread.Sleep(3000);
                            //измеряем
                            var measurePoint = (decimal) appa107N.GetValue() - refValue;

                            flkCalib5522A.Out.SetOutput(CalibrMain.COut.State.Off);

                            var mantisa =
                                MathStatistics.GetMantissa(RangeResolution
                                                          .MainPhysicalQuantity.GetNoramalizeValueToSi() /
                                                           currPoint.MainPhysicalQuantity.GetNoramalizeValueToSi());
                            //округляем измерения
                            MathStatistics.Round(ref measurePoint, mantisa);

                            operation.Getting =
                                new MeasPoint<Resistance>(measurePoint, currPoint.MainPhysicalQuantity.Multiplier);
                            operation.Expected = currPoint;
                            //расчет погрешности для конкретной точки предела измерения
                            operation.ErrorCalculation = (inA, inB) =>
                            {
                                var result = BaseTolCoeff * Math.Abs(operation.Expected.MainPhysicalQuantity.Value) +
                                             EdMlRaz *
                                             RangeResolution.MainPhysicalQuantity.Value *
                                             (decimal) (RangeResolution
                                                       .MainPhysicalQuantity.Multiplier.GetDoubleValue() /
                                                        currPoint.MainPhysicalQuantity.Multiplier
                                                                 .GetDoubleValue());

                                MathStatistics.Round(ref result, mantisa);
                                return new MeasPoint<Resistance>(result);
                            };

                            operation.LowerTolerance = operation.Expected - operation.Error;
                            operation.UpperTolerance = operation.Expected + operation.Error;
                            operation.IsGood = () =>
                            {
                                if (operation.Expected == null || operation.Getting == null ||
                                    operation.LowerTolerance == null || operation.UpperTolerance == null) return false;

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
                    operation.CompliteWork = () => Hepls.HelpsCompliteWork(operation, UserItemOperation);
                    DataRow.Add(DataRow.IndexOf(operation) == -1
                                    ? operation
                                    : (BasicOperationVerefication<MeasPoint<Resistance>>) operation.Clone());
                }
            }

            #endregion

            //public override async Task StartWork(CancellationToken token)
            //{
            //    await base.StartWork(token);
            //    appa107N?.Dispose();
            //}
        }

        #endregion OHM

        //////////////////////////////******FAR*******///////////////////////////////

        #region FAR

        public class Oper9FarMeasureBase : ParagraphBase<MeasPoint<Capacity>>
        {
            private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

            #region Fields

            //множители для пределов.
            public decimal BaseMultiplier;

            public decimal BaseTolCoeff;

            /// <summary>
            /// Число пределов измерения
            /// </summary>
            private int CountOfRanges;

            /// <summary>
            /// довесок к формуле погрешности- единица младшего разряда
            /// </summary>
            public int EdMlRaz; //значение для пределов свыше 200 мВ

            /// <summary>
            /// Массив поверяемых точек напряжения.
            /// </summary>
            protected MeasPoint<Capacity>[] FarMeasPoints;

            protected MeasPoint<Capacity> RangeResolution;

            #endregion

            #region Property

            /// <summary>
            /// Код предела измерения на приборе
            /// </summary>
            public Mult107_109N.RangeCode OperationRangeCode { get; protected set; }

            /// <summary>
            /// Предел измерения поверяемого прибора, необходимый для работы
            /// </summary>
            public Mult107_109N.RangeNominal OperationRangeNominal { get; protected set; }

            /// <summary>
            /// Режим операции измерения прибора
            /// </summary>
            public Mult107_109N.MeasureMode OperMeasureMode { get; protected set; }

            //контрлируемый прибор
            protected Mult107_109N appa107N { get; set; }

            //эталон
            protected Calib5522A flkCalib5522A { get; set; }

            #endregion

            public Oper9FarMeasureBase(IUserItemOperation userItemOperation) : base(userItemOperation)
            {
                Name = "Определение погрешности измерения электрической ёмкости";
                OperMeasureMode = Mult107_109N.MeasureMode.Cap;

                OperationRangeCode = Mult107_109N.RangeCode.Range1Manual;
                OperationRangeNominal = Mult107_109N.RangeNominal.RangeNone;
                Sheme = ShemeTemplateDefault.TemplateSheme;

                CountOfRanges = 8;
            }

            #region Methods

            protected override DataTable FillData()
            {
                var dataTable = base.FillData();

                foreach (var row in DataRow)
                {
                    var dataRow = dataTable.NewRow();
                    var dds = row as BasicOperationVerefication<MeasPoint<Capacity>>;
                    // ReSharper disable once PossibleNullReferenceException
                    if (dds == null) continue;
                    dataRow[0] = OperationRangeNominal.GetStringValue();
                    dataRow[1] = dds.Expected?.Description;
                    dataRow[2] = dds.Getting?.Description;
                    dataRow[3] = dds.LowerTolerance?.Description;
                    dataRow[4] = dds.UpperTolerance?.Description;
                    if (dds.IsGood == null)
                        dataRow[5] = "не выполнено";
                    else
                        dataRow[5] = dds.IsGood() ? "Годен" : "Брак";
                    dataTable.Rows.Add(dataRow);
                }

                return dataTable;
            }

            /// <inheritdoc />
            protected override string[] GenerateDataColumnTypeObject()
            {
                return new[]
                {
                    "Предел измерения",
                    "Поверяемая точка", "Измеренное значение", "Минимальное допустимое значение",
                    "Максимальное допустимое значение"
                }.Concat(base.GenerateDataColumnTypeObject()).ToArray();
                ;
            }

            /// <inheritdoc />
            protected override string GetReportTableName()
            {
                return null;
            }

            protected override void InitWork(CancellationTokenSource token)
            {
                base.InitWork(token);
                if (appa107N == null || flkCalib5522A == null) return;

                foreach (var currPoint in FarMeasPoints)
                {
                    var operation = new BasicOperationVerefication<MeasPoint<Capacity>>();
                    operation.InitWork = async () =>
                    {
                        try
                        {
                            if (appa107N.StringConnection.Equals("COM1"))
                                appa107N.StringConnection = GetStringConnect(appa107N);
                            flkCalib5522A.StringConnection ??= GetStringConnect(flkCalib5522A);

                            await Task.Run(() => { flkCalib5522A.Out.SetOutput(CalibrMain.COut.State.Off); });

                            while (OperMeasureMode !=
                                   await Task<Mult107_109N.MeasureMode>.Factory.StartNew(() => appa107N.GetMeasureMode))
                                UserItemOperation.ServicePack.MessageBox()
                                                 .Show($"Установите режим измерения: {OperMeasureMode.GetStringValue()} {OperMeasureMode}",
                                                       "Указание оператору", MessageButton.OK,
                                                       MessageIcon.Information,
                                                       MessageResult.OK);

                            while (await Task<Mult107_109N.RangeSwitchMode>.Factory.StartNew(() => appa107N
                                                                                                .GetRangeSwitchMode) ==
                                   Mult107_109N.RangeSwitchMode.Auto)
                                UserItemOperation.ServicePack.MessageBox()
                                                 .Show("Установите ручной режим переключения пределов.");

                            while (OperationRangeNominal !=
                                   await Task<Mult107_109N.RangeNominal>
                                        .Factory.StartNew(() => appa107N.GetRangeNominal))
                            {
                                int countPushRangeButton;
                                CountOfRanges = 8;
                                var curRange = (int) appa107N.GetRangeCode - 127;
                                var targetRange = (int) OperationRangeCode - 127;
                                countPushRangeButton = Hepls.CountOfPushButton(CountOfRanges, curRange, targetRange);

                                UserItemOperation.ServicePack.MessageBox()
                                                 .Show($"Текущий предел измерения прибора {appa107N.GetRangeNominal.GetStringValue()}\n Необходимо установить предел {OperationRangeNominal.GetStringValue()} " +
                                                       $"Нажмите на приборе клавишу Range {countPushRangeButton} раз.",
                                                       "Указание оператору", MessageButton.OK, MessageIcon.Information,
                                                       MessageResult.OK);
                            }
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
                            flkCalib5522A.Out.Set.Capacitance.SetValue(currPoint);
                            flkCalib5522A.Out.SetOutput(CalibrMain.COut.State.On);
                            if (currPoint.MainPhysicalQuantity.Multiplier == UnitMultiplier.Mili &&
                                appa107N.GetRangeNominal == Mult107_109N.RangeNominal.Range40mF)
                                Thread.Sleep(90000);
                            else if (currPoint.MainPhysicalQuantity.Multiplier == UnitMultiplier.Mili &&
                                     appa107N.GetRangeNominal == Mult107_109N.RangeNominal.Range4mF)
                                Thread.Sleep(12000);
                            else
                                Thread.Sleep(4000);

                            //измеряем
                            var measurePoint = (decimal) appa107N.GetSingleValue();
                            flkCalib5522A.Out.SetOutput(CalibrMain.COut.State.Off);

                            var mantisa =
                                MathStatistics.GetMantissa(RangeResolution
                                                          .MainPhysicalQuantity.GetNoramalizeValueToSi() /
                                                           currPoint.MainPhysicalQuantity.GetNoramalizeValueToSi());
                            //округляем измерения
                            MathStatistics.Round(ref measurePoint, mantisa);

                            operation.Getting =
                                new MeasPoint<Capacity>(measurePoint, currPoint.MainPhysicalQuantity.Multiplier);
                            operation.Expected = currPoint;
                            //расчет погрешности для конкретной точки предела измерения
                            operation.ErrorCalculation = (inA, inB) =>
                            {
                                var result = BaseTolCoeff * Math.Abs(operation.Expected.MainPhysicalQuantity.Value) +
                                             EdMlRaz *
                                             RangeResolution.MainPhysicalQuantity.Value *
                                             (decimal) (RangeResolution
                                                       .MainPhysicalQuantity.Multiplier.GetDoubleValue() /
                                                        currPoint.MainPhysicalQuantity.Multiplier
                                                                 .GetDoubleValue());

                                //округляем измерения
                                MathStatistics.Round(ref measurePoint, mantisa);
                                return new MeasPoint<Capacity>(result.CompareTo(currPoint.MainPhysicalQuantity
                                                                                         .Multiplier));
                            };

                            operation.LowerTolerance = operation.Expected - operation.Error;
                            operation.UpperTolerance = operation.Expected + operation.Error;
                            operation.IsGood = () =>
                            {
                                if (operation.Expected == null || operation.Getting == null ||
                                    operation.LowerTolerance == null || operation.UpperTolerance == null) return false;
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
                    operation.CompliteWork = () => Hepls.HelpsCompliteWork(operation, UserItemOperation);
                    DataRow.Add(DataRow.IndexOf(operation) == -1
                                    ? operation
                                    : (BasicOperationVerefication<MeasPoint<Capacity>>) operation.Clone());
                }
            }

            #endregion

            //public override async Task StartWork(CancellationToken token)
            //{
            //    await base.StartWork(token);
            //    appa107N?.Dispose();
            //}
        }

        public class Oper9_1Far_4nF_Measure : Oper9FarMeasureBase
        {
            public Oper9_1Far_4nF_Measure(Mult107_109N.RangeNominal inRangeNominal,
                IUserItemOperation userItemOperation, string inResourceDir) :
                base(userItemOperation)
            {
                OperationRangeCode = Mult107_109N.RangeCode.Range1Manual;
                OperationRangeNominal = inRangeNominal;
                BaseTolCoeff = (decimal) 0.015;
                EdMlRaz = 10;
                RangeResolution = new MeasPoint<Capacity>(1, UnitMultiplier.Pico);

                Name = OperationRangeNominal.GetStringValue();

                FarMeasPoints = new[]
                {
                    new MeasPoint<Capacity>(3, UnitMultiplier.Nano)
                };

                Sheme = ShemeTemplateDefault.TemplateSheme;
                Sheme.AssemblyLocalName = inResourceDir;
            }

            #region Methods

            /// <inheritdoc />
            protected override string GetReportTableName()
            {
                return "FillTabBmOper9_1Far_4nF_Measure";
            }

            #endregion
        }

        public class Oper9_1Far_40nF_Measure : Oper9FarMeasureBase
        {
            public Oper9_1Far_40nF_Measure(Mult107_109N.RangeNominal inRangeNominal,
                IUserItemOperation userItemOperation) :
                base(userItemOperation)
            {
                OperationRangeCode = Mult107_109N.RangeCode.Range2Manual;
                OperationRangeNominal = inRangeNominal;
                RangeResolution = new MeasPoint<Capacity>(10, UnitMultiplier.Pico);
                BaseTolCoeff = (decimal) 0.015;
                EdMlRaz = 10;
                Name = OperationRangeNominal.GetStringValue();

                FarMeasPoints = new[]
                {
                    new MeasPoint<Capacity>(30, UnitMultiplier.Nano)
                };
            }

            #region Methods

            /// <inheritdoc />
            protected override string GetReportTableName()
            {
                return "FillTabBmOper9_1Far_40nF_Measure";
            }

            #endregion
        }

        public class Oper9_1Far_400nF_Measure : Oper9FarMeasureBase
        {
            public Oper9_1Far_400nF_Measure(Mult107_109N.RangeNominal inRangeNominal,
                IUserItemOperation userItemOperation)
                : base(userItemOperation)
            {
                OperationRangeCode = Mult107_109N.RangeCode.Range3Manual;
                OperationRangeNominal = inRangeNominal;
                RangeResolution = new MeasPoint<Capacity>(100, UnitMultiplier.Pico);
                BaseTolCoeff = (decimal) 0.009;
                EdMlRaz = 5;
                Name = OperationRangeNominal.GetStringValue();

                BaseTolCoeff = (decimal) 0.009;
                EdMlRaz = 5;

                FarMeasPoints = new[]
                {
                    new MeasPoint<Capacity>(300, UnitMultiplier.Nano)
                };
            }

            #region Methods

            /// <inheritdoc />
            protected override string GetReportTableName()
            {
                return "FillTabBmOper9_1Far_400nF_Measure";
            }

            #endregion
        }

        public class Oper9_1Far_4uF_Measure : Oper9FarMeasureBase
        {
            public Oper9_1Far_4uF_Measure(Mult107_109N.RangeNominal inRangeNominal,
                IUserItemOperation userItemOperation) :
                base(userItemOperation)
            {
                OperationRangeCode = Mult107_109N.RangeCode.Range4Manual;
                OperationRangeNominal = inRangeNominal;
                Name = OperationRangeNominal.GetStringValue();

                BaseTolCoeff = (decimal) 0.009;
                EdMlRaz = 5;
                RangeResolution = new MeasPoint<Capacity>(1, UnitMultiplier.Nano);
                BaseTolCoeff = (decimal) 0.009;
                EdMlRaz = 5;

                FarMeasPoints = new[]
                {
                    new MeasPoint<Capacity>(3, UnitMultiplier.Micro)
                };
            }

            #region Methods

            /// <inheritdoc />
            protected override string GetReportTableName()
            {
                return "FillTabBmOper9_1Far_4uF_Measure";
            }

            #endregion
        }

        public class Oper9_1Far_40uF_Measure : Oper9FarMeasureBase
        {
            public Oper9_1Far_40uF_Measure(Mult107_109N.RangeNominal inRangeNominal,
                IUserItemOperation userItemOperation) :
                base(userItemOperation)
            {
                OperationRangeCode = Mult107_109N.RangeCode.Range5Manual;
                OperationRangeNominal = inRangeNominal;
                Name = OperationRangeNominal.GetStringValue();

                BaseTolCoeff = (decimal) 0.012;
                EdMlRaz = 5;
                RangeResolution = new MeasPoint<Capacity>(10, UnitMultiplier.Nano);

                FarMeasPoints = new[]
                {
                    new MeasPoint<Capacity>(30, UnitMultiplier.Micro)
                };
            }

            #region Methods

            /// <inheritdoc />
            protected override string GetReportTableName()
            {
                return "FillTabBmOper9_1Far_40uF_Measure";
            }

            #endregion
        }

        public class Oper9_1Far_400uF_Measure : Oper9FarMeasureBase
        {
            public Oper9_1Far_400uF_Measure(Mult107_109N.RangeNominal inRangeNominal,
                IUserItemOperation userItemOperation)
                : base(userItemOperation)
            {
                OperationRangeCode = Mult107_109N.RangeCode.Range6Manual;
                OperationRangeNominal = inRangeNominal;
                Name = OperationRangeNominal.GetStringValue();

                BaseTolCoeff = (decimal) 0.012;
                EdMlRaz = 5;
                RangeResolution = new MeasPoint<Capacity>(100, UnitMultiplier.Nano);

                FarMeasPoints = new[]
                {
                    new MeasPoint<Capacity>(300, UnitMultiplier.Micro)
                };
            }

            #region Methods

            /// <inheritdoc />
            protected override string GetReportTableName()
            {
                return "FillTabBmOper9_1Far_400uF_Measure";
            }

            #endregion
        }

        public class Oper9_1Far_4mF_Measure : Oper9FarMeasureBase
        {
            public Oper9_1Far_4mF_Measure(Mult107_109N.RangeNominal inRangeNominal,
                IUserItemOperation userItemOperation) :
                base(userItemOperation)
            {
                OperationRangeCode = Mult107_109N.RangeCode.Range7Manual;
                OperationRangeNominal = inRangeNominal;
                Name = OperationRangeNominal.GetStringValue();

                BaseTolCoeff = (decimal) 0.015;
                EdMlRaz = 5;
                RangeResolution = new MeasPoint<Capacity>(1, UnitMultiplier.Micro);

                FarMeasPoints = new[]
                {
                    new MeasPoint<Capacity>(3, UnitMultiplier.Mili)
                };
            }

            #region Methods

            /// <inheritdoc />
            protected override string GetReportTableName()
            {
                return "FillTabBmOper9_1Far_4mF_Measure";
            }

            #endregion
        }

        public class Oper9_1Far_40mF_Measure : Oper9FarMeasureBase
        {
            public Oper9_1Far_40mF_Measure(Mult107_109N.RangeNominal inRangeNominal,
                IUserItemOperation userItemOperation) :
                base(userItemOperation)
            {
                OperationRangeCode = Mult107_109N.RangeCode.Range8Manual;
                OperationRangeNominal = inRangeNominal;
                Name = OperationRangeNominal.GetStringValue();

                BaseTolCoeff = (decimal) 0.015;
                EdMlRaz = 5;
                RangeResolution = new MeasPoint<Capacity>(10, UnitMultiplier.Micro);

                FarMeasPoints = new[]
                {
                    new MeasPoint<Capacity>(30, UnitMultiplier.Mili)
                };
            }

            #region Methods

            /// <inheritdoc />
            protected override string GetReportTableName()
            {
                return "FillTabBmOper9_1Far_40mF_Measure";
            }

            #endregion
        }

        #endregion FAR

        //////////////////////////////******TEMP*******///////////////////////////////

        #region TEMP

        public class Oper10TemperatureMeasureBase : ParagraphBase<MeasPoint<CelsiumGrad>>
        {
            private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

            #region Fields

            //множители для пределов.
            public decimal BaseMultiplier;

            /// <summary>
            /// Процент от измеряемой точки для расчета погрешности (уже переведе в абсолютные единицы).
            /// </summary>
            public decimal BaseTolCoeff = (decimal) 0.0006;

            /// <summary>
            /// Число пределов измерения.
            /// </summary>
            private int CountOfRanges;

            /// <summary>
            /// Массив поверяемых точек напряжения.
            /// </summary>
            protected MeasPoint<CelsiumGrad>[] DegC_Point;

            /// <summary>
            /// довесок к формуле погрешности- число единиц младшего разряда
            /// </summary>
            public int EdMlRaz;

            /// <summary>
            /// Разарешение пределеа измерения (последний значащий разряд)
            /// </summary>
            protected MeasPoint<CelsiumGrad> RangeResolution;

            #endregion

            #region Property

            /// <summary>
            /// Код предела измерения на приборе
            /// </summary>
            public Mult107_109N.RangeCode OperationRangeCode { get; protected set; }

            /// <summary>
            /// Предел измерения поверяемого прибора, необходимый для работы
            /// </summary>
            public Mult107_109N.RangeNominal OperationRangeNominal { get; protected set; }

            /// <summary>
            /// Режим операции измерения прибора
            /// </summary>
            public Mult107_109N.MeasureMode OperMeasureMode { get; protected set; }

            //контрлируемый прибор
            protected Mult107_109N appa107N { get; set; }

            //эталон
            protected Calib5522A flkCalib5522A { get; set; }

            #endregion

            public Oper10TemperatureMeasureBase(IUserItemOperation userItemOperation, string inDirectory) : base(userItemOperation)
            {
                Name = "Определение погрешности измерения температуры, градусы Цельсия";
                OperMeasureMode = Mult107_109N.MeasureMode.degC;

                OperationRangeCode = Mult107_109N.RangeCode.Range1Manual;
                OperationRangeNominal = Mult107_109N.RangeNominal.RangeNone;

                DataRow = new List<IBasicOperation<MeasPoint<CelsiumGrad>>>();
                Sheme = new ShemeImage
                {
                    AssemblyLocalName = inDirectory,
                    Description = "Измерительная схема",
                    Number = 5,
                    FileName = @"appa_10XN_Temp_5522.jpg",
                    ExtendedDescription = "Соберите измерительную схему, согласно рисунку"
                };
            }

            #region Methods

            protected override DataTable FillData()
            {
                var dataTable = base.FillData();

                foreach (var row in DataRow)
                {
                    var dataRow = dataTable.NewRow();
                    var dds = row as BasicOperationVerefication<MeasPoint<CelsiumGrad>>;
                    // ReSharper disable once PossibleNullReferenceException
                    if (dds == null) continue;
                    dataRow[0] = OperationRangeNominal.GetStringValue();
                    dataRow[1] = dds.Expected?.Description;
                    dataRow[2] = dds.Getting?.Description;
                    dataRow[3] = dds.LowerTolerance?.Description;
                    dataRow[4] = dds.UpperTolerance?.Description;
                    if (dds.IsGood == null)
                        dataRow[5] = "не выполнено";
                    else
                        dataRow[5] = dds.IsGood() ? "Годен" : "Брак";
                    dataTable.Rows.Add(dataRow);
                }

                return dataTable;
            }

            /// <inheritdoc />
            protected override string[] GenerateDataColumnTypeObject()
            {
                return new[]
                {
                    "Предел измерения",
                    "Поверяемая точка", "Измеренное значение", "Минимальное допустимое значение",
                    "Максимальное допустимое значение"
                }.Concat(base.GenerateDataColumnTypeObject()).ToArray();
            }

            /// <inheritdoc />
            protected override string GetReportTableName()
            {
                return null;
            }

            protected override void InitWork(CancellationTokenSource token)
            {
                base.InitWork(token);
                if (appa107N == null || flkCalib5522A == null) return;

                foreach (var currPoint in DegC_Point)
                {
                    var operation = new BasicOperationVerefication<MeasPoint<CelsiumGrad>>();
                    operation.InitWork = async () =>
                    {
                        try
                        {
                            if (appa107N.StringConnection.Equals("COM1"))
                                appa107N.StringConnection = GetStringConnect(appa107N);
                            flkCalib5522A.StringConnection ??= GetStringConnect(flkCalib5522A);

                            await Task.Run(() => { flkCalib5522A.Out.SetOutput(CalibrMain.COut.State.Off); });

                            while (OperMeasureMode !=
                                   await Task<Mult107_109N.MeasureMode>.Factory.StartNew(() => appa107N.GetMeasureMode))
                                UserItemOperation.ServicePack.MessageBox()
                                                 .Show($"Установите режим измерения: {OperMeasureMode.GetStringValue()} {OperMeasureMode}",
                                                       "Указание оператору", MessageButton.OK,
                                                       MessageIcon.Information,
                                                       MessageResult.OK);

                            while (await Task<Mult107_109N.RangeSwitchMode>.Factory.StartNew(() => appa107N
                                                                                                .GetRangeSwitchMode) ==
                                   Mult107_109N.RangeSwitchMode.Auto)
                                UserItemOperation.ServicePack.MessageBox()
                                                 .Show("Установите ручной режим переключения пределов.");

                            while (OperationRangeNominal !=
                                   await Task<Mult107_109N.RangeNominal>
                                        .Factory.StartNew(() => appa107N.GetRangeNominal))
                            {
                                int countPushRangeButton;

                                if (currPoint.MainPhysicalQuantity.Multiplier == UnitMultiplier.Mili)
                                {
                                    CountOfRanges = 2;
                                    UserItemOperation.ServicePack.MessageBox()
                                                     .Show($"Текущий предел измерения прибора {appa107N.GetRangeNominal.GetStringValue()}\n Необходимо установить предел {OperationRangeNominal.GetStringValue()} " +
                                                           $"Нажмите на приборе клавишу Range {countPushRangeButton = 1} раз.",
                                                           "Указание оператору", MessageButton.OK,
                                                           MessageIcon.Information,
                                                           MessageResult.OK);
                                }
                                else
                                {
                                    //работает только для ручного режима переключения пределов
                                    CountOfRanges = 2;
                                    var curRange = (int) appa107N.GetRangeCode - 127;
                                    var targetRange = (int) OperationRangeCode - 127;
                                    countPushRangeButton =
                                        Hepls.CountOfPushButton(CountOfRanges, curRange, targetRange);

                                    UserItemOperation.ServicePack.MessageBox()
                                                     .Show($"Текущий предел измерения прибора {appa107N.GetRangeNominal.GetStringValue()}\n Необходимо установить предел {OperationRangeNominal.GetStringValue()} " +
                                                           $"Нажмите на приборе клавишу Range {countPushRangeButton} раз.",
                                                           "Указание оператору", MessageButton.OK,
                                                           MessageIcon.Information,
                                                           MessageResult.OK);
                                }
                            }
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
                            flkCalib5522A.Out.Set.Temperature.SetTermoCouple(CalibrMain.COut.CSet.СTemperature
                                                                                       .TypeTermocouple.K);
                            flkCalib5522A.Out.Set.Temperature.SetValue(currPoint);
                            flkCalib5522A.Out.SetOutput(CalibrMain.COut.State.On);
                            Thread.Sleep(3000);
                            //измеряем
                            var measurePoint = (decimal) appa107N.GetValue();

                            flkCalib5522A.Out.SetOutput(CalibrMain.COut.State.Off);

                            var mantisa =
                                MathStatistics.GetMantissa(RangeResolution
                                                          .MainPhysicalQuantity.GetNoramalizeValueToSi() /
                                                           currPoint.MainPhysicalQuantity.GetNoramalizeValueToSi(),
                                                           true);
                            //округляем измерения
                            MathStatistics.Round(ref measurePoint, mantisa);

                            operation.Getting =
                                new MeasPoint<CelsiumGrad>(measurePoint);
                            operation.Expected = currPoint;
                            //расчет погрешности для конкретной точки предела измерения
                            operation.ErrorCalculation = (inA, inB) =>
                            {
                                var result = BaseTolCoeff * operation.Expected.MainPhysicalQuantity.Value + EdMlRaz *
                                    RangeResolution.MainPhysicalQuantity.Value *
                                    (decimal) (RangeResolution
                                              .MainPhysicalQuantity.Multiplier.GetDoubleValue() /
                                               currPoint.MainPhysicalQuantity.Multiplier.GetDoubleValue()
                                    );

                                MathStatistics.Round(ref result, mantisa);
                                return new MeasPoint<CelsiumGrad>(result, currPoint.MainPhysicalQuantity.Multiplier);
                            };

                            operation.LowerTolerance = operation.Expected - operation.Error;
                            operation.UpperTolerance = operation.Expected + operation.Error;
                            operation.IsGood = () =>
                            {
                                if (operation.Expected == null || operation.Getting == null ||
                                    operation.LowerTolerance == null || operation.UpperTolerance == null) return false;
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

                    operation.CompliteWork = () => Hepls.HelpsCompliteWork(operation, UserItemOperation);
                    DataRow.Add(DataRow.IndexOf(operation) == -1
                                    ? operation
                                    : (BasicOperationVerefication<MeasPoint<CelsiumGrad>>) operation.Clone());
                }
            }

            #endregion
        }

        public class Oper10_1Temperature_Minus200_Minus100_Measure : Oper10TemperatureMeasureBase
        {
            public Oper10_1Temperature_Minus200_Minus100_Measure(Mult107_109N.RangeNominal inRangeNominal,
                IUserItemOperation userItemOperation, string inDirectory) :
                base(userItemOperation, inDirectory)
            {
                OperationRangeCode = Mult107_109N.RangeCode.Range1Manual;
                OperationRangeNominal = inRangeNominal;
                RangeResolution = new MeasPoint<CelsiumGrad>(100, UnitMultiplier.Mili);
                Name = "-200 ⁰C ... -100 ⁰C";

                BaseTolCoeff = (decimal) 0.001;
                EdMlRaz = 60;

                DegC_Point = new[] {new MeasPoint<CelsiumGrad>(-200)};
            }

            #region Methods

            /// <inheritdoc />
            protected override string GetReportTableName()
            {
                return "FillTabBmOper10_1Tem_Minus200_Minus100";
            }

            #endregion
        }

        public class Oper10_1Temperature_Minus100_400_Measure : Oper10TemperatureMeasureBase
        {
            public Oper10_1Temperature_Minus100_400_Measure(Mult107_109N.RangeNominal inRangeNominal,
                IUserItemOperation userItemOperation, string inDirecory) :
                base(userItemOperation, inDirecory)
            {
                OperationRangeCode = Mult107_109N.RangeCode.Range2Manual;
                OperationRangeNominal = inRangeNominal;
                RangeResolution = new MeasPoint<CelsiumGrad>(100, UnitMultiplier.Mili);
                Name = "-100 ⁰C ... 400 ⁰C";

                BaseTolCoeff = (decimal) 0.001;
                EdMlRaz = 30;

                DegC_Point = new[]
                {
                    new MeasPoint<CelsiumGrad>(-100),
                    new MeasPoint<CelsiumGrad>(0),
                    new MeasPoint<CelsiumGrad>(100)
                };
            }

            #region Methods

            /// <inheritdoc />
            protected override string GetReportTableName()
            {
                return "FillTabBmOper10_1Tem_Minus100_400";
            }

            #endregion
        }

        public class Oper10_1Temperature_400_1200_Measure : Oper10TemperatureMeasureBase
        {
            public Oper10_1Temperature_400_1200_Measure(Mult107_109N.RangeNominal inRangeNominal,
                IUserItemOperation userItemOperation, string inDirectory) :
                base(userItemOperation, inDirectory)
            {
                OperationRangeCode = Mult107_109N.RangeCode.Range2Manual;
                OperationRangeNominal = inRangeNominal;
                RangeResolution = new MeasPoint<CelsiumGrad>(1);
                Name = "400 ⁰C ... 1200 ⁰C";

                BaseTolCoeff = (decimal) 0.001;
                EdMlRaz = 3;

                DegC_Point = new[]
                {
                    new MeasPoint<CelsiumGrad>(500),
                    new MeasPoint<CelsiumGrad>(800),
                    new MeasPoint<CelsiumGrad>(1200)
                };
            }

            #region Methods

            /// <inheritdoc />
            protected override string GetReportTableName()
            {
                return "FillTabBmOper10_1Tem_400_1200";
            }

            #endregion
        }

        #endregion TEMP
    }
}