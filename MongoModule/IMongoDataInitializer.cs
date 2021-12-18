namespace MongoModule
{
    public interface IMongoDataInitializer<ValueT>
    {
        public static abstract void Initialize(ref ValueT Value);
    }
}