using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MbUnit.Framework;
using Redis.PubSub.Reactive;
using StackExchange.Redis;

namespace Redis.PubSub.Reactive.Tests
{
    [TestFixture]
    public class MainTest
    {
        [Test]
        public void ListenToPubSubChannel()        
        {
            ConnectionMultiplexer mp;

            var list = new List<String>();
            var result = new List<String>();

            mp = ConnectionMultiplexer.Connect("localhost");
            var sub = mp.GetSubscriber();

            var obs = new Redis.PubSub.Reactive.Observable("localhost", "MyChannel");

            obs.Subscribe((value) => list.Add(value));

            for (int i = 1; i < 100; i++)
            {
                sub.Publish("MyChannel", i);
                result.Add(i.ToString());
            }

            Assert.AreElementsEqual(list, result);


        }
    }
}
