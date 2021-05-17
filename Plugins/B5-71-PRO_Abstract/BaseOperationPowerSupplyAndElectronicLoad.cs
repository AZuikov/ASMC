using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AP.Math;
using ASMC.Core.Model;
using ASMC.Data.Model;
using ASMC.Devices.IEEE.Keysight.ElectronicLoad;
using ASMC.Devices.IEEE.Keysight.Multimeter;
using ASMC.Devices.Port.Profigrupp;

namespace B5_71_PRO_Abstract
{
    public abstract class BaseOperationPowerSupplyAndElectronicLoad : ParagraphBase<decimal>
    {
        protected B571Pro Bp { get; set; }
        protected MainN3300 Load { get; set; }
        public BaseOperationPowerSupplyAndElectronicLoad(IUserItemOperation userItemOperation) : base(userItemOperation)
        {
        }

        protected override string GetReportTableName()
        {
            throw new NotImplementedException();
        }
        //----------------------------------------------------------------------------------
        protected decimal ErrorCalculationVolt(decimal inExpected, bool isSumm = false)
        {
            decimal error = Bp.TolleranceFormulaVolt(inExpected);
            decimal result = isSumm ? inExpected + error : inExpected - error;
            MathStatistics.Round(ref result, 3);

            return result;
        }

        protected void SetLowAndUppToleranceAndIsGood_Volt(BasicOperationVerefication<decimal> inOperation)
        {
            inOperation.LowerCalculation = (expected) => ErrorCalculationVolt(expected);
            inOperation.UpperCalculation = (expected) => ErrorCalculationVolt(expected, true);
            inOperation.IsGood = (getting) => (getting < inOperation.UpperTolerance) &
                                     (getting > inOperation.LowerTolerance);
        }
        //-----------------------------------------------------------------------------------
        protected decimal ErrorCalculationVoltUnstable(decimal inExpected, bool isSumm = false)
        {
            decimal error = Bp.TolleranceVoltageUnstability;
            decimal result = isSumm ? inExpected + error : inExpected - error;
            MathStatistics.Round(ref result, 3);

            return result;
        }

        protected void SetLowAndUppToleranceAndIsGood_VoltUnstable(BasicOperationVerefication<decimal> inOperation)
        {
            inOperation.LowerCalculation = (expected) => ErrorCalculationVoltUnstable(expected);
            inOperation.UpperCalculation = (expected) => ErrorCalculationVoltUnstable(expected, true);
            inOperation.IsGood = (getting) => (getting < inOperation.UpperTolerance) &
                                              (getting > inOperation.LowerTolerance);
        }
        //----------------------------------------------------------------------------------
        protected decimal ErrorCalculationCurr(decimal inExpected, bool isSumm = false)
        {
            decimal error = Bp.TolleranceFormulaVolt(inExpected);
            decimal result = isSumm ? inExpected + error : inExpected - error;
            MathStatistics.Round(ref result, 3);

            return result;
        }

        protected void SetLowAndUppToleranceAndIsGood_Curr(BasicOperationVerefication<decimal> inOperation)
        {
            inOperation.LowerCalculation = (expected) => ErrorCalculationCurr(expected);
            inOperation.UpperCalculation = (expected) => ErrorCalculationCurr(expected, true);
            inOperation.IsGood = (getting) => (getting < inOperation.UpperTolerance) &
                                              (getting > inOperation.LowerTolerance);
        }
        //----------------------------------------------------------------------------------
        protected decimal ErrorCalculationCurrUnstable(decimal inExpected, bool isSumm = false)
        {
            decimal error = Bp.TolleranceCurrentUnstability;
            decimal result = isSumm ? inExpected + error : inExpected - error;
            MathStatistics.Round(ref result, 3);

            return result;
        }

        protected void SetLowAndUppToleranceAndIsGood_CurrUnstable(BasicOperationVerefication<decimal> inOperation)
        {
            inOperation.LowerCalculation = (expected) => ErrorCalculationCurrUnstable(expected);
            inOperation.UpperCalculation = (expected) => ErrorCalculationCurrUnstable(expected, true);
            inOperation.IsGood = (getting) => (getting < inOperation.UpperTolerance) &
                                              (getting > inOperation.LowerTolerance);
        }
        //----------------------------------------------------------------------------------
        protected decimal ErrorCalculationVoltPuls(decimal inExpected, bool isSumm = false)
        {
            decimal error = Bp.TolleranceVoltPuls;
            decimal result = isSumm ? inExpected + error : inExpected - error;
            MathStatistics.Round(ref result, 3);

            return result;
        }

        protected void SetLowAndUppToleranceAndIsGood_VoltPuls(BasicOperationVerefication<decimal> inOperation)
        {
            inOperation.LowerCalculation = (expected) => ErrorCalculationVoltPuls(expected);
            inOperation.UpperCalculation = (expected) => ErrorCalculationVoltPuls(expected, true);
            inOperation.IsGood = (getting) => (getting < inOperation.UpperTolerance) &
                                              (getting > inOperation.LowerTolerance);
        }
        //----------------------------------------------------------------------------------
        protected decimal ErrorCalculationCurrPuls(decimal inExpected, bool isSumm = false)
        {
            decimal error = Bp.TolleranceCurrentPuls;
            decimal result = isSumm ? inExpected + error : inExpected - error;
            MathStatistics.Round(ref result, 3);

            return result;
        }

        protected void SetLowAndUppToleranceAndIsGood_CurrPuls(BasicOperationVerefication<decimal> inOperation)
        {
            inOperation.LowerCalculation = (expected) => ErrorCalculationCurrPuls(expected);
            inOperation.UpperCalculation = (expected) => ErrorCalculationCurrPuls(expected, true);
            inOperation.IsGood = (getting) => (getting < inOperation.UpperTolerance) &
                                              (getting > inOperation.LowerTolerance);
        }
    }

    public abstract class BaseOparationWithMultimeter : BaseOperationPowerSupplyAndElectronicLoad
    {
        protected Keysight34401A Mult { get; set; }

        protected BaseOparationWithMultimeter(IUserItemOperation userItemOperation) : base(userItemOperation)
        {
        }
    }
}
