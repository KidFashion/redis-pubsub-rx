﻿using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;

namespace Redis.PubSub.Reactive
{
    public class Observable : ObservableBase<RedisValue>, IDisposable 
    {

        private ConnectionMultiplexer _connection;
        private ISubscriber _subscription;

        SynchronizedCollection<IObserver<RedisValue>> _observerList = new SynchronizedCollection<IObserver<RedisValue>>();


        public Observable(string connectionString, string channelName)
            : this(ConnectionMultiplexer.Connect(connectionString),channelName)
        {
            Contract.Requires<ArgumentNullException>(String.IsNullOrEmpty(connectionString));
            Contract.Requires<ArgumentException>(!String.IsNullOrEmpty(channelName));
        }

        protected internal Observable(ConnectionMultiplexer multiplexer, string channelName)
        {
            Contract.Requires<ArgumentNullException>(multiplexer != null);
            Contract.Requires<ArgumentException>(!String.IsNullOrEmpty(channelName));

            _connection = multiplexer;

            _subscription = multiplexer.GetSubscriber();
            _subscription.Subscribe(channelName,
                (channel, value) => _observerList.ToList().ForEach(item => item.OnNext(value))
                );
        }


        protected override IDisposable SubscribeCore(IObserver<RedisValue> observer)
        {
            Contract.Ensures(_observerList != null);

            Contract.Assume(observer != null);

            _observerList.Add(observer);

            return System.Reactive.Disposables.Disposable.Create(() => { _observerList.Remove(observer); });
        }



        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {

            if (!disposed)
            {
                if (disposing)
                {
                    // Managed and unmanaged resources can be disposed.

                    _connection.Dispose();

                    _observerList.ToList().ForEach(item => item.OnCompleted());

                }

            }
            disposed = true;
        }

        private bool disposed = false;


    }
}
