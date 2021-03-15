using System.Data;
using System.Threading;
using System.Threading.Tasks;
using AP.Reports.Utils;
using AP.Utils.Data;
using ASMC.Core.Model;
using ASMC.Data.Model;
using ASMC.Data.Model.Interface;
using ASMC.Data.Model.PhysicalQuantity;
using ASMC.Devices.IEEE.Fluke.Calibrator;
using ASMC.Devices.Interface.SourceAndMeter;
using DevExpress.Mvvm;
using NLog;
using Ivi.Visa;


namespace Belvar_V7_40_1
{
    /// <summary>
    /// Придоставляет базувую реализацию для пунктов поверки
    /// </summary>
    /// <typeparam name = "TOperation"></typeparam>
    public abstract class OperationBase<TOperation> : ParagraphBase<TOperation>
    {
        protected ICalibratorMultimeterFlukeBase Calibrator { get; set; }

        protected ASMC.Devices.IEEE.Belvar_V7_40_1 Multimetr { get; set; }

        /// <inheritdoc />
        protected OperationBase(IUserItemOperation userItemOperation) : base(userItemOperation)
        {
        }

        #region Methods

        protected override DataTable FillData()
        {
            var dataTable= base.FillData();
            
            foreach (var row in DataRow)
            {
                var dataRow = dataTable.NewRow();
                var rowFromDataRow = row as BasicOperationVerefication<TOperation>;
                if (rowFromDataRow == null) continue;
                dataRow["Предел измерения"] = rowFromDataRow.Name?.ToString();
                dataRow["Поверяемое значение"] = rowFromDataRow.Expected?.ToString();
                dataRow["Измеренное значение"] = rowFromDataRow.Getting?.ToString();
                if (rowFromDataRow.LowerTolerance != null)
                    dataRow["Минимальное допустимое значение"] = rowFromDataRow.LowerTolerance;
                if (rowFromDataRow.UpperTolerance != null)
                    dataRow["Максимальное допустимое значение"] = rowFromDataRow.UpperTolerance;
                if (rowFromDataRow.Getting != null)
                {
                    dataRow["Результат"] = rowFromDataRow.IsGood() ? ConstGood : ConstBad;
                }
                else
                {
                    dataRow["Результат"] = "не выполнено";
                }
                dataTable.Rows.Add(dataRow);
            }

            return dataTable;
        }

        protected override string[] GenerateDataColumnTypeObject()
        {
            return new[]
            {
                "Предел измерения",
                "Поверяемое значение",
                "Измеренное значение",
                "Минимальное допустимое значение",
                "Максимальное допустимое значение",
                "Результат"
            };
        }

        /// <inheritdoc />
        protected override string GetReportTableName()
        {
            return MarkReportEnum.FillTableByMark.GetStringValue() + GetType().Name;
        }

        protected bool IsSetRange<T>(IRangePhysicalQuantity<T> inRangeStorage) where T : class, IPhysicalQuantity<T>, new()
        {
            return inRangeStorage.SelectRange != null;
        }

        protected void ShowNotSupportedMeasurePointMeessage<T>(IMeterPhysicalQuantity<T> mult,
            ISourcePhysicalQuantity<T> sourse, MeasPoint<T> rangeToSetOnMetr, MeasPoint<T> testingMeasureValue) where T : class, IPhysicalQuantity<T>, new()
        {
            string message = "!!!ВНИМАНИЕ!!!\n\n";
            string endStr = ", согласно характеристикам в его файле точности.\n\n";
            //разберемся, у кого нет диапазона?
            if (!IsSetRange<T>(mult.RangeStorage))
            {
                //todo как-то проинформировать пользователя
                message = message + $"Предел {rangeToSetOnMetr.Description} нельзя измерить на {Multimetr.UserType}{endStr}";
            }
            if (!IsSetRange<T>(sourse.RangeStorage))
            {
                //todo как-то проинформировать пользователя
                message = message + $"Значение {testingMeasureValue.Description} нельзя воспроизвести с помощью {Calibrator.UserType}{endStr}";
            }

            message = message + $"\n\n!!!Данное значение не будет добавлено в протокол!!!";

            UserItemOperation.ServicePack.MessageBox()
                             .Show(message,
                                   "Значение физической величины вне технических характеристик оборудования",
                                   MessageButton.OK, MessageIcon.Information, MessageResult.Yes);
        }


        /// <param name = "token"></param>
        /// <inheritdoc />
        protected void InitWork<T>(IMeterPhysicalQuantity<T> multimetr, ISourcePhysicalQuantity<T> sourse, MeasPoint<T> rangeMeasureValue, MeasPoint<T> controlValue,
             Logger loger, CancellationTokenSource _token)
            where T : class, IPhysicalQuantity<T>, new()
        {
            multimetr.RangeStorage.SetRange(rangeMeasureValue);
            multimetr.RangeStorage.IsAutoRange = false;
            CatchException<IOTimeoutException>(() => multimetr.Setting(), _token, loger);
            CatchException<IOTimeoutException>(() => sourse.SetValue(controlValue), _token, loger);
        }

        protected (MeasPoint<TPhysicalQuantity>, IOTimeoutException) BodyWork<TPhysicalQuantity>(
            IMeterPhysicalQuantity<TPhysicalQuantity> multimetr, ISourcePhysicalQuantity<TPhysicalQuantity> sourse,
            Logger logger, CancellationTokenSource _token)
            where TPhysicalQuantity : class, IPhysicalQuantity<TPhysicalQuantity>, new()
        {
            CatchException<IOTimeoutException>(() => sourse.OutputOn(), _token, logger);
            
            (MeasPoint<TPhysicalQuantity>, IOTimeoutException) result;
            try
            {
                result = CatchException<IOTimeoutException, MeasPoint<TPhysicalQuantity>>(
                                                                                          () => multimetr.GetValue(), _token, logger);

            }
            finally
            {
                CatchException<IOTimeoutException>(() => sourse.OutputOff(), _token, logger);
            }

            return result;
        }

        /// <summary>
        ///     Позволяет получить погрешность для указаной точки.
        /// </summary>
        /// <typeparam name="T">
        ///     Физическая велечина для которой необходима получить погрешность <see cref="IPhysicalQuantity" />
        /// </typeparam>
        /// <param name="rangeStorage">Диапазон на котором определяется погрешность.</param>
        /// <param name="expected">Точка на диапазоне для которой определяется погрешность.</param>
        /// <returns></returns>
        protected MeasPoint<T> AllowableError<T>(IRangePhysicalQuantity<T> rangeStorage, MeasPoint<T> expected)
            where T : class, IPhysicalQuantity<T>, new()
        {
            rangeStorage.SetRange(expected);
            var toll = rangeStorage.SelectRange.AccuracyChatacteristic.GetAccuracy(
                                                                                   expected.MainPhysicalQuantity.GetNoramalizeValueToSi(),
                                                                                   rangeStorage.SelectRange.End.MainPhysicalQuantity.GetNoramalizeValueToSi());
            return new MeasPoint<T>(toll);
        }

        protected Task<bool> CompliteWorkAsync<T>(IMeasuringOperation<T> operation)
        {
            if (operation.IsGood == null || operation.IsGood())
                return Task.FromResult(operation.IsGood == null || operation.IsGood());

            return ShowQuestionMessage(operation.ToString()) == MessageResult.No
                ? Task.FromResult(true)
                : Task.FromResult(operation.IsGood == null || operation.IsGood());

            MessageResult ShowQuestionMessage(string message)
            {
                return UserItemOperation.ServicePack.MessageBox()
                                        .Show($"Текущая точка {message} не проходит по допуску:\n" +
                                              "Повторить измерение этой точки?",
                                              "Информация по текущему измерению",
                                              MessageButton.YesNo, MessageIcon.Question,
                                              MessageResult.Yes);
            }
        }

        protected bool ChekedOperation<T>(IBasicOperationVerefication<MeasPoint<T>> operation)
            where T : class, IPhysicalQuantity<T>, new()
        {
            return operation.Getting <= operation.UpperTolerance &&
                   operation.Getting >= operation.LowerTolerance;
        }

        protected override void ConnectionToDevice()
        {
            Calibrator = (ICalibratorMultimeterFlukeBase)GetSelectedDevice<ICalibratorMultimeterFlukeBase>();
            Calibrator.StringConnection = GetStringConnect(Calibrator);
            Calibrator.InitializeAsync();

            Multimetr = (ASMC.Devices.IEEE.Belvar_V7_40_1)GetSelectedDevice<ASMC.Devices.IEEE.Belvar_V7_40_1>();
            Multimetr.StringConnection = GetStringConnect(Multimetr);
            Multimetr.InitializeAsync();
        }

        #endregion
    }

    public abstract class MultiOperationBase<T1, T2> : OperationBase<MeasPoint<T1, T2>>
        where T1 : class, IPhysicalQuantity<T1>, new() where T2 : class, IPhysicalQuantity<T2>, new()
    {
        protected MultiOperationBase(IUserItemOperation userItemOperation) : base(userItemOperation)
        {
        }

        protected override string[] GenerateDataColumnTypeObject()
        {
            return new[]
            {
                "Предел измерения",
                "Поверяемое значение",
                "Значение частоты",
                "Измеренное значение",
                "Минимальное допустимое значение",
                "Максимальное допустимое значение",
                "Результат"
            };
        }

        protected override DataTable FillData()
        {
            var dataTable = base.FillData();
            dataTable.Rows.Clear();

            foreach (var row in DataRow)
            {
                var dataRow = dataTable.NewRow();
                var rowFromDataRow = row as BasicOperationVerefication<MeasPoint<T1, T2>>;
                if (rowFromDataRow == null) continue;
                dataRow["Предел измерения"] = rowFromDataRow.Name?.ToString();
                dataRow["Поверяемое значение"] = rowFromDataRow.Expected?.MainPhysicalQuantity.ToString();
                dataRow["Значение частоты"] = rowFromDataRow.Expected?.AdditionalPhysicalQuantity.ToString();
                dataRow["Измеренное значение"] = rowFromDataRow.Getting?.MainPhysicalQuantity.ToString();
                if (rowFromDataRow.LowerTolerance != null)
                    dataRow["Минимальное допустимое значение"] = rowFromDataRow.LowerTolerance.MainPhysicalQuantity.ToString(); 
                if (rowFromDataRow.UpperTolerance != null)
                    dataRow["Максимальное допустимое значение"] = rowFromDataRow.UpperTolerance.MainPhysicalQuantity.ToString(); 
                if ( rowFromDataRow.Getting != null)
                {
                    dataRow["Результат"] = rowFromDataRow.IsGood() ? ConstGood : ConstBad;
                }
                else
                {
                    dataRow["Результат"] = "не выполнено";
                }
                dataTable.Rows.Add(dataRow);
            }

            return dataTable;
        }

        protected void ShowNotSupportedMeasurePointMeessage(IMeterPhysicalQuantity<T1, T2> mult,
            ISourcePhysicalQuantity<T1, T2> sourse, MeasPoint<T1,T2> rangeToSetOnMetr, MeasPoint<T1, T2> testingMeasureValue)
        {
            string message = "!!!ВНИМАНИЕ!!!\n\n";
            string endStr = ", согласно характеристикам в его файле точности.\n\n";
            //разберемся, у кого нет диапазона?
            if (!RangeIsSet(mult.RangeStorage))
            {
                //todo как-то проинформировать пользователя
                message = message + $"Предел {rangeToSetOnMetr.Description} нельзя измерить на {Multimetr.UserType}{endStr}";
            }
            if (!RangeIsSet(sourse.RangeStorage))
            {
                //todo как-то проинформировать пользователя
                message = message + $"Значение {testingMeasureValue.Description} нельзя воспроизвести с помощью {Calibrator.UserType}{endStr}";
            }

            message = message + $"\n\n!!!Данное значение не будет добавлено в протокол!!!";

            UserItemOperation.ServicePack.MessageBox()
                             .Show(message,
                                   "Значение физической величины вне технических характеристик оборудования",
                                   MessageButton.OK, MessageIcon.Information, MessageResult.Yes);
        }

        protected IPhysicalRange<T1, T2> InitWork(IMeterPhysicalQuantity<T1, T2> mult,
            ISourcePhysicalQuantity<T1, T2> sourse, MeasPoint<T1, T2> rangeToSetOnMetr,
            MeasPoint<T1, T2> testingMeasureValue, Logger loger, CancellationTokenSource _token)
        {
            
            mult.RangeStorage.SetRange(rangeToSetOnMetr);
            mult.RangeStorage.IsAutoRange = false;
            CatchException<IOTimeoutException>(() => mult.Setting(), _token, loger);
            
            sourse.RangeStorage.SetRange(testingMeasureValue);
            CatchException<IOTimeoutException>(() => sourse.SetValue(testingMeasureValue), _token, loger);
            return mult.RangeStorage.SelectRange;
        }

        /// <summary>
        /// Проверка, установлен ли диапазон измерения/воспроизведения физ. величины.
        /// </summary>
        /// <param name="inRangeStorage">Точностные характеристики устройства из файла точности.</param>
        /// <returns></returns>
        protected bool RangeIsSet(IRangePhysicalQuantity<T1, T2> inRangeStorage)
        {
            return inRangeStorage.SelectRange != null ;
        }

        protected (MeasPoint<T1>, IOTimeoutException) BodyWork(
            IMeterPhysicalQuantity<T1, T2> mult, ISourcePhysicalQuantity<T1, T2> sourse,
            Logger logger, CancellationTokenSource _token)

        {
            CatchException<IOTimeoutException>(() => sourse.OutputOn(), _token, logger);
            (MeasPoint<T1>, IOTimeoutException) result;
            try
            {
                
                result = CatchException<IOTimeoutException, MeasPoint<T1>>(
                                                                           () => mult.GetValue(), _token, logger);
            }
            finally
            {
                CatchException<IOTimeoutException>(() => sourse.OutputOff(), _token, logger);
            }

            return result;
        }

        protected MeasPoint<T1, T2> ConvertMeasPoint(MeasPoint<T1> gettingMeasPoint,
            MeasPoint<T1, T2> exepectedMeasPoint)
        {
            return new MeasPoint<T1, T2>(gettingMeasPoint.MainPhysicalQuantity,
                                         exepectedMeasPoint.AdditionalPhysicalQuantity);
        }

        /// <summary>
        ///     Позволяет получить погрешность для указаной точки.
        /// </summary>
        /// <typeparam name="T">
        ///     Физическая фелечина для которой необходима получить погрешность <see cref="IPhysicalQuantity" />
        /// </typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T1"></typeparam>
        /// <param name="rangeStorage">Диапазон на котором определяется погрешность.</param>
        /// <param name="expected">Точка на диапазоне для которой определяется погрешность.</param>
        /// <returns></returns>
        protected MeasPoint<T1, T2> AllowableError(IRangePhysicalQuantity<T1, T2> rangeStorage,
            MeasPoint<T1, T2> expected)
        {
            rangeStorage.SetRange(expected);
            //todo а если выбранный предел null?
            var toll = rangeStorage.SelectRange.AccuracyChatacteristic.GetAccuracy(
                                                                                   expected.MainPhysicalQuantity.GetNoramalizeValueToSi(),
                                                                                   rangeStorage.SelectRange.End.MainPhysicalQuantity.GetNoramalizeValueToSi());
            return ConvertMeasPoint(new MeasPoint<T1>(toll), expected);
        }

        protected bool ChekedOperation(IBasicOperationVerefication<MeasPoint<T1, T2>> operation)
        {
            return operation.Getting <= operation.UpperTolerance &&
                   operation.Getting >= operation.LowerTolerance;
        }
    }
}