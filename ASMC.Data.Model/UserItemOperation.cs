using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ASMC.Data.Model.Interface;

namespace ASMC.Data.Model
{
    public interface IUserItemOperation<T>  : IUserItemOperationBase
    {
        List<IBasicOperation<T>> DataRow { get; set; }

    }
    /// <summary>
    /// Интерфейст описывающий подключаемые настройки утроств, необходимыех для выполнения операций.
    /// </summary>
    public class DeviceInterface    : IDevice
    {

        public string Value
        {
            get; set;
        }
        public string[] Aray { get; set; }

        public string[] Name
        {
            get; set;
        }
    }
    public interface IDevice
    {
        /// <summary>
        /// 
        /// </summary>
        string[] Name
        {
            get; set;
        }
        string Value
        {
            get; set;
        }
        string[] Aray
        {
            get; set;
        }

    }
    /// <summary>
    /// Полная структура Метрологического контроля
    /// </summary>
    public interface IUserItemOperation
    {
        void RefreshDevice();
     
        /// <summary>
        /// Перечень подключаемых устройств.
        /// </summary>
        IDevice[] Device
        {
            get;
        }
        /// <summary>
        /// Перечень операций
        /// </summary>
        IUserItemOperationBase[] UserItemOperation
        {
            get;
        }
         /// <summary>
         /// Перечень необхзодимого оборудования
         /// </summary>
        string[] Accessories { get; }
    }
    /// <summary>
    /// Содержет доступныйе виды Метрологического контроля.
    /// </summary>
    public interface IOperation
    {
        IUserItemOperation UserItemOperationFirsVerf { get; }
        IUserItemOperation UserItemOperationPeriodVerf { get;  }
        IUserItemOperation UserItemOperationCalibr
        {
            get; 
        }
    }
    /// <summary>
    /// Предоставляет интерфес пункта операции
    /// </summary>
    public interface IUserItemOperationBase
    {
        Guid Guid { get; } 
        void StartSinglWork(Guid guid);
        string Name
        {
            get; set;
        }
        bool IsSpeedWork
        {
            get; set;
        }
        ShemeImage Sheme
        {
            get; set;
        }
        bool? IsGood
        {
            get; set;
        }
        DataTable Data { get; }
        void StartWork(); 
    }
    public abstract class AbstractUserItemOperationBase  :IUserItemOperationBase
    {
        public Guid Guid { get; }  =new Guid();
        public abstract void StartSinglWork(Guid guid);

        public string Name { get; set; }
        public bool IsSpeedWork { get; set; }
        public ShemeImage Sheme { get; set; }
        public bool? IsGood { get; set; }
        public abstract void StartWork();
        public DataTable Data
        {
            get => FillData();
        } 
        protected abstract DataTable FillData();
    }
    public class ShemeImage
    {
        public string Path { get; set; }
        public int Number { get; set; }
    }
}
