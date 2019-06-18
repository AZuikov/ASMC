using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AP.Utils.Data
{
    /// <summary>
    /// Представляет интерфейс контекста
    /// управления данными сущности.
    /// </summary>
    public interface IEntityContext
    {
        /// <summary>
        /// Загружает данные сущности
        /// указанного типа.
        /// </summary>
        /// <typeparam name="TEntity">Тип сущности для загрузки данных.</typeparam>
        /// <param name="param">Задает анонимный тип для инициализации
        /// параметров, передаваемых в запрос при загрузке.</param>
        DataTable LoadData<TEntity>(object param = null);

        /// <summary>
        /// Загружает данные с преобразованием в
        /// список сущностей указанного типа.
        /// </summary>
        /// <typeparam name="T">Тип сущности для отображения данных.</typeparam>
        /// <returns>Возвращает массив <see cref="T:T"/>.</returns>
        /// <param name="param">Задает анонимный тип для инициализации
        /// параметров, передаваемых в запрос при загрузке.</param>
        T[] LoadMany<T>(object param = null);

        /// <summary>
        /// Загружает данные с преобразованием
        /// в сущность указанного типа.
        /// </summary>
        /// <typeparam name="T">Тип сущности для отображения данных.</typeparam>
        /// <param name="enitityKey">Значение уникального ключа записи данных.</param>
        /// <param name="param">Задает анонимный тип для инициализации
        /// параметров, передаваемых в запрос при загрузке.</param>
        T LoadSingle<T>(object enitityKey, object param = null);

        /// <summary>
        /// Загружает данные с проецированием
        /// в свойства указанной сущности.
        /// </summary>
        /// <param name="entity">Сущность для отображения данных.</param>
        /// <param name="param">Задает анонимный тип для инициализации
        /// параметров, передаваемых в запрос при загрузке.</param>
        /// <returns>Возвращает истинно, если данные указанной сущности
        /// успешно загружены; иначе ложно.</returns>
        bool Load(object entity, object param = null);

        /// <summary>
        /// Сохраняет данные сущности.
        /// </summary>
        /// <param name="entity">Сущность, данные которой необходимо записать.</param>
        void Save(object entity);

        /// <summary>
        /// Удаляет данные объектной сущности.
        /// </summary>
        /// <param name="entity">Сущность, данные которой необходимо удалить</param>
        void Delete(object entity);

        /// <summary>
        /// Преобразует данные в объектную сущность.
        /// </summary>
        /// <typeparam name="T">Тип объектной сущности для отображения данных.</typeparam>
        /// <param name="dataRow">Запись данных объектной сущности.</param>
        /// <returns>Возвращает экземпляр <see cref="T:T"/>.</returns>
        T Parse<T>(DataRow dataRow) where T : new();

        /// <summary>
        /// Преобразует данные в список объектных сущностей.
        /// </summary>
        /// <typeparam name="T">Тип сущности для отображения данных.</typeparam>
        /// <param name="dataTable">Таблица данных объектных сущностей.</param>
        /// <returns>Возвращает массив <see cref="T:T"/>.</returns>
        T[] Parse<T>(DataTable dataTable) where T : new();

        /// <summary>
        /// Осуществляет поиск данных объектной сущности.
        /// </summary>
        /// <param name="dataTable">Таблица данных объектных сущностей.</param>
        /// <param name="entity">Сущность, данные которой необходимо найти.</param>
        /// <returns>Возвращает экземпляр <see cref="DataRow"/>.</returns>
        DataRow Find(DataTable dataTable, object entity);
    }
}
