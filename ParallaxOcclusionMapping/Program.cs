using System;

namespace ParallaxOcclusionMapping
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (POMDemo game = new POMDemo())
            {
                game.Run();
            }
        }
    }
}

