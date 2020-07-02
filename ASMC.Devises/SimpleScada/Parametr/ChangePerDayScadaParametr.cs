

using System;

namespace ASMC.Devises.SimpleScada.Parametr
{
    public class ChangePerDayScadaParametr : ScadaParametrDecorator
    {
        public ChangePerDayScadaParametr(int id, IParametr parametr) : base(id, parametr)
        {
            Procedure = "Запрос_среднего_за_указаный_с_датчика";
            Parameters = new[] { new Tuple<string, object>("Id", Id), new Tuple<string, object>("mydate", DateTime.Now) };
        }
    }
}
