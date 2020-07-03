

namespace ASMC.Devises.SimpleScada.Parametr
{
    public abstract class ScadaParametrDecorator: ScadaParametr
    {
        protected IParametr Parametr;

        protected ScadaParametrDecorator(int id, IParametr parametr) : base(id,((ScadaParametr)parametr).DataProvider)
        {
            this.Parametr = parametr;
        }

    }
}
