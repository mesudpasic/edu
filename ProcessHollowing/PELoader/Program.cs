using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace PHLoader
{
    internal class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 2) {
                Console.WriteLine("You need to specify existing source and target executables.");
                return;
            }
            string source = args[0];
            string target = args[1];
            if (String.IsNullOrEmpty(source) || String.IsNullOrEmpty(target))
            {
                Console.WriteLine("You need to specify existing source and target executables.");
                return;
            }
            if (!File.Exists(source) || !File.Exists(target))
            {
                Console.WriteLine("You need to specify existing source and target executables.");
            }
            else
            {
                bool status = PELoader.Execute(source, target);
                Console.WriteLine(status ? "Success." : "Failed.");
            }
        }
    }
}
