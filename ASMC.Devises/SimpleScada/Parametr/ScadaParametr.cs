using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Common;
using System.Runtime.CompilerServices;

using ASMC.Devises.Annotations;
using Palsys.Data.Model.Metr;
using Palsys.Utils.Data;

namespace ASMC.Devises.SimpleScada.Parametr
{
    public class ScadaParametr : IParametrScada, INotifyPropertyChanged
    {
        private  MeasuredValue _measuredValue;

        public ScadaParametr(int id, IDataProvider dataProvider) : this(dataProvider)
        {
            Id = id;
            Parameters = new[] {new Tuple<string, object>("Id", Id)};
        }

        public ScadaParametr(IDataProvider dp)
        {
            DataProvider = (IDataProvider) dp.Clone();
            DatebaseName = "OGMetr_Climate";
            DataProvider.Catalog = DatebaseName;
            Procedure = "Запрос_текущего_значения";
            PropertyChanged+= OnPropertyChanged;
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            switch (propertyChangedEventArgs.PropertyName)
            {
                case nameof(MeasuredValue):
                    Name = $"{MeasuredValue?.Name}";
                    break;
            }
        }

        public IDataProvider DataProvider { get; }
        public bool IsChecked { get; set; }
        public int Id { get; protected set; }
        public double Value { get; set; }
        public string Name { get; set; }

        public virtual void FillValue()
        {
            var arrayParametrs = new List<DbParameter>();
            foreach (var parametr in Parameters)
                arrayParametrs.Add(DataProvider.GetParameter(parametr.Item1, parametr.Item2));
            var value = DataProvider.ExecuteScalar(Procedure, true, arrayParametrs.ToArray());
            Value = Convert.ToDouble(value);
        }

        public string DatebaseName { get; set; }
        public string Procedure { get; set; }
        public Tuple<string, object>[] Parameters { get; set; }

        public MeasuredValue MeasuredValue
        {
            get { return _measuredValue; }
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