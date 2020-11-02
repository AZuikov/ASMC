using System;
using System.IO;
using System.Linq;
using System.Reflection;
using ASMC.Core.ViewModel;
using DevExpress.Xpf.Core.Native;

namespace ASMC.Common.ViewModel
{
    public class QuestionTextViewModel : SelectionViewModel
    {
        private string _resultStr;
        private bool _checkBox=true;
        private string _description;
        private string _fileNameDescription;
        private string AssemblyLocalName { get; set; }

        /// <inheritdoc />
        protected override void OnEntityChanged()
        {
            base.OnEntityChanged();
                var enter = Entity as Tuple<string, Assembly>;
                if (enter != null)
                {
                    AssemblyLocalName = Path.GetFileNameWithoutExtension(enter.Item2?.ManifestModule.Name);
                    FileNameDescription = enter.Item1;
            }
        }

        public string Description
        {
            get => _description;
            set => SetProperty(ref _description, value, nameof(Description), ChangedSelected);
        }
        public string ResultStr
        {
            get => _resultStr;
            set => SetProperty(ref _resultStr, value, nameof(ResultStr), ChangedSelected);
        }

        private void ChangedSelected()
        {
            Entity = new Tuple<string,bool>(!CheckBox ? "Не соответствует, по причине: " + ResultStr : "Соответствует", CheckBox);
        }

        public bool CheckBox
        {
            get => _checkBox;
            set => SetProperty(ref _checkBox, value, nameof(CheckBox), ChangedSelected);
        }
        public string FileNameDescription
        {
            get => _fileNameDescription;
            set => SetProperty(ref _fileNameDescription, value, nameof(FileNameDescription), ChangedPath);
        }

        private void ChangedPath()
        {
            
            var path = $@"{Directory.GetCurrentDirectory()}\Plugins\{AssemblyLocalName}";
            if (!Directory.Exists(path))
                return;
            var docPath = Directory.GetFiles(path, FileNameDescription+".rtf", SearchOption.AllDirectories).FirstOrDefault();
            if (docPath == null) return;
            using (var fs = File.Open(docPath, FileMode.Open))
            {
                Description = fs.ReadString();
            }
        }

        /// <inheritdoc />
        protected override void OnInitializing()
        {
            base.OnInitializing();

            Entity = new Tuple<string, bool>(null, true);
        }
    }
}