using System.Collections.Generic;
using System.Linq;
using ASMC.Common.ViewModel;

namespace ASMC.Common.Model
{
    /// <summary>
    /// Сущность предоставляющая реализацию дерева
    /// </summary>
    public class TreeNode:BaseViewModel
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
                return Nodes.First();
            }
        }
        /// <summary>
        /// Позволяет получить последний узел
        /// </summary>
        public TreeNode LastNode
        {
            get
            {
                return Nodes.Last();
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

        protected TreeNode()
        {
            Nodes = new CollectionNode(this);
        }
    }
    public class CollectionNode : List<TreeNode>
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
}
