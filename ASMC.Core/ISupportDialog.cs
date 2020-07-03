using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASMC.Core
{
    public interface ISupportDialog
    {
        bool? DialogResult
        {
            get;
        }

        void Initialize();
    }
}
