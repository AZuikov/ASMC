using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AP.Extension;
using AP.Reports.Utils;
using AP.Utils.Data;
using ASMC.Common.Helps;
using ASMC.Core.Model;
using ASMC.Data.Model;
using ASMC.Data.Model.PhysicalQuantity;
using ASMC.Devices.Interface.SourceAndMeter;
using NLog;

namespace Belvar_V7_40_1
{
    /// <summary>
    /// Предоставляет реализацию внешнего осномотра.
    /// </summary>
    public sealed class VisualInspection : OperationBase<bool>
    {
        /// <inheritdoc />
        public VisualInspection(IUserItemOperation userItemOperation) : base(userItemOperation)
        {
            Name = "Внешний осмотр";
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
                dataRow[0] = dds.Getting ? "соответствует требованиям" : dds.Comment;
                data.Rows.Add(dataRow);
            }

            return data;
        }

        /// <inheritdoc />
        protected override string GetReportTableName()
        {
            return MarkReportEnum.InsetrTextByMark.GetStringValue() + GetType().Name;
        }

        /// <inheritdoc />
        protected override void InitWork(CancellationTokenSource token)
        {
            base.InitWork(token);
            DataRow.Add(new DialogOperationHelp(this, "V740_1_VisualTest.rtf"));
        }

        #endregion
    }

    /// <summary>
    /// Предоставляет операцию опробывания.
    /// </summary>
    public sealed class Testing : OperationBase<bool>
    {
        /// <inheritdoc />
        public Testing(IUserItemOperation userItemOperation) : base(userItemOperation)
        {
            Name = "Опробование";
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
                dataRow[0] = dds.Getting ? "соответствует требованиям" : dds.Comment;
                data.Rows.Add(dataRow);
            }

            return data;
        }

        /// <inheritdoc />
        protected override string GetReportTableName()
        {
            return MarkReportEnum.InsetrTextByMark.GetStringValue() + GetType().Name;
        }

        protected override void InitWork(CancellationTokenSource token)
        {
            base.InitWork(token);
            DataRow.Add(new DialogOperationHelp(this, "V740_1_Oprobovanie.rtf"));
        }

        #endregion
    }

    public sealed class DcvTest : OperationBase<MeasPoint<Voltage>>
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public DcvTest(IUserItemOperation userItemOperation) : base(userItemOperation)
        {
            Name = "Определение погрешности измерения постоянного напряжения";
        }

        #region Methods

        protected override void InitWork(CancellationTokenSource token)
        {
            base.InitWork(token);

            var dic = new Dictionary<int, double[]>();
            dic.Add(0, new[] { 0.035, 1, 25, 50, 75, 95 });
            dic.Add(1, new[] { 5.0, 25, 50, 75, 95 });
            dic.Add(2, new[] { 10.0, 50, 95 });
            dic.Add(3, new[] { 10.0, 50, 95 });
            dic.Add(4, new[] { 10.0, 50, 95 });


            MeasPoint<Voltage>[] testPoint =  (MeasPoint<Voltage>[]) GetTestPoints(Multimetr.DcVoltage, dic);




            foreach (var measPoint in testPoint)
            {
                var operation = new BasicOperationVerefication<MeasPoint<Voltage>>();

                operation.Expected = (MeasPoint<Voltage>) measPoint;
                operation.InitWorkAsync = () =>
                {
                    InitWork(Multimetr.DcVoltage, Clalibrator.DcVoltage, measPoint, Logger, token);

                    return Task.CompletedTask;
                };
                operation.BodyWorkAsync = () =>
                {
                    operation.Getting = BodyWork(Multimetr.DcVoltage, Clalibrator.DcVoltage, Logger, token).Item1;
                };
                operation.ErrorCalculation = (expected, getting) => null;
                operation.LowerCalculation = expected =>
                    expected - AllowableError(Multimetr.DcVoltage.RangeStorage, expected);

                operation.UpperCalculation = expected =>
                    expected + AllowableError(Multimetr.DcVoltage.RangeStorage, expected);

                operation.CompliteWorkAsync = () => CompliteWorkAsync(operation);

                operation.IsGood = () => ChekedOperation(operation);

                DataRow.Add(operation);
            }
        }

        #endregion
    }

    public sealed class AcvTest : OperationBase<MeasPoint<Voltage>>
    {
        public AcvTest(IUserItemOperation userItemOperation) : base(userItemOperation)
        {
            Name = "Определение погрешности измерения переменного напряжения";
        }

        #region Methods

        protected override void InitWork(CancellationTokenSource token)
        {
            base.InitWork(token);

            var FreqFullSet = new MeasPoint<Frequency>[]
            {
                new MeasPoint<Frequency>(20),
                new MeasPoint<Frequency>(40),
                new MeasPoint<Frequency>(10, UnitMultiplier.Kilo),
                new MeasPoint<Frequency>(20, UnitMultiplier.Kilo),
                new MeasPoint<Frequency>(50, UnitMultiplier.Kilo),
                new MeasPoint<Frequency>(100, UnitMultiplier.Kilo),
            };

            var FreqSmallSet = new MeasPoint<Frequency>[]
            {
                new MeasPoint<Frequency>(20),
                new MeasPoint<Frequency>(40),
                new MeasPoint<Frequency>(500),
                new MeasPoint<Frequency>(1, UnitMultiplier.Kilo)
                
            };

            var percentKit1 = new[] {10.0,25,50,75,95 };
            var percentKit2 = new[] {25.0,50,75,95 };
            var percentKit4 = new[] {10.0,50,95 };
            var percentKit5 = new[] {10.0,40 };

            var dic  = new Dictionary<int,(double[],MeasPoint<Frequency>[])>();
            dic.Add(0,value: (percentKit1,FreqFullSet));
            dic.Add(1,value: (percentKit2,FreqFullSet));//на 100 кГц должно быть 90% от предельного значения (последняя точка)
            dic.Add(2,value: (percentKit4,FreqFullSet));//на 100 кГц должно быть 90% от предельного значения (последняя точка)
            dic.Add(3,value: (percentKit4,FreqFullSet));//на 100 кГц должно быть 90% от предельного значения (последняя точка)
            dic.Add(4,value: (percentKit5,FreqSmallSet));


            IMeasPoint<Voltage, Frequency>[] GetTestPoints(IMeterPhysicalQuantity<Voltage> metr,
                Dictionary<int, (double[], MeasPoint<Frequency>[])> boockPercentAndFreq)
            {
                var endPoinArray = metr.RangeStorage.Ranges.Ranges.Select(q => q.End).OrderBy(q => q.GetMainValue()).ToArray();
                var testPoint = new List<IMeasPoint<Voltage,Frequency>>();

                for (int i = 0; i < endPoinArray.Length; i++)
                {
                    var range = endPoinArray[i];
                    var frequencys = boockPercentAndFreq[i].Item2;
                    var percentKit = boockPercentAndFreq[i].Item1;

                    foreach (var freq in frequencys)
                    {
                        if ((0 < i && i < endPoinArray.Length - 1) && (freq == new MeasPoint<Frequency>(100, UnitMultiplier.Kilo))) //проверка на диапазоны и частоту
                        {
                            percentKit[percentKit.Length - 1] = 0.90; // изменение конечной поверяемой точки предела на частоте 100 кГц для пределов 2 В, 20 В, 200 В (указано в МП таблица 26, стр. 7, столбец поверяемые отметки).
                        }
                    }
                    
                }

                return testPoint.ToArray();
            }

            var allPoints = GetTestPoints(Multimetr.AcVoltage, dic);

        }

        #endregion
    }

    public sealed class DciTest : OperationBase<MeasPoint<Current>>
    {
        public DciTest(IUserItemOperation userItemOperation) : base(userItemOperation)
        {
            Name = "Определение погрешности измерения постоянного тока";
        }

        #region Methods

        protected override void InitWork(CancellationTokenSource token)
        {
            base.InitWork(token);
            var testPoint = new[]
            {
                new MeasPoint<Current>(0.05M, UnitMultiplier.Micro ),
                new MeasPoint<Current>(2.0M , UnitMultiplier.Micro),
                new MeasPoint<Current>(20.0M, UnitMultiplier.Micro),
                new MeasPoint<Current>(100M , UnitMultiplier.Micro),
                new MeasPoint<Current>(150M , UnitMultiplier.Micro),
                new MeasPoint<Current>(190M , UnitMultiplier.Micro),
                
                new MeasPoint<Current>(0.2M, UnitMultiplier.Mili),
                new MeasPoint<Current>(1.0M, UnitMultiplier.Mili),
                new MeasPoint<Current>(1.9M, UnitMultiplier.Mili),
                
                new MeasPoint<Current>(2.0M, UnitMultiplier.Mili),
                new MeasPoint<Current>(10.0M, UnitMultiplier.Mili),
                new MeasPoint<Current>(19.0M, UnitMultiplier.Mili),
                
                new MeasPoint<Current>(20M, UnitMultiplier.Mili),
                new MeasPoint<Current>(100M, UnitMultiplier.Mili),
                new MeasPoint<Current>(190M, UnitMultiplier.Mili),
                
                new MeasPoint<Current>(100M, UnitMultiplier.Mili),
                new MeasPoint<Current>(1000M, UnitMultiplier.Mili),
                new MeasPoint<Current>(1900M, UnitMultiplier.Mili),
                
            };
        }

        #endregion
    }

    public sealed class AciTest : OperationBase<MeasPoint<Current,Frequency>>
    {
        public AciTest(IUserItemOperation userItemOperation) : base(userItemOperation)
        {
            Name = "Определение погрешности измерения постоянного тока";
        }

        protected override void InitWork(CancellationTokenSource token)
        {
            base.InitWork(token);
            MeasPoint<Current,Frequency> testPoints = new MeasPoint<Current, Frequency>(0,0);

            var arr = new MeasPoint<Current,Frequency>().GetArayMeasPointsInParcent(new MeasPoint<Current,Frequency>(200))
        }
    }
}