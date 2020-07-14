using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ASMC.Core.ViewModel;
using ASMC.Data.Model;

namespace ASMC.ViewModel
{
    public class ShemViewModel : FromBaseViewModel
    {
        private ShemeImage _shema;

        /// <summary>
        /// ПОзволяет получать или задавать  отображенную схему.
        /// </summary>
        public ShemeImage Shema
        {
            get => _shema;
            set => SetProperty(ref _shema, value, nameof(Shema));
        }
    }
}
