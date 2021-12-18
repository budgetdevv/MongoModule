using System;
using System.Collections.Generic;
using System.Net;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;

namespace MongoModule
{
    public static class MongoModule<KeyT, ValueT, SerializationPhaseT, CollectionConfigT>
        where KeyT: unmanaged
        where SerializationPhaseT: IMongoSerializationPhase<KeyT, ValueT>
        where CollectionConfigT: IMongoCollectionConfig
    {
        public struct MongoDataContainer
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
            public MongoDataContainer(KeyT key, ValueT value)
            {
                Key = key;
                
                Value = value;
            }
        
            public KeyT Key;
        
            public ValueT Value;
        }
        
        private sealed class Serializer: SerializerBase<MongoDataContainer>
        {
            public override void Serialize(BsonSerializationContext Context, BsonSerializationArgs Args, MongoDataContainer Container)
            {
                var Writer = Context.Writer;
                
                Writer.WriteStartDocument();

                SerializationPhaseT.Serialize(ref Container.Value, Context, Args);
                
                Writer.WriteEndDocument();
            }
        
            public override MongoDataContainer Deserialize(BsonDeserializationContext Context, BsonDeserializationArgs Args)
            {
                var Reader = Context.Reader;
                
                Reader.ReadStartDocument();
                
                Unsafe.SkipInit(out KeyT Key);

                Unsafe.SkipInit(out ValueT Value);
                
                SerializationPhaseT.Deserialize_GetKey(ref Key, Context, Args);

                SerializationPhaseT.Deserialize_GetValue(ref Value, Context, Args);
                
                Reader.ReadEndDocument();
                
                return new MongoDataContainer(Key, Value);
            }
        }
        
        private static readonly IMongoCollection<MongoDataContainer> Collection;
        
        // ReSharper disable once StaticMemberInGenericType
        private static readonly bool UseInMemoryDB;
        
        private static readonly Dictionary<KeyT, ValueT> Cache;

        static MongoModule()
        {
            BsonSerializer.RegisterSerializer(typeof(MongoDataContainer), new Serializer());

            UseInMemoryDB = CollectionConfigT.IsInMemoryDB();
            
            Collection = ConnectionPool.CreateOrGetConnection(CollectionConfigT.GetConnectionString())
                .GetDatabase(CollectionConfigT.GetDBName())
                .GetCollection<MongoDataContainer>(CollectionConfigT.GetCollectionName());

            if (UseInMemoryDB)
            {
                Cache = new Dictionary<KeyT, ValueT>(unchecked((int) Collection.EstimatedDocumentCount()));
        
                foreach (var Item in Collection.FindSync(FilterDefinition<MongoDataContainer>.Empty).ToEnumerable())
                {
                    Cache[Item.Key] = Item.Value;
                }
            }
        
            else
            {
                Cache = new Dictionary<KeyT, ValueT>(CollectionConfigT.GetInitialCacheSize());
            }
        }

        private struct NoInitializer: IMongoDataInitializer<ValueT>
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
            public static void Initialize(ref ValueT Value)
            {
                //JIT should optimize this away
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static ref ValueT GetOrCreateItemRef(KeyT Key)
        {
            return ref GetOrCreateItemRef<NoInitializer>(Key);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static ref ValueT GetOrCreateItemRef<InitializerT>(KeyT Key)
            where InitializerT: IMongoDataInitializer<ValueT>
        {
            ref var Slot = ref MemoryMarshal.GetReference(
                MemoryMarshal.CreateSpan(ref CollectionsMarshal.GetValueRefOrAddDefault(Cache, Key, out var Exists), 1));
        
            if (Exists)
            {
                return ref Slot;
            }
        
            return ref LoadOrCreateItemRef<InitializerT>(Key, ref Slot);
        }
        
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static ref ValueT LoadOrCreateItemRef<InitializerT>(KeyT Key, ref ValueT Slot)
            where InitializerT: IMongoDataInitializer<ValueT>
        {
            var Items = Collection.Find(new FilterDefinitionBuilder<MongoDataContainer>().Eq("_id", Key)).ToEnumerable();

            foreach (var Item in Items)
            {
                Slot = Item.Value;

                goto End;
            }

            InitializerT.Initialize(ref Slot);
            
            End:
            return ref Slot;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static void UpdateItem(KeyT Key, ref ValueT Item)
        {
            var Container = new MongoDataContainer(Key, Item);

            Collection.ReplaceOne(new FilterDefinitionBuilder<MongoDataContainer>().Eq("_id", Key), Container, MongoHelpers.ReplaceOpts);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)] //Aggressive opt will actually cause the free branch opti to fail
        public static bool ContainsItemOfKey(KeyT Key)
        {
            if (UseInMemoryDB) //Free branch
            {
                return Cache.ContainsKey(Key);
            }
        
            else
            {
                return Cache.ContainsKey(Key) || Collection.Find(new FilterDefinitionBuilder<MongoDataContainer>().Eq("_id", Key)).Any();
            }
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static void DeleteItemOfKey(KeyT Key)
        {
            Cache.Remove(Key);
        
            Collection.DeleteOneAsync(new FilterDefinitionBuilder<MongoDataContainer>().Eq("_id", Key));
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static Dictionary<KeyT, ValueT> UnsafeGetCache()
        {
            return Cache;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)] //Aggressive opt will actually cause the free branch opti to fail
        public static void UnsafeEvictFromCache(KeyT Key)
        {
            if (UseInMemoryDB) //Free branch
            {
                throw new Exception("Cannot evict cache for InMemoryDB!");
            }
            
            Cache.Remove(Key);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static IMongoCollection<MongoDataContainer> UnsafeGetMongoCollection()
        {
            return Collection;
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static void EnsureInitialized()
        {
            //JITter should NOT optimize this method away
        }
    }
}