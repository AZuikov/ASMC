using System;
using System.Data;
using System.Linq;
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
using ASMC.Devices.IEEE.Keysight.Multimeter;
using ASMC.Devices.Interface;
using NLog;

namespace Multimetr34401A
{
    /// <summary>
    ///     Придоставляет базувую реализацию для пунктов поверки
    /// </summary>
    /// <typeparam name="TOperation"></typeparam>
    public abstract class OperationBase<TOperation> : ParagraphBase<TOperation>
    {
        protected ICalibratorOld Clalibrator { get; set; }

        protected Keysight34401A Multimetr { get; set; }
        /// <inheritdoc />
        protected OperationBase(IUserItemOperation userItemOperation) : base(userItemOperation)
        {

        }


        #region Methods
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
        #region Methods

        protected override void ConnectionToDevice()
        {
            Clalibrator = (ICalibratorOld) GetSelectedDevice<ICalibratorOld>();
            //Clalibrator.StringConnection = GetStringConnect(Clalibrator);
            Multimetr= (Keysight34401A) GetSelectedDevice<Keysight34401A>();
            Multimetr.StringConnection = GetStringConnect(Multimetr);
        }

        #endregion

        /// <param name="token"></param>
        /// <inheritdoc />
        protected override void InitWork(CancellationTokenSource token)
        {
            /*Сдесь должна быть инициализация*/
            base.InitWork(token);
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
            if (DataRow.Count == 1)
            {
                var dds = DataRow[0] as BasicOperation<bool>;
                // ReSharper disable once PossibleNullReferenceException
                dataRow[0] = dds.Getting ? "соответствует требованиям" : dds.Comment;
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
                operation.InitWork = () =>
                {
                    try
                    {
                        Multimetr.DcVoltage.Range = setPoint;
                        Multimetr.DcVoltage.AutoRange = false;
                        Multimetr.DcVoltage.IsEnable = true;
                        Multimetr.DcVoltage.
                        Clalibrator.SetVoltageDc(setPoint);
                        Clalibrator.IsEnableOutput=true;
                    }
                    catch (Exception e)
                    {
                        Logger.Error(e);
                        throw;
                    }
                    return Task.CompletedTask;
                };
                operation.BodyWorkAsync = () =>
                {
                    try
                    {
                        Multimetr.DcVoltage.Setting();

                        Multimetr.DcVoltage.Getting();
                        //Multimetr.DcVoltage.Value

                        //Clalibrator.Setting();
                    }
                    catch (Exception e)
                    {
                        Logger.Error(e);
                        token.Cancel();
                    }
                    finally
                    {
                        Clalibrator.IsEnableOutput = false;
                    }
                   
                };
                operation.CompliteWork = () =>
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
                };
                DataRow.Add(operation);
            }
         
                

          
            //DataRow.Add();
        }
    }

}