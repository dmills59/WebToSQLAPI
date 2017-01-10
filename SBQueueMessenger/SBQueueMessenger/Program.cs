using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;

namespace SBQueueMessenger
{
    class Program
    {
        static void Main(string[] args)
        {
            var connectionString = "Endpoint=sb://devpocservicebus.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=e9rMeMcXGbsgZ4dX1oMSvwc6YLqnZAemJnKgj52l/mo=";
            var queueName = "devpoctodologicappqueue";
            var client = QueueClient.CreateFromConnectionString(connectionString, queueName);
 
            //Send
            var message = new BrokeredMessage("This is a test message from a Service Bus Message Queue!");
            client.Send(message);

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
