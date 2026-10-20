namespace CalendarExpansionVisualizer
{
    partial class frmTests
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
            grpInputs = new GroupBox();
            lblContext = new Label();
            lblStart = new Label();
            dtpStart = new DateTimePicker();
            lblEnd = new Label();
            dtpEnd = new DateTimePicker();
            lblSubject = new Label();
            txtSubject = new TextBox();
            lblInterval = new Label();
            numInterval = new NumericUpDown();
            lblEventId = new Label();
            txtEventId = new TextBox();
            lblSchedules = new Label();
            txtSchedules = new TextBox();
            grpTests = new GroupBox();
            rbGetSchedule = new RadioButton();
            rbCalendarView = new RadioButton();
            rbSubjectInRange = new RadioButton();
            rbSubjectAll = new RadioButton();
            rbInstances = new RadioButton();
            rbDelta = new RadioButton();
            chkShowResults = new CheckBox();
            btnRun = new Button();
            lblStartedCaption = new Label();
            lblStarted = new Label();
            lblEndedCaption = new Label();
            lblEnded = new Label();
            lblElapsedCaption = new Label();
            lblElapsed = new Label();
            txtResults = new RichTextBox();
            grpInputs.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numInterval).BeginInit();
            grpTests.SuspendLayout();
            SuspendLayout();
            // 
            // grpInputs
            // 
            grpInputs.Controls.Add(lblContext);
            grpInputs.Controls.Add(lblStart);
            grpInputs.Controls.Add(dtpStart);
            grpInputs.Controls.Add(lblEnd);
            grpInputs.Controls.Add(dtpEnd);
            grpInputs.Controls.Add(lblSubject);
            grpInputs.Controls.Add(txtSubject);
            grpInputs.Controls.Add(lblInterval);
            grpInputs.Controls.Add(numInterval);
            grpInputs.Controls.Add(lblEventId);
            grpInputs.Controls.Add(txtEventId);
            grpInputs.Controls.Add(lblSchedules);
            grpInputs.Controls.Add(txtSchedules);
            grpInputs.Location = new Point(15, 15);
            grpInputs.Name = "grpInputs";
            grpInputs.Padding = new Padding(4);
            grpInputs.Size = new Size(700, 432);
            grpInputs.TabIndex = 0;
            grpInputs.TabStop = false;
            grpInputs.Text = "Inputs";
            // 
            // lblContext
            // 
            lblContext.AutoSize = true;
            lblContext.ForeColor = Color.DimGray;
            lblContext.Location = new Point(19, 34);
            lblContext.Name = "lblContext";
            lblContext.Size = new Size(0, 25);
            lblContext.TabIndex = 0;
            // 
            // lblStart
            // 
            lblStart.AutoSize = true;
            lblStart.Location = new Point(20, 69);
            lblStart.Name = "lblStart";
            lblStart.Size = new Size(99, 25);
            lblStart.TabIndex = 1;
            lblStart.Text = "Start (local)";
            // 
            // dtpStart
            // 
            dtpStart.CustomFormat = "yyyy-MM-dd HH:mm";
            dtpStart.Format = DateTimePickerFormat.Custom;
            dtpStart.Location = new Point(180, 69);
            dtpStart.Name = "dtpStart";
            dtpStart.ShowUpDown = true;
            dtpStart.Size = new Size(240, 31);
            dtpStart.TabIndex = 2;
            // 
            // lblEnd
            // 
            lblEnd.AutoSize = true;
            lblEnd.Location = new Point(20, 115);
            lblEnd.Name = "lblEnd";
            lblEnd.Size = new Size(93, 25);
            lblEnd.TabIndex = 3;
            lblEnd.Text = "End (local)";
            // 
            // dtpEnd
            // 
            dtpEnd.CustomFormat = "yyyy-MM-dd HH:mm";
            dtpEnd.Format = DateTimePickerFormat.Custom;
            dtpEnd.Location = new Point(177, 109);
            dtpEnd.Name = "dtpEnd";
            dtpEnd.ShowUpDown = true;
            dtpEnd.Size = new Size(240, 31);
            dtpEnd.TabIndex = 4;
            // 
            // lblSubject
            // 
            lblSubject.AutoSize = true;
            lblSubject.Location = new Point(19, 150);
            lblSubject.Name = "lblSubject";
            lblSubject.Size = new Size(70, 25);
            lblSubject.TabIndex = 5;
            lblSubject.Text = "Subject";
            // 
            // txtSubject
            // 
            txtSubject.Location = new Point(180, 146);
            txtSubject.Name = "txtSubject";
            txtSubject.Size = new Size(490, 31);
            txtSubject.TabIndex = 6;
            // 
            // lblInterval
            // 
            lblInterval.AutoSize = true;
            lblInterval.Location = new Point(19, 192);
            lblInterval.Name = "lblInterval";
            lblInterval.Size = new Size(161, 25);
            lblInterval.TabIndex = 7;
            lblInterval.Text = "Avail. interval (min)";
            // 
            // numInterval
            // 
            numInterval.Location = new Point(180, 190);
            numInterval.Maximum = new decimal(new int[] { 1440, 0, 0, 0 });
            numInterval.Minimum = new decimal(new int[] { 5, 0, 0, 0 });
            numInterval.Name = "numInterval";
            numInterval.Size = new Size(120, 31);
            numInterval.TabIndex = 8;
            numInterval.Value = new decimal(new int[] { 30, 0, 0, 0 });
            // 
            // lblEventId
            // 
            lblEventId.AutoSize = true;
            lblEventId.Location = new Point(19, 234);
            lblEventId.Name = "lblEventId";
            lblEventId.Size = new Size(76, 25);
            lblEventId.TabIndex = 9;
            lblEventId.Text = "Event Id";
            // 
            // txtEventId
            // 
            txtEventId.Location = new Point(180, 230);
            txtEventId.Name = "txtEventId";
            txtEventId.Size = new Size(490, 31);
            txtEventId.TabIndex = 10;
            // 
            // lblSchedules
            // 
            lblSchedules.AutoSize = true;
            lblSchedules.Location = new Point(19, 276);
            lblSchedules.Name = "lblSchedules";
            lblSchedules.Size = new Size(242, 25);
            lblSchedules.TabIndex = 11;
            lblSchedules.Text = "Schedules (One UPN per line)";
            // 
            // txtSchedules
            // 
            txtSchedules.Location = new Point(181, 314);
            txtSchedules.Multiline = true;
            txtSchedules.Name = "txtSchedules";
            txtSchedules.ScrollBars = ScrollBars.Vertical;
            txtSchedules.Size = new Size(490, 111);
            txtSchedules.TabIndex = 12;
            // 
            // grpTests
            // 
            grpTests.Controls.Add(rbGetSchedule);
            grpTests.Controls.Add(rbCalendarView);
            grpTests.Controls.Add(rbSubjectInRange);
            grpTests.Controls.Add(rbSubjectAll);
            grpTests.Controls.Add(rbInstances);
            grpTests.Controls.Add(rbDelta);
            grpTests.Location = new Point(730, 15);
            grpTests.Name = "grpTests";
            grpTests.Padding = new Padding(4);
            grpTests.Size = new Size(470, 360);
            grpTests.TabIndex = 1;
            grpTests.TabStop = false;
            grpTests.Text = "Tests to run";
            grpTests.Enter += grpTests_Enter;
            // 
            // rbGetSchedule
            // 
            rbGetSchedule.AutoSize = true;
            rbGetSchedule.Checked = true;
            rbGetSchedule.Location = new Point(19, 44);
            rbGetSchedule.Name = "rbGetSchedule";
            rbGetSchedule.Size = new Size(135, 29);
            rbGetSchedule.TabIndex = 0;
            rbGetSchedule.TabStop = true;
            rbGetSchedule.Text = "GetSchedule";
            rbGetSchedule.UseVisualStyleBackColor = true;
            // 
            // rbCalendarView
            // 
            rbCalendarView.AutoSize = true;
            rbCalendarView.Location = new Point(19, 90);
            rbCalendarView.Name = "rbCalendarView";
            rbCalendarView.Size = new Size(143, 29);
            rbCalendarView.TabIndex = 1;
            rbCalendarView.Text = "CalendarView";
            rbCalendarView.UseVisualStyleBackColor = true;
            // 
            // rbSubjectInRange
            // 
            rbSubjectInRange.AutoSize = true;
            rbSubjectInRange.Location = new Point(19, 136);
            rbSubjectInRange.Name = "rbSubjectInRange";
            rbSubjectInRange.Size = new Size(259, 29);
            rbSubjectInRange.TabIndex = 2;
            rbSubjectInRange.Text = "Search subject in time range";
            rbSubjectInRange.UseVisualStyleBackColor = true;
            // 
            // rbSubjectAll
            // 
            rbSubjectAll.AutoSize = true;
            rbSubjectAll.Location = new Point(19, 182);
            rbSubjectAll.Name = "rbSubjectAll";
            rbSubjectAll.Size = new Size(256, 29);
            rbSubjectAll.TabIndex = 3;
            rbSubjectAll.Text = "Search all events for subject";
            rbSubjectAll.UseVisualStyleBackColor = true;
            // 
            // rbInstances
            // 
            rbInstances.AutoSize = true;
            rbInstances.Location = new Point(19, 228);
            rbInstances.Name = "rbInstances";
            rbInstances.Size = new Size(226, 29);
            rbInstances.TabIndex = 4;
            rbInstances.Text = "Event instances in range";
            rbInstances.UseVisualStyleBackColor = true;
            // 
            // rbDelta
            // 
            rbDelta.AutoSize = true;
            rbDelta.Location = new Point(19, 274);
            rbDelta.Name = "rbDelta";
            rbDelta.Size = new Size(187, 29);
            rbDelta.TabIndex = 5;
            rbDelta.Text = "CalendarView delta";
            rbDelta.UseVisualStyleBackColor = true;
            // 
            // chkShowResults
            // 
            chkShowResults.AutoSize = true;
            chkShowResults.Checked = true;
            chkShowResults.CheckState = CheckState.Checked;
            chkShowResults.Location = new Point(12, 462);
            chkShowResults.Name = "chkShowResults";
            chkShowResults.Size = new Size(226, 29);
            chkShowResults.TabIndex = 2;
            chkShowResults.Text = "Show results in text box";
            chkShowResults.UseVisualStyleBackColor = true;
            // 
            // btnRun
            // 
            btnRun.Location = new Point(293, 453);
            btnRun.Name = "btnRun";
            btnRun.Size = new Size(139, 44);
            btnRun.TabIndex = 3;
            btnRun.Text = "Run Tests";
            btnRun.UseVisualStyleBackColor = true;
            btnRun.Click += btnRun_Click;
            // 
            // lblStartedCaption
            // 
            lblStartedCaption.AutoSize = true;
            lblStartedCaption.Location = new Point(458, 466);
            lblStartedCaption.Name = "lblStartedCaption";
            lblStartedCaption.Size = new Size(72, 25);
            lblStartedCaption.TabIndex = 4;
            lblStartedCaption.Text = "Started:";
            // 
            // lblStarted
            // 
            lblStarted.AutoSize = true;
            lblStarted.Location = new Point(536, 466);
            lblStarted.Name = "lblStarted";
            lblStarted.Size = new Size(19, 25);
            lblStarted.TabIndex = 5;
            lblStarted.Text = "_";
            // 
            // lblEndedCaption
            // 
            lblEndedCaption.AutoSize = true;
            lblEndedCaption.Location = new Point(675, 466);
            lblEndedCaption.Name = "lblEndedCaption";
            lblEndedCaption.Size = new Size(66, 25);
            lblEndedCaption.TabIndex = 6;
            lblEndedCaption.Text = "Ended:";
            // 
            // lblEnded
            // 
            lblEnded.AutoSize = true;
            lblEnded.Location = new Point(747, 462);
            lblEnded.Name = "lblEnded";
            lblEnded.Size = new Size(19, 25);
            lblEnded.TabIndex = 7;
            lblEnded.Text = "_";
            // 
            // lblElapsedCaption
            // 
            lblElapsedCaption.AutoSize = true;
            lblElapsedCaption.Location = new Point(925, 466);
            lblElapsedCaption.Name = "lblElapsedCaption";
            lblElapsedCaption.Size = new Size(77, 25);
            lblElapsedCaption.TabIndex = 8;
            lblElapsedCaption.Text = "Elapsed:";
            // 
            // lblElapsed
            // 
            lblElapsed.AutoSize = true;
            lblElapsed.Location = new Point(1008, 466);
            lblElapsed.Name = "lblElapsed";
            lblElapsed.Size = new Size(19, 25);
            lblElapsed.TabIndex = 9;
            lblElapsed.Text = "_";
            // 
            // txtResults
            // 
            txtResults.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            txtResults.BackColor = Color.White;
            txtResults.Font = new Font("Consolas", 9F);
            txtResults.Location = new Point(12, 503);
            txtResults.Name = "txtResults";
            txtResults.ReadOnly = true;
            txtResults.Size = new Size(1271, 443);
            txtResults.TabIndex = 10;
            txtResults.Text = "";
            txtResults.WordWrap = false;
            // 
            // frmTests
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1301, 958);
            Controls.Add(grpInputs);
            Controls.Add(grpTests);
            Controls.Add(chkShowResults);
            Controls.Add(btnRun);
            Controls.Add(lblStartedCaption);
            Controls.Add(lblStarted);
            Controls.Add(lblEndedCaption);
            Controls.Add(lblEnded);
            Controls.Add(lblElapsedCaption);
            Controls.Add(lblElapsed);
            Controls.Add(txtResults);
            MinimumSize = new Size(1000, 600);
            Name = "frmTests";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Graph API Tests";
            Load += frmTests_Load;
            grpInputs.ResumeLayout(false);
            grpInputs.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numInterval).EndInit();
            grpTests.ResumeLayout(false);
            grpTests.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private GroupBox grpInputs;
        private Label lblContext;
        private Label lblStart;
        private DateTimePicker dtpStart;
        private Label lblEnd;
        private DateTimePicker dtpEnd;
        private Label lblSubject;
        private TextBox txtSubject;
        private Label lblInterval;
        private NumericUpDown numInterval;
        private Label lblEventId;
        private TextBox txtEventId;
        private Label lblSchedules;
        private TextBox txtSchedules;
        private GroupBox grpTests;
        private RadioButton rbGetSchedule;
        private RadioButton rbCalendarView;
        private RadioButton rbSubjectInRange;
        private RadioButton rbSubjectAll;
        private RadioButton rbInstances;
        private RadioButton rbDelta;
        private CheckBox chkShowResults;
        private Button btnRun;
        private Label lblStartedCaption;
        private Label lblStarted;
        private Label lblEndedCaption;
        private Label lblEnded;
        private Label lblElapsedCaption;
        private Label lblElapsed;
        private RichTextBox txtResults;
    }
}
