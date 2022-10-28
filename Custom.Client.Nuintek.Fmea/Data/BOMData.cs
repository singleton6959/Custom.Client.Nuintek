using Adaptive.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Custom.Client.Nuintek.Fmea
{

    public class ECOData
    {
        public string ECId = string.Empty;
        public string ECNumber = string.Empty;
        public string ECName = string.Empty;
        public string ECReason = string.Empty;
        public string ECApplyDate = string.Empty;
    }
    public class BOMCreateData
    {
        //private int serial = 0;
        public string IF_ID { get; set; }
        public string p_P_Number { get; set; }
        public string c_S_BDDIS { get; set; }
        public string c_S_BDEFF { get; set; }
        public string p_P_MEINS { get; set; }
        public int sequence { get; set; }
        public string c_P_Number { get; set; }
        public float c_quantity { get; set; }
        public string c_P_MEINS { get; set; }
        public string c_S_Material { get; set; }
        public bool isSAP { get; set; }
        public string structure_id { get; set; }
        public string S_SORTF { get; set; }
        public string S_POTX1 { get; set; }

        public override string ToString()
        {
            return "\r\nIF_ID: " + IF_ID + "\r\nIF_IUD: I\r\nMATNR: " + p_P_Number + "\r\nSTATL: " + c_S_BDDIS + "\r\nDATUV: " + c_S_BDEFF + "\r\nBASMN: 1\r\nBASME: " + p_P_MEINS + "\r\nPOSNR:" + sequence + "\r\nIDNRK: " + c_P_Number + "\r\nMEINGE: " + c_quantity + "\r\nMEINS: " + c_P_MEINS + "\r\nBEIKZ: " + c_S_Material + S_SORTF + "\r\nPOTX1:" + S_POTX1 + "\r\n";
        }
        //public BOMCreateData(string parentId, string childId)
        //{
        //    string sql = "SELECT IF_ID FROM IFID_REGISTRY WHERE parent_id='" + parentId + "' AND child_id='" + childId + "'";
        //    IF_ID = Services.ApplicationServices.DataSvc.ExecuteScalar(sql);
        //    if (string.IsNullOrEmpty(IF_ID))
        //    {
        //        sql = "SELECT NEXT VALUE FOR IF_ID_SERIAL";
        //        serial = Convert.ToInt32(Services.ApplicationServices.DataSvc.ExecuteScalar(sql));
        //        string date = DateTime.Now.ToString("yyMMdd");
        //        IF_ID = string.Format("ZBOM_{0}_{1}", date, serial.ToString("D6"));
        //        sql = "INSERT INTO IFID_REGISTRY (parent_id, child_id, IF_ID) VALUES ('" + parentId + "', '" + childId + "', '" + IF_ID + "')";
        //        Services.ApplicationServices.DataSvc.ExecuteNonQuery(sql);
        //    }
        //}
    }

    public class BOMUpdateData
    {
        //private int serial = 0;
        public string IF_ID { get; set; }
        public string p_P_Number { get; set; }
        public string c_P_Number { get; set; }
        public string E_Number { get; set; }
        public string E_Name { get; set; }
        public string E_Reason { get; set; }
        public string E_ApplyDate { get; set; }
        public int sequence { get; set; }
        public float quantity { get; set; }
        public string c_P_MEINS { get; set; }
        public char ECGBN { get; set; }   // (I: 생성, U: 변경, D: 삭제)
        public bool isSAP { get; set; }
        public string structure_id { get; set; }
        public string S_SORTF { get; set; }
        public string S_POTX1 { get; set; }
        public string S_AENST { get; set; }
        public override string ToString()
        {
            return "IF_ID: " + IF_ID + "\r\nIF_IUD: U\r\nMATNR: " + p_P_Number + "\r\nIDNRK_A: " + c_P_Number + "\r\nAENNR: " + E_Number + "\r\nAETXT1: " + E_Name + "\r\nAETXT2: " + E_Reason + "\r\nDATUV: " + E_ApplyDate + "\r\nPOSNR_A:" + sequence + "\r\nMENGE_A:" + quantity + "\r\nMEINS_A: " + c_P_MEINS + "\r\nECGBN:" + ECGBN + "\r\nSORTF:" + S_SORTF + "\r\nPOTX1:" + S_POTX1 + "\r\nAENST:" + S_AENST;
        }
        //public BOMUpdateData(string parentId, string childId)
        //{
        //    string sql = "SELECT IF_ID FROM IFID_REGISTRY WHERE parent_id = '" + parentId + "' AND child_id='" + childId + "'";
        //    IF_ID = Services.ApplicationServices.DataSvc.ExecuteScalar(sql);
        //    if (string.IsNullOrEmpty(IF_ID))
        //    {
        //        sql = "SELECT NEXT VALUE FOR IF_ID_SERIAL";
        //        serial = Convert.ToInt32(Services.ApplicationServices.DataSvc.ExecuteScalar(sql));
        //        string date = DateTime.Now.ToString("yyMMdd");
        //        IF_ID = string.Format("ZBOM_{0}_{1}", date, serial.ToString("D6"));
        //        sql = "INSERT INTO IFID_REGISTRY (parent_id, child_id, IF_ID) VALUES ('" + parentId + "', '" + childId + "', '" + IF_ID + "')";
        //        Services.ApplicationServices.DataSvc.ExecuteNonQuery(sql);
        //    }
        //}
    }

    //Grid용 class
    //public class BOMData
    //{
    //    public int? RowNumber { get; set; }
    //    public string Tree_id { get; set; }
    //    public int? Tree_level { get; set; }
    //    public string parent_id { get; set; }
    //    public string child_id { get; set; }
    //    public string PNumber { get; set; }
    //    public string PVersion { get; set; }
    //    public string flag { get; set; }
    //    public double? quantity { get; set; }
    //    public int? sequence { get; set; }

    //    public int GridColsCount = 10;

    //    public object[] ToArrayForGrid()
    //    {
    //        var array = new object[this.GridColsCount];
    //        //array[0]은 X(grid용)
    //        array[0] = this.RowNumber;
    //        array[1] = this.Tree_id;
    //        array[2] = this.Tree_level;
    //        array[3] = this.sequence;
    //        array[4] = this.parent_id;
    //        array[5] = this.child_id;
    //        array[6] = this.PNumber;
    //        array[7] = this.PVersion;
    //        array[8] = this.quantity;
    //        array[9] = this.flag;
            
            
    //        return array;
    //    }
    //}
    public class ESReply
    {
        public string REP_FLAG { get; set; }
        public string REP_MESSAGE { get; set; }
        public string REP_COUNT { get; set; }
        public string REP_DATE { get; set; }
        public string REP_TIME { get; set; }

        public override string ToString()
        {
            return "REP_FLAG: " + REP_FLAG + "\nREP_MESSAGE: " + REP_MESSAGE + "\nREP_COUNT: " + REP_COUNT + "\nREP_DATE: " + REP_DATE + "\nREP_TIME: " + REP_TIME;
        }
    }

    //전송용 class
    class BOMList
    {
        int ListCount = 10;

        public string[] RefineList()
        {
            //"urid", "parent_id", "child_id", "S$ItemType", "P$Number", "P$Version", "P$Name", "quantity", "S$Location", "S$Remark", "sequence" 
            var array = new string[this.ListCount];
            array[0] = "Tree_id";
            array[1] = "tree_level";
            array[2] = "sequence";
            array[3] = "parent_id";
            array[4] = "child_id";
            array[5] = "P$Number";
            array[6] = "P$Version";
            array[7] = "urid";
            array[8] = "quantity";
            array[9] = "s$supplytype";
            return array;
        }
    }

}
