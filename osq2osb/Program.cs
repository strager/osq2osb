using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace osq2osb {
    class Program {
        static IDictionary<FileCollectionWatcher, string> watchers;

        static void Main(string[] args) {
            if(args.Length == 0) {
                Console.WriteLine("Parsing from console...");
                
                var executionContext = new ExecutionContext();

                try {
                    using(var reader = new LocatedTextReaderWrapper(Console.In)) {
                        foreach(var node in Parser.Parser.ReadNodes(reader)) {
                            string output = node.Execute(executionContext);

                            Console.Write(output);
                        }
                    }
                } catch(Exception e) {
                    Console.WriteLine("Error: " + e.ToString());
                }
            } else {
                watchers = new Dictionary<FileCollectionWatcher, string>();

                foreach(var filename in args) {
                    var watcher = new FileCollectionWatcher();
                    watcher.Changed += FileChanged;
                    watchers[watcher] = filename;

                    ParseFile(watcher, filename);

                    Console.WriteLine("Watching " + filename + " for changes...");
                }

                System.Threading.Thread.Sleep(-1);
            }
        }

        private static void ParseFile(FileCollectionWatcher watcher, string filename) {
            Console.Write("Parsing " + filename + "...");
            
            using(var inputFile = File.Open(filename, FileMode.Open, FileAccess.Read))
            using(var outputFile = File.Open(Path.Combine(Path.GetDirectoryName(filename), Path.GetFileNameWithoutExtension(filename)) + ".osb", FileMode.Create, FileAccess.Write)) {
                var executionContext = new ExecutionContext();

                using(var reader = new LocatedTextReaderWrapper(inputFile, new Parser.Location(filename)))
                using(var writer = new StreamWriter(outputFile)) {
                    try {
                        foreach(var node in Parser.Parser.ReadNodes(reader)) {
                            string output = node.Execute(executionContext);

                            writer.Write(output);
                        }

                        watcher.Clear();
                    } catch(Exception e) {
                        Console.WriteLine("\nError: " + e.ToString());

                        return;
                    } finally {
                        if(!watcher.Contains(filename)) {
                            watcher.Add(filename);
                        }

                        foreach(string file in executionContext.Dependancies.Where((file) => !watcher.Contains(file))) {
                            watcher.Add(file);
                        }
                    }
                }
            }

            Console.WriteLine("  Done!");
        }

        private static void FileChanged(object sender, FileSystemEventArgs e) {
            var watcher = (FileCollectionWatcher)sender;

            ParseFile(watcher, watchers[watcher]);
        }
    }
}
