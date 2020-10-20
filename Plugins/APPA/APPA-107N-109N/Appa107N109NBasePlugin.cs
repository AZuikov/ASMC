using AP.Math;
using AP.Utils.Data;
using AP.Utils.Helps;
using ASMC.Core.Model;
using ASMC.Data.Model;
using ASMC.Data.Model.Interface;
using ASMC.Devices.IEEE;
using ASMC.Devices.IEEE.Fluke.Calibrator;
using ASMC.Devices.Port.APPA;
using DevExpress.Mvvm;
using NLog;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using ASMC.Data.Model.PhysicalQuantity;

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
        /// <param name = "CountOfRange"> Общее количество пределов измерения на данном режиме.</param>
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
            IUserItemOperation UserItemOperation) where T: IPhysicalQuantity, new()
        {
            if (!operation.IsGood())
            {
                var answer =
                    UserItemOperation.ServicePack.MessageBox()
                                     .Show($"Текущая точка {operation.Expected.Description} не проходит по допуску:\n" +
                                           $"Минимально допустимое значение {operation.LowerTolerance.Description}\n" +
                                           $"Максимально допустимое значение {operation.UpperTolerance.Description}\n" +
                                           $"Допустимое значение погрешности {operation.Error.Description}\n" +
                                           $"ИЗМЕРЕННОЕ значение {operation.Getting.Description}\n\n" +
                                           $"\nФАКТИЧЕСКАЯ погрешность {operation.Expected- operation.Getting}\n\n" +
                                           "Повторить измерение этой точки?",
                                           "Информация по текущему измерению",
                                           MessageButton.YesNo, MessageIcon.Question,
                                           MessageResult.Yes);

                if (answer == MessageResult.No) return Task.FromResult(true);
            }

            return Task.FromResult(operation.IsGood());
        }

        public static Task<bool> HelpsCompliteWork(BasicOperationVerefication<MeasPoint> operation,
            IUserItemOperation UserItemOperation)
        {
            if (!operation.IsGood())
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

            return Task.FromResult(operation.IsGood());
        }

        #endregion Methods
    }

    public abstract class OpertionFirsVerf : ASMC.Core.Model.Operation
    {
        public OpertionFirsVerf(ServicePack servicePack) : base(servicePack)
        {
            //Необходимые устройства
            ControlDevices = new IDeviceUi[]
                {new Device { Devices = new IDeviceBase[] { new Calib5522A() }, Description = "Многофунциональный калибратор"}};

            Accessories = new[]
            {
                "Интерфейсный кабель для клибратора (GPIB или COM порт)",
                "Кабель banana - banana 2 шт.",
                "Интерфейсный кабель для прибора APPA-107N/APPA-109N USB-COM инфракрасный."
            };

            DocumentName = "APPA_107N_109N";
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

        protected override void InitWork()
        {
            base.InitWork();
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

        #endregion Methods

        public List<IBasicOperation<bool>> DataRow { get; set; }
    }

    public class Oper2Oprobovanie : ParagraphBase<bool>
    {
        public Oper2Oprobovanie(IUserItemOperation userItemOperation) : base(userItemOperation)
        {
            Name = "Опробование";
            DataRow = new List<IBasicOperation<bool>>();
        }

        #region Methods

        /// <inheritdoc />
        protected override string[] GenerateDataColumnTypeObject()
        {
            return new[] { "Результат опробования" };
        }
        /// <inheritdoc />
        protected override string GetReportTableName()
        {
            return "ITBmOprobovanie";
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

        protected override void InitWork()
        {
            base.InitWork();
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

        #endregion Methods

    }

    //////////////////////////////******DCV*******///////////////////////////////

    #region DCV

    public class Oper3DcvMeasureBase : ParagraphBase<MeasPoint<Voltage>>
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        #region Fields

        /// <summary>
        /// множители для пределов.
        /// </summary>
        public decimal BaseMultipliers;

        /// <summary>
        /// Процент от измеряемой точки для расчета погрешности (уже переведе в абсолютные единицы).
        /// </summary>
        public decimal BaseTolCoeff = (decimal)0.0006;

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

        ///// <summary>
        ///// Имя закладки таблички в результирующем протоколе doc (Ms Word).
        ///// </summary>
        //protected string ReportTableName;

        /// <summary>
        /// Это пустая точка, которая содержит только единицы измерения текущего
        /// поверяемого предела и множитель для единицы измерения.
        /// </summary>
        protected MeasPoint<Voltage> thisRangeUnits;

        /// <summary>
        /// Массив поверяемых точек напряжения.
        /// </summary>
        protected MeasPoint<Voltage>[] VoltPoint;

        #endregion Fields

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
        protected Mult107_109N appa107N { get; set; }

        //эталон
        protected Calib5522A flkCalib5522A { get; set; }

        #endregion Property

        public Oper3DcvMeasureBase(IUserItemOperation userItemOperation, string inResourceDir) : base(userItemOperation)
        {

            thisRangeUnits = new MeasPoint<Voltage>();
            Name = "Определение погрешности измерения постоянного напряжения";
            OperMeasureMode = Mult107_109N.MeasureMode.DCV;

            OperationDcRangeCode = Mult107_109N.RangeCode.Range1Manual;
            OperationDcRangeNominal = Mult107_109N.RangeNominal.RangeNone;

            DataRow = new List<IBasicOperation<MeasPoint<Voltage>>>();
            Sheme = ShemeTemplateDefault.TemplateSheme;
            Sheme.AssemblyLocalName = inResourceDir;
        }

        #region Methods

   
        /// <inheritdoc />
        protected override string[] GenerateDataColumnTypeObject()
        {
            return new[] {"Предел измерения",
                "Поверяемая точка", "Измеренное значение", "Минимальное допустимое значение", "Максимальное допустимое значение"  }.Concat(base.GenerateDataColumnTypeObject()).ToArray(); 
        }
        /// <inheritdoc />
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

        protected override void InitWork()
        {
            base.InitWork();
            if (appa107N == null || flkCalib5522A == null) return;


            foreach (var currPoint in VoltPoint)
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

                        while (OperMeasureMode != await Task<Mult107_109N.MeasureMode>.Factory.StartNew(() => appa107N.GetMeasureMode))
                            UserItemOperation.ServicePack.MessageBox()
                                             .Show($"Установите режим измерения: {OperMeasureMode.GetStringValue()} {OperMeasureMode}",
                                                   "Указание оператору", MessageButton.OK,
                                                   MessageIcon.Information,
                                                   MessageResult.OK);

                        while (await Task<Mult107_109N.RangeSwitchMode>.Factory.StartNew(() => appa107N.GetRangeSwitchMode) == Mult107_109N.RangeSwitchMode.Auto)
                            UserItemOperation.ServicePack.MessageBox()
                                             .Show("Установите ручной режим переключения пределов.");

                        while (OperationDcRangeNominal != await Task<Mult107_109N.RangeNominal>.Factory.StartNew(() => appa107N.GetRangeNominal))
                        {
                            int countPushRangeButton;

                            if (thisRangeUnits.MainPhysicalQuantity.Multipliers == UnitMultipliers.Mili)
                            {
                                CountOfRanges = 2;
                                UserItemOperation.ServicePack.MessageBox()
                                                 .Show($"Текущий предел измерения прибора {appa107N.GetRangeNominal.GetStringValue()}\n Необходимо установить предел {OperationDcRangeNominal.GetStringValue()} " +
                                                       $"Нажмите на приборе клавишу Range {countPushRangeButton = 1} раз.",
                                                       "Указание оператору", MessageButton.OK, MessageIcon.Information,
                                                       MessageResult.OK);
                            }
                            else
                            {
                                //работает только для ручного режима переключения пределов
                                CountOfRanges = 4;
                                var curRange = (int)appa107N.GetRangeCode - 127;
                                var targetRange = (int)OperationDcRangeCode - 127;
                                countPushRangeButton = Hepls.CountOfPushButton(CountOfRanges, curRange, targetRange);

                                UserItemOperation.ServicePack.MessageBox()
                                                 .Show($"Текущий предел измерения прибора {appa107N.GetRangeNominal.GetStringValue()}\n Необходимо установить предел {OperationDcRangeNominal.GetStringValue()} " +
                                                       $"Нажмите на приборе клавишу Range {countPushRangeButton} раз.",
                                                       "Указание оператору", MessageButton.OK, MessageIcon.Information,
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
                operation.BodyWork = () =>
                {
                    try
                    {
                        flkCalib5522A.Out.Set.Voltage.Dc.SetValue(currPoint.MainPhysicalQuantity.Value, currPoint.MainPhysicalQuantity.Multipliers);
                        flkCalib5522A.Out.ClearMemoryRegister();
                        flkCalib5522A.Out.SetOutput(CalibrMain.COut.State.On);
                        Thread.Sleep(2000);
                        //измеряем
                        var measurePoint = (decimal)appa107N.GetValue();
                        flkCalib5522A.Out.SetOutput(CalibrMain.COut.State.Off);

                        var mantisa =
                            MathStatistics.GetMantissa((decimal)(RangeResolution
                                                                 .MainPhysicalQuantity.Multipliers
                                                                 .GetDoubleValue() /
                                                                  currPoint.MainPhysicalQuantity.Multipliers.GetDoubleValue()));
                        //округляем измерения
                        MathStatistics.Round(ref measurePoint, mantisa);

                        operation.Getting = new MeasPoint<Voltage>(measurePoint, thisRangeUnits.MainPhysicalQuantity.Multipliers);
                            //new AcVariablePoint(measurePoint, MeasureUnits.V, thisRangeUnits.Multipliers);
                        operation.Expected = currPoint;
                        //расчет погрешности для конкретной точки предела измерения
                        operation.ErrorCalculation = (inA, inB) =>
                        {
                            var result =
                                BaseTolCoeff * Math.Abs(operation.Expected.MainPhysicalQuantity.Value) +
                                EdMlRaz *
                                RangeResolution.MainPhysicalQuantity.Value *
                                (decimal)(RangeResolution
                                          .MainPhysicalQuantity.Multipliers.GetDoubleValue() /
                                           currPoint.MainPhysicalQuantity.Multipliers.GetDoubleValue());
                            var mantisa =
                                MathStatistics.GetMantissa((decimal)(RangeResolution
                                                                     .MainPhysicalQuantity.Multipliers
                                                                     .GetDoubleValue() /
                                                                      currPoint.MainPhysicalQuantity
                                                                               .Multipliers
                                                                               .GetDoubleValue()));
                            MathStatistics.Round(ref result, mantisa);
                            return new MeasPoint<Voltage>(result, thisRangeUnits.MainPhysicalQuantity.Multipliers);
                        };

                        operation.LowerTolerance = operation.Expected - operation.Error;
                       operation.UpperTolerance = operation.Expected + operation.Error;
                        
                        operation.IsGood = () =>
                        {
                            if (operation.Getting == null || operation.Expected == null ||
                                operation.UpperTolerance == null || operation.LowerTolerance == null) return false;
                            return (operation.Getting.MainPhysicalQuantity.Value <
                                    operation.UpperTolerance.MainPhysicalQuantity.Value) &
                                   (operation.Getting.MainPhysicalQuantity.Value >
                                    operation.LowerTolerance.MainPhysicalQuantity.Value);
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
                                : (BasicOperationVerefication<MeasPoint<Voltage>>)operation.Clone());
            }
        }

        #endregion Methods

        //public override async Task StartWork(CancellationToken token)
        //{
        //    await base.StartWork(token);
        //    appa107N?.Dispose();
        //}

       
    }

    public class Oper3_1DC_2V_Measure : Oper3DcvMeasureBase
    {
        public Oper3_1DC_2V_Measure(Mult107_109N.RangeNominal inRangeNominal, IUserItemOperation userItemOperation, string inResourceDir) :
            base(userItemOperation, inResourceDir)
        {
            OperationDcRangeCode = Mult107_109N.RangeCode.Range1Manual;
            OperationDcRangeNominal = inRangeNominal;

            Name = OperationDcRangeNominal.GetStringValue();

            BaseTolCoeff = (decimal)0.0006;
            EdMlRaz = 10;
            RangeResolution = new MeasPoint<Voltage>(100,  UnitMultipliers.Micro);

            BaseMultipliers = 100;
            VoltPoint = new MeasPoint<Voltage>[6];
            VoltPoint[0] = new MeasPoint<Voltage>((decimal)0.4, thisRangeUnits.MainPhysicalQuantity.Multipliers);
            VoltPoint[1] = new MeasPoint<Voltage>((decimal)0.8, thisRangeUnits.MainPhysicalQuantity.Multipliers);
            VoltPoint[2] = new MeasPoint<Voltage>((decimal)1.2, thisRangeUnits.MainPhysicalQuantity.Multipliers);
            VoltPoint[3] = new MeasPoint<Voltage>((decimal)1.6, thisRangeUnits.MainPhysicalQuantity.Multipliers);
            VoltPoint[4] = new MeasPoint<Voltage>((decimal)1.8, thisRangeUnits.MainPhysicalQuantity.Multipliers);
            VoltPoint[5] = new MeasPoint<Voltage>((decimal)-1.8, thisRangeUnits.MainPhysicalQuantity.Multipliers);
        }

        /// <inheritdoc />
        protected override string GetReportTableName()
        {
            return "FillTabBmOper3_1DC_2V_Measure";
        }
    }

    public class Oper3_1DC_20V_Measure : Oper3DcvMeasureBase
    {
        public Oper3_1DC_20V_Measure(Mult107_109N.RangeNominal inRangeNominal, IUserItemOperation userItemOperation, string inResourceDir) :
            base(userItemOperation, inResourceDir)
        {
            OperationDcRangeCode = Mult107_109N.RangeCode.Range2Manual;
            OperationDcRangeNominal = inRangeNominal;
            RangeResolution = new MeasPoint<Voltage>(1,  UnitMultipliers.Mili);
            Name = OperationDcRangeNominal.GetStringValue();

            BaseMultipliers = 1000;
            VoltPoint = new MeasPoint<Voltage>[6];
            VoltPoint[0] = new MeasPoint<Voltage>(4, thisRangeUnits.MainPhysicalQuantity.Multipliers);
            VoltPoint[1] = new MeasPoint<Voltage>(8, thisRangeUnits.MainPhysicalQuantity.Multipliers);
            VoltPoint[2] = new MeasPoint<Voltage>(12,thisRangeUnits.MainPhysicalQuantity.Multipliers);
            VoltPoint[3] = new MeasPoint<Voltage>(16,thisRangeUnits.MainPhysicalQuantity.Multipliers);
            VoltPoint[4] = new MeasPoint<Voltage>(18,thisRangeUnits.MainPhysicalQuantity.Multipliers);
            VoltPoint[5] = new MeasPoint<Voltage>(-18, thisRangeUnits.MainPhysicalQuantity.Multipliers);
        }

        /// <inheritdoc />
        protected override string GetReportTableName()
        {
            return "FillTabBmOper3_1DC_20V_Measure";
            ;
        }
    }

    public class Oper3_1DC_200V_Measure : Oper3DcvMeasureBase
    {
        public Oper3_1DC_200V_Measure(Mult107_109N.RangeNominal inRangeNominal, IUserItemOperation userItemOperation, string inResourceDir) :
            base(userItemOperation, inResourceDir)
        {
            OperationDcRangeCode = Mult107_109N.RangeCode.Range3Manual;
            OperationDcRangeNominal = inRangeNominal;

            Name = OperationDcRangeNominal.GetStringValue();

            BaseTolCoeff = (decimal)0.0006;
            EdMlRaz = 10;
            RangeResolution = new MeasPoint<Voltage>(10,  UnitMultipliers.Mili);

            BaseMultipliers = 10000;
            VoltPoint = new    MeasPoint<Voltage>[6];
            VoltPoint[0] = new MeasPoint<Voltage>(40,  thisRangeUnits.MainPhysicalQuantity.Multipliers);
            VoltPoint[1] = new MeasPoint<Voltage>(80,  thisRangeUnits.MainPhysicalQuantity.Multipliers);
            VoltPoint[2] = new MeasPoint<Voltage>(120, thisRangeUnits.MainPhysicalQuantity.Multipliers);
            VoltPoint[3] = new MeasPoint<Voltage>(160, thisRangeUnits.MainPhysicalQuantity.Multipliers);
            VoltPoint[4] = new MeasPoint<Voltage>(180, thisRangeUnits.MainPhysicalQuantity.Multipliers);
            VoltPoint[5] = new MeasPoint<Voltage>(-180, thisRangeUnits.MainPhysicalQuantity.Multipliers);
        }

        /// <inheritdoc />
        protected override string GetReportTableName()
        {
           return "FillTabBmOper3_1DC_200V_Measure";
        }
    }

    public class Oper3_1DC_1000V_Measure : Oper3DcvMeasureBase
    {
        public Oper3_1DC_1000V_Measure(Mult107_109N.RangeNominal inRangeNominal, IUserItemOperation userItemOperation, string inResourceDir) :
            base(userItemOperation, inResourceDir)
        {
            OperationDcRangeCode = Mult107_109N.RangeCode.Range4Manual;
            OperationDcRangeNominal = inRangeNominal;

            Name = OperationDcRangeNominal.GetStringValue();

            BaseTolCoeff = (decimal)0.0006;
            EdMlRaz = 10;
            RangeResolution = new MeasPoint<Voltage>(100,  UnitMultipliers.Mili);

            BaseMultipliers = 1;
            VoltPoint = new    MeasPoint<Voltage>[6];      
            VoltPoint[0] = new MeasPoint<Voltage>(100,   thisRangeUnits.MainPhysicalQuantity.Multipliers);
            VoltPoint[1] = new MeasPoint<Voltage>(200,   thisRangeUnits.MainPhysicalQuantity.Multipliers);
            VoltPoint[2] = new MeasPoint<Voltage>(400,   thisRangeUnits.MainPhysicalQuantity.Multipliers);
            VoltPoint[3] = new MeasPoint<Voltage>(700,   thisRangeUnits.MainPhysicalQuantity.Multipliers);
            VoltPoint[4] = new MeasPoint<Voltage>(900,   thisRangeUnits.MainPhysicalQuantity.Multipliers);
            VoltPoint[5] = new MeasPoint<Voltage>(-900,  thisRangeUnits.MainPhysicalQuantity.Multipliers);
        }

        /// <inheritdoc />
        protected override string GetReportTableName()
        {
            return "FillTabBmOper3_1DC_1000V_Measure";
        }
    }

    public class Oper3_1DC_20mV_Measure : Oper3DcvMeasureBase
    {
        public Oper3_1DC_20mV_Measure(Mult107_109N.RangeNominal inRangeNominal, IUserItemOperation userItemOperation, string inResourceDir) :
            base(userItemOperation, inResourceDir)
        {
            thisRangeUnits = new MeasPoint<Voltage>(0, UnitMultipliers.Mili);
            OperationDcRangeCode = Mult107_109N.RangeCode.Range1Manual;
            OperationDcRangeNominal = inRangeNominal;
            RangeResolution = new MeasPoint<Voltage>(1,  UnitMultipliers.Micro);
            Name = OperationDcRangeNominal.GetStringValue();

            BaseTolCoeff = (decimal)0.0006;
            EdMlRaz = 60;
            RangeResolution = new MeasPoint<Voltage>(1,  UnitMultipliers.Micro);

            BaseMultipliers = 1;
            VoltPoint = new MeasPoint<Voltage>[6];
            VoltPoint[0] = new MeasPoint<Voltage>(4,   thisRangeUnits.MainPhysicalQuantity.Multipliers);
            VoltPoint[1] = new MeasPoint<Voltage>(8,   thisRangeUnits.MainPhysicalQuantity.Multipliers);
            VoltPoint[2] = new MeasPoint<Voltage>(12,  thisRangeUnits.MainPhysicalQuantity.Multipliers);
            VoltPoint[3] = new MeasPoint<Voltage>(16,  thisRangeUnits.MainPhysicalQuantity.Multipliers);
            VoltPoint[4] = new MeasPoint<Voltage>(18,  thisRangeUnits.MainPhysicalQuantity.Multipliers);
            VoltPoint[5] = new MeasPoint<Voltage>(-18, thisRangeUnits.MainPhysicalQuantity.Multipliers);
        }

        /// <inheritdoc />
        protected override string GetReportTableName()
        {
            return "FillTabBmOper3_1DC_20mV_Measure";
        }
    }

    public class Oper3_1DC_200mV_Measure : Oper3DcvMeasureBase
    {
        public Oper3_1DC_200mV_Measure(Mult107_109N.RangeNominal inRangeNominal, IUserItemOperation userItemOperation, string inResourceDir) :
            base(userItemOperation, inResourceDir)
        {
            thisRangeUnits = new MeasPoint<Voltage>(0, UnitMultipliers.Mili);
            OperationDcRangeCode = Mult107_109N.RangeCode.Range1Manual;
            OperationDcRangeNominal = inRangeNominal;
            RangeResolution = new MeasPoint<Voltage>(10, UnitMultipliers.Micro);
            Name = OperationDcRangeNominal.GetStringValue();

            BaseTolCoeff = (decimal)0.0006;
            EdMlRaz = 20;
            RangeResolution = new MeasPoint<Voltage>(10,  UnitMultipliers.Micro);

            BaseMultipliers = 10;
            VoltPoint = new MeasPoint<Voltage>[6];
            VoltPoint[0] = new MeasPoint<Voltage>(40,  thisRangeUnits.MainPhysicalQuantity.Multipliers);
            VoltPoint[1] = new MeasPoint<Voltage>(80,  thisRangeUnits.MainPhysicalQuantity.Multipliers);
            VoltPoint[2] = new MeasPoint<Voltage>(120, thisRangeUnits.MainPhysicalQuantity.Multipliers);
            VoltPoint[3] = new MeasPoint<Voltage>(160, thisRangeUnits.MainPhysicalQuantity.Multipliers);
            VoltPoint[4] = new MeasPoint<Voltage>(180, thisRangeUnits.MainPhysicalQuantity.Multipliers);
            VoltPoint[5] = new MeasPoint<Voltage>(-180, thisRangeUnits.MainPhysicalQuantity.Multipliers);
        }

        /// <inheritdoc />
        protected override string GetReportTableName()
        {
            return "FillTabBmOper3_1DC_200mV_Measure";
        }
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

    public class Oper4AcvMeasureBase : ParagraphBase<MeasPoint<Voltage>>
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
        protected MeasPoint<Voltage> thisRangeUnits;

        /// <summary>
        /// Множитель для поверяемых точек. (Если точки можно посчитать простым умножением).
        /// </summary>
        protected decimal VoltMultipliers;

        /// <summary>
        /// Итоговый массив поверяемых точек. У каждого номинала напряжения вложены номиналы частот для текущей точки.
        /// </summary>
        public List<MeasPoint<Voltage>> VoltPoint = new List<MeasPoint<Voltage>>();

        #endregion Fields

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

        #endregion Property

        public Oper4AcvMeasureBase(IUserItemOperation userItemOperation, string inResourceDir) : base(userItemOperation)
        {
            thisRangeUnits = new MeasPoint<Voltage>(0, UnitMultipliers.None);
            Name = "Определение погрешности измерения переменного напряжения";
            OperMeasureMode = Mult107_109N.MeasureMode.ACV;

            OperationAcRangeCode = Mult107_109N.RangeCode.Range1Manual;
            OperationAcRangeNominal = Mult107_109N.RangeNominal.RangeNone;
            DataRow = new List<IBasicOperation<MeasPoint<Voltage>>>();
            Sheme = ShemeTemplateDefault.TemplateSheme;
            Sheme.AssemblyLocalName = inResourceDir;
        }

        #region Methods

      
        /// <inheritdoc />
        protected override string[] GenerateDataColumnTypeObject()
        {
            return new[] {"Предел измерения",
                "Поверяемая точка", "Измеренное значение", "Минимальное допустимое значение", "Максимальное допустимое значение"}.Concat(base.GenerateDataColumnTypeObject()).ToArray(); ;
        }
        /// <inheritdoc />
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
                var dds = row as BasicOperationVerefication<MeasPoint<Voltage>>;
                // ReSharper disable once PossibleNullReferenceException
                if (dds == null) continue;
                dataRow[0] = OperationAcRangeNominal.GetStringValue();
                dataRow[1] = dds.Expected?.MainPhysicalQuantity.ToString();
                //Нужно выводить в таблицу напряжение и частоты!!!
                dataRow[2] = dds.Expected?.AdditionalPhysicalQuantity.FirstOrDefault(q => q.GetType() is Frequency).ToString();
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

        protected override void InitWork()
        {
            base.InitWork();
            if (flkCalib5522A == null || appa107N == null) return;

            foreach (var Point in VoltPoint)
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
                            while (OperMeasureMode != await Task<Mult107_109N.MeasureMode>.Factory.StartNew(() => appa107N.GetMeasureMode))
                                UserItemOperation.ServicePack.MessageBox()
                                                 .Show($"Установите режим измерения: {OperMeasureMode.GetStringValue()} {OperMeasureMode}",
                                                       "Указание оператору", MessageButton.OK,
                                                       MessageIcon.Information,
                                                       MessageResult.OK);

                            while (await Task<Mult107_109N.RangeSwitchMode>.Factory.StartNew(() => appa107N.GetRangeSwitchMode) == Mult107_109N.RangeSwitchMode.Auto)
                                UserItemOperation.ServicePack.MessageBox()
                                                 .Show("Установите ручной режим переключения пределов.");

                            while (OperationAcRangeNominal != await Task<Mult107_109N.RangeNominal>.Factory.StartNew(() => appa107N.GetRangeNominal))
                            {
                                int countPushRangeButton;

                                if (thisRangeUnits.MainPhysicalQuantity.Multipliers == UnitMultipliers.Mili)
                                {
                                    UserItemOperation.ServicePack.MessageBox()
                                                     .Show($"Текущий предел измерения прибора {appa107N.GetRangeNominal.GetStringValue()}\n Необходимо установить предел {OperationAcRangeNominal.GetStringValue()} " +
                                                           $"Нажмите на приборе клавишу Range {countPushRangeButton = 1} раз.",
                                                           "Указание оператору", MessageButton.OK, MessageIcon.Information,
                                                           MessageResult.OK);
                                }
                                else
                                {
                                //работает только для ручного режима переключения пределов
                                CountOfRanges = 4;
                                    var curRange = (int)appa107N.GetRangeCode - 127;
                                    var targetRange = (int)OperationAcRangeCode - 127;
                                    countPushRangeButton = Hepls.CountOfPushButton(CountOfRanges, curRange, targetRange);

                                    UserItemOperation.ServicePack.MessageBox()
                                                     .Show($"Текущий предел измерения прибора {appa107N.GetRangeNominal.GetStringValue()}\n Необходимо установить предел {OperationAcRangeNominal.GetStringValue()} " +
                                                           $"Нажмите на приборе клавишу Range {countPushRangeButton} раз.",
                                                           "Указание оператору", MessageButton.OK, MessageIcon.Information,
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
                    operation.BodyWork = () =>
                    {
                        try
                        {
                        //вычисляе на сколько знаков округлять
                        var mantisa =
                                MathStatistics.GetMantissa((decimal)(RangeResolution
                                                                     .MainPhysicalQuantity.Multipliers
                                                                     .GetDoubleValue() /
                                                                      Point.MainPhysicalQuantity
                                                                              .Multipliers
                                                                              .GetDoubleValue()));

                            operation.Expected = new MeasPoint<Voltage>(Point.MainPhysicalQuantity.Value, thisRangeUnits.MainPhysicalQuantity.Multipliers, new Frequency
                            {
                                Value = Point.AdditionalPhysicalQuantity[0].Value,
                                Multipliers = Point.AdditionalPhysicalQuantity[0].Multipliers

                            });
                                
                        //расчет погрешности для конкретной точки предела измерения
                        ConstructTooleranceFormula(new MeasPoint<Frequency>(Point.AdditionalPhysicalQuantity[0].Value, Point.AdditionalPhysicalQuantity[0].Multipliers)); // функция подбирает коэффициенты для формулы погрешности
                        operation.ErrorCalculation = (inA, inB) =>
                            {
                                var result = BaseTolCoeff * Math.Abs(operation.Expected.MainPhysicalQuantity.Value) +
                                             EdMlRaz *
                                             RangeResolution.MainPhysicalQuantity.Value *
                                             (decimal)(RangeResolution
                                                       .MainPhysicalQuantity.Multipliers.GetDoubleValue() /
                                                        Point.MainPhysicalQuantity.Multipliers
                                                                .GetDoubleValue()
                                             );

                                MathStatistics.Round(ref result, mantisa);
                                return new MeasPoint<Voltage>(result,  thisRangeUnits.MainPhysicalQuantity.Multipliers);
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

                            if (freqPoint.IsFake && Point.fakePointFlag)
                            {
                                Logger.Info($"фальшивая точка {Point} {freqPoint.Description}");
                                measurePoint =
                                    (decimal)
                                    MathStatistics
                                       .RandomToRange((double)operation.LowerTolerance.MainPhysicalQuantity.Value,
                                                      (double)operation.UpperTolerance.MainPhysicalQuantity.Value);
                            }
                            else
                            {
                                flkCalib5522A.Out.Set.Voltage.Ac.SetValue(Point.MainPhysicalQuantity.Value,
                                                                          freqPoint.Value,
                                                                          Point.MainPhysicalQuantity
                                                                                  .Multipliers,
                                                                          freqPoint.Multipliers);
                                flkCalib5522A.Out.ClearMemoryRegister();
                                flkCalib5522A.Out.SetOutput(CalibrMain.COut.State.On);
                                Thread.Sleep(2000);
                            //измеряем
                            measurePoint = (decimal)appa107N.GetValue();
                                flkCalib5522A.Out.SetOutput(CalibrMain.COut.State.Off);
                            }

                        //округляем измерения
                        MathStatistics.Round(ref measurePoint, mantisa);

                            operation.Getting =
                                new MeasPoint<Voltage>(measurePoint,  thisRangeUnits.MainPhysicalQuantity.Multipliers);
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
                                    : (BasicOperationVerefication<MeasPoint<Voltage>>)operation.Clone());
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
                inFreq.MainPhysicalQuantity.Multipliers == UnitMultipliers.None)
            {
                EdMlRaz = 80;
                if (inFreq.MainPhysicalQuantity.Value >= 40 && inFreq.MainPhysicalQuantity.Value <= 100) BaseTolCoeff = (decimal)0.007;
                if (inFreq.MainPhysicalQuantity.Value > 100 && inFreq.MainPhysicalQuantity.Value <= 1000) BaseTolCoeff = (decimal)0.01;
                return;
            }

            if (OperationAcRangeNominal == Mult107_109N.RangeNominal.Range2V ||
                OperationAcRangeNominal == Mult107_109N.RangeNominal.Range20V ||
                OperationAcRangeNominal == Mult107_109N.RangeNominal.Range200V)
            {
                if (inFreq.MainPhysicalQuantity.Multipliers == UnitMultipliers.None)
                {
                    EdMlRaz = 50;
                    if (inFreq.MainPhysicalQuantity.Value >= 40 && inFreq.MainPhysicalQuantity.Value <= 100) BaseTolCoeff = (decimal)0.007;
                    if (inFreq.MainPhysicalQuantity.Value > 100 && inFreq.MainPhysicalQuantity.Value <= 1000) BaseTolCoeff = (decimal)0.01;
                    return;
                }

                if (inFreq.MainPhysicalQuantity.Multipliers == UnitMultipliers.Kilo)
                {
                    if (inFreq.MainPhysicalQuantity.Value >= 1 && inFreq.MainPhysicalQuantity.Value <= 10)
                    {
                        BaseTolCoeff = (decimal)0.02;
                        EdMlRaz = 60;
                        return;
                    }

                    if (inFreq.MainPhysicalQuantity.Value > 10 && inFreq.MainPhysicalQuantity.Value <= 20)
                    {
                        BaseTolCoeff = (decimal)0.03;
                        EdMlRaz = 70;
                        return;
                    }

                    if (inFreq.MainPhysicalQuantity.Value > 20 && inFreq.MainPhysicalQuantity.Value <= 50)
                    {
                        BaseTolCoeff = (decimal)0.05;
                        EdMlRaz = 80;
                        return;
                    }

                    if (inFreq.MainPhysicalQuantity.Value > 50 && inFreq.MainPhysicalQuantity.Value <= 100)
                    {
                        BaseTolCoeff = (decimal)0.1;
                        EdMlRaz = 100;
                        return;
                    }
                }
            }

            if (OperationAcRangeNominal == Mult107_109N.RangeNominal.Range750V)
            {
                EdMlRaz = 50;
                if (inFreq.MainPhysicalQuantity.Value >= 40 && inFreq.MainPhysicalQuantity.Value <= 100) BaseTolCoeff = (decimal)0.007;
                if (inFreq.MainPhysicalQuantity.Value > 100 && inFreq.MainPhysicalQuantity.Value <= 1000) BaseTolCoeff = (decimal)0.01;
            }
        }

        #endregion Methods

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
        public Ope4_1_AcV_20mV_Measure(Mult107_109N.RangeNominal inRangeNominal, IUserItemOperation userItemOperation, string inResourceDir) :
            base(userItemOperation, inResourceDir)
        {
            thisRangeUnits = new MeasPoint<Voltage>( 0,UnitMultipliers.Mili);
            OperMeasureMode = Mult107_109N.MeasureMode.ACmV;
            OperationAcRangeCode = Mult107_109N.RangeCode.Range1Manual;
            OperationAcRangeNominal = inRangeNominal;
            Name = OperationAcRangeNominal.GetStringValue();
            Sheme = ShemeTemplateDefault.TemplateSheme;

            VoltMultipliers = 1;

            RangeResolution = new MeasPoint<Voltage>(1,  UnitMultipliers.Micro);

            

            
            VoltPoint.Add(new MeasPoint<Voltage>(4 * VoltMultipliers, thisRangeUnits.MainPhysicalQuantity.Multipliers, new Frequency{Value = 40}  ));
            VoltPoint.Add(new MeasPoint<Voltage>(4 * VoltMultipliers, thisRangeUnits.MainPhysicalQuantity.Multipliers, new Frequency{Value = 1000}));
            VoltPoint.Add(new MeasPoint<Voltage>(10 * VoltMultipliers, thisRangeUnits.MainPhysicalQuantity.Multipliers, new Frequency{Value = 40}  ));
            VoltPoint.Add(new MeasPoint<Voltage>(10 * VoltMultipliers, thisRangeUnits.MainPhysicalQuantity.Multipliers, new Frequency { Value = 1000 }));
            VoltPoint.Add(new MeasPoint<Voltage>(18 * VoltMultipliers, thisRangeUnits.MainPhysicalQuantity.Multipliers, new Frequency{Value = 40}  ));
            VoltPoint.Add(new MeasPoint<Voltage>(18 * VoltMultipliers, thisRangeUnits.MainPhysicalQuantity.Multipliers, new Frequency { Value = 1000 }));

        }

        /// <inheritdoc />
        protected override string GetReportTableName()
        {
            return "FillTabBmOpe4_1_AcV_20mV_Measure";
        }
    }

    public class Ope4_1_AcV_200mV_Measure : Oper4AcvMeasureBase
    {
        public Ope4_1_AcV_200mV_Measure(Mult107_109N.RangeNominal inRangeNominal, IUserItemOperation userItemOperation, string inResourceDir)
            : base(userItemOperation, inResourceDir)
        {
            thisRangeUnits = new MeasPoint<Voltage>(0, UnitMultipliers.Mili);
           
            OperMeasureMode = Mult107_109N.MeasureMode.ACmV;
            OperationAcRangeCode = Mult107_109N.RangeCode.Range2Manual;
            OperationAcRangeNominal = inRangeNominal;
            Name = OperationAcRangeNominal.GetStringValue();
            Sheme = ShemeTemplateDefault.TemplateSheme;

            VoltMultipliers = 10;

            RangeResolution = new MeasPoint<Voltage>(10, UnitMultipliers.Micro);

            //HerzVPoint = new MeasPoint<Frequency>[2];
            //HerzVPoint[0] = new MeasPoint(MeasureUnits.Herz, UnitMultipliers.None, 40);
            //HerzVPoint[1] = new MeasPoint(MeasureUnits.Herz, UnitMultipliers.None, 1000);

            
            VoltPoint.Add( new MeasPoint<Voltage>(4 * VoltMultipliers, thisRangeUnits.MainPhysicalQuantity.Multipliers, new Frequency{Value = 40}  ));
            VoltPoint.Add( new MeasPoint<Voltage>(4 * VoltMultipliers, thisRangeUnits.MainPhysicalQuantity.Multipliers, new Frequency { Value = 1000 }));
            VoltPoint.Add( new MeasPoint<Voltage>(10 * VoltMultipliers, thisRangeUnits.MainPhysicalQuantity.Multipliers,new Frequency{Value = 40}  ));
            VoltPoint.Add( new MeasPoint<Voltage>(10 * VoltMultipliers, thisRangeUnits.MainPhysicalQuantity.Multipliers, new Frequency { Value = 1000 }));
            VoltPoint.Add( new MeasPoint<Voltage>(18 * VoltMultipliers, thisRangeUnits.MainPhysicalQuantity.Multipliers,new Frequency{Value = 40}  ));
            VoltPoint.Add( new MeasPoint<Voltage>(18 * VoltMultipliers, thisRangeUnits.MainPhysicalQuantity.Multipliers, new Frequency { Value = 1000 }));
        }

        /// <inheritdoc />
        protected override string GetReportTableName()
        {
           return "FillTabBmOpe4_1_AcV_200mV_Measure";
        }
    }

    public class Ope4_1_AcV_2V_Measure : Oper4AcvMeasureBase
    {
        public Ope4_1_AcV_2V_Measure(Mult107_109N.RangeNominal inRangeNominal, IUserItemOperation userItemOperation, string inResourceDir) :
            base(userItemOperation, inResourceDir)
        {
            

            OperMeasureMode = Mult107_109N.MeasureMode.ACV;
            OperationAcRangeCode = Mult107_109N.RangeCode.Range1Manual;
            OperationAcRangeNominal = inRangeNominal;
            Name = OperationAcRangeNominal.GetStringValue();
            Sheme = ShemeTemplateDefault.TemplateSheme;

            VoltMultipliers = 1;

            RangeResolution = new MeasPoint<Voltage>((decimal)0.1, UnitMultipliers.Mili);

            
            //конкретно для первой точки 0.2 нужны не все частоты, поэтому вырежем только необходимые
            VoltPoint.Add(new MeasPoint<Voltage>((decimal)0.2,  thisRangeUnits.MainPhysicalQuantity.Multipliers, new Frequency{Value = 40 * VoltMultipliers}));
            VoltPoint.Add(new MeasPoint<Voltage>((decimal)0.2,  thisRangeUnits.MainPhysicalQuantity.Multipliers, new Frequency{Value = 1000 * VoltMultipliers }));
            VoltPoint.Add(new MeasPoint<Voltage>((decimal)0.2,  thisRangeUnits.MainPhysicalQuantity.Multipliers, new Frequency{Value = 10 * VoltMultipliers, Multipliers = UnitMultipliers.Kilo}));
            VoltPoint.Add(new MeasPoint<Voltage>((decimal)0.2,  thisRangeUnits.MainPhysicalQuantity.Multipliers, new Frequency{Value = 20 * VoltMultipliers, Multipliers = UnitMultipliers.Kilo }));

            VoltPoint.Add(new MeasPoint<Voltage>(1,  thisRangeUnits.MainPhysicalQuantity.Multipliers, new Frequency{Value = 40 * VoltMultipliers}));
            VoltPoint.Add(new MeasPoint<Voltage>(1,  thisRangeUnits.MainPhysicalQuantity.Multipliers, new Frequency{Value = 1000 * VoltMultipliers }));
            VoltPoint.Add(new MeasPoint<Voltage>(1,  thisRangeUnits.MainPhysicalQuantity.Multipliers, new Frequency{Value = 10 * VoltMultipliers, Multipliers = UnitMultipliers.Kilo}));
            VoltPoint.Add(new MeasPoint<Voltage>(1,  thisRangeUnits.MainPhysicalQuantity.Multipliers, new Frequency { Value = 20 * VoltMultipliers, Multipliers = UnitMultipliers.Kilo }));
            VoltPoint.Add(new MeasPoint<Voltage>(1,  thisRangeUnits.MainPhysicalQuantity.Multipliers, new Frequency{Value = 50 * VoltMultipliers , Multipliers =  UnitMultipliers.Kilo}));
            VoltPoint.Add(new MeasPoint<Voltage>(1,  thisRangeUnits.MainPhysicalQuantity.Multipliers, new Frequency { Value = 100 * VoltMultipliers, Multipliers = UnitMultipliers.Kilo }));

            VoltPoint.Add(new MeasPoint<Voltage>((decimal)1.8,  thisRangeUnits.MainPhysicalQuantity.Multipliers, new Frequency{Value = 40 * VoltMultipliers}));
            VoltPoint.Add(new MeasPoint<Voltage>((decimal)1.8,  thisRangeUnits.MainPhysicalQuantity.Multipliers, new Frequency{Value = 1000 * VoltMultipliers }));
            VoltPoint.Add(new MeasPoint<Voltage>((decimal)1.8,  thisRangeUnits.MainPhysicalQuantity.Multipliers, new Frequency{Value = 10 * VoltMultipliers, Multipliers = UnitMultipliers.Kilo}));
            VoltPoint.Add(new MeasPoint<Voltage>((decimal)1.8,  thisRangeUnits.MainPhysicalQuantity.Multipliers, new Frequency { Value = 20 * VoltMultipliers, Multipliers = UnitMultipliers.Kilo }));
            VoltPoint.Add(new MeasPoint<Voltage>((decimal)1.8,  thisRangeUnits.MainPhysicalQuantity.Multipliers, new Frequency{Value = 50 * VoltMultipliers , Multipliers =  UnitMultipliers.Kilo}));
            VoltPoint.Add(new MeasPoint<Voltage>((decimal)1.8,  thisRangeUnits.MainPhysicalQuantity.Multipliers, new Frequency { Value = 100 * VoltMultipliers, Multipliers = UnitMultipliers.Kilo }));
        }

        /// <inheritdoc />
        protected override string GetReportTableName()
        {
           return "FillTabBmOpe4_1_AcV_2V_Measure";
        }
    }

    public class Ope4_1_AcV_20V_Measure : Oper4AcvMeasureBase
    {
        public Ope4_1_AcV_20V_Measure(Mult107_109N.RangeNominal inRangeNominal, IUserItemOperation userItemOperation, string inResourceDir) :
            base(userItemOperation, inResourceDir)
        {
            OperMeasureMode = Mult107_109N.MeasureMode.ACV;
            OperationAcRangeCode = Mult107_109N.RangeCode.Range2Manual;
            OperationAcRangeNominal = inRangeNominal;
            Name = OperationAcRangeNominal.GetStringValue();
            Sheme = ShemeTemplateDefault.TemplateSheme;

            VoltMultipliers = 10;

            RangeResolution = new MeasPoint<Voltage>(1,  UnitMultipliers.Mili);

            
            
            //конкретно для первой точки 2 нужны не все частоты, поэтому вырежем только необходимые
            
            VoltPoint.Add(new MeasPoint<Voltage>((decimal)0.2 * VoltMultipliers,  thisRangeUnits.MainPhysicalQuantity.Multipliers, new Frequency{Value = 40}));
            VoltPoint.Add(new MeasPoint<Voltage>((decimal)0.2 * VoltMultipliers,  thisRangeUnits.MainPhysicalQuantity.Multipliers, new Frequency{Value = 1000}));
            VoltPoint.Add(new MeasPoint<Voltage>((decimal)0.2 * VoltMultipliers,  thisRangeUnits.MainPhysicalQuantity.Multipliers, new Frequency{Value = 10, Multipliers = UnitMultipliers.Kilo}));
            VoltPoint.Add(new MeasPoint<Voltage>((decimal)0.2 * VoltMultipliers,  thisRangeUnits.MainPhysicalQuantity.Multipliers, new Frequency {Value = 20, Multipliers = UnitMultipliers.Kilo}));

            VoltPoint.Add(new MeasPoint<Voltage>(1 * VoltMultipliers,  thisRangeUnits.MainPhysicalQuantity.Multipliers,  new Frequency{Value = 40}));
            VoltPoint.Add(new MeasPoint<Voltage>(1 * VoltMultipliers,  thisRangeUnits.MainPhysicalQuantity.Multipliers,  new Frequency{Value = 1000}));
            VoltPoint.Add(new MeasPoint<Voltage>(1 * VoltMultipliers,  thisRangeUnits.MainPhysicalQuantity.Multipliers,  new Frequency{Value = 10, Multipliers = UnitMultipliers.Kilo }));
            VoltPoint.Add(new MeasPoint<Voltage>(1 * VoltMultipliers,  thisRangeUnits.MainPhysicalQuantity.Multipliers,  new Frequency{Value = 20, Multipliers = UnitMultipliers.Kilo }));
            VoltPoint.Add(new MeasPoint<Voltage>(1 * VoltMultipliers,  thisRangeUnits.MainPhysicalQuantity.Multipliers,  new Frequency{Value = 50, Multipliers = UnitMultipliers.Kilo }));
            VoltPoint.Add(new MeasPoint<Voltage>(1 * VoltMultipliers,  thisRangeUnits.MainPhysicalQuantity.Multipliers,  new Frequency{Value = 100, Multipliers = UnitMultipliers.Kilo}));

            VoltPoint.Add(new MeasPoint<Voltage>((decimal)1.8 * VoltMultipliers,  thisRangeUnits.MainPhysicalQuantity.Multipliers, new Frequency{Value = 40}));
            VoltPoint.Add(new MeasPoint<Voltage>((decimal)1.8 * VoltMultipliers,  thisRangeUnits.MainPhysicalQuantity.Multipliers, new Frequency{Value = 1000}));
            VoltPoint.Add(new MeasPoint<Voltage>((decimal)1.8 * VoltMultipliers,  thisRangeUnits.MainPhysicalQuantity.Multipliers, new Frequency{Value = 10, Multipliers = UnitMultipliers.Kilo }));
            VoltPoint.Add(new MeasPoint<Voltage>((decimal)1.8 * VoltMultipliers,  thisRangeUnits.MainPhysicalQuantity.Multipliers, new Frequency{Value = 20, Multipliers = UnitMultipliers.Kilo }));
            VoltPoint.Add(new MeasPoint<Voltage>((decimal)1.8 * VoltMultipliers,  thisRangeUnits.MainPhysicalQuantity.Multipliers, new Frequency{Value = 50, Multipliers = UnitMultipliers.Kilo }));
            VoltPoint.Add(new MeasPoint<Voltage>((decimal)1.8 * VoltMultipliers,  thisRangeUnits.MainPhysicalQuantity.Multipliers, new Frequency { Value = 100, Multipliers = UnitMultipliers.Kilo }));
        }

        /// <inheritdoc />
        protected override string GetReportTableName()
        {
            return "FillTabBmOpe4_1_AcV_20V_Measure";
        }
    }

    public class Ope4_1_AcV_200V_Measure : Oper4AcvMeasureBase
    {
        public Ope4_1_AcV_200V_Measure(Mult107_109N.RangeNominal inRangeNominal, IUserItemOperation userItemOperation, string inResourceDir) :
            base(userItemOperation, inResourceDir)
        {
            OperMeasureMode = Mult107_109N.MeasureMode.ACV;
            OperationAcRangeCode = Mult107_109N.RangeCode.Range3Manual;
            OperationAcRangeNominal = inRangeNominal;
            Name = OperationAcRangeNominal.GetStringValue();
            Sheme = ShemeTemplateDefault.TemplateSheme;

            VoltMultipliers = 100;

            RangeResolution = new MeasPoint<Voltage>(10,  UnitMultipliers.Mili);

            HerzVPoint = new MeasPoint[6];
            HerzVPoint[0] = new MeasPoint<Frequency>(40,   UnitMultipliers.None).IsFake = true;
            HerzVPoint[1] = new MeasPoint<Frequency>(1000, UnitMultipliers.None);
            HerzVPoint[2] = new MeasPoint<Frequency>(10  , UnitMultipliers.Kilo);
            HerzVPoint[3] = new MeasPoint<Frequency>(20  , UnitMultipliers.Kilo);
            HerzVPoint[4] = new MeasPoint<Frequency>(50  , UnitMultipliers.Kilo);
            HerzVPoint[5] = new MeasPoint<Frequency>(100 , UnitMultipliers.Kilo).IsFake  =true;

            VoltPoint = new AcVariablePoint[3];
            //конкретно для первой точки 0.2 нужны не все частоты, поэтому вырежем только необходимые
            var trimHerzArr = new MeasPoint[4];
            Array.Copy(HerzVPoint, trimHerzArr, 4);
            VoltPoint[0] = new AcVariablePoint((decimal)0.2 * VoltMultipliers, thisRangeUnits.Units, thisRangeUnits.Multipliers, trimHerzArr);
            VoltPoint[1] = new AcVariablePoint(1 * VoltMultipliers, thisRangeUnits.Units, thisRangeUnits.Multipliers, HerzVPoint, true);
            VoltPoint[2] = new AcVariablePoint((decimal)1.8 * VoltMultipliers, thisRangeUnits.Units, thisRangeUnits.Multipliers, HerzVPoint, true);
        }

        /// <inheritdoc />
        protected override string GetReportTableName()
        {
            return "FillTabBmOpe4_1_AcV_200V_Measure";
        }
    }

    public class Ope4_1_AcV_750V_Measure : Oper4AcvMeasureBase
    {
        public Ope4_1_AcV_750V_Measure(Mult107_109N.RangeNominal inRangeNominal, IUserItemOperation userItemOperation, string inResourceDir)
            : base(userItemOperation, inResourceDir)
        {
            OperMeasureMode = Mult107_109N.MeasureMode.ACV;
            OperationAcRangeCode = Mult107_109N.RangeCode.Range4Manual;
            OperationAcRangeNominal = inRangeNominal;
            Name = OperationAcRangeNominal.GetStringValue();

            VoltMultipliers = 1;

            RangeResolution = new AcVariablePoint(100, MeasureUnits.V, UnitMultipliers.Mili);

            HerzVPoint = new MeasPoint[2];
            HerzVPoint[0] = new MeasPoint(MeasureUnits.Herz, UnitMultipliers.None, 40, true);
            HerzVPoint[1] = new MeasPoint(MeasureUnits.Herz, UnitMultipliers.None, 1000);

            VoltPoint = new AcVariablePoint[3];
            VoltPoint[0] = new AcVariablePoint(100 * VoltMultipliers, thisRangeUnits.Units,
                                               thisRangeUnits.Multipliers, HerzVPoint, true);
            VoltPoint[1] = new AcVariablePoint(400 * VoltMultipliers, thisRangeUnits.Units,
                                               thisRangeUnits.Multipliers, HerzVPoint, true);
            VoltPoint[2] = new AcVariablePoint(700 * VoltMultipliers, thisRangeUnits.Units,
                                               thisRangeUnits.Multipliers, HerzVPoint, true);
        }

        /// <inheritdoc />
        protected override string GetReportTableName()
        {
            return "FillTabBmOpe41AcV750VMeasure";
        }
    }

    #endregion ACV

    //////////////////////////////******DCI*******///////////////////////////////

    #region DCI

    public class Oper5DciMeasureBase : ParagraphBase<MeasPoint>
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        #region Fields

        //множители для пределов.
        public decimal BaseMultipliers;

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
        protected MeasPoint[] CurrentDciPoint;

        /// <summary>
        /// довесок к формуле погрешности- число единиц младшего разряда
        /// </summary>
        public int EdMlRaz; //значение для пределов свыше 200 мВ

        /// <summary>
        /// Разарешение пределеа измерения (последний значащий разряд)
        /// </summary>
        protected AcVariablePoint RangeResolution;

        ///// <summary>
        ///// Имя закладки таблички в результирующем протоколе doc (Ms Word).
        ///// </summary>
        //protected string ReportTableName;

        /// <summary>
        /// Это пустая точка, которая содержит только единицы измерения текущего
        /// поверяемого предела и множитель для единицы измерения.
        /// </summary>
        protected MeasPoint thisRangeUnits;

        #endregion Fields

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

        #endregion Property

        public Oper5DciMeasureBase(IUserItemOperation userItemOperation, string inResourceDir) : base(userItemOperation)
        {
            Name = "Определение погрешности измерения постоянного тока";
            OperMeasureMode = Mult107_109N.MeasureMode.DCmA;

            OperationRangeCode = Mult107_109N.RangeCode.Range1Manual;
            OperationRangeNominal = Mult107_109N.RangeNominal.RangeNone;
            DataRow = new List<IBasicOperation<MeasPoint>>();

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

      
        /// <inheritdoc />
        protected override string[] GenerateDataColumnTypeObject()
        {
            return new[] {"Предел измерения",
                "Поверяемая точка", "Измеренное значение", "Минимальное допустимое значение", "Максимальное допустимое значение"  }.Concat(base.GenerateDataColumnTypeObject()).ToArray(); ;
        }
        /// <inheritdoc />
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
                var dds = row as BasicOperationVerefication<MeasPoint>;
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

        protected override void InitWork()
        {
            base.InitWork();
            if (appa107N == null || flkCalib5522A == null) return;
            foreach (var currPoint in CurrentDciPoint)
            {
                var operation = new BasicOperationVerefication<MeasPoint>();
                operation.InitWork = async () =>
                {
                    try
                    {
                        if (appa107N.StringConnection.Equals("COM1"))
                            appa107N.StringConnection = GetStringConnect(appa107N);
                        flkCalib5522A.StringConnection ??= GetStringConnect(flkCalib5522A);

                        await Task.Run(() => { flkCalib5522A.Out.SetOutput(CalibrMain.COut.State.Off); });

                        var testMode = appa107N.GetMeasureMode;
                        while (OperMeasureMode != await Task<Mult107_109N.MeasureMode>.Factory.StartNew(() => appa107N.GetMeasureMode))
                            UserItemOperation.ServicePack.MessageBox()
                                             .Show($"Установите режим измерения: {OperMeasureMode.GetStringValue()} {OperMeasureMode}",
                                                   "Указание оператору", MessageButton.OK,
                                                   MessageIcon.Information,
                                                   MessageResult.OK);

                        while (await Task<Mult107_109N.RangeSwitchMode>.Factory.StartNew(() => appa107N.GetRangeSwitchMode) == Mult107_109N.RangeSwitchMode.Auto)
                            UserItemOperation.ServicePack.MessageBox()
                                             .Show("Установите ручной режим переключения пределов.");

                        while (OperationRangeNominal != await Task<Mult107_109N.RangeNominal>.Factory.StartNew(() => appa107N.GetRangeNominal))
                        {
                            int countPushRangeButton;

                            if (thisRangeUnits.Multipliers == UnitMultipliers.Mili)
                            {
                                CountOfRanges = 2;
                                UserItemOperation.ServicePack.MessageBox()
                                                 .Show($"Текущий предел измерения прибора {appa107N.GetRangeNominal.GetStringValue()}\n Необходимо установить предел {OperationRangeNominal.GetStringValue()} " +
                                                       $"Нажмите на приборе клавишу Range {countPushRangeButton = 1} раз.",
                                                       "Указание оператору", MessageButton.OK, MessageIcon.Information,
                                                       MessageResult.OK);
                            }
                            else
                            {
                                //работает только для ручного режима переключения пределов
                                CountOfRanges = 2;
                                var curRange = (int)appa107N.GetRangeCode - 127;
                                var targetRange = (int)OperationRangeCode - 127;
                                countPushRangeButton = Hepls.CountOfPushButton(CountOfRanges, curRange, targetRange);

                                UserItemOperation.ServicePack.MessageBox()
                                                 .Show($"Текущий предел измерения прибора {appa107N.GetRangeNominal.GetStringValue()}\n Необходимо установить предел {OperationRangeNominal.GetStringValue()} " +
                                                       $"Нажмите на приборе клавишу Range {countPushRangeButton} раз.",
                                                       "Указание оператору", MessageButton.OK, MessageIcon.Information,
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
                operation.BodyWork = () =>
                {
                    try
                    {
                        flkCalib5522A.Out.Set.Current.Dc.SetValue(currPoint.Value *
                                                                  (decimal)currPoint.Multipliers.GetDoubleValue());
                        flkCalib5522A.Out.SetOutput(CalibrMain.COut.State.On);
                        Thread.Sleep(2000);
                        //измеряем
                        var measurePoint = (decimal)appa107N.GetValue();

                        flkCalib5522A.Out.SetOutput(CalibrMain.COut.State.Off);

                        var mantisa =
                            MathStatistics.GetMantissa((decimal)(RangeResolution
                                                                 .MainPhysicalQuantity.Multipliers
                                                                 .GetDoubleValue() /
                                                                  currPoint.Multipliers.GetDoubleValue()));
                        //округляем измерения
                        MathStatistics.Round(ref measurePoint, mantisa);

                        operation.Getting =
                            new MeasPoint(thisRangeUnits.Units, thisRangeUnits.Multipliers, measurePoint);
                        operation.Expected = currPoint;
                        //расчет погрешности для конкретной точки предела измерения
                        operation.ErrorCalculation = (inA, inB) =>
                        {
                            var result = BaseTolCoeff * Math.Abs(operation.Expected.Value) + EdMlRaz *
                                RangeResolution.MainPhysicalQuantity.Value *
                                (decimal)(RangeResolution
                                          .MainPhysicalQuantity.Multipliers.GetDoubleValue() /
                                           currPoint.Multipliers
                                                    .GetDoubleValue());
                            var mantisa =
                                MathStatistics.GetMantissa((decimal)(RangeResolution
                                                                     .MainPhysicalQuantity.Multipliers
                                                                     .GetDoubleValue() *
                                                                      (double)RangeResolution
                                                                              .MainPhysicalQuantity.Value /
                                                                      currPoint.Multipliers
                                                                               .GetDoubleValue()));
                            MathStatistics.Round(ref result, mantisa);
                            return new MeasPoint(thisRangeUnits.Units, thisRangeUnits.Multipliers, result);
                        };

                        operation.LowerTolerance = new MeasPoint(thisRangeUnits.Units, thisRangeUnits.Multipliers,
                                                                 operation.Expected.Value -
                                                                 operation.Error.Value);

                        operation.UpperTolerance = new MeasPoint(thisRangeUnits.Units, thisRangeUnits.Multipliers,
                                                                 operation.Expected.Value +
                                                                 operation.Error.Value);

                        operation.IsGood = () =>
                        {
                            if (operation.Getting == null || operation.Expected == null ||
                                operation.UpperTolerance == null || operation.LowerTolerance == null)
                            {
                                return false;
                            }

                            return (operation.Getting.Value < operation.UpperTolerance.Value) &
                                   (operation.Getting.Value > operation.LowerTolerance.Value);
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
                                : (BasicOperationVerefication<MeasPoint>)operation.Clone());
            }
        }

        #endregion Methods

        //public override async Task StartWork(CancellationToken token)
        //{
        //    await base.StartWork(token);
        //    appa107N?.Dispose();
        //}

      
    }

    public class Oper5_1Dci_20mA_Measure : Oper5DciMeasureBase
    {
        public Oper5_1Dci_20mA_Measure(Mult107_109N.RangeNominal inRangeNominal, IUserItemOperation userItemOperation, string inResourceDir) :
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
            BaseTolCoeff = (decimal)0.002;
            EdMlRaz = 40;
            RangeResolution = new AcVariablePoint(1, MeasureUnits.I, UnitMultipliers.Micro);

            thisRangeUnits = new MeasPoint(MeasureUnits.I, UnitMultipliers.Mili, 0);

            BaseMultipliers = 1;
            CurrentDciPoint = new MeasPoint[5];
            CurrentDciPoint[0] =
                new MeasPoint(thisRangeUnits.Units, thisRangeUnits.Multipliers, 4 * BaseMultipliers);
            CurrentDciPoint[1] =
                new MeasPoint(thisRangeUnits.Units, thisRangeUnits.Multipliers, 8 * BaseMultipliers);
            CurrentDciPoint[2] =
                new MeasPoint(thisRangeUnits.Units, thisRangeUnits.Multipliers, 12 * BaseMultipliers);
            CurrentDciPoint[3] =
                new MeasPoint(thisRangeUnits.Units, thisRangeUnits.Multipliers, 18 * BaseMultipliers);
            CurrentDciPoint[4] =
                new MeasPoint(thisRangeUnits.Units, thisRangeUnits.Multipliers, -18 * BaseMultipliers);
        }

        /// <inheritdoc />
        protected override string GetReportTableName()
        {
            return "FillTabBmOper5_1Dci_20mA_Measure";
        }
    }

    public class Oper5_1Dci_200mA_Measure : Oper5DciMeasureBase
    {
        public Oper5_1Dci_200mA_Measure(Mult107_109N.RangeNominal inRangeNominal, IUserItemOperation userItemOperation, string inResourceDir)
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
            BaseTolCoeff = (decimal)0.002;
            EdMlRaz = 40;
            RangeResolution = new AcVariablePoint(10, MeasureUnits.I, UnitMultipliers.Micro);

            thisRangeUnits = new MeasPoint(MeasureUnits.I, UnitMultipliers.Mili, 0);

            BaseMultipliers = 10;
            CurrentDciPoint = new MeasPoint[5];
            CurrentDciPoint[0] =
                new MeasPoint(thisRangeUnits.Units, thisRangeUnits.Multipliers, 4 * BaseMultipliers);
            CurrentDciPoint[1] =
                new MeasPoint(thisRangeUnits.Units, thisRangeUnits.Multipliers, 8 * BaseMultipliers);
            CurrentDciPoint[2] =
                new MeasPoint(thisRangeUnits.Units, thisRangeUnits.Multipliers, 12 * BaseMultipliers);
            CurrentDciPoint[3] =
                new MeasPoint(thisRangeUnits.Units, thisRangeUnits.Multipliers, 18 * BaseMultipliers);
            CurrentDciPoint[4] =
                new MeasPoint(thisRangeUnits.Units, thisRangeUnits.Multipliers, -18 * BaseMultipliers);
        }

        /// <inheritdoc />
        protected override string GetReportTableName()
        {
            return "FillTabBmOper5_1Dci_200mA_Measure";
        }
    }

    public class Oper5_1Dci_2A_Measure : Oper5DciMeasureBase
    {
        public Oper5_1Dci_2A_Measure(Mult107_109N.RangeNominal inRangeNominal, IUserItemOperation userItemOperation, string inResourceDir) :
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
            BaseTolCoeff = (decimal)0.002;
            EdMlRaz = 40;
            RangeResolution = new AcVariablePoint(100, MeasureUnits.I, UnitMultipliers.Micro);

            thisRangeUnits = new MeasPoint(MeasureUnits.I, UnitMultipliers.None, 0);

            BaseMultipliers = (decimal)0.1;
            CurrentDciPoint = new MeasPoint[5];
            CurrentDciPoint[0] =
                new MeasPoint(thisRangeUnits.Units, thisRangeUnits.Multipliers, 4 * BaseMultipliers);
            CurrentDciPoint[1] =
                new MeasPoint(thisRangeUnits.Units, thisRangeUnits.Multipliers, 8 * BaseMultipliers);
            CurrentDciPoint[2] =
                new MeasPoint(thisRangeUnits.Units, thisRangeUnits.Multipliers, 12 * BaseMultipliers);
            CurrentDciPoint[3] =
                new MeasPoint(thisRangeUnits.Units, thisRangeUnits.Multipliers, 18 * BaseMultipliers);
            CurrentDciPoint[4] =
                new MeasPoint(thisRangeUnits.Units, thisRangeUnits.Multipliers, -18 * BaseMultipliers);

            Sheme = new ShemeImage
            {
                AssemblyLocalName = Assembly.GetExecutingAssembly().GetName().Name,
                Description = "Измерительная схема",
                Number = 3,
                FileName = "appa_10XN_A_Aux_5522A.jpg",
                ExtendedDescription = "Соберите измерительную схему, согласно рисунку"
            };
        }

        /// <inheritdoc />
        protected override string GetReportTableName()
        {
            return "FillTabBmOper5_1Dci_2A_Measure";
        }
    }

    public class Oper5_2_1Dci_10A_Measure : Oper5DciMeasureBase
    {
        public Oper5_2_1Dci_10A_Measure(Mult107_109N.RangeNominal inRangeNominal, IUserItemOperation userItemOperation, string inResourceDir)
            : base(userItemOperation, inResourceDir)
        {
            
            OperMeasureMode = Mult107_109N.MeasureMode.DCI;
            OperationRangeCode = Mult107_109N.RangeCode.Range2Manual;
            OperationRangeNominal = inRangeNominal;

            BaseTolCoeff = (decimal)0.002;
            EdMlRaz = 40;
            RangeResolution = new AcVariablePoint(1, MeasureUnits.I, UnitMultipliers.Mili);

            thisRangeUnits = new MeasPoint(MeasureUnits.I, UnitMultipliers.None, 0);

            CurrentDciPoint = new MeasPoint[1];
            CurrentDciPoint[0] = new MeasPoint(thisRangeUnits.Units, thisRangeUnits.Multipliers, 1);

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

        /// <inheritdoc />
        protected override string GetReportTableName()
        {
            return "FillTabBmOper5_2_1Dci_10A_Measure";
        }
    }

    public class Oper5_2_2Dci_10A_Measure : Oper5DciMeasureBase
    {
        public Oper5_2_2Dci_10A_Measure(Mult107_109N.RangeNominal inRangeNominal, IUserItemOperation userItemOperation, string inResourceDir)
            : base(userItemOperation, inResourceDir)
        {
            OperMeasureMode = Mult107_109N.MeasureMode.DCI;
            OperationRangeCode = Mult107_109N.RangeCode.Range2Manual;
            OperationRangeNominal = inRangeNominal;
            Name = OperationRangeNominal.GetStringValue();

            BaseTolCoeff = (decimal)0.002;
            EdMlRaz = 40;
            RangeResolution = new AcVariablePoint(1, MeasureUnits.I, UnitMultipliers.Mili);

            thisRangeUnits = new MeasPoint(MeasureUnits.I, UnitMultipliers.None, 0);

            CurrentDciPoint = new MeasPoint[3];
            CurrentDciPoint[0] = new MeasPoint(thisRangeUnits.Units, thisRangeUnits.Multipliers, 5);
            CurrentDciPoint[1] = new MeasPoint(thisRangeUnits.Units, thisRangeUnits.Multipliers, 9);
            CurrentDciPoint[2] = new MeasPoint(thisRangeUnits.Units, thisRangeUnits.Multipliers, -9);

            Sheme = new ShemeImage
            {
                AssemblyLocalName = Assembly.GetExecutingAssembly().GetName().Name,
                Description = "Измерительная схема",
                Number = 4,
                FileName = "appa_10XN_20A_5522A.jpg",
                ExtendedDescription = "Соберите измерительную схему, согласно рисунку"
            };
        }

        /// <inheritdoc />
        protected override string GetReportTableName()
        {
            return "FillTabBmOper5_2_2Dci_10A_Measure";
        }
    }

    #endregion DCI

    //////////////////////////////******ACI*******///////////////////////////////

    #region ACI

    public class Oper6AciMeasureBase : ParagraphBase<AcVariablePoint>
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        #region Fields

        /// <summary>
        /// Итоговый массив поверяемых точек. У каждого номинала напряжения вложены номиналы частот для текущей точки.
        /// </summary>
        public AcVariablePoint[] AciPoint;

        /// <summary>
        /// Число пределов измерения
        /// </summary>
        private int CountOfRanges;

        /// <summary>
        /// Множитель для поверяемых точек. (Если точки можно посчитать простым умножением).
        /// </summary>
        protected decimal CurrentMultipliers;

        /// <summary>
        /// Набор частот, характерный для данного предела измерения
        /// </summary>
        protected MeasPoint[] HerzPoint;

        

        /// <summary>
        /// Это пустая точка, которая содержит только единицы измерения текущего
        /// поверяемого предела и множитель для единицы измерения.
        /// </summary>
        protected MeasPoint thisRangeUnits;

        #endregion Fields

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

        #endregion Property

        public Oper6AciMeasureBase(IUserItemOperation userItemOperation, string inResourceDir) : base(userItemOperation)
        {
            Name = "Определение погрешности измерения переменного тока";
            OperMeasureMode = Mult107_109N.MeasureMode.ACV;

            OperationRangeCode = Mult107_109N.RangeCode.Range1Manual;
            OperationRangeNominal = Mult107_109N.RangeNominal.RangeNone;
            DataRow = new List<IBasicOperation<AcVariablePoint>>();

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
        protected void ConstructTooleranceFormula(MeasPoint inFreq)
        {
            if (OperationRangeNominal == Mult107_109N.RangeNominal.Range20mA &&
                inFreq.Multipliers == UnitMultipliers.None)
            {
                if (inFreq.Value >= 40 && inFreq.Value < 500)
                {
                    BaseTolCoeff = (decimal)0.008;
                    EdMlRaz = 50;
                }

                if (inFreq.Value >= 500 && inFreq.Value < 1000)
                {
                    BaseTolCoeff = (decimal)0.012;
                    EdMlRaz = 80;
                }
            }
            else if (OperationRangeNominal == Mult107_109N.RangeNominal.Range200mA ||
                     OperationRangeNominal == Mult107_109N.RangeNominal.Range2A ||
                     OperationRangeNominal == Mult107_109N.RangeNominal.Range10A)
            {
                if (inFreq.Value >= 40 && inFreq.Value < 500)
                {
                    BaseTolCoeff = (decimal)0.008;
                    EdMlRaz = 50;
                }
                else if (inFreq.Value >= 500 && inFreq.Value < 1000)
                {
                    BaseTolCoeff = (decimal)0.012;
                    EdMlRaz = 80;
                }
                else if (inFreq.Value >= 1000 && inFreq.Value <= 3000)
                {
                    BaseTolCoeff = (decimal)0.02;
                    EdMlRaz = 80;
                }
            }
        }

   
        /// <inheritdoc />
        protected override string[] GenerateDataColumnTypeObject()
        {
            return new[] {"Предел измерения",
                "Поверяемая точка", "Измеренное значение", "Минимальное допустимое значение", "Максимальное допустимое значение"  }.Concat(base.GenerateDataColumnTypeObject()).ToArray(); ;
        }
        /// <inheritdoc />
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
                var dds = row as BasicOperationVerefication<AcVariablePoint>;
                // ReSharper disable once PossibleNullReferenceException
                if (dds == null) continue;
                dataRow[0] = OperationRangeNominal.GetStringValue();
                dataRow[1] = dds.Expected.MainPhysicalQuantity.Description;
                //тут может упасть!!!
                dataRow[2] = dds.Expected?.Herz[0].Description;
                dataRow[3] = dds.Getting?.MainPhysicalQuantity.Description;
                dataRow[4] = dds.LowerTolerance?.MainPhysicalQuantity.Description;
                dataRow[5] = dds.UpperTolerance?.MainPhysicalQuantity.Description;
                if (dds.IsGood == null)
                    dataRow[6] = "не выполнено";
                else
                    dataRow[6] = dds.IsGood() ? "Годен" : "Брак";
                dataTable.Rows.Add(dataRow);
            }

            return dataTable;
        }

        protected override void InitWork()
        {
            base.InitWork();
            if (flkCalib5522A == null || appa107N == null) return;
           

            foreach (var curr in AciPoint)
                foreach (var freqPoint in curr.Herz)
                {
                    var operation = new BasicOperationVerefication<AcVariablePoint>();
                    operation.InitWork = async () =>
                    {
                        try
                        {
                            if (appa107N.StringConnection.Equals("COM1"))
                                appa107N.StringConnection = GetStringConnect(appa107N);
                            flkCalib5522A.StringConnection ??= GetStringConnect(flkCalib5522A);

                            await Task.Run(() => { flkCalib5522A.Out.SetOutput(CalibrMain.COut.State.Off); });

                            var testMode = appa107N.GetMeasureMode;
                            while (OperMeasureMode != await Task<Mult107_109N.MeasureMode>.Factory.StartNew(() => appa107N.GetMeasureMode))
                                UserItemOperation.ServicePack.MessageBox()
                                                 .Show($"Установите режим измерения: {OperMeasureMode.GetStringValue()} {OperMeasureMode}",
                                                       "Указание оператору", MessageButton.OK,
                                                       MessageIcon.Information,
                                                       MessageResult.OK);

                            while (await Task<Mult107_109N.RangeSwitchMode>.Factory.StartNew(() => appa107N.GetRangeSwitchMode) == Mult107_109N.RangeSwitchMode.Auto)
                                UserItemOperation.ServicePack.MessageBox()
                                                 .Show("Установите ручной режим переключения пределов.");

                            while (OperationRangeNominal != await Task<Mult107_109N.RangeNominal>.Factory.StartNew(() => appa107N.GetRangeNominal))
                            {
                                int countPushRangeButton;

                                if (thisRangeUnits.Multipliers == UnitMultipliers.Mili)
                                {
                                    CountOfRanges = 2;
                                    UserItemOperation.ServicePack.MessageBox()
                                                     .Show($"Текущий предел измерения прибора {appa107N.GetRangeNominal.GetStringValue()}\n Необходимо установить предел {OperationRangeNominal.GetStringValue()} " +
                                                           $"Нажмите на приборе клавишу Range {countPushRangeButton = 1} раз.",
                                                           "Указание оператору", MessageButton.OK, MessageIcon.Information,
                                                           MessageResult.OK);
                                }
                                else
                                {
                                //работает только для ручного режима переключения пределов
                                CountOfRanges = 2;
                                    var curRange = (int)appa107N.GetRangeCode - 127;
                                    var targetRange = (int)OperationRangeCode - 127;
                                    countPushRangeButton = Hepls.CountOfPushButton(CountOfRanges, curRange, targetRange);

                                    UserItemOperation.ServicePack.MessageBox()
                                                     .Show($"Текущий предел измерения прибора {appa107N.GetRangeNominal.GetStringValue()}\n Необходимо установить предел {OperationRangeNominal.GetStringValue()} " +
                                                           $"Нажмите на приборе клавишу Range {countPushRangeButton} раз.",
                                                           "Указание оператору", MessageButton.OK, MessageIcon.Information,
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

                    operation.BodyWork = () =>
                    {
                        try
                        {
                            var mantisa =
                                MathStatistics.GetMantissa((decimal)(RangeResolution
                                                                     .MainPhysicalQuantity.Multipliers
                                                                     .GetDoubleValue() * (double)RangeResolution
                                                                                                 .MainPhysicalQuantity
                                                                                                 .Value /
                                                                      curr.MainPhysicalQuantity.Multipliers
                                                                          .GetDoubleValue()), true);

                            operation.Expected = new AcVariablePoint(curr.MainPhysicalQuantity.Value,
                                                                     thisRangeUnits.Units, thisRangeUnits.Multipliers,
                                                                     new[] { freqPoint });

                        //расчет погрешности для конкретной точки предела измерения
                        TolleranceConstruct(freqPoint); // функция подбирает коэффициенты для формулы погрешности
                        operation.ErrorCalculation = (inA, inB) =>
                            {
                                var result = BaseTolCoeff * Math.Abs(operation.Expected.MainPhysicalQuantity.Value) +
                                             EdMlRaz *
                                             RangeResolution.MainPhysicalQuantity.Value *
                                             (decimal)(RangeResolution
                                                       .MainPhysicalQuantity.Multipliers.GetDoubleValue() /
                                                        curr.MainPhysicalQuantity.Multipliers
                                                            .GetDoubleValue());

                                MathStatistics.Round(ref result, mantisa);
                                return new AcVariablePoint(result, thisRangeUnits.Units, thisRangeUnits.Multipliers);
                            };

                            operation.LowerTolerance =
                                new AcVariablePoint(operation.Expected.MainPhysicalQuantity.Value -
                                                    operation.Error.MainPhysicalQuantity.Value,
                                                    operation.Expected.MainPhysicalQuantity.Units,
                                                    operation.Expected.MainPhysicalQuantity.Multipliers);
                        //operation.Expected - operation.Error;
                        operation.UpperTolerance =
                                new AcVariablePoint(operation.Expected.MainPhysicalQuantity.Value +
                                                    operation.Error.MainPhysicalQuantity.Value,
                                                    operation.Expected.MainPhysicalQuantity.Units,
                                                    operation.Expected.MainPhysicalQuantity.Multipliers);
                        //operation.Expected + operation.Error;
                        operation.IsGood = () =>
                            {
                                if (operation.Expected.MainPhysicalQuantity == null ||
                                    operation.Getting.MainPhysicalQuantity == null ||
                                    operation.LowerTolerance.MainPhysicalQuantity == null ||
                                    operation.UpperTolerance.MainPhysicalQuantity == null) return false;

                                return (operation.Getting.MainPhysicalQuantity.Value <
                                        operation.UpperTolerance.MainPhysicalQuantity.Value) &
                                       (operation.Getting.MainPhysicalQuantity.Value >
                                        operation.LowerTolerance.MainPhysicalQuantity.Value);
                            };

                            decimal measurePoint = 0;
                            if (freqPoint.IsFake)
                            {
                                measurePoint =
                                    (decimal)
                                    MathStatistics
                                       .RandomToRange((double)operation.LowerTolerance.MainPhysicalQuantity.Value,
                                                      (double)operation.UpperTolerance.MainPhysicalQuantity.Value);
                            }
                            else
                            {
                                flkCalib5522A.Out.Set.Current.Ac.SetValue(curr.MainPhysicalQuantity.Value,
                                                                          freqPoint.Value,
                                                                          curr.MainPhysicalQuantity.Multipliers,
                                                                          freqPoint.Multipliers);
                                flkCalib5522A.Out.SetOutput(CalibrMain.COut.State.On);
                                Thread.Sleep(2000);
                            //измеряем
                            measurePoint = (decimal)appa107N.GetValue();
                                flkCalib5522A.Out.SetOutput(CalibrMain.COut.State.Off);
                            }

                        //округляем измерения
                        MathStatistics.Round(ref measurePoint, mantisa);

                            operation.Getting =
                                new AcVariablePoint(measurePoint, thisRangeUnits.Units, thisRangeUnits.Multipliers);
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
                                    : (BasicOperationVerefication<AcVariablePoint>)operation.Clone());
                }
        }

        /// <summary>
        /// В зависимости от напряжения и частоты подбирает коэффициенты для расчета погрешности.
        /// </summary>
        private void TolleranceConstruct(MeasPoint freqPoint)
        {
            //вычислим реальную частоту
            var realFreq = freqPoint.Value * (decimal)freqPoint.Multipliers.GetDoubleValue();
            if (OperationRangeNominal == Mult107_109N.RangeNominal.Range20mA)
            {
                if (realFreq >= 40 && realFreq <= 500)
                {
                    BaseTolCoeff = (decimal)0.008;
                    EdMlRaz = 50;
                    return;
                }

                if (realFreq > 500 && realFreq <= 1000)
                {
                    BaseTolCoeff = (decimal)0.012;
                    EdMlRaz = 80;
                    return;
                }
            }

            if (OperationRangeNominal == Mult107_109N.RangeNominal.Range200mA ||
                OperationRangeNominal == Mult107_109N.RangeNominal.Range2A ||
                OperationRangeNominal == Mult107_109N.RangeNominal.Range10A)
            {
                if (realFreq >= 40 && realFreq <= 500)
                {
                    BaseTolCoeff = (decimal)0.008;
                    EdMlRaz = 50;
                    return;
                }

                EdMlRaz = 80;

                if (realFreq > 500 && realFreq <= 1000) BaseTolCoeff = (decimal)0.012;
                if (realFreq > 1000 && realFreq <= 3000) BaseTolCoeff = (decimal)0.020;
            }
        }

        #endregion Methods

        

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
        protected AcVariablePoint RangeResolution;

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
            RangeResolution = new AcVariablePoint(1, MeasureUnits.I, UnitMultipliers.Micro);
            Name = OperationRangeNominal.GetStringValue();

            thisRangeUnits = new MeasPoint(MeasureUnits.I, UnitMultipliers.Mili, 0);

            CurrentMultipliers = 1;

            HerzPoint = new MeasPoint[2];
            HerzPoint[0] = new MeasPoint(MeasureUnits.Herz, UnitMultipliers.None, 40);
            HerzPoint[1] = new MeasPoint(MeasureUnits.Herz, UnitMultipliers.None, 1000);

            AciPoint = new AcVariablePoint[3];
            AciPoint[0] = new AcVariablePoint(4 * CurrentMultipliers, thisRangeUnits.Units,
                                              thisRangeUnits.Multipliers, HerzPoint);
            AciPoint[1] = new AcVariablePoint(10 * CurrentMultipliers, thisRangeUnits.Units,
                                              thisRangeUnits.Multipliers, HerzPoint);
            AciPoint[2] = new AcVariablePoint(18 * CurrentMultipliers, thisRangeUnits.Units,
                                              thisRangeUnits.Multipliers, HerzPoint);
        }

        /// <inheritdoc />
        protected override string GetReportTableName()
        {
            return "FillTabBmOper6_1Aci_20mA_Measure";
        }
    }

    public class Oper6_1Aci_200mA_Measure : Oper6AciMeasureBase
    {
        public Oper6_1Aci_200mA_Measure(Mult107_109N.RangeNominal inRangeNominal,
            IUserItemOperation userItemOperation, string inResourceDir) : base(userItemOperation, inResourceDir)
        {
            OperMeasureMode = Mult107_109N.MeasureMode.ACmA;
            OperationRangeCode = Mult107_109N.RangeCode.Range2Manual;
            OperationRangeNominal = inRangeNominal;
            RangeResolution = new AcVariablePoint(10, MeasureUnits.I, UnitMultipliers.Micro);
            Name = OperationRangeNominal.GetStringValue();

            thisRangeUnits = new MeasPoint(MeasureUnits.I, UnitMultipliers.Mili, 0);

            CurrentMultipliers = 10;

            HerzPoint = new MeasPoint[3];
            HerzPoint[0] = new MeasPoint(MeasureUnits.Herz, UnitMultipliers.None, 40);
            HerzPoint[1] = new MeasPoint(MeasureUnits.Herz, UnitMultipliers.None, 1000);
            HerzPoint[2] = new MeasPoint(MeasureUnits.Herz, UnitMultipliers.Kilo, 3);

            AciPoint = new AcVariablePoint[3];
            AciPoint[0] = new AcVariablePoint(4 * CurrentMultipliers, thisRangeUnits.Units,
                                              thisRangeUnits.Multipliers, HerzPoint);
            AciPoint[1] = new AcVariablePoint(10 * CurrentMultipliers, thisRangeUnits.Units,
                                              thisRangeUnits.Multipliers, HerzPoint);
            AciPoint[2] = new AcVariablePoint(18 * CurrentMultipliers, thisRangeUnits.Units,
                                              thisRangeUnits.Multipliers, HerzPoint);
        }

        /// <inheritdoc />
        protected override string GetReportTableName()
        {
            return "FillTabBmOper6_1Aci_200mA_Measure";
        }
    }

    public class Oper6_1Aci_2A_Measure : Oper6AciMeasureBase
    {
        public Oper6_1Aci_2A_Measure(Mult107_109N.RangeNominal inRangeNominal,
            IUserItemOperation userItemOperation, string inResourceDir) : base(userItemOperation, inResourceDir)
        {
            OperMeasureMode = Mult107_109N.MeasureMode.ACI;
            OperationRangeCode = Mult107_109N.RangeCode.Range1Manual;
            OperationRangeNominal = inRangeNominal;
            RangeResolution = new AcVariablePoint(100, MeasureUnits.I, UnitMultipliers.Micro);
            Name = OperationRangeNominal.GetStringValue();

            thisRangeUnits = new MeasPoint(MeasureUnits.I, UnitMultipliers.None, 0);
            CurrentMultipliers = (decimal)0.1;

            HerzPoint = new MeasPoint[3];
            HerzPoint[0] = new MeasPoint(MeasureUnits.Herz, UnitMultipliers.None, 40);
            HerzPoint[1] = new MeasPoint(MeasureUnits.Herz, UnitMultipliers.None, 1000);
            HerzPoint[2] = new MeasPoint(MeasureUnits.Herz, UnitMultipliers.Kilo, 3);

            AciPoint = new AcVariablePoint[3];
            AciPoint[0] = new AcVariablePoint(4 * CurrentMultipliers, thisRangeUnits.Units,
                                              thisRangeUnits.Multipliers, HerzPoint);
            AciPoint[1] = new AcVariablePoint(10 * CurrentMultipliers, thisRangeUnits.Units,
                                              thisRangeUnits.Multipliers, HerzPoint);
            AciPoint[2] = new AcVariablePoint(18 * CurrentMultipliers, thisRangeUnits.Units,
                                              thisRangeUnits.Multipliers, HerzPoint);

            Sheme = new ShemeImage
            {
                AssemblyLocalName = Assembly.GetExecutingAssembly().GetName().Name,
                Description = "Измерительная схема",
                Number = 3,
                FileName = "appa_10XN_A_Aux_5522A.jpg",
                ExtendedDescription = "Соберите измерительную схему, согласно рисунку"
            };
        }

        /// <inheritdoc />
        protected override string GetReportTableName()
        {
            return "FillTabBmOper6_1Aci_2A_Measure";
        }
    }

    public class Oper6_2_1Aci_10A_Measure : Oper6AciMeasureBase
    {
        public Oper6_2_1Aci_10A_Measure(Mult107_109N.RangeNominal inRangeNominal,
            IUserItemOperation userItemOperation, string inResourceDir) : base(userItemOperation, inResourceDir)
        {
            OperMeasureMode = Mult107_109N.MeasureMode.ACI;
            OperationRangeCode = Mult107_109N.RangeCode.Range2Manual;
            OperationRangeNominal = inRangeNominal;
            RangeResolution = new AcVariablePoint(1, MeasureUnits.I, UnitMultipliers.Mili);

            thisRangeUnits = new MeasPoint(MeasureUnits.I, UnitMultipliers.None, 0);

            HerzPoint = new MeasPoint[3];
            HerzPoint[0] = new MeasPoint(MeasureUnits.Herz, UnitMultipliers.None, 40);
            HerzPoint[1] = new MeasPoint(MeasureUnits.Herz, UnitMultipliers.None, 1000);
            HerzPoint[2] = new MeasPoint(MeasureUnits.Herz, UnitMultipliers.Kilo, 3);

            AciPoint = new AcVariablePoint[1];
            AciPoint[0] = new AcVariablePoint(2, thisRangeUnits.Units, thisRangeUnits.Multipliers, HerzPoint);
            Name = OperationRangeNominal.GetStringValue() +
                   $" точка {AciPoint[0].MainPhysicalQuantity.Description}";

            Sheme = new ShemeImage
            {
                AssemblyLocalName = Assembly.GetExecutingAssembly().GetName().Name,
                Description = "Измерительная схема",
                Number = 3,
                FileName = "appa_10XN_A_Aux_5522A.jpg",
                ExtendedDescription = "Соберите измерительную схему, согласно рисунку"
            };
        }

        /// <inheritdoc />
        protected override string GetReportTableName()
        {
            return "FillTabBmOper6_2_1Aci_10A_Measure";
        }
    }

    public class Oper6_2_2Aci_10A_Measure : Oper6AciMeasureBase
    {
        public Oper6_2_2Aci_10A_Measure(Mult107_109N.RangeNominal inRangeNominal,
            IUserItemOperation userItemOperation, string inResourceDir) : base(userItemOperation, inResourceDir)
        {
            
            OperMeasureMode = Mult107_109N.MeasureMode.ACI;
            OperationRangeCode = Mult107_109N.RangeCode.Range2Manual;
            OperationRangeNominal = inRangeNominal;
            RangeResolution = new AcVariablePoint(1, MeasureUnits.I, UnitMultipliers.Mili);
            Name = OperationRangeNominal.GetStringValue();
            thisRangeUnits = new MeasPoint(MeasureUnits.I, UnitMultipliers.None, 0);

            HerzPoint = new MeasPoint[3];
            HerzPoint[0] = new MeasPoint(MeasureUnits.Herz, UnitMultipliers.None, 40, true);
            HerzPoint[1] = new MeasPoint(MeasureUnits.Herz, UnitMultipliers.None, 1000);
            HerzPoint[2] = new MeasPoint(MeasureUnits.Herz, UnitMultipliers.Kilo, 3);

            AciPoint = new AcVariablePoint[2];
            AciPoint[0] = new AcVariablePoint(5, thisRangeUnits.Units, thisRangeUnits.Multipliers, HerzPoint);
            AciPoint[1] = new AcVariablePoint(9, thisRangeUnits.Units, thisRangeUnits.Multipliers, HerzPoint);

            Sheme = new ShemeImage
            {
                AssemblyLocalName = Assembly.GetExecutingAssembly().GetName().Name,
                Description = "Измерительная схема",
                Number = 4,
                FileName = "appa_10XN_20A_5522A.jpg",
                ExtendedDescription = "Соберите измерительную схему, согласно рисунку"
            };
        }

        /// <inheritdoc />
        protected override string GetReportTableName()
        {
            return "FillTabBmOper6_2_2Aci_10A_Measure";
        }
    }

    #endregion ACI

    //////////////////////////////******FREQ*******///////////////////////////////

    #region FREQ

    public class Oper7FreqMeasureBase : ParagraphBase<MeasPoint>
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        #region Fields

        //множители для пределов.
        public decimal BaseMultipliers;

        public decimal BaseTolCoeff;

        /// <summary>
        /// Число пределов измерения
        /// </summary>
        private int CountOfRanges;

        /// <summary>
        /// довесок к формуле погрешности- единица младшего разряда
        /// </summary>
        public int EdMlRaz;

        protected MeasPoint[] HerzPoint;

        protected AcVariablePoint RangeResolution;

       

        /// <summary>
        /// Это пустая точка, которая содержит только единицы измерения текущего
        /// поверяемого предела и множитель для единицы измерения.
        /// </summary>
        protected MeasPoint thisRangeUnits;

        /// <summary>
        /// Массив поверяемых точек напряжения.
        /// </summary>
        protected AcVariablePoint[] VoltPoint;

        #endregion Fields

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

        #endregion Property

        public Oper7FreqMeasureBase(IUserItemOperation userItemOperation, string inResourceDir) : base(userItemOperation)
        {
            Name = "Определение погрешности измерения частоты переменного напряжения";
            OperMeasureMode = Mult107_109N.MeasureMode.Herz;

            OperationRangeCode = Mult107_109N.RangeCode.Range1Manual;
            OperationRangeNominal = Mult107_109N.RangeNominal.RangeNone;

            DataRow = new List<IBasicOperation<MeasPoint>>();
            Sheme = ShemeTemplateDefault.TemplateSheme;
            Sheme.AssemblyLocalName = inResourceDir;
        }

        #region Methods

        /// <inheritdoc />
        protected override string[] GenerateDataColumnTypeObject()
        {
            return new[] {"Предел измерения",
                "Поверяемая точка", "Измеренное значение", "Минимальное допустимое значение", "Максимальное допустимое значение"  }.Concat(base.GenerateDataColumnTypeObject()).ToArray(); ;
        }
        /// <inheritdoc />
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
                var dds = row as BasicOperationVerefication<MeasPoint>;
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

        protected override void InitWork()
        {
            base.InitWork();
            if (appa107N == null || flkCalib5522A == null) return;

            foreach (var voltPoint in VoltPoint)
                foreach (var freqPoint in voltPoint.Herz)
                {
                    var operation = new BasicOperationVerefication<MeasPoint>();
                    operation.InitWork = async () =>
                    {
                        try
                        {
                            if (appa107N.StringConnection.Equals("COM1"))
                                appa107N.StringConnection = GetStringConnect(appa107N);
                            flkCalib5522A.StringConnection ??= GetStringConnect(flkCalib5522A);

                            await Task.Run(() => { flkCalib5522A.Out.SetOutput(CalibrMain.COut.State.Off); });

                            while (OperMeasureMode != await Task<Mult107_109N.MeasureMode>.Factory.StartNew(() => appa107N.GetMeasureMode))
                                UserItemOperation.ServicePack.MessageBox()
                                                 .Show($"Установите режим измерения: {OperMeasureMode.GetStringValue()} {OperMeasureMode}",
                                                       "Указание оператору", MessageButton.OK,
                                                       MessageIcon.Information,
                                                       MessageResult.OK);

                            while (await Task<Mult107_109N.RangeSwitchMode>.Factory.StartNew(() => appa107N.GetRangeSwitchMode) != Mult107_109N.RangeSwitchMode.Auto)
                                UserItemOperation.ServicePack.MessageBox()
                                                 .Show("Установите автоматический режим переключения пределов.");
                        }
                        catch (Exception e)
                        {
                            Logger.Error(e);
                            throw;
                        }
                    };
                    operation.BodyWork = () =>
                    {
                        try
                        {
                            flkCalib5522A.Out.Set.Voltage.Ac.SetValue(voltPoint.MainPhysicalQuantity.Value,
                                                                      freqPoint.Value,
                                                                      voltPoint.MainPhysicalQuantity.Multipliers,
                                                                      freqPoint.Multipliers);
                            flkCalib5522A.Out.SetOutput(CalibrMain.COut.State.On);
                            Thread.Sleep(100);
                        //измеряем
                        var measurePoint = (decimal)appa107N.GetValue();

                            flkCalib5522A.Out.SetOutput(CalibrMain.COut.State.Off);

                            operation.Getting =
                                new MeasPoint(thisRangeUnits.Units, thisRangeUnits.Multipliers, measurePoint);
                            operation.Expected = freqPoint;

                            var mantisa =
                                MathStatistics.GetMantissa((decimal)(RangeResolution
                                                                     .MainPhysicalQuantity.Multipliers
                                                                     .GetDoubleValue() *
                                                                      (double)RangeResolution
                                                                              .MainPhysicalQuantity.Value /
                                                                      freqPoint.Multipliers
                                                                               .GetDoubleValue()));

                        //расчет погрешности для конкретной точки предела измерения
                        operation.ErrorCalculation = (inA, inB) =>
                            {
                                var result = BaseTolCoeff * operation.Expected.Value + EdMlRaz *
                                    RangeResolution.MainPhysicalQuantity.Value *
                                    (decimal)(RangeResolution
                                              .MainPhysicalQuantity.Multipliers.GetDoubleValue() /
                                               freqPoint.Multipliers
                                                        .GetDoubleValue()
                                    );

                                MathStatistics.Round(ref result, mantisa);
                                return new MeasPoint(thisRangeUnits.Units, thisRangeUnits.Multipliers, result);
                            };

                            operation.LowerTolerance = new MeasPoint(thisRangeUnits.Units, thisRangeUnits.Multipliers,
                                                                     operation.Expected.Value -
                                                                     operation.Error.Value);
                            operation.UpperTolerance = new MeasPoint(thisRangeUnits.Units, thisRangeUnits.Multipliers,
                                                                     operation.Expected.Value +
                                                                     operation.Error.Value);
                            operation.IsGood = () =>
                            {
                                if (operation.Expected == null || operation.Getting == null ||
                                    operation.LowerTolerance == null || operation.UpperTolerance == null) return false;
                                return (operation.Getting.Value < operation.UpperTolerance.Value) &
                                       (operation.Getting.Value > operation.LowerTolerance.Value);
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
                                    : (BasicOperationVerefication<MeasPoint>)operation.Clone());
                }
        }

        #endregion Methods

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
            thisRangeUnits = new MeasPoint(MeasureUnits.Herz, UnitMultipliers.None, 0);
            BaseTolCoeff = (decimal)0.0001;
            EdMlRaz = 50;
            RangeResolution = new AcVariablePoint(1, MeasureUnits.Herz, UnitMultipliers.Mili);

            HerzPoint = new MeasPoint[1];
            HerzPoint[0] = new MeasPoint(thisRangeUnits.Units, thisRangeUnits.Multipliers, 10);

            VoltPoint = new AcVariablePoint[1];
            VoltPoint[0] = new AcVariablePoint((decimal)0.5, MeasureUnits.V, UnitMultipliers.None, HerzPoint);
        }

        /// <inheritdoc />
        protected override string GetReportTableName()
        {
           return "FillTabBmOper71Freq20HzMeasureBase";
        }
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
            thisRangeUnits = new MeasPoint(MeasureUnits.Herz, UnitMultipliers.None, 0);
            BaseTolCoeff = (decimal)0.0001;
            EdMlRaz = 10;
            RangeResolution = new AcVariablePoint(10, MeasureUnits.Herz, UnitMultipliers.Mili);

            HerzPoint = new MeasPoint[1];
            HerzPoint[0] = new MeasPoint(thisRangeUnits.Units, thisRangeUnits.Multipliers, 100);

            VoltPoint = new AcVariablePoint[1];
            VoltPoint[0] = new AcVariablePoint((decimal)0.5, MeasureUnits.V, UnitMultipliers.None, HerzPoint);
        }

        /// <inheritdoc />
        protected override string GetReportTableName()
        {
            return "FillTabBmOper71Freq200HzMeasureBase";
        }
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
            thisRangeUnits = new MeasPoint(MeasureUnits.Herz, UnitMultipliers.Kilo, 0);
            BaseTolCoeff = (decimal)0.0001;
            EdMlRaz = 10;
            RangeResolution = new AcVariablePoint(100, MeasureUnits.Herz, UnitMultipliers.Mili);

            HerzPoint = new MeasPoint[1];
            HerzPoint[0] = new MeasPoint(thisRangeUnits.Units, thisRangeUnits.Multipliers, 1);

            VoltPoint = new AcVariablePoint[1];
            VoltPoint[0] = new AcVariablePoint((decimal)0.5, MeasureUnits.V, UnitMultipliers.None, HerzPoint);
        }

        /// <inheritdoc />
        protected override string GetReportTableName()
        {
            return "FillTabBmOper71Freq2kHzMeasureBase";
        }
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
            thisRangeUnits = new MeasPoint(MeasureUnits.Herz, UnitMultipliers.Kilo, 0);
            BaseTolCoeff = (decimal)0.0001;
            EdMlRaz = 10;
            RangeResolution = new AcVariablePoint(1, MeasureUnits.Herz, UnitMultipliers.None);

            HerzPoint = new MeasPoint[1];
            HerzPoint[0] = new MeasPoint(thisRangeUnits.Units, thisRangeUnits.Multipliers, 10);

            VoltPoint = new AcVariablePoint[1];
            VoltPoint[0] = new AcVariablePoint((decimal)0.5, MeasureUnits.V, UnitMultipliers.None, HerzPoint);
        }

        /// <inheritdoc />
        protected override string GetReportTableName()
        {
            return "FillTabBmOper71Freq20kHzMeasureBase";
        }
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
            thisRangeUnits = new MeasPoint(MeasureUnits.Herz, UnitMultipliers.Kilo, 0);
            BaseTolCoeff = (decimal)0.0001;
            EdMlRaz = 10;
            RangeResolution = new AcVariablePoint(10, MeasureUnits.Herz, UnitMultipliers.None);

            HerzPoint = new MeasPoint[1];
            HerzPoint[0] = new MeasPoint(thisRangeUnits.Units, thisRangeUnits.Multipliers, 100);

            VoltPoint = new AcVariablePoint[1];
            VoltPoint[0] = new AcVariablePoint((decimal)0.5, MeasureUnits.V, UnitMultipliers.None, HerzPoint);
        }

        /// <inheritdoc />
        protected override string GetReportTableName()
        {
           return "FillTabBmOper71Freq200kHzMeasureBase";
        }
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
            thisRangeUnits = new MeasPoint(MeasureUnits.Herz, UnitMultipliers.Mega, 0);
            BaseTolCoeff = (decimal)0.0001;
            EdMlRaz = 10;
            RangeResolution = new AcVariablePoint(100, MeasureUnits.Herz, UnitMultipliers.None);

            HerzPoint = new MeasPoint[1];
            HerzPoint[0] = new MeasPoint(thisRangeUnits.Units, thisRangeUnits.Multipliers, 1);

            VoltPoint = new AcVariablePoint[1];
            VoltPoint[0] = new AcVariablePoint((decimal)0.5, MeasureUnits.V, UnitMultipliers.None, HerzPoint);
        }

        /// <inheritdoc />
        protected override string GetReportTableName()
        {
            return "FillTabBmOper71Freq1MHzMeasureBase";
        }
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

            BaseTolCoeff = (decimal)0.003;
            EdMlRaz = 30;
            RangeResolution = new AcVariablePoint(10, MeasureUnits.Ohm, UnitMultipliers.Mili);

            thisRangeUnits = new MeasPoint(MeasureUnits.Ohm, UnitMultipliers.None, 0);

            BaseMultipliers = 1;
            OhmPoint = new MeasPoint[3];
            OhmPoint[0] = new MeasPoint(thisRangeUnits.Units, thisRangeUnits.Multipliers, 50 * BaseMultipliers);
            OhmPoint[1] = new MeasPoint(thisRangeUnits.Units, thisRangeUnits.Multipliers, 100 * BaseMultipliers);
            OhmPoint[2] = new MeasPoint(thisRangeUnits.Units, thisRangeUnits.Multipliers, 200 * BaseMultipliers);
        }

        /// <inheritdoc />
        protected override string GetReportTableName()
        {
            return "FillTabBmOper8_1Resistance_200Ohm_Meas";
        }
    }

    public class Oper8_1Resistance_2kOhm_Measure : Oper8ResistanceMeasureBase
    {
        public Oper8_1Resistance_2kOhm_Measure(Mult107_109N.RangeNominal inRangeNominal,
            IUserItemOperation userItemOperation, string inResourceDir) : base(userItemOperation, inResourceDir)
        {
            OperationOhmRangeCode = Mult107_109N.RangeCode.Range2Manual;
            OperationOhmRangeNominal = inRangeNominal;
            Name = OperationOhmRangeNominal.GetStringValue();

            thisRangeUnits = new MeasPoint(MeasureUnits.Ohm, UnitMultipliers.Kilo, 0);

            BaseTolCoeff = (decimal)0.003;
            EdMlRaz = 30;
            RangeResolution = new AcVariablePoint(100, MeasureUnits.Ohm, UnitMultipliers.Mili);

            BaseMultipliers = 1;
            OhmPoint = new MeasPoint[5];
            OhmPoint[0] = new MeasPoint(thisRangeUnits.Units, thisRangeUnits.Multipliers,
                                        (decimal)0.4 * BaseMultipliers);
            OhmPoint[1] = new MeasPoint(thisRangeUnits.Units, thisRangeUnits.Multipliers,
                                        (decimal)0.8 * BaseMultipliers);
            OhmPoint[2] = new MeasPoint(thisRangeUnits.Units, thisRangeUnits.Multipliers, 1 * BaseMultipliers);
            OhmPoint[3] = new MeasPoint(thisRangeUnits.Units, thisRangeUnits.Multipliers,
                                        (decimal)1.5 * BaseMultipliers);
            OhmPoint[4] = new MeasPoint(thisRangeUnits.Units, thisRangeUnits.Multipliers,
                                        (decimal)1.8 * BaseMultipliers);
        }

        /// <inheritdoc />
        protected override string GetReportTableName()
        {
            return "FillTabBmOper8_1Resistance_2kOhm_Meas";
        }
    }

    public class Oper8_1Resistance_20kOhm_Measure : Oper8ResistanceMeasureBase
    {
        public Oper8_1Resistance_20kOhm_Measure(Mult107_109N.RangeNominal inRangeNominal,
            IUserItemOperation userItemOperation, string inResourceDir) : base(userItemOperation, inResourceDir)
        {
            OperationOhmRangeCode = Mult107_109N.RangeCode.Range3Manual;
            OperationOhmRangeNominal = inRangeNominal;

            Name = OperationOhmRangeNominal.GetStringValue();

            thisRangeUnits = new MeasPoint(MeasureUnits.Ohm, UnitMultipliers.Kilo, 0);

            BaseTolCoeff = (decimal)0.003;
            EdMlRaz = 30;
            RangeResolution = new AcVariablePoint(1, MeasureUnits.Ohm, UnitMultipliers.None);

            BaseMultipliers = 10;
            OhmPoint = new MeasPoint[5];
            OhmPoint[0] = new MeasPoint(thisRangeUnits.Units, thisRangeUnits.Multipliers,
                                        (decimal)0.4 * BaseMultipliers);
            OhmPoint[1] = new MeasPoint(thisRangeUnits.Units, thisRangeUnits.Multipliers,
                                        (decimal)0.8 * BaseMultipliers);
            OhmPoint[2] = new MeasPoint(thisRangeUnits.Units, thisRangeUnits.Multipliers, 1 * BaseMultipliers);
            OhmPoint[3] = new MeasPoint(thisRangeUnits.Units, thisRangeUnits.Multipliers,
                                        (decimal)1.5 * BaseMultipliers);
            OhmPoint[4] = new MeasPoint(thisRangeUnits.Units, thisRangeUnits.Multipliers,
                                        (decimal)1.8 * BaseMultipliers);
        }

        /// <inheritdoc />
        protected override string GetReportTableName()
        {
            return "FillTabBmOper8_1Resistance_20kOhm_Meas";
        }
    }

    public class Oper8_1Resistance_200kOhm_Measure : Oper8ResistanceMeasureBase
    {
        public Oper8_1Resistance_200kOhm_Measure(Mult107_109N.RangeNominal inRangeNominal,
            IUserItemOperation userItemOperation, string inResourceDir) : base(userItemOperation, inResourceDir)
        {
            OperationOhmRangeCode = Mult107_109N.RangeCode.Range4Manual;
            OperationOhmRangeNominal = inRangeNominal;
            Name = OperationOhmRangeNominal.GetStringValue();

            thisRangeUnits = new MeasPoint(MeasureUnits.Ohm, UnitMultipliers.Kilo, 0);

            BaseTolCoeff = (decimal)0.003;
            EdMlRaz = 30;
            RangeResolution = new AcVariablePoint(10, MeasureUnits.Ohm, UnitMultipliers.None);

            BaseMultipliers = 100;
            OhmPoint = new MeasPoint[5];
            OhmPoint[0] = new MeasPoint(thisRangeUnits.Units, thisRangeUnits.Multipliers,
                                        (decimal)0.4 * BaseMultipliers);
            OhmPoint[1] = new MeasPoint(thisRangeUnits.Units, thisRangeUnits.Multipliers,
                                        (decimal)0.8 * BaseMultipliers);
            OhmPoint[2] = new MeasPoint(thisRangeUnits.Units, thisRangeUnits.Multipliers, 1 * BaseMultipliers);
            OhmPoint[3] = new MeasPoint(thisRangeUnits.Units, thisRangeUnits.Multipliers,
                                        (decimal)1.5 * BaseMultipliers);
            OhmPoint[4] = new MeasPoint(thisRangeUnits.Units, thisRangeUnits.Multipliers,
                                        (decimal)1.8 * BaseMultipliers);
        }

        /// <inheritdoc />
        protected override string GetReportTableName()
        {
            return "FillTabBmOper8_1Resistance_200kOhm_Meas";
        }
    }

    public class Oper8_1Resistance_2MOhm_Measure : Oper8ResistanceMeasureBase
    {
        public Oper8_1Resistance_2MOhm_Measure(Mult107_109N.RangeNominal inRangeNominal,
            IUserItemOperation userItemOperation, string inResourceDir) : base(userItemOperation, inResourceDir)
        {
            OperationOhmRangeCode = Mult107_109N.RangeCode.Range5Manual;
            OperationOhmRangeNominal = inRangeNominal;
            Name = OperationOhmRangeNominal.GetStringValue();

            thisRangeUnits = new MeasPoint(MeasureUnits.Ohm, UnitMultipliers.Mega, 0);

            BaseTolCoeff = (decimal)0.003;
            EdMlRaz = 50;
            RangeResolution = new AcVariablePoint(100, MeasureUnits.Ohm, UnitMultipliers.None);

            BaseMultipliers = 1;
            OhmPoint = new MeasPoint[5];
            OhmPoint[0] = new MeasPoint(thisRangeUnits.Units, thisRangeUnits.Multipliers,
                                        (decimal)0.4 * BaseMultipliers);
            OhmPoint[1] = new MeasPoint(thisRangeUnits.Units, thisRangeUnits.Multipliers,
                                        (decimal)0.8 * BaseMultipliers);
            OhmPoint[2] = new MeasPoint(thisRangeUnits.Units, thisRangeUnits.Multipliers, 1 * BaseMultipliers);
            OhmPoint[3] = new MeasPoint(thisRangeUnits.Units, thisRangeUnits.Multipliers,
                                        (decimal)1.5 * BaseMultipliers);
            OhmPoint[4] = new MeasPoint(thisRangeUnits.Units, thisRangeUnits.Multipliers,
                                        (decimal)1.8 * BaseMultipliers);
        }

        /// <inheritdoc />
        protected override string GetReportTableName()
        {
            return "FillTabBmOper8_1Resistance_2MOhm_Meas";
        }
    }

    public class Oper8_1Resistance_20MOhm_Measure : Oper8ResistanceMeasureBase
    {
        public Oper8_1Resistance_20MOhm_Measure(Mult107_109N.RangeNominal inRangeNominal,
            IUserItemOperation userItemOperation, string inResourceDir) : base(userItemOperation, inResourceDir)
        {
            OperationOhmRangeCode = Mult107_109N.RangeCode.Range6Manual;
            OperationOhmRangeNominal = inRangeNominal;
            Name = OperationOhmRangeNominal.GetStringValue();

            thisRangeUnits = new MeasPoint(MeasureUnits.Ohm, UnitMultipliers.Mega, 0);
            BaseTolCoeff = (decimal)0.05;
            EdMlRaz = 50;
            RangeResolution = new AcVariablePoint(1, MeasureUnits.Ohm, UnitMultipliers.Kilo);

            BaseMultipliers = 1;
            OhmPoint = new MeasPoint[3];
            OhmPoint[0] = new MeasPoint(thisRangeUnits.Units, thisRangeUnits.Multipliers, 5 * BaseMultipliers);
            OhmPoint[1] = new MeasPoint(thisRangeUnits.Units, thisRangeUnits.Multipliers, 10 * BaseMultipliers);
            OhmPoint[2] = new MeasPoint(thisRangeUnits.Units, thisRangeUnits.Multipliers, 20 * BaseMultipliers);
        }

        /// <inheritdoc />
        protected override string GetReportTableName()
        {
            return "FillTabBmOper8_1Resistance_20MOhm_Meas";
        }
    }

    public class Oper8_1Resistance_200MOhm_Measure : Oper8ResistanceMeasureBase
    {
        public Oper8_1Resistance_200MOhm_Measure(Mult107_109N.RangeNominal inRangeNominal,
            IUserItemOperation userItemOperation, string inResourceDir) : base(userItemOperation, inResourceDir)
        {
            OperationOhmRangeCode = Mult107_109N.RangeCode.Range7Manual;
            OperationOhmRangeNominal = inRangeNominal;
            Name = OperationOhmRangeNominal.GetStringValue();

            thisRangeUnits = new MeasPoint(MeasureUnits.Ohm, UnitMultipliers.Mega, 0);

            BaseTolCoeff = (decimal)0.05;
            EdMlRaz = 20;
            RangeResolution = new AcVariablePoint(1, MeasureUnits.Ohm, UnitMultipliers.Mega);

            BaseMultipliers = 10;
            OhmPoint = new MeasPoint[3];
            OhmPoint[0] = new MeasPoint(thisRangeUnits.Units, thisRangeUnits.Multipliers, 5 * BaseMultipliers);
            OhmPoint[1] = new MeasPoint(thisRangeUnits.Units, thisRangeUnits.Multipliers, 10 * BaseMultipliers);
            OhmPoint[2] = new MeasPoint(thisRangeUnits.Units, thisRangeUnits.Multipliers, 20 * BaseMultipliers);
        }

        /// <inheritdoc />
        protected override string GetReportTableName()
        {
            return "FillTabBmOper8_1Resistance_200MOhm_Meas";
        }
    }

    public class Oper8_1Resistance_2GOhm_Measure : Oper8ResistanceMeasureBase
    {
        public Oper8_1Resistance_2GOhm_Measure(Mult107_109N.RangeNominal inRangeNominal,
            IUserItemOperation userItemOperation, string inResourceDir) : base(userItemOperation, inResourceDir)
        {
            OperationOhmRangeCode = Mult107_109N.RangeCode.Range8Manual;
            OperationOhmRangeNominal = inRangeNominal;
            Name = OperationOhmRangeNominal.GetStringValue();

            thisRangeUnits = new MeasPoint(MeasureUnits.Ohm, UnitMultipliers.Giga, 0);

            BaseTolCoeff = (decimal)0.05;
            EdMlRaz = 8;
            RangeResolution = new AcVariablePoint(100, MeasureUnits.Ohm, UnitMultipliers.Mega);

            OhmPoint = new MeasPoint[1];
            OhmPoint[0] = new MeasPoint(thisRangeUnits.Units, thisRangeUnits.Multipliers, (decimal)0.9);
        }

        /// <inheritdoc />
        protected override string GetReportTableName()
        {
            return "FillTabBmOper8_1Resistance_2GOhm_Meas";
        }
    }

    public class Oper8ResistanceMeasureBase : ParagraphBase<MeasPoint>
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        #region Fields

        //множители для пределов.
        public decimal BaseMultipliers;

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
        protected MeasPoint[] OhmPoint;

        protected AcVariablePoint RangeResolution;


        /// <summary>
        /// Это пустая точка, которая содержит только единицы измерения текущего
        /// поверяемого предела и множитель для единицы измерения.
        /// </summary>
        protected MeasPoint thisRangeUnits;

        #endregion Fields

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

        #endregion Property

        public Oper8ResistanceMeasureBase(IUserItemOperation userItemOperation, string inResourceDir) : base(userItemOperation)
        {
            Name = "Определение погрешности измерения электрического сопротивления";
            OperMeasureMode = Mult107_109N.MeasureMode.Ohm;

            OperationOhmRangeCode = Mult107_109N.RangeCode.Range1Manual;
            OperationOhmRangeNominal = Mult107_109N.RangeNominal.RangeNone;

            DataRow = new List<IBasicOperation<MeasPoint>>();
            Sheme = ShemeTemplateDefault.TemplateSheme;
            Sheme.AssemblyLocalName = inResourceDir;
        }

        #region Methods

  
        /// <inheritdoc />
        protected override string[] GenerateDataColumnTypeObject()
        {
            return new[] {"Предел измерения",
                "Поверяемая точка", "Измеренное значение", "Минимальное допустимое значение", "Максимальное допустимое значение" }.Concat(base.GenerateDataColumnTypeObject()).ToArray(); ;
        }
        /// <inheritdoc />
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
                var dds = row as BasicOperationVerefication<MeasPoint>;
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

        protected override void InitWork()
        {
            base.InitWork();
            if (appa107N == null || flkCalib5522A == null) return;

            foreach (var currPoint in OhmPoint)
            {
                var operation = new BasicOperationVerefication<MeasPoint>();
                operation.InitWork = async () =>
                {
                    try
                    {
                        if (appa107N.StringConnection.Equals("COM1"))
                            appa107N.StringConnection = GetStringConnect(appa107N);
                        flkCalib5522A.StringConnection ??= GetStringConnect(flkCalib5522A);

                        await Task.Run(() => { flkCalib5522A.Out.SetOutput(CalibrMain.COut.State.Off); });

                        while (OperMeasureMode != await Task<Mult107_109N.MeasureMode>.Factory.StartNew(() => appa107N.GetMeasureMode))
                            UserItemOperation.ServicePack.MessageBox()
                                             .Show($"Установите режим измерения: {OperMeasureMode.GetStringValue()} {OperMeasureMode}",
                                                   "Указание оператору", MessageButton.OK,
                                                   MessageIcon.Information,
                                                   MessageResult.OK);

                        while (await Task<Mult107_109N.RangeSwitchMode>.Factory.StartNew(() => appa107N.GetRangeSwitchMode) == Mult107_109N.RangeSwitchMode.Auto)
                            UserItemOperation.ServicePack.MessageBox()
                                             .Show("Установите ручной режим переключения пределов.");

                        while (OperationOhmRangeNominal != await Task<Mult107_109N.RangeNominal>.Factory.StartNew(() => appa107N.GetRangeNominal))
                        {
                            int countPushRangeButton;

                            if (thisRangeUnits.Multipliers == UnitMultipliers.Mili)
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
                                var curRange = (int)appa107N.GetRangeCode - 127;
                                var targetRange = (int)OperationOhmRangeCode - 127;
                                countPushRangeButton = Hepls.CountOfPushButton(CountOfRanges, curRange, targetRange);

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
                operation.BodyWork = () =>
                {
                    try
                    {
                        // компенсаци проводов на младших пределах
                        decimal refValue = 0;
                        if (thisRangeUnits.Multipliers == UnitMultipliers.None)
                        {
                            flkCalib5522A.Out.Set.Resistance.SetValue(0);
                            flkCalib5522A.Out.SetOutput(CalibrMain.COut.State.On);
                            Thread.Sleep(3000);
                            //измеряем
                            refValue = (decimal)appa107N.GetValue();
                        }

                        flkCalib5522A.Out.Set.Resistance.SetValue(currPoint.Value *
                                                                  (decimal)currPoint
                                                                           .Multipliers.GetDoubleValue());
                        flkCalib5522A.Out.SetOutput(CalibrMain.COut.State.On);

                        if (thisRangeUnits.Multipliers == UnitMultipliers.Mega) Thread.Sleep(9000);
                        else if (thisRangeUnits.Multipliers == UnitMultipliers.Giga) Thread.Sleep(12000);
                        else
                            Thread.Sleep(3000);
                        //измеряем
                        var measurePoint = (decimal)appa107N.GetValue() - refValue;

                        flkCalib5522A.Out.SetOutput(CalibrMain.COut.State.Off);

                        var mantisa =
                            MathStatistics.GetMantissa((decimal)(RangeResolution
                                                                 .MainPhysicalQuantity.Multipliers
                                                                 .GetDoubleValue() *
                                                                  (double)RangeResolution
                                                                          .MainPhysicalQuantity.Value /
                                                                  currPoint.Multipliers
                                                                           .GetDoubleValue()));
                        //округляем измерения
                        MathStatistics.Round(ref measurePoint, mantisa);

                        operation.Getting =
                            new MeasPoint(thisRangeUnits.Units, thisRangeUnits.Multipliers, measurePoint);
                        operation.Expected = currPoint;
                        //расчет погрешности для конкретной точки предела измерения
                        operation.ErrorCalculation = (inA, inB) =>
                        {
                            var result = BaseTolCoeff * Math.Abs(operation.Expected.Value) + EdMlRaz *
                                RangeResolution.MainPhysicalQuantity.Value *
                                (decimal)(RangeResolution
                                          .MainPhysicalQuantity.Multipliers.GetDoubleValue() /
                                           currPoint.Multipliers
                                                    .GetDoubleValue());
                            var mantisa =
                                MathStatistics.GetMantissa((decimal)(RangeResolution
                                                                     .MainPhysicalQuantity.Multipliers
                                                                     .GetDoubleValue() *
                                                                      (double)RangeResolution
                                                                              .MainPhysicalQuantity.Value /
                                                                      currPoint.Multipliers
                                                                               .GetDoubleValue()));
                            MathStatistics.Round(ref result, mantisa);
                            return new MeasPoint(thisRangeUnits.Units, thisRangeUnits.Multipliers, result);
                        };

                        operation.LowerTolerance = new MeasPoint(thisRangeUnits.Units, thisRangeUnits.Multipliers,
                                                                 operation.Expected.Value -
                                                                 operation.Error.Value);
                        operation.UpperTolerance = new MeasPoint(thisRangeUnits.Units, thisRangeUnits.Multipliers,
                                                                 operation.Expected.Value +
                                                                 operation.Error.Value);
                        operation.IsGood = () =>
                        {
                            if (operation.Expected == null || operation.Getting == null ||
                                operation.LowerTolerance == null || operation.UpperTolerance == null) return false;

                            return (operation.Getting.Value < operation.UpperTolerance.Value) &
                                   (operation.Getting.Value > operation.LowerTolerance.Value);
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
                                : (BasicOperationVerefication<MeasPoint>)operation.Clone());
            }
        }

        #endregion Methods

      

        //public override async Task StartWork(CancellationToken token)
        //{
        //    await base.StartWork(token);
        //    appa107N?.Dispose();
        //}
    }

    #endregion OHM

    //////////////////////////////******FAR*******///////////////////////////////

    #region FAR

    public class Oper9FarMeasureBase : ParagraphBase<MeasPoint>
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        #region Fields

        //множители для пределов.
        public decimal BaseMultipliers;

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
        protected MeasPoint[] FarMeasPoints;

        protected AcVariablePoint RangeResolution;


        /// <summary>
        /// Это пустая точка, которая содержит только единицы измерения текущего
        /// поверяемого предела и множитель для единицы измерения.
        /// </summary>
        protected MeasPoint thisRangeUnits;

        #endregion Fields

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

        #endregion Property

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

     

        /// <inheritdoc />
        protected override string[] GenerateDataColumnTypeObject()
        {
            return new[] {"Предел измерения",
                "Поверяемая точка", "Измеренное значение", "Минимальное допустимое значение", "Максимальное допустимое значение" }.Concat(base.GenerateDataColumnTypeObject()).ToArray(); ;
        }
        /// <inheritdoc />
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
                var dds = row as BasicOperationVerefication<MeasPoint>;
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

        protected override void InitWork()
        {
            base.InitWork();
            if (appa107N == null || flkCalib5522A == null) return;

          
            foreach (var currPoint in FarMeasPoints)
            {
                var operation = new BasicOperationVerefication<MeasPoint>();
                operation.InitWork = async () =>
                {
                    try
                    {
                        if (appa107N.StringConnection.Equals("COM1"))
                            appa107N.StringConnection = GetStringConnect(appa107N);
                        flkCalib5522A.StringConnection ??= GetStringConnect(flkCalib5522A);

                        await Task.Run(() => { flkCalib5522A.Out.SetOutput(CalibrMain.COut.State.Off); });

                        while (OperMeasureMode != await Task<Mult107_109N.MeasureMode>.Factory.StartNew(() => appa107N.GetMeasureMode))
                            UserItemOperation.ServicePack.MessageBox()
                                             .Show($"Установите режим измерения: {OperMeasureMode.GetStringValue()} {OperMeasureMode}",
                                                   "Указание оператору", MessageButton.OK,
                                                   MessageIcon.Information,
                                                   MessageResult.OK);

                        while (await Task<Mult107_109N.RangeSwitchMode>.Factory.StartNew(() => appa107N.GetRangeSwitchMode) == Mult107_109N.RangeSwitchMode.Auto)
                            UserItemOperation.ServicePack.MessageBox()
                                             .Show("Установите ручной режим переключения пределов.");

                        while (OperationRangeNominal != await Task<Mult107_109N.RangeNominal>.Factory.StartNew(() => appa107N.GetRangeNominal))
                        {
                            int countPushRangeButton;
                            CountOfRanges = 8;
                            var curRange = (int)appa107N.GetRangeCode - 127;
                            var targetRange = (int)OperationRangeCode - 127;
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
                operation.BodyWork = () =>
                {
                    try
                    {
                        flkCalib5522A.Out.Set.Capacitance.SetValue(currPoint.Value *
                                                                   (decimal)currPoint
                                                                            .Multipliers.GetDoubleValue());
                        flkCalib5522A.Out.SetOutput(CalibrMain.COut.State.On);
                        if (thisRangeUnits.Multipliers == UnitMultipliers.Mili &&
                            appa107N.GetRangeNominal == Mult107_109N.RangeNominal.Range40mF)
                            Thread.Sleep(90000);
                        else if (thisRangeUnits.Multipliers == UnitMultipliers.Mili &&
                                 appa107N.GetRangeNominal == Mult107_109N.RangeNominal.Range4mF) Thread.Sleep(12000);
                        else
                            Thread.Sleep(4000);

                        //измеряем
                        var measurePoint = (decimal)appa107N.GetSingleValue();
                        flkCalib5522A.Out.SetOutput(CalibrMain.COut.State.Off);

                        var mantisa =
                            MathStatistics.GetMantissa((decimal)(RangeResolution
                                                                 .MainPhysicalQuantity.Multipliers
                                                                 .GetDoubleValue() /
                                                                  currPoint.Multipliers
                                                                           .GetDoubleValue()));
                        //округляем измерения
                        MathStatistics.Round(ref measurePoint, mantisa);

                        operation.Getting =
                            new MeasPoint(thisRangeUnits.Units, thisRangeUnits.Multipliers, measurePoint);
                        operation.Expected = currPoint;
                        //расчет погрешности для конкретной точки предела измерения
                        operation.ErrorCalculation = (inA, inB) =>
                        {
                            var result = BaseTolCoeff * Math.Abs(operation.Expected.Value) + EdMlRaz *
                                RangeResolution.MainPhysicalQuantity.Value *
                                (decimal)(RangeResolution
                                          .MainPhysicalQuantity.Multipliers.GetDoubleValue() /
                                           currPoint.Multipliers
                                                    .GetDoubleValue());
                            var mantisa =
                                MathStatistics.GetMantissa((decimal)(RangeResolution
                                                                     .MainPhysicalQuantity.Multipliers
                                                                     .GetDoubleValue() /
                                                                      currPoint.Multipliers
                                                                               .GetDoubleValue()));
                            //округляем измерения
                            MathStatistics.Round(ref measurePoint, mantisa);
                            return new MeasPoint(thisRangeUnits.Units, thisRangeUnits.Multipliers, result);
                        };

                        operation.LowerTolerance = new MeasPoint(thisRangeUnits.Units, thisRangeUnits.Multipliers,
                                                                 operation.Expected.Value -
                                                                 operation.Error.Value);
                        operation.UpperTolerance = new MeasPoint(thisRangeUnits.Units, thisRangeUnits.Multipliers,
                                                                 operation.Expected.Value +
                                                                 operation.Error.Value);
                        operation.IsGood = () =>
                        {
                            if (operation.Expected == null || operation.Getting == null ||
                                operation.LowerTolerance == null || operation.UpperTolerance == null) return false;
                            return (operation.Getting.Value < operation.UpperTolerance.Value) &
                                   (operation.Getting.Value > operation.LowerTolerance.Value);
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
                                : (BasicOperationVerefication<MeasPoint>)operation.Clone());
            }
        }

        #endregion Methods

      

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
            BaseTolCoeff = (decimal)0.015;
            EdMlRaz = 10;
            RangeResolution = new AcVariablePoint(1, MeasureUnits.Far, UnitMultipliers.Pico);

            Name = OperationRangeNominal.GetStringValue();
            thisRangeUnits = new MeasPoint(MeasureUnits.Far, UnitMultipliers.Nano, 0);

            FarMeasPoints = new MeasPoint[1];
            FarMeasPoints[0] = new MeasPoint(thisRangeUnits.Units, thisRangeUnits.Multipliers, 3);
            Sheme = ShemeTemplateDefault.TemplateSheme;
            Sheme.AssemblyLocalName = inResourceDir;
        }

        /// <inheritdoc />
        protected override string GetReportTableName()
        {
            return "FillTabBmOper9_1Far_4nF_Measure";
        }
    }

    public class Oper9_1Far_40nF_Measure : Oper9FarMeasureBase
    {
        public Oper9_1Far_40nF_Measure(Mult107_109N.RangeNominal inRangeNominal,
            IUserItemOperation userItemOperation) :
            base(userItemOperation)
        {

            OperationRangeCode = Mult107_109N.RangeCode.Range2Manual;
            OperationRangeNominal = inRangeNominal;
            RangeResolution = new AcVariablePoint(10, MeasureUnits.Far, UnitMultipliers.Pico);
            BaseTolCoeff = (decimal)0.015;
            EdMlRaz = 10;
            Name = OperationRangeNominal.GetStringValue();
            thisRangeUnits = new MeasPoint(MeasureUnits.Far, UnitMultipliers.Nano, 0);

            FarMeasPoints = new MeasPoint[1];
            FarMeasPoints[0] = new MeasPoint(thisRangeUnits.Units, thisRangeUnits.Multipliers, 30);
        }

        /// <inheritdoc />
        protected override string GetReportTableName()
        {
            return "FillTabBmOper9_1Far_40nF_Measure";
        }
    }

    public class Oper9_1Far_400nF_Measure : Oper9FarMeasureBase
    {
        public Oper9_1Far_400nF_Measure(Mult107_109N.RangeNominal inRangeNominal,
            IUserItemOperation userItemOperation)
            : base(userItemOperation)
        {
            OperationRangeCode = Mult107_109N.RangeCode.Range3Manual;
            OperationRangeNominal = inRangeNominal;
            RangeResolution = new AcVariablePoint(100, MeasureUnits.Far, UnitMultipliers.Pico);
            BaseTolCoeff = (decimal)0.009;
            EdMlRaz = 5;
            Name = OperationRangeNominal.GetStringValue();
            thisRangeUnits = new MeasPoint(MeasureUnits.Far, UnitMultipliers.Nano, 0);

            BaseTolCoeff = (decimal)0.009;
            EdMlRaz = 5;

            FarMeasPoints = new MeasPoint[1];
            FarMeasPoints[0] = new MeasPoint(thisRangeUnits.Units, thisRangeUnits.Multipliers, 300);
        }

        /// <inheritdoc />
        protected override string GetReportTableName()
        {
            return "FillTabBmOper9_1Far_400nF_Measure";
        }
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
            thisRangeUnits = new MeasPoint(MeasureUnits.Far, UnitMultipliers.Micro, 0);

            BaseTolCoeff = (decimal)0.009;
            EdMlRaz = 5;
            RangeResolution = new AcVariablePoint(1, MeasureUnits.Far, UnitMultipliers.Nano);
            BaseTolCoeff = (decimal)0.009;
            EdMlRaz = 5;

            FarMeasPoints = new MeasPoint[1];
            FarMeasPoints[0] = new MeasPoint(thisRangeUnits.Units, thisRangeUnits.Multipliers, 3);
        }

        /// <inheritdoc />
        protected override string GetReportTableName()
        {
            return "FillTabBmOper9_1Far_4uF_Measure";
        }
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
            thisRangeUnits = new MeasPoint(MeasureUnits.Far, UnitMultipliers.Micro, 0);

            BaseTolCoeff = (decimal)0.012;
            EdMlRaz = 5;
            RangeResolution = new AcVariablePoint(10, MeasureUnits.Far, UnitMultipliers.Nano);

            FarMeasPoints = new MeasPoint[1];
            FarMeasPoints[0] = new MeasPoint(thisRangeUnits.Units, thisRangeUnits.Multipliers, 30);
        }

        /// <inheritdoc />
        protected override string GetReportTableName()
        {
            return "FillTabBmOper9_1Far_40uF_Measure";
        }
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
            thisRangeUnits = new MeasPoint(MeasureUnits.Far, UnitMultipliers.Micro, 0);

            BaseTolCoeff = (decimal)0.012;
            EdMlRaz = 5;
            RangeResolution = new AcVariablePoint(100, MeasureUnits.Far, UnitMultipliers.Nano);

            FarMeasPoints = new MeasPoint[1];
            FarMeasPoints[0] = new MeasPoint(thisRangeUnits.Units, thisRangeUnits.Multipliers, 300);
        }

        /// <inheritdoc />
        protected override string GetReportTableName()
        {
            return "FillTabBmOper9_1Far_400uF_Measure";
        }
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
            thisRangeUnits = new MeasPoint(MeasureUnits.Far, UnitMultipliers.Mili, 0);

            BaseTolCoeff = (decimal)0.015;
            EdMlRaz = 5;
            RangeResolution = new AcVariablePoint(1, MeasureUnits.Far, UnitMultipliers.Micro);

            FarMeasPoints = new MeasPoint[1];
            FarMeasPoints[0] = new MeasPoint(thisRangeUnits.Units, thisRangeUnits.Multipliers, 3);
        }

        /// <inheritdoc />
        protected override string GetReportTableName()
        {
            return "FillTabBmOper9_1Far_4mF_Measure";
        }
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
            thisRangeUnits = new MeasPoint(MeasureUnits.Far, UnitMultipliers.Mili, 0);

            BaseTolCoeff = (decimal)0.015;
            EdMlRaz = 5;
            RangeResolution = new AcVariablePoint(10, MeasureUnits.Far, UnitMultipliers.Micro);

            FarMeasPoints = new MeasPoint[1];
            FarMeasPoints[0] = new MeasPoint(thisRangeUnits.Units, thisRangeUnits.Multipliers, 30);
        }

        /// <inheritdoc />
        protected override string GetReportTableName()
        {
            return "FillTabBmOper9_1Far_40mF_Measure";
        }
    }

    #endregion FAR

    //////////////////////////////******TEMP*******///////////////////////////////

    #region TEMP

    public class Oper10TemperatureMeasureBase : ParagraphBase<MeasPoint>
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        #region Fields

        //множители для пределов.
        public decimal BaseMultipliers;

        /// <summary>
        /// Процент от измеряемой точки для расчета погрешности (уже переведе в абсолютные единицы).
        /// </summary>
        public decimal BaseTolCoeff = (decimal)0.0006;

        /// <summary>
        /// Число пределов измерения.
        /// </summary>
        private int CountOfRanges;

        /// <summary>
        /// Массив поверяемых точек напряжения.
        /// </summary>
        protected MeasPoint[] DegC_Point;

        /// <summary>
        /// довесок к формуле погрешности- число единиц младшего разряда
        /// </summary>
        public int EdMlRaz;

        /// <summary>
        /// Разарешение пределеа измерения (последний значащий разряд)
        /// </summary>
        protected AcVariablePoint RangeResolution;

        

        /// <summary>
        /// Это пустая точка, которая содержит только единицы измерения текущего
        /// поверяемого предела и множитель для единицы измерения.
        /// </summary>
        protected MeasPoint thisRangeUnits;

        #endregion Fields

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

        #endregion Property

        public Oper10TemperatureMeasureBase(IUserItemOperation userItemOperation) : base(userItemOperation)
        {
            Name = "Определение погрешности измерения температуры, градусы Цельсия";
            OperMeasureMode = Mult107_109N.MeasureMode.degC;

            OperationRangeCode = Mult107_109N.RangeCode.Range1Manual;
            OperationRangeNominal = Mult107_109N.RangeNominal.RangeNone;

            DataRow = new List<IBasicOperation<MeasPoint>>();
            Sheme = new ShemeImage
            {
                AssemblyLocalName = Assembly.GetExecutingAssembly().GetName().Name,
                Description = "Измерительная схема",
                Number = 5,
                FileName = @"appa_10XN_Temp_5522.jpg",
                ExtendedDescription = "Соберите измерительную схему, согласно рисунку"
            };
        }

        #region Methods

    
        /// <inheritdoc />
        protected override string[] GenerateDataColumnTypeObject()
        {
            return new[] {"Предел измерения",
                "Поверяемая точка", "Измеренное значение", "Минимальное допустимое значение", "Максимальное допустимое значение" }.Concat(base.GenerateDataColumnTypeObject()).ToArray();
        }
        /// <inheritdoc />
        protected override string GetReportTableName()
        {
            return null;
        }


        protected override DataTable FillData()
        {
            var dataTable= base.FillData();

            foreach (var row in DataRow)
            {
                var dataRow = dataTable.NewRow();
                var dds = row as BasicOperationVerefication<MeasPoint>;
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

        protected override void InitWork()
        {
            base.InitWork();
            if (appa107N == null || flkCalib5522A == null) return;

            foreach (var currPoint in DegC_Point)
            {
                var operation = new BasicOperationVerefication<MeasPoint>();
                operation.InitWork = async () =>
                {
                    try
                    {
                        if (appa107N.StringConnection.Equals("COM1"))
                            appa107N.StringConnection = GetStringConnect(appa107N);
                        flkCalib5522A.StringConnection ??= GetStringConnect(flkCalib5522A);

                        await Task.Run(() => { flkCalib5522A.Out.SetOutput(CalibrMain.COut.State.Off); });

                        while (OperMeasureMode != await Task<Mult107_109N.MeasureMode>.Factory.StartNew(() => appa107N.GetMeasureMode))
                            UserItemOperation.ServicePack.MessageBox()
                                             .Show($"Установите режим измерения: {OperMeasureMode.GetStringValue()} {OperMeasureMode}",
                                                   "Указание оператору", MessageButton.OK,
                                                   MessageIcon.Information,
                                                   MessageResult.OK);

                        while (await Task<Mult107_109N.RangeSwitchMode>.Factory.StartNew(() => appa107N.GetRangeSwitchMode) == Mult107_109N.RangeSwitchMode.Auto)
                            UserItemOperation.ServicePack.MessageBox()
                                             .Show("Установите ручной режим переключения пределов.");

                        while (OperationRangeNominal != await Task<Mult107_109N.RangeNominal>.Factory.StartNew(() => appa107N.GetRangeNominal))
                        {
                            int countPushRangeButton;

                            if (thisRangeUnits.Multipliers == UnitMultipliers.Mili)
                            {
                                CountOfRanges = 2;
                                UserItemOperation.ServicePack.MessageBox()
                                                 .Show($"Текущий предел измерения прибора {appa107N.GetRangeNominal.GetStringValue()}\n Необходимо установить предел {OperationRangeNominal.GetStringValue()} " +
                                                       $"Нажмите на приборе клавишу Range {countPushRangeButton = 1} раз.",
                                                       "Указание оператору", MessageButton.OK, MessageIcon.Information,
                                                       MessageResult.OK);
                            }
                            else
                            {
                                //работает только для ручного режима переключения пределов
                                CountOfRanges = 2;
                                var curRange = (int)appa107N.GetRangeCode - 127;
                                var targetRange = (int)OperationRangeCode - 127;
                                countPushRangeButton = Hepls.CountOfPushButton(CountOfRanges, curRange, targetRange);

                                UserItemOperation.ServicePack.MessageBox()
                                                 .Show($"Текущий предел измерения прибора {appa107N.GetRangeNominal.GetStringValue()}\n Необходимо установить предел {OperationRangeNominal.GetStringValue()} " +
                                                       $"Нажмите на приборе клавишу Range {countPushRangeButton} раз.",
                                                       "Указание оператору", MessageButton.OK, MessageIcon.Information,
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
                operation.BodyWork = () =>
                {
                    try
                    {
                        flkCalib5522A.Out.Set.Temperature.SetTermoCouple(CalibrMain.COut.CSet.СTemperature
                                                                                   .TypeTermocouple.K);
                        flkCalib5522A.Out.Set.Temperature.SetValue((double)currPoint.Value);
                        flkCalib5522A.Out.SetOutput(CalibrMain.COut.State.On);
                        Thread.Sleep(3000);
                        //измеряем
                        var measurePoint = (decimal)appa107N.GetValue();

                        flkCalib5522A.Out.SetOutput(CalibrMain.COut.State.Off);

                        var mantisa =
                            MathStatistics.GetMantissa((decimal)(RangeResolution
                                                                 .MainPhysicalQuantity.Multipliers
                                                                 .GetDoubleValue() *
                                                                  (double)RangeResolution
                                                                          .MainPhysicalQuantity.Value /
                                                                  currPoint.Multipliers.GetDoubleValue()), true);
                        //округляем измерения
                        MathStatistics.Round(ref measurePoint, mantisa);

                        operation.Getting =
                            new MeasPoint(thisRangeUnits.Units, thisRangeUnits.Multipliers, measurePoint);
                        operation.Expected = currPoint;
                        //расчет погрешности для конкретной точки предела измерения
                        operation.ErrorCalculation = (inA, inB) =>
                        {
                            var result = BaseTolCoeff * operation.Expected.Value + EdMlRaz *
                                RangeResolution.MainPhysicalQuantity.Value *
                                (decimal)(RangeResolution
                                          .MainPhysicalQuantity.Multipliers.GetDoubleValue() /
                                           currPoint.Multipliers.GetDoubleValue()
                                );

                            MathStatistics.Round(ref result, mantisa);
                            return new MeasPoint(thisRangeUnits.Units, thisRangeUnits.Multipliers, result);
                        };

                        operation.LowerTolerance = new MeasPoint(thisRangeUnits.Units, thisRangeUnits.Multipliers,
                                                                 operation.Expected.Value -
                                                                 operation.Error.Value);
                        operation.UpperTolerance = new MeasPoint(thisRangeUnits.Units, thisRangeUnits.Multipliers,
                                                                 operation.Expected.Value +
                                                                 operation.Error.Value);
                        operation.IsGood = () =>
                        {
                            if (operation.Expected == null || operation.Getting == null ||
                                operation.LowerTolerance == null || operation.UpperTolerance == null) return false;
                            return (operation.Getting.Value < operation.UpperTolerance.Value) &
                                   (operation.Getting.Value > operation.LowerTolerance.Value);
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
                                : (BasicOperationVerefication<MeasPoint>)operation.Clone());
            }
        }

        #endregion Methods

      

        //public override async Task StartWork(CancellationToken token)
        //{
        //    await base.StartWork(token);
        //    appa107N?.Dispose();
        //}
    }

    public class Oper10_1Temperature_Minus200_Minus100_Measure : Oper10TemperatureMeasureBase
    {
        public Oper10_1Temperature_Minus200_Minus100_Measure(Mult107_109N.RangeNominal inRangeNominal,
            IUserItemOperation userItemOperation) :
            base(userItemOperation)
        {
          
            OperationRangeCode = Mult107_109N.RangeCode.Range1Manual;
            OperationRangeNominal = inRangeNominal;
            RangeResolution = new AcVariablePoint(100, MeasureUnits.degC, UnitMultipliers.Mili);
            Name = "-200 ⁰C ... -100 ⁰C";
            thisRangeUnits = new MeasPoint(MeasureUnits.degC, UnitMultipliers.None, 0);

            BaseTolCoeff = (decimal)0.001;
            EdMlRaz = 60;

            DegC_Point = new[] { new MeasPoint(thisRangeUnits.Units, thisRangeUnits.Multipliers, -200) };
        }

        /// <inheritdoc />
        protected override string GetReportTableName()
        {
            return "FillTabBmOper10_1Tem_Minus200_Minus100";
        }
    }

    public class Oper10_1Temperature_Minus100_400_Measure : Oper10TemperatureMeasureBase
    {
        public Oper10_1Temperature_Minus100_400_Measure(Mult107_109N.RangeNominal inRangeNominal,
            IUserItemOperation userItemOperation) :
            base(userItemOperation)
        {
            OperationRangeCode = Mult107_109N.RangeCode.Range2Manual;
            OperationRangeNominal = inRangeNominal;
            RangeResolution = new AcVariablePoint(100, MeasureUnits.degC, UnitMultipliers.Mili);
            Name = "-100 ⁰C ... 400 ⁰C";
            thisRangeUnits = new MeasPoint(MeasureUnits.degC, UnitMultipliers.None, 0);

            BaseTolCoeff = (decimal)0.001;
            EdMlRaz = 30;

            DegC_Point = new[]
            {
                new MeasPoint(thisRangeUnits.Units, thisRangeUnits.Multipliers, -100),
                new MeasPoint(thisRangeUnits.Units, thisRangeUnits.Multipliers, 0),
                new MeasPoint(thisRangeUnits.Units, thisRangeUnits.Multipliers, 100)
            };
        }

        /// <inheritdoc />
        protected override string GetReportTableName()
        {
            return "FillTabBmOper10_1Tem_Minus100_400";
        }
    }

    public class Oper10_1Temperature_400_1200_Measure : Oper10TemperatureMeasureBase
    {
        public Oper10_1Temperature_400_1200_Measure(Mult107_109N.RangeNominal inRangeNominal,
            IUserItemOperation userItemOperation) :
            base(userItemOperation)
        {
          
            OperationRangeCode = Mult107_109N.RangeCode.Range2Manual;
            OperationRangeNominal = inRangeNominal;
            RangeResolution = new AcVariablePoint(1, MeasureUnits.degC, UnitMultipliers.None);
            Name = "400 ⁰C ... 1200 ⁰C";
            thisRangeUnits = new MeasPoint(MeasureUnits.degC, UnitMultipliers.None, 0);

            BaseTolCoeff = (decimal)0.001;
            EdMlRaz = 3;

            DegC_Point = new[]
            {
                new MeasPoint(thisRangeUnits.Units, thisRangeUnits.Multipliers, 500),
                new MeasPoint(thisRangeUnits.Units, thisRangeUnits.Multipliers, 800),
                new MeasPoint(thisRangeUnits.Units, thisRangeUnits.Multipliers, 1200)
            };
        }

        /// <inheritdoc />
        protected override string GetReportTableName()
        {
            return "FillTabBmOper10_1Tem_400_1200";
        }
    }

    #endregion TEMP
}