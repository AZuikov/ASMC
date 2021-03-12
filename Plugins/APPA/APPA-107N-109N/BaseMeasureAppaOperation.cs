using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AP.Extension;
using AP.Utils.Data;
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
        public virtual Mult107_109N.RangeAppaNominal OperationRangeAppaNominal { get; protected set; }

        public virtual PhysicalRange<T> PhysicalRangeAppa { get; protected set; }

        /// <summary>
        /// Режим операции измерения прибора.
        /// </summary>
        public Mult107_109N.MeasureMode OperMeasureMode { get; protected set; }

        //контрлируемый прибор
        protected Mult107_109N appa10XN { get; set; }

        //эталон
        protected ICalibratorMultimeterFlukeBase flkCalib5522A { get; set; }

        #endregion

        public BaseMeasureAppaOperation(IUserItemOperation userItemOperation) : base(userItemOperation)
        {
        }

        protected override string GetReportTableName()
        {
            throw new NotImplementedException();
        }

        protected MeasPoint<T> ErrorCalc(BasicOperationVerefication<MeasPoint<T>> inOperation)
        {
            
            if (PhysicalRangeAppa == null)
                throw new
                    Exception($"Не удалось подобрать предел измерения прибора для точки {inOperation.Expected.Description}");
            
            MeasPoint<T>  tolMeasPoint = PhysicalRangeAppa.CalculateTollerance(inOperation.Expected);

             
            return tolMeasPoint;
        }
        protected void SetUpperAndLowerToleranceAndIsGood(BasicOperationVerefication<MeasPoint<T>> inOperation)
        {
            var err = ErrorCalc(inOperation);
            inOperation.LowerCalculation = (expected)=>
            {
                MeasPoint<T> result = expected - err;
                result.MainPhysicalQuantity.ChangeMultiplier(expected.MainPhysicalQuantity.Multiplier);
                return result;
            };
            inOperation.UpperCalculation = (expected)=>
            {
                MeasPoint<T> result = expected + err;
                result.MainPhysicalQuantity.ChangeMultiplier(expected.MainPhysicalQuantity.Multiplier);
                return result;
            };
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
       public virtual Mult107_109N.RangeAppaNominal OperationRangeAppaNominal { get; protected set; }

        public virtual PhysicalRange<T,T1> PhysicalRangeAppa { get; protected set; }

        /// <summary>
        /// Режим операции измерения прибора.
        /// </summary>
        public Mult107_109N.MeasureMode OperMeasureMode { get; protected set; }

       //контрлируемый прибор
       protected Mult107_109N appa10XN { get; set; }

       //эталон
       protected ICalibratorMultimeterFlukeBase flkCalib5522A { get; set; }

       #endregion

        public BaseMeasureAppaAcOperation(IUserItemOperation userItemOperation) : base(userItemOperation)
       {
       }

       protected override string GetReportTableName()
       {
           throw new NotImplementedException();
       }

       protected MeasPoint<T,T1> ErrorCalc(BasicOperationVerefication<MeasPoint<T,T1>> inOperation)
       {

           if (PhysicalRangeAppa == null)
               throw new
                   Exception($"Не удалось подобрать предел измерения прибора для точки {inOperation.Expected.Description}");

           MeasPoint<T,T1> tolMeasPoint = PhysicalRangeAppa.CalculateTollerance(inOperation.Expected);

           if (inOperation.Expected.MainPhysicalQuantity.Value < 0)
               tolMeasPoint = tolMeasPoint * -1;

           return tolMeasPoint;
       }

        protected void SetUpperAndLowerToleranceAndIsGood(BasicOperationVerefication<MeasPoint<T,T1>> inOperation)
        {
            var err = ErrorCalc(inOperation);
           inOperation.LowerCalculation = (expected) =>
           {
               MeasPoint<T, T1> result = expected - err;
               result.MainPhysicalQuantity.ChangeMultiplier(expected.MainPhysicalQuantity.Multiplier);
               return result;
           };
           inOperation.UpperCalculation = (expected) =>
           {
               MeasPoint<T, T1> result = expected + err;
               result.MainPhysicalQuantity.ChangeMultiplier(expected.MainPhysicalQuantity.Multiplier);
               return result;
           };
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
