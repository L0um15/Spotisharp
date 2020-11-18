using System;

namespace SpotiSharp
{
    class Program
    {
        static void Main(string[] args)
            => new SpotiSharp().MainAsync(args).GetAwaiter().GetResult();
    }
}
