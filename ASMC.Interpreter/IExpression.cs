using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASMC.Interpreter
{
    public interface IExpression
    {
        object Interpret(Context context);
    }
}
