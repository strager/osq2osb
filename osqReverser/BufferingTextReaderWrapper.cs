using System;
using System.IO;
using System.Linq;
using System.Text;

namespace osqReverser {
    public class BufferingTextReaderWrapper : TextReader, IDisposable {
        private readonly bool mustDisposeSource;
        private bool disposed;
        private TextReader source;

        public BufferingTextReaderWrapper(TextReader source) {
            if(source == null) {
                throw new ArgumentNullException("source");
            }

            this.source = source;
        }

        public BufferingTextReaderWrapper(string source) :
            this(new StringReader(source)) {
            this.mustDisposeSource = true;
        }

        public BufferingTextReaderWrapper(Stream source) :
            this(source, true) {
        }

        public BufferingTextReaderWrapper(Stream source, bool wraperOwnsStream) :
            this(new StreamReader(source)) {
            this.mustDisposeSource = wraperOwnsStream;
        }

        public string BufferedText {
            get {
                return this.textBuffer.ToString();
            }
        }

        private readonly StringBuilder textBuffer = new StringBuilder();

        public void ClearBuffer() {
            textBuffer.Remove(0, textBuffer.Length);    // Clear.
        }

        public override int Read() {
            int ret = this.source.Read();

            if(ret >= 0) {
                textBuffer.Append((char)ret);
            }

            return ret;
        }

        public override int Read(char[] buffer, int index, int count) {
            int ret = this.source.Read(buffer, index, count);

            if(ret >= 0) {
                textBuffer.Append(string.Concat(buffer.Skip(index).Take(count)));
            }

            return ret;
        }

        public override int ReadBlock(char[] buffer, int index, int count) {
            int ret = this.source.ReadBlock(buffer, index, count);

            if(ret >= 0) {
                textBuffer.Append(string.Concat(buffer.Skip(index).Take(count)));
            }

            return ret;
        }

        public override string ReadLine() {
            string ret = this.source.ReadLine();

            if(ret != null) {
                textBuffer.Append(ret);
                textBuffer.Append(Environment.NewLine);
            }

            return ret;
        }

        public override string ReadToEnd() {
            string ret = this.source.ReadToEnd();

            if(ret != null) {
                textBuffer.Append(ret);
            }

            return ret;
        }

        public override void Close() {
            // Nothing.
        }

        public override int Peek() {
            return this.source.Peek();
        }

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