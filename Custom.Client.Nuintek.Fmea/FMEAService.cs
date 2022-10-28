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

namespace Custom.Client.Nuintek.Fmea
{
    public class FMEAServices
    {
        ProgressForm progressform;

        BOMList list = new BOMList();
        private string IFProductTblName = "[CLK_ACC].[dbo].T_MD_MODL_PLM_RCV_Z";
        private string IFPartTblName = "[CLK_ACC].[dbo].T_MD_PART_PLM_RCV_Z";
        //string IFTbName = "[LCT].[LCT].[dbo].[lct_TIFPJTBOM]";
        //string IFTbName = "lct_TIFPJTBOM";//test 용 110번 table
        //string IFTbTranIDColName = "seq";
        public string UserID { get; set; }

        private bool bTestIgnoreMdmError = false;
        private bool bTestLocalMdm = false;

        public NuintekUtils Utils { get; set; }

        public FMEAServices()
        {
            Utils = new NuintekUtils();
            if (bTestLocalMdm)
            {
                IFProductTblName = "T_MD_MODL_PLM_RCV_Z";
                IFPartTblName = "T_MD_PART_PLM_RCV_Z";
            }
        }

        #region Public Service Interface
        public int SendPartMasterToMdm(List<string> objectIdList, string userId, bool isPart, string entityId)
        {
            //MessageBox.Show("send bom to sap " + objectIdList.Count);
            int result = 0;
            for (int list = 0; list < objectIdList.Count; list++)
            {
                string objId = objectIdList[list];
                result = -77;
                try
                {
                    string sql =
            "    SELECT 0 as tree_level, urid as child_id, c.P$IsERP,                                                                                                                                            " +
            "                  c.P$Number, c.P$MTART, c.P$Name, c.state, c.P$ZSPEC, c.P$MEINS, c.P$WERKS, c.P$SPART, c.P$MATKL, c.P$ZSVC, c.P$WRKST, c.P$BRGEW, c.P$GEWEI, c.P$NTGEW, c.P$ZCOPYORG, c.P$ZDEP,                                    " +
            "				   c.creator, c.create_date, c.P$ZVTWEG, c.P$ZTYPE, c.P$ZCONT, c.P$ZEFFIC, c.P$ZCAPA, c.P$ZREVI, c.P$ZCHASSIS, c.P$ZPRANK, c.P$ZCOLOR, c.P$ZYEAR, c.P$ZMS, c.P$ZPOWER,                                               " +
            "				   c.P$ZSET, c.P$ZBRAND, c.P$ZPGR, c.P$ZMOEM, c.P$ZCONTRY, c.P$ZRFG, c.P$ZPSORT, c.P$ZDOEX, c.P$ZOUTMAT, c.P$ZELECTRIC, c.P$ZHZ, c.P$ZPOWERCODE,                                                                     " +
            "				   c.P$ZWIDTH, c.P$ZDEPTH, c.P$ZHIGHT, c.P$ZWIDTHPACK, c.P$ZDEPTHPACK, c.P$ZHEIGHTPACK, c.P$ZQ20FT, c.P$ZQ40FT, c.P$ZQ40FTHQ, c.P$ZQ20FTWP, c.P$ZQ40FTWP, c.P$ZQ40FTWPHQ,                                            " +
            "				   c.P$ZBUYEROPTION, c.P$ZOUTDOOR, c.P$ZINDOOR1, c.P$ZINDOOR2, c.P$ZINDOOR3, c.P$ZINDOOR4, c.P$ZINSMAT, c.P$ZCOMPRESSOR1, c.P$ZCOMPRESSOR2,                                                                          " +
            "				   c.P$ZOPTION1, c.P$ZOPTION2, c.P$ZOPTION3, c.P$ZOPTION4, c.P$ZOPTION5,                                                                                                                                             " +
            "                  c.P$BSTME, c.P$UMREN, c.P$SOBSL, c.P$ZDRAW, c.P$ZOPTION6, c.P$ZTREAT, c.P$ZTHICK, c.P$ZSURFACETREAT, c.P$ZSORT, c.P$ZRLENGTH, c.P$ZLENGTH1, c.P$ZLENGTH2, c.P$ZLWIDTH, c.P$ZCHARAC, c.P$ZADD, c.P$ZADDCON,        " +
            "                  c.P$ZHEADSHAPE, c.P$ZBSPEC, c.P$ZMAT, c.P$ZHO, c.P$TYPE, c.P$ZSLENGTH, c.P$ZWSPEC, c.P$ZSORT1, c.P$ZSORT2, c.P$ZSORT3, c.P$ZSORT4, c.P$TRAGR, c.P$FMEASendState                                                                       " +
            "    FROM     part_info AS c                                                                                                                                                                       " +
            "    where urid = '" + objId + "'";

                    DataTable topDt = Services.ApplicationServices.DataSvc.ExecuteDataTable(sql);
                    DataRow topDr = topDt.Rows[0];

                    bool isNew = true;

                    result = InsertPartMaster(topDr, isNew, isPart, entityId);
                    if (result < 0)
                    {
                        return result;
                    }
                    DataTable structureDt = GetTopDownStructure(objId);
                    //// 전송 성공했으므로 IsERP 를 1로 설정 ==> 이부분을 주석처리함 (이유 : 건건히 성공할때 각각 IsERP 를 처리하도록 수정함)
                    for (int i = 0; i < structureDt.Rows.Count; i++)
                    {
                        DataRow dr = structureDt.Rows[i];
                        result = InsertPartMaster(dr, isNew, isPart, entityId);
                        if (result < 0)
                        {
                            return result;
                        }
                    }
                }
                catch (Exception ex2)
                {
                    MessageBox.Show("알 수 없는 오류: " + ex2.Message);
                }
                if (result < 0)
                {
                    return result;
                }
            }
            return result;
        }

        #endregion

        #region 파트마스터 등록 관련

        private bool IsError(int nRet, out string errMsg)
        {
            if (nRet < 0)
            {
                switch (nRet)
                {
                    case -1:
                        errMsg = "DB 오류 발생";
                        break;
                    case -3:
                        errMsg = "알수없는 MTART";
                        break;
                    case -11:   // sap create send fail
                    case -12:   // sap update send fail
                    case -99:   // Exception 발생
                    case -98:   // Exception 발생
                    case -97:   // Exception 발생
                        errMsg = "DB 오류 발생";
                        // 내부에서 msg 를 생성하는 케이스
                        break;
                    default:
                        errMsg = "알수없는 오류";
                        break;
                }
                return true;
            }
            errMsg = string.Empty;
            return false;
        }

        // hschoi : 반제품 부품
        //public int InsertPartMaster(DataRow dr)
        //{
        //    int result = -99;
        //    result = InsertPartInfo(dr, true);  // 나중에 바꿔야됌 true 값

        //    return result;
        //}

        public int InsertPartMaster(DataRow dr, bool isNew, bool isPart, string entityId)
        {
            int result = -99;
            try
            {
                string partId = dr["child_id"]?.ToString();
                string MTART = dr["P$MTART"]?.ToString();
                string FMEASendState = dr["P$FMEASendState"]?.ToString() ?? string.Empty;
                if (FMEASendState == "")
                {
                    isNew = true;
                }
                else if (FMEASendState == "U")
                {
                    isNew = false;
                }
                else if (FMEASendState == "S")
                {
                    return 1;
                }
                //MessageBox.Show("insert partmaster");
                if (MTART == "FERT" || MTART == "HAWA") // 제품 / 상품일 경우
                {
                    result = InsertProductInfo(dr, isNew);

                }
                else if (MTART == "HALB" || MTART == "ROH") // 반제품 / 부품일 경우
                {
                    result = InsertPartInfo(dr, isNew);
                }
                else
                {
                    result = -3;  // Invalid MTART
                }
                //if (result >= 0)
                //{
                //    //Adaptive.Service.Services.ApplicationServices.ObjectSvc.UpdatePropertyValue(partId, "P$IsERP", "1");
                //}
                Utils.SetSAPStatusValue(result, isPart, entityId, true);
                if (result > 0)
                {
                    UpdateSetFMEASendState(partId);
                }
            }
            catch (Exception ex)
            {
                Utils.SetSAPStatusValue(-1, isPart, entityId, true);
                if (!bTestIgnoreMdmError)
                    throw ex;
            }
            if (bTestIgnoreMdmError)
                return 99;

            return result;
        }

        static int serial = 0;
        private int InsertPartInfo(DataRow dr, bool isNew)
        {
            try
            {
                string IF_NUM          = DateTime.Now.ToString("yyyyMMddHHmmss") + serial;
                serial++;
                string MATNR           = dr["P$Number"]?.ToString();
                string MTART           = dr["P$MTART"]?.ToString();
                string MAKTX           = dr["P$Name"]?.ToString();
                string ZSPEC           = dr["P$ZSPEC"]?.ToString();
                string MSTAE = ""; //dr["state"]?.ToString();
                string WERKS           = dr["P$WERKS"]?.ToString();
                string SPART           = dr["P$SPART"]?.ToString();
                string MATKL           = dr["P$MATKL"]?.ToString();
                string MEINS           = dr["P$MEINS"]?.ToString();
                string BSTME           = dr["P$BSTME"]?.ToString();
                string UMREN           = dr["P$UMREN"]?.ToString();
                string SOBSL           = dr["P$SOBSL"]?.ToString();
                string ZSVC            = dr["P$ZSVC"]?.ToString();
                string ZPDRAW          = dr["P$ZDRAW"]?.ToString();
                if (ZPDRAW == "유") ZPDRAW = "1";
                else ZPDRAW = "0";
                string ZPDEP           = dr["P$ZDEP"]?.ToString();
                string ZPERNAM         = dr["creator"]?.ToString();
                DateTime date = (DateTime)dr["create_date"];
                string ZPERSDA         = date.ToString("yyyyMMdd");
                string ZPOPTION1       = dr["P$ZOPTION1"]?.ToString();
                string ZPOPTION2       = dr["P$ZOPTION2"]?.ToString();
                string ZPOPTION3       = dr["P$ZOPTION3"]?.ToString();
                string ZPOPTION4       = dr["P$ZOPTION4"]?.ToString();
                string ZPOPTION5       = dr["P$ZOPTION5"]?.ToString();
                string ZPOPTION6       = dr["P$ZOPTION6"]?.ToString();
                string ZPTYPE          = dr["P$TYPE"]?.ToString();
                string ZPTREAT         = dr["P$ZTREAT"]?.ToString();
                string ZPTHICK         = dr["P$ZTHICK"]?.ToString();
                string ZPSURFACETREAT  = dr["P$ZSURFACETREAT"]?.ToString();
                string ZPCOLOR         = dr["P$ZCOLOR"]?.ToString();
                string ZPSORT          = dr["P$ZSORT"]?.ToString();
                string ZPWIDTH         = dr["P$ZWIDTH"]?.ToString();
                string ZPDEPTH         = dr["P$ZDEPTH"]?.ToString();
                string ZPRLENGTH       = dr["P$ZRLENGTH"]?.ToString();
                string ZPLENGTH1       = dr["P$ZLENGTH1"]?.ToString();
                string ZPLENGTH2       = dr["P$ZLENGTH2"]?.ToString();
                string ZPALWIDTH       = dr["P$ZLWIDTH"]?.ToString();
                string ZPCHARAC        = dr["P$ZCHARAC"]?.ToString();
                string ZPADD           = dr["P$ZADD"]?.ToString();
                string ZPADDCON        = dr["P$ZADDCON"]?.ToString();
                string ZPHEADSHAPE     = dr["P$ZHEADSHAPE"]?.ToString();
                string ZPSLENGTH       = dr["P$ZSLENGTH"]?.ToString();
                string ZPBSPEC         = dr["P$ZBSPEC"]?.ToString();
                string ZPMAT           = dr["P$ZMAT"]?.ToString();
                string ZPHO            = dr["P$ZHO"]?.ToString();
                string ZPWSPEC         = dr["P$ZWSPEC"]?.ToString();
                string IF_FLAG         = "N";
                string IF_IUD          = isNew ? "I" : "U";
                string IF_WHO          = "PLM";
                string IF_DATE         = DateTime.Now.ToString("yyyyMMdd");
                string IF_TIME         = DateTime.Now.ToString("HHmmss");
                string IF_MSG          = "";


                string qry = @"insert into " + IFPartTblName + "(" +
                    "IF_NUMBER" +
                    ",MATNR"                  +
                    ",MTART"                  +
                    ",MAKTX"                  +
                    ",ZSPEC"                  +
                    ",MSTAE"                  +
                    ",WERKS"                  +
                    ",SPART"                  +
                    ",MATKL"                  +
                    ",MEINS"                  +
                    ",BSTME"                  +
                    ",UMREN"                  +
                    ",SOBSL"                  +
                    ",ZSVC"                   +
                    ",ZPDRAW"                 +
                    ",ZPDEP"                  +
                    ",ZPERNAM"                +
                    ",ZPERSDA"                +
                    ",ZPOPTION1"              +
                    ",ZPOPTION2"              +
                    ",ZPOPTION3"              +
                    ",ZPOPTION4"              +
                    ",ZPOPTION5"              +
                    ",ZPOPTION6"              +
                    ",ZPTYPE"                 +
                    ",ZPTREAT"                +
                    ",ZPTHICK"                +
                    ",ZPSURFACETREAT"         +
                    ",ZPCOLOR"                +
                    ",ZPSORT"                 +
                    ",ZPWIDTH"                +
                    ",ZPDEPTH"                +
                    ",ZPRLENGTH"              +
                    ",ZPLENGTH1"              +
                    ",ZPLENGTH2"              +
                    ",ZPALWIDTH"              +
                    ",ZPCHARAC"               +
                    ",ZPADD"                  +
                    ",ZPADDCON"               +
                    ",ZPHEADSHAPE"            +
                    ",ZPSLENGTH"              +
                    ",ZPBSPEC"                +
                    ",ZPMAT"                  +
                    ",ZPHO"                   +
                    ",ZPWSPEC"                +
                    ",IF_FLAG"                +
                    ",IF_IUD"                 +
                    ",IF_WHO"                 +
                    ",IF_DATE"                +
                    ",IF_TIME"                +
                    ",IF_MSG"                 +
                    ") values ('";
                qry += IF_NUM          + "', '";
                qry += MATNR           + "', '";
                qry += MTART           + "', '";
                qry += MAKTX           + "', '";
                qry += ZSPEC           + "', '";
                qry += MSTAE           + "', '";
                qry += WERKS           + "', '";
                qry += SPART           + "', '";
                qry += MATKL           + "', '";
                qry += MEINS           + "', '";
                qry += BSTME           + "', '";
                qry += UMREN           + "', '";
                qry += SOBSL           + "', '";
                qry += ZSVC            + "', '";
                qry += ZPDRAW          + "', '";
                qry += ZPDEP           + "', '";
                qry += ZPERNAM         + "', '";
                qry += ZPERSDA         + "', '";
                qry += ZPOPTION1       + "', '";
                qry += ZPOPTION2       + "', '";
                qry += ZPOPTION3       + "', '";
                qry += ZPOPTION4       + "', '";
                qry += ZPOPTION5       + "', '";
                qry += ZPOPTION6       + "', '";
                qry += ZPTYPE          + "', '";
                qry += ZPTREAT         + "', '";
                qry += ZPTHICK         + "', '";
                qry += ZPSURFACETREAT  + "', '";
                qry += ZPCOLOR         + "', '";
                qry += ZPSORT          + "', '";
                qry += ZPWIDTH         + "', '";
                qry += ZPDEPTH         + "', '";
                qry += ZPRLENGTH       + "', '";
                qry += ZPLENGTH1       + "', '";
                qry += ZPLENGTH2       + "', '";
                qry += ZPALWIDTH       + "', '";
                qry += ZPCHARAC        + "', '";
                qry += ZPADD           + "', '";
                qry += ZPADDCON        + "', '";
                qry += ZPHEADSHAPE     + "', '";
                qry += ZPSLENGTH       + "', '";
                qry += ZPBSPEC         + "', '";
                qry += ZPMAT           + "', '";
                qry += ZPHO            + "', '";
                qry += ZPWSPEC         + "', '";
                qry += IF_FLAG         + "', '";
                qry += IF_IUD          + "', '";
                qry += IF_WHO          + "', '";
                qry += IF_DATE         + "', '";
                qry += IF_TIME         + "', '";
                qry += IF_MSG + "'";
                qry += ")";

                return Adaptive.Service.Services.ApplicationServices.DataSvc.ExecuteNonQuery(qry);
            }
            catch (Exception ex)
            {
                throw ex;
                //Adaptive.Service.Services.ApplicationServices.HistorySvc.CreateLog("userAddress", "userName", "제상품 정보 연계오류(ProductInfo)", ex.Message);
                //return -98;
            }
        }

        private int InsertProductInfo(DataRow dr, bool isNew)
        {
            try
            {
                string IF_NUM           = DateTime.Now.ToString("yyyyMMddHHmmss") + serial;
                serial++;
                string MATNR            = dr["P$Number"]?.ToString();
                string MTART            = dr["P$MTART"]?.ToString();
                string MAKTX            = dr["P$Name"]?.ToString();
                string MSTAE = ""; //dr["State"]?.ToString();
                string ZSPEC            = dr["P$ZSPEC"]?.ToString();
                string MEINS            = dr["P$MEINS"]?.ToString();
                string WERKS            = dr["P$WERKS"]?.ToString();
                string SPART            = dr["P$SPART"]?.ToString();
                string MATKL            = dr["P$MATKL"]?.ToString();
                string ZSVC             = dr["P$ZSVC"]?.ToString();
                string WRKST            = dr["P$WRKST"]?.ToString();
                string BRGEW            = dr["P$BRGEW"]?.ToString();
                double dBRGEW = 0;
                double.TryParse(BRGEW, out dBRGEW);
                BRGEW = string.Format("{0:N3}", dBRGEW);
                string GEWEI            = dr["P$GEWEI"]?.ToString();
                string NTGEW            = dr["P$NTGEW"]?.ToString();
                double dNTGEW = 0;
                double.TryParse(NTGEW, out dNTGEW);
                NTGEW = string.Format("{0:N3}", dNTGEW);
                string ZCOPYORG         = dr["P$ZCOPYORG"]?.ToString();
                string ZDEP             = dr["P$ZDEP"]?.ToString();
                string ZERNAM           = dr["creator"]?.ToString();
                DateTime date = (DateTime)dr["create_date"];
                //string ZPERSDA = date.ToString("yyyyMMdd");
                string ZERSDA           = date.ToString("yyyyMMdd");
                string ZVTWEG           = dr["P$ZVTWEG"]?.ToString();
                string ZTYPE            = dr["P$ZTYPE"]?.ToString();
                string ZCONT            = dr["P$ZCONT"]?.ToString();
                string ZEFFIC           = dr["P$ZEFFIC"]?.ToString();
                string ZCAPA            = dr["P$ZCAPA"]?.ToString();
                string ZREVI            = dr["P$ZREVI"]?.ToString();
                string ZCHASSIS         = dr["P$ZCHASSIS"]?.ToString();
                string ZPRANK           = dr["P$ZPRANK"]?.ToString();
                string ZCOLOR           = dr["P$ZCOLOR"]?.ToString();
                string ZYEAR            = dr["P$ZYEAR"]?.ToString();
                string ZMS              = dr["P$ZMS"]?.ToString();
                string ZPOWER           = dr["P$ZPOWER"]?.ToString();
                string ZSET             = dr["P$ZSET"]?.ToString();
                string ZBRAND           = dr["P$ZBRAND"]?.ToString();
                string ZPGR             = dr["P$ZPGR"]?.ToString();
                string ZMOEM            = dr["P$ZMOEM"]?.ToString();
                string ZCONTRY          = dr["P$ZCONTRY"]?.ToString();
                string ZRFG             = dr["P$ZRFG"]?.ToString();
                string ZPSORT           = dr["P$ZPSORT"]?.ToString();
                string ZDOEX            = dr["P$ZDOEX"]?.ToString();
                string ZOUTMAT          = dr["P$ZOUTMAT"]?.ToString();
                string ZELECTRIC        = dr["P$ZELECTRIC"]?.ToString();
                string ZHZ              = dr["P$ZHZ"]?.ToString();
                string ZPOWERCODE       = dr["P$ZPOWERCODE"]?.ToString();
                string ZWIDTH           = dr["P$ZWIDTH"]?.ToString();
                string ZDEPTH           = dr["P$ZDEPTH"]?.ToString();
                string ZHIGHT           = dr["P$ZHIGHT"]?.ToString();
                string ZWIDTHPACK       = dr["P$ZWIDTHPACK"]?.ToString();
                string ZDEPTHPACK       = dr["P$ZDEPTHPACK"]?.ToString();
                string ZHEIGHTPACK      = dr["P$ZHEIGHTPACK"]?.ToString();
                string ZQ20FT           = dr["P$ZQ20FT"]?.ToString();
                string ZQ40FT           = dr["P$ZQ40FT"]?.ToString();
                string ZQ40FTHQ         = dr["P$ZQ40FTHQ"]?.ToString();
                string ZQ20FTWP         = dr["P$ZQ20FTWP"]?.ToString();
                string ZQ40FTWP         = dr["P$ZQ40FTWP"]?.ToString();
                string ZQ40FTWPHQ       = dr["P$ZQ40FTWPHQ"]?.ToString();
                string ZBUYEROPTION     = dr["P$ZBUYEROPTION"]?.ToString();
                string ZOUTDOOR         = dr["P$ZOUTDOOR"]?.ToString();
                string ZINDOOR1         = dr["P$ZINDOOR1"]?.ToString();
                string ZINDOOR2         = dr["P$ZINDOOR2"]?.ToString();
                string ZINDOOR3         = dr["P$ZINDOOR3"]?.ToString();
                string ZINDOOR4         = dr["P$ZINDOOR4"]?.ToString();
                string ZINSMAT          = dr["P$ZINSMAT"]?.ToString();
                string ZCOMPRESSOR1     = dr["P$ZCOMPRESSOR1"]?.ToString();
                string ZCOMPRESSOR2     = dr["P$ZCOMPRESSOR2"]?.ToString();
                string ZOPTION1         = dr["P$ZOPTION1"]?.ToString();
                string ZOPTION2         = dr["P$ZOPTION2"]?.ToString();
                string ZOPTION3         = dr["P$ZOPTION3"]?.ToString();
                string ZOPTION4         = dr["P$ZOPTION4"]?.ToString();
                string ZOPTION5         = dr["P$ZOPTION5"]?.ToString();
                string IF_FLAG = "N";
                string IF_IUD = isNew ? "I" : "U";
                string IF_WHO = "PLM";
                string IF_DATE = DateTime.Now.ToString("yyyyMMdd");
                string IF_TIME = DateTime.Now.ToString("HHmmss");
                string IF_MSG = "";
                string ZSORT1 = dr["P$ZSORT1"]?.ToString();
                string ZSORT2 = dr["P$ZSORT2"]?.ToString();
                string ZSORT3 = dr["P$ZSORT3"]?.ToString();
                string ZSORT4 = dr["P$ZSORT4"]?.ToString();
                string TRAGR = dr["P$TRAGR"]?.ToString();

                string qry = @"insert into " + IFProductTblName + "(" +
                    "IF_NUMBER" +
                    ",MATNR" +
                    ",MTART" +
                    ",MAKTX" +
                    ",MSTAE" +
                    ",ZSPEC" +
                    ",MEINS" +
                    ",WERKS" +
                    ",SPART" +
                    ",MATKL" +
                    ",ZSVC" +
                    ",WRKST" +
                    ",BRGEW" +
                    ",GEWEI" +
                    ",NTGEW" +
                    ",ZCOPYORG" +
                    ",ZDEP" +
                    ",ZERNAM" +
                    ",ZERSDA" +
                    ",ZVTWEG" +
                    ",ZTYPE" +
                    ",ZCONT" +
                    ",ZEFFIC" +
                    ",ZCAPA" +
                    ",ZREVI" +
                    ",ZCHASSIS" +
                    ",ZPRANK" +
                    ",ZCOLOR" +
                    ",ZYEAR" +
                    ",ZMS" +
                    ",ZPOWER" +
                    ",ZSET" +
                    ",ZBRAND" +
                    ",ZPGR" +
                    ",ZMOEM" +
                    ",ZCONTRY" +
                    ",ZRFG" +
                    ",ZPSORT" +
                    ",ZDOEX" +
                    ",ZOUTMAT" +
                    ",ZELECTRIC" +
                    ",ZHZ" +
                    ",ZPOWERCODE" +
                    ",ZWIDTH" +
                    ",ZDEPTH" +
                    ",ZHIGHT" +
                    ",ZWIDTHPACK" +
                    ",ZDEPTHPACK" +
                    ",ZHEIGHTPACK" +
                    ",ZQ20FT" +
                    ",ZQ40FT" +
                    ",ZQ40FTHQ" +
                    ",ZQ20FTWP" +
                    ",ZQ40FTWP" +
                    ",ZQ40FTWPHQ" +
                    ",ZBUYEROPTION" +
                    ",ZOUTDOOR" +
                    ",ZINDOOR1" +
                    ",ZINDOOR2" +
                    ",ZINDOOR3" +
                    ",ZINDOOR4" +
                    ",ZINSMAT" +
                    ",ZCOMPRESSOR1" +
                    ",ZCOMPRESSOR2" +
                    ",ZOPTION1" +
                    ",ZOPTION2" +
                    ",ZOPTION3" +
                    ",ZOPTION4" +
                    ",ZOPTION5" +
                    ",IF_FLAG" +
                    ",IF_IUD" +
                    ",IF_WHO" +
                    ",IF_DATE" +
                    ",IF_TIME" +
                    ",IF_MSG" +
                    ",ZSORT1" +
                    ",ZSORT2" +
                    ",ZSORT3" +
                    ",ZSORT4" +
                    //",TRAGR" +
                    ",DISGR" +
                    ") values ('"; 
                qry += IF_NUM + "', '";
                qry += MATNR + "', '";
                qry += MTART + "', '";
                qry += MAKTX + "', '";
                qry += MSTAE + "', '";
                qry += ZSPEC + "', '";
                qry += MEINS + "', '";
                qry += WERKS + "', '";
                qry += SPART + "', '";
                qry += MATKL + "', '";
                qry += ZSVC + "', '";
                qry += WRKST + "', '";
                qry += BRGEW + "', '";
                qry += GEWEI + "', '";
                qry += NTGEW + "', '";
                qry += ZCOPYORG + "', '";
                qry += ZDEP + "', '";
                qry += ZERNAM + "', '";
                qry += ZERSDA + "', '";
                qry += ZVTWEG + "', '";
                qry += ZTYPE + "', '";
                qry += ZCONT + "', '";
                qry += ZEFFIC + "', '";
                qry += ZCAPA + "', '";
                qry += ZREVI + "', '";
                qry += ZCHASSIS + "', '";
                qry += ZPRANK + "', '";
                qry += ZCOLOR + "', '";
                qry += ZYEAR + "', '";
                qry += ZMS + "', '";
                qry += ZPOWER + "', '";
                qry += ZSET + "', '";
                qry += ZBRAND + "', '";
                qry += ZPGR + "', '";
                qry += ZMOEM + "', '";
                qry += ZCONTRY + "', '";
                qry += ZRFG + "', '";
                qry += ZPSORT + "', '";
                qry += ZDOEX + "', '";
                qry += ZOUTMAT + "', '";
                qry += ZELECTRIC + "', '";
                qry += ZHZ + "', '";
                qry += ZPOWERCODE + "', '";
                qry += ZWIDTH + "', '";
                qry += ZDEPTH + "', '";
                qry += ZHIGHT + "', '";
                qry += ZWIDTHPACK + "', '";
                qry += ZDEPTHPACK + "', '";
                qry += ZHEIGHTPACK + "', '";
                qry += ZQ20FT + "', '";
                qry += ZQ40FT + "', '";
                qry += ZQ40FTHQ + "', '";
                qry += ZQ20FTWP + "', '";
                qry += ZQ40FTWP + "', '";
                qry += ZQ40FTWPHQ + "', '";
                qry += ZBUYEROPTION + "', '";
                qry += ZOUTDOOR + "', '";
                qry += ZINDOOR1 + "', '";
                qry += ZINDOOR2 + "', '";
                qry += ZINDOOR3 + "', '";
                qry += ZINDOOR4 + "', '";
                qry += ZINSMAT + "', '";
                qry += ZCOMPRESSOR1 + "', '";
                qry += ZCOMPRESSOR2 + "', '";
                qry += ZOPTION1 + "', '";
                qry += ZOPTION2 + "', '";
                qry += ZOPTION3 + "', '";
                qry += ZOPTION4 + "', '";
                qry += ZOPTION5 + "', '";
                qry += IF_FLAG + "', '";
                qry += IF_IUD + "', '";
                qry += IF_WHO + "', '";
                qry += IF_DATE + "', '";
                qry += IF_TIME + "', '";
                qry += IF_MSG +  "', '";
                qry += ZSORT1 +  "', '";
                qry += ZSORT2 +  "', '";
                qry += ZSORT3 +  "', '";
                qry += ZSORT4 + "', '";
                qry += TRAGR + "'";
                qry += ")";

                return Adaptive.Service.Services.ApplicationServices.DataSvc.ExecuteNonQuery(qry);
            }
            catch (Exception ex)
            {
                throw ex;
                //Adaptive.Service.Services.ApplicationServices.HistorySvc.CreateLog("userAddress", "userName", "제상품 정보 연계오류(ProductInfo)", ex.Message);
                //return -97;
            }
        }


        ////Trans_ID get
        //public string getBOMTransID()
        //{
        //    string transID = "1";

        //    string getTransQuery = @"select top(1) " + IFTbTranIDColName + " from " + IFTbName + " order by " + IFTbTranIDColName + " desc";
        //    DataTable dt = Adaptive.Service.Services.ApplicationServices.DataSvc.ExecuteDataTable(getTransQuery);
        //    if (dt.Rows.Count == 0)
        //    {
        //        return transID;
        //    }
        //    else
        //    {
        //        int count = int.Parse(dt.Rows[0].ItemArray[0].ToString()) + 1;
        //        transID = count.ToString();
        //    }
        //    return transID;
        //}

        // ERP전송 후 P$IsERPSent 1로 업데이트
        public int UpdateClearFMEASendState(string entityId)
        {
            //string strIsErp = bSetIsErp ? "1" : "0";
            //Adaptive.Service.Services.ApplicationServices.ObjectSvc.UpdatePropertyValue(entityId, "P$IsERP", strIsErp);

            string updateQuery = @"update p set p.P$FMEASendState = 'U' from part_properties p inner join " +
                "(select a.urid from part_info a inner join part_properties b on a.P$Number = b.P$Number where a.latest = 1 and b.part_id = '" + entityId + "') c on p.part_id = c.urid";
            return Adaptive.Service.Services.ApplicationServices.DataSvc.ExecuteNonQuery(updateQuery);
        }

        public int UpdateSetFMEASendState(string entityId)
        {
            //string strIsErp = bSetIsErp ? "1" : "0";
            //Adaptive.Service.Services.ApplicationServices.ObjectSvc.UpdatePropertyValue(entityId, "P$IsERP", strIsErp);

            string updateQuery = @"update part_properties set P$FMEASendState = 'S' where part_id = '" + entityId + "'";
            return Adaptive.Service.Services.ApplicationServices.DataSvc.ExecuteNonQuery(updateQuery);
        }

        private DataTable GetTopDownStructure(string urid)
        {
            try
            {
                string query =
            "WITH structure AS(SELECT  1 AS tree_level, CAST(0 AS VARCHAR(100)) +'.' + RIGHT('00000' + CAST(i.sequence AS VARCHAR(5)), 5) + '-' + RIGHT('00' + Cast((ROW_NUMBER() OVER (PARTITION BY i.parent_id, i.sequence ORDER BY i.sequence)) AS VARCHAR(2)), 2) AS tree_id, i.urid, i.parent_id, i.child_id, i.sequence, i.quantity, i.structure_id,                   " +
            "                                 i.S$Material, i.S$BDDIS, i.S$BDEFF, i.S$IsSAP                                                                                                                                                                 " +
            "                  FROM     structure_info AS i INNER JOIN                                                                                                                                                                           " +
            "                                 part_entity AS c ON i.child_id = c.urid                                                                                                                                                            " +
            "                  WHERE(i.structure_id LIKE 'S$EBOM%') AND(i.parent_id = '" + urid + "') AND(c.deleted = 0)                                                                                                                    " +
            "                  UNION ALL                                                                                                                                                                                                         " +
            "                  SELECT s.tree_level + 1 AS Expr1, CAST(s.tree_id AS VARCHAR(100)) +'.' + RIGHT('00000' + CAST(i.sequence AS VARCHAR(5)), 5) + '-' + RIGHT('00' + Cast((ROW_NUMBER() OVER (PARTITION BY i.parent_id, i.sequence ORDER BY i.sequence)) AS VARCHAR(2)), 2) AS tree_id, i.urid, i.parent_id, i.child_id, i.sequence, i.quantity, i.structure_id,  " +
            "                                 i.S$Material, i.S$BDDIS, i.S$BDEFF, i.S$IsSAP                                                                                                                                                              " +
            "                  FROM     structure_info AS i INNER JOIN                                                                                                                                                                           " +
            "                                 structure AS s ON i.parent_id = s.child_id INNER JOIN                                                                                                                                              " +
            "                                 part_entity AS c ON i.child_id = c.urid                                                                                                                                                            " +
            "                  WHERE(i.structure_id LIKE 'S$EBOM%') AND(c.deleted = 0))                                                                                                                                                          " +
            "    SELECT s.tree_id, s.tree_level, s.urid, s.parent_id, s.child_id,                                                                                                                                                                " +
            "                  s.sequence, s.quantity, s.S$Material, s.S$BDDIS, s.S$BDEFF, s.S$IsSAP, c.P$IsERP,                                                                                                                                            " +
            "                  c.P$Number, c.P$MTART, c.P$Name, c.state, c.P$ZSPEC, c.P$MEINS, c.P$WERKS, c.P$SPART, c.P$MATKL, c.P$ZSVC, c.P$WRKST, c.P$BRGEW, c.P$GEWEI, c.P$NTGEW, c.P$ZCOPYORG, c.P$ZDEP,                                    " +
            "				   c.creator, c.create_date, c.P$ZVTWEG, c.P$ZTYPE, c.P$ZCONT, c.P$ZEFFIC, c.P$ZCAPA, c.P$ZREVI, c.P$ZCHASSIS, c.P$ZPRANK, c.P$ZCOLOR, c.P$ZYEAR, c.P$ZMS, c.P$ZPOWER,                                               " +
            "				   c.P$ZSET, c.P$ZBRAND, c.P$ZPGR, c.P$ZMOEM, c.P$ZCONTRY, c.P$ZRFG, c.P$ZPSORT, c.P$ZDOEX, c.P$ZOUTMAT, c.P$ZELECTRIC, c.P$ZHZ, c.P$ZPOWERCODE,                                                                     " +
            "				   c.P$ZWIDTH, c.P$ZDEPTH, c.P$ZHIGHT, c.P$ZWIDTHPACK, c.P$ZDEPTHPACK, c.P$ZHEIGHTPACK, c.P$ZQ20FT, c.P$ZQ40FT, c.P$ZQ40FTHQ, c.P$ZQ20FTWP, c.P$ZQ40FTWP, c.P$ZQ40FTWPHQ,                                            " +
            "				   c.P$ZBUYEROPTION, c.P$ZOUTDOOR, c.P$ZINDOOR1, c.P$ZINDOOR2, c.P$ZINDOOR3, c.P$ZINDOOR4, c.P$ZINSMAT, c.P$ZCOMPRESSOR1, c.P$ZCOMPRESSOR2,                                                                          " +
            "				   c.P$ZOPTION1, c.P$ZOPTION2, c.P$ZOPTION3, c.P$ZOPTION4, c.P$ZOPTION5,                                                                                                                                             " +
            "                  c.P$BSTME, c.P$UMREN, c.P$SOBSL, c.P$ZDRAW, c.P$ZOPTION6, c.P$ZTREAT, c.P$ZTHICK, c.P$ZSURFACETREAT, c.P$ZSORT, c.P$ZRLENGTH, c.P$ZLENGTH1, c.P$ZLENGTH2, c.P$ZLWIDTH, c.P$ZCHARAC, c.P$ZADD, c.P$ZADDCON,        " +
            "                  c.P$ZHEADSHAPE, c.P$ZBSPEC, c.P$ZMAT, c.P$ZHO, c.P$TYPE, c.P$ZSLENGTH, c.P$ZWSPEC, c.P$ZSORT1, c.P$ZSORT2, c.P$ZSORT3, c.P$ZSORT4, c.P$TRAGR, c.P$FMEASendState                                                                                                                                " +
            "    FROM     structure AS s INNER JOIN                                                                                                                                                                                              " +
            "                   part_info AS c ON s.child_id = c.part_id                                                                                                                                                                         " +
            "    ORDER BY s.tree_id                                                                                                                                                                                                              ";


                DataTable topDt = Services.ApplicationServices.DataSvc.ExecuteDataTable(query);
                return topDt;
            }
            catch (Exception ex)
            {
                //MessageBox.Show("TopDownStructure Error: " + ex.Message);
                throw ex;
            }
        }

        #endregion


    }
}
