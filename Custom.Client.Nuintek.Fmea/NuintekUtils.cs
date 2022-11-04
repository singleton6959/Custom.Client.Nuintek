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

        public DataTable GetTopDownStructure(string urid)
        {
            string preQuery = @"
                                WITH structure
                                     AS (SELECT 1                                        AS tree_level,
                                                Cast(0 AS VARCHAR(100)) + '.'
                                                + RIGHT('00000' + Cast(i.sequence AS VARCHAR(5)), 5)
                                                + '-'
                                                + RIGHT('00' + Cast((Row_number() OVER (partition BY
                                                i.parent_id,
                                                    i.sequence
                                                ORDER BY i.sequence)) AS VARCHAR(2)), 2) AS tree_id,
                                                i.urid,
                                                i.parent_id,
                                                i.child_id,
                                                i.structure_id,
                                                i.sequence,
				                                i.quantity
                                         FROM   structure_info AS i
                                                INNER JOIN part_entity AS c
                                                        ON i.child_id = c.urid
                                         WHERE ( i.structure_id LIKE 'S$EBOM%' )
                                               AND ( i.parent_id = '{0}' )
                                               AND ( c.deleted = 0 )
                                         UNION ALL
                                         SELECT s.tree_level + 1                         AS Expr1,
                                                Cast(s.tree_id AS VARCHAR(100)) + '.'
                                                + RIGHT('00000' + Cast(i.sequence AS VARCHAR(5)), 5)
                                                + '-'
                                                + RIGHT('00' + Cast((Row_number() OVER (partition BY
                                                i.parent_id,
                                                i.sequence
                                                ORDER BY i.sequence)) AS VARCHAR(2)), 2) AS tree_id,
                                                i.urid,
                                                i.parent_id,
                                                i.child_id,
                                                i.structure_id,
                                                i.sequence,
				                                i.quantity
                                         FROM   structure_info AS i
                                                INNER JOIN structure AS s
                                                        ON i.parent_id = s.child_id
                                                INNER JOIN part_entity AS c
                                                        ON i.child_id = c.urid
                                         WHERE ( i.structure_id LIKE 'S$EBOM%' )
                                               AND ( c.deleted = 0 ))
                                SELECT s.tree_id,
                                       s.tree_level,
                                       s.urid,
                                       s.parent_id,
                                       s.child_id,
                                       s.structure_id,
                                       s.sequence,
	                                   s.quantity,
                                       c.p$number,
                                       c.p$name,
                                       c.P$Spec,
                                       c.P$SendFMEAStatus,
	                                   c.P$Model,
	                                   c.P$ClassCode,
	                                   cp.C$Code AS ModelCode,
	                                   cpp.C$Code AS ClassCodeCode
                                FROM   structure AS s
                                       INNER JOIN part_info AS c
                                               ON s.child_id = c.part_id
	                                   LEFT OUTER JOIN code_properties cp ON C.P$Model = cp.C$Name
	                                   LEFT OUTER JOIN code_properties cpp ON C.P$ClassCode = cpp.C$Name
                                ORDER  BY s.tree_id
                                ";
            string sql = string.Format(preQuery, urid);
            DataTable topDt = Services.ApplicationServices.DataSvc.ExecuteDataTable(sql);
            return topDt;
        }

        public void UpdateSendFMEAStatus(string status, string urid)
        {
            try
            {
                string sql = string.Empty;
                if (status == "S")
                {
                    sql = string.Format("UPDATE part_properties SET P$SendFMEAStatus='{0}' WHERE part_id='{1}'", status, urid);
                }
                else if (status == "U")
                {
                    sql = @"update p set p.P$SendFMEAStatus = 'U' from part_properties p inner join " +
                    "(select a.urid from part_info a inner join part_properties b on a.P$Number = b.P$Number where a.latest = 1 and b.part_id = '" + urid + "') c on p.part_id = c.urid";
                }
                Adaptive.Service.Services.ApplicationServices.DataSvc.ExecuteNonQuery(sql);
            }
            catch (Exception ex)
            {

            }
            
        }

    }
}
