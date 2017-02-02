using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using DBChangesDataAPI.Models;
using Microsoft.Azure;
using System.Diagnostics;
using System;
using Newtonsoft.Json;

namespace DBChangesDataAPI.Controllers
{
    public class DBChangesController : ApiController
    {
        // Uncomment following lines for service principal authentication
        //private static string trustedCallerClientId = ConfigurationManager.AppSettings["todo:TrustedCallerClientId"];
        //private static string trustedCallerServicePrincipalId = ConfigurationManager.AppSettings["todo:TrustedCallerServicePrincipalId"];

        private static Dictionary<int, DBChanges> sqlData = new Dictionary<int, DBChanges>();
        private static string sqlDatabaseConnectionString = CloudConfigurationManager.GetSetting("sqlDatabaseConnectionString");
        static DBChangesController()
        {
        }

        private static void CheckCallerId()
        {
            // Uncomment following lines for service principal authentication
            //string currentCallerClientId = ClaimsPrincipal.Current.FindFirst("appid").Value;
            //string currentCallerServicePrincipalId = ClaimsPrincipal.Current.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier").Value;
            //if (currentCallerClientId != trustedCallerClientId || currentCallerServicePrincipalId != trustedCallerServicePrincipalId)
            //{
            //    throw new HttpResponseException(new HttpResponseMessage { StatusCode = HttpStatusCode.Unauthorized, ReasonPhrase = "The appID or service principal ID is not the expected value." });
            //}
        }

        // GET: api/ToDoItemList
        public IEnumerable<DBChanges> Get(string owner)
        {
            CheckCallerId();

            string selectDataQuery = "SELECT * FROM ToDoItems WHERE Owner=N'" + owner + "' OR Owner=N'*'";
            SQL.SQLTextQuery queryPerformer = new SQL.SQLTextQuery(sqlDatabaseConnectionString);
            IEnumerable<Dictionary<string, object>> resultCollection = queryPerformer.PerformQuery_new(selectDataQuery, null);

            if (resultCollection.Any())
            {
                int i = 0;
                sqlData.Clear();
                foreach (var resultGroup in resultCollection)
                {
                    //string serialized = JsonConvert.SerializeObject(resultGroup);

                    sqlData.Add(i++, new DBChanges { ID = (int)resultGroup["ItemId"], Owner = (string)resultGroup["Owner"], Description = (string)resultGroup["Description"] });
                }
            }
            return sqlData.Values;
        }

        // POST: api/ToDoItemList
        public void Post(DBChanges todo)
        {
            CheckCallerId();
            //We're using SQL CommandText Parameter.Add which needs a specially formatted list of fields for value substitution

            string datafields = "@";
            Trace.TraceInformation("TODO= ID:" + todo.ID + ", key[0]:" + todo.DBchanges[0].Key + ", value[0]" + todo.DBchanges[0].Value);
            try
            {
                foreach (var fieldname in todo.DBchanges)
                {
                    datafields += fieldname.Key + ",@";
                }
                string insertDataQuery = "insert into dbo.todoitems values(" + datafields.TrimEnd(new char[] { ',', '@' }) + ")";
                Trace.TraceInformation("Insert Query: " + insertDataQuery);

                SQL.SQLTextQuery queryPerformer = new SQL.SQLTextQuery(sqlDatabaseConnectionString);
                IEnumerable<Dictionary<string, object>> resultCollection = queryPerformer.PerformQuery_new(insertDataQuery, todo.DBchanges);

                if (resultCollection != null && resultCollection.Any())
                {
                    int i = 0;
                    foreach (var resultGroup in resultCollection)
                    {
                        sqlData.Add(i++, new DBChanges { ID = (int)resultGroup["ItemId"], Owner = (string)resultGroup["Owner"], Description = (string)resultGroup["Description"] });
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError("Error occurred during POST:" + ex.ToString());
            }
        }

        public void Put(DBChanges todo)
        {
            CheckCallerId();
            if (todo != null)
            {
                string datafields = "";
                foreach (var fieldname in todo.DBchanges)
                {
                    datafields += fieldname.Key + "=@" + fieldname.Key + ",";
                }

                string updateDataQuery = "update todoitems set " + datafields.TrimEnd(new char[] { ',' }) + " where ItemID=" + todo.ID; //TODO change the static ItemID to a @ parameter
                SQL.SQLTextQuery queryPerformer = new SQL.SQLTextQuery(sqlDatabaseConnectionString);
                IEnumerable<Dictionary<string, object>> resultCollection = queryPerformer.PerformQuery_new(updateDataQuery, todo.DBchanges);
            }
        }

        // DELETE: api/ToDoItemList/5
        public void Delete(string owner, int id)
        {
            CheckCallerId();
            string updateDataQuery = "DELETE FROM todoitems WHERE owner = '" + owner + "' AND ItemID='" + id + "'";
            SQL.SQLTextQuery queryPerformer = new SQL.SQLTextQuery(sqlDatabaseConnectionString);
            IEnumerable<Dictionary<string, object>> resultCollection = queryPerformer.PerformQuery_new(updateDataQuery, null);
        }
    }
}
