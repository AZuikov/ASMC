using ASMC.Data.Model;

namespace ASMC.Core.Model
{
    /// <inheritdoc />
    public abstract class Operation : IUserItemOperation
    {
        protected Operation(ServicePack servicePack)
        {
            ServicePack = servicePack;
        }

        /// <inheritdoc />
        public IDeviceUi[] TestDevices { get; set; }

        /// <inheritdoc />
        public IUserItemOperationBase[] UserItemOperation { get; set; }

        /// <inheritdoc />
        public string DocumentName { get; protected set; }

        /// <inheritdoc />
        public string[] Accessories { get; protected set; }

        /// <inheritdoc />
        public string[] AddresDevice { get; set; }

        /// <inheritdoc />
        public IDeviceUi[] ControlDevices { get; set; }

        /// <summary>
        /// Проверяет всели эталоны подключены
        /// </summary>
        public abstract void RefreshDevice();

        /// <inheritdoc />
        public abstract void FindDevice();

        public ServicePack ServicePack { get; set; }
    }
}