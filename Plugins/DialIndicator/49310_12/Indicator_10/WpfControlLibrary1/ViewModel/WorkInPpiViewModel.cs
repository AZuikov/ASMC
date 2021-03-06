﻿using System;
using System.Linq;
using System.Timers;
using System.Windows.Input;
using ASMC.Common.ViewModel;
using ASMC.Core.ViewModel;
using ASMC.Devices.UInterface.RemoveDevice.ViewModel;
using ASMC.Devices.USB_Device.SKBIS.Lir917;
using DevExpress.Mvvm;
using NLog;

namespace mp2192_92.DialIndicator.ViewModel
{
    public class WorkInPpiViewModel: SelectionViewModel
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private TableViewModel _content;
        private Ppi _ppi;
        public ICommand SetNullCommand { get; }
        public WorkInPpiViewModel()
        {
            SetNullCommand = new DelegateCommand(OnSetNull);
            WebCam = new WebCamViewModel();
            Ppi = new Ppi();
            _timer = new Timer(50);
            _timer.Elapsed += _timer_Elapsed;
        }

        private void OnSetNull()
        {
            try
            {
                Ppi.ResetAll();
            }
            catch (Exception e)
            {
                Alert(e);
            }
        }

        private void _timer_Elapsed(object sender, ElapsedEventArgs args)
        {
            if (Content?.Selected!=null)
            {
                try
                {
                    Content.Selected.Value = Ppi.MeasValue;
                }
                catch(Exception e)
                {
                    _timer.Elapsed -= _timer_Elapsed;
                    Alert(e);
                }
            }
        }

        private readonly Timer _timer;

        public Ppi Ppi
        {
            get => _ppi;
            set => SetProperty(ref _ppi, value, nameof(Ppi));
        }
        /// <inheritdoc />
        protected override void OnInitialized()
        {
            try
            {
                base.OnInitialized();
                WebCam.OnInitialized();
                WebCam.StartVideo();
                Ppi.Initialization();
                Ppi.StartContinuousMeas();
                _timer.Start();
            }
            catch (Exception e)
            {
                Alert(e);
            }
           
        }

        /// <inheritdoc />
        public override void Close()
        {
            Ppi.StopMeas();
            Ppi.Dispose();
            WebCam.StopVideo();
            _timer.Elapsed -= _timer_Elapsed;
            _timer.Dispose();
            base.Close();
        }

        /// <inheritdoc />
        protected override bool CanSelect()
        {
            return _content.Cells.All(p => !string.IsNullOrWhiteSpace(p?.Value?.ToString()));
        }

        public WebCamViewModel WebCam { get; } 

        public TableViewModel Content { get=> _content; set=>SetProperty(ref _content, value, nameof(Content)); }
    }
}
