using Dapper;
using SummaryRptAutomation.Model;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SummaryRptAutomation.Helper
{
    public class RequestHelper
    {
        public static string LasteErrorMessage { get; set; } = string.Empty;
        public static string DbConnectString_Midware { get; set; } = ConfigurationManager.ConnectionStrings["MWconnectionString"].ConnectionString;
        public static bool IsBusy { get; set; } = false;

        const string _dispatch = "Dispatch";
        const string _payment = "Payment";
               
        public static void ExecuteRequest()
        {
            try
            {
                if (IsBusy) return;
                IsBusy = true;

                var sql = @"SELECT * FROM SummaryRequest 
                            WHERE IsPost = 0 and isTry < 3";

                var conn = new SqlConnection(DbConnectString_Midware);
                var requests = conn.Query<RequestModel>(sql).ToList();

                if (requests == null)
                {
                    IsBusy = false;
                    return;
                }
                if (requests.Count == 0)
                {
                    IsBusy = false;
                    return;
                }

                HandleRequest(requests);
                IsBusy = false;
            }
            catch (Exception e)
            {
                Log($"{e.Message}\n{ e.StackTrace}");
                IsBusy = false;
            }
        }

        static void HandleRequest(List<RequestModel> requests)
        {
            try
            {
                if (requests == null) return;
                if (requests.Count == 0) return;
                requests.ForEach(x =>
                {

                    switch (x.DocType)
                    {
                        case _dispatch:
                            {
                                StartGenerateInvoiceSummaryReport(x);
                                break;
                            }
                        case _payment:
                            {
                                StartGeneratePaymentSummaryReport(x);
                                break;
                            }
                        default:
                            {
                                Log($"Request {x.Guid} DocType {x.DocType} is not found.");
                                UpdateFailed(x.Guid);
                                break;
                            }
                    }
                });
            }
            catch (Exception e)
            {
                Log($"{e.Message}\n{ e.StackTrace}");
            }
        }

        static void StartGeneratePaymentSummaryReport(RequestModel request)
        {
            try
            {
                var filepath = GetReportFilePath(request.DocType);
                var crConf = GetConnection(request);

                string pathcombined = Path.Combine(filepath.LayoutPath, filepath.LayoutName);

                string reportName = $"PaymentSummary_{request.Guid}.pdf";
                string destinationpath = Path.Combine(filepath.DestinationPath, reportName);
                var conn = new SqlConnectionStringBuilder(DbConnectString_Midware);

                var report = new CrystalReport(pathcombined, crConf, conn.InitialCatalog);

                report.SetParameterValue("CompanyID@", request.CompanyID);
                report.SetParameterValue("StartDate@", request.StartDate);
                report.SetParameterValue("EndDate@", request.EndDate);

                report.SaveAsPdf(destinationpath);
                report.Close();

                UpdateSuccess(request.Guid, reportName);
                Log($"Payment Summary Report  {request.Guid} success to generate.\n");
            }
            catch (Exception e)
            {
                Log($"Payment Summary Report {request.Guid} fail.\n{e.Message}\n{ e.StackTrace}");
                UpdateFailed(request.Guid);
            }
        }

        static void StartGenerateInvoiceSummaryReport(RequestModel request)
        {
            try
            {
                var filepath = GetReportFilePath(request.DocType);
                var crConf = GetConnection(request);

                string pathcombined = Path.Combine(filepath.LayoutPath, filepath.LayoutName);

                string reportName = $"InvoiceSummary_{request.Guid}.pdf";
                string destinationpath = Path.Combine(filepath.DestinationPath, reportName);

                var conn = new SqlConnectionStringBuilder(DbConnectString_Midware);

                var report = new CrystalReport(pathcombined, crConf, conn.InitialCatalog);
                report.SetParameterValue("CompanyID@", request.CompanyID);
                report.SetParameterValue("Status@", request.Status);
                report.SetParameterValue("StartDate@", request.StartDate);
                report.SetParameterValue("EndDate@", request.EndDate);

                report.SaveAsPdf(destinationpath);
                report.Close();

                UpdateSuccess(request.Guid, reportName);
                Log($"Invoice Summary Report {request.Guid} success to generate.\n");
            }
            catch (Exception e)
            {
                Log($"Invoice Summary Report {request.Guid} fail.\n{e.Message}\n{ e.StackTrace}");
                UpdateFailed(request.Guid);
            }
        }

        static RptLayoutPath GetReportFilePath(string docType)
        {
            try
            {
                var fileId = "";
                switch (docType)
                {
                    case _dispatch:
                        {
                            fileId = "Summary_Dispatch";
                            break;
                        }
                    case _payment:
                        {
                            fileId = "Summary_Payment";
                            break;
                        }
                    default:
                        {
                            Log("Doc type not found when get file path.");
                            break;
                        }
                }
                var conn = new SqlConnection(DbConnectString_Midware);

                var query = "SELECT * FROM RptLayoutPath WHERE FileId = @FileId";

                var crCon = conn.Query<RptLayoutPath>(query, new { FileId = fileId }).FirstOrDefault();

                if (crCon == null) throw new Exception($"Failed to get file path.");

                return crCon;
            }
            catch (Exception e)
            {
                Log($"{e.Message}\n{ e.StackTrace}");
                return null;
            }
        }

        static CrConfiguration GetConnection(RequestModel request)
        {
            try
            {
                var conn = new SqlConnection(DbConnectString_Midware);

                var query = "SELECT CompanyID, Server, DbUserName, DbPassword, CompanyDB FROM DBCommon WHERE CompanyID = @CompanyId";

                var crCon = conn.Query<CrConfiguration>(query, new { CompanyId = request.CompanyID }).FirstOrDefault();

                if (crCon == null) throw new Exception($"Failed to get connection string. [{request.Id}]");

                return crCon;
            }
            catch (Exception e)
            {
                Log($"{e.Message}\n{ e.StackTrace}");
                return null;
            }
        }

        static void UpdateSuccess(Guid docGuid, string destination)
        {
            try
            {
                var conn = new SqlConnection(DbConnectString_Midware);

                var query = "  UPDATE SummaryRequest SET IsPost = 1, Path = @DestinationPath, CreatedDate = GETDATE() WHERE Guid = @Guid";

                conn.Execute(query, new { Guid = docGuid, DestinationPath = destination });

            }
            catch (Exception e)
            {
                Log($"{e.Message}\n{ e.StackTrace}");
            }
        }

        static void UpdateFailed(Guid docGuid)
        {
            try
            {
                var conn = new SqlConnection(DbConnectString_Midware);

                var query = "UPDATE SummaryRequest SET IsTry= IsTry + 1 WHERE Guid = @Guid";

                conn.Execute(query, new { Guid = docGuid });
            }
            catch (Exception e)
            {
                Log($"{e.Message}\n{ e.StackTrace}");
            }
        }

        static void Log(string message)
        {
            Program.filelogger.Log(message.ToString());
        }
    }
}
