using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;
using System.Runtime.Serialization;
using System.IO;
using Newtonsoft.Json;

namespace SBQueueMessenger
{
    [DataContract]
    public class DataTransaction
    {
        [DataMember]
        public int ItemID { get; set; }
        [DataMember]
        public string Description { get; set;}
        [DataMember]
        public string Owner { get; set; }
        //[DataMember]
        //public List<string> alist { get; set; }
        [DataMember]
        public List<KeyValuePair<string, string>> DBChanges { get; set; }
    }
    class Program
    {
        static void Main(string[] args)
        {
            var connectionString = "Endpoint=sb://devpocservicebus.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=e9rMeMcXGbsgZ4dX1oMSvwc6YLqnZAemJnKgj52l/mo=";
            var queueName = "devpoctodologicappqueue";
            var client = QueueClient.CreateFromConnectionString(connectionString, queueName);

            DataTransaction dtin = new DataTransaction()
            {
                ItemID = 101,
                Description = "Create",
                Owner = "Alan",
                //alist = new List<string> { "ID=122", "Owner = Alan", "Description=Shovel the sidewalk" },
                DBChanges = new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("ItemID","101"),
                    new KeyValuePair<string, string>("Description","Decript the message"),
                    new KeyValuePair<string,string>("Owner","Samuel")
                }
            };

            //"ContentData":"{\"ItemID\":101,\"Description\":\"Delete\",\"Owner\":\"Alan\",\"alist\":[\"ItemID=122\",\"Owner = Alan\",\"Description=Shovel the sidewalk\"]}"}
            //"ContentData":"{\"ItemID\":101,\"Description\":\"Delete\",\"Owner\":\"Alan\",\"alist\":[\"ID=122\",\"Owner = Alan\",\"Description=Shovel the sidewalk\"],\"DBChanges\":[{\"Key\":\"ItemID\",\"Value\":\"101\"},{\"Key\":\"Description\",\"Value\":\"Decript the message\"},{\"Key\":\"Owner\",\"Value\":\"Samuel\"}]}"}
            var json = JsonConvert.SerializeObject(dtin);
            var payloadstream = new MemoryStream(Encoding.UTF8.GetBytes(json));
            var msg = new BrokeredMessage(payloadstream,true);
            msg.ContentType = "application/json";
            client.Send(msg);

            //Send
            //String messagestr = "{\"ID\": 0, \"Description\": \"Create\",\"Owner\": \"Boss Hogs\",";
            //messagestr += "\"DBchanges\": [{\"key\": \"ItemID\",\"value\": \"999\"},{\"key\": \"Owner\", \"value\": \"Steve Wisnewski\"}]}";
            //var message = new BrokeredMessage(messagestr);
            //var message = new BrokeredMessage("Create");
            //var message = new BrokeredMessage(DateTime.Now.ToString() + ": This is a test message from a Service Bus Message Queue!");
            //client.Send(message);

            //Receive
            /*client.OnMessage(message =>
            {
                Console.WriteLine(String.Format("Message body: {0}", message.GetBody<String>()));
                Console.WriteLine(String.Format("Message id: {0}", message.MessageId));
            });

            Console.ReadLine();
    */
        }
    }
}
