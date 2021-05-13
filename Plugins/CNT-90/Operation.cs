using AP.Extension;
using ASMC.Core.Model;
using ASMC.Data.Model;
using ASMC.Devices.IEEE.Keysight.Generator;
using System;
using System.IO;
using System.Reflection;
using ASMC.Devices.IEEE;

namespace CNT_90
{
    public class OperationPrimary<TTestDevices> : Operation where TTestDevices : IUserType, new()
    {
        public OperationPrimary(string documentName, ServicePack servicePac) : base(servicePac)
        {
            ControlDevices = new IDeviceUi[]
            {
                new Device {Devices = new IUserType[] {new GeneratorOfSignals_81160A() }},
                //new Device {Devices = new IUserType[] {/*Указать список 2 устройств с помощью которых производится поверка*/}}
            };
            TestDevices = new IDeviceUi[]
            {
                new Device
                {
                    Devices = new IUserType[] {new TTestDevices()}, IsCanStringConnect = false,
                    Description = $@"{new TTestDevices().UserType}"
                }
            };
            UserItemOperation = new IUserItemOperationBase[]
            {
                new VisualInspection(this),
                new Testing(this),
                new FrequencyMeasureCNT90(this)
                /*Остальная часть методики*/
            };
            DocumentName = documentName;
            Accessories = new string[]
            {
                /*Указать перечень*/
            };
            string path = Directory.GetCurrentDirectory() + "\\Plugins\\" + Assembly.GetExecutingAssembly().GetName().Name + "\\Resources\\points_CNT-90.asmc";
            //todo пользователь должен иметь возможность выбрать файл с поверяемыми точками

            //OperationExtension.FillTestPoint(this, path);
        }

        /// <inheritdoc />
        public override async void RefreshDevice()
        {
            AddresDevice = IeeeBase.AllStringConnect;
        }

        /// <inheritdoc />
        public override void FindDevice()
        {
            throw new NotImplementedException();
        }
    }
}