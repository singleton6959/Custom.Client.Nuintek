using Adaptive;
using Adaptive.Service;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Custom.Client.Nuintek.Fmea
{
    public class NuintekUtils
    {
        public virtual void WriteLog(string msg)
        {
            MessageBox.Show(msg);
        }

        public string GetPreviousID(string objectId)
        {
            //Adaptive.Data.DbService dbSvc = new Adaptive.Data.DbService();
            string sql;

            // hschoi : P$IsSAP 부활 (structure의 S$IsSAP 은 SAP 전송 확인 용도로 쓰고, 이전버전 체크는 P$IsSAP을 활용하도록 한다)
            //sql = string.Format("SELECT p.urid, p.version_sequence, p.released FROM Part_info p inner join structure_info s on p.urid = s.parent_id WHERE s.S$IsSAP = 1 and p.urid != '{0}' and p.version_id = (SELECT version_id FROM Part_entity WHERE urid='{0}') AND p.deleted=0 ORDER BY p.version_sequence DESC", objectId);
            sql = string.Format("SELECT p.urid, p.version_sequence, p.released FROM Part_info p WHERE p.P$IsSAP = 1 and p.urid != '{0}' and p.version_id = (SELECT version_id FROM Part_entity WHERE urid='{0}') AND p.deleted=0 ORDER BY p.version_sequence DESC", objectId);

            string prevId = string.Empty;
            string currId = string.Empty;

            try
            {
                DataTable dt = Services.ApplicationServices.DataSvc.ExecuteDataTable(sql);

                bool findCurruntId = false;

                if (dt.Rows.Count > 0)
                {
                    prevId = dt.Rows[0]["urid"]?.ToString();
                }
                return prevId;
            }
            catch (Exception ex)
            {
                return prevId;
            }

        }

        public string SetSAPStatusValue(int result, bool isPart, string entityId, bool isSendFMEA)
        {
            string sql = string.Empty;
            string sendTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            string statusText = string.Empty;
            string statusCode = string.Empty;
            if (result >= 0)
            {
                if (isSendFMEA)
                {
                    statusText = "전송 대기중";
                    statusCode = "P";
                }
                else
                {
                    statusText = "전송 완료";
                    statusCode = "S";
                }
            }
            else
            {
                if (isSendFMEA)
                {
                    statusText = "FMEA 전송 실패";
                    statusCode = "M";
                }
                else
                {
                    statusText = "전송 실패";
                    statusCode = "P"; //"F";
                }
                
            }

            if (isPart)
            {
                if (isSendFMEA)
                {
                    sql = string.Format("UPDATE part_properties SET P$SAPStatusText='{0}', P$SAPStatusCode='{1}', P$FMEASendDate='{2}' WHERE part_id='{3}'", statusText, statusCode, sendTime, entityId); // engineeringchange_properties
                }
                else
                {
                    sql = string.Format("UPDATE part_properties SET P$SAPStatusText='{0}', P$SAPStatusCode='{1}' WHERE part_id='{2}'", statusText, statusCode, entityId); // engineeringchange_properties
                }
            }
            else
            {
                if (isSendFMEA)
                {
                    sql = string.Format("UPDATE engineeringchange_properties SET E$SAPStatusText='{0}', E$SAPStatusCode='{1}', E$FMEASendDate='{2}' WHERE engineeringchange_id='{3}'", statusText, statusCode, sendTime, entityId);
                }
                else
                {
                    sql = string.Format("UPDATE engineeringchange_properties SET E$SAPStatusText='{0}', E$SAPStatusCode='{1}' WHERE engineeringchange_id='{2}'", statusText, statusCode, entityId);
                }
            }
            try
            {
                Services.ApplicationServices.DataSvc.ExecuteScalar(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return statusText;
        }

        public DataTable GetSAPStatusValue(bool isPart, string entityId)
        {
            string sql = string.Empty;
            if (isPart)
            {
                sql = string.Format("SELECT P$SAPStatusText, P$SAPStatusCode from part_properties WHERE part_id='{0}'", entityId);
            }
            else
            {
                sql = string.Format("SELECT E$SAPStatusText, E$SAPStatusCode from engineeringchange_properties WHERE engineeringchange_id='{0}'", entityId);
            }
            try
            {
                return Services.ApplicationServices.DataSvc.ExecuteDataTable(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

    }
}
