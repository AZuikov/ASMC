using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Accord.Video.DirectShow;
using ASMC.Common.ViewModel;
using ASMC.Core;
using ASMC.Core.ViewModel;
using ASMC.Data.Model;
using ASMC.Devices.USB_Device.WebCam;
using DevExpress.Mvvm;
using DevExpress.Xpf.Core.Commands;

namespace ASMC.Devices.UInterface.RemoveDevice.ViewModel
{
    public class WebCamUi : IControlPannelDevice
    {
        public WebCamUi()
        {
            ViewModel = new WebCamViewModel();
            DocumentType = "WebCamView";
            Assembly = Assembly.GetExecutingAssembly();
            Device = new WebCam();
        }

        /// <inheritdoc />
        public string DocumentType { get; }

        /// <inheritdoc />
        public INotifyPropertyChanged ViewModel { get; }

        /// <inheritdoc />
        public Assembly Assembly { get; }

        /// <inheritdoc />
        public IUserType Device { get; }

        /// <inheritdoc />
        public string UserType => Device.UserType;
    }
    /// <summary>
    /// Предоставляет базовый класс для VM использующую видеоустройство.
    /// </summary>
    public class WebCamViewModel: BindableBase, ISupportSettings
    {
        /// <summary>
        /// Объект для работы с видеоустройством.
        /// </summary>
        public WebCam WebCam { get; private set; }

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
        public System.Windows.Input.ICommand PropertyWebCamShowCommand { get; }
        /// <summary>
        /// Комманда запуска видеопотока.
        /// </summary>
        public System.Windows.Input.ICommand StartVideoCommand { get; }
        /// <summary>
        /// Комманда остановки видеопотока.
        /// </summary>
        public System.Windows.Input.ICommand StopVideoCommand { get; }
        /// <summary>
        /// Комманда обнавления видеоустройств.
        /// </summary>
        public System.Windows.Input.ICommand RefreshVideoDeviceCommand { get; }
        public WebCamViewModel()
        {
            WebCam = new WebCam();
            PropertyWebCamShowCommand = new DelegateCommand(OnPropertyWebCamShowCommand);
            StartVideoCommand = new DelegateCommand(StartVideo);
            StopVideoCommand = new DelegateCommand(StopVideo);
            RefreshVideoDeviceCommand = new DelegateCommand(RefreshVideoDevice);
        }

        private void OnPropertyWebCamShowCommand()
        {
           WebCam.ShowProperty();
        }


        public void StartVideo()
        {
            if (WebCam.Source == null) return;
            WebCam.Start();
        }

        public void StopVideo()
        {
            WebCam.Notifly -= WebCam_Notifly;
            WebCam.Stop();

        }
        /// <inheritdoc />
        public void OnInitialized()
        {
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


        /// <inheritdoc />
        public object Settings { get; set; }
    }
}
