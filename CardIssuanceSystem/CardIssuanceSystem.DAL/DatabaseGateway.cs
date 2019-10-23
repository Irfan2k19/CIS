using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardIssuanceSystem.DAL
{
    public static class DatabaseGateway
    {
        public static string CONNECTION_STRING = string.Empty;//ConfigurationManager.ConnectionStrings        ["TSADB"].ConnectionString;
        static DatabaseGateway()
        {
            CONNECTION_STRING = GetConnectionString;
        }
        static string GetConnectionString
        {
            get
            {
                using (var db = new SoneriCISEntities())
                {
                    return db.Database.Connection.ConnectionString;
                }
            }
        }

        /// <summary>
        /// Identify sql custom/user defined exceptions
        /// </summary>
        /// <param name="exception"></param>
        private static void ThrowSQLException(SqlException sqlException)
        {
            // If sql custom/user defined exception
            if (sqlException.Number == 50000)
                throw new Exception(sqlException.Message);
            else
                throw sqlException;
        }

        /// <summary>
        /// Gets the Data (DataTable) using the specific Stored Procedure.
        /// </summary>
        /// <param name="procedure">Stored Procedure Name</param>
        /// <param name="parameters">Parameters (if any)</param>
        /// <returns>Data Table as a result of SP execution.</returns>
        public static DataTable GetDataUsingStoredProcedure(string procedure, List<SqlParameter> parameters)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(CONNECTION_STRING))
                {
                    conn.Open();
                    using (SqlCommand command = new SqlCommand(procedure, conn))
                    {
                        command.CommandTimeout = 30;
                        command.CommandType = CommandType.StoredProcedure;

                        if (parameters != null)
                        {
                            foreach (SqlParameter parameter in parameters)
                            {
                                command.Parameters.Add(parameter);
                            }
                        }

                        var reader = command.ExecuteReader();
                        DataTable dt = new DataTable();
                        if (reader.HasRows)
                        {
                            dt.Load(reader);
                        }
                        conn.Close();
                        return dt;
                    }
                }
            }
            catch (SqlException sqlException)
            {
                ThrowSQLException(sqlException);
            }

            return null;
        }

        /// <summary>
        /// Gets the Data (DataTable) using the specific Stored Procedure.
        /// </summary>
        /// <param name="procedure">Stored Procedure Name</param>
        /// <param name="parameters">Parameters (if any)</param>
        /// <returns>Data Table as a result of SP execution.</returns>
        public static DataTable GetDataUsingStoredProcedureQuery(string query)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(CONNECTION_STRING))
                {
                    conn.Open();
                    using (SqlCommand command = new SqlCommand(query, conn))
                    {
                        command.CommandTimeout = 660;

                        var reader = command.ExecuteReader();
                        DataTable dt = new DataTable();
                        if (reader.HasRows)
                        {
                            dt.Load(reader);
                        }
                        conn.Close();
                        return dt;
                    }
                }
            }
            catch (SqlException sqlException)
            {
                ThrowSQLException(sqlException);
            }

            return null;
        }
        /// <summary>
        /// Gets the data using the stored procedure and parameters provided
        /// </summary>
        /// <param name="procedure"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static DataSet GetDataSetUsingStoredProcedure(string procedure, List<SqlParameter> parameters)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(CONNECTION_STRING))
                {
                    conn.Open();
                    using (SqlCommand command = new SqlCommand(procedure, conn))
                    {
                        command.CommandTimeout = 30;
                        command.CommandType = CommandType.StoredProcedure;

                        if (parameters != null)
                        {
                            foreach (SqlParameter parameter in parameters)
                            {
                                command.Parameters.Add(parameter);
                            }
                        }

                        using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                        {
                            DataSet dataset = new DataSet();
                            adapter.Fill(dataset);
                            return dataset;
                        }
                    }
                }
            }
            catch (SqlException sqlException)
            {
                ThrowSQLException(sqlException);
            }

            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="procedure"></param>
        /// <param name="parameters"></param>
        /// <param name="conn"></param>
        /// <param name="sqlTrans"></param>
        /// <returns></returns>
        public static DataSet GetDataSetUsingStoredProcedure(string procedure, List<SqlParameter> parameters, SqlConnection conn, SqlTransaction sqlTrans)
        {
            try
            {
                using (SqlCommand command = new SqlCommand(procedure, conn))
                {
                    command.CommandTimeout = 30;
                    command.CommandType = CommandType.StoredProcedure;
                    command.Transaction = sqlTrans;

                    if (parameters != null)
                    {
                        foreach (SqlParameter parameter in parameters)
                        {
                            command.Parameters.Add(parameter);
                        }
                    }

                    using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                    {
                        DataSet dataset = new DataSet();
                        adapter.Fill(dataset);
                        return dataset;
                    }
                }
            }
            catch (SqlException sqlException)
            {
                ThrowSQLException(sqlException);
            }

            return null;
        }

        /// <summary>
        /// Get scalar data using stored procedure and parameters provided. Casting should be done by the caller.
        /// </summary>
        /// <param name="procedure">Stored Procedure name</param>
        /// <param name="parameters">Stored Procedure parameters</param>
        /// <returns>An object of generic type. </returns>
        public static object GetScalarDataUsingStoredProcedure(string procedure, List<SqlParameter> parameters)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(CONNECTION_STRING))
                {
                    conn.Open();
                    using (SqlCommand command = new SqlCommand(procedure, conn))
                    {
                        command.CommandTimeout = 30;
                        command.CommandType = CommandType.StoredProcedure;

                        if (parameters != null)
                        {
                            foreach (SqlParameter parameter in parameters)
                            {
                                command.Parameters.Add(parameter);
                            }
                        }

                        var result = command.ExecuteScalar();

                        return result;
                    }
                }
            }
            catch (SqlException sqlException)
            {
                ThrowSQLException(sqlException);
            }

            return null;
        }

        /// <summary>
        /// Execute stored procedure with given sql parameters
        /// </summary>
        /// <param name="procedure"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static void ExecuteStoredProcedure(string procedure, List<SqlParameter> parameters)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(CONNECTION_STRING))
                {
                    conn.Open();
                    using (SqlCommand command = new SqlCommand(procedure, conn))
                    {
                        command.CommandTimeout = 30;
                        command.CommandType = CommandType.StoredProcedure;

                        foreach (SqlParameter parameter in parameters)
                        {
                            command.Parameters.Add(parameter);
                        }
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (SqlException sqlException)
            {
                ThrowSQLException(sqlException);
            }
        }
    }
}
