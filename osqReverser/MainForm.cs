using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using osq;
using osq.Parser;
using Encoder = osq.Encoder;

namespace osqReverser {
    public partial class MainForm : Form {
        private readonly Encoder reverser = new Encoder {
            Parser = new Parser(),
            ExecutionContext = new ExecutionContext(),
        };

        public MainForm() {
            InitializeComponent();

            osb2osq.Enabled = false;
        }

        private void osq2osb_Click(object sender, EventArgs e) {
            using(var reader = new LocatedTextReaderWrapper(osqScript.Text)) {
                reverser.Parser.InputReader = reader;

                osbScript.Text = reverser.Encode();
            }

            osb2osq.Enabled = true;
        }

        private void osb2osq_Click(object sender, EventArgs e) {
            osqScript.Text = reverser.Decode(osbScript.Text);
        }
    }
}