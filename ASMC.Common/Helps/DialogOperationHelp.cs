using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using ASMC.Core.Model;
using ASMC.Data.Model;
using FastMember;
using NLog;

namespace ASMC.Common.Helps
{
    public sealed class DialogOperationHelp : BasicOperation<bool>
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        public DialogOperationHelp(IUserItemOperationBase operation, string nameFile, Assembly findProjectFile=null)
        {
            Logger.Debug($@"Указан файл для загрузки {nameFile} и будет поиск в папке сборке {findProjectFile?? Assembly.GetExecutingAssembly()}");
            Expected = true;
            IsGood = () => Equals(Expected,Getting);
            InitWork = () =>
            {
                var accessor = TypeAccessor.Create(operation.GetType());
                var service = ((IUserItemOperation)accessor[operation, nameof(ParagraphBase<object>.UserItemOperation)]).ServicePack.QuestionText(); 
                service.Title = operation.Name;
                service.Entity = new Tuple<string, Assembly>(nameFile, findProjectFile);
                //Bug необходима отмена операции
                if (service.Show() != true)
                {
                    Logger.Info($@"Операция {operation.Name} была отменена");
                    return default;
                }
                var res = service.Entity as Tuple<string, bool>;
                Getting = res.Item2;
                Comment = res.Item1;
                operation.IsGood = Getting;
                return Task.CompletedTask;
            };
            this.CompliteWork = () => Task.FromResult(true);
        }

        public DialogOperationHelp(IUserItemOperationBase operation, Func<Task<bool>> initWork, string nameFile,
            Assembly findProjectFile = null)
        {
            Logger.Debug($@"Указан файл для загрузки {nameFile} и будет поиск в папке сборке {findProjectFile ?? Assembly.GetExecutingAssembly()}");
            Expected = true;
            IsGood = () => Equals(Expected, Getting);
            InitWork = async () =>
            {
                var accessor = TypeAccessor.Create(operation.GetType());
                var service = ((IUserItemOperation)accessor[operation, nameof(ParagraphBase<object>.UserItemOperation)]).ServicePack.QuestionText();
                service.Title = operation.Name;
                service.Entity = new Tuple<string, Assembly>(nameFile, findProjectFile);
                //Bug необходима отмена операции
                if (service.Show() != true)
                {
                    Logger.Info($@"Операция {operation.Name} была отменена");
                    return;
                }
                var res = service.Entity as Tuple<string, bool>;
                Getting = res.Item2 && initWork().Result;
                Comment = res.Item1;
                operation.IsGood = Getting;
            };
            this.CompliteWork = () => Task.FromResult(true);
        }
    }
}
