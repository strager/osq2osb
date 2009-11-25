using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace osq2osb {
    class Program {
        static void Main(string[] args) {
            if(args.Length == 0) {
                Console.WriteLine("Parsing from console...");

                var parser = new Parser();

                parser.Parse(Console.In, Console.Out);
            } else {
                List<FileSystemWatcher> watchers = new List<FileSystemWatcher>();

                foreach(var filename in args) {
                    ParseFile(filename);

                    Console.WriteLine("Watching " + filename + " for changes...");

                    var watcher = new FileSystemWatcher(Path.GetDirectoryName(filename), Path.GetFileName(filename));
                    watcher.Changed += new FileSystemEventHandler(FileChanged);

                    watcher.EnableRaisingEvents = true;

                    watchers.Add(watcher);
                }

                System.Threading.Thread.Sleep(-1);
            }
        }

        private static void ParseFile(string filename) {
            Console.Write("Parsing " + filename + "...");

            using(var inputFile = File.Open(filename, FileMode.Open, FileAccess.Read))
            using(var outputFile = File.Open(Path.Combine(Path.GetDirectoryName(filename), Path.GetFileNameWithoutExtension(filename)) + ".osb", FileMode.Create, FileAccess.Write)) {
                var parser = new Parser();

                using(var reader = new StreamReader(inputFile))
                using(var writer = new StreamWriter(outputFile)) {
                    try {
                        parser.Parse(reader, writer);
                    } catch(Exception e) {
                        Console.WriteLine("\nError: " + e.Message);
                        Console.WriteLine("Stack trace:");
                        Console.Write(e.StackTrace);
                    }
                }
            }

            Console.WriteLine("  Done!");
        }

        private static void FileChanged(object source, FileSystemEventArgs e) {
            ParseFile(e.FullPath);
        }
    }
}
