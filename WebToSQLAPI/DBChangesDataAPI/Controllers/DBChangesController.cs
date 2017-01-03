using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using ToDoListDataAPI.Models;
using Microsoft.Azure;
using Newtonsoft.Json;

namespace DBChangesDataAPI.Controllers
{
    public class ToDoListController : ApiController
    {
        // Uncomment following lines for service principal authentication
        //private static string trustedCallerClientId = ConfigurationManager.AppSettings["todo:TrustedCallerClientId"];
        //private static string trustedCallerServicePrincipalId = ConfigurationManager.AppSettings["todo:TrustedCallerServicePrincipalId"];

        private static Dictionary<int, ToDoItem> mockData = new Dictionary<int, ToDoItem>();
        private static Dictionary<int, ToDoItem> sqlData = new Dictionary<int, ToDoItem>();
        private static string sqlDatabaseConnectionString = CloudConfigurationManager.GetSetting("sqlDatabaseConnectionString");
        static ToDoListController()
        {
            //mockData.Add(0, new ToDoItem { ID = 0, Owner = "*", Description = "feed the dog" });
            //mockData.Add(1, new ToDoItem { ID = 1, Owner = "*", Description = "take the dog on a walk" });
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
        public IEnumerable<ToDoItem> Get(string owner)
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

                    sqlData.Add(i++, new ToDoItem { ID = (int)resultGroup["ItemId"], Owner = (string)resultGroup["Owner"], Description = (string)resultGroup["Description"] });
                }
            }
            return sqlData.Values;
            // return mockData.Values.Where(m => m.Owner == owner || owner == "*"); defunct
        }

        // GET: api/ToDoItemList/5
        public ToDoItem GetById(string owner, int id)
        {
            CheckCallerId();

            return mockData.Values.Where(m => (m.Owner == owner || owner == "*") && m.ID == id).First();
            // Insert calls to SQLTextQuery to retreive data from SQL
        }

        // POST: api/ToDoItemList
        public void Post(ToDoItem todo)
        {
            CheckCallerId();
            //We're using SQL CommandText Parameter.Add which needs a specially formatted list of fields for value substitution

            string datafields = "@";
            foreach (var fieldname in todo.DBchanges)
            {
                datafields += fieldname.Key + ",@";
            }
            string insertDataQuery = "insert into dbo.todoitems values(" + datafields.TrimEnd(new char[] { ',', '@' }) + ")";
            SQL.SQLTextQuery queryPerformer = new SQL.SQLTextQuery(sqlDatabaseConnectionString);
            IEnumerable<Dictionary<string, object>> resultCollection = queryPerformer.PerformQuery_new(insertDataQuery, todo.DBchanges);

            if (resultCollection != null && resultCollection.Any())
            {
                int i = 0;
                foreach (var resultGroup in resultCollection)
                {
                    sqlData.Add(i++, new ToDoItem { ID = (int)resultGroup["ItemId"], Owner = (string)resultGroup["Owner"], Description = (string)resultGroup["Description"] });
                }
            }
        }

        public void Put(ToDoItem todo)
        {
            CheckCallerId();
            if (todo != null)
            {
                string datafields = "";
                foreach (var fieldname in todo.DBchanges)
                {
                    datafields += fieldname.Key + "=@" + fieldname.Key + ",";
                }
                //string updateDataQuery = " update todoitems set owner=@Owner,Description=@Description where ItemID=" + todo.ID;
                string updateDataQuery = "update todoitems set " + datafields.TrimEnd(new char[] { ',' }) + " where ItemID=" + todo.ID; //TODO change the static ItemID to a @ parameter
                SQL.SQLTextQuery queryPerformer = new SQL.SQLTextQuery(sqlDatabaseConnectionString);
                IEnumerable<Dictionary<string, object>> resultCollection = queryPerformer.PerformQuery_new(updateDataQuery, todo.DBchanges);
            }
        }

        // DELETE: api/ToDoItemList/5
        public void Delete(string owner, int id)
        {
            CheckCallerId();
            //string updateDataQuery = "DELETE FROM todoitems WHERE owner = N'" + owner + "' AND ItemID=N'" + id + "'";
            string updateDataQuery = "DELETE FROM todoitems WHERE owner = '" + owner + "' AND ItemID='" + id + "'";
            SQL.SQLTextQuery queryPerformer = new SQL.SQLTextQuery(sqlDatabaseConnectionString);
            IEnumerable<Dictionary<string, object>> resultCollection = queryPerformer.PerformQuery_new(updateDataQuery, null);
        }
    }
}
