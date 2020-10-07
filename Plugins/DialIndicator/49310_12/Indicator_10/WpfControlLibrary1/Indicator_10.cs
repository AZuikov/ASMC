using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;
using ASMC.Core.Model;
using ASMC.Data.Model;
using ASMC.Data.Model.Interface;
using Indicator_10.First;
using Indicator_10.Periodic;

namespace Indicator_10
{
    /// <inheritdoc />
    // ReSharper disable once UnusedMember.Global
    public class Indicator10 : Program<Verefication>
    {
        /// <inheritdoc />
        public Indicator10(ServicePack service) : base(service)
        {
            this.Type = "ИЧ10";
            this.Grsi = "318-49, 32512-06, 33841-07, 40149-08, 42499-09, 49310-12, 54058-13, 57937-14, 64188-16, 69534-17, До 26 декабря 1991 года";
            this.Range = "(0...10) мм";
        }
    }

    public class Verefication : OperationMetrControlBase
    {

        public Verefication(ServicePack servicePac)
        {
            this.UserItemOperationPrimaryVerf = new OpertionFirsVerf(servicePac);
            this.UserItemOperationPeriodicVerf = new OpertionPeriodicVerf(servicePac);
        }
    }

    //public abstract class IndicatorParagraphBase<T> : ParagraphBase, IUserItemOperation<T>
    //{
    //    protected const string ConstGood = "Годен";
    //    protected const string ConstBad = "Брак";
    //    protected const string ConstNotUsed = "Не выполнено";

    //    /// <summary>
    //    /// Имя закладки таблички в протоколе.
    //    /// </summary>
    //    protected abstract string GetReportTableName();
        

    //    /// <summary>
    //    /// Предоставляет перечень имен столбцов таблицы для отчетов
    //    /// </summary>
    //    protected abstract DataColumn[] GetColumnName();
        

    //    /// <inheritdoc />
    //    protected IndicatorParagraphBase(IUserItemOperation userItemOperation) : base(userItemOperation)
    //    {
    //        DataRow = (List<IBasicOperation<T>>)Activator.CreateInstance(typeof(List<IBasicOperation<T>>));
    //    }


    //    /// <inheritdoc />
      

    //    ///// <inheritdoc />
    //    //protected override void InitWork()
    //    //{
    //    //    DataRow.Clear();
    //    //}

    //    ///// <inheritdoc />
    //    //public List<IBasicOperation<T>> DataRow { get; set; }
    //}
}
