using System;
using ASMC.Core.Model;
using ASMC.Data.Model;
using ASMC.Devices.USB_Device.SKBIS.Lir917;
using ASMC.Devices.USB_Device.WebCam;

namespace mp2192_92.DialIndicator.First
{
    public class OpertionFirsVerf<T> : Operation where T : IUserType, new()
    {
        public OpertionFirsVerf(ServicePack servicePac) : base(servicePac)
        {
            ControlDevices = new IDeviceUi[]
            {
                new Device {Devices = new IUserType[] {new Ppi()}},
                new Device {Devices = new IUserType[] {new WebCam()}}
            };
            TestDevices = new IDeviceUi[]
            {
                new Device
                {
                    Devices = new IUserType[] {new T()}
                }
            };
            UserItemOperation = new IUserItemOperationBase[]
            {
                new VisualInspection(this),
                new Testing(this),

                new ConnectionDiametr(this), 
                new LinerRoughness(this),
                new TipRoughness(this),
                new ArrowWidch(this),
                new StrokeWidch(this),
                new StrokeLength(this),
                new BetweenArrowDial(this),
                new MeasuringForce(this),
                new PerpendicularPressure(this),
                new RangeIndications(this),
                new VariationReading(this),
                new DeterminationError(this),
                new Determination_01Error(this)

            };
            DocumentName = "ИЧ-10 Первичная МП 2192-92";
        }
    
            //Accessories = new[]
            //{
            //    "Весы настольные циферблатные РН-3Ц13У",
            //    "Приспособление для определения измерительного усилия и его колебаний мод. 253",
            //    "Граммометр часового типа Г 3,0",
            //    "Прибор для поверки индикаторов ППИ-50"
            //};
        /// <inheritdoc />
        public override void RefreshDevice()
        {
            //AddresDevice = ASMC.Devices.USB_Device.SiliconLabs.UsbExpressWrapper.FindAllDevice.Select(q => q.Number.ToString()).Concat(WebCam.GetVideoInputDevice.Select(q => q.MonikerString)).ToArray();
        }
        /// <inheritdoc />
        public override void FindDevice()
        {
            throw new NotImplementedException();
        }
    }
}