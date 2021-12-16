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
    }
}