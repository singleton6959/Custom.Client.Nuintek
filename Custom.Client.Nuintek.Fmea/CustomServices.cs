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
        #region USER
        public string CreateUser(string entityId, string userId)
        {
            FMEAServices fmeaService = new FMEAServices();
            fmeaService.SendUserInfo(entityId, "I");
            return string.Empty;
        }
        public string UpdateUser(string entityId, string userId)
        {
            FMEAServices fmeaService = new FMEAServices();
            fmeaService.SendUserInfo(entityId, "U");
            return string.Empty;
        }
        public string DeleteUser(string entityId, string userId)
        {
            FMEAServices fmeaService = new FMEAServices();
            fmeaService.SendUserInfo(entityId, "D");
            return string.Empty;
        }
        #endregion

        #region PART
        //public string CreatePart(string entityId, string userId)
        //{
        //    FMEAServices fmeaService = new FMEAServices();
        //    fmeaService.SendPartInfo(entityId, "I");
        //    return string.Empty;
        //}
        //public string UpdatePart(string entityId, string userId)
        //{
        //    FMEAServices fmeaService = new FMEAServices();
        //    fmeaService.SendPartInfo(entityId, "U");
        //    return string.Empty;
        //}
        //public string DeletePart(string entityId, string userId)
        //{
        //    FMEAServices fmeaService = new FMEAServices();
        //    fmeaService.SendPartInfo(entityId, "D");
        //    return string.Empty;
        //}
        #endregion

        #region CODE
        public string CreateCode(string entityId, string userId)
        {
            FMEAServices fmeaService = new FMEAServices();
            fmeaService.SendCodeInfo(entityId, "I");
            return string.Empty;
        }
        public string UpdateCode(string entityId, string userId)
        {
            FMEAServices fmeaService = new FMEAServices();
            fmeaService.SendCodeInfo(entityId, "U");
            return string.Empty;
        }
        public string DeleteCode(string entityId, string userId)
        {
            FMEAServices fmeaService = new FMEAServices();
            fmeaService.SendCodeInfo(entityId, "D");
            return string.Empty;
        }
        #endregion

        #region PROJECT
        public string CreateProject(string entityId, string userId)
        {
            FMEAServices fmeaService = new FMEAServices();
            fmeaService.SendProjectInfo(entityId, "I");
            return string.Empty;
        }
        public string UpdateProject(string entityId, string userId)
        {
            FMEAServices fmeaService = new FMEAServices();
            fmeaService.SendProjectInfo(entityId, "U");
            return string.Empty;
        }
        public string DeleteProject(string entityId, string userId)
        {
            FMEAServices fmeaService = new FMEAServices();
            fmeaService.SendProjectInfo(entityId, "D");
            return string.Empty;
        }
        #endregion

        #region BOM
        //public string CreateBOM(string entityId, string userId)
        //{
        //    FMEAServices fmeaService = new FMEAServices();
        //    fmeaService.SendBOMAndPartInfo(entityId, "I");
        //    return string.Empty;
        //}
        //public string UpdateBOM(string entityId, string userId)
        //{
        //    FMEAServices fmeaService = new FMEAServices();
        //    fmeaService.SendBOMAndPartInfo(entityId, "U");
        //    return string.Empty;
        //}
        //public string DeleteBOM(string entityId, string userId)
        //{
        //    FMEAServices fmeaService = new FMEAServices();
        //    fmeaService.SendBOMAndPartInfo(entityId, "D");
        //    return string.Empty;
        //}
        #endregion

        public string UpdateBOMPart(string entityId, string userId)
        {
            FMEAServices fmeaService = new FMEAServices();
            fmeaService.SendBOMAndPartInfo(entityId);

            return string.Empty;
        }

        public string RevisionBOM(string entityId, string userId)
        {
            NuintekUtils nu = new NuintekUtils();
            nu.UpdateSendFMEAStatus("U", entityId);
            return string.Empty;
        }
    }
}
