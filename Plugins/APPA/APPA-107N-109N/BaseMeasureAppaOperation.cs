using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AP.Extension;
using ASMC.Core.Model;
using ASMC.Data.Model;
using ASMC.Data.Model.PhysicalQuantity;
using ASMC.Devices.IEEE.Fluke.Calibrator;
using ASMC.Devices.Port.APPA;
using NLog.LayoutRenderers.Wrappers;

namespace APPA_107N_109N
{
   public abstract class BaseMeasureAppaOperation<T> : ParagraphBase<MeasPoint<T>> where T : PhysicalQuantity<T>, new()
    {
       

        #region Property

        /// <summary>
        /// Код предела измерения на поверяемого прибора.
        /// </summary>
        public virtual Mult107_109N.RangeCode OperationRangeCode { get; protected set; }

        /// <summary>
        /// Предел измерения поверяемого прибора, необходимый для работы.
        /// </summary>
        public virtual Mult107_109N.RangeNominal OperationRangeNominal { get; protected set; }

        /// <summary>
        /// Режим операции измерения прибора.
        /// </summary>
        public Mult107_109N.MeasureMode OperMeasureMode { get; protected set; }

        //контрлируемый прибор
        protected Mult107_109N appa10XN { get; set; }

        //эталон
        protected Calib5522A flkCalib5522A { get; set; }

        #endregion

        public BaseMeasureAppaOperation(IUserItemOperation userItemOperation) : base(userItemOperation)
        {
        }

        protected override string GetReportTableName()
        {
            throw new NotImplementedException();
        }

        protected void SetUpperAndLowerToleranceAndIsGood(BasicOperationVerefication<MeasPoint<T>> inOperation)
        {
            inOperation.LowerCalculation = (expected)=> expected - inOperation.Error;
            inOperation.UpperCalculation = (expected)=> expected + inOperation.Error;
            inOperation.LowerTolerance.MainPhysicalQuantity.ChangeMultiplier(inOperation
                                                                          .Expected.MainPhysicalQuantity
                                                                          .Multiplier);
            inOperation.UpperTolerance.MainPhysicalQuantity.ChangeMultiplier(inOperation
                                                                          .Expected.MainPhysicalQuantity
                                                                          .Multiplier);

            inOperation.IsGood = () =>
            {
                if (inOperation.Getting == null || inOperation.Expected == null ||
                    inOperation.UpperTolerance == null || inOperation.LowerTolerance == null) return false;
                return (inOperation.Getting < inOperation.UpperTolerance) &
                       (inOperation.Getting > inOperation.LowerTolerance);
            };
        }
    }

   public class BaseMeasureAppaAcOperation<T, T1> : ParagraphBase<MeasPoint<T, T1>>
       where T : PhysicalQuantity<T>, new() where T1 : PhysicalQuantity<T1>, new()
   {
       #region Property

       /// <summary>
       /// Код предела измерения на поверяемого прибора.
       /// </summary>
       public virtual Mult107_109N.RangeCode OperationRangeCode { get; protected set; }

       /// <summary>
       /// Предел измерения поверяемого прибора, необходимый для работы.
       /// </summary>
       public virtual Mult107_109N.RangeNominal OperationRangeNominal { get; protected set; }

       /// <summary>
       /// Режим операции измерения прибора.
       /// </summary>
       public Mult107_109N.MeasureMode OperMeasureMode { get; protected set; }

       //контрлируемый прибор
       protected Mult107_109N appa10XN { get; set; }

       //эталон
       protected Calib5522A flkCalib5522A { get; set; }

       #endregion

        public BaseMeasureAppaAcOperation(IUserItemOperation userItemOperation) : base(userItemOperation)
       {
       }

       protected override string GetReportTableName()
       {
           throw new NotImplementedException();
       }

       protected void SetUpperAndLowerToleranceAndIsGood(BasicOperationVerefication<MeasPoint<T,T1>> inOperation)
       {
           inOperation.LowerCalculation = (expected) => expected - inOperation.Error;
           inOperation.UpperCalculation = (expected) => expected + inOperation.Error;
           inOperation.LowerTolerance.MainPhysicalQuantity.ChangeMultiplier(inOperation
                                                                           .Expected.MainPhysicalQuantity
                                                                           .Multiplier);
           inOperation.UpperTolerance.MainPhysicalQuantity.ChangeMultiplier(inOperation
                                                                           .Expected.MainPhysicalQuantity
                                                                           .Multiplier);

           inOperation.IsGood = () =>
           {
               if (inOperation.Getting == null || inOperation.Expected == null ||
                   inOperation.UpperTolerance == null || inOperation.LowerTolerance == null) return false;
               return (inOperation.Getting < inOperation.UpperTolerance) &
                      (inOperation.Getting > inOperation.LowerTolerance);
           };
       }
    }
}
