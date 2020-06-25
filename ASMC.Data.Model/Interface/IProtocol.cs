using System;
using ASMC.Data.Model.SimpleScada;
using Palsys.Data.Model.Metr;

namespace ASMC.Data.Model.Interface
{
    public interface IProtocol
    {

        /// <summary>
        /// Возвращает или задает количество страниц в протаколе.
        /// </summary>
        int PagesNumber
        {
            get; set;
        }
        /// <summary>
        /// Возвращает или задает номер протокола.
        /// </summary>
        string Number
        {
            get; set;
        }
        /// <summary>
        /// Возвращает или задает таблицу со средствами поверки.
        /// </summary>
        MiInstance [] StandartsList
        {
            get;
            set;
        }
        /// <summary>
        /// Возвращает или задает номер протокола.
        /// </summary>
        DateTime? Date
        {
            get; set;
        }
        /// <summary>
        /// Возвращает или задает место проведения.
        /// </summary>
        Room Room { get; set; }
    }
}
