using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Grpc.Core;
using Pubsub;

namespace gRPCTest.Server
{
    class Program
    {
        static void Main(string[] args)
        {
            const int port = 50052;
            var pubsubImp = new PubSubImpl();
            Grpc.Core.Server server = new Grpc.Core.Server
            {
                Services = { PubSub.BindService(pubsubImp) },
                Ports = { new ServerPort("localhost", port, ServerCredentials.Insecure) }
            };

            server.Start();

            Console.WriteLine("RouteGuide server listening on port " + port);
            Console.WriteLine("Insert event. 'q' to quit.");
            string input;
            while ((input = Console.ReadLine()) != "q")
            {
                pubsubImp.Publish(input);
            }

            Console.WriteLine("Press any key to stop the server...");
            Console.ReadKey();

            server.ShutdownAsync().Wait();
        }
    }

    public class PubSubImpl : PubSub.PubSubBase
    {
        private readonly BufferBlock<Event> _buffer = new BufferBlock<Event>();

        private Dictionary<string, IServerStreamWriter<Event>> _subscriberWritersMap =
            new Dictionary<string, IServerStreamWriter<Event>>();

         public override async Task Subscribe(Subscription subscription, IServerStreamWriter<Event> responseStream, ServerCallContext context)
        {
            _subscriberWritersMap[subscription.Id] = responseStream;

            while (_subscriberWritersMap.ContainsKey(subscription.Id))
            {
                var @event = await _buffer.ReceiveAsync();
                foreach (var serverStreamWriter in _subscriberWritersMap.Values)
                {
                    await serverStreamWriter.WriteAsync(@event);
                }
            }
        }

        public override Task<Unsubscription> Unsubscribe(Subscription request, ServerCallContext context)
        {
            _subscriberWritersMap.Remove(request.Id);
            return Task.FromResult(new Unsubscription() { Id = request.Id });
        }

        public void Publish(string input)
        {
            _buffer.Post(new Event() { Value = input });
        }
    }
}
