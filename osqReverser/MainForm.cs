using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using osq;
using osq.TreeNode;

namespace osqReverser {
    public partial class MainForm : Form {
        private List<ConvertedNode> scriptNodes;

        public MainForm() {
            InitializeComponent();

            osb2osq.Enabled = false;
        }

        private void osq2osb_Click(object sender, EventArgs e) {
            scriptNodes = new List<ConvertedNode>();

            var output = new StringBuilder();

            // TODO Actually store original data.
            using(var bufferingReader = new BufferingTextReaderWrapper(osqScript.Text))
            using(var reader = new LocatedTextReaderWrapper(bufferingReader)) {
                var context = new ExecutionContext();

                NodeBase node;

                while((node = Parser.ReadNode(reader)) != null) {
                    string curOutput = node.Execute(context);

                    output.Append(curOutput);

                    var converted = new ConvertedNode {
                        Node = node,
                        NodeOutput = curOutput,
                        OriginalScript = bufferingReader.BufferedText
                    };

                    scriptNodes.Add(converted);

                    bufferingReader.ClearBuffer();
                }
            }

            osbScript.Text = output.ToString();

            osb2osq.Enabled = true;
        }

        private void osb2osq_Click(object sender, EventArgs e) {
            var output = new StringBuilder();
            var input = osbScript.Text;

            foreach(var convertedNode in scriptNodes) {
                int index = input.IndexOf(convertedNode.NodeOutput);

                if(index < 0) {
                    continue;
                }

                output.Append(input.Substring(0, index));
                output.Append(convertedNode.OriginalScript);

                input = input.Substring(index + convertedNode.NodeOutput.Length);
            }

            output.Append(input);

            osqScript.Text = output.ToString();
        }
    }
}