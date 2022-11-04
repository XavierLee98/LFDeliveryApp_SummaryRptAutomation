using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;
using SummaryRptAutomation.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SummaryRptAutomation.Helper
{
    public class CrystalReport
    {
        ReportDocument rptdoc;

        public CrystalReport(string rptfile, CrConfiguration _crconfig, string MidwareDB)
        {
            rptdoc = new ReportDocument();
            rptdoc.Load(rptfile);

            ConnectionInfo cntinfo = new ConnectionInfo();
            TableLogOnInfos tableloginfos = new TableLogOnInfos();
            TableLogOnInfo tableloginfo = new TableLogOnInfo();

            cntinfo.ServerName = _crconfig.Server;
            cntinfo.DatabaseName = MidwareDB;
            cntinfo.UserID = _crconfig.DbUserName;
            cntinfo.Password = _crconfig.DbPassword;

            foreach (Table CrTable in rptdoc.Database.Tables)
            {
                tableloginfo = CrTable.LogOnInfo;
                tableloginfo.ConnectionInfo = cntinfo;
                CrTable.ApplyLogOnInfo(tableloginfo);
            }

            cntinfo = null;
            tableloginfos = null;
            tableloginfo = null;
        }

        ~CrystalReport()
        {
            Close();
            rptdoc = null;
        }

        public void SetParameterValue(string name, object value)
        {
            if (!rptdoc.ParameterFields.OfType<ParameterField>().Where(pp => pp.Name == name).Any()) return;

            rptdoc.SetParameterValue(name, value);
        }

        public void SaveAsPdf(string path)
        {
            rptdoc.ExportToDisk(ExportFormatType.PortableDocFormat, path);
        }

        public void Close()
        {
            rptdoc.Close();
            rptdoc.Dispose();
        }

    }
}
