using ASMC.Data.Model;
using ASMC.Data.Model.Interface;

namespace ASMC.Core.Model
{
    public abstract class Program : IProgram
    {
        protected Program(ServicePack service)
        {
            Service = service;
        }
        /// <inheritdoc />
        public string Type { get; protected set; }

        /// <inheritdoc />
        public string Grsi { get; protected set; } 
        /// <inheritdoc />
        public string Range { get; protected set; } 
        /// <inheritdoc />
        public string Accuracy { get; protected set; } 
       
        /// <inheritdoc />
        public OperationMetrControlBase Operation { get; protected set; }
       
        public ServicePack Service { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            return $@"{Type} {Range} {Accuracy} {Grsi}";
        }
    }
}