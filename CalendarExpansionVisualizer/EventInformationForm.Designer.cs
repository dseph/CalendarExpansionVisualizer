namespace CalendarExpansionVisualizer
{
    partial class EventInformationForm
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
            txtBody = new TextBox();
            txtWarn = new TextBox();
            pnlButtons = new Panel();
            btnClose = new Button();
            btnListChanges = new Button();
            lblTitle = new Label();
            pnlButtons.SuspendLayout();
            SuspendLayout();
            // 
            // txtBody
            // 
            txtBody.BackColor = Color.White;
            txtBody.Dock = DockStyle.Fill;
            txtBody.Font = new Font("Consolas", 9.5F);
            txtBody.Location = new Point(0, 50);
            txtBody.Margin = new Padding(4);
            txtBody.Multiline = true;
            txtBody.Name = "txtBody";
            txtBody.ReadOnly = true;
            txtBody.ScrollBars = ScrollBars.Both;
            txtBody.Size = new Size(968, 614);
            txtBody.TabIndex = 0;
            txtBody.WordWrap = false;
            // 
            // txtWarn
            // 
            txtWarn.BackColor = Color.White;
            txtWarn.Dock = DockStyle.Bottom;
            txtWarn.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            txtWarn.ForeColor = Color.Red;
            txtWarn.Location = new Point(0, 664);
            txtWarn.Margin = new Padding(4);
            txtWarn.Multiline = true;
            txtWarn.Name = "txtWarn";
            txtWarn.ReadOnly = true;
            txtWarn.ScrollBars = ScrollBars.Vertical;
            txtWarn.Size = new Size(968, 112);
            txtWarn.TabIndex = 1;
            txtWarn.Visible = false;
            // 
            // pnlButtons
            // 
            pnlButtons.Controls.Add(btnClose);
            pnlButtons.Controls.Add(btnListChanges);
            pnlButtons.Dock = DockStyle.Bottom;
            pnlButtons.Location = new Point(0, 776);
            pnlButtons.Margin = new Padding(4);
            pnlButtons.Name = "pnlButtons";
            pnlButtons.Size = new Size(968, 55);
            pnlButtons.TabIndex = 2;
            pnlButtons.Paint += pnlButtons_Paint;
            // 
            // btnClose
            // 
            btnClose.DialogResult = DialogResult.OK;
            btnClose.Dock = DockStyle.Right;
            btnClose.Location = new Point(843, 0);
            btnClose.Margin = new Padding(4);
            btnClose.Name = "btnClose";
            btnClose.Size = new Size(125, 55);
            btnClose.TabIndex = 0;
            btnClose.Text = "Close";
            btnClose.UseVisualStyleBackColor = true;
            // 
            // btnListChanges
            // 
            btnListChanges.Dock = DockStyle.Left;
            btnListChanges.Enabled = false;
            btnListChanges.Location = new Point(0, 0);
            btnListChanges.Margin = new Padding(4);
            btnListChanges.Name = "btnListChanges";
            btnListChanges.Size = new Size(290, 55);
            btnListChanges.TabIndex = 1;
            btnListChanges.Text = "List Recurrence Changes";
            btnListChanges.UseVisualStyleBackColor = true;
            btnListChanges.Click += btnListChanges_Click;
            // 
            // lblTitle
            // 
            lblTitle.Dock = DockStyle.Top;
            lblTitle.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            lblTitle.Location = new Point(0, 0);
            lblTitle.Margin = new Padding(4, 0, 4, 0);
            lblTitle.Name = "lblTitle";
            lblTitle.Padding = new Padding(10, 0, 0, 0);
            lblTitle.Size = new Size(968, 50);
            lblTitle.TabIndex = 3;
            lblTitle.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // EventInformationForm
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(968, 831);
            Controls.Add(txtBody);
            Controls.Add(txtWarn);
            Controls.Add(pnlButtons);
            Controls.Add(lblTitle);
            Margin = new Padding(4);
            MinimumSize = new Size(620, 486);
            Name = "EventInformationForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Event Information";
            pnlButtons.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TextBox txtBody;
        private TextBox txtWarn;
        private Panel pnlButtons;
        private Button btnClose;
        private Button btnListChanges;
        private Label lblTitle;
    }
}
