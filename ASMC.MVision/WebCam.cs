using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AForge.Video.DirectShow;
using NLog;

namespace ASMC.MVision
{
    public class WebCam
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly VideoCaptureDevice _videoCaptureDevice;

        public static FilterInfoCollection GetVideoInputDevice
        {
            get
            {
                return new FilterInfoCollection(FilterCategory.VideoInputDevice);
            }
        }


        public WebCam()
        {

            _videoCaptureDevice = new VideoCaptureDevice();
        }

        public delegate  void FrameHandlir(Bitmap bitmap);
        public event FrameHandlir Notifly;
        public FilterInfo Source
        {
            get
            {
                return GetVideoInputDevice.Cast<FilterInfo>()
                                           .FirstOrDefault(q => q.MonikerString.Equals(_videoCaptureDevice.Source));
                 
            }
            set
            {
                _videoCaptureDevice.Source = value.MonikerString;

                _videoCaptureDevice.VideoResolution = _videoCaptureDevice.VideoCapabilities[7];
                Logger.Debug($@"Назначен источник {value.Name}.");
            }
        }

        public void Start()
        {
            
            _videoCaptureDevice.NewFrame += VideoCaptureDevice_NewFrame;
            _videoCaptureDevice.Start();
            Logger.Debug($@"Запущено считывание");
        }


        public void Stop()
        {
            _videoCaptureDevice.Stop();
            _videoCaptureDevice.NewFrame -= VideoCaptureDevice_NewFrame;
            Logger.Debug($@"Остановлено считывание");
        }
        private void VideoCaptureDevice_NewFrame(object sender, AForge.Video.NewFrameEventArgs eventArgs)
        {
            Notifly?.Invoke((Bitmap)eventArgs.Frame.Clone());
           //var video =  sender as VideoCaptureDevice;
           // eventArgs.Frame.Save(@"C:\Users\02tav01\Pictures\dsada." + ImageFormat.Jpeg.ToString(), ImageFormat.Jpeg);
            Logger.Debug($@"кадр {sender}");
        }

        public class CameraControlProperty
        {
            public int Value { get; set; }
            public CameraControlFlags Flags { get; set; }
        }

       
    }
}
