using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Windows.Forms;
using static System.Windows.Forms.AxHost;
using static System.Windows.Forms.Design.AxImporter;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Rebar;
using System.Xml.Linq;
using System.Drawing;
using System.IO;
using System.Security.Policy;
using Adaptive.Membership;
using System.IO.Ports;
using Adaptive.Service;
using Adaptive;

namespace Custom.Client.Nuintek.Fmea
{
    public class FMEAServices
    {
        public void SendUserInfo(string entityId, string IF_IUD)
        {
            try
            {
                string preSql = @"
                            INSERT INTO IF001_USER
                                        (IF_DATE,
                                            IF_FLAG,
                                            IF_IUD,
                                            ID,
                                            UserName,
                                            Password,
                                            DepartmentCode,
                                            Department,
                                            PositionCode,
                                            Position)
                            (SELECT '{0}' AS IF_DATE,
                                    'N'          AS IF_FLAG,
                                    '{1}'        AS IF_IUD,
                                    U$UserName,
                                    U$Name,
                                    U$Password,
                                    cp.C$code    AS DepartmentCode,
                                    U$Department,
                                    cpp.C$code   AS PositionCode,
                                    U$Position
                                FROM   user_properties up
                                    LEFT OUTER JOIN code_properties cp
                                            ON up.U$Department = cp.C$name
                                    LEFT OUTER JOIN code_properties cpp
                                            ON up.U$Position = cpp.C$name
                                WHERE  up.user_id = '{2}') 
                            ";
                string sql = string.Format(preSql, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), IF_IUD, entityId);
                Services.ApplicationServices.DataSvc.ExecuteNonQuery(sql);

            }
            catch (Exception ex)
            {
            }
        }
        
        public void SendPartInfo(string entityId, string IF_IUD)
        {
            try
            {
                string preSql = @"
                            INSERT INTO IF002_PART
                                        (IF_DATE,
                                         IF_FLAG,
                                         IF_IUD,
                                         Number,
                                         Name,
                                         ModelCode,
                                         Model,
                                         ClassCodeCode,
                                         ClassCode,
                                         Spec)
                            (SELECT '{0}' AS IF_DATE,
                                    'N'          AS IF_FLAG,
                                    '{1}'        AS IF_IUD,
                                    P$Number,
                                    P$Name,
                                    cp.C$Code    AS ModelCode,
                                    P$Model,
                                    cpp.C$Code   AS ClassCodeCode,
                                    P$Classcode,
                                    P$Spec
                             FROM   part_properties pp
                                    LEFT OUTER JOIN code_properties cp
                                                 ON pp.P$Model = cp.C$Name
                                    LEFT OUTER JOIN code_properties cpp
                                                 ON pp.P$Classcode = cpp.C$Name
                             WHERE  pp.part_id = '{2}') ";
                string sql = string.Format(preSql, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), IF_IUD, entityId);
                Services.ApplicationServices.DataSvc.ExecuteNonQuery(sql);
            }
            catch (Exception ex)
            {

            }
        }

        public void SendCodeInfo(string entityId, string IF_IUD)
        {
            try
            {
                string sql = string.Empty;
                string[] codeStrSplit = entityId.Split('-');
                string[] codeNumSplit = codeStrSplit[1].Split('.');
                string codeNumber = codeNumSplit[0];
                string preSql = @"
                        INSERT INTO {3}
                                    (IF_DATE,
                                     IF_FLAG,
                                     IF_IUD,
                                     Code,
                                     Name)
                        SELECT '{0}' AS IF_DATE,
                               'N'          AS IF_FLAG,
                               '{1}'        AS IF_IUD,
                               C$Code,
                               C$Name
                        FROM   code_properties
                        WHERE  code_id = '{2}' ";
                if (codeNumber == "1089")
                {
                    sql = string.Format(preSql, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), IF_IUD, entityId, "IF004_MODEL");
                    Services.ApplicationServices.DataSvc.ExecuteNonQuery(sql);
                }
                else if (codeNumber == "1093")
                {
                    sql = string.Format(preSql, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), IF_IUD, entityId, "IF006_CLIENT");
                    Services.ApplicationServices.DataSvc.ExecuteNonQuery(sql);
                }
            }
            catch (Exception ex)
            {
            }
        }

        public void SendProjectInfo(string entityId, string IF_IUD)
        {
            try
            {
                string preSql = @"
                            INSERT INTO IF009_PROJECT
                                            (IF_DATE,
                                            IF_FLAG,
                                            IF_IUD,
                                            Number,
                                            Name)
                            SELECT '{0}' AS IF_DATE,
                                    'N'          AS IF_FLAG,
                                    '{1}'        AS IF_IUD,
                                    PJ$Number,
                                    PJ$Name
                            FROM   project_properties
                            WHERE  project_id = '{2}'";
                string sql = string.Format(preSql, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), IF_IUD, entityId);
                Services.ApplicationServices.DataSvc.ExecuteNonQuery(sql);
            }
            catch (Exception)
            {
            }
        }

        public void SendBOMAndPartInfo(string entityId)
        {
            try
            {
                string objectType = URID.GetObjectType(entityId);

                NuintekUtils nu = new NuintekUtils();
                DataTable structureDt = nu.GetTopDownStructure(entityId);
                string date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                string sql = string.Format("SELECT P$Number, P$Name, cp.C$Code AS ModelCode, P$Model, cpp.C$Code AS ClassCodeCode, P$ClassCode, P$Spec, P$SendFMEAStatus FROM part_properties pp LEFT OUTER JOIN code_properties cp ON pp.P$Model = cp.C$Name LEFT OUTER JOIN code_properties cpp ON pp.P$ClassCode = cpp.C$Name WHERE pp.part_id = '{0}'", entityId);
                DataTable topDt = Services.ApplicationServices.DataSvc.ExecuteDataTable(sql);
                string parent_number = topDt.Rows[0]["P$Number"].ToString();
                Dictionary<string, string> dicParentNum = new Dictionary<string, string>();
                dicParentNum.Add(entityId, parent_number);

                string sendFMEAStatus = topDt.Rows[0]["P$SendFMEAStatus"].ToString();
                string parent_id = string.Empty;
                string P_Number = string.Empty;
                string P_Name = string.Empty;
                string P_ModelCode = string.Empty;
                string P_Model = string.Empty;
                string P_ClassCodeCode = string.Empty;
                string P_ClassCode = string.Empty;
                string P_Spec = string.Empty;
                string IF_IUD = string.Empty;
                if (sendFMEAStatus != "S")
                {
                    IF_IUD = GetIUD(sendFMEAStatus);
                    P_Number = topDt.Rows[0]["P$Number"].ToString();
                    P_Name = topDt.Rows[0]["P$Name"].ToString();
                    P_ModelCode = topDt.Rows[0]["ModelCode"].ToString();
                    P_Model = topDt.Rows[0]["P$Model"].ToString();
                    P_ClassCodeCode = topDt.Rows[0]["ClassCodeCode"].ToString();
                    P_ClassCode = topDt.Rows[0]["P$ClassCode"].ToString();
                    P_Spec = topDt.Rows[0]["P$Spec"].ToString();

                    if (objectType == "ASSY")
                    {
                        // insert top part
                        sql = string.Format("INSERT INTO IF002_PART (IF_DATE, IF_FLAG, IF_IUD, Number, Name, ModelCode, Model, ClassCodeCode, ClassCode, Spec) VALUES ('{0}', 'N', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}', '{8}')", date, IF_IUD, P_Number, P_Name, P_ModelCode, P_Model, P_ClassCodeCode, P_ClassCode, P_Spec);

                        Services.ApplicationServices.DataSvc.ExecuteNonQuery(sql);
                        nu.UpdateSendFMEAStatus("S", entityId);
                    }
                }
                Dictionary<string, string> dicParentIdSendStatus = new Dictionary<string, string>();
                dicParentIdSendStatus.Add(entityId, sendFMEAStatus);
                for (int i = 0; i < structureDt.Rows.Count; i++)
                {
                    sendFMEAStatus = structureDt.Rows[i]["P$SendFMEAStatus"].ToString();
                    string child_id = structureDt.Rows[i]["child_id"].ToString();
                    objectType = URID.GetObjectType(child_id);
                    if (!dicParentIdSendStatus.ContainsKey(child_id)) 
                        dicParentIdSendStatus.Add(child_id, sendFMEAStatus);
                    parent_id = structureDt.Rows[i]["parent_id"].ToString();
                    HashSet<string> parentChildId = new HashSet<string>();
                    string p_c_id = parent_id + child_id;
                    //sql = string.Format("SELECT P$SendFMEAStatus FROM part_properties WHERE part_id = '{0}'", child_id);
                    //sendFMEAStatus = Services.ApplicationServices.DataSvc.ExecuteScalar(sql); //structureDt.Rows[i]["P$SendFMEAStatus"].ToString();
                    string parentSendFMEAStatus = "";
                    dicParentIdSendStatus.TryGetValue(parent_id, out parentSendFMEAStatus);
                    if (parentSendFMEAStatus != "S" && !parentChildId.Contains(p_c_id))
                    {
                        parentChildId.Add(p_c_id);
                        P_Number = structureDt.Rows[i]["p$number"].ToString(); // child part number
                        P_Name = structureDt.Rows[i]["p$name"].ToString();
                        P_ModelCode = structureDt.Rows[i]["ModelCode"].ToString();
                        P_Model = structureDt.Rows[i]["P$Model"].ToString();
                        P_ClassCodeCode = structureDt.Rows[i]["ClassCodeCode"].ToString();
                        P_ClassCode = structureDt.Rows[i]["P$ClassCode"].ToString();
                        P_Spec = structureDt.Rows[i]["P$Spec"].ToString();
                        

                        if (!dicParentNum.ContainsKey(parent_id))
                        {
                            sql = string.Format("SELECT P$Number FROM part_properties WHERE part_id = '{0}'", parent_id);
                            DataTable dt = Services.ApplicationServices.DataSvc.ExecuteDataTable(sql);
                            dicParentNum.Add(parent_id, dt.Rows[0]["P$Number"].ToString());
                        }
                        parent_number = dicParentNum[parent_id];

                        if (sendFMEAStatus != "S" && objectType == "ASSY")
                        {
                            // insert part
                            IF_IUD = GetIUD(sendFMEAStatus);
                            sql = string.Format("INSERT INTO IF002_PART (IF_DATE, IF_FLAG, IF_IUD, Number, Name, ModelCode, Model, ClassCodeCode, ClassCode, Spec) VALUES ('{0}', 'N', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}', '{8}')", date, IF_IUD, P_Number, P_Name, P_ModelCode, P_Model, P_ClassCodeCode, P_ClassCode, P_Spec);
                            Services.ApplicationServices.DataSvc.ExecuteScalar(sql);
                            nu.UpdateSendFMEAStatus("S", child_id);
                        }

                        string sequence = structureDt.Rows[i]["sequence"].ToString();
                        string quantity = structureDt.Rows[i]["quantity"].ToString();
                        IF_IUD = GetIUD(parentSendFMEAStatus);
                        // insert bom
                        sql = String.Format("INSERT INTO IF008_BOM (IF_DATE, IF_FLAG, IF_IUD, PNumber, CNumber, sequence, quantity) VALUES ('{0}', 'N', '{1}', '{2}', '{3}', '{4}', '{5}')", date, IF_IUD, parent_number, P_Number, sequence, quantity);
                        Services.ApplicationServices.DataSvc.ExecuteScalar(sql);

                    }
                }
            }
            catch (Exception ex)
            {
            }
        }

        public string GetIUD(string sendFMEAStatus)
        {
            if (string.IsNullOrEmpty(sendFMEAStatus))
            {
                return "I";
            }
            else if (sendFMEAStatus == "U")
            {
                return "U";
            }
            else
            {
                return "E";
            }
        }
    }
}
