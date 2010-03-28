namespace osq.Parser {
    public class ParserOptions {
        public bool AllowVariableShorthand {
            get;
            set;
        }

        public ParserOptions() {
            AllowVariableShorthand = true;
        }

        public ParserOptions Clone() {
            return this.MemberwiseClone() as ParserOptions;
        }
    }
}


