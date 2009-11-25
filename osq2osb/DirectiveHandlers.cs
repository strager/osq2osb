using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;

namespace osq2osb {
    partial class Parser {
        class DirectiveHandlers {
            public static void Handle(string line, TextReader input, TextWriter output, Parser parser) {
                Regex typeRe = new Regex(@"(?!#)\w+\b");
                string match = typeRe.Match(line).Value;

                switch(match) {
                    case "def":
                        HandleDefine(line, input, output, parser);
                        break;

                    case "each":
                        HandleEach(line, input, output, parser);
                        break;

                    case "let":
                        HandleLet(line, input, output, parser);
                        break;

                    case "rep":
                        HandleRep(line, input, output, parser);
                        break;

                    case "for":
                        HandleFor(line, input, output, parser);
                        break;

                    default:
                        throw new InvalidDataException("Unknown directive " + match);
                }
            }

            public static void HandleDefine(string line, TextReader input, TextWriter output, Parser parser) {
                Regex re = new Regex(@"(?!#)\w+\b\s+(?<name>\w+)(\((?<params>[^\)]*)\))?((?<=\))\s*|\s+)(?<value>.*)$", RegexOptions.ExplicitCapture);
                var match = re.Match(line);

                if(!match.Success) {
                    throw new InvalidDataException("Bad form for #def directive.");
                }

                string name = match.Groups["name"].Value;
                string[] parameters = match.Groups["params"].Value.Split(',');
                parameters = parameters.Select((s) => { return s.Trim(); }).Where((s) => { return s.Length > 0; }).ToArray();
                string body = match.Groups["value"].Value;

                if(body == "") {
                    string curLine;

                    while((curLine = input.ReadLine()) != null) {
                        if((new Regex(@"^#\s*enddef").IsMatch(curLine))) {
                            break;
                        }

                        body += curLine + Environment.NewLine;
                    }
                }

                Variant v = new Variant(body, parameters);

                parser.SetVariable(name, v);
            }

            public static void HandleEach(string line, TextReader input, TextWriter output, Parser parser) {
                Regex re = new Regex(@"(?!#)\w+\b\s+(?<name>\w+)\s+(?<array>.+)$", RegexOptions.ExplicitCapture);
                var match = re.Match(line);

                if(!match.Success) {
                    throw new InvalidDataException("Bad form for #each directive.");
                }

                string name = match.Groups["name"].Value;
                string[] array = match.Groups["array"].Value.Split(',');
                array = array.Select((s) => { return s.Trim(); }).Where((s) => { return s.Length > 0; }).ToArray();

                string body = "";

                string curLine;

                while((curLine = input.ReadLine()) != null) {
                    if((new Regex(@"^#\s*endeach").IsMatch(curLine))) {
                        break;
                    }

                    body += curLine + Environment.NewLine;
                }

                var subParser = new Parser(parser);

                foreach(var item in array) {
                    subParser.SetVariable(name, new Variant(item));

                    using(var bodyReader = new StringReader(body)) {
                        subParser.Parse(bodyReader, output);
                    }
                }
            }

            public static void HandleLet(string line, TextReader input, TextWriter output, Parser parser) {
                Regex re = new Regex(@"(?!#)\w+\b\s+(?<name>\w+)\s+(?<value>.+)$", RegexOptions.ExplicitCapture);
                var match = re.Match(line);

                if(!match.Success) {
                    throw new InvalidDataException("Bad form for #let directive.");
                }

                string name = match.Groups["name"].Value;
                string value = match.Groups["value"].Value;

                var subParser = new Parser(parser);

                StringBuilder parsedValue = new StringBuilder();

                using(var valueReader = new StringReader(value))
                using(var parsedValueWriter = new StringWriter(parsedValue)) {
                    subParser.Parse(valueReader, parsedValueWriter);
                }

                parser.SetVariable(name, new Variant(parsedValue.ToString().Trim(Environment.NewLine.ToCharArray())));
            }

            public static void HandleRep(string line, TextReader input, TextWriter output, Parser parser) {
                Regex re = new Regex(@"(?!#)\w+\b\s+(?<count>\d+)\s*$", RegexOptions.ExplicitCapture);
                var match = re.Match(line);

                if(!match.Success) {
                    throw new InvalidDataException("Bad form for #rep directive.");
                }

                int count = int.Parse(match.Groups["count"].Value);

                string body = "";

                string curLine;

                while((curLine = input.ReadLine()) != null) {
                    if((new Regex(@"^#\s*endrep").IsMatch(curLine))) {
                        break;
                    }

                    body += curLine + Environment.NewLine;
                }

                var subParser = new Parser(parser);

                for(int i = 0; i < count; ++i) {
                    using(var bodyReader = new StringReader(body)) {
                        subParser.Parse(bodyReader, output);
                    }
                }
            }

            public static void HandleFor(string line, TextReader input, TextWriter output, Parser parser) {
                Regex re = new Regex(@"(?!#)\w+\b\s+(?<name>\w+)\s+(?<start>\d+)\s+(?<end>\d+)$", RegexOptions.ExplicitCapture);
                var match = re.Match(line);

                if(!match.Success) {
                    throw new InvalidDataException("Bad form for #for directive.");
                }

                string name = match.Groups["name"].Value;
                int start = int.Parse(match.Groups["start"].Value);
                int end = int.Parse(match.Groups["end"].Value);

                string body = "";

                string curLine;

                while((curLine = input.ReadLine()) != null) {
                    if((new Regex(@"^#\s*endfor").IsMatch(curLine))) {
                        break;
                    }

                    body += curLine + Environment.NewLine;
                }

                var subParser = new Parser(parser);
                subParser.SetVariable(name, new Variant(start));

                while(subParser.GetVariable(name).AsNumber < end) {
                    using(var bodyReader = new StringReader(body)) {
                        subParser.Parse(bodyReader, output);
                    }

                    subParser.SetVariable(name, new Variant(subParser.GetVariable(name).AsNumber + 1));
                }
            }
        }
    }
}
