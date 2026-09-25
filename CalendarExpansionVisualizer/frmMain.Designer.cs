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
            lblElapsed = new Label();
            lblLogPath = new Label();
            txtLogPath = new TextBox();
            btnBrowseLog = new Button();
            chkEnableLog = new CheckBox();
            lblListMode = new Label();
            rbListExceptions = new RadioButton();
            rbListDeletions = new RadioButton();
            rbListBoth = new RadioButton();
            btnList = new Button();
            btnTests = new Button();
            dgvEvents = new DataGridView();
            lblStatus = new Label();
            grpAuth.SuspendLayout();
            grpQuery.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvEvents).BeginInit();
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
            grpAuth.Size = new Size(1213, 262);
            grpAuth.TabIndex = 0;
            grpAuth.TabStop = false;
            grpAuth.Text = "Authentication";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(571, 160);
            label1.Margin = new Padding(4, 0, 4, 0);
            label1.Name = "label1";
            label1.Size = new Size(417, 25);
            label1.TabIndex = 11;
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
            lblTenantId.TabIndex = 3;
            lblTenantId.Text = "Tenant ID";
            // 
            // txtTenantId
            // 
            txtTenantId.Location = new Point(162, 71);
            txtTenantId.Margin = new Padding(4);
            txtTenantId.Name = "txtTenantId";
            txtTenantId.Size = new Size(374, 31);
            txtTenantId.TabIndex = 2;
            // 
            // lblClientId
            // 
            lblClientId.AutoSize = true;
            lblClientId.Location = new Point(19, 116);
            lblClientId.Margin = new Padding(4, 0, 4, 0);
            lblClientId.Name = "lblClientId";
            lblClientId.Size = new Size(79, 25);
            lblClientId.TabIndex = 4;
            lblClientId.Text = "Client ID";
            // 
            // txtClientId
            // 
            txtClientId.Location = new Point(162, 112);
            txtClientId.Margin = new Padding(4);
            txtClientId.Name = "txtClientId";
            txtClientId.Size = new Size(374, 31);
            txtClientId.TabIndex = 3;
            // 
            // lblClientSecret
            // 
            lblClientSecret.AutoSize = true;
            lblClientSecret.Location = new Point(19, 158);
            lblClientSecret.Margin = new Padding(4, 0, 4, 0);
            lblClientSecret.Name = "lblClientSecret";
            lblClientSecret.Size = new Size(109, 25);
            lblClientSecret.TabIndex = 5;
            lblClientSecret.Text = "Client Secret";
            // 
            // txtClientSecret
            // 
            txtClientSecret.Location = new Point(162, 154);
            txtClientSecret.Margin = new Padding(4);
            txtClientSecret.Name = "txtClientSecret";
            txtClientSecret.Size = new Size(374, 31);
            txtClientSecret.TabIndex = 4;
            txtClientSecret.UseSystemPasswordChar = true;
            // 
            // lblRedirectUri
            // 
            lblRedirectUri.AutoSize = true;
            lblRedirectUri.Location = new Point(575, 75);
            lblRedirectUri.Margin = new Padding(4, 0, 4, 0);
            lblRedirectUri.Name = "lblRedirectUri";
            lblRedirectUri.Size = new Size(108, 25);
            lblRedirectUri.TabIndex = 6;
            lblRedirectUri.Text = "Redirect URI";
            // 
            // txtRedirectUri
            // 
            txtRedirectUri.Location = new Point(700, 71);
            txtRedirectUri.Margin = new Padding(4);
            txtRedirectUri.Name = "txtRedirectUri";
            txtRedirectUri.Size = new Size(349, 31);
            txtRedirectUri.TabIndex = 5;
            txtRedirectUri.Text = "http://localhost";
            // 
            // lblUpn
            // 
            lblUpn.AutoSize = true;
            lblUpn.Location = new Point(575, 116);
            lblUpn.Margin = new Padding(4, 0, 4, 0);
            lblUpn.Name = "lblUpn";
            lblUpn.Size = new Size(115, 25);
            lblUpn.TabIndex = 7;
            lblUpn.Text = "Mailbox UPN";
            // 
            // txtUpn
            // 
            txtUpn.Enabled = false;
            txtUpn.Location = new Point(700, 112);
            txtUpn.Margin = new Padding(4);
            txtUpn.Name = "txtUpn";
            txtUpn.Size = new Size(349, 31);
            txtUpn.TabIndex = 6;
            // 
            // btnGetToken
            // 
            btnGetToken.Location = new Point(162, 200);
            btnGetToken.Margin = new Padding(4);
            btnGetToken.Name = "btnGetToken";
            btnGetToken.Size = new Size(175, 44);
            btnGetToken.TabIndex = 7;
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
            lblCloud.TabIndex = 9;
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
            cboCloud.TabIndex = 10;
            // 
            // grpQuery
            // 
            grpQuery.Controls.Add(lblStart);
            grpQuery.Controls.Add(dtpStart);
            grpQuery.Controls.Add(lblEnd);
            grpQuery.Controls.Add(dtpEnd);
            grpQuery.Controls.Add(btnFetch);
            grpQuery.Controls.Add(btnCheck);
            grpQuery.Controls.Add(lblElapsed);
            grpQuery.Controls.Add(lblLogPath);
            grpQuery.Controls.Add(txtLogPath);
            grpQuery.Controls.Add(btnBrowseLog);
            grpQuery.Controls.Add(chkEnableLog);
            grpQuery.Controls.Add(lblListMode);
            grpQuery.Controls.Add(rbListExceptions);
            grpQuery.Controls.Add(rbListDeletions);
            grpQuery.Controls.Add(rbListBoth);
            grpQuery.Controls.Add(btnList);
            grpQuery.Controls.Add(btnTests);
            grpQuery.Location = new Point(15, 285);
            grpQuery.Margin = new Padding(4);
            grpQuery.Name = "grpQuery";
            grpQuery.Padding = new Padding(4);
            grpQuery.Size = new Size(1215, 181);
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
            dtpStart.TabIndex = 0;
            dtpStart.ValueChanged += StartDateChanged;
            // 
            // lblEnd
            // 
            lblEnd.AutoSize = true;
            lblEnd.Location = new Point(369, 41);
            lblEnd.Margin = new Padding(4, 0, 4, 0);
            lblEnd.Name = "lblEnd";
            lblEnd.Size = new Size(84, 25);
            lblEnd.TabIndex = 1;
            lblEnd.Text = "End Date";
            // 
            // dtpEnd
            // 
            dtpEnd.Format = DateTimePickerFormat.Short;
            dtpEnd.Location = new Point(462, 38);
            dtpEnd.Margin = new Padding(4);
            dtpEnd.Name = "dtpEnd";
            dtpEnd.Size = new Size(224, 31);
            dtpEnd.TabIndex = 1;
            // 
            // btnFetch
            // 
            btnFetch.Enabled = false;
            btnFetch.Location = new Point(725, 31);
            btnFetch.Margin = new Padding(4);
            btnFetch.Name = "btnFetch";
            btnFetch.Size = new Size(150, 44);
            btnFetch.TabIndex = 2;
            btnFetch.Text = "Fetch List";
            btnFetch.UseVisualStyleBackColor = true;
            btnFetch.Click += btnFetch_Click;
            // 
            // btnCheck
            // 
            btnCheck.Enabled = false;
            btnCheck.Location = new Point(888, 31);
            btnCheck.Margin = new Padding(4);
            btnCheck.Name = "btnCheck";
            btnCheck.Size = new Size(162, 44);
            btnCheck.TabIndex = 3;
            btnCheck.Text = "Check";
            btnCheck.UseVisualStyleBackColor = true;
            btnCheck.Click += btnCheck_Click;
            // 
            // lblElapsed
            // 
            lblElapsed.AutoSize = true;
            lblElapsed.Location = new Point(1060, 41);
            lblElapsed.Margin = new Padding(4, 0, 4, 0);
            lblElapsed.Name = "lblElapsed";
            lblElapsed.Size = new Size(0, 25);
            lblElapsed.TabIndex = 11;
            // 
            // lblLogPath
            // 
            lblLogPath.AutoSize = true;
            lblLogPath.Location = new Point(19, 92);
            lblLogPath.Margin = new Padding(4, 0, 4, 0);
            lblLogPath.Name = "lblLogPath";
            lblLogPath.Size = new Size(73, 25);
            lblLogPath.TabIndex = 4;
            lblLogPath.Text = "Log File";
            // 
            // txtLogPath
            // 
            txtLogPath.Location = new Point(119, 88);
            txtLogPath.Margin = new Padding(4);
            txtLogPath.Name = "txtLogPath";
            txtLogPath.Size = new Size(755, 31);
            txtLogPath.TabIndex = 4;
            // 
            // btnBrowseLog
            // 
            btnBrowseLog.Location = new Point(888, 83);
            btnBrowseLog.Margin = new Padding(4);
            btnBrowseLog.Name = "btnBrowseLog";
            btnBrowseLog.Size = new Size(162, 40);
            btnBrowseLog.TabIndex = 5;
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
            chkEnableLog.TabIndex = 6;
            chkEnableLog.Text = "Enable logging";
            chkEnableLog.UseVisualStyleBackColor = true;
            // 
            // lblListMode
            // 
            lblListMode.AutoSize = true;
            lblListMode.Location = new Point(19, 139);
            lblListMode.Margin = new Padding(4, 0, 4, 0);
            lblListMode.Name = "lblListMode";
            lblListMode.Size = new Size(153, 25);
            lblListMode.TabIndex = 7;
            lblListMode.Text = "List (whole series):";
            // 
            // rbListExceptions
            // 
            rbListExceptions.AutoSize = true;
            rbListExceptions.Checked = true;
            rbListExceptions.Location = new Point(188, 137);
            rbListExceptions.Margin = new Padding(4);
            rbListExceptions.Name = "rbListExceptions";
            rbListExceptions.Size = new Size(121, 29);
            rbListExceptions.TabIndex = 7;
            rbListExceptions.TabStop = true;
            rbListExceptions.Text = "Exceptions";
            rbListExceptions.UseVisualStyleBackColor = true;
            // 
            // rbListDeletions
            // 
            rbListDeletions.AutoSize = true;
            rbListDeletions.Location = new Point(338, 137);
            rbListDeletions.Margin = new Padding(4);
            rbListDeletions.Name = "rbListDeletions";
            rbListDeletions.Size = new Size(111, 29);
            rbListDeletions.TabIndex = 8;
            rbListDeletions.Text = "Deletions";
            rbListDeletions.UseVisualStyleBackColor = true;
            // 
            // rbListBoth
            // 
            rbListBoth.AutoSize = true;
            rbListBoth.Location = new Point(475, 137);
            rbListBoth.Margin = new Padding(4);
            rbListBoth.Name = "rbListBoth";
            rbListBoth.Size = new Size(235, 29);
            rbListBoth.TabIndex = 9;
            rbListBoth.Text = "Exceptions and Deletions";
            rbListBoth.UseVisualStyleBackColor = true;
            // 
            // btnList
            // 
            btnList.Enabled = false;
            btnList.Location = new Point(748, 127);
            btnList.Margin = new Padding(4);
            btnList.Name = "btnList";
            btnList.Size = new Size(162, 40);
            btnList.TabIndex = 10;
            btnList.Text = "List";
            btnList.UseVisualStyleBackColor = true;
            btnList.Click += btnList_Click;
            // 
            // btnTests
            // 
            btnTests.Enabled = false;
            btnTests.Location = new Point(972, 133);
            btnTests.Margin = new Padding(4);
            btnTests.Name = "btnTests";
            btnTests.Size = new Size(162, 40);
            btnTests.TabIndex = 11;
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
            dgvEvents.Location = new Point(15, 474);
            dgvEvents.Margin = new Padding(4);
            dgvEvents.Name = "dgvEvents";
            dgvEvents.ReadOnly = true;
            dgvEvents.RowHeadersWidth = 51;
            dgvEvents.Size = new Size(1213, 407);
            dgvEvents.TabIndex = 2;
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
            ClientSize = new Size(1243, 921);
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
        private Label lblElapsed;
        private Label lblLogPath;
        private TextBox txtLogPath;
        private Button btnBrowseLog;
        private CheckBox chkEnableLog;
        private Label lblListMode;
        private RadioButton rbListExceptions;
        private RadioButton rbListDeletions;
        private RadioButton rbListBoth;
        private Button btnList;
        private Button btnTests;
        private DataGridView dgvEvents;
        private Label lblStatus;
        private Label label1;
    }
}
