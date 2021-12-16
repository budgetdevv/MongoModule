using MongoDB.Driver;

namespace MongoModule
{
    internal static class MongoHelpers
    {
        public static readonly ReplaceOptions ReplaceOpts = new ReplaceOptions()
        {
            IsUpsert = true,
            BypassDocumentValidation = true
        };

        // public static string GetConnectionString<MongoModuleT, KeyT, ValueT, SerializationPhaseT>()
        //     where MongoModuleT: IMongoModule<MongoModuleT, KeyT, ValueT, SerializationPhaseT> 
        //     where KeyT : unmanaged 
        //     where SerializationPhaseT : IMongoSerializationPhase<ValueT>
        // {
        //     return MongoModuleT.GetConnectionString();
        // }
        //
        // public static string GetDBName<MongoModuleT, KeyT, ValueT, SerializationPhaseT>()
        //     where MongoModuleT: IMongoModule<MongoModuleT, KeyT, ValueT, SerializationPhaseT> 
        //     where KeyT : unmanaged 
        //     where SerializationPhaseT : IMongoSerializationPhase<ValueT>
        // {
        //     return MongoModuleT.GetDBName();
        // }
        //
        // public static string GetCollectionName<MongoModuleT, KeyT, ValueT, SerializationPhaseT>()
        //     where MongoModuleT: IMongoModule<MongoModuleT, KeyT, ValueT, SerializationPhaseT> 
        //     where KeyT : unmanaged 
        //     where SerializationPhaseT : IMongoSerializationPhase<ValueT>
        // {
        //     return MongoModuleT.GetCollectionName();
        // }
        //
        // public static bool IsInMemoryDB<MongoModuleT, KeyT, ValueT, SerializationPhaseT>()
        //     where MongoModuleT: IMongoModule<MongoModuleT, KeyT, ValueT, SerializationPhaseT> 
        //     where KeyT : unmanaged 
        //     where SerializationPhaseT : IMongoSerializationPhase<ValueT>
        // {
        //     return MongoModuleT.IsInMemoryDB();
        // }
        //
        // public static int GetInitialCacheSize<MongoModuleT, KeyT, ValueT, SerializationPhaseT>()
        //     where MongoModuleT: IMongoModule<MongoModuleT, KeyT, ValueT, SerializationPhaseT> 
        //     where KeyT : unmanaged 
        //     where SerializationPhaseT : IMongoSerializationPhase<ValueT>
        // {
        //     return MongoModuleT.GetInitialCacheSize();
        // }
    }
}