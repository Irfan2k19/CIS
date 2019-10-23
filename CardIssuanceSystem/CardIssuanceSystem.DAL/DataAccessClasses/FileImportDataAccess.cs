using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardIssuanceSystem.DAL.DataAccessClasses
{
    public class FileImportDataAccess
    {
        #region Insert
        public bool AddFileImport(tbl_File_Imports row)
        {

            try
            {
                using (var db = new SoneriCISEntities())
                {
                    db.tbl_File_Imports.Add(row);
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

        public List<tbl_File_Imports> GetFileImports()
        {
            try
            {
                List<tbl_File_Imports> lst = new List<DAL.tbl_File_Imports>();
                using (var db = new SoneriCISEntities())
                {
                    lst = db.tbl_File_Imports.ToList();
                }
                return lst;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public tbl_File_Imports GetFileImportById(int FileImportID)
        {
            try
            {
                tbl_File_Imports imports = new DAL.tbl_File_Imports();
                using (var db = new SoneriCISEntities())
                {
                    imports = db.tbl_File_Imports.Where(a => a.ID == FileImportID).FirstOrDefault();
                }
                return imports;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public DataTable GetFileImports(string accountNo, string cardNo, string branchCode, int? CardTypeId, DateTime? productionDate, DateTime? ImportDate)
        {
            try
            {
                int CT = CardTypeId ?? -1;
                string ProdDate = Convert.ToString(productionDate);
                string ImpDate = Convert.ToString(ImportDate);
                var builder = new StringBuilder();

                builder.Append("exec sp_Reports_CardImport ");
                builder.Append("'" + accountNo + "','" + cardNo + "','" + branchCode + "'," + CT.ToString() + ",");
                builder.Append("'" + ProdDate + "','" + ImpDate + "'");

                return DatabaseGateway.GetDataUsingStoredProcedureQuery(builder.ToString());
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion
    }
}
