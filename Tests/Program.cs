using System;
using System.Runtime.CompilerServices;
using MongoModule;

namespace Tests // Note: actual namespace depends on the project name.
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // ref var Foo = ref IMongoModule<TestModule, ulong, Foo, TestSerializationPhase>.GetOrCreateItemRef(69);
            //
            // Console.WriteLine(Foo.Int);
            //
            // Foo.Int = 69;
            //
            // IMongoModule<TestModule, ulong, Foo, TestSerializationPhase>.UpdateItem(69, ref Foo);

            Console.WriteLine("Yes");

            var Instance = new TestModule();
            //
            // ref var Foo = ref Instance.Add(69);
            //
            // Console.WriteLine(Foo.Int);
            //
            // Foo.Int = 69;
            //
            // Instance.UpdateItem(69, ref Foo);
        }
    }
}