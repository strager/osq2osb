using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using osq;

namespace osqReverser {
    public partial class MainForm : Form {
        private readonly Reverser reverser = new Reverser();

        public MainForm() {
            InitializeComponent();

            osb2osq.Enabled = false;
        }

        private void osq2osb_Click(object sender, EventArgs e) {
            using(var reader = new LocatedTextReaderWrapper(osqScript.Text)) {
                osbScript.Text = reverser.Parse(reader);
            }

            osb2osq.Enabled = true;
        }

        private void osb2osq_Click(object sender, EventArgs e) {
            osqScript.Text = reverser.Reverse(osbScript.Text);
        }
    }
}