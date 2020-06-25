using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASMC.Properties
{
    internal class AssemblySettings
    {
        public MainSettings Main { get; set; } = new MainSettings();

        public class MainSettings /*: WindowSettingsBase*/
        {
            public MainSettings()
            {
            //    Width = 800;
            //    Height = 600;
            }
        }
    }
}
