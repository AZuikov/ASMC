﻿using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Documents;
using ASMC.Common.ViewModel;
using DevExpress.Xpf.Core.Native;

namespace ASMC.Core.ViewModel
{
    public class QuestionTextViewModel : FromBaseViewModel
    {
        private string _resultStr;
        private bool _checkBox;
        private string _description;
        private string _fileNameDescription;
        private string AssemblyLocalName { get; set; }
        public QuestionTextViewModel()
        {
            this.AllowSelect = true;
        }

        /// <inheritdoc />
        protected override void OnEntityChanged()
        {
            base.OnEntityChanged();
            var enter = Entity as (string, string)? ?? (null, null);
            AssemblyLocalName = Path.GetFileNameWithoutExtension(enter.Item2);
            FileNameDescription = enter.Item1;
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
            Entity = (document: ResultStr, check: CheckBox);
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

    }
}