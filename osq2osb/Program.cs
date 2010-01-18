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
                
                var executionContext = new ExecutionContext();

                try {
                    foreach(var node in Parser.Parser.Parse(Console.In)) {
                        node.Execute(Console.Out, executionContext);
                    }
                } catch(Exception e) {
                    Console.WriteLine("Error: " + e.ToString());
                }
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
                var executionContext = new ExecutionContext();

                using(var reader = new StreamReader(inputFile))
                using(var writer = new StreamWriter(outputFile)) {
                    try {
                        foreach(var node in Parser.Parser.Parse(reader, new Parser.Location(filename))) {
                            node.Execute(writer, executionContext);
                        }
                    } catch(Exception e) {
                        Console.WriteLine("\nError: " + e.ToString());

                        return;
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
