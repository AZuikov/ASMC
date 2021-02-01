using System;
using System.Collections.Generic;
using ASMC.Data.Model;
using ASMC.Data.Model.PhysicalQuantity;

namespace ASMC.Devices.WithoutInterface.HourIndicator
{
    /// <summary>
    ///     Презосталвет базовую реализацию часового интидикатора
    /// </summary>
    public class IchBase : IUserType
    {
        #region Property


        /// <summary>
        ///     Здает измерительный диапазон.
        /// </summary>
        public virtual RangeStorage<PhysicalRange<Length>> RangesFull { get; set; }


        public virtual MeasPoint<Length> Variation { get; set; }


        #endregion

        #region IUserType Members

        /// <inheritdoc />
        public string UserType { get; set; }

        #endregion


      
    }

}