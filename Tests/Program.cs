using System;

namespace Tests
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var Instance = new TestModule();
            
            ref var Foo = ref Instance.Add(69);
            
            Console.WriteLine(Foo.Int);
            
            Foo.Int = 69;

            Foo.String = null;
            
            Instance.UpdateItem(69, ref Foo);
        }
    }
}