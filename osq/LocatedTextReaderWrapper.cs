using System;
using System.IO;
using System.Linq;

namespace osq {
    public class LocatedTextReaderWrapper : TextReader, IDisposable {
        private TextReader source;
        private Location location;
        private bool mustDisposeSource = false;

        public Location Location {
            get {
                return this.location;
            }

            set {
                if(value == null) {
                    throw new ArgumentNullException("value");
                }

                this.location = value;
            }
        }

        public void SkipWhiteSpace() {
            while(Peek() >= 0 && char.IsWhiteSpace((char)Peek())) {
                Location.AdvanceCharacter((char)Read());
            }
        }

        public LocatedTextReaderWrapper(TextReader source) :
            this(source, new Location()) {
        }

        public LocatedTextReaderWrapper(TextReader source, Location location) {
            if(source == null) {
                throw new ArgumentNullException("source");
            }

            if(location == null) {
                throw new ArgumentNullException("location");
            }

            this.location = location;
            this.source = source;
        }

        public LocatedTextReaderWrapper(string source) :
            this(source, new Location()) {
        }

        public LocatedTextReaderWrapper(string source, Location location) :
            this(new StringReader(source), location) {
            this.mustDisposeSource = true;
        }

        public LocatedTextReaderWrapper(Stream source) :
            this(source, new Location()) {
        }

        public LocatedTextReaderWrapper(Stream source, Location location) :
            this(new StreamReader(source), location) {
            this.mustDisposeSource = true;
        }

        public override int Read() {
            int ret = this.source.Read();

            if(ret >= 0) {
                this.location.AdvanceCharacter((char)ret);
            }

            return ret;
        }

        public override int Read(char[] buffer, int index, int count) {
            int ret = this.source.Read(buffer, index, count);

            if(ret >= 0) {
                this.location.AdvanceString(string.Concat(buffer.Skip(index).Take(count)));
            }

            return ret;
        }

        public override int ReadBlock(char[] buffer, int index, int count) {
            int ret = this.source.ReadBlock(buffer, index, count);

            if(ret >= 0) {
                this.location.AdvanceString(string.Concat(buffer.Skip(index).Take(count)));
            }

            return ret;
        }

        public override string ReadLine() {
            string ret = this.source.ReadLine();

            if(ret != null) {
                this.location.AdvanceString(ret);
                this.location.AdvanceLine();
            }

            return ret;
        }

        public override string ReadToEnd() {
            string ret = this.source.ReadToEnd();

            if(ret != null) {
                this.location.AdvanceString(ret);
            }

            return ret;
        }

        public override void Close() {
            // Nothing.
        }

        public override int Peek() {
            return this.source.Peek();
        }

        private bool disposed = false;

        protected override void Dispose(bool disposing) {
            if(!this.disposed) {
                if(disposing) {
                    if(this.mustDisposeSource && this.source != null) {
                        this.source.Dispose();
                    }
                }

                this.source = null;
                this.disposed = true;
            }

            base.Dispose(disposing);
        }
    }
}