using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Diagnostics;
using System;

namespace DBChangesDataAPI.SQL
{
    public class SQLTextQuery
    {
        private readonly string _sqlDatabaseConnectionString;

        public SQLTextQuery(string sqlDatabaseConnectionString)
        {
            _sqlDatabaseConnectionString = sqlDatabaseConnectionString;
        }

        public bool RunSqlCommand(string query)
        {
            var command = PrepareSqlCommand(query);

            return PerformNonQuery(command);
        }
        public IEnumerable<Dictionary<string, object>> PerformQuery(string query)
        {
            var command = PrepareSqlCommand(query);
            return PerformQuery(command);
        }

        private IEnumerable<Dictionary<string, object>> PerformQuery(SqlCommand commandToRun)
        {
            IEnumerable<Dictionary<string, object>> result = null;
            StringBuilder errorMessages = new StringBuilder();
            try
            {
                using (var sqlConnection = new SqlConnection(_sqlDatabaseConnectionString))
                {
                    sqlConnection.Open();

                    commandToRun.Connection = sqlConnection;
                    using (SqlDataReader r = commandToRun.ExecuteReader())//not sure sqldatareader is best choice
                    {
                        int recordsaffected = r.RecordsAffected;
                        result = Serialize(r);
                    }

                    sqlConnection.Close();
                }
            }
            catch (SqlException odbcEx)
            {
                for (int i = 0; i < odbcEx.Errors.Count; i++)
                {
                    errorMessages.Append("Index #" + i + "\n" +
                        "Message: " + odbcEx.Errors[i].Message + "\n" +
                        "Error Number: " + odbcEx.Errors[i].Number + "\n" +
                        "LineNumber: " + odbcEx.Errors[i].LineNumber + "\n" +
                        "Source: " + odbcEx.Errors[i].Source + "\n" +
                        "Procedure: " + odbcEx.Errors[i].Procedure + "\n");
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Unknown Exception Thrown. Type:   " + ex.GetType().Name + "Message:   " + ex.Message);
            }
            return result;
        }

        private bool PerformNonQuery(SqlCommand commandToRun)
        {
            try
            {
                using (var sqlConnection = new SqlConnection(_sqlDatabaseConnectionString))
                {
                    sqlConnection.Open();

                    commandToRun.Connection = sqlConnection;
                    commandToRun.ExecuteNonQuery();

                    sqlConnection.Close();
                    return true;
                }
            }
            catch (Exception ex)
            {
            }
            return false;
        }

        private SqlCommand PrepareSqlCommand(string query)
        {
            SqlCommand command = new SqlCommand(query)
            {
                CommandType = CommandType.Text
            };
            return command;
        }
        public IEnumerable<Dictionary<string, object>> PerformQuery_new(string sqltext, List<KeyValuePair<string, string>> dbchanges)
        {
            SqlCommand command = new SqlCommand()
            {
                CommandType = CommandType.Text
                //CommandText = sqltext
            };
            command.CommandText = sqltext;
            //PrepareSqlCommand_new(command, new { a = 1, B = 2, c = 3 });
            Trace.TraceInformation("CommandText: " + sqltext + "DBChanges" + dbchanges[0].Key + dbchanges[0].Value + "," + dbchanges[1].Key + dbchanges[1].Value);
            PrepareSqlCommand_new(command, dbchanges);
            return PerformQuery(command);
        }
        //TODO: Implement Extension Method on AddInputParameters
        //http://stackoverflow.com/questions/5495416/using-sqlcommand-how-to-add-multiple-parameters-to-its-object-insertion-via
        //private static void PrepareSqlCommand_new<T>(SqlCommand command,T parameters) where T : class
        private static void PrepareSqlCommand_new(SqlCommand command, List<KeyValuePair<string, string>> parameters)
        {
            //SqlCommand command = new SqlCommand();
            if (parameters != null)
            {
                foreach (KeyValuePair<string, string> Item in parameters)
                {
                    object val = Item.Value;
                    var p = command.CreateParameter();
                    p.ParameterName = Item.Key.ToString();
                    p.Value = val ?? DBNull.Value;
                    command.Parameters.Add(p);
                }

                /*foreach (var prop in parameters.GetType().GetProperties())
                {
                    object val = prop.GetValue(parameters, null);
                    var p = command.CreateParameter();
                    p.ParameterName = prop.Name;
                    p.Value = val ?? DBNull.Value;
                    command.Parameters.Add(p);
                }*/
            }
        }
        private IEnumerable<Dictionary<string, object>> Serialize(SqlDataReader reader)
        {
            var results = new List<Dictionary<string, object>>();
            while (reader.Read())
            {
                var row = new Dictionary<string, object>();

                for (int i = 0; i < reader.FieldCount; i++)
                {
                    row.Add(reader.GetName(i), reader.GetValue(i));
                }
                results.Add(row);
            }
            return results;
        }
    }
}