using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ASMC.Core.Model;

namespace ProgramFor34461A
{
    public class Main : Program<Program34461A>
    {
        /// <inheritdoc />
        public Main(ServicePack service) : base(service)
        {
            Type = "34461A";
            Grsi = "72879-18";
            Range = "no range";
        }
    }
    public class Program34461A : OperationMetrControlBase
    {

        public Program34461A(ServicePack servicePac)
        {
            UserItemOperationPrimaryVerf = new OperationPrimary<ASMC.Devices.IEEE.Keysight.Multimeter.Keysight34461A>("34461A_protocol", servicePac);
        }

    }


}
