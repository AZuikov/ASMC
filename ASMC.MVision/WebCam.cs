using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Accord;
using Accord.Imaging;
using Accord.Imaging.Filters;
using Accord.Video;
using Accord.Video.DirectShow;
using NLog;

namespace ASMC.MVision
{
    public class WebCam:IDisposable
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly VideoCaptureDevice _videoCaptureDevice;

        /// <summary>
        /// Позволяет получать список подключенных видеоустройств.
        /// </summary>
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

        /// <summary>
        /// Позволяет получать или задавать настройки видео.
        /// </summary>
        public VideoCapabilities VideoResolution
        {
            get
            {
                return _videoCaptureDevice.VideoResolution;
            }
            set
            {
                _videoCaptureDevice.VideoResolution = value;
            }
        }

        /// <summary>
        /// Позволяет получать массив доступных настройк видео.
        /// </summary>
        public VideoCapabilities [] VideoCapabilities
        {
            get
            {
                return !string.IsNullOrWhiteSpace(_videoCaptureDevice.Source) ? _videoCaptureDevice.VideoCapabilities : null;
            }
        }

        public delegate  void FrameHandlir(Bitmap bitmap);
        public event FrameHandlir Notifly;
        /// <summary>
        /// Позволяет получить или задавать истокчник для видеоустройства.
        /// </summary>
        public FilterInfo Source
        {
            get
            {
                return GetVideoInputDevice.FirstOrDefault(q => q.MonikerString.Equals(_videoCaptureDevice.Source));
                 
            }
            set
            {
                _videoCaptureDevice.Source = value.MonikerString;
                _videoCaptureDevice.VideoResolution = _videoCaptureDevice.VideoCapabilities[7];
                Logger.Debug($@"Назначен источник {value.Name}.");
            }
        }

        /// <summary>
        /// Запускает видеопоток с устройства.
        /// </summary>
        public void Start()
        {
            if (!_videoCaptureDevice.IsRunning)
            {
                _videoCaptureDevice.NewFrame += VideoCaptureDevice_NewFrame;
                _videoCaptureDevice.Start();
                Logger.Debug($@"Трансляция с камеры запущена.");
            }

           
        }
        /// <summary>
        /// Отображает настройки видекоустройства.
        /// </summary>
        public async void ShowProperty()
        {
           await Task.Factory.StartNew(()=> _videoCaptureDevice.DisplayPropertyPage(IntPtr.Zero));
        }

        /// <summary>
        /// Применяет предустановленые фильтры к каждому кадру.
        /// </summary>
        /// <param name="bitmap"></param>
        /// <returns></returns>
        public virtual Bitmap DefaultFilters(Bitmap bitmap)
        {
            bitmap= MirrorFiltr(bitmap);
            bitmap= GrayFiltr(bitmap);
            return bitmap;
        }

        /// <summary>
        /// Позволяет задать пользовательские фильтр к кадру.
        /// </summary>
        public Func<Bitmap, Bitmap> SetFilterImage { get; set; }

        /// <summary>
        ///Позволяет получить или задать отрожение изображение по горизонтали.
        /// </summary>
        public bool MirrorHorizontal
        {
            get;
            set;
        }
        /// <summary>
        ///Позволяет получить или задать отрожение изображение по вертикали.
        /// </summary>
        public bool MirrorVertical
        {
            get;
            set;
        }
        /// <summary>
        ///Позволяет получить или задать отрожение изображение в оттенках серого..
        /// </summary>
        public bool GrayColor
        {
            get;
            set;
        }
        private Bitmap GrayFiltr(Bitmap bitmap)
        {
            if (GrayColor)
            {
                bitmap= new Grayscale(0.2125,0.7154,0.0721).Apply(bitmap);
            }
            return bitmap;
        }


        private Bitmap MirrorFiltr(Bitmap bitmap)
        {
            return new  Mirror( MirrorVertical, MirrorHorizontal).Apply(bitmap);
        }

        private void VideoCaptureDevice_NewFrame(object sender, NewFrameEventArgs eventargs)
        {
            var image = (Bitmap) eventargs.Frame.Clone();
            image= DefaultFilters(image);
            Notifly?.Invoke(SetFilterImage != null ? SetFilterImage(image) : image);
        }

        /// <summary>
        /// Останавливает видеопоток.
        /// </summary>
        public void Stop()
        {
            if (_videoCaptureDevice.IsRunning)
            {
                _videoCaptureDevice.Stop();
                _videoCaptureDevice.NewFrame -= VideoCaptureDevice_NewFrame;
                Logger.Debug($@"Трансляция с камеры остановлена.");
            }
            
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Stop();
        }
    }
}
