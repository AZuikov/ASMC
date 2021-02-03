using System.IO;
using System.Linq;
using ASMC.Core.ViewModel;
using ASMC.Data.Model;
using DevExpress.Xpf.Core.Native;
using NLog;

namespace ASMC.Common.ViewModel
{
    public class ShemViewModel : SelectionViewModel
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private SchemeImage _shema;
        private string _pathImage;
        private string _text;

        /// <inheritdoc />
        protected override void OnEntityChanged()
        {
            if (Shema==null)
            {
                Shema = Entity as SchemeImage;
            }
        }

        /// <summary>
        /// ПОзволяет получать или задавать  отображенную схему.
        /// </summary>
        public SchemeImage Shema
        {
            get => _shema;
            set => SetProperty(ref _shema, value, nameof(Shema), ChangedCallback);
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
