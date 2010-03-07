using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using osq;

namespace osqReverser {
    public partial class MainForm : Form {
        private readonly Converter reverser = new Converter();

        public MainForm() {
            InitializeComponent();

            osb2osq.Enabled = false;
        }

        private void osq2osb_Click(object sender, EventArgs e) {
            using(var reader = new LocatedTextReaderWrapper(osqScript.Text)) {
                osbScript.Text = reverser.Convert(reader);
            }

            osb2osq.Enabled = true;
        }

        private void osb2osq_Click(object sender, EventArgs e) {
            osqScript.Text = reverser.SourceFromOutput(osbScript.Text);
        }
    }
}