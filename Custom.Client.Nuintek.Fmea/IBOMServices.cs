using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace Custom.Client.Nuintek.Fmea
{
    public interface IBOMServices
    {
        DataTable GetCompareTable(string entityId);

        string LinkwithERP(string entityId, string userId);

        string SendItemToSap(string entityId, string userId);

        string InsertBOMInfo(DataRow Item);

        string InsertPJInfo(string entityId, string userId);

        string UpdateItemToSap(string entityId, string userId);

        string UpdateIsSentFlag(string entityId, string userId);

        string GetPreVersionUrid(string entityId);

        DataTable CompareBOM(DataTable curTable, DataTable preTable);

        DataTable RefineCols(DataTable orignTable);

        string getBOMTransID();
    }

}
