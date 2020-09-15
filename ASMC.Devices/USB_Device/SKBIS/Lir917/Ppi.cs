using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using ASMC.Devices.IEEE.Keysight.ElectronicLoad;
using ASMC.Devices.USB_Device.SiliconLabs;

namespace ASMC.Devices.USB_Device.SKBIS.Lir917
{
    public class Ppi : Driver, IDisposable
    {
        /// <summary>
        /// Делитель
        /// </summary>
        private const double Divisor = 10;
        private readonly Timer _timer;
        public bool IsEnabledMeas { get; set; }
        public Ppi()
        {
            _timer = new Timer {Interval = 20, AutoReset = true};
        }
        public UsbExpressWrapper.Product Product
        {
            get
            {
                return NubmerDevice != null ? UsbExpressWrapper.GetProduct((int) NubmerDevice) : throw new Exception($"Номер устройства {UserType} не указан.");
            }
        }

        public void Initialization()
        {
            Open();
        }
        private void _timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Query();
            MeasValue = Relative+CalibrationFactor;
        }

        public void StartContinuousMeas()
        {
            if (IsEnabledMeas) return;
            _timer.Elapsed += _timer_Elapsed;
            _timer.Start();
            IsEnabledMeas = true;
        }

        public void StopMeas()
        {
            if (!IsEnabledMeas) return;
            _timer.Stop();
            _timer.Elapsed -= _timer_Elapsed;
            IsEnabledMeas = false;
        }

        /// <inheritdoc />
        public override void Dispose()
        {
            base.Dispose();
            _timer.Dispose();
        }

        public double CalibrationFactor { get;  set; }

        /// <summary>
        /// Коды напровления движения датчика
        /// </summary>
        public enum Motion
        {

            /// <summary>
            /// неопределенный ход
            /// </summary>
            [Description("Ход неопределен")]
            UndentUncertainMove,
            /// <summary>
            /// Прямой ход
            /// </summary>
            [Description("Прямой ход")]
            DerectNove,
            /// <summary>
            /// Обратный ход
            /// </summary>
            [Description("Обратный ход")]
            ReverseMove
        }
        /// <summary>
        /// Последнее измеренное значение
        /// </summary>
        protected double? LastMeas { get; set; }

        public Motion CurrentMotion { get; protected set; }
        private double _measValue;
        public double MeasValue
        {
            get => _measValue;
            protected set
            {
                _measValue = (value + CalibrationFactor) / Divisor;
                SetMeas();
            }
        }

        private void SetMeas()
        {
            if (LastMeas>MeasValue)
            {
                CurrentMotion = Motion.ReverseMove;
            }
            else if (LastMeas < MeasValue)
            {
                CurrentMotion = Motion.DerectNove;
            }
            else
            {
                CurrentMotion = Motion.UndentUncertainMove;
            }  
            LastMeas = MeasValue;
        }

    }
}
