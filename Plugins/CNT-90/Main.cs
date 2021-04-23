using ASMC.Core.Model;
using ASMC.Devices.IEEE.PENDULUM;

namespace CNT_90
{
    public class Main : Program<Program_CNT_90>
    {
        /// <inheritdoc />
        public Main(ServicePack service) : base(service)
        {
            Type = "CNT-90";
            Grsi = "41567-09";
            Range = "0,001 Гц...300 МГц";
        }
    }

    public class Program_CNT_90 : OperationMetrControlBase
    {
        public Program_CNT_90(ServicePack servicePac)
        {
            UserItemOperationPrimaryVerf = new OperationPrimary<Pendulum_CNT_90>("CNT-90_protocol", servicePac);
        }
    }
}