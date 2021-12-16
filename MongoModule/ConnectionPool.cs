using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using MongoDB.Driver;

namespace MongoModule
{
    internal static class ConnectionPool
    {
        private static readonly Dictionary<string, MongoClient> Connections;

        static ConnectionPool()
        {
            Connections = new Dictionary<string, MongoClient>(10);

            Connections[""] = new MongoClient(); //Local
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MongoClient CreateOrGetConnection(string ConnectionString)
        {
            ref var Connection = ref CollectionsMarshal
                .GetValueRefOrAddDefault(Connections, ConnectionString, out var Exists);

            if (Exists)
            {
                return Connection;
            }

            return CreateConnection(ConnectionString, ref Connection);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static MongoClient CreateConnection(string ConnectionString, ref MongoClient Client)
        {
            return Client = new MongoClient(ConnectionString);
        }
    }
}