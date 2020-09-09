using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Accord.Video.DirectShow;
using ASMC.MVision;
using DevExpress.Mvvm;

namespace ASMC.Common.ViewModel
{
    /// <summary>
    /// Предоставляет базовый класс для VM использующую видеоустройство.
    /// </summary>
    public class WebCamViewModel: ClosableViewModel
    {
        /// <summary>
        /// Объект для работы с видеоустройством.
        /// </summary>
        protected WebCam WebCam { get; set; }
        /// <summary>
        /// Источник видеопотока.
        /// </summary>
        public BitmapImage VideoSourse
        {
            get => _videoSourse;
            set => SetProperty(ref _videoSourse, value, nameof(VideoSourse));
        }
        private FilterInfo[] _videoDevise;
        private BitmapImage _videoSourse;

        public FilterInfo[] VideoDevise
        {
            get => _videoDevise;
            set => SetProperty(ref _videoDevise, value, nameof(VideoDevise));
        }

        /// <summary>
        /// Команда отображения дефолтного окна настроек видекоустройства.
        /// </summary>
        public ICommand PropertyWebCamShowCommand { get; }
        /// <summary>
        /// Комманда запуска видеопотока.
        /// </summary>
        public ICommand StartVideoCommand { get; }
        /// <summary>
        /// Комманда остановки видеопотока.
        /// </summary>
        public ICommand StopVideoCommand { get; }
        /// <summary>
        /// Комманда обнавления видеоустройств.
        /// </summary>
        public ICommand RefreshVideoDeviceCommand { get; }
        public WebCamViewModel()
        {
            PropertyWebCamShowCommand = new DelegateCommand(OnPropertyWebCamShowCommand);
            StartVideoCommand = new DelegateCommand(StartVideo);
            StopVideoCommand = new DelegateCommand(StopVideo);
            RefreshVideoDeviceCommand = new DelegateCommand(RefreshVideoDevice);
        }

        private void OnPropertyWebCamShowCommand()
        {
           WebCam.ShowProperty();
        }

        /// <inheritdoc />
        public override void Close()
        {
            StopVideo();
        }

        protected void StartVideo()
        {
            if (WebCam.Source == null) return;
            WebCam.Start();
        }

        protected void StopVideo()
        {
            WebCam.Notifly -= WebCam_Notifly;
            WebCam.Stop();

        }
        /// <inheritdoc />
        protected override void OnInitialized()
        {
            base.OnInitialized();
            WebCam = new WebCam();
            WebCam.Notifly += WebCam_Notifly;
            RefreshVideoDevice();
        }

        protected void RefreshVideoDevice()
        {
            VideoDevise = WebCam.GetVideoInputDevice.ToArray();
        }

        /// <summary>
        /// Преобразует кадры в источник.
        /// </summary>
        /// <example>
        /// <remarks>При перегузке обязательно, вызов базового метода в самом конце.</remarks>
        /// <code>
        ///   protected override void WebCam_Notifly(Bitmap bitmap)
        ///   {
        ///     /*Ваш код*/
        ///     base.WebCam_Notifly(bitmap);
        ///    }
        /// </code></example>
        /// <param name="bitmap"></param>
        protected virtual void WebCam_Notifly(Bitmap bitmap)
        {
            BitmapImage bmi;
            using (MemoryStream ms = new MemoryStream())
            {
                bitmap.Save(ms, ImageFormat.Tiff);
                ms.Seek(0, SeekOrigin.Begin);
                bmi = new BitmapImage();
                bmi.BeginInit();
                bmi.StreamSource = ms;
                bmi.CacheOption = BitmapCacheOption.OnLoad;
                bmi.EndInit();
                bmi.Freeze();
            }
            VideoSourse = bmi;
        }

       
    }
}
