using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using Adaptive.Service;
using Adaptive;
using System.Windows.Forms;

namespace Custom.Client.Nuintek.Fmea
{
    public class CustomServices
    {
        public string FMEAServices(string entityId, string userId)
        {
            FMEAServices fmeaService = new FMEAServices();
            //MessageBox.Show("FMEAServices 호출  entityId: " + entityId);
            URID urid = new URID(entityId);
            string masterType = urid.MasterType;

            try
            {
                //MessageBox.Show(entityId);
                List<string> objectIdList = new List<string>();
                string ECId = string.Empty;

                ECOData ecoData = null;
                bool isPart = false;

                if (masterType == "Part")
                {
                    objectIdList.Add(entityId);
                    isPart = true;
                }
                else  // EngineeringChange
                {
                    ECId = entityId;
                    // TODO : 나머지 설계변경값을 여기서 설정한다. (완)

                    string sql = string.Format("SELECT E$Number, E$Name, E$Reason, E$ApplyDate FROM engineeringchange_info WHERE urid = '{0}'", ECId);
                    DataTable ecDt = Services.ApplicationServices.DataSvc.ExecuteDataTable(sql);

                    ecoData = new ECOData();
                    ecoData.ECId = ECId;
                    ecoData.ECNumber = ecDt.Rows[0]["E$Number"].ToString();
                    ecoData.ECName = ecDt.Rows[0]["E$Name"].ToString();
                    ecoData.ECReason = ecDt.Rows[0]["E$Reason"].ToString();
                    ecoData.ECApplyDate = ecDt.Rows[0]["E$ApplyDate"].ToString();

                    DataTable linkedPart = Adaptive.Service.Services.ApplicationServices.RelationSvc.GetRelationList("Part", entityId, "AttachedPart");
                    for (int i = 0; i < linkedPart.Rows.Count; i++)
                    {
                        objectIdList.Add(linkedPart.Rows[i]["urid"].ToString());
                    }
                }

                int result = fmeaService.SendPartMasterToMdm(objectIdList, userId, isPart, entityId);

                if (result < 0)
                {
                    //string errMsg = string.Format("BOM 전송 에러발생. errorCode : {0}, 에러발생부품번호 : {1}, 에러메시지 : {2}", result, sapService.errPartNumber, sapService.errMsg);
                    //MessageBox.Show(errMsg);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("부품 마스터 전송 에러 발생 : " + ex.Message);
                MessageBox.Show("stack trace : " + ex.StackTrace);
                return string.Empty;
            }

            return string.Empty;
        }

        //public string LinkWithERPforECO(string entityId, string userId)
        //{
        //    DataTable dt = Adaptive.Service.Services.ApplicationServices.RelationSvc.GetRelationList("Part", entityId, "AttachedPart");
        //    if (dt.Rows.Count > 0)
        //    {
        //        BOMServices service = new BOMServices();
        //        foreach (DataRow dr in dt.Rows)
        //        {
        //            string urid = dr["urid"].ToString();

        //            service.SendItemToSap(entityId, userId);
        //        }
        //    }
        //    return string.Empty;
        //}

        // hschoi 2022.0811 BOM 개정 발생 시 IsERP를 0으로 만들어서 개정버전이 향후 ERP 전송이 될수 있도록 호출되어야 하는 외부함수 (AfterAction 에 등록 필요) 
        public string RevisionBOM(string entityId, string userId)
        {
            FMEAServices fmeaServices = new FMEAServices();
            MessageBox.Show("RevisionBOM Called");
            fmeaServices.UpdateClearFMEASendState(entityId);
            return string.Empty;
        }
    }
}
