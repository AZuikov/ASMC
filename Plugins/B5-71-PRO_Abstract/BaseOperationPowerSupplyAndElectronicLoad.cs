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

        protected decimal ErrorCalculation(decimal inExpected, bool isSumm = false)
        {
            decimal error = Bp.TolleranceFormulaVolt(inExpected);
            decimal result = isSumm ? inExpected + error : inExpected - error;
            MathStatistics.Round(ref result, 3);

            return result;
        }

        protected void SetLowAndUppToleranceAndIsGood(BasicOperationVerefication<decimal> inOperation)
        {
            inOperation.LowerCalculation = (expected) => ErrorCalculation(expected);
            inOperation.UpperCalculation = (expected) => ErrorCalculation(expected, true);
            inOperation.IsGood = () => (inOperation.Getting < inOperation.UpperTolerance) &
                                     (inOperation.Getting > inOperation.LowerTolerance);
        }

    }

    public abstract class BaseOparationWithMultimeter : BaseOperationPowerSupplyAndElectronicLoad
    {
        protected Mult_34401A Mult { get; set; }

        protected BaseOparationWithMultimeter(IUserItemOperation userItemOperation) : base(userItemOperation)
        {
        }
    }
}
