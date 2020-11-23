using System;
using System.Linq;
using System.Threading.Tasks;
using ASMC.Core.Model;
using ASMC.Data.Model;
using ASMC.Devices.UInterface.RemoveDevice.ViewModel;
using ASMC.Devices.USB_Device.SiliconLabs;
using ASMC.Devices.USB_Device.SKBIS.Lir917;
using ASMC.Devices.USB_Device.WebCam;

namespace mp2192_92.DialIndicator.Periodic
{
    public class OpertionPeriodicVerf<T> : Operation where T : IUserType, new()
    {
        public OpertionPeriodicVerf(string documentName, ServicePack servicePac) : base(servicePac)
        {
            ControlDevices = new IDeviceUi[]
            {
                new Device {Devices = new IUserType[] {new Ppi()}},
                new Device {Devices = new IUserType[] {new WebCamUi()}}
            };
            TestDevices = new IDeviceUi[]
            {
                new Device
                {
                    Devices = new IUserType[]
                    {
                        new T()
                    },
                    IsCanStringConnect = false,
                    Description = $@"Индикатор частового типа {new T().UserType}"
                }
            };
            UserItemOperation = new IUserItemOperationBase[]
            {
                new VisualInspection(this),
                new Testing(this), 
                new MeasuringForce(this),
                new PerpendicularPressure(this),
                new RangeIndications(this),
                new VariationReading(this),
                new DeterminationError(this)
            };
            Accessories = new[]
            {
                "Весы настольные циферблатные РН-3Ц13У",
                "Приспособление для определения измерительного усилия и его колебаний мод. 253",
                "Граммометр часового типа Г 3,0",
                "Прибор для поверки индикаторов ППИ-50"
            };
            DocumentName = documentName;
        }

        /// <inheritdoc />
        public override async void RefreshDevice()
        {
            await Task.Factory.StartNew(() =>
            {
                AddresDevice = UsbExpressWrapper.FindAllDevice?.Select(q => q.Number.ToString())
                    .Concat(WebCam.GetVideoInputDevice?.Select(q => q.MonikerString) ?? Array.Empty<string>()).ToArray();
            });
        }

        /// <inheritdoc />
        public override void FindDevice()
        {
            throw new NotImplementedException();
        }
    }
}