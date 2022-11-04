using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SummaryRptAutomation.Helper
{
    public class FileLogger
    {
        readonly string _LogFolder = "Request_Log";
        public string LastErrorMessage { get; set; } = string.Empty;
        public void Log(string message)
        {
            try
            {
                var currentDirectory = AppDomain.CurrentDomain.BaseDirectory;
                var accessPath = Path.Combine(currentDirectory, _LogFolder);

                if (!Directory.Exists(accessPath)) Directory.CreateDirectory(accessPath);

                var fileName = $"{DateTime.Now:yyyyMMdd}.txt";
                var actualFilePath = Path.Combine(accessPath, fileName);

                message = $"\nOn {DateTime.Now:HH:mm:ss} \n{message}\n\n";

                using (StreamWriter streamWriter = new StreamWriter(actualFilePath, true))
                {
                    streamWriter.WriteLine(message);
                    streamWriter.Close();
                }

            }
            catch (Exception e)
            {
                LastErrorMessage = $"{e.Message}\n{e.StackTrace}";
            }
        }
    }
}
