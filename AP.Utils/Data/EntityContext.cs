using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using Dapper;

namespace AP.Utils.Data
{
    /// <summary>
    /// Представляет контекст
    /// данных сущности.
    /// </summary>
    public sealed class EntityContext
    {
        private static IList<Type> _mapperInitialized;

        #region Properties

        /// <summary>
        /// Возвращает или задает интерфейс
        /// доступа к источнику данных.
        /// </summary>
        public IDataProvider DataProvider
        {
            get; set;
        }

        #endregion

        /// <summary>
        /// Инициализирует новый экземпляр класса
        /// <see cref="EntityContext"/>.
        /// </summary>
        /// <param name="dataProvider">Задает интерфейс доступа
        /// к источнику данных.</param>
        public EntityContext(IDataProvider dataProvider)
        {
            DataProvider = dataProvider;
        }

        #region Methods

        /// <summary>
        /// Загружает данные с преобразованием в
        /// список сущностей указанного типа.
        /// </summary>
        /// <typeparam name="T">Тип сущности для отображения данных.</typeparam>
        /// <returns>Возвращает массив <see cref="T:T"/>.</returns>
        /// <param name="param">Задает анонимный тип для инициализации
        /// параметров, передаваемых в запрос при загрузке.</param>
        public T[] LoadMany<T>(object param = null) where T : new()
        {
            var t = typeof(T);

            var proc = t.GetCustomAttributes<StoredProcedureAttribute>()
                .FirstOrDefault(p => p.Operation == StoredProcedureOp.SelectMany)?.Name;
            if(string.IsNullOrEmpty(proc))
                throw new InvalidOperationException(proc);

            try
            {
                InitializeSqlMapper(t);
                return DataProvider.OpenConnection().Query<T>(proc, param)?.ToArray();
            }
            finally
            {
                DataProvider.CloseConnection();
            }
        }

        /// <summary>
        /// Загружает данные с преобразованием
        /// в сущность указанного типа.
        /// </summary>
        /// <typeparam name="T">Тип сущности для отображения данных.</typeparam>
        /// <param name="enitityKey">Значение уникального ключа записи данных.</param>
        /// <param name="param">Задает анонимный тип для инициализации
        /// параметров, передаваемых в запрос при загрузке.</param>
        public T LoadSingle<T>(object entityKey, object param = null) where T : new()
        {
            return (T)Load(typeof(T), entityKey, param);
        }

        /// <summary>
        /// Загружает данные с проецированием
        /// в свойства указанной сущности.
        /// </summary>
        /// <param name="entity">Сущность для отображения данных.</param>
        /// <param name="param">Задает анонимный тип для инициализации
        /// параметров, передаваемых в запрос при загрузке.</param>
        /// <returns>Возвращает истинно, если данные указанной сущности
        /// успешно загружены; иначе ложно.</returns>
        public bool Load(object entity, object param = null)
        {
            if(entity == null)
                throw new ArgumentNullException(nameof(entity));

            var key = GetEntityKey(entity, out var columnName);
            if(string.IsNullOrEmpty(columnName))
                throw new InvalidOperationException(entity.GetType().ToString());

            var src = Load(entity.GetType(), key, param);
            if(src == null)
                return false;

            Copy(src, entity);
            return true;
        }

        /// <summary>
        /// Сохраняет данные сущности.
        /// </summary>
        /// <param name="entity">Сущность, данные которой необходимо записать.</param>
        public void Save(object entity)
        {
            if(entity == null)
                throw new ArgumentNullException(nameof(entity));

            var t = entity.GetType();
            var proc = t.GetCustomAttributes<StoredProcedureAttribute>()
                .FirstOrDefault(p => p.Operation == StoredProcedureOp.Update)?.Name;
            if(string.IsNullOrEmpty(proc))
                throw new InvalidOperationException();

            //var param = ListSqlParams(DataProvider, proc);

            throw new NotImplementedException();
        }

        /// <summary>
        /// Удаляет данные объектной сущности.
        /// </summary>
        /// <param name="entity">Сущность, данные которой необходимо удалить</param>
        public void Delete(object entity)
        {
            if(entity == null)
                throw new ArgumentNullException(nameof(entity));

            var t = entity.GetType();
            var proc = t.GetCustomAttributes<StoredProcedureAttribute>()
                .FirstOrDefault(p => p.Operation == StoredProcedureOp.Delete)?.Name;
            if(string.IsNullOrEmpty(proc))
                throw new InvalidOperationException(t.ToString());

            var id = GetEntityKey(entity, out var columnName);
            if(string.IsNullOrEmpty(columnName))
                throw new InvalidOperationException(t.ToString());

            if(id != null)
                DataProvider.ExecuteNonQuery(proc, true, DataProvider.GetParameter(columnName, id));
        }

        /// <summary>
        /// Загружает данные сущности
        /// указанного типа.
        /// </summary>
        /// <typeparam name="TEntity">Тип сущности для загрузки данных.</typeparam>
        /// <param name="param">Задает анонимный тип для инициализации
        /// параметров, передаваемых в запрос при загрузке.</param>
        public DataTable LoadData<T>(object param = null)
        {
            var t = typeof(T);
            var proc = t.GetCustomAttributes<StoredProcedureAttribute>()
                .FirstOrDefault(p => p.Operation == StoredProcedureOp.SelectMany)?.Name;
            if(string.IsNullOrEmpty(proc))
                throw new InvalidOperationException(proc);

            var data = DataProvider.Execute(proc, true, GetSqlParameters(param));
            if(data != null)
            {
                var prop = t.GetProperties().FirstOrDefault(p => p.GetCustomAttribute<KeyAttribute>() != null);
                var key = prop?.GetCustomAttribute<ColumnAttribute>()?.Name ?? prop?.Name;

                if(key != null)
                    data.PrimaryKey = new[] { data.Columns[key] };
            }

            return data;
        }

        /// <summary>
        /// Преобразует данные в объектную сущность.
        /// </summary>
        /// <typeparam name="T">Тип объектной сущности для отображения данных.</typeparam>
        /// <param name="dataRow">Запись данных объектной сущности.</param>
        /// <returns>Возвращает экземпляр <see cref="T:T"/>.</returns>
        public T Parse<T>(DataRow dataRow) where T : new()
        {
            var mapper = new EntityMapper();
            return mapper.Map<T>(dataRow);
        }

        /// <summary>
        /// Преобразует данные в список объектных сущностей.
        /// </summary>
        /// <typeparam name="T">Тип сущности для отображения данных.</typeparam>
        /// <param name="dataTable">Таблица данных объектных сущностей.</param>
        /// <returns>Возвращает массив <see cref="T:T"/>.</returns>
        public T[] Parse<T>(DataTable dataTable) where T : new()
        {
            var mapper = new EntityMapper();
            return mapper.Map<T>(dataTable).ToArray();
        }

        /// <summary>
        /// Осуществляет поиск данных объектной сущности.
        /// </summary>
        /// <param name="dataTable">Таблица данных объектных сущностей.</param>
        /// <param name="entity">Сущность, данные которой необходимо найти.</param>
        /// <returns>Возвращает экземпляр <see cref="DataRow"/>.</returns>
        public DataRow Find(DataTable dataTable, object entity)
        {
            if(dataTable == null)
                throw new ArgumentNullException(nameof(dataTable));
            if(entity == null)
                return null;

            var t = entity.GetType();
            var key = GetEntityKey(entity, out var columnName);
            if(string.IsNullOrEmpty(columnName))
                throw new InvalidOperationException(t.ToString());

            return dataTable.Rows.Find(key);
        }

        private object Load(Type entityType, object entityKey, object param = null)
        {
            if(entityType == null)
                throw new ArgumentNullException(nameof(entityType));

            var proc = entityType.GetCustomAttributes<StoredProcedureAttribute>()
                .FirstOrDefault(p => p.Operation == StoredProcedureOp.Select);
            if(string.IsNullOrEmpty(proc?.Name))
                throw new InvalidOperationException(entityType.ToString());

            var paramName = proc.KeyName ?? GetEntityKey(entityType);
            if(string.IsNullOrEmpty(paramName))
                throw new InvalidOperationException(entityType.ToString());

         

            var parameter = GetSqlParameters(param).Concat(new[]
            {
                DataProvider.GetParameter($"@{paramName}",
                    proc.KeyFormat != null ? string.Format(proc.KeyFormat, entityKey) : entityKey)
            });

            var row = DataProvider.Execute(proc.Name, true, parameter.ToArray())?.Rows.Cast<DataRow>().FirstOrDefault();
            return row != null ? new EntityMapper().Map(row, entityType) : null;

           
        }

        private DbParameter[] GetSqlParameters(object param)
        {
            if(param == null)
                return new DbParameter[0];

            return param.GetType().GetProperties().Select(prop => DataProvider.GetParameter(prop.Name, prop.GetValue(param))).ToArray();
        }

        private string GetEntityKey(Type entityType)
        {
            var prop = entityType.GetProperties().FirstOrDefault(p => p.GetCustomAttribute<KeyAttribute>() != null);
            return prop?.GetCustomAttribute<ColumnAttribute>()?.Name ?? prop?.Name;
        }

        private object GetEntityKey(object entity, out string columnName)
        {
            var t = entity.GetType();
            var prop = t.GetProperties().FirstOrDefault(p => p.GetCustomAttribute<KeyAttribute>() != null);
            columnName = prop?.GetCustomAttribute<ColumnAttribute>()?.Name ?? prop?.Name;

            return prop?.GetValue(entity);
        }

        private static void InitializeSqlMapper(Type entityType)
        {
            if(_mapperInitialized == null)
                _mapperInitialized = new List<Type>();
            else if(_mapperInitialized.Contains(entityType))
                return;

            SqlMapper.SetTypeMap(entityType, new ForeignKeyAttributeTypeMapper(entityType));
            _mapperInitialized.Add(entityType);
        }

      

        private static void Copy(object source, object destination)
        {
            if(source == null)
                throw new ArgumentNullException(nameof(source));
            if(destination == null)
                throw new ArgumentNullException(nameof(destination));

            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public;

            var dest = destination.GetType().GetProperties(flags).Where(p => p.CanWrite).ToArray();
            foreach(var prop in source.GetType().GetProperties(flags).Where(p => p.CanRead))
            {
                var targetProp = dest.FirstOrDefault(p => p.Name == prop.Name);
                if(targetProp == null)
                    continue;

                if(!targetProp.PropertyType.IsAssignableFrom(prop.PropertyType))
                    continue;

                var setMethod = targetProp.GetSetMethod(true);
                if(setMethod != null && setMethod.IsPrivate)
                    continue;

                targetProp.SetValue(destination, prop.GetValue(source, null), null);
            }
        }



        #endregion
    }
}
