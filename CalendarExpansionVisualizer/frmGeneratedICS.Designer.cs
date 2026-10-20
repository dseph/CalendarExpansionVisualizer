namespace CalendarExpansionVisualizer
{
    partial class frmGeneratedICS
    {
        private System.ComponentModel.IContainer? components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                components?.Dispose();
            }

            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            lblTitle = new Label();
            txtIcs = new RichTextBox();
            pnlButtons = new Panel();
            btnCopy = new Button();
            btnSave = new Button();
            btnClose = new Button();
            pnlButtons.SuspendLayout();
            SuspendLayout();
            // 
            // lblTitle
            // 
            lblTitle.Dock = DockStyle.Top;
            lblTitle.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            lblTitle.Location = new Point(0, 0);
            lblTitle.Name = "lblTitle";
            lblTitle.Padding = new Padding(10, 0, 0, 0);
            lblTitle.Size = new Size(1000, 50);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "Generated ICS";
            lblTitle.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // txtIcs
            // 
            txtIcs.BackColor = Color.White;
            txtIcs.BorderStyle = BorderStyle.FixedSingle;
            txtIcs.DetectUrls = false;
            txtIcs.Dock = DockStyle.Fill;
            txtIcs.Font = new Font("Consolas", 9.5F);
            txtIcs.Location = new Point(0, 50);
            txtIcs.Name = "txtIcs";
            txtIcs.ReadOnly = true;
            txtIcs.Size = new Size(1000, 645);
            txtIcs.TabIndex = 1;
            txtIcs.Text = "";
            txtIcs.WordWrap = false;
            txtIcs.TextChanged += txtIcs_TextChanged;
            // 
            // pnlButtons
            // 
            pnlButtons.Controls.Add(btnCopy);
            pnlButtons.Controls.Add(btnSave);
            pnlButtons.Controls.Add(btnClose);
            pnlButtons.Dock = DockStyle.Bottom;
            pnlButtons.Location = new Point(0, 695);
            pnlButtons.Name = "pnlButtons";
            pnlButtons.Size = new Size(1000, 55);
            pnlButtons.TabIndex = 2;
            // 
            // btnCopy
            // 
            btnCopy.Dock = DockStyle.Left;
            btnCopy.Location = new Point(125, 0);
            btnCopy.Name = "btnCopy";
            btnCopy.Size = new Size(125, 55);
            btnCopy.TabIndex = 0;
            btnCopy.Text = "Copy";
            btnCopy.UseVisualStyleBackColor = true;
            btnCopy.Click += btnCopy_Click;
            // 
            // btnSave
            // 
            btnSave.Dock = DockStyle.Left;
            btnSave.Location = new Point(0, 0);
            btnSave.Name = "btnSave";
            btnSave.Size = new Size(125, 55);
            btnSave.TabIndex = 1;
            btnSave.Text = "Save As...";
            btnSave.UseVisualStyleBackColor = true;
            btnSave.Click += btnSave_Click;
            // 
            // btnClose
            // 
            btnClose.DialogResult = DialogResult.OK;
            btnClose.Dock = DockStyle.Right;
            btnClose.Location = new Point(875, 0);
            btnClose.Name = "btnClose";
            btnClose.Size = new Size(125, 55);
            btnClose.TabIndex = 2;
            btnClose.Text = "Close";
            btnClose.UseVisualStyleBackColor = true;
            // 
            // frmGeneratedICS
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1000, 750);
            Controls.Add(txtIcs);
            Controls.Add(lblTitle);
            Controls.Add(pnlButtons);
            MinimumSize = new Size(650, 500);
            Name = "frmGeneratedICS";
            StartPosition = FormStartPosition.CenterParent;
            Text = "frmGenerated ICS";
            pnlButtons.ResumeLayout(false);
            ResumeLayout(false);
        }

        private Label lblTitle;
        private RichTextBox txtIcs;
        private Panel pnlButtons;
        private Button btnCopy;
        private Button btnSave;
        private Button btnClose;
    }
}
