using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;

namespace MongoModule
{
    public interface IMongoModule<ThisT, KeyT, ValueT, SerializationPhaseT>
        where ThisT: IMongoModule<ThisT, KeyT, ValueT, SerializationPhaseT>     
        where KeyT: unmanaged
        where SerializationPhaseT: IMongoSerializationPhase<ValueT>
    {
        public static readonly EqualityComparer<KeyT> KeyEC = EqualityComparer<KeyT>.Default;
        
        private struct MongoDataContainer
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
            public MongoDataContainer(KeyT key, ValueT value)
            {
                Key = key;
                
                Value = value;
            }
        
            public KeyT Key;
        
            public ValueT Value;
        
            [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
            public bool KeyIs(KeyT InputKey)
            {
                return KeyEC.Equals(Key, InputKey);
            }
        }
        
        private sealed class Serializer: SerializerBase<MongoDataContainer>
        {
            public override void Serialize(BsonSerializationContext Context, BsonSerializationArgs Args, MongoDataContainer Container)
            {
                var Writer = Context.Writer;
                
                Writer.WriteStartDocument();
                
                Writer.WriteName("_id");
                
                Writer.WriteBytes(Container.Key.ToBson());
                
                SerializationPhaseT.Serialize(ref Container.Value, Context, Args);
                
                Writer.WriteEndDocument();
            }
        
            public override MongoDataContainer Deserialize(BsonDeserializationContext Context, BsonDeserializationArgs Args)
            {
                var Reader = Context.Reader;
                
                Reader.ReadStartDocument();
                
                var ID = Reader.ReadObjectId();
        
                Unsafe.SkipInit(out ValueT Value);
                
                SerializationPhaseT.Deserialize(ref Value, Context, Args);
                
                Reader.ReadEndDocument();
                
                return new MongoDataContainer(Unsafe.As<ObjectId, KeyT>(ref ID), Value);
                
                // ref var Slot = ref CollectionsMarshal.GetValueRefOrAddDefault(Cache, Unsafe.As<ObjectId, KeyT>(ref ID), out _);
            }
        }
        
        private static readonly IMongoCollection<MongoDataContainer> Collection;
        
        // ReSharper disable once StaticMemberInGenericType
        private static readonly bool UseInMemoryDB;
        
        private static readonly Dictionary<KeyT, ValueT> Cache;

        // static unsafe IMongoModule()
        // {
        //     Console.WriteLine("Test");
        //
        //     var Type = typeof(ThisT);
        //
        //     var Flags = (BindingFlags) (-1);
        //     
        //     var IsInMemoryDBMethod = (delegate*<bool>) Type.GetMethod("IsInMemoryDB", Flags)!.MethodHandle.GetFunctionPointer();
        //     
        //     var GetConnectionStringMethod = (delegate*<string>) Type.GetMethod("GetConnectionString", Flags)!.MethodHandle.GetFunctionPointer();
        //     
        //     var GetDBNameMethod = (delegate*<string>) Type.GetMethod("GetDBName", Flags)!.MethodHandle.GetFunctionPointer();
        //     
        //     var GetCollectionNameMethod = (delegate*<string>) Type.GetMethod("GetCollectionName", Flags)!.MethodHandle.GetFunctionPointer();
        //     
        //     UseInMemoryDB = IsInMemoryDBMethod();
        //
        //     Collection = ConnectionPool.CreateOrGetConnection(GetConnectionStringMethod())
        //         .GetDatabase(GetDBNameMethod())
        //         .GetCollection<MongoDataContainer>(GetCollectionNameMethod());
        //     
        //     // UseInMemoryDB = ThisT.IsInMemoryDB();
        //     //
        //     // Collection = ConnectionPool.CreateOrGetConnection(ThisT.GetConnectionString())
        //     //     .GetDatabase(ThisT.GetDBName())
        //     //     .GetCollection<MongoDataContainer>(ThisT.GetCollectionName());
        //
        //     if (UseInMemoryDB)
        //     {
        //         Cache = new Dictionary<KeyT, ValueT>(unchecked((int) Collection.EstimatedDocumentCount()));
        //
        //         foreach (var Item in Collection.FindSync(FilterDefinition<MongoDataContainer>.Empty).ToEnumerable())
        //         {
        //             Cache[Item.Key] = Item.Value;
        //         }
        //     }
        //
        //     else
        //     {
        //         Cache = new Dictionary<KeyT, ValueT>(ThisT.GetInitialCacheSize());
        //     }
        //     
        //     // UseInMemoryDB = MongoHelpers.IsInMemoryDB<ThisT, KeyT, ValueT, SerializationPhaseT>();
        //     //
        //     // Collection = ConnectionPool.CreateOrGetConnection(MongoHelpers.GetConnectionString<ThisT, KeyT, ValueT, SerializationPhaseT>())
        //     //     .GetDatabase(MongoHelpers.GetDBName<ThisT, KeyT, ValueT, SerializationPhaseT>())
        //     //     .GetCollection<MongoDataContainer>(MongoHelpers.GetCollectionName<ThisT, KeyT, ValueT, SerializationPhaseT>());
        //     //
        //     // if (UseInMemoryDB)
        //     // {
        //     //     Cache = new Dictionary<KeyT, ValueT>(unchecked((int) Collection.EstimatedDocumentCount()));
        //     //
        //     //     foreach (var Item in Collection.FindSync(FilterDefinition<MongoDataContainer>.Empty).ToEnumerable())
        //     //     {
        //     //         Cache[Item.Key] = Item.Value;
        //     //     }
        //     // }
        //     //
        //     // else
        //     // {
        //     //     Cache = new Dictionary<KeyT, ValueT>(MongoHelpers.GetInitialCacheSize<ThisT, KeyT, ValueT, SerializationPhaseT>());
        //     // }
        // }

        public static abstract string GetConnectionString();
        
        public static abstract string GetDBName();
        
        public static abstract string GetCollectionName();
        
        public static abstract bool IsInMemoryDB();
        
        public static abstract int GetInitialCacheSize();

        // [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        // public static ref ValueT GetOrCreateItemRef(KeyT Key)
        // {
        //     ref var Slot = ref MemoryMarshal.GetReference(
        //         MemoryMarshal.CreateSpan(ref CollectionsMarshal.GetValueRefOrAddDefault(Cache, Key, out var Exists), 1));
        //
        //     if (Exists)
        //     {
        //         return ref Slot;
        //     }
        //
        //     return ref LoadOrCreateItemRef(Key, ref Slot);
        // }
        //
        // [MethodImpl(MethodImplOptions.NoInlining)]
        // private static ref ValueT LoadOrCreateItemRef(KeyT Key, ref ValueT Slot)
        // {
        //     //var Filter = new FilterDefinitionBuilder<MongoDataContainer>().
        //
        //     var Item = Collection.Find(x => x.KeyIs(Key)).SingleOrDefault();
        //     
        //     Slot = Item.Value;
        //     
        //     return ref Slot;
        // }
        //
        // [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        // public static void UpdateItem(KeyT Key, ref ValueT Item)
        // {
        //     var Container = new MongoDataContainer(Key, Item);
        //
        //     Collection.ReplaceOne(x => x.KeyIs(Key), Container, MongoHelpers.ReplaceOpts);
        // }
        //
        // [MethodImpl(MethodImplOptions.AggressiveInlining)] //Aggressive opt will actually cause the free branch opti to fail
        // public static bool ContainsItemOfKey(KeyT Key)
        // {
        //     if (UseInMemoryDB) //Free branch
        //     {
        //         return Cache.ContainsKey(Key);
        //     }
        //
        //     else
        //     {
        //         return Cache.ContainsKey(Key) || Collection.Find(x => x.KeyIs(Key)).Any();
        //     }
        // }
        //
        // [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        // public static void DeleteItemOfKey(KeyT Key)
        // {
        //     Cache.Remove(Key);
        //
        //     Collection.DeleteOneAsync(x => x.KeyIs(Key));
        // }
        //
        // [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        // public static Dictionary<KeyT, ValueT> GetCache()
        // {
        //     return Cache;
        // }
    }
}