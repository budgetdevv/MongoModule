using MongoDB.Bson.Serialization;

namespace MongoModule
{
    public interface IMongoSerializationPhase<KeyT, ValueT>
    {
        public static abstract void Serialize(ref ValueT Value, BsonSerializationContext Context, BsonSerializationArgs Args);
            
        public static abstract void Deserialize_GetKey(ref KeyT Value, BsonDeserializationContext Context, BsonDeserializationArgs Args);
        
        public static abstract void Deserialize_GetValue(ref ValueT Value, BsonDeserializationContext Context, BsonDeserializationArgs Args);
    }
}