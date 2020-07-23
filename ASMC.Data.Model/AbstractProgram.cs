using ASMC.Data.Model.Interface;
using DevExpress.Mvvm;

namespace ASMC.Data.Model
{
    public abstract class AbstractProgram : IProgram
    {
        /// <inheritdoc />
        public string Type { get; protected set; }

        /// <inheritdoc />
        public string Grsi { get; protected set; }

        /// <inheritdoc />
        public string Range { get; protected set; }

        /// <inheritdoc />
        public string Accuracy { get; protected set; }

        /// <inheritdoc />
        public IMessageBoxService TaskMessageService
        {
            get => Operation?.TaskMessageService;
            set => Operation.TaskMessageService = value;
        }  
        /// <inheritdoc />
        public OperationBase Operation { get; protected set; }    = new OperationBase();
    }
}