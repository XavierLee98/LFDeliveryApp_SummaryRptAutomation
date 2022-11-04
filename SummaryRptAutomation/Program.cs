using SummaryRptAutomation.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Topshelf;

namespace SummaryRptAutomation
{
    class Program
    {
        public static FileLogger filelogger = new FileLogger();

        static void Main(string[] args)
        {
            var exitCode = HostFactory.Run(x =>
            {
                x.Service<Worker>(s =>
                {
                    s.ConstructUsing(worker => new Worker()); //Call this Worker
                    s.WhenStarted(worker => worker.Start());//Call worker start method
                    s.WhenStopped(worker => worker.Stop());//Call worker start method
                });

                //Service Setting 
                x.RunAsLocalSystem();
                x.SetServiceName("FTDeliveryApp_SummaryReportAutomationService");
                x.SetDisplayName("DeliveryApp Summary Report Automation Service");
                x.SetDescription("This service is help to auto generate report about all dispatch or payment summary.");
            });


            int exitCodeValue = (int)Convert.ChangeType(exitCode, exitCode.GetTypeCode());
            Environment.ExitCode = exitCodeValue;
        }
    }
}
