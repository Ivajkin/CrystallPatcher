using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace CrystalPatcher
{
    class PatchPacker
    {
        public const string LogFilePath = "Patcher.log";

        const string AddFilesPath = "Add\\";
        const string ReplaceFilesPath = "Replace\\";

        /// <summary>
        /// Создаёт патч.
        /// </summary>
        /// <param name="directory_previous_version"></param>
        /// <param name="directory_next_version"></param>
        /// <param name="patch_pack_directory"></param>
        public void WritePack(string directory_previous_version, string directory_next_version, string patch_pack_directory)
        {
            directory_previous_version = StringHelper.NormalizePath(directory_previous_version);
            directory_next_version = StringHelper.NormalizePath(directory_next_version);
            patch_pack_directory = StringHelper.NormalizePath(patch_pack_directory);

            if (Directory.Exists(patch_pack_directory))
            {
                if (Messages.Ask("Директория уже существует: '" + patch_pack_directory + "'. Заменить?"))
                {
                    Microsoft.VisualBasic.FileIO.FileSystem.DeleteDirectory(patch_pack_directory, Microsoft.VisualBasic.FileIO.UIOption.OnlyErrorDialogs, Microsoft.VisualBasic.FileIO.RecycleOption.SendToRecycleBin);
                    Messages.Show("Директория '" + patch_pack_directory + "' помещена в корзину");
                }
            }

            PatchInfo patch_data = Check(directory_previous_version, directory_next_version);
            patch_data.WriteInfo(patch_pack_directory);
            // Новые.
            foreach (string file in patch_data.files_to_add)
            {
                string dir = patch_pack_directory + AddFilesPath + StringHelper.GetFileParentDirectory(file);
                if (!Directory.Exists(dir) && dir != "")
                {
                    Directory.CreateDirectory(dir);
                }

                File.Copy(directory_next_version + file, patch_pack_directory + AddFilesPath + file, false);
            }
            // Замена старых.
            foreach (string file in patch_data.files_to_replace)
            {
                string dir = patch_pack_directory + ReplaceFilesPath + StringHelper.GetFileParentDirectory(file);
                if (!Directory.Exists(dir) && dir != "")
                {
                    Directory.CreateDirectory(dir);
                }
                File.Copy(directory_next_version + file, patch_pack_directory + ReplaceFilesPath + file, true);
            }
        }
        /// <summary>
        /// Проверка файлов, создание заголовка для патча.
        /// </summary>
        /// <param name="directory_previous_version"></param>
        /// <param name="directory_next_version"></param>
        /// <returns></returns>
        private PatchInfo Check(string directory_previous_version, string directory_next_version)
        {
            string[] files_prev = Directory.GetFiles(directory_previous_version, "*.*", SearchOption.AllDirectories);
            string[] files_next = Directory.GetFiles(directory_next_version, "*.*", SearchOption.AllDirectories);
            {
                var q = from o in files_prev select o.Substring(directory_previous_version.Length);
                files_prev = q.ToArray();
            }
            {
                var q = from o in files_next select o.Substring(directory_next_version.Length);
                files_next = q.ToArray();
            }

            // Файлы - на удаление, на замену и на добавление.
            // Удаление - есть в старой, нет в новой.
            string[] on_kill;
            {
                var q = from o in files_prev where !files_next.Contains(o) select o;
                on_kill = q.ToArray();
            }
            // Замена - есть в обеих версиях, но разные.
            string[] on_replace;
            {
                var q = from o in files_next where files_prev.Contains(o) && FilesAreDifferent(directory_previous_version + o, directory_next_version + o) select o;
                on_replace = q.ToArray();
            }
            // Добаление - нет в старой, но есть в новой.
            string[] on_add;
            {
                var q = from o in files_next where !files_prev.Contains(o) select o;
                on_add = q.ToArray();
            }


            // Создаём объект для записи.
            PatchInfo patch_data = new PatchInfo();
            patch_data.files_to_add = on_add;
            patch_data.files_to_remove = on_kill;
            patch_data.files_to_replace = on_replace;
            patch_data.sourceInfo.version = "";// directory_previous_version;
            patch_data.sourceInfo.setHash("");
            patch_data.targetInfo.version = "";// directory_next_version;
            patch_data.targetInfo.setHash("");
            return patch_data;
        }
        /// <summary>
        /// Проверка, содержат ли файлы разные данные.
        /// </summary>
        /// <param name="fileA"></param>
        /// <param name="fileB"></param>
        /// <returns></returns>
        private bool FilesAreDifferent(string fileA, string fileB)
        {
            FileStream fsA = new FileStream(fileA, FileMode.Open);
            FileStream fsB = new FileStream(fileB, FileMode.Open);
            if (fsA.Length != fsB.Length)
            {
                return true;
            }
            fsA.Close();
            fsB.Close();
            byte[] bytesA = File.ReadAllBytes(fileA);
            byte[] bytesB = File.ReadAllBytes(fileB);
            for (int i = 0; i < bytesB.LongLength; ++i)
            {
                if (bytesA[i] != bytesB[i])
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Применение готового патча к старой версии.
        /// </summary>
        /// <param name="directory_previous_path">Директория к которой применяем патч.</param>
        /// <param name="prev_version">Версия данных к которым применяем патч.</param>
        /// <param name="patch_pack_directory">Директория с данными для патча.</param>
        public void ApplyPatch(string directory_previous_path, string patch_pack_directory)
        {
            directory_previous_path = StringHelper.NormalizePath(directory_previous_path);
            patch_pack_directory = StringHelper.NormalizePath(patch_pack_directory);

            PatchInfo patch_data = new PatchInfo();
            bool canApply = false;
            if (Directory.Exists(patch_pack_directory))
            {
                try
                {
                    patch_data.ReadInfo(patch_pack_directory);
                    canApply = CheckToUpdate(directory_previous_path, patch_data);
                }
                catch (Exception e)
                {
                    canApply = false;
                    throw e;
                }
            }
            if (!canApply)
                throw new Exception("Директория не потходит для обновления либо данные для обновления повреждены.");

            // Новые.
            foreach (string file in patch_data.files_to_add)
            {
                string dir_dest = directory_previous_path + StringHelper.GetFileParentDirectory(file);
                if (!Directory.Exists(dir_dest) && dir_dest != "")
                {
                    Directory.CreateDirectory(dir_dest);
                }

                File.Copy(patch_pack_directory + AddFilesPath + file, directory_previous_path + file, false);
            }
            // Замена старых.
            foreach (string file in patch_data.files_to_replace)
            {
                File.Copy(patch_pack_directory + ReplaceFilesPath + file, directory_previous_path + file, true);
            }
            // Уничтожение лишних.
            foreach (string file in patch_data.files_to_remove)
            {
                File.Delete(directory_previous_path + file);
            }
        }
        /// <summary>
        /// Проверка перед обновлением (можно ли обновлять).
        /// </summary>
        /// <param name="directory_previous_path">Директория к которой применяется патч.</param>
        /// <param name="patch_data">Информация для патчинга.</param>
        /// <param name="prev_version">Версия данных к которым применяется патч. Должна соответствовать версии исходных материалов при создании патча.</param>
        /// <returns>Пройдена ли проверка (можно применять патч).</returns>
        private bool CheckToUpdate(string directory_previous_path, PatchInfo patch_data)
        {
            directory_previous_path = StringHelper.NormalizePath(directory_previous_path);

            for (int i = 0; i < patch_data.files_to_remove.LongLength; ++i)
            {
                if (!File.Exists(directory_previous_path + patch_data.files_to_remove[i]))
                {
                    return false;
                }
            }
            for (int i = 0; i < patch_data.files_to_replace.LongLength; ++i)
            {
                if (!File.Exists(directory_previous_path + patch_data.files_to_replace[i]))
                {
                    return false;
                }
            }
            if (patch_data.sourceInfo.version != "")
            {
                return false;
            }

            return true;
        }
    }
}
