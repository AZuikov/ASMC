using ASMC.Data.Model.Interface;
using Palsys.Data.Model.Metr;

namespace ASMC.Data.Model.Devises.Parametr
{
    public interface IMeasuredParametr:IParametr
    {
        MeasuredValue MeasuredValue
        { get; set; }
    }

   
}
