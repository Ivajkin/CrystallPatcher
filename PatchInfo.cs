using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Policy;
using System.IO;

namespace CrystalPatcher
{
    struct PatchInfo
    {
        const string patchHeaderFile = "Patcher.info";
        public const string patcher_version = "beta: 0.1.0.6";

        public struct DataInfo
        {
            public string version;
            private string hash;
            public void Write(string file_path)
            {
                StreamWriter sw = new StreamWriter(file_path);
                sw.WriteLine(version);
                sw.WriteLine(hash);
                sw.Close();
            }

            internal void Read(string file_path)
            {
                StreamReader sr = new StreamReader(file_path);
                version = sr.ReadLine();
                hash = sr.ReadLine();
                sr.Close();
            }

            internal void setHash(string new_hash)
            {
                hash = new_hash;
            }
        }
        const string sourceInfoFile = "SourceData.info";
        public DataInfo sourceInfo;
        const string targetInfoFile = "TargetData.info";
        public DataInfo targetInfo;

        const string removeListFile = "RemoveFiles.list";
        public string[] files_to_remove;

        const string addListFile = "AddFiles.list";
        public string[] files_to_add;

        const string replaceListFile = "ReplaceFiles.list";
        public string[] files_to_replace;

        public void WriteInfo(string directory_path)
        {
            directory_path = directory_path.TrimEnd("\\/".ToCharArray()) + "\\InfoFiles\\";

            if (!Directory.Exists(directory_path))
            {
                Directory.CreateDirectory(directory_path);
            }

            StreamWriter sw = new StreamWriter(directory_path + patchHeaderFile);
            sw.WriteLine(patcher_version);
            sw.Close();

            _WriteFileList(directory_path + removeListFile, files_to_remove);
            _WriteFileList(directory_path + addListFile, files_to_add);
            _WriteFileList(directory_path + replaceListFile, files_to_replace);

            sourceInfo.Write(directory_path + sourceInfoFile);
            targetInfo.Write(directory_path + targetInfoFile);
        }
        private void _WriteFileList(string file_path, string[] file_list)
        {
            StreamWriter sw = new StreamWriter(file_path);
            foreach (string file in file_list)
            {
                sw.WriteLine(file);
            }
            sw.Close();
        }
        public void ReadInfo(string directory_path)
        {
            directory_path = directory_path.TrimEnd("\\/".ToCharArray()) + "\\InfoFiles\\";

            StreamReader sr = new StreamReader(directory_path + patchHeaderFile);
            string tmp_ver = sr.ReadLine();
            sr.Close();
            if (patcher_version != tmp_ver)
            {
                throw new Exception("Версия данных отличается от версии патчера. Версия патчера:\n" + patcher_version);
            }

            _ReadFileList(directory_path + removeListFile, out files_to_remove);
            _ReadFileList(directory_path + addListFile, out files_to_add);
            _ReadFileList(directory_path + replaceListFile, out files_to_replace);

            sourceInfo.Read(directory_path + sourceInfoFile);
            targetInfo.Read(directory_path + targetInfoFile);
        }

        private void _ReadFileList(string file_path, out string[] file_list)
        {
            List<string> tmp_file_list = new List<string>();

            StreamReader sr = new StreamReader(file_path);
            while (!sr.EndOfStream)
            {
                tmp_file_list.Add(sr.ReadLine());
            }
            sr.Close();

            file_list = tmp_file_list.ToArray();
        }
    }
}
