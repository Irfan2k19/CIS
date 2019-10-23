using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardIssuanceSystem.DAL.DataAccessClasses
{
    public class FileExportDataAccess
    {
        #region Insert
        public bool AddFileExport(tbl_File_Exports row)
        {

            try
            {
                using (var db = new SoneriCISEntities())
                {
                    db.tbl_File_Exports.Add(row);
                    db.SaveChanges();
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        #endregion

        #region Update
      

        #endregion

        #region Delete



        #endregion

        #region Select

        public List<tbl_File_Exports> GetFileExports()
        {
            try
            {
                List<tbl_File_Exports> lst = new List<DAL.tbl_File_Exports>();
                using (var db = new SoneriCISEntities())
                {
                    lst = db.tbl_File_Exports.ToList();
                }
                return lst;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public tbl_File_Exports GetFileExportById(int FileExportID)
        {
            try
            {
                tbl_File_Exports Exports = new DAL.tbl_File_Exports();
                using (var db = new SoneriCISEntities())
                {
                    Exports = db.tbl_File_Exports.Where(a => a.ID == FileExportID).FirstOrDefault();
                }
                return Exports;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion
    }
}
