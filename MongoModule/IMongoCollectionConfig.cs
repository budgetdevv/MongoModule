namespace MongoModule
{
    public interface IMongoCollectionConfig
    {
        public static abstract string GetConnectionString();
        
        public static abstract string GetDBName();
        
        public static abstract string GetCollectionName();
        
        public static abstract bool IsInMemoryDB();
        
        public static abstract int GetInitialCacheSize();
    }
}