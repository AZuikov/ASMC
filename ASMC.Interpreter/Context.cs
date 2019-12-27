using System.Collections.Generic;

namespace ASMC.Interpreter
{
    /// <summary>
    /// Содержит контент
    /// </summary>
    public class Context
    {
        private readonly Dictionary<string, object> _variables;

        private readonly Dictionary<string, MethodDeclaration> _methodes;

        public Context()
        {
            _variables = new Dictionary<string, object>();
            _methodes= new Dictionary<string, MethodDeclaration>();
        }

        public MethodDeclaration GetMethod(string indetifer)
        {
            return _methodes[indetifer];
        }
        public void SetMethod(string name, MethodDeclaration method)
        {
            if(_methodes.ContainsKey(name))
                _methodes[name] = method;
            else
                _methodes.Add(name, method);
        }
        public object GetVariable(string name)
        {
            return _variables[name];
        }

        public void SetVariabel(string name, object value)
        {
            if (_variables.ContainsKey(name))
                _variables[name] = value;
            else
                _variables.Add(name, value);
        }

        public Context Parent { get; set; }
        public IList<Context> Children { get; set; }
    }
    /// <summary>
    /// Переменные
    /// </summary>
    public class Variable
    {

        public string Indetifer
        {
            get; set;
        }
        public Context Parent { get; set; }
        public Context Body
        {
            get; set;
        }
    }

    public class MethodDeclaration : ITreeNode<Context>
    {
        public Context Argument { get; set; }
        public Context Parent { get; set; }
        public string Indetifer { get; set; }
        public IList<Context> Children { get; set; }
        public Context Body { get; set; }
    }

    /// <summary>
    /// Описыает дерево
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface ITreeNode <T>
    {
        T Parent { get; set; }
        IList<T> Children
        {
            get; set;
        }
    }
}