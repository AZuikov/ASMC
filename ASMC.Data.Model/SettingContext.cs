using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using Palsys.Utils;
using Palsys.Utils.Data;
using Palsys.Utils.Localization;

namespace ASMC.Data.Model
{
    public class SettingContext
    {


        public IDataProvider DataProvider { get; set; }

        public DataTable LoadData<TSetting>(object param = null)
        {
            throw new NotImplementedException();
        }

        public T[] LoadMany<T>(object param = null) where T : new()
        {
            var t = typeof(T);

            var proc = t.GetCustomAttributes<StoredProcedureAttribute>()
                .FirstOrDefault(p => p.Operation == StoredProcedureOp.SelectMany)?.Name;
            if(string.IsNullOrEmpty(proc))
                throw new InvalidOperationException(
                    string.Format(LocalizationManager.Default["SettingContextSelectManyProcedureUndefinedError"], t.Name));

            try
            {
                var data = LoadData<T>(param);
                return data != null ? Parse<T>(data) : new T[0];
            }
            finally
            {
                DataProvider.CloseConnection();
            }
        }

        public T LoadSingle<T>(object enitityKey, object param = null) where T : new()
        {
            return (T)Load(typeof(T), enitityKey, param);
        }

        public bool Load(object entity, object param = null)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            var key = GetEntityKey(entity, out var columnName);
            if (string.IsNullOrEmpty(columnName))
                throw new InvalidOperationException(string.Format(
                    LocalizationManager.Default["SettingContextKeyUndefinedError"], entity.GetType()));

            if (key == null)
                return false;

            var src = Load(entity.GetType(), key, param);
            if (src == null)
                return false;

            src.ShallowCopy(entity);
            return true;
        }

        public void Save(object entity)
        {
            throw new NotImplementedException();
        }

        public void Delete(object entity)
        {
            throw new NotImplementedException();
        }

        public T Parse<T>(DataRow dataRow) where T : new()
        {
            throw new NotImplementedException();
        }

        public T[] Parse<T>(DataTable dataTable) where T : new()
        {
            throw new NotImplementedException();
        }

        public DataRow Find(DataTable dataTable, object entity)
        {
            throw new NotImplementedException();
        }

        protected static string ExtendSqlQuery(string query, IEnumerable<DbParameter> parameters)
        {
            var sb = new StringBuilder(query);
            var f = true;

            foreach (var prm in parameters)
            {
                if (f)
                {
                    sb.Append(CultureInfo.CurrentCulture.CompareInfo.IndexOf(query, "where",
                                  CompareOptions.OrdinalIgnoreCase) < 0
                        ? " where ("
                        : " and (");
                    f = false;
                }
                else
                {
                    sb.Append(" and ");
                }

                sb.Append(prm.ParameterName + "=");
                sb.Append("@" + prm.ParameterName);
            }

            if (!f)
                sb.Append(")");

            return sb.ToString();
        }

        protected static object GetEntityKey(object setting, out string columnName)
        {
            if (setting == null)
                throw new ArgumentNullException(nameof(setting));

            var t = setting.GetType();
            var prop = t.GetProperties().FirstOrDefault(p => p.GetCustomAttribute<KeyAttribute>() != null);

            columnName = prop?.GetCustomAttribute<TableCellAttribute>()?.Name ?? prop?.Name; //CheckKey(t);
            return prop?.GetValue(setting);
        }

        protected static string GetSettingKey(Type entityType)
        {
            var prop = entityType.GetProperties().FirstOrDefault(p => p.GetCustomAttribute<KeyAttribute>() != null);
            return prop?.GetCustomAttribute<TableCellAttribute>()?.Name ?? prop?.Name;
        }

        /// <summary>
        ///     Формирует список параметров из списка пар
        ///     имя-значение для передачи в SQL запрос.
        /// </summary>
        /// <param name="param">Список пар имя-значение.</param>
        /// <returns>Возвращает массив <see cref="DbParameter" />.</returns>
        protected DbParameter[] GetSqlParameters(IEnumerable<KeyValuePair<string, object>> param)
        {
            return param?.Select(p => DataProvider.GetParameter(p.Key, p.Value)).ToArray() ?? new DbParameter[0];
        }

        /// <summary>
        ///     Формирует список параметров из свойств
        ///     анонимного типа для передачи в SQL запрос.
        /// </summary>
        /// <param name="param">Объект, содержащий набор значений параметров.</param>
        /// <returns>Возвращает массив <see cref="DbParameter" />.</returns>
        protected DbParameter[] GetSqlParameters(object param)
        {
            if (param == null)
                return new DbParameter[0];
            if (param is DynamicParam)
                return ((DynamicParam) param).AsSqlParameters(DataProvider).ToArray();

            return param.GetType().GetProperties()
                .Select(prop => DataProvider.GetParameter(prop.Name, prop.GetValue(param))).ToArray();
        }

        private object Load(Type settingType, object entityKey, object param = null)
        {
            if (settingType == null)
                throw new ArgumentNullException(nameof(settingType));

            var proc = settingType.GetCustomAttributes<StoredProcedureAttribute>()
                .FirstOrDefault(p => p.Operation == StoredProcedureOp.Select);
            if (string.IsNullOrEmpty(proc?.Name))
                throw new InvalidOperationException(string.Format(
                    LocalizationManager.Default["EntityContextSelectProcedureUndefinedError"], settingType));

            var paramName = proc.KeyName ?? GetSettingKey(settingType);
            if (string.IsNullOrEmpty(paramName))
                throw new InvalidOperationException(
                    string.Format(LocalizationManager.Default["EntityContextKeyUndefinedError"], settingType));

            var keyParam = DataProvider.GetParameter($"@{paramName}",
                proc.KeyFormat != null ? string.Format(proc.KeyFormat, entityKey) : entityKey);

            var fixedParams = GetSqlParameters(proc.GetParams()).Concat(new[] {keyParam});
            var prm = fixedParams.Concat(GetSqlParameters(param)).ToArray(); //TODO Return only distinct params
            var query = proc.IsStoredProcedure ? proc.Name : ExtendSqlQuery(proc.Name, prm);

            var row = DataProvider.Execute(query, proc.IsStoredProcedure, prm)?.Rows.Cast<DataRow>().FirstOrDefault();
            return row != null ? new EntityMapperSetting().Map(row, settingType) : null;
        }
    }
}