using MongoDB.Bson.Serialization;

namespace MongoModule
{
    public interface IMongoSerializationPhase<ValueT>
    {
        public static abstract void Serialize(ref ValueT Value, BsonSerializationContext Context, BsonSerializationArgs Args);
            
        public static abstract void Deserialize(ref ValueT Value, BsonDeserializationContext Context, BsonDeserializationArgs Args);
    }
}