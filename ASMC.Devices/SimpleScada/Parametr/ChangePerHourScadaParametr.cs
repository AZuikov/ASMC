using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace ASMC.Devices.SimpleScada.Parametr
{
    public class ChangePerHourScadaParametr : ScadaParametrDecorator
    {
     
        public ChangePerHourScadaParametr(int id, IParametr parametr) : base(id, parametr)
        {
            Procedure = "Запрос_дельты_за_час_с_датчика";
        }

        //public override void FillValue()
        //{
        //   base.FillValue();
        //}
    }
}
