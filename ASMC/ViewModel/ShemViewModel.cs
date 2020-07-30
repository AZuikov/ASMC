using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using ASMC.Core.ViewModel;
using ASMC.Data.Model;
using DevExpress.Xpf.Core.Native;

namespace ASMC.ViewModel
{
    public class ShemViewModel : FromBaseViewModel
    {
        private ShemeImage _shema;
        private string _pathImage;
        private string _text;

        /// <summary>
        /// ПОзволяет получать или задавать  отображенную схему.
        /// </summary>
        public ShemeImage Shema
        {
            get => _shema;
            set => SetProperty(ref _shema, value, nameof(Shema), ChangedCallback);
        }

        protected override void OnEntityChanged()
        {
            Shema= Entity as ShemeImage ;   
            base.OnEntityChanged();
        }
        public string Text 
        {
            get => _text;
            set => SetProperty(ref _text, value, nameof(Text));
        }
        public string PathImage
        {
            get => _pathImage;
            set => SetProperty(ref _pathImage, value, nameof(PathImage));
        }
        private void ChangedCallback()
        {
          
            
            var path = $@"{Directory.GetCurrentDirectory()}\Plugins";
            if(!Directory.Exists(path))
                return;
            PathImage = Directory.GetFiles(path, Shema.FileName, SearchOption.AllDirectories).FirstOrDefault();
            if(Shema.FileNameDescription==null)   return;
               var docPath = Directory.GetFiles(path, Shema.FileNameDescription, SearchOption.AllDirectories).FirstOrDefault();
            if (docPath == null) return;
            using (var fs = File.Open(docPath, FileMode.Open))
            {
                Text = fs.ReadString();
            }
        } 
    }
}
