using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardIssuanceSystem.DAL.DataAccessClasses
{
    public class RegionDataAccess
    {
        #region Insert
        public bool AddRegion(tbl_Region row)
        {

            try
            {
                using (var db = new SoneriCISEntities())
                {
                    db.tbl_Region.Add(row);
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

        public bool UpdateRegion(tbl_Region row)
        {

            try
            {
                using (var db = new SoneriCISEntities())
                {
                    tbl_Region val = new tbl_Region();
                    val = db.tbl_Region.Where(a => a.Description == row.Description).FirstOrDefault();
                    val.Title = row.Title;
                    val.FED = row.FED;
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

        #endregion

        #region Select
        public List<tbl_Region> GetAllRegions()
        {
            try
            {
                List<tbl_Region> lst = new List<DAL.tbl_Region>();
                using (var db = new SoneriCISEntities())
                {
                    lst = db.tbl_Region.ToList();
                }
                return lst;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public List<tbl_Region> GetRegionsbyID(string Id)
        {
            try
            {
                List<tbl_Region> lst = new List<tbl_Region>();
                using (var db = new SoneriCISEntities())
                {
                    //lst = db.tbl_Region.Where(x=>x.Description==Id).Select(x => new  {  x.ID,  x.Title, x.Description }).FirstOrDefault();
                    lst = (from a in db.tbl_Region where a.Description == Id select new tbl_Region { ID = a.ID, Title = a.Title, Description = a.Description }).ToList();
                    
                }
                return lst;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public List<tbl_Region> GetRegionsbyID(string Id, bool isActive)
        {
            try
            {
                List<tbl_Region> lst = new List<tbl_Region>();
                using (var db = new SoneriCISEntities())
                {
                    //lst = db.tbl_Region.Where(x=>x.Description==Id).Select(x => new  {  x.ID,  x.Title, x.Description }).FirstOrDefault();
                    lst = db.tbl_Region.Where(e => e.Description == Id && e.IsActive == isActive).ToList();
                    //lst = (from a in db.tbl_Region where a.Description == Id && a.IsActive == isActive select new tbl_Region { ID = a.ID, Title = a.Title, Description = a.Description, FED = a.FED }).ToList();

                }
                return lst;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }




        #endregion
    }
}

