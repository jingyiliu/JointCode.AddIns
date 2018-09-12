namespace JointCode.AddIns.UiLib
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.mainMenu = new System.Windows.Forms.MenuStrip();
            this.menuItemFile = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItemExit = new System.Windows.Forms.ToolStripMenuItem();
            this.statusBar = new System.Windows.Forms.StatusStrip();
            this.imageList = new System.Windows.Forms.ImageList(this.components);
            this.toolBar = new System.Windows.Forms.ToolStrip();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripContainer1 = new System.Windows.Forms.ToolStripContainer();
            this.btnStartEngine = new System.Windows.Forms.Button();
            this.btnStopEngine = new System.Windows.Forms.Button();
            this.btnLoadAssemblies = new System.Windows.Forms.Button();
            this.btnDisable = new System.Windows.Forms.Button();
            this.btnEnable = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.txtInformation = new System.Windows.Forms.TextBox();
            this.btnUnloadEP = new System.Windows.Forms.Button();
            this.btnLoadEP = new System.Windows.Forms.Button();
            this.btnStop = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.txtExtensionPoint = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.btnStart = new System.Windows.Forms.Button();
            this.txtAddin = new System.Windows.Forms.TextBox();
            this.mainMenu.SuspendLayout();
            this.toolBar.SuspendLayout();
            this.toolStripContainer1.BottomToolStripPanel.SuspendLayout();
            this.toolStripContainer1.ContentPanel.SuspendLayout();
            this.toolStripContainer1.TopToolStripPanel.SuspendLayout();
            this.toolStripContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // mainMenu
            // 
            this.mainMenu.Dock = System.Windows.Forms.DockStyle.None;
            this.mainMenu.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.mainMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuItemFile});
            this.mainMenu.Location = new System.Drawing.Point(0, 0);
            this.mainMenu.Name = "mainMenu";
            this.mainMenu.Size = new System.Drawing.Size(940, 28);
            this.mainMenu.TabIndex = 7;
            // 
            // menuItemFile
            // 
            this.menuItemFile.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuItemExit});
            this.menuItemFile.Name = "menuItemFile";
            this.menuItemFile.Size = new System.Drawing.Size(46, 24);
            this.menuItemFile.Text = "&File";
            // 
            // menuItemExit
            // 
            this.menuItemExit.Name = "menuItemExit";
            this.menuItemExit.Size = new System.Drawing.Size(110, 26);
            this.menuItemExit.Text = "&Exit";
            this.menuItemExit.Click += new System.EventHandler(this.menuItemExit_Click);
            // 
            // statusBar
            // 
            this.statusBar.Dock = System.Windows.Forms.DockStyle.None;
            this.statusBar.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.statusBar.Location = new System.Drawing.Point(0, 0);
            this.statusBar.Name = "statusBar";
            this.statusBar.Size = new System.Drawing.Size(940, 22);
            this.statusBar.TabIndex = 4;
            // 
            // imageList
            // 
            this.imageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList.ImageStream")));
            this.imageList.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList.Images.SetKeyName(0, "");
            this.imageList.Images.SetKeyName(1, "");
            this.imageList.Images.SetKeyName(2, "");
            this.imageList.Images.SetKeyName(3, "");
            this.imageList.Images.SetKeyName(4, "");
            this.imageList.Images.SetKeyName(5, "");
            this.imageList.Images.SetKeyName(6, "");
            this.imageList.Images.SetKeyName(7, "");
            this.imageList.Images.SetKeyName(8, "");
            // 
            // toolBar
            // 
            this.toolBar.Dock = System.Windows.Forms.DockStyle.None;
            this.toolBar.ImageList = this.imageList;
            this.toolBar.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.toolBar.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripSeparator1});
            this.toolBar.Location = new System.Drawing.Point(0, 0);
            this.toolBar.Name = "toolBar";
            this.toolBar.Size = new System.Drawing.Size(18, 25);
            this.toolBar.TabIndex = 6;
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // toolStripContainer1
            // 
            // 
            // toolStripContainer1.BottomToolStripPanel
            // 
            this.toolStripContainer1.BottomToolStripPanel.Controls.Add(this.statusBar);
            // 
            // toolStripContainer1.ContentPanel
            // 
            this.toolStripContainer1.ContentPanel.Controls.Add(this.btnStartEngine);
            this.toolStripContainer1.ContentPanel.Controls.Add(this.btnStopEngine);
            this.toolStripContainer1.ContentPanel.Controls.Add(this.btnLoadAssemblies);
            this.toolStripContainer1.ContentPanel.Controls.Add(this.btnDisable);
            this.toolStripContainer1.ContentPanel.Controls.Add(this.btnEnable);
            this.toolStripContainer1.ContentPanel.Controls.Add(this.label3);
            this.toolStripContainer1.ContentPanel.Controls.Add(this.toolBar);
            this.toolStripContainer1.ContentPanel.Controls.Add(this.txtInformation);
            this.toolStripContainer1.ContentPanel.Controls.Add(this.btnUnloadEP);
            this.toolStripContainer1.ContentPanel.Controls.Add(this.btnLoadEP);
            this.toolStripContainer1.ContentPanel.Controls.Add(this.btnStop);
            this.toolStripContainer1.ContentPanel.Controls.Add(this.label2);
            this.toolStripContainer1.ContentPanel.Controls.Add(this.txtExtensionPoint);
            this.toolStripContainer1.ContentPanel.Controls.Add(this.label1);
            this.toolStripContainer1.ContentPanel.Controls.Add(this.btnStart);
            this.toolStripContainer1.ContentPanel.Controls.Add(this.txtAddin);
            this.toolStripContainer1.ContentPanel.Size = new System.Drawing.Size(940, 810);
            this.toolStripContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.toolStripContainer1.Location = new System.Drawing.Point(0, 0);
            this.toolStripContainer1.Name = "toolStripContainer1";
            this.toolStripContainer1.Size = new System.Drawing.Size(940, 860);
            this.toolStripContainer1.TabIndex = 0;
            // 
            // toolStripContainer1.TopToolStripPanel
            // 
            this.toolStripContainer1.TopToolStripPanel.Controls.Add(this.mainMenu);
            // 
            // btnStartEngine
            // 
            this.btnStartEngine.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnStartEngine.Location = new System.Drawing.Point(36, 151);
            this.btnStartEngine.Name = "btnStartEngine";
            this.btnStartEngine.Size = new System.Drawing.Size(117, 23);
            this.btnStartEngine.TabIndex = 22;
            this.btnStartEngine.Text = "Start Engine";
            this.btnStartEngine.UseVisualStyleBackColor = true;
            this.btnStartEngine.Click += new System.EventHandler(this.btnStartEngine_Click);
            // 
            // btnStopEngine
            // 
            this.btnStopEngine.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnStopEngine.Location = new System.Drawing.Point(159, 151);
            this.btnStopEngine.Name = "btnStopEngine";
            this.btnStopEngine.Size = new System.Drawing.Size(120, 23);
            this.btnStopEngine.TabIndex = 21;
            this.btnStopEngine.Text = "Stop Engine";
            this.btnStopEngine.UseVisualStyleBackColor = true;
            this.btnStopEngine.Click += new System.EventHandler(this.btnStopEngine_Click);
            // 
            // btnLoadAssemblies
            // 
            this.btnLoadAssemblies.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnLoadAssemblies.Location = new System.Drawing.Point(748, 122);
            this.btnLoadAssemblies.Name = "btnLoadAssemblies";
            this.btnLoadAssemblies.Size = new System.Drawing.Size(142, 23);
            this.btnLoadAssemblies.TabIndex = 20;
            this.btnLoadAssemblies.Text = "Load Assemblies";
            this.btnLoadAssemblies.UseVisualStyleBackColor = true;
            this.btnLoadAssemblies.Click += new System.EventHandler(this.btnLoadAssemblies_Click);
            // 
            // btnDisable
            // 
            this.btnDisable.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnDisable.Location = new System.Drawing.Point(412, 122);
            this.btnDisable.Name = "btnDisable";
            this.btnDisable.Size = new System.Drawing.Size(120, 23);
            this.btnDisable.TabIndex = 19;
            this.btnDisable.Text = "Disable Addin";
            this.btnDisable.UseVisualStyleBackColor = true;
            this.btnDisable.Click += new System.EventHandler(this.btnDisable_Click);
            // 
            // btnEnable
            // 
            this.btnEnable.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnEnable.Location = new System.Drawing.Point(285, 122);
            this.btnEnable.Name = "btnEnable";
            this.btnEnable.Size = new System.Drawing.Size(117, 23);
            this.btnEnable.TabIndex = 18;
            this.btnEnable.Text = "Enable Addin";
            this.btnEnable.UseVisualStyleBackColor = true;
            this.btnEnable.Click += new System.EventHandler(this.btnEnable_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(33, 201);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(95, 15);
            this.label3.TabIndex = 17;
            this.label3.Text = "Information";
            // 
            // txtInformation
            // 
            this.txtInformation.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtInformation.Location = new System.Drawing.Point(36, 231);
            this.txtInformation.Multiline = true;
            this.txtInformation.Name = "txtInformation";
            this.txtInformation.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtInformation.Size = new System.Drawing.Size(867, 540);
            this.txtInformation.TabIndex = 16;
            // 
            // btnUnloadEP
            // 
            this.btnUnloadEP.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnUnloadEP.Location = new System.Drawing.Point(652, 122);
            this.btnUnloadEP.Name = "btnUnloadEP";
            this.btnUnloadEP.Size = new System.Drawing.Size(90, 23);
            this.btnUnloadEP.TabIndex = 15;
            this.btnUnloadEP.Text = "Unload EP";
            this.btnUnloadEP.UseVisualStyleBackColor = true;
            this.btnUnloadEP.Click += new System.EventHandler(this.btnUnloadEP_Click);
            // 
            // btnLoadEP
            // 
            this.btnLoadEP.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnLoadEP.Location = new System.Drawing.Point(538, 122);
            this.btnLoadEP.Name = "btnLoadEP";
            this.btnLoadEP.Size = new System.Drawing.Size(108, 23);
            this.btnLoadEP.TabIndex = 14;
            this.btnLoadEP.Text = "Load EP";
            this.btnLoadEP.UseVisualStyleBackColor = true;
            this.btnLoadEP.Click += new System.EventHandler(this.btnLoadEP_Click);
            // 
            // btnStop
            // 
            this.btnStop.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnStop.Location = new System.Drawing.Point(159, 122);
            this.btnStop.Name = "btnStop";
            this.btnStop.Size = new System.Drawing.Size(120, 23);
            this.btnStop.TabIndex = 13;
            this.btnStop.Text = "Stop Addin";
            this.btnStop.UseVisualStyleBackColor = true;
            this.btnStop.Click += new System.EventHandler(this.btnStop_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(34, 85);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(119, 15);
            this.label2.TabIndex = 12;
            this.label2.Text = "ExtensionPoint";
            // 
            // txtExtensionPoint
            // 
            this.txtExtensionPoint.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtExtensionPoint.Location = new System.Drawing.Point(163, 83);
            this.txtExtensionPoint.Name = "txtExtensionPoint";
            this.txtExtensionPoint.Size = new System.Drawing.Size(740, 25);
            this.txtExtensionPoint.TabIndex = 11;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(34, 50);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(47, 15);
            this.label1.TabIndex = 10;
            this.label1.Text = "Addin";
            // 
            // btnStart
            // 
            this.btnStart.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnStart.Location = new System.Drawing.Point(36, 122);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(117, 23);
            this.btnStart.TabIndex = 9;
            this.btnStart.Text = "Start Addin";
            this.btnStart.UseVisualStyleBackColor = true;
            this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
            // 
            // txtAddin
            // 
            this.txtAddin.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtAddin.Location = new System.Drawing.Point(163, 46);
            this.txtAddin.Name = "txtAddin";
            this.txtAddin.Size = new System.Drawing.Size(740, 25);
            this.txtAddin.TabIndex = 8;
            // 
            // MainForm
            // 
            this.ClientSize = new System.Drawing.Size(940, 860);
            this.Controls.Add(this.toolStripContainer1);
            this.IsMdiContainer = true;
            this.MainMenuStrip = this.mainMenu;
            this.Name = "MainForm";
            this.Text = "JointCode.AddIns.Shell";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.mainMenu.ResumeLayout(false);
            this.mainMenu.PerformLayout();
            this.toolBar.ResumeLayout(false);
            this.toolBar.PerformLayout();
            this.toolStripContainer1.BottomToolStripPanel.ResumeLayout(false);
            this.toolStripContainer1.BottomToolStripPanel.PerformLayout();
            this.toolStripContainer1.ContentPanel.ResumeLayout(false);
            this.toolStripContainer1.ContentPanel.PerformLayout();
            this.toolStripContainer1.TopToolStripPanel.ResumeLayout(false);
            this.toolStripContainer1.TopToolStripPanel.PerformLayout();
            this.toolStripContainer1.ResumeLayout(false);
            this.toolStripContainer1.PerformLayout();
            this.ResumeLayout(false);

		}
		#endregion

        private System.Windows.Forms.ToolStripContainer toolStripContainer1;
        private System.Windows.Forms.ImageList imageList;
        private System.Windows.Forms.ToolStrip toolBar;
        private System.Windows.Forms.MenuStrip mainMenu;
        private System.Windows.Forms.ToolStripMenuItem menuItemFile;
        private System.Windows.Forms.ToolStripMenuItem menuItemExit;
        private System.Windows.Forms.StatusStrip statusBar;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtInformation;
        private System.Windows.Forms.Button btnUnloadEP;
        private System.Windows.Forms.Button btnLoadEP;
        private System.Windows.Forms.Button btnStop;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtExtensionPoint;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnStart;
        private System.Windows.Forms.TextBox txtAddin;
        private System.Windows.Forms.Button btnLoadAssemblies;
        private System.Windows.Forms.Button btnDisable;
        private System.Windows.Forms.Button btnEnable;
        private System.Windows.Forms.Button btnStopEngine;
        private System.Windows.Forms.Button btnStartEngine;
    }
}