using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.IO;
using System.Text.RegularExpressions;

namespace CrystalPatcher
{
    static class Program
    {
        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            string destination_dir = StringHelper.GetFileParentDirectory(Application.ExecutablePath);
            if (destination_dir != Directory.GetCurrentDirectory())
            {
                Directory.SetCurrentDirectory(destination_dir);
            }

            bool silent = false;
            if (args.Length == 0)
            {
                Messages.WriteToLog(null);

                // По умолчанию - без аргуметов.
                PatchPacker pp = new PatchPacker();
                try
                {
                    pp.ApplyPatch("./", "./PatchData.pack");
                }
                catch(Exception e)
                {
                    MessageBox.Show(e.Message);
                    Messages.ShowHelp();
                    return;
                }
                Messages.Show("Обновление успешно применено.\nДиректория './' обновлена с помощью патча './PatchData.pack'");
            }
            else
            {
                try
                {
                    string[] _args = args;
                    for (int i = 0; i < args.LongLength; ++i)
                    {
                        _args[i] += " ";
                    }
                    string arg = "";
                    arg = string.Concat(args);

                    // Проверка 'тихого' (без сообщений) режима.
                    {
                        Regex regex = new Regex("/silent");
                        if (regex.IsMatch(arg))
                        {
                            silent = true;
                        }
                    }
                    if (!silent)
                    {
                        Regex regex = new Regex("/help");
                        if (regex.IsMatch(arg))
                        {
                            Messages.ShowHelp();
                        }
                    }

                    // Применение патча.
                    {
                        Regex regex = new Regex(@"/patch path:\[(.+)\] update-pack:\[(.+)\]");
                        Match m = regex.Match(arg);
                        if (m.Success)
                        {
                            PatchPacker pp = new PatchPacker();
                            pp.ApplyPatch(m.Groups[1].ToString(), m.Groups[2].ToString());
                            if (!silent)
                            {
                                Messages.Show("Обновление успешно применено.\nДиректория '" + m.Groups[1].ToString() + "' обновлена с помощью патча '" + m.Groups[2].ToString() + "'");
                            }
                        }
                    }

                    // Создание патча.
                    {
                        Regex regex = new Regex(@"/create-update old:\[(.+)\] new:\[(.+)\] update-pack:\[(.+)\]");
                        Match m = regex.Match(arg);
                        if (m.Success)
                        {
                            PatchPacker pp = new PatchPacker();
                            pp.WritePack(m.Groups[1].ToString(), m.Groups[2].ToString(), m.Groups[3].ToString());
                            if (!silent)
                            {
                                Messages.Show("Создание обновления успешно завершено.\nОбновление на основе директорий:\n\tСтарая: '" + m.Groups[1].ToString() + "'\n\tНовая: '" + m.Groups[2].ToString() + "'. Обновление сохранено в директории: '" + m.Groups[3].ToString() + "'.");
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message);
                    return;
                }
                if (!silent)
                {
                    Application.EnableVisualStyles();
                    Application.SetCompatibleTextRenderingDefault(false);
                    Application.Run(new MainForm());
                }
            }

        }
    }
}
