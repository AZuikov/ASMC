using ASMC.Data.Model;
using ASMC.Data.Model.Interface;

namespace APPA_107N_109N
{
    public class APPA_107N : IProrgam

    {
        public APPA_107N()
        {
            Type = "Мультиметр цифровой";
            Grsi = "20085-11";
            Range = "Пост. напр. 0 - 1000 В, пер. напр. 0 - 750 В,\n" +
                    " пост./пер. ток 0 - 10 А, изм. частоты до 1 МГц,\n" +
                    " эл. сопр. 0 - 2 ГОм, эл. ёмкость до 40 мФ.";
            Accuracy = "DCV 0.06%, ACV 1%, DCI 0.2%, ACI 1.2%,\n" +
                       " FREQ 0.01%, OHM 5%, FAR 1.5%";
        }

        public string Type { get; }
        public string Grsi { get; }
        public string Range { get; }
        public string Accuracy { get; }
        public AbstraktOperation AbstraktOperation { get; }
    }
}
