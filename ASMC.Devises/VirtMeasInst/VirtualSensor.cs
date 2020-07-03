using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using ASMC.Devises.Annotations;
using ASMC.Devises.SimpleScada;
using Palsys.Data.Model.Metr;

namespace ASMC.Devises.VirtMeasInst
{
    public class VirtualSensor: IMeasuredParametr , INotifyPropertyChanged
    {
        private MeasuredValue _measuredValue;
        private readonly double _minValue;
        private readonly double _maxValue;

        public VirtualSensor(double minValue, double maxValue)
        {
            _minValue = minValue;
            _maxValue = maxValue;
            PropertyChanged += OnPropertyChanged;
        }
        private void OnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            switch(propertyChangedEventArgs.PropertyName)
            {
                case nameof(MeasuredValue):
                    Name = $"{MeasuredValue?.Name}";
                    break;
            }
        }

        public bool IsChecked { get; set; }
        public int Id { get; set; }
        public double Value { get; set; }

        public string Name
        {
            get; set;
        }

        public void FillValue()
        {
            Value = new Random().NextDouble() * (_maxValue - _minValue) + _minValue;
        }

        public MeasuredValue MeasuredValue
        {
            get
            {
                return _measuredValue;
            }
            set
            {
                _measuredValue = value;
                OnPropertyChanged(nameof(MeasuredValue));
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
