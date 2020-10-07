using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using ASMC.Core.Model;
using ASMC.Data.Model;
using ASMC.Devices.WithoutInterface.HourIndicator;
using DevExpress.Mvvm.UI;
using Indicator_10.ViewModel;
using WindowService = ASMC.Common.UI.WindowService;

namespace Indicator_10
{

    public class MeasuringForce : ParagraphBase<MeasPoint>
    {
        protected Ich Ich { get; set; }

        /// <inheritdoc />
        public MeasuringForce(IUserItemOperation userItemOperation) : base(userItemOperation)
        {
            Name = "Определение измерительного усилия и его колебания";
        }

        /// <inheritdoc />
        protected override string GetReportTableName()
        {
            return "Определение измерительного усилия и его колебания";
        }

        /// <inheritdoc />
        protected override DataColumn[] GetColumnName()
        {
            return new [] { new DataColumn("Точка диапазона измерений индикатора, мм"), 
                new DataColumn("Показания весов, г"),
                new DataColumn("При изменении направления хода изм. стержня"),
                new DataColumn("Колебание при прямом/обратном ходе"),
                new DataColumn("Максимальное при прямом ходе"),
            }; 
        }

        /// <inheritdoc />
        protected override DataTable FillData()
        {

            var dataTable = base.FillData();

            foreach (var row in DataRow)
            {
                var dataRow = dataTable.NewRow();
                var dds = row as BasicOperationVerefication<MeasPoint>;
                // ReSharper disable once PossibleNullReferenceException
                if (dds == null) continue;
                dataRow[0] = dds.Expected?.Description;
                dataRow[1] = dds.Getting?.Description;
                dataRow[2] = dds.LowerTolerance?.Description;
                dataRow[3] = dds.UpperTolerance?.Description;
                if (dds.IsGood == null)
                    dataRow[5] = ConstNotUsed;
                else
                    dataRow[5] = dds.IsGood() ? ConstGood : ConstBad;
                dataTable.Rows.Add(dataRow);
            }

            return dataTable;
        }

        /// <inheritdoc />
        protected override void InitWork()
        {
            base.InitWork();
            var operation = new BasicOperation<MeasPoint>();
            operation.InitWork = async () =>
            {
                var a = this.UserItemOperation.ServicePack.FreeWindow as WindowService;
                var vm = new MeasuringForceViewModel(UserItemOperation.TestDevices.FirstOrDefault().SelectedDevice as Ich);
                a.ViewLocator = new ViewLocator(Assembly.GetExecutingAssembly());
                a.Show("MeasuringForceView", vm,null, null);
            };
            DataRow.Add(operation);
        }
    }

    //public class ConnectionDiametr : IndicatorParagraphBase<MeasPoint>
    //{
    ///// <inheritdoc />
    //protected override DataColumn[] GetColumnName()
    //{
    //return new[] { new DataColumn("Присоединительный диаметр"), new DataColumn("Присоединительный диаметр"), new DataColumn("Минимальный диаметр гильзы"), new DataColumn("Максимальный диаметр гильзы") };
    //}

    //    /// <inheritdoc />
    //    protected override DataTable FillData()
    //    {

    //        var dataTable = base.FillData();

    //        foreach (var row in DataRow)
    //        {
    //            var dataRow = dataTable.NewRow();
    //            var dds = row as BasicOperationVerefication<MeasPoint>;
    //            // ReSharper disable once PossibleNullReferenceException
    //            if (dds == null) continue;
    //            dataRow[0] = dds.Expected?.Description;
    //            dataRow[1] = dds.Getting?.Description;
    //            dataRow[2] = dds.LowerTolerance?.Description;
    //            dataRow[3] = dds.UpperTolerance?.Description;
    //            if (dds.IsGood == null)
    //                dataRow[5] = ConstNotUsed;
    //            else
    //                dataRow[5] = dds.IsGood() ? ConstGood : ConstBad;
    //            dataTable.Rows.Add(dataRow);
    //        }

    //        return dataTable;
    //    }

    //    /// <inheritdoc />
    //    protected override void InitWork()
    //    {
    //        base.InitWork();
    //        var operation = new BasicOperation<MeasPoint>();
    //        operation.InitWork = () =>
    //        {
    //            var a = this.UserItemOperation.ServicePack.FreeWindow as WindowService;
    //            a.Show();
    //            var vm = new
    //                a.Show();
    //        }
    //        DataRow.Add(operation);
    //    }
    //}
}
