using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace CrystalPatcher
{
    public static class Messages
    {
        const string help_string =
@"Параметры:

    /help
            - открытие этого окна
    /silent
            - не выводить лишних сообщений и информационных окон
    /patch path:[директория которую обновляем] update-pack:[директория с патчем]
            - применение патча (по умолчанию './' и './PatchData.pack')
    /create-update old:[директория содержащая предыдущую версию] new:[дериктория содежащая новую версию] update-pack:[директория для создания патча]
            - создание обновления из разности двух директорий";

        const string readme_string =
@"================================================================+
 CrystalPatcher
 Copyright 2010 © Ивайкин Тимофей (Ivajkin Timofej)
    версия (version): " + PatchInfo.patcher_version + @"
================================================================+
Утилита предназначена для создания и примения обновлений.
Работа основана на сравнении старых и новых данных и создании
информации для применения обновления.
В простейшем случае для обновления достаточно поместить CrystalPatcher.exe
и пакет с обновлением вида 'PatchData.pack' (может быть папкой)
в директорию которую вы хотите обновить.
Обязательно убедитесь в соответствии версий изначальных данных и пакета оновления.

Используйте /help для получения подробной справки.
";

        public static bool Ask(string question) {
            return (MessageBox.Show(question, null, MessageBoxButtons.YesNo) == DialogResult.Yes);
        }
        public static void Show(string message)
        {
            MessageBox.Show(message);
            WriteToLog(message);
        }
        public static void WriteToLog(string text)
        {
            System.Globalization.CultureInfo ci = new System.Globalization.CultureInfo("ru-RU");

            bool write_intro = !File.Exists(PatchPacker.LogFilePath);
            StreamWriter sw = new StreamWriter(PatchPacker.LogFilePath, true);
            if (write_intro)
            {
                sw.WriteLine(readme_string);
            }
            if (!(text == null || text == ""))
            {/*
                sw.WriteLine(
                    @"
                    ----------------------------------------------------------------+
                    Применяю обновление:
                        дата (date): " + DateTime.Now.ToString("G") + @"
                    ----------------------------------------------------------------+
                    ");
            }
            else
            {*/
                sw.WriteLine("--------" + DateTime.Now.ToString("G") + "-----------------------------------------+");
                sw.WriteLine(text);
            }
            sw.Close();
        }
        public static void ShowHelp()
        {
            // Помощь
            Messages.Show(help_string);
        }
    }
}
