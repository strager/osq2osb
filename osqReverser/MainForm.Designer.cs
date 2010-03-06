namespace osqReverser
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.osqScript = new System.Windows.Forms.TextBox();
            this.osbScript = new System.Windows.Forms.TextBox();
            this.osq2osb = new System.Windows.Forms.Button();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.osb2osq = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // osqScript
            // 
            this.osqScript.Dock = System.Windows.Forms.DockStyle.Fill;
            this.osqScript.Location = new System.Drawing.Point(0, 0);
            this.osqScript.Multiline = true;
            this.osqScript.Name = "osqScript";
            this.osqScript.Size = new System.Drawing.Size(581, 776);
            this.osqScript.TabIndex = 0;
            this.osqScript.TextChanged += new System.EventHandler(this.osqScript_TextChanged);
            // 
            // osbScript
            // 
            this.osbScript.Dock = System.Windows.Forms.DockStyle.Fill;
            this.osbScript.Location = new System.Drawing.Point(0, 0);
            this.osbScript.Multiline = true;
            this.osbScript.Name = "osbScript";
            this.osbScript.Size = new System.Drawing.Size(583, 776);
            this.osbScript.TabIndex = 1;
            // 
            // osq2osb
            // 
            this.osq2osb.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.osq2osb.Location = new System.Drawing.Point(517, 794);
            this.osq2osb.Name = "osq2osb";
            this.osq2osb.Size = new System.Drawing.Size(159, 45);
            this.osq2osb.TabIndex = 2;
            this.osq2osb.Text = "OSQ > OSB";
            this.osq2osb.UseVisualStyleBackColor = true;
            this.osq2osb.Click += new System.EventHandler(this.osq2osb_Click);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer1.Location = new System.Drawing.Point(12, 12);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.osqScript);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.osbScript);
            this.splitContainer1.Size = new System.Drawing.Size(1168, 776);
            this.splitContainer1.SplitterDistance = 581;
            this.splitContainer1.TabIndex = 3;
            // 
            // osb2osq
            // 
            this.osb2osq.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.osb2osq.Location = new System.Drawing.Point(517, 845);
            this.osb2osq.Name = "osb2osq";
            this.osb2osq.Size = new System.Drawing.Size(159, 45);
            this.osb2osq.TabIndex = 4;
            this.osb2osq.Text = "OSQ < OSB";
            this.osb2osq.UseVisualStyleBackColor = true;
            this.osb2osq.Click += new System.EventHandler(this.osb2osq_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1192, 902);
            this.Controls.Add(this.osb2osq);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.osq2osb);
            this.Name = "MainForm";
            this.Text = "osq Reverser";
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TextBox osqScript;
        private System.Windows.Forms.TextBox osbScript;
        private System.Windows.Forms.Button osq2osb;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.Button osb2osq;
    }
}