using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using osq2osb.Parser;

namespace osq2osb {
    public class LocatedTextReaderWrapper : TextReader {
        private TextReader source;
        private Location location;

        public Location Location {
            get {
                return location;
            }

            set {
                if(value == null) {
                    throw new ArgumentNullException("value");
                }

                location = value;
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

        public override int Read() {
            int ret = source.Read();

            if(ret >= 0) {
                location.AdvanceCharacter((char)ret);
            }

            return ret;
        }

        public override int Read(char[] buffer, int index, int count) {
            int ret = source.Read(buffer, index, count);

            if(ret >= 0) {
                location.AdvanceString(string.Concat(buffer.Skip(index).Take(count)));
            }

            return ret;
        }

        public override int ReadBlock(char[] buffer, int index, int count) {
            int ret = source.ReadBlock(buffer, index, count);

            if(ret >= 0) {
                location.AdvanceString(string.Concat(buffer.Skip(index).Take(count)));
            }

            return ret;
        }

        public override string ReadLine() {
            string ret = source.ReadLine();

            if(ret != null) {
                location.AdvanceString(ret);
                location.AdvanceLine();
            }

            return ret;
        }

        public override string ReadToEnd() {
            string ret = source.ReadToEnd();

            if(ret != null) {
                location.AdvanceString(ret);
            }

            return ret;
        }

        public override void Close() {
            // Nothing.
        }

        public override int Peek() {
            return source.Peek();
        }
    }
}
