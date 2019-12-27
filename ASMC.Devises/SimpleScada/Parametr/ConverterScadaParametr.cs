using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Palsys.Utils.Data;

namespace ASMC.Devises.SimpleScada.Parametr
{
    public class ConverterScadaParametr: ScadaParametr
    {
        public Func<double,double > Func  { get; set; }
        public ConverterScadaParametr(int id, IDataProvider dataProvider) : base(id, dataProvider)
        {
           
           
        }

        public ConverterScadaParametr(IDataProvider dp) : base(dp)
        {
        }

        public override void FillValue()
        {
            base.FillValue();
            Value = Func(Value);
        }
    }
}
