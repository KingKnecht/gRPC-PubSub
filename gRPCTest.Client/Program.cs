using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grpc.Core;
using Pubsub;

namespace gRPCTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var channel = new Channel("127.0.0.1:50052", ChannelCredentials.Insecure);
            var subscriber = new Subsriber(new PubSub.PubSubClient(channel));

            Task.Run(async () =>
            {
                await subscriber.Subscribe();
            }).GetAwaiter();

            Console.WriteLine("Hit key to unsubscribe");
            Console.ReadLine();

            subscriber.Unsubscribe();

            Console.WriteLine("Unsubscribed...");

            Console.WriteLine("Hit key to exit...");
            Console.ReadLine();
            
        }

        public class Subsriber
        {
            private readonly PubSub.PubSubClient _pubSubClient;
            private Subscription _subscription;

            public Subsriber(PubSub.PubSubClient pubSubClient)
            {
                _pubSubClient = pubSubClient;
            }

            public async Task Subscribe()
            {
                _subscription = new Subscription() { Id = Guid.NewGuid().ToString() };
                using (var call = _pubSubClient.Subscribe(_subscription))
                {
                    //Receive
                    var responseReaderTask = Task.Run(async () =>
                    {
                        while (await call.ResponseStream.MoveNext())
                        {
                            Console.WriteLine("Event received: " + call.ResponseStream.Current);
                        }
                    });

                    await responseReaderTask;
                }
            }

            public void Unsubscribe()
            {
                _pubSubClient.Unsubscribe(_subscription);
            }
        }
    }
}
