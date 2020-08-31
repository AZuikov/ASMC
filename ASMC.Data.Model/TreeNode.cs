using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ASMC.Data.Model
{
    /// <summary>
    /// Сущность предоставляющая реализацию дерева
    /// </summary>
    public class TreeNode: ITreeNode
    {
        /// <inheritdoc />
        public bool? IsGood { get; set; }

        /// <inheritdoc />
        public bool IsWork { get; set; }

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
        public ITreeNode FirstNode
        {
            get
            {
                return  Nodes.FirstOrDefault();
            }
        }
        /// <summary>
        /// Позволяет получить последний узел
        /// </summary>
        public ITreeNode LastNode
        {
            get
            {
                return  Nodes.LastOrDefault();
            }
        }
      
        /// <summary>
        /// Позволяет получить родительский узел.
        /// </summary>
        public ITreeNode Parent
        {
            get;
            set;
        }
        public CollectionNode Nodes
        {
            get;
        }

        public TreeNode()
        {
            Nodes = new CollectionNode(this);
        }
    }
    public class CollectionNode :IList<ITreeNode>, IReadOnlyList<ITreeNode>
    {
        private readonly List<ITreeNode> _list;
        public ITreeNode Parent
        {
            get;
            protected set;
        }
        public CollectionNode(ITreeNode parent)
        {
            _list= new List<ITreeNode>();
            Parent = parent;
        }



        /// <inheritdoc />
        public IEnumerator<ITreeNode> GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        /// <inheritdoc />
        public void Add(ITreeNode item)
        {
            if (item==null) return;
            item.Parent = Parent;
            _list.Add(item);
        }

        /// <inheritdoc />
        public void Clear()
        {
          _list.Clear();
        }

        /// <inheritdoc />
        public bool Contains(ITreeNode item)
        {
            return _list.Contains(item);
        }

        /// <inheritdoc />
        public void CopyTo(ITreeNode[] array, int arrayIndex)
        {
            _list.CopyTo(array, arrayIndex);
        }

        /// <inheritdoc />
        public bool Remove(ITreeNode item)
        {
           return _list.Remove(item);
        }

        /// <inheritdoc />
        public int Count => _list.Count;

        /// <inheritdoc />
        public bool IsReadOnly { get; }


        /// <inheritdoc />
        public int IndexOf(ITreeNode item)
        {
            return _list.IndexOf(item);
        }

        /// <inheritdoc />
        public void Insert(int index, ITreeNode item)
        {
            _list.Insert(index, item);
        }

        /// <inheritdoc />
        public void RemoveAt(int index)
        {
           _list.RemoveAt(index);
        }

        /// <inheritdoc />
        public ITreeNode this[int index]
        {
            get => _list[index];
            set => _list[index]=value;
        }
    }


    public interface ITreeNode
    {
        /// <summary>
        /// Предоставляет результат операции
        /// </summary>
        // ReSharper disable once UnusedMember.Global
        bool? IsGood { get; set; }

        /// <summary>
        /// Флаг указывающий, что данная операция выполняется в текущий момент.
        /// </summary>
        // ReSharper disable once UnusedMember.Global
        bool IsWork { get; set; }

        /// <summary>
        /// Позволяет получать и задавать имя узла.
        /// </summary>
        string Name
        {
            get; set;
        }
        /// <summary>
        /// Позволяет получить первый узел.
        /// </summary>
        ITreeNode FirstNode
        {
            get;
        }
        /// <summary>
        /// Позволяет получить последний узел
        /// </summary>
        ITreeNode LastNode
        {
            get;
        }
        /// <summary>
        /// Позволяет получить родительский узел.
        /// </summary>
        ITreeNode Parent
        {
            get;
            set;
        }
         CollectionNode Nodes
        {
            get;
        }
    }
}
