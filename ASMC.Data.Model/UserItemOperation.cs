using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
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
        public string Description { get; set; }
        public string[] Name { get; set; }
        public string SelectedName { get; set; }
        public string StringConnect { get; set; }
    }

    public interface IDevice
    {    
        string Description { get; set; }
        /// <summary>
        /// позволяет поучать или задавать перечень взаимозаменяемых устройств.
        /// </summary>
        string[] Name
        {
            get; set;
        } 
        /// <summary>
        /// Позволяет задать или получить имя выбранного прибора
        /// </summary>
        string SelectedName
        {
            get; set;
        }
        /// <summary>
        /// Позволяет задать или получить строку подключения к прибору
        /// </summary>
        string StringConnect
        {
            get; set;
        }

    }
    /// <summary>
    /// Предоставлет интерфес операций метрологического контроля.
    /// </summary>
    public interface IUserItemOperation
    {
        /// <summary>
        /// Выполняет обнавление списка устройств.
        /// </summary>
        void RefreshDevice();
     
        /// <summary>
        ///  Возвращает перечень подключаемых устройств.
        /// </summary>
        IDevice[] Device
        {
            get;
        }
        /// <summary>
        ///  Возвращает перечень операций
        /// </summary>
        IUserItemOperationBase[] UserItemOperation
        {
            get;
        }
        /// <summary>
        ///  Возвращает перечень необходимого оборудования.
        /// </summary>
        string[] Accessories { get; }
    }
    /// <summary>
    /// Содержет доступныйе виды Метрологического контроля.
    /// </summary>
    public abstract class AbstraktOperation
    {
        /// <summary>
        /// Позволяет задать или получить признак определяющий ускоренную работу.
        /// </summary>
        public bool IsSpeedWork
        {
            get; set;
        }
        /// <summary>
        /// Позволяет задать или получить тип операции.
        /// </summary>
        public TypeOpeation SelectedTypeOpeation { get; set; }

        public async void StartWorkAsync()
        {
            foreach (var opertion in SelectedOperation.UserItemOperation)
            {
                await Task.Run(() => opertion.StartWork());
            }                        
        }

        /// <summary>
        /// Позволяет получить выбранную операцию.
        /// </summary>
        public IUserItemOperation SelectedOperation
        {
            get
            {
                switch (SelectedTypeOpeation)
                {
                    case TypeOpeation.PrimaryVerf:
                        return  IsSpeedWork? SpeedUserItemOperationPrimaryVerf : UserItemOperationPrimaryVerf;
                    case TypeOpeation.PeriodicVerf:
                        return IsSpeedWork ? SpeedUserItemOperationPeriodicVerf : UserItemOperationPeriodicVerf;
                    case TypeOpeation.Calibration:
                        return IsSpeedWork ? SpeedUserItemOperationCalibration : UserItemOperationCalibration;
                }

                return null;
            }
        }

        /// <summary>
        /// Позволяет  задать или получить операции первичной поверки.
        /// </summary>
        protected IUserItemOperation UserItemOperationPrimaryVerf { get;  set; }
        /// <summary>
        /// Позволяет  задать или получить операции переодической поверки.
        /// </summary>
        protected IUserItemOperation UserItemOperationPeriodicVerf
        {
            get; set;
        }
        /// <summary>
        /// Позволяет  задать или получить операции калибровки.
        /// </summary>
        protected IUserItemOperation UserItemOperationCalibration
        {
            get; set;
        }
        /// <summary>
        /// Позволяет  задать или получить операции ускоренной первичной поверки.
        /// </summary>
        protected IUserItemOperation SpeedUserItemOperationPrimaryVerf
        {
            get; set;
        }
        /// <summary>
        /// Позволяет  задать или получить операции ускоренной переодической поверки.
        /// </summary>
        protected IUserItemOperation SpeedUserItemOperationPeriodicVerf
        {
            get; set;
        }
        /// <summary>
        /// Позволяет  задать или получить операции ускоренной калибровки.
        /// </summary>
        protected IUserItemOperation SpeedUserItemOperationCalibration
        {
            get; set;
        }
        /// <summary>
        /// Содержит перечесления типов операции.
        /// </summary>
        public  enum TypeOpeation
        {
            PrimaryVerf,
            PeriodicVerf,
            Calibration
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
         /// <summary>
         /// Позволяет 
         /// </summary>
        ShemeImage Sheme
        {
            get; 
        }
        bool? IsGood
        {
            get; set;
        }
        DataTable Data { get; }
        void StartWork(); 
    }
    public abstract class AbstractUserItemOperationBase: TreeNode,IUserItemOperationBase
    {
        /// <summary>
        /// Позволяет получить гуид операции.
        /// </summary>
        public Guid Guid { get; }  =new Guid();
        /// <summary>
        /// Запускает выполнение операций с указаном Гуиду.
        /// </summary>
        /// <param name="guid"></param>
        public abstract void StartSinglWork(Guid guid);
        /// <summary>
        /// Позволяет задать и получить признак ускоренного выполнения операций.
        /// </summary>
        public bool IsSpeedWork { get; set; }
        /// <summary>
        /// Gjpd
        /// </summary>
        public ShemeImage Sheme { get; set; }
        public bool? IsGood { get; set; }
        public abstract void StartWork();

        public DataTable Data
        {
            get => FillData();
        } 
        protected abstract DataTable FillData();
     

    }
    /// <summary>
    /// Сущность предоставляющая реализацию дерева
    /// </summary>
    public class TreeNode
    {
        /// <summary>
        /// Позволяет получать и задавать имя узла.
        /// </summary>
        public string Name
        {
            get; set;
        }
        /// <summary>
        /// Позволяет получить первый узел.
        /// </summary>
        public TreeNode FirstNode
        {
            get { return Nodes.First(); }
        }
        /// <summary>
        /// Позволяет получить последний узел
        /// </summary>
        public TreeNode LastNode
        {
            get { return Nodes.Last();}
        }
        /// <summary>
        /// Позволяет получить родительский узел.
        /// </summary>
        public TreeNode Parent
        {
            get { return Nodes.Parent; }
        }
        public CollectionNode Nodes { get; }

        protected TreeNode()
        {
            Nodes =  new CollectionNode(this);
        }
    }
    public class CollectionNode : List<TreeNode>
    {
        public TreeNode Parent { get; }
        public CollectionNode(TreeNode parent)
        {
            Parent = parent;
        }
    }
    public class ShemeImage
    {
        public string Path { get; set; }
        public int Number { get; set; }
    }
}
