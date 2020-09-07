using ASMC.Common.ViewModel;
using ASMC.Data.Model;
using DevExpress.Xpf.Core.Native;
using System.IO;
using System.Linq;
using NLog;

namespace ASMC.Core.ViewModel
{
    public class ShemViewModel : FromBaseViewModel
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
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
            Shema = Entity as ShemeImage;
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


            var path = $@"{Directory.GetCurrentDirectory()}\Plugins\{Shema.AssemblyLocalName}";
            Logger.Debug($"Ищем путь к картинке {path}");
            if (!Directory.Exists(path))
                return;
            PathImage = Directory.GetFiles(path, Shema.FileName, SearchOption.AllDirectories).FirstOrDefault();
            Logger.Debug($"Найдена картинка по расположению: {PathImage}");
            if (Shema.FileNameDescription == null) return;
            var docPath = Directory.GetFiles(path, Shema.FileNameDescription, SearchOption.AllDirectories).FirstOrDefault();
            if (docPath == null) return;
            using (var fs = File.Open(docPath, FileMode.Open))
            {
                Text = fs.ReadString();
            }
        }
    }
}
