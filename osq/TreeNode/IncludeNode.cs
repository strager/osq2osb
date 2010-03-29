using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using osq.Parser;

namespace osq.TreeNode {
    [DirectiveAttribute("inc(lude)?")]
    public class IncludeNode : DirectiveNode {
        public TokenNode Filename {
            get;
            private set;
        }

        private Parser.Parser parentParser;

        public IncludeNode(ITokenReader tokenReader, INodeReader nodeReader, string directiveName = null, Location location = null) :
            base(directiveName, location) {
            Filename = ExpressionRewriter.Rewrite(tokenReader);

            this.parentParser = nodeReader as Parser.Parser;
        }

        public override string Execute(ExecutionContext context) {
            // TODO Clean up.
            var output = new StringBuilder();

            string filePath = Filename.Evaluate(context) as string;

            if(filePath == null) {
                throw new DataTypeException("Need string for filename", this);
            }

            if(Location != null && Location.FileName != null) {
                filePath = Path.GetDirectoryName(Location.FileName) + Path.DirectorySeparatorChar + filePath;
            }

            using(var inputFile = File.Open(filePath, FileMode.Open, FileAccess.Read))
            using(var reader = new LocatedTextReaderWrapper(inputFile, new Location(filePath), wrapperOwnsStream: false)) {
                foreach(var node in GetNodeReader(reader)) {
                    output.Append(node.Execute(context));
                }
            }

            if(!context.Dependencies.Contains(filePath)) {
                context.Dependencies.Add(filePath);
            }

            return output.ToString();
        }

        private INodeReader GetNodeReader(LocatedTextReaderWrapper textReader) {
            // HACK I really hate this coupling.
            if(this.parentParser == null) {
                return new Parser.Parser(textReader);
            }

            return new Parser.Parser(this.parentParser, textReader);
        }
    }
}