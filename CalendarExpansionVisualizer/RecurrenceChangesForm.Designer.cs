namespace CalendarExpansionVisualizer
{
    partial class RecurrenceChangesForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            pnlTop = new Panel();
            rbExceptions = new RadioButton();
            rbDeletions = new RadioButton();
            rbBoth = new RadioButton();
            btnList = new Button();
            txtOutput = new TextBox();
            pnlTop.SuspendLayout();
            SuspendLayout();
            // 
            // pnlTop
            // 
            pnlTop.Controls.Add(rbExceptions);
            pnlTop.Controls.Add(rbDeletions);
            pnlTop.Controls.Add(rbBoth);
            pnlTop.Controls.Add(btnList);
            pnlTop.Dock = DockStyle.Top;
            pnlTop.Location = new Point(0, 0);
            pnlTop.Margin = new Padding(4, 5, 4, 5);
            pnlTop.Name = "pnlTop";
            pnlTop.Size = new Size(1120, 87);
            pnlTop.TabIndex = 0;
            // 
            // rbExceptions
            // 
            rbExceptions.AutoSize = true;
            rbExceptions.Checked = true;
            rbExceptions.Location = new Point(17, 23);
            rbExceptions.Margin = new Padding(4, 5, 4, 5);
            rbExceptions.Name = "rbExceptions";
            rbExceptions.Size = new Size(152, 29);
            rbExceptions.TabIndex = 0;
            rbExceptions.TabStop = true;
            rbExceptions.Text = "List Exceptions";
            rbExceptions.UseVisualStyleBackColor = true;
            // 
            // rbDeletions
            // 
            rbDeletions.AutoSize = true;
            rbDeletions.Location = new Point(229, 23);
            rbDeletions.Margin = new Padding(4, 5, 4, 5);
            rbDeletions.Name = "rbDeletions";
            rbDeletions.Size = new Size(142, 29);
            rbDeletions.TabIndex = 1;
            rbDeletions.Text = "List Deletions";
            rbDeletions.UseVisualStyleBackColor = true;
            // 
            // rbBoth
            // 
            rbBoth.AutoSize = true;
            rbBoth.Location = new Point(429, 23);
            rbBoth.Margin = new Padding(4, 5, 4, 5);
            rbBoth.Name = "rbBoth";
            rbBoth.Size = new Size(266, 29);
            rbBoth.TabIndex = 2;
            rbBoth.Text = "List Exceptions and Deletions";
            rbBoth.UseVisualStyleBackColor = true;
            // 
            // btnList
            // 
            btnList.Location = new Point(943, 17);
            btnList.Margin = new Padding(4, 5, 4, 5);
            btnList.Name = "btnList";
            btnList.Size = new Size(157, 53);
            btnList.TabIndex = 3;
            btnList.Text = "List";
            btnList.UseVisualStyleBackColor = true;
            btnList.Click += btnList_Click;
            // 
            // txtOutput
            // 
            txtOutput.BackColor = Color.White;
            txtOutput.Dock = DockStyle.Fill;
            txtOutput.Font = new Font("Consolas", 9.5F);
            txtOutput.Location = new Point(0, 87);
            txtOutput.Margin = new Padding(4, 5, 4, 5);
            txtOutput.Multiline = true;
            txtOutput.Name = "txtOutput";
            txtOutput.ReadOnly = true;
            txtOutput.ScrollBars = ScrollBars.Both;
            txtOutput.Size = new Size(1120, 848);
            txtOutput.TabIndex = 1;
            txtOutput.WordWrap = false;
            txtOutput.TextChanged += txtOutput_TextChanged;
            // 
            // RecurrenceChangesForm
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1120, 935);
            Controls.Add(txtOutput);
            Controls.Add(pnlTop);
            Margin = new Padding(4, 5, 4, 5);
            MinimumSize = new Size(791, 563);
            Name = "RecurrenceChangesForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Recurrence Changes";
            pnlTop.ResumeLayout(false);
            pnlTop.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Panel pnlTop;
        private RadioButton rbExceptions;
        private RadioButton rbDeletions;
        private RadioButton rbBoth;
        private Button btnList;
        private TextBox txtOutput;
    }
}
