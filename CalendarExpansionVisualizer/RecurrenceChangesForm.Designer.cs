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
            pnlTop.Name = "pnlTop";
            pnlTop.Size = new Size(784, 52);
            pnlTop.TabIndex = 0;
            //
            // rbExceptions
            //
            rbExceptions.AutoSize = true;
            rbExceptions.Checked = true;
            rbExceptions.Location = new Point(12, 14);
            rbExceptions.Name = "rbExceptions";
            rbExceptions.Size = new Size(130, 24);
            rbExceptions.TabIndex = 0;
            rbExceptions.TabStop = true;
            rbExceptions.Text = "List Exceptions";
            rbExceptions.UseVisualStyleBackColor = true;
            //
            // rbDeletions
            //
            rbDeletions.AutoSize = true;
            rbDeletions.Location = new Point(160, 14);
            rbDeletions.Name = "rbDeletions";
            rbDeletions.Size = new Size(120, 24);
            rbDeletions.TabIndex = 1;
            rbDeletions.Text = "List Deletions";
            rbDeletions.UseVisualStyleBackColor = true;
            //
            // rbBoth
            //
            rbBoth.AutoSize = true;
            rbBoth.Location = new Point(300, 14);
            rbBoth.Name = "rbBoth";
            rbBoth.Size = new Size(260, 24);
            rbBoth.TabIndex = 2;
            rbBoth.Text = "List Exceptions and Deletions";
            rbBoth.UseVisualStyleBackColor = true;
            //
            // btnList
            //
            btnList.Location = new Point(660, 10);
            btnList.Name = "btnList";
            btnList.Size = new Size(110, 32);
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
            txtOutput.Location = new Point(0, 52);
            txtOutput.Multiline = true;
            txtOutput.Name = "txtOutput";
            txtOutput.ReadOnly = true;
            txtOutput.ScrollBars = ScrollBars.Both;
            txtOutput.Size = new Size(784, 509);
            txtOutput.TabIndex = 1;
            txtOutput.WordWrap = false;
            //
            // RecurrenceChangesForm
            //
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(784, 561);
            Controls.Add(txtOutput);
            Controls.Add(pnlTop);
            MinimumSize = new Size(560, 360);
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
