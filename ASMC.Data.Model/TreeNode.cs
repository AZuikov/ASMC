using System.Collections.Generic;
using System.Linq;

namespace ASMC.Data.Model
{
    /// <summary>
    /// Сущность предоставляющая реализацию дерева
    /// </summary>
    public class TreeNode: ITreeNode
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
            get
            {
                return (TreeNode) Nodes.First();
            }
        }
        /// <summary>
        /// Позволяет получить последний узел
        /// </summary>
        public TreeNode LastNode
        {
            get
            {
                return (TreeNode) Nodes.Last();
            }
        }
        /// <summary>
        /// Позволяет получить родительский узел.
        /// </summary>
        public TreeNode Parent
        {
            get
            {
                return Nodes.Parent;
            }
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
    public class CollectionNode : List<ITreeNode>
    {
        public TreeNode Parent
        {
            get;
        }
        public CollectionNode(TreeNode parent)
        {
            Parent = parent;
        }
    }


    public interface ITreeNode
    {
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
         TreeNode FirstNode
        {
            get;
        }
        /// <summary>
        /// Позволяет получить последний узел
        /// </summary>
         TreeNode LastNode
        {
            get;
        }
        /// <summary>
        /// Позволяет получить родительский узел.
        /// </summary>
         TreeNode Parent
        {
            get;
        }
         CollectionNode Nodes
        {
            get;
        }
    }
}
