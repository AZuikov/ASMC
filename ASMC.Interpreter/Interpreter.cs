using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASMC.Interpreter
{
    public class Interpreter
    {

    }
    public class TerminalExpression : IExpression
    {
        private readonly string _name;
        public TerminalExpression(string variableName)
        {
            _name = variableName;
        }
        public dynamic Interpret(Context context)
        {
            return context.GetVariable(_name);
        }
    }
    public class AddNotTerminalExpression : IExpression
    {
        private readonly IExpression _leftExpression;
        private readonly IExpression _rigchtExpression;
        public AddNotTerminalExpression(IExpression left, IExpression right)
        {
            _leftExpression = left;
            _rigchtExpression = right;

        }
        public object Interpret(Context context)
        {
            if (decimal.TryParse(_leftExpression.Interpret(context).ToString(),out var a)&& decimal.TryParse(_rigchtExpression.Interpret(context).ToString(), out var b))
            {
                return a + b;
            }
           return _leftExpression.Interpret(context).ToString() + _rigchtExpression.Interpret(context);
        }
    }
    public class SubNotTerminalExpression : IExpression
    {
        private readonly IExpression _leftExpression;
        private readonly IExpression _rigchtExpression;
        public SubNotTerminalExpression(IExpression left, IExpression right)
        {
            _leftExpression = left;
            _rigchtExpression = right;

        }
        public object Interpret(Context context)
        {
            if(decimal.TryParse(_leftExpression.Interpret(context).ToString(), out var a) && decimal.TryParse(_rigchtExpression.Interpret(context).ToString(), out var b))
            {
                return a - b;
            }
            return null;
        }
    }
}
