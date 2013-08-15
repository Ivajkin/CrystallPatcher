using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CrystalPatcher
{
    public static class StringHelper
    {
        public static string GetFileParentDirectory(string file_path)
        {
            int slash_pos = file_path.Length;
            while (--slash_pos != 0 && file_path[slash_pos] != '\\') ;
            return file_path.Substring(0, slash_pos);
        }
        public static string NormalizePath(string directory_previous_path)
        {
            directory_previous_path = directory_previous_path.TrimEnd("\\/".ToCharArray()) + "\\";
            return directory_previous_path;
        }
    }
}
