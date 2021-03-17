using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ASMC.Core.Model;
using ASMC.Data.Model;
using ASMC.Data.Model.PhysicalQuantity;
using ASMC.Devices.IEEE.Fluke.Calibrator;
using ASMC.Devices.Interface;
using ASMC.Devices.OWEN;
using ASMC.Devices.UInterface.TRM.ViewModel;
using DevExpress.Mvvm;

namespace OWEN_TRM202
{
   public abstract class BaseMeasureOperation<T>: ParagraphBase<MeasPoint<T>> where T : PhysicalQuantity<T> ,new()
    {
        #region Fields

        protected ICalibratorMultimeterFlukeBase Calibrator;

        //protected FlukeTypeTermocouple CalibrFlukeTypeTermocouple;

        protected TRM202Device.in_t CoupleTypeTrm;
        /// <summary>
        /// Перечь проверяемых точек.
        /// </summary>
        protected MeasPoint<T>[] measPoints;
        /// <summary>
        /// Диапазон измерения типа датчика ТРМ.
        /// </summary>
        protected RangeStorage<PhysicalRange<T>> MeasureRanges;
        protected TRM202DeviceUI trm202;

        #endregion Fields

        #region Property

        protected ushort _chanelNumber { get; set; }

        #endregion Property

        protected BaseMeasureOperation(IUserItemOperation userItemOperation) : base(userItemOperation)
        {
        }

        protected override void ConnectionToDevice()
        {
            trm202 = (UserItemOperation.TestDevices.FirstOrDefault(q => q.SelectedDevice as TRM202DeviceUI != null)
                                       .SelectedDevice as IControlPannelDevice).Device as TRM202DeviceUI;
            Calibrator =
                UserItemOperation.ControlDevices.FirstOrDefault(q => q.SelectedDevice as ICalibratorMultimeterFlukeBase != null)
                                 .SelectedDevice as ICalibratorMultimeterFlukeBase;
            trm202.StringConnection = GetStringConnect(trm202);
            Calibrator.StringConnection ??= GetStringConnect(Calibrator);
        }

        protected override DataTable FillData()
        {
            var dataTable = base.FillData();
            foreach (var row in DataRow)
            {
                var dataRow = dataTable.NewRow();
                var dds = row as BasicOperationVerefication<MeasPoint<T>>;
                if (dds == null) continue;
                dataRow["Поверяемая точка"] = dds?.Expected?.Description;
                dataRow["Измеренное значение"] = dds?.Getting?.Description;
                
                PhysicalRange<T> range = MeasureRanges.GetRangePointBelong(dds?.Expected);
                
                if (range!=null && dds?.Getting != null && dds?.Expected != null)
                {
                    
                    //посчитаем основную приведенную погрешность
                    dataRow["Основная приведенная погрешность"] =
                        Helps.CalculateBasicRedundanceTol<T>(dds.Getting, dds.Expected, range).Description;
                    var tolRange = (decimal)range.AccuracyChatacteristic.RangePercentFloor;
                    MeasPoint<Percent> RangeTol = new MeasPoint<Percent>(tolRange);
                    dataRow["Допустимое значение приведенной погрешности"] = RangeTol.Description;

                }
                if (dds?.IsGood == null)
                    dataRow[dataRow.Table.Columns.Count - 1] = "не выполнено";
                else
                    dataRow[dataRow.Table.Columns.Count - 1] = dds.IsGood() ? "Годен" : "Брак";
                dataTable.Rows.Add(dataRow);
            }

            return dataTable;
        }

    }

    public static class Helps
    {
        #region Methods

        public static void AttentionWindow(IUserItemOperation UserItemOperation)
        {
            UserItemOperation.ServicePack.MessageBox().Show("Внимание!!!\n в процессе выполнения программы будут изменены настройки прибора.\n" +
                                                            "Необходимо зарнее сделать резервную копию настроек прибора с помощью программы \"Конфигуратор ТРМ\".\n" +
                                                            "С помощью программы конфигуратора: \n 1) подключитесь к прибору\n2)считать все параметры с прибора\n" +
                                                            "3)сохранить загруженную с прибора конфигурацию в отдельном файле." +
                                                            "\n\nДля восстановления прежних настроек прибора необходимо: 1)Загрузить в конфигураторе файл с сохраненными ранее настройками\n" +
                                                            "2)Записать все параметры в прибор");
        }

        public static Task<bool> HelpsCompliteWork<T>(BasicOperationVerefication<MeasPoint<T>> operation,
            IUserItemOperation UserItemOperation) where T : class, IPhysicalQuantity<T>, new()
        {
            if (operation.Getting !=null && operation.IsGood != null && !operation.IsGood())
            {
                var answer =
                    UserItemOperation.ServicePack.MessageBox()
                                     .Show($"Текущая точка {operation.Expected.Description} не проходит по допуску:\n" +
                                           $"Минимально допустимое значение {operation.LowerTolerance.Description}\n" +
                                           $"Максимально допустимое значение {operation.UpperTolerance.Description}\n" +
                                           $"ИЗМЕРЕННОЕ значение {operation.Getting.Description}\n\n" +
                                           $"\nФАКТИЧЕСКАЯ погрешность {(operation.Expected - operation.Getting).Description}\n\n" +
                                           "Повторить измерение этой точки?",
                                           "Информация по текущему измерению",
                                           MessageButton.YesNo, MessageIcon.Question,
                                           MessageResult.Yes);

                if (answer == MessageResult.No) return Task.FromResult(true);
            }else if (operation.IsGood == null || operation.Getting == null)
            {
                return Task.FromResult(true);
            }
            
            return Task.FromResult(operation.IsGood());
        }

        public static MeasPoint<Percent> CalculateBasicRedundanceTol<T>(MeasPoint<T> measPoint, MeasPoint<T> StdPoint, PhysicalRange<T> range)  where T : PhysicalQuantity<T>, new()
        {
            decimal val1 = measPoint.MainPhysicalQuantity.Value - StdPoint.MainPhysicalQuantity.Value;
            decimal val2 = (val1 / range.GetRangeLeght.MainPhysicalQuantity.Value) * 100;
            val2 = Math.Round(val2, 2);
            MeasPoint<Percent> resultTol = new MeasPoint<Percent>(val2);
            return resultTol;
        }



        #endregion Methods
    }

}
