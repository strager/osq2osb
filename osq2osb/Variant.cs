using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace osq2osb {
    partial class Parser {
        public class Variant {
            public enum Type {
                String,
                Number,
                Function
            };

            private string[] parameterList = new string[]{ };
            private string asString = "";
            private double asNumber = 0;

            private Type type = Type.String;

            public string AsString {
                get {
                    switch(type) {
                        case Type.String:
                            return asString;

                        case Type.Number:
                            return asNumber.ToString();

                        case Type.Function:
                            return "func(" + string.Join(",", parameterList) + ")\n" + asString;

                        default:
                            throw new InvalidOperationException("Variant has bad type.");
                    }
                }

                set {
                    asString = value;
                    type = Type.String;
                }
            }

            public double AsNumber {
                get {
                    switch(type) {
                        case Type.String:
                            return double.Parse(asString);

                        case Type.Number:
                            return asNumber;

                        case Type.Function:
                            return double.NaN;

                        default:
                            throw new InvalidOperationException("Variant has bad type.");
                    }
                }

                set {
                    asNumber = value;
                    type = Type.Number;
                }
            }

            public string AsFunctionBody {
                get {
                    switch(type) {
                        case Type.String:
                            return asString;

                        case Type.Number:
                            return asNumber.ToString();

                        case Type.Function:
                            return asString;

                        default:
                            throw new InvalidOperationException("Variant has bad type.");
                    }
                }

                set {
                    asString = value;
                    type = Type.Function;
                }
            }

            public Type VariantType {
                get { return type; }
            }

            public IEnumerable<string> ParameterList {
                get { return parameterList; }
                set { parameterList = value.ToArray(); }
            }

            public Variant() {
            }

            public Variant(string data) {
                AsString = data;
            }

            public Variant(double data) {
                AsNumber = data;
            }

            public Variant(string data, string[] paramList) {
                parameterList = paramList;
                AsFunctionBody = data;
            }

            public Variant(string data, Type type) {
                asString = data;
                ConvertToType(type);
            }

            public void ConvertToType(Type type) {
                /* This may look crazy... */
                switch(type) {
                    case Type.String:
                        AsString = AsString;
                        break;

                    case Type.Number:
                        AsNumber = AsNumber;
                        break;

                    case Type.Function:
                        AsFunctionBody = AsFunctionBody;
                        break;
                }
            }

            public Variant AsType(Type type) {
                Variant v = new Variant();
                v.parameterList = parameterList;
                v.asString = asString;
                v.asNumber = asNumber;

                v.ConvertToType(type);

                return v;
            }

            public void CopyFrom(Variant other) {
                this.asNumber = other.asNumber;
                this.asString = other.asString.Clone() as string;
                this.parameterList = other.parameterList.Clone() as string[];
                this.type = other.type;
            }
        }
    }
}
