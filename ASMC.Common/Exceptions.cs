using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ASMC.Common
{
    public static class Exceptions
    {
        /// <summary>
        /// Исключения считающияся фатальными
        /// </summary>
        private  static  readonly  List<Type> FatalExceptionsList = new List<Type>()
        {
            typeof(OutOfMemoryException),
            typeof(StackOverflowException),
            typeof(ArgumentOutOfRangeException),
            typeof(IndexOutOfRangeException),
            typeof(NullReferenceException),
            typeof(ArgumentNullException)
        };
        /// <summary>
        /// Сборка сообщения 
        /// </summary>
        /// <param name="ex"></param>
        /// <returns></returns>
        public static string FullMessage(this Exception ex)
        {
            var  builder = new  StringBuilder();
            while (ex!=null)
            {
                builder.AppendFormat("{0}{1}", ex, Environment.NewLine);
                ex = ex.InnerException;
            }
            return builder.ToString();
        }

        private static bool NotFatal(this Exception ex)
        {
            return FatalExceptionsList.All(curFatal => ex.GetType() != curFatal);
        }
        public static bool IsFatal(this Exception ex)
        {
            return !NotFatal(ex);
        }
        public static void TryFilterCatch(Action tryAction, Func<Exception, bool> isReciverPossible,
            Action handlerAction)
        {
            try
            {
                tryAction();
            }
            catch (Exception ex)
            { 
                if(!isReciverPossible(ex)) throw;
                handlerAction();
            }
        }
    }
}
