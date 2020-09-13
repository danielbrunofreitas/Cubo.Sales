using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Cubo.Dados.Infraestrutura
{
    public class Logger
    {
        public static async void Gerarlog(String lines)
        {
            await Task.Run(() =>
            {
                string folder = AppDomain.CurrentDomain.BaseDirectory;
                string logFolder = System.IO.Path.Combine(folder, "logs\\log.txt");
                System.IO.FileInfo fileinfo = new System.IO.FileInfo(logFolder);
                fileinfo.Directory.Create();
                System.IO.StreamWriter file = new System.IO.StreamWriter(logFolder, true);
                file.WriteLine(lines);
                file.Close();
            });
        }
    }
}
