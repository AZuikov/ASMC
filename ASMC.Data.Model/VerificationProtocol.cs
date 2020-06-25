using System;
using ASMC.Data.Model.Interface;
using ASMC.Data.Model.SimpleScada;
using Palsys.Data.Model.Metr;

namespace ASMC.Data.Model
{
    public class VerificationProtocol : IProtocol
    {
        public enum PeriodicityTypes
        {
            NotFirst,
            First
        }

        public int PagesNumber { get; set; }
        public string Number { get; set; }
        public MiInstance[] StandartsList { get; set; }
        public DateTime? Date { get; set; }
        public Room Room { get; set; }
    }
}