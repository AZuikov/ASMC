namespace ASMC.Data.Model.Interface
{
   public interface IEmployees
    {
        /// <summary>
        /// Имя
        /// </summary>
        string Name { get; set; }
        /// <summary>
        /// Фамилия
        /// </summary>
        string Surname { get; set; }
        /// <summary>
        /// Отчетсво
        /// </summary>
        string Patronymic { get; set; }
        /// <summary>
        /// ФИО полность
        /// </summary>
        string FullName { get; }

        /// <summary>
        /// Фамилия И.О.
        /// </summary>
        string LittleName { get; }
    }
}
