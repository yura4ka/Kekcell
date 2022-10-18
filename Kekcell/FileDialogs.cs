using Microsoft.Win32;

namespace Kekcell
{
    static class FileDialogs
    {
        private static readonly string _fileName = "table.klsx";
        private static readonly string _defaultExt = ".klsx";
        private static readonly string _filter = "Kekcell document (.klsx)|*.klsx";

        public static string Open()
        {
            var dialog = new OpenFileDialog
            {
                FileName = _fileName,
                DefaultExt = _defaultExt,
                Filter = _filter
            };
            bool? result = dialog.ShowDialog();
            return result == true ? dialog.FileName : "";
        }

        public static string Save(string filePath = "table")
        {
            var dialog = new SaveFileDialog
            {
                FileName = filePath,
                DefaultExt = _defaultExt,
                Filter = _filter
            };
            bool? result = dialog.ShowDialog();
            return result == true ? dialog.FileName : "";
        }
    }
}
