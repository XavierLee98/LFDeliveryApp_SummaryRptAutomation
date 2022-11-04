using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SummaryRptAutomation.Model
{
    public class RptLayoutPath
    {
        public int Id { get; set; }

        public string FileId { get; set; }

        public string LayoutPath { get; set; }

        public string LayoutName { get; set; }

        public string DestinationPath { get; set; }

    }
}
