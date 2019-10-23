using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardIssuanceSystem.DAL.DataAccessClasses
{
    public class FilePathDataAccess
    {
        #region Insert
        public bool AddFilePath(tbl_File_Paths row)
        {

            try
            {
                using (var db = new SoneriCISEntities())
                {
                    db.tbl_File_Paths.Add(row);
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
        public bool UpdateFilePath(tbl_File_Paths row)
        {

            try
            {
                using (var db = new SoneriCISEntities())
                {
                    tbl_File_Paths val = new DAL.tbl_File_Paths();
                    val = db.tbl_File_Paths.Where(a => a.ID == row.ID).FirstOrDefault();
                    val.PathType = row.PathType;
                    val.Path = row.Path;
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

        #region Delete



        #endregion

        #region Select

        public List<tbl_File_Paths> GetFilePath()
        {
            try
            {
                List<tbl_File_Paths> lst = new List<DAL.tbl_File_Paths>();
                using (var db = new SoneriCISEntities())
                {
                    lst = db.tbl_File_Paths.ToList();
                }
                return lst;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public tbl_File_Paths GetFilePathByTypeID(string typeID)
        {
            try
            {
                tbl_File_Paths row = new DAL.tbl_File_Paths();
                using (var db = new SoneriCISEntities())
                {
                    row = db.tbl_File_Paths.FirstOrDefault(e => e.PathType == typeID);
                }
                return row;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public tbl_File_Paths GetFilePathByTypeID2(string typeID)
        {
            try
            {
                tbl_File_Paths row = new DAL.tbl_File_Paths();
                using (var db = new SoneriCISEntities())
                {
                    row = db.tbl_File_Paths.FirstOrDefault(e => e.PathType == typeID);
                }
                return row;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion
    }
}
