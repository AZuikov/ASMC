using System;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using AP.Reports.Utils;
using AP.Utils.Data;
using ASMC.Common.Helps;
using ASMC.Core.Model;
using ASMC.Data.Model;
using ASMC.Data.Model.PhysicalQuantity;
using ASMC.Devices.IEEE;
using ASMC.Devices.IEEE.Fluke.Calibrator;
using ASMC.Devices.IEEE.Keysight.Multimeter;
using ASMC.Devices.Interface;
using DevExpress.Mvvm;
using Ivi.Visa;
using NLog;

namespace Multimetr34401A
{
    /// <summary>
    ///     Придоставляет базувую реализацию для пунктов поверки
    /// </summary>
    /// <typeparam name="TOperation"></typeparam>
    public abstract class OperationBase<TOperation> : ParagraphBase<TOperation>
    {
        private  readonly Logger Logger = LogManager.GetCurrentClassLogger();
        protected ICalibratorMultimeterFlukeBase Clalibrator { get; set; }

        protected Keysight34401A Multimetr { get; set; }
        /// <inheritdoc />
        protected OperationBase(IUserItemOperation userItemOperation) : base(userItemOperation)
        {

        }


        #region Methods


        /// <inheritdoc />
        protected override string[] GenerateDataColumnTypeObject()
        {
            return new[] { "Ожидаемое значение",
                "Измеренное значение",
                "Минимальное допустимое значение",
                "Максимальное допустимое значение"
            }.Concat(base.GenerateDataColumnTypeObject()).ToArray();
        }

        /// <summary>
        /// Создает схему
        /// </summary>
        /// <param name="filename">Имя файла с разширением</param>
        /// <param name="number">Номер схемы</param>
        /// <returns></returns>
        protected SchemeImage ShemeGeneration(string filename, int number)
        {
            return new SchemeImage
            {
                AssemblyLocalName = Assembly.GetExecutingAssembly().GetName().Name,
                Description = "Измерительная схема. "+ this.Name,
                Number = number,
                FileName = filename,
                ExtendedDescription = "Соберите измерительную схему, согласно рисунку"

            };
        }

        /// <inheritdoc />
        protected override string GetReportTableName()
        {
            return MarkReportEnum.FillTableByMark.GetStringValue() + GetType().Name;
        }

        protected override void ConnectionToDevice()
        {
            Clalibrator = (ICalibratorMultimeterFlukeBase) GetSelectedDevice<ICalibratorMultimeterFlukeBase>();
            //Clalibrator.StringConnection = GetStringConnect(Clalibrator);
            Multimetr= (Keysight34401A) GetSelectedDevice<Keysight34401A>();
            Multimetr.StringConnection = GetStringConnect(Multimetr);
        }

        /// <inheritdoc />
        protected override DataTable FillData()
        {
            var data = base.FillData();

            foreach (var row in DataRow)
            {
                var dataRow = data.NewRow();
                var dds = row as BasicOperationVerefication<TOperation>;
                if (dds==null) continue;
                // ReSharper disable once PossibleNullReferenceException
                dataRow[0] = dds.Getting.ToString();
                dataRow[1] = dds.Expected.ToString();
                dataRow[2] = dds.LowerTolerance.ToString();
                dataRow[3] = dds.UpperTolerance.ToString();
                dataRow[4] = string.IsNullOrWhiteSpace(dds.Comment) ? (dds.IsGood() ? ConstGood : ConstBad) : dds.Comment;
                data.Rows.Add(dataRow);
            }
          

            return data;
        }



        #endregion
    }


    /// <summary>
    ///     Предоставляет реализацию внешнего осномотра.
    /// </summary>
    public sealed class VisualInspection : OperationBase<bool>
    {
        /// <inheritdoc />
        public VisualInspection(IUserItemOperation userItemOperation) : base(userItemOperation)
        {
            Name = "Внешний осмотр";
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
        protected override string[] GenerateDataColumnTypeObject()
        {
            return new[] { "Результат внешнего оснотра" };
        }
        /// <inheritdoc />
        protected override void InitWork(CancellationTokenSource token)
        {
            base.InitWork(token);
            DataRow.Add(new DialogOperationHelp(this, "Внешний осмотр"));
        }
    }

    /// <summary>
    ///     Предоставляет операцию опробывания.
    /// </summary>
    public sealed class Testing : OperationBase<bool>
    {
        /// <inheritdoc />
        public Testing(IUserItemOperation userItemOperation) : base(userItemOperation)
        {
            Name = "Опробывание";
        }

        /// <inheritdoc />
        protected override string GetReportTableName()
        {
            return MarkReportEnum.InsetrTextByMark.GetStringValue() + GetType().Name;
        }
        /// <inheritdoc />
        protected override string[] GenerateDataColumnTypeObject()
        {
            return new[] { "Результат опробывания" };
        }
        protected override void InitWork(CancellationTokenSource token)
        {
            base.InitWork(token);
            DataRow.Add(new DialogOperationHelp(this, Test, "Внешний осмотр"));
        }

        private async Task<bool> Test()
        {
            try
            {
              return  await Task.Factory.StartNew(() => Multimetr.SelfTest(), CancellationToken.None);
            }
            catch (Exception)
            {
                return false;
            }

        }
        /// <inheritdoc />
        protected override DataTable FillData()
        {
            var data = base.FillData();

            var dataRow = data.NewRow();
            if (DataRow.Count != 1) return data;

            var dds = DataRow[0] as BasicOperation<bool>;
            // ReSharper disable once PossibleNullReferenceException
            dataRow[0] = dds.Getting ? "соответствует требованиям" : dds.Comment;
            data.Rows.Add(dataRow);

            return data;
        }

    }


    public abstract class MultiPoint<T1,T2> : OperationBase<MeasPoint<T1,T2>> where T1 : class, IPhysicalQuantity<T1>, new() where T2 : class, IPhysicalQuantity<T2>, new()
    {
        /// <inheritdoc />
        protected MultiPoint(IUserItemOperation userItemOperation) : base(userItemOperation)
        {
        }
        /// <inheritdoc />
        protected override DataTable FillData()
        {
            var data = base.FillData();

            foreach (var row in DataRow)
            {
                var dataRow = data.NewRow();
                var dds = row as BasicOperationVerefication<MeasPoint<T1, T2>>;
                if (dds == null) continue;
                // ReSharper disable once PossibleNullReferenceException
                dataRow[0] = dds.Getting.ToString();
                dataRow[1] = dds.Expected.MainPhysicalQuantity.ToString();
                dataRow[2] = dds.LowerTolerance.MainPhysicalQuantity.ToString();
                dataRow[3] = dds.UpperTolerance.MainPhysicalQuantity.ToString();
                dataRow[4] = string.IsNullOrWhiteSpace(dds.Comment) ? (dds.IsGood() ? ConstGood : ConstBad) : dds.Comment;
                data.Rows.Add(dataRow);
            }


            return data;
        }
    }

    public sealed class DCVoltageError: OperationBase<MeasPoint<Voltage>>
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        /// <inheritdoc />
        public DCVoltageError(IUserItemOperation userItemOperation) : base(userItemOperation)
        {
            Name = "Определение погрешности DC";
            Sheme = ShemeGeneration("",0);
        }
        protected override void InitWork(CancellationTokenSource token)
        {
            base.InitWork(token);
            var voltRef =  new []{ new MeasPoint<Voltage>(0.1m), new MeasPoint<Voltage>(1), new MeasPoint<Voltage>(10), new MeasPoint<Voltage>(100), new MeasPoint<Voltage>(1000) };
            foreach (var setPoint in voltRef)
            {
                var operation = new BasicOperationVerefication<MeasPoint<Voltage>>();

                operation.Expected = setPoint;
                operation.InitWorkAsync = () =>
                {
                    Multimetr.DcVoltage.RangeStorage.SetRange(setPoint);
                    Multimetr.DcVoltage.RangeStorage.IsAutoRange = false;
                    CatchException<IOTimeoutException>(()=>Multimetr.DcVoltage.Setting(), token, Logger);
                    CatchException<IOTimeoutException>(() => Clalibrator.DcVoltage.SetValue(setPoint), token, Logger);
                   
                    return Task.CompletedTask;
                };
                operation.BodyWorkAsync = () =>
                {
                   
                        CatchException<IOTimeoutException>(() => Clalibrator.DcVoltage.OutputOn(), token, Logger);
                        operation.Getting = CatchException<IOTimeoutException,MeasPoint<Voltage>>(() => Multimetr.DcVoltage.GetValue(), token, Logger).Item1;
                        CatchException<IOTimeoutException>(() => Clalibrator.DcVoltage.OutputOff(), token, Logger);

                };
                operation.CompliteWorkAsync = () =>
                {
                    if (operation.IsGood == null || operation.IsGood())
                        return Task.FromResult(operation.IsGood == null || operation.IsGood());
                    var answer =
                        UserItemOperation.ServicePack.MessageBox()
                            .Show($"Текущая точка {operation} не проходит по допуску:\n" +
                                  $"Повторить измерение этой точки?",
                                "Информация по текущему измерению",
                                MessageButton.YesNo, MessageIcon.Question,
                                MessageResult.Yes);

                    return answer == MessageResult.No ? Task.FromResult(true) : Task.FromResult(operation.IsGood == null || operation.IsGood());
                };
                operation.IsGood = () => operation.Getting <= operation.UpperTolerance &&
                                         operation.Getting >= operation.LowerTolerance;
                DataRow.Add(operation);
            }
        }
    }
   
}