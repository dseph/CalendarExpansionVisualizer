namespace CalendarExpansionVisualizer
{
    partial class frmMain
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
            components = new System.ComponentModel.Container();
            grpAuth = new GroupBox();
            label1 = new Label();
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
            lblCloud = new Label();
            cboCloud = new ComboBox();
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
            chkEnableLog = new CheckBox();
            btnTests = new Button();
            dgvEvents = new DataGridView();
            cmsEvents = new ContextMenuStrip(components);
            mnuShowGeneratedIcs = new ToolStripMenuItem();
            mnuEventInformation = new ToolStripMenuItem();
            lblStatus = new Label();
            grpAuth.SuspendLayout();
            grpQuery.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvEvents).BeginInit();
            cmsEvents.SuspendLayout();
            SuspendLayout();
            // 
            // grpAuth
            // 
            grpAuth.Controls.Add(label1);
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
            grpAuth.Controls.Add(lblCloud);
            grpAuth.Controls.Add(cboCloud);
            grpAuth.Location = new Point(15, 15);
            grpAuth.Margin = new Padding(4);
            grpAuth.Name = "grpAuth";
            grpAuth.Padding = new Padding(4);
            grpAuth.Size = new Size(1319, 262);
            grpAuth.TabIndex = 0;
            grpAuth.TabStop = false;
            grpAuth.Text = "Authentication";
            grpAuth.Enter += grpAuth_Enter;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(634, 160);
            label1.Margin = new Padding(4, 0, 4, 0);
            label1.Name = "label1";
            label1.Size = new Size(417, 25);
            label1.TabIndex = 15;
            label1.Text = "Note: Calendars.Read Graph is permission required.";
            // 
            // rbDelegated
            // 
            rbDelegated.AutoSize = true;
            rbDelegated.Location = new Point(19, 31);
            rbDelegated.Margin = new Padding(4);
            rbDelegated.Name = "rbDelegated";
            rbDelegated.Size = new Size(166, 29);
            rbDelegated.TabIndex = 0;
            rbDelegated.Text = "Delegated (user)";
            rbDelegated.UseVisualStyleBackColor = true;
            rbDelegated.CheckedChanged += AuthFlowChanged;
            // 
            // rbApplication
            // 
            rbApplication.AutoSize = true;
            rbApplication.Location = new Point(238, 31);
            rbApplication.Margin = new Padding(4);
            rbApplication.Name = "rbApplication";
            rbApplication.Size = new Size(214, 29);
            rbApplication.TabIndex = 1;
            rbApplication.Text = "Application (app-only)";
            rbApplication.UseVisualStyleBackColor = true;
            rbApplication.CheckedChanged += AuthFlowChanged;
            // 
            // rbMicrosoftApp
            // 
            rbMicrosoftApp.AutoSize = true;
            rbMicrosoftApp.Checked = true;
            rbMicrosoftApp.Location = new Point(500, 31);
            rbMicrosoftApp.Margin = new Padding(4);
            rbMicrosoftApp.Name = "rbMicrosoftApp";
            rbMicrosoftApp.Size = new Size(269, 29);
            rbMicrosoftApp.TabIndex = 2;
            rbMicrosoftApp.TabStop = true;
            rbMicrosoftApp.Text = "Microsoft App ID (delegated)";
            rbMicrosoftApp.UseVisualStyleBackColor = true;
            rbMicrosoftApp.CheckedChanged += AuthFlowChanged;
            // 
            // lblTenantId
            // 
            lblTenantId.AutoSize = true;
            lblTenantId.Location = new Point(19, 75);
            lblTenantId.Margin = new Padding(4, 0, 4, 0);
            lblTenantId.Name = "lblTenantId";
            lblTenantId.Size = new Size(86, 25);
            lblTenantId.TabIndex = 5;
            lblTenantId.Text = "Tenant ID";
            // 
            // txtTenantId
            // 
            txtTenantId.Location = new Point(162, 71);
            txtTenantId.Margin = new Padding(4);
            txtTenantId.Name = "txtTenantId";
            txtTenantId.Size = new Size(448, 31);
            txtTenantId.TabIndex = 6;
            // 
            // lblClientId
            // 
            lblClientId.AutoSize = true;
            lblClientId.Location = new Point(19, 116);
            lblClientId.Margin = new Padding(4, 0, 4, 0);
            lblClientId.Name = "lblClientId";
            lblClientId.Size = new Size(79, 25);
            lblClientId.TabIndex = 7;
            lblClientId.Text = "Client ID";
            // 
            // txtClientId
            // 
            txtClientId.Location = new Point(162, 112);
            txtClientId.Margin = new Padding(4);
            txtClientId.Name = "txtClientId";
            txtClientId.Size = new Size(448, 31);
            txtClientId.TabIndex = 8;
            // 
            // lblClientSecret
            // 
            lblClientSecret.AutoSize = true;
            lblClientSecret.Location = new Point(19, 158);
            lblClientSecret.Margin = new Padding(4, 0, 4, 0);
            lblClientSecret.Name = "lblClientSecret";
            lblClientSecret.Size = new Size(109, 25);
            lblClientSecret.TabIndex = 9;
            lblClientSecret.Text = "Client Secret";
            // 
            // txtClientSecret
            // 
            txtClientSecret.Location = new Point(162, 154);
            txtClientSecret.Margin = new Padding(4);
            txtClientSecret.Name = "txtClientSecret";
            txtClientSecret.Size = new Size(448, 31);
            txtClientSecret.TabIndex = 10;
            txtClientSecret.UseSystemPasswordChar = true;
            // 
            // lblRedirectUri
            // 
            lblRedirectUri.AutoSize = true;
            lblRedirectUri.Location = new Point(638, 75);
            lblRedirectUri.Margin = new Padding(4, 0, 4, 0);
            lblRedirectUri.Name = "lblRedirectUri";
            lblRedirectUri.Size = new Size(108, 25);
            lblRedirectUri.TabIndex = 11;
            lblRedirectUri.Text = "Redirect URI";
            // 
            // txtRedirectUri
            // 
            txtRedirectUri.Location = new Point(763, 71);
            txtRedirectUri.Margin = new Padding(4);
            txtRedirectUri.Name = "txtRedirectUri";
            txtRedirectUri.Size = new Size(527, 31);
            txtRedirectUri.TabIndex = 12;
            txtRedirectUri.Text = "http://localhost";
            // 
            // lblUpn
            // 
            lblUpn.AutoSize = true;
            lblUpn.Location = new Point(638, 116);
            lblUpn.Margin = new Padding(4, 0, 4, 0);
            lblUpn.Name = "lblUpn";
            lblUpn.Size = new Size(115, 25);
            lblUpn.TabIndex = 13;
            lblUpn.Text = "Mailbox UPN";
            // 
            // txtUpn
            // 
            txtUpn.Enabled = false;
            txtUpn.Location = new Point(763, 112);
            txtUpn.Margin = new Padding(4);
            txtUpn.Name = "txtUpn";
            txtUpn.Size = new Size(527, 31);
            txtUpn.TabIndex = 14;
            // 
            // btnGetToken
            // 
            btnGetToken.Location = new Point(162, 200);
            btnGetToken.Margin = new Padding(4);
            btnGetToken.Name = "btnGetToken";
            btnGetToken.Size = new Size(175, 44);
            btnGetToken.TabIndex = 16;
            btnGetToken.Text = "Get Token";
            btnGetToken.UseVisualStyleBackColor = true;
            btnGetToken.Click += btnGetToken_Click;
            // 
            // lblTokenStatus
            // 
            lblTokenStatus.AutoSize = true;
            lblTokenStatus.ForeColor = Color.DimGray;
            lblTokenStatus.Location = new Point(362, 210);
            lblTokenStatus.Margin = new Padding(4, 0, 4, 0);
            lblTokenStatus.Name = "lblTokenStatus";
            lblTokenStatus.Size = new Size(159, 25);
            lblTokenStatus.TabIndex = 8;
            lblTokenStatus.Text = "No token acquired";
            // 
            // lblCloud
            // 
            lblCloud.AutoSize = true;
            lblCloud.Location = new Point(799, 31);
            lblCloud.Margin = new Padding(4, 0, 4, 0);
            lblCloud.Name = "lblCloud";
            lblCloud.Size = new Size(59, 25);
            lblCloud.TabIndex = 3;
            lblCloud.Text = "Cloud";
            // 
            // cboCloud
            // 
            cboCloud.DropDownStyle = ComboBoxStyle.DropDownList;
            cboCloud.FormattingEnabled = true;
            cboCloud.Items.AddRange(new object[] { "Commercial", "GCC", "GCC High", "DoD" });
            cboCloud.Location = new Point(869, 27);
            cboCloud.Margin = new Padding(4);
            cboCloud.Name = "cboCloud";
            cboCloud.Size = new Size(195, 33);
            cboCloud.TabIndex = 4;
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
            grpQuery.Controls.Add(chkEnableLog);
            grpQuery.Controls.Add(btnTests);
            grpQuery.Location = new Point(15, 285);
            grpQuery.Margin = new Padding(4);
            grpQuery.Name = "grpQuery";
            grpQuery.Padding = new Padding(4);
            grpQuery.Size = new Size(1319, 134);
            grpQuery.TabIndex = 1;
            grpQuery.TabStop = false;
            grpQuery.Text = "Calendar Query";
            // 
            // lblStart
            // 
            lblStart.AutoSize = true;
            lblStart.Location = new Point(19, 41);
            lblStart.Margin = new Padding(4, 0, 4, 0);
            lblStart.Name = "lblStart";
            lblStart.Size = new Size(90, 25);
            lblStart.TabIndex = 0;
            lblStart.Text = "Start Date";
            // 
            // dtpStart
            // 
            dtpStart.Format = DateTimePickerFormat.Short;
            dtpStart.Location = new Point(119, 38);
            dtpStart.Margin = new Padding(4);
            dtpStart.Name = "dtpStart";
            dtpStart.Size = new Size(224, 31);
            dtpStart.TabIndex = 1;
            dtpStart.ValueChanged += StartDateChanged;
            // 
            // lblEnd
            // 
            lblEnd.AutoSize = true;
            lblEnd.Location = new Point(369, 41);
            lblEnd.Margin = new Padding(4, 0, 4, 0);
            lblEnd.Name = "lblEnd";
            lblEnd.Size = new Size(84, 25);
            lblEnd.TabIndex = 2;
            lblEnd.Text = "End Date";
            // 
            // dtpEnd
            // 
            dtpEnd.Format = DateTimePickerFormat.Short;
            dtpEnd.Location = new Point(462, 38);
            dtpEnd.Margin = new Padding(4);
            dtpEnd.Name = "dtpEnd";
            dtpEnd.Size = new Size(224, 31);
            dtpEnd.TabIndex = 3;
            // 
            // btnFetch
            // 
            btnFetch.Enabled = false;
            btnFetch.Location = new Point(792, 32);
            btnFetch.Margin = new Padding(4);
            btnFetch.Name = "btnFetch";
            btnFetch.Size = new Size(150, 44);
            btnFetch.TabIndex = 4;
            btnFetch.Text = "Fetch List";
            btnFetch.UseVisualStyleBackColor = true;
            btnFetch.Click += btnFetch_Click;
            // 
            // btnCheck
            // 
            btnCheck.Enabled = false;
            btnCheck.Location = new Point(956, 33);
            btnCheck.Margin = new Padding(4);
            btnCheck.Name = "btnCheck";
            btnCheck.Size = new Size(162, 44);
            btnCheck.TabIndex = 5;
            btnCheck.Text = "Check";
            btnCheck.UseVisualStyleBackColor = true;
            btnCheck.Click += btnCheck_Click;
            // 
            // lblLogPath
            // 
            lblLogPath.AutoSize = true;
            lblLogPath.Location = new Point(19, 92);
            lblLogPath.Margin = new Padding(4, 0, 4, 0);
            lblLogPath.Name = "lblLogPath";
            lblLogPath.Size = new Size(73, 25);
            lblLogPath.TabIndex = 7;
            lblLogPath.Text = "Log File";
            // 
            // txtLogPath
            // 
            txtLogPath.Location = new Point(119, 88);
            txtLogPath.Margin = new Padding(4);
            txtLogPath.Name = "txtLogPath";
            txtLogPath.Size = new Size(755, 31);
            txtLogPath.TabIndex = 8;
            // 
            // btnBrowseLog
            // 
            btnBrowseLog.Location = new Point(888, 83);
            btnBrowseLog.Margin = new Padding(4);
            btnBrowseLog.Name = "btnBrowseLog";
            btnBrowseLog.Size = new Size(162, 40);
            btnBrowseLog.TabIndex = 9;
            btnBrowseLog.Text = "Browse...";
            btnBrowseLog.UseVisualStyleBackColor = true;
            btnBrowseLog.Click += btnBrowseLog_Click;
            // 
            // chkEnableLog
            // 
            chkEnableLog.AutoSize = true;
            chkEnableLog.Location = new Point(1058, 92);
            chkEnableLog.Margin = new Padding(4);
            chkEnableLog.Name = "chkEnableLog";
            chkEnableLog.Size = new Size(157, 29);
            chkEnableLog.TabIndex = 10;
            chkEnableLog.Text = "Enable logging";
            chkEnableLog.UseVisualStyleBackColor = true;
            // 
            // btnTests
            // 
            btnTests.Enabled = false;
            btnTests.Location = new Point(1137, 32);
            btnTests.Margin = new Padding(4);
            btnTests.Name = "btnTests";
            btnTests.Size = new Size(174, 44);
            btnTests.TabIndex = 6;
            btnTests.Text = "Tests";
            btnTests.UseVisualStyleBackColor = true;
            btnTests.Click += btnTests_Click;
            // 
            // dgvEvents
            // 
            dgvEvents.AllowUserToAddRows = false;
            dgvEvents.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dgvEvents.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvEvents.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvEvents.ContextMenuStrip = cmsEvents;
            dgvEvents.Location = new Point(17, 427);
            dgvEvents.Margin = new Padding(4);
            dgvEvents.Name = "dgvEvents";
            dgvEvents.ReadOnly = true;
            dgvEvents.RowHeadersWidth = 51;
            dgvEvents.Size = new Size(1319, 454);
            dgvEvents.TabIndex = 2;
            dgvEvents.CellMouseDown += dgvEvents_CellMouseDown;
            // 
            // cmsEvents
            // 
            cmsEvents.ImageScalingSize = new Size(24, 24);
            cmsEvents.Items.AddRange(new ToolStripItem[] { mnuShowGeneratedIcs, mnuEventInformation });
            cmsEvents.Name = "cmsEvents";
            cmsEvents.Size = new Size(246, 68);
            cmsEvents.Closed += cmsEvents_Closed;
            cmsEvents.Opening += cmsEvents_Opening;
            // 
            // mnuShowGeneratedIcs
            // 
            mnuShowGeneratedIcs.Name = "mnuShowGeneratedIcs";
            mnuShowGeneratedIcs.Size = new Size(245, 32);
            mnuShowGeneratedIcs.Text = "Show Generated ICS";
            mnuShowGeneratedIcs.Click += mnuShowGeneratedIcs_Click;
            // 
            // mnuEventInformation
            // 
            mnuEventInformation.Name = "mnuEventInformation";
            mnuEventInformation.Size = new Size(245, 32);
            mnuEventInformation.Text = "Event Information";
            mnuEventInformation.Click += mnuEventInformation_Click;
            // 
            // lblStatus
            // 
            lblStatus.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            lblStatus.AutoSize = true;
            lblStatus.Location = new Point(15, 885);
            lblStatus.Margin = new Padding(4, 0, 4, 0);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(60, 25);
            lblStatus.TabIndex = 3;
            lblStatus.Text = "Ready";
            // 
            // frmMain
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1349, 921);
            Controls.Add(grpAuth);
            Controls.Add(grpQuery);
            Controls.Add(dgvEvents);
            Controls.Add(lblStatus);
            Margin = new Padding(4);
            MinimumSize = new Size(1122, 966);
            Name = "frmMain";
            Text = "Calendar Expansion Visualizer";
            Load += frmMain_Load;
            grpAuth.ResumeLayout(false);
            grpAuth.PerformLayout();
            grpQuery.ResumeLayout(false);
            grpQuery.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dgvEvents).EndInit();
            cmsEvents.ResumeLayout(false);
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
        private Label lblCloud;
        private ComboBox cboCloud;
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
        private CheckBox chkEnableLog;
        private Button btnTests;
        private DataGridView dgvEvents;
        private ContextMenuStrip cmsEvents;
        private ToolStripMenuItem mnuShowGeneratedIcs;
        private ToolStripMenuItem mnuEventInformation;
        private Label lblStatus;
        private Label label1;
    }
}
