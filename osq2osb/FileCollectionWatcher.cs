using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace osq2osb {
    class FileCollectionWatcher {
        private readonly IDictionary<string, FileSystemWatcher> watchers = new Dictionary<string, FileSystemWatcher>();

        public IEnumerable<string> Files {
            get {
                return this.watchers.Keys;
            }
        }

        public bool Contains(string file) {
            return Files.Any((filename) => file.Equals(filename, StringComparison.InvariantCultureIgnoreCase));
        }

        public void Add(string file) {
            var watcher = new FileSystemWatcher(Path.GetDirectoryName(file), Path.GetFileName(file));

            watcher.Changed += FileChanged;
            watcher.EnableRaisingEvents = true;

            this.watchers[file] = watcher;
        }

        public void Clear() {
            foreach(var p in this.watchers) {
                p.Value.Dispose();
            }

            this.watchers.Clear();
        }

        public event FileSystemEventHandler Changed;

        protected virtual void OnChanged(FileSystemEventArgs e) {
            FileSystemEventHandler changed = Changed;

            if(changed != null) {
                changed(this, e);
            }
        }

        private void FileChanged(object sender, FileSystemEventArgs e) {
            OnChanged(e);
        }
    }
}
