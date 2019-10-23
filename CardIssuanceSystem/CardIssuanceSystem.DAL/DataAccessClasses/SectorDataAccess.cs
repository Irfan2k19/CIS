using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardIssuanceSystem.DAL.DataAccessClasses
{
    public class SectorDataAccess
    {

        #region Insert
        public bool AddSector(tbl_Sector row)
        {

            try
            {
                using (var db = new SoneriCISEntities())
                {
                    db.tbl_Sector.Add(row);
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
        public bool UpdateSector(tbl_Sector row)
        {

            try
            {
                using (var db = new SoneriCISEntities())
                {
                    tbl_Sector val = new DAL.tbl_Sector();
                    val = db.tbl_Sector.Where(a => a.Code == row.Code).FirstOrDefault();
                    val.Code = row.Code;
                    val.Description = row.Description;
                    val.IsActive = row.IsActive;
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

        public bool DeleteSector(tbl_Sector row)
        {

            try
            {
                using (var db = new SoneriCISEntities())
                {
                    tbl_Sector val = new DAL.tbl_Sector();
                    val = db.tbl_Sector.Where(a => a.ID == row.ID).FirstOrDefault();
                    val.IsActive = false;

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

        #region Select

        public List<tbl_Sector> GetSector()
        {
            try
            {
                List<tbl_Sector> lst = new List<tbl_Sector>();
                using (var db = new SoneriCISEntities())
                {
                    lst = db.tbl_Sector.ToList();
                }
                return lst;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public tbl_Sector GetSectorbyID(int ID)
        {
            try
            {
                tbl_Sector row = new tbl_Sector();
                using (var db = new SoneriCISEntities())
                {
                    row = db.tbl_Sector.FirstOrDefault(e => e.ID==ID);
                }
                return row;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public tbl_Sector GetSectorbyCode(string Code)
        {
            try
            {
                tbl_Sector row = new tbl_Sector();
                using (var db = new SoneriCISEntities())
                {
                    row = db.tbl_Sector.FirstOrDefault(e => e.Code == Code);
                }
                return row;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public bool CheckSector(string Code)
        {
            try
            {
                tbl_Sector row = new tbl_Sector();
                using (var db = new SoneriCISEntities())
                {
                    row = db.tbl_Sector.FirstOrDefault(e => e.Code == Code && e.IsActive==true);
                }
                if (row != null)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        #endregion
    }
}
