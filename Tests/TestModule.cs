using System;
using System.Runtime.CompilerServices;
using MongoDB.Bson.Serialization;
using MongoModule;

namespace Tests
{
    public struct TestSerializationPhase: IMongoSerializationPhase<ulong, Foo>
    {
        public static void Serialize(ref Foo Value, BsonSerializationContext Context, BsonSerializationArgs Args)
        {
            var Writer = Context.Writer;

            Writer.WriteName("String");
            
            Writer.WriteString(Value.String ?? "");

            Writer.WriteName("Int");
            
            Writer.WriteInt32(Value.Int);
        }

        public static void Deserialize_GetKey(ref ulong Value, BsonDeserializationContext Context, BsonDeserializationArgs Args)
        {
            Value = unchecked((ulong) Context.Reader.ReadInt64());
        }

        public static void Deserialize_GetValue(ref Foo Value, BsonDeserializationContext Context, BsonDeserializationArgs Args)
        {
            var Reader = Context.Reader;

            Value = new Foo(Reader.ReadString(), Reader.ReadInt32());
        }
    }

    public struct Foo
    {
        public string String;

        public int Int;

        public Foo(string s, int i)
        {
            String = s;
            Int = i;
        }
    }

    public struct TestConfig: IMongoCollectionConfig
    {
        public static string GetConnectionString()
        {
            return "";
        } 

        public static string GetDBName()
        {
            return "TestDB";
        }

        public static string GetCollectionName()
        {
            return "TestCollection";
        }

        public static bool IsInMemoryDB()
        {
            return true;
        }

        public static int GetInitialCacheSize()
        {
            throw new Exception("This shouldn't run!");
        }
    }
    
    public class TestModule: IMongoModule<ulong, Foo, TestSerializationPhase, TestConfig>
    {

        public ref Foo Add(ulong Key)
        {
            return ref IMongoModule<ulong, Foo, TestSerializationPhase, TestConfig>.GetOrCreateItemRef(Key);
        }
        
        public void UpdateItem(ulong Key, ref Foo Foo)
        {
            IMongoModule<ulong, Foo, TestSerializationPhase, TestConfig>.UpdateItem(Key, ref Foo);
        }
    }
}