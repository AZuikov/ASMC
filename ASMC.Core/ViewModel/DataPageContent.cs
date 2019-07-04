using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AP.Utils.Data;
using NLog;

namespace ASMC.Core.ViewModel
{
    ///// <summary>
    ///// Модель представления страницы данных.
    ///// </summary>
    //public class DataPageContent : BaseSelectorViewModel
    //{
    //    #region Fields

    //    private static readonly Logger Logger; /*= LogManager.GetCurrentClassLogger()*/

    //    private string _header;
    //    private bool _canClose = true;
    //    private object _entity;

    //    private CancellationTokenSource _tokenSource;

    //    #endregion

    //    #region Properties

    //    /// <summary>
    //    /// Возвращает или задает заголовок
    //    /// страницы данных.
    //    /// </summary>
    //    public string Header
    //    {
    //        get => _header;
    //        set => SetProperty(ref _header, value, nameof(Header));
    //    }

    //    /// <summary>
    //    /// Возвращает истинно, если пользователю
    //    /// возможно закрыть связанное представление;
    //    /// иначе ложно.
    //    /// </summary>
    //    public bool CanClose
    //    {
    //        get => _canClose;
    //        protected set => SetProperty(ref _canClose, value, nameof(CanClose));
    //    }

    //    /// <summary>
    //    /// Возвращает или задает сущность,
    //    /// соответствующую текущей записи
    //    /// данных.
    //    /// </summary>
    //    public object Entity
    //    {
    //        get => _entity;
    //        protected set => SetProperty(ref _entity, value, nameof(Entity));
    //    }

    //    /// <summary>
    //    /// Возвращает контекст доступа к
    //    /// данным объектных сущностей.
    //    /// </summary>
    //    protected EntityContext EntityContext => DataProvider?.GetEntityContext();

    //    #endregion

    //    /// <summary>
    //    /// Инициализирует новый экземпляр класса
    //    /// <see cref="DataPageContent"/>.
    //    /// </summary>
    //    public DataPageContent()
    //    {
    //    }

    //    #region Methods

    //    /// <inheritdoc />
    //    public sealed override void Refresh()
    //    {
    //        try
    //        {
    //            Data = ReloadData();
    //        }
    //        catch(Exception e)
    //        {
    //            Logger.Debug(e);
    //            if(!Alert(e))
    //                throw;
    //        }
    //    }

    //    /// <inheritdoc />
    //    public sealed override async Task RefreshAsync()
    //    {
    //        if(IsBusy)
    //            return;

    //        try
    //        {
    //            IsBusy = true;
    //            await Task.Factory.StartNew(Refresh);
    //        }
    //        catch(Exception e)
    //        {
    //            Logger.Error(e);
    //            if(!Alert(e))
    //                throw;
    //        }
    //        finally
    //        {
    //            IsBusy = false;
    //        }
    //    }

    //    /// <inheritdoc />
    //    public sealed override void AddRecord()
    //    {
    //        try
    //        {
    //            OnAddRecord(null);
    //        }
    //        catch(Exception e)
    //        {
    //            Logger.Error(e);
    //            if(!Alert(e))
    //                throw;
    //        }
    //    }

    //    /// <inheritdoc />
    //    public sealed override void EditRecord(DataRow dataRow)
    //    {
    //        try
    //        {
    //            if(dataRow != null)
    //                OnEditRecord(dataRow);
    //        }
    //        catch(Exception e)
    //        {
    //            Logger.Error(e);
    //            if(!Alert(e))
    //                throw;
    //        }
    //    }

    //    /// <inheritdoc />
    //    public sealed override void RemoveRecord(DataRow dataRow)
    //    {
    //        try
    //        {
    //            if(dataRow != null)
    //                OnRemoveRecord(dataRow);
    //        }
    //        catch(Exception e)
    //        {
    //            Logger.Error(e);
    //            if(!Alert(e))
    //                throw;
    //        }
    //    }

    //    /// <summary>
    //    /// Выполняет асинхронное отложенное проецирование
    //    /// данных записи на тип <see cref="T:TEntity"/>.
    //    /// </summary>
    //    /// <typeparam name="TEntity">Тип объекта.</typeparam>
    //    /// <param name="dataRow">Запись данных.</param>
    //    /// <param name="delay">Временной интервал задержки в миллисекундах.</param>
    //    protected async void ResolveEntityAsync<TEntity>(DataRow dataRow, int delay = 150) where TEntity : class, new()
    //    {
    //        try
    //        {
    //            if(_tokenSource != null)
    //            {
    //                _tokenSource.Cancel();
    //                _tokenSource.Dispose();
    //            }
    //            _tokenSource = new CancellationTokenSource();

    //            await Task.Delay(delay, _tokenSource.Token).ContinueWith(t =>
    //            {
    //                if(!t.IsCanceled)
    //                    Entity = EntityContext.Parse<TEntity>(dataRow);
    //            });
    //        }
    //        catch(Exception e)
    //        {
    //            Logger.Error(e);
    //            if(!Alert(e))
    //                throw;
    //        }
    //    }

    //    /// <summary>
    //    /// Осуществляет поиск записи
    //    /// с данными сущности
    //    /// </summary>
    //    /// <param name="entity">Сущность,
    //    /// данные по которой необходимо найти.</param>
    //    /// <returns>Возвращает экземпляр
    //    /// <see cref="DataRow"/>.</returns>
    //    protected DataRow Find(object entity)
    //    {
    //        try
    //        {
    //            return EntityContext.Find(Data, entity);
    //        }
    //        catch(Exception e)
    //        {
    //            Logger.Error(e);
    //            throw;
    //        }
    //    }

    //    /// <summary>
    //    /// Инициирует добавление новой записи
    //    /// на страницу данных.
    //    /// </summary>
    //    protected virtual void OnAddRecord(DataRow dataRow)
    //    {
    //        try
    //        {
    //            UpdateDataRow(dataRow);
    //        }
    //        catch(Exception e)
    //        {
    //            Logger.Error(e);
    //            throw;
    //        }
    //    }

    //    /// <summary>
    //    /// Инициирует добавление новой или
    //    /// обновление существующей записи на
    //    /// странице данных.
    //    /// </summary>
    //    protected virtual void OnEditRecord(DataRow dataRow)
    //    {
    //        try
    //        {
    //            UpdateDataRow(dataRow);
    //        }
    //        catch(Exception e)
    //        {
    //            Logger.Error(e);
    //            throw;
    //        }
    //    }

    //    /// <summary>
    //    /// Инициирует удаление существующей записи
    //    /// на странице данных.
    //    /// </summary>
    //    protected virtual void OnRemoveRecord(DataRow dataRow)
    //    {
    //        try
    //        {
    //            DeleteDataRow(dataRow);
    //        }
    //        catch(Exception e)
    //        {
    //            Logger.Error(e);
    //            throw;
    //        }
    //    }

    //    /// <summary>
    //    /// Инициирует обновление записей на
    //    /// странице данных из связанного
    //    /// источника данных.
    //    /// </summary>
    //    protected virtual DataTable ReloadData()
    //    {
    //        throw new NotImplementedException();
    //    }

    //    private static void UpdateDataRow(DataRow dataRow)
    //    {
    //        if(dataRow == null ||
    //            dataRow.RowState == DataRowState.Deleted)
    //            return;

    //        if(dataRow.RowState == DataRowState.Detached)
    //            dataRow.Table.Rows.Add(dataRow);
    //        else
    //        {
    //            var table = dataRow.Table;

    //            table.BeginLoadData();
    //            table.LoadDataRow(dataRow.ItemArray, LoadOption.OverwriteChanges);
    //            table.EndLoadData();
    //        }
    //    }

    //    private static void DeleteDataRow(DataRow dataRow)
    //    {
    //        if(dataRow != null &&
    //            (dataRow.RowState != DataRowState.Detached ||
    //            dataRow.RowState != DataRowState.Deleted))
    //            dataRow.Delete();
    //    }

    //    #endregion
    //}
}
