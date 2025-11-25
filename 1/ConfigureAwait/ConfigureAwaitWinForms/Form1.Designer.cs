namespace ConfigureAwaitWinForms;

partial class Form1
{
    /// <summary>
    ///  Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    ///  Clean up any resources being used.
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
    ///  Required method for Designer support - do not modify
    ///  the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        this.btnTestWithConfigureAwait = new Button();
        this.btnTestWithoutConfigureAwait = new Button();
        this.btnTestDeadlock = new Button();
        this.btnTestPerformance = new Button();
        this.txtOutput = new TextBox();
        this.lblTitle = new Label();
        this.lblThreadInfo = new Label();
        this.btnTestLibraryCode = new Button();
        this.btnClear = new Button();
        this.SuspendLayout();
        // 
        // btnTestWithConfigureAwait
        // 
        this.btnTestWithConfigureAwait.Location = new System.Drawing.Point(12, 60);
        this.btnTestWithConfigureAwait.Name = "btnTestWithConfigureAwait";
        this.btnTestWithConfigureAwait.Size = new System.Drawing.Size(250, 40);
        this.btnTestWithConfigureAwait.TabIndex = 0;
        this.btnTestWithConfigureAwait.Text = "✅ با ConfigureAwait(false)";
        this.btnTestWithConfigureAwait.UseVisualStyleBackColor = true;
        this.btnTestWithConfigureAwait.Click += new System.EventHandler(this.BtnTestWithConfigureAwait_Click);
        // 
        // btnTestWithoutConfigureAwait
        // 
        this.btnTestWithoutConfigureAwait.Location = new System.Drawing.Point(268, 60);
        this.btnTestWithoutConfigureAwait.Name = "btnTestWithoutConfigureAwait";
        this.btnTestWithoutConfigureAwait.Size = new System.Drawing.Size(250, 40);
        this.btnTestWithoutConfigureAwait.TabIndex = 1;
        this.btnTestWithoutConfigureAwait.Text = "❌ بدون ConfigureAwait";
        this.btnTestWithoutConfigureAwait.UseVisualStyleBackColor = true;
        this.btnTestWithoutConfigureAwait.Click += new System.EventHandler(this.BtnTestWithoutConfigureAwait_Click);
        // 
        // btnTestDeadlock
        // 
        this.btnTestDeadlock.Location = new System.Drawing.Point(524, 60);
        this.btnTestDeadlock.Name = "btnTestDeadlock";
        this.btnTestDeadlock.Size = new System.Drawing.Size(250, 40);
        this.btnTestDeadlock.TabIndex = 2;
        this.btnTestDeadlock.Text = "⚠️ تست Deadlock (با .Result)";
        this.btnTestDeadlock.UseVisualStyleBackColor = true;
        this.btnTestDeadlock.Click += new System.EventHandler(this.BtnTestDeadlock_Click);
        // 
        // btnTestPerformance
        // 
        this.btnTestPerformance.Location = new System.Drawing.Point(12, 106);
        this.btnTestPerformance.Name = "btnTestPerformance";
        this.btnTestPerformance.Size = new System.Drawing.Size(250, 40);
        this.btnTestPerformance.TabIndex = 3;
        this.btnTestPerformance.Text = "📊 مقایسه عملکرد";
        this.btnTestPerformance.UseVisualStyleBackColor = true;
        this.btnTestPerformance.Click += new System.EventHandler(this.BtnTestPerformance_Click);
        // 
        // btnTestLibraryCode
        // 
        this.btnTestLibraryCode.Location = new System.Drawing.Point(268, 106);
        this.btnTestLibraryCode.Name = "btnTestLibraryCode";
        this.btnTestLibraryCode.Size = new System.Drawing.Size(250, 40);
        this.btnTestLibraryCode.TabIndex = 4;
        this.btnTestLibraryCode.Text = "📚 Library Code مثال";
        this.btnTestLibraryCode.UseVisualStyleBackColor = true;
        this.btnTestLibraryCode.Click += new System.EventHandler(this.BtnTestLibraryCode_Click);
        // 
        // btnClear
        // 
        this.btnClear.Location = new System.Drawing.Point(524, 106);
        this.btnClear.Name = "btnClear";
        this.btnClear.Size = new System.Drawing.Size(250, 40);
        this.btnClear.TabIndex = 5;
        this.btnClear.Text = "🗑️ پاک کردن خروجی";
        this.btnClear.UseVisualStyleBackColor = true;
        this.btnClear.Click += new System.EventHandler(this.BtnClear_Click);
        // 
        // btnTestDeadlockClassic
        // 
        this.btnTestDeadlockClassic = new Button();
        this.btnTestDeadlockClassic.Location = new System.Drawing.Point(12, 152);
        this.btnTestDeadlockClassic.Name = "btnTestDeadlockClassic";
        this.btnTestDeadlockClassic.Size = new System.Drawing.Size(380, 40);
        this.btnTestDeadlockClassic.TabIndex = 6;
        this.btnTestDeadlockClassic.Text = "🔴 Deadlock کلاسیک (با Task.Run)";
        this.btnTestDeadlockClassic.UseVisualStyleBackColor = true;
        this.btnTestDeadlockClassic.Click += new System.EventHandler(this.BtnTestDeadlockClassic_Click);
        // 
        // txtOutput
        // 
        this.txtOutput.Anchor = ((AnchorStyles)((((AnchorStyles.Top | AnchorStyles.Bottom) 
        | AnchorStyles.Left) 
        | AnchorStyles.Right)));
        this.txtOutput.Font = new System.Drawing.Font("Consolas", 9F);
        this.txtOutput.Location = new System.Drawing.Point(12, 198);
        this.txtOutput.Multiline = true;
        this.txtOutput.Name = "txtOutput";
        this.txtOutput.ReadOnly = true;
        this.txtOutput.ScrollBars = ScrollBars.Vertical;
        this.txtOutput.Size = new System.Drawing.Size(762, 372);
        this.txtOutput.TabIndex = 6;
        // 
        // lblTitle
        // 
        this.lblTitle.AutoSize = true;
        this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 14F, FontStyle.Bold);
        this.lblTitle.Location = new System.Drawing.Point(12, 9);
        this.lblTitle.Name = "lblTitle";
        this.lblTitle.Size = new System.Drawing.Size(480, 25);
        this.lblTitle.TabIndex = 7;
        this.lblTitle.Text = "ConfigureAwait(false) در WinForms - نمایش اهمیت استفاده";
        // 
        // lblThreadInfo
        // 
        this.lblThreadInfo.AutoSize = true;
        this.lblThreadInfo.Location = new System.Drawing.Point(398, 160);
        this.lblThreadInfo.Name = "lblThreadInfo";
        this.lblThreadInfo.Size = new System.Drawing.Size(0, 15);
        this.lblThreadInfo.TabIndex = 8;
        // 
        // Form1
        // 
        this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
        this.AutoScaleMode = AutoScaleMode.Font;
        this.ClientSize = new System.Drawing.Size(786, 582);
        this.Controls.Add(this.lblThreadInfo);
        this.Controls.Add(this.lblTitle);
        this.Controls.Add(this.txtOutput);
        this.Controls.Add(this.btnTestDeadlockClassic);
        this.Controls.Add(this.btnClear);
        this.Controls.Add(this.btnTestLibraryCode);
        this.Controls.Add(this.btnTestPerformance);
        this.Controls.Add(this.btnTestDeadlock);
        this.Controls.Add(this.btnTestWithoutConfigureAwait);
        this.Controls.Add(this.btnTestWithConfigureAwait);
        this.Name = "Form1";
        this.Text = "ConfigureAwait Demo - WinForms";
        this.ResumeLayout(false);
        this.PerformLayout();
    }

    #endregion

    private Button btnTestWithConfigureAwait;
    private Button btnTestWithoutConfigureAwait;
    private Button btnTestDeadlock;
    private Button btnTestPerformance;
    private Button btnTestLibraryCode;
    private Button btnClear;
    private Button btnTestDeadlockClassic;
    private TextBox txtOutput;
    private Label lblTitle;
    private Label lblThreadInfo;
}
