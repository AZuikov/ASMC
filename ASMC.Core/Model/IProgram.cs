namespace ASMC.Core.Model
{
    public interface IProgram
    {

     
        /// <summary>
        /// Позволяет получать тип СИ
        /// </summary>
        string Type
        {
            get; 
        }
        /// <summary>
        /// Позволяет получать госреестр СИ
        /// </summary>
        string Grsi
        {
            get; 
        }
        /// <summary>
        /// Позволяет получать диапазон СИ
        /// </summary>
        string Range
        {
            get;
        }
        /// <summary>
        /// Позволяет получить характеристику точности СИ
        /// </summary>
        string Accuracy
        {
            get;
        }
        /// <summary>
        /// Позволяет получить сушность метрологических операций
        /// </summary>
        OperationMetrControlBase Operation
        {
            get;
        }
    }
}
