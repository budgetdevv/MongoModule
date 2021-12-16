using System;
using System.Runtime.CompilerServices;
using MongoDB.Bson.Serialization;
using MongoModule;

namespace Tests
{
    public struct TestSerializationPhase: IMongoSerializationPhase<Foo>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static void Serialize(ref Foo Value, BsonSerializationContext Context, BsonSerializationArgs Args)
        {
            var Writer = Context.Writer;
            
            Writer.WriteName("String");

            Writer.WriteString(Value.String);
            
            Writer.WriteName("Int");
            
            Writer.WriteInt32(Value.Int);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static void Deserialize(ref Foo Value, BsonDeserializationContext Context, BsonDeserializationArgs Args)
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
    
    public class TestModule: IMongoModule<TestModule, ulong, Foo, TestSerializationPhase>
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

        // public ref Foo Add(ulong Key)
        // {
        //     return ref IMongoModule<TestModule, ulong, Foo, TestSerializationPhase>.GetOrCreateItemRef(Key);
        // }
        //
        // public void UpdateItem(ulong Key, ref Foo Foo)
        // {
        //     IMongoModule<TestModule, ulong, Foo, TestSerializationPhase>.UpdateItem(Key, ref Foo);
        // }
    }
}