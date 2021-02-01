using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ASMC.Data.Model;
using ASMC.Data.Model.PhysicalQuantity;

namespace ASMC.Devices.WithoutInterface.HourIndicator.MC52415_13
{
    public abstract class DIMC52415_13 : IchBase
    {
        /// <inheritdoc />
        public override RangeStorage<PhysicalRange<Length>> RangesFull => GetRangesFull();
        /// <summary>
        ///     Позволяет задать полный измерительный диапазон.
        /// </summary>
        /// <param name="currentAccuracyClass"></param>
        /// <returns></returns>
        protected virtual RangeStorage<PhysicalRange<Length>> GetRangesFull()
        {
            return null;
        }
    }

    public class DISeries801 : DIMC52415_13
    {
        /// <inheritdoc />
        protected override RangeStorage<PhysicalRange<Length>> GetRangesFull()
        {
            return base.GetRangesFull();
        }
    }
}
