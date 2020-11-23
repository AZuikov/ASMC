using System;
using System.Linq;
using System.Threading.Tasks;
using ASMC.Core.Model;
using ASMC.Data.Model;
using ASMC.Devices.USB_Device.SiliconLabs;
using ASMC.Devices.USB_Device.SKBIS.Lir917;
using ASMC.Devices.USB_Device.WebCam;

namespace mp2192_92.DialIndicator.First
{
    public class OpertionFirsVerf<T> : Operation where T : IUserType, new()
    {
        public OpertionFirsVerf(string documentName, ServicePack servicePac) : base(servicePac)
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
                    Devices = new IUserType[] {new T()}, IsCanStringConnect = false,
                    Description = $@"Индикатор частового типа {new T().UserType}"
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
            DocumentName = documentName;
            Accessories = new[]
            {
                "Калибр-скоба 8h7 по ГОСТ 16675",
                "Рычажный микрометр типа МР с диапазоном измерений 0-25 мм по ГОСТ 4381",
                "Образцы шероховатости по ГОСТ 9378 или детали-образцы с параметром шероховатости Ra = 0,63 мкм и Ra = 0,1 мкм",
                "Микроскоп инструментальный по ГОСТ 8074",
                "Циферблатные настольные весы с ценой деления 5 г по ГОСТ 23711",
                "Стойка типа С-11 по ГОСТ 10197 с дополнительным кронштейном с присоединительным диаметром 8 мм",
                "Прибор ППИ-50 (или приспособление с микрометрической головкой) с диапазоном измерений 0-50 мм, вариацией показаний не более 1 мкм, наибольшей разностью погрешностей на любом участке длиной в 1 мм не более 2 мкм и на всем диапазоне измерений не более 3 мкм",
                "Граммометр с ценой деления 0,1 Н, диапазоном измерений 0,5-3 Н, погрешностью не более  0,1 Н или динамометрическое приспособление, отградуированное на усилие 2,5 Н с погрешностью не более  0,1 Н"
            };
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