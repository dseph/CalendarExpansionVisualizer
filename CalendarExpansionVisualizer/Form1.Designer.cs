namespace CalendarExpansionVisualizer
{
    partial class Form1
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
            grpAuth = new GroupBox();
            rbDelegated = new RadioButton();
            rbApplication = new RadioButton();
            rbMicrosoftApp = new RadioButton();
            lblTenantId = new Label();
            txtTenantId = new TextBox();
            lblClientId = new Label();
            txtClientId = new TextBox();
            lblClientSecret = new Label();
            txtClientSecret = new TextBox();
            lblRedirectUri = new Label();
            txtRedirectUri = new TextBox();
            lblUpn = new Label();
            txtUpn = new TextBox();
            btnGetToken = new Button();
            lblTokenStatus = new Label();
            grpQuery = new GroupBox();
            lblStart = new Label();
            dtpStart = new DateTimePicker();
            lblEnd = new Label();
            dtpEnd = new DateTimePicker();
            btnFetch = new Button();
            btnCheck = new Button();
            lblLogPath = new Label();
            txtLogPath = new TextBox();
            btnBrowseLog = new Button();
            dgvEvents = new DataGridView();
            lblStatus = new Label();
            grpAuth.SuspendLayout();
            grpQuery.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvEvents).BeginInit();
            SuspendLayout();
            //
            // grpAuth
            //
            grpAuth.Controls.Add(rbDelegated);
            grpAuth.Controls.Add(rbApplication);
            grpAuth.Controls.Add(rbMicrosoftApp);
            grpAuth.Controls.Add(lblTenantId);
            grpAuth.Controls.Add(txtTenantId);
            grpAuth.Controls.Add(lblClientId);
            grpAuth.Controls.Add(txtClientId);
            grpAuth.Controls.Add(lblClientSecret);
            grpAuth.Controls.Add(txtClientSecret);
            grpAuth.Controls.Add(lblRedirectUri);
            grpAuth.Controls.Add(txtRedirectUri);
            grpAuth.Controls.Add(lblUpn);
            grpAuth.Controls.Add(txtUpn);
            grpAuth.Controls.Add(btnGetToken);
            grpAuth.Controls.Add(lblTokenStatus);
            grpAuth.Location = new Point(12, 12);
            grpAuth.Name = "grpAuth";
            grpAuth.Size = new Size(860, 210);
            grpAuth.TabIndex = 0;
            grpAuth.TabStop = false;
            grpAuth.Text = "Authentication";
            //
            // rbDelegated
            //
            rbDelegated.AutoSize = true;
            rbDelegated.Checked = true;
            rbDelegated.Location = new Point(15, 25);
            rbDelegated.Name = "rbDelegated";
            rbDelegated.Size = new Size(157, 24);
            rbDelegated.TabIndex = 0;
            rbDelegated.TabStop = true;
            rbDelegated.Text = "Delegated (user)";
            rbDelegated.UseVisualStyleBackColor = true;
            rbDelegated.CheckedChanged += AuthFlowChanged;
            //
            // rbApplication
            //
            rbApplication.AutoSize = true;
            rbApplication.Location = new Point(190, 25);
            rbApplication.Name = "rbApplication";
            rbApplication.Size = new Size(210, 24);
            rbApplication.TabIndex = 1;
            rbApplication.Text = "Application (app-only)";
            rbApplication.UseVisualStyleBackColor = true;
            rbApplication.CheckedChanged += AuthFlowChanged;
            //
            // rbMicrosoftApp
            //
            rbMicrosoftApp.AutoSize = true;
            rbMicrosoftApp.Location = new Point(400, 25);
            rbMicrosoftApp.Name = "rbMicrosoftApp";
            rbMicrosoftApp.Size = new Size(260, 24);
            rbMicrosoftApp.TabIndex = 2;
            rbMicrosoftApp.Text = "Microsoft App ID (delegated)";
            rbMicrosoftApp.UseVisualStyleBackColor = true;
            rbMicrosoftApp.CheckedChanged += AuthFlowChanged;
            //
            // lblTenantId
            //
            lblTenantId.AutoSize = true;
            lblTenantId.Location = new Point(15, 60);
            lblTenantId.Name = "lblTenantId";
            lblTenantId.Size = new Size(75, 20);
            lblTenantId.Text = "Tenant ID";
            //
            // txtTenantId
            //
            txtTenantId.Location = new Point(130, 57);
            txtTenantId.Name = "txtTenantId";
            txtTenantId.Size = new Size(300, 27);
            txtTenantId.TabIndex = 2;
            //
            // lblClientId
            //
            lblClientId.AutoSize = true;
            lblClientId.Location = new Point(15, 93);
            lblClientId.Name = "lblClientId";
            lblClientId.Size = new Size(66, 20);
            lblClientId.Text = "Client ID";
            //
            // txtClientId
            //
            txtClientId.Location = new Point(130, 90);
            txtClientId.Name = "txtClientId";
            txtClientId.Size = new Size(300, 27);
            txtClientId.TabIndex = 3;
            //
            // lblClientSecret
            //
            lblClientSecret.AutoSize = true;
            lblClientSecret.Location = new Point(15, 126);
            lblClientSecret.Name = "lblClientSecret";
            lblClientSecret.Size = new Size(95, 20);
            lblClientSecret.Text = "Client Secret";
            //
            // txtClientSecret
            //
            txtClientSecret.Location = new Point(130, 123);
            txtClientSecret.Name = "txtClientSecret";
            txtClientSecret.UseSystemPasswordChar = true;
            txtClientSecret.Size = new Size(300, 27);
            txtClientSecret.TabIndex = 4;
            //
            // lblRedirectUri
            //
            lblRedirectUri.AutoSize = true;
            lblRedirectUri.Location = new Point(460, 60);
            lblRedirectUri.Name = "lblRedirectUri";
            lblRedirectUri.Size = new Size(90, 20);
            lblRedirectUri.Text = "Redirect URI";
            //
            // txtRedirectUri
            //
            txtRedirectUri.Location = new Point(560, 57);
            txtRedirectUri.Name = "txtRedirectUri";
            txtRedirectUri.Text = "http://localhost";
            txtRedirectUri.Size = new Size(280, 27);
            txtRedirectUri.TabIndex = 5;
            //
            // lblUpn
            //
            lblUpn.AutoSize = true;
            lblUpn.Location = new Point(460, 93);
            lblUpn.Name = "lblUpn";
            lblUpn.Size = new Size(100, 20);
            lblUpn.Text = "Mailbox UPN";
            //
            // txtUpn
            //
            txtUpn.Location = new Point(560, 90);
            txtUpn.Name = "txtUpn";
            txtUpn.Enabled = false;
            txtUpn.Size = new Size(280, 27);
            txtUpn.TabIndex = 6;
            //
            // btnGetToken
            //
            btnGetToken.Location = new Point(130, 160);
            btnGetToken.Name = "btnGetToken";
            btnGetToken.Size = new Size(140, 35);
            btnGetToken.TabIndex = 7;
            btnGetToken.Text = "Get Token";
            btnGetToken.UseVisualStyleBackColor = true;
            btnGetToken.Click += btnGetToken_Click;
            //
            // lblTokenStatus
            //
            lblTokenStatus.AutoSize = true;
            lblTokenStatus.ForeColor = Color.DimGray;
            lblTokenStatus.Location = new Point(290, 168);
            lblTokenStatus.Name = "lblTokenStatus";
            lblTokenStatus.Size = new Size(120, 20);
            lblTokenStatus.Text = "No token acquired";
            //
            // grpQuery
            //
            grpQuery.Controls.Add(lblStart);
            grpQuery.Controls.Add(dtpStart);
            grpQuery.Controls.Add(lblEnd);
            grpQuery.Controls.Add(dtpEnd);
            grpQuery.Controls.Add(btnFetch);
            grpQuery.Controls.Add(btnCheck);
            grpQuery.Controls.Add(lblLogPath);
            grpQuery.Controls.Add(txtLogPath);
            grpQuery.Controls.Add(btnBrowseLog);
            grpQuery.Location = new Point(12, 228);
            grpQuery.Name = "grpQuery";
            grpQuery.Size = new Size(860, 130);
            grpQuery.TabIndex = 1;
            grpQuery.TabStop = false;
            grpQuery.Text = "Calendar Query";
            //
            // lblStart
            //
            lblStart.AutoSize = true;
            lblStart.Location = new Point(15, 33);
            lblStart.Name = "lblStart";
            lblStart.Size = new Size(75, 20);
            lblStart.Text = "Start Date";
            //
            // dtpStart
            //
            dtpStart.Format = DateTimePickerFormat.Custom;
            dtpStart.CustomFormat = "yyyy-MM-dd HH:mm";
            dtpStart.ShowUpDown = true;
            dtpStart.Location = new Point(95, 30);
            dtpStart.Name = "dtpStart";
            dtpStart.Size = new Size(180, 27);
            dtpStart.TabIndex = 0;
            dtpStart.ValueChanged += StartDateChanged;
            //
            // lblEnd
            //
            lblEnd.AutoSize = true;
            lblEnd.Location = new Point(295, 33);
            lblEnd.Name = "lblEnd";
            lblEnd.Size = new Size(70, 20);
            lblEnd.Text = "End Date";
            //
            // dtpEnd
            //
            dtpEnd.Format = DateTimePickerFormat.Custom;
            dtpEnd.CustomFormat = "yyyy-MM-dd HH:mm";
            dtpEnd.ShowUpDown = true;
            dtpEnd.Location = new Point(370, 30);
            dtpEnd.Name = "dtpEnd";
            dtpEnd.Size = new Size(180, 27);
            dtpEnd.TabIndex = 1;
            //
            // btnFetch
            //
            btnFetch.Location = new Point(580, 25);
            btnFetch.Name = "btnFetch";
            btnFetch.Size = new Size(120, 35);
            btnFetch.TabIndex = 2;
            btnFetch.Text = "Fetch List";
            btnFetch.Enabled = false;
            btnFetch.UseVisualStyleBackColor = true;
            btnFetch.Click += btnFetch_Click;
            //
            // btnCheck
            //
            btnCheck.Location = new Point(710, 25);
            btnCheck.Name = "btnCheck";
            btnCheck.Size = new Size(130, 35);
            btnCheck.TabIndex = 3;
            btnCheck.Text = "Check";
            btnCheck.Enabled = false;
            btnCheck.UseVisualStyleBackColor = true;
            btnCheck.Click += btnCheck_Click;
            //
            // lblLogPath
            //
            lblLogPath.AutoSize = true;
            lblLogPath.Location = new Point(15, 83);
            lblLogPath.Name = "lblLogPath";
            lblLogPath.Size = new Size(60, 20);
            lblLogPath.Text = "Log File";
            //
            // txtLogPath
            //
            txtLogPath.Location = new Point(95, 80);
            txtLogPath.Name = "txtLogPath";
            txtLogPath.Size = new Size(605, 27);
            txtLogPath.TabIndex = 4;
            //
            // btnBrowseLog
            //
            btnBrowseLog.Location = new Point(710, 78);
            btnBrowseLog.Name = "btnBrowseLog";
            btnBrowseLog.Size = new Size(130, 32);
            btnBrowseLog.TabIndex = 5;
            btnBrowseLog.Text = "Browse...";
            btnBrowseLog.UseVisualStyleBackColor = true;
            btnBrowseLog.Click += btnBrowseLog_Click;
            //
            // dgvEvents
            //
            dgvEvents.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvEvents.AllowUserToAddRows = false;
            dgvEvents.ReadOnly = true;
            dgvEvents.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvEvents.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dgvEvents.Location = new Point(12, 364);
            dgvEvents.Name = "dgvEvents";
            dgvEvents.RowHeadersWidth = 51;
            dgvEvents.Size = new Size(860, 290);
            dgvEvents.TabIndex = 2;
            //
            // lblStatus
            //
            lblStatus.AutoSize = true;
            lblStatus.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            lblStatus.Location = new Point(12, 662);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(50, 20);
            lblStatus.Text = "Ready";
            //
            // Form1
            //
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(884, 691);
            Controls.Add(grpAuth);
            Controls.Add(grpQuery);
            Controls.Add(dgvEvents);
            Controls.Add(lblStatus);
            MinimumSize = new Size(902, 738);
            Name = "Form1";
            Text = "Calendar Expansion Visualizer";
            grpAuth.ResumeLayout(false);
            grpAuth.PerformLayout();
            grpQuery.ResumeLayout(false);
            grpQuery.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dgvEvents).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private GroupBox grpAuth;
        private RadioButton rbDelegated;
        private RadioButton rbApplication;
        private RadioButton rbMicrosoftApp;
        private Label lblTenantId;
        private TextBox txtTenantId;
        private Label lblClientId;
        private TextBox txtClientId;
        private Label lblClientSecret;
        private TextBox txtClientSecret;
        private Label lblRedirectUri;
        private TextBox txtRedirectUri;
        private Label lblUpn;
        private TextBox txtUpn;
        private Button btnGetToken;
        private Label lblTokenStatus;
        private GroupBox grpQuery;
        private Label lblStart;
        private DateTimePicker dtpStart;
        private Label lblEnd;
        private DateTimePicker dtpEnd;
        private Button btnFetch;
        private Button btnCheck;
        private Label lblLogPath;
        private TextBox txtLogPath;
        private Button btnBrowseLog;
        private DataGridView dgvEvents;
        private Label lblStatus;
    }
}
