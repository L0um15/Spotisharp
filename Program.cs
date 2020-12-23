using System;

namespace SpotiSharp
{
    class Program
    {
        static void Main(string[] args)
            => SpotiSharp.MainAsync(args).GetAwaiter().GetResult();
    }
}
