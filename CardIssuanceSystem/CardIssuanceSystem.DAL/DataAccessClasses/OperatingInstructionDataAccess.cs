using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardIssuanceSystem.DAL.DataAccessClasses
{
    public class OperatingInstructionDataAccess
    {
        #region Insert
        public bool AddOperatingInstruction(tbl_OperatingInstructions row)
        {

            try
            {
                using (var db = new SoneriCISEntities())
                {
                    db.tbl_OperatingInstructions.Add(row);
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
        public bool UpdateOperatingInstructions(tbl_OperatingInstructions row)
        {

            try
            {
                using (var db = new SoneriCISEntities())
                {
                    tbl_OperatingInstructions val = new tbl_OperatingInstructions();
                    val = db.tbl_OperatingInstructions.Where(a => a.Code==row.Code).FirstOrDefault();
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

        public bool DeleteOperatingInstructions(tbl_OperatingInstructions row)
        {

            try
            {
                using (var db = new SoneriCISEntities())
                {
                    tbl_OperatingInstructions val = new tbl_OperatingInstructions();
                    val = db.tbl_OperatingInstructions.Where(a => a.ID == row.ID).FirstOrDefault();
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

        public List<tbl_OperatingInstructions> GetOperatingInstructions()
        {
            try
            {
                List<tbl_OperatingInstructions> lst = new List<tbl_OperatingInstructions>();
                using (var db = new SoneriCISEntities())
                {
                    lst = db.tbl_OperatingInstructions.ToList();
                }
                return lst;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public tbl_OperatingInstructions GetOperatingInstructionsbyID(int ID)
        {
            try
            {
                tbl_OperatingInstructions row = new tbl_OperatingInstructions();
                using (var db = new SoneriCISEntities())
                {
                    row = db.tbl_OperatingInstructions.FirstOrDefault(e => e.ID == ID);
                }
                return row;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
        public tbl_OperatingInstructions GetOperatingInstructionsbyCode(string Code)
        {
            try
            {
                tbl_OperatingInstructions row = new tbl_OperatingInstructions();
                using (var db = new SoneriCISEntities())
                {
                    row = db.tbl_OperatingInstructions.FirstOrDefault(e => e.Code == Code);
                }
                return row;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
        public bool CheckPostingCode(string Code)
        {
            try
            {
                tbl_OperatingInstructions row = new tbl_OperatingInstructions();
                using (var db = new SoneriCISEntities())
                {
                    row = db.tbl_OperatingInstructions.FirstOrDefault(e => e.Code == Code && e.IsActive == true);
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
