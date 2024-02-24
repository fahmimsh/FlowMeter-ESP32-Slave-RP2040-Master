namespace SCADA
{
    partial class form_main
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(form_main));
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.status_txt = new System.Windows.Forms.ToolStripStatusLabel();
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.panel_main = new System.Windows.Forms.Panel();
            this.timer_handle_opc_tag1 = new System.Windows.Forms.Timer(this.components);
            this.timer_handle_opc_tag2 = new System.Windows.Forms.Timer(this.components);
            this.timer_delete_glg_popup = new System.Windows.Forms.Timer(this.components);
            this.iconsAppToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menu_file = new System.Windows.Forms.ToolStripMenuItem();
            this.menu_file_exit = new System.Windows.Forms.ToolStripMenuItem();
            this.menu_setting = new System.Windows.Forms.ToolStripMenuItem();
            this.menu_tag = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.menu_label = new System.Windows.Forms.ToolStripMenuItem();
            this.menu_hmi = new System.Windows.Forms.ToolStripMenuItem();
            this.menu_log = new System.Windows.Forms.ToolStripMenuItem();
            this.menu_view_chart = new System.Windows.Forms.ToolStripMenuItem();
            this.menu_exit = new System.Windows.Forms.ToolStripMenuItem();
            this.menu_minimize = new System.Windows.Forms.ToolStripMenuItem();
            this.menu_connect_opc = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStrip1.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.BackColor = System.Drawing.SystemColors.ActiveBorder;
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.iconsAppToolStripMenuItem,
            this.menu_file,
            this.menu_setting,
            this.menu_hmi,
            this.menu_log,
            this.menu_view_chart,
            this.menu_exit,
            this.menu_minimize,
            this.menu_connect_opc});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Padding = new System.Windows.Forms.Padding(4, 2, 0, 2);
            this.menuStrip1.Size = new System.Drawing.Size(1243, 28);
            this.menuStrip1.TabIndex = 4;
            this.menuStrip1.Text = "menuStrip_connect";
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.status_txt});
            this.statusStrip1.Location = new System.Drawing.Point(0, 567);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(1243, 22);
            this.statusStrip1.TabIndex = 5;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // status_txt
            // 
            this.status_txt.Name = "status_txt";
            this.status_txt.Size = new System.Drawing.Size(38, 17);
            this.status_txt.Text = "status";
            // 
            // imageList1
            // 
            this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList1.Images.SetKeyName(0, "icons8-energy-meter-16.png");
            this.imageList1.Images.SetKeyName(1, "iconTransferLog.ico");
            // 
            // panel_main
            // 
            this.panel_main.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel_main.Location = new System.Drawing.Point(0, 28);
            this.panel_main.Name = "panel_main";
            this.panel_main.Size = new System.Drawing.Size(1243, 539);
            this.panel_main.TabIndex = 6;
            // 
            // timer_handle_opc_tag1
            // 
            this.timer_handle_opc_tag1.Interval = 1;
            this.timer_handle_opc_tag1.Tick += new System.EventHandler(this.timer_handle_opc_tag1_Tick);
            // 
            // timer_handle_opc_tag2
            // 
            this.timer_handle_opc_tag2.Interval = 1;
            this.timer_handle_opc_tag2.Tick += new System.EventHandler(this.timer_handle_opc_tag2_Tick);
            // 
            // timer_delete_glg_popup
            // 
            this.timer_delete_glg_popup.Enabled = true;
            this.timer_delete_glg_popup.Interval = 1;
            this.timer_delete_glg_popup.Tick += new System.EventHandler(this.timer_delete_glg_popup_Tick);
            // 
            // iconsAppToolStripMenuItem
            // 
            this.iconsAppToolStripMenuItem.Image = global::SCADA.Properties.Resources.icons8_scale_16;
            this.iconsAppToolStripMenuItem.Name = "iconsAppToolStripMenuItem";
            this.iconsAppToolStripMenuItem.Size = new System.Drawing.Size(32, 24);
            // 
            // menu_file
            // 
            this.menu_file.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menu_file_exit});
            this.menu_file.Image = global::SCADA.Properties.Resources.icons_layers;
            this.menu_file.Name = "menu_file";
            this.menu_file.Size = new System.Drawing.Size(57, 24);
            this.menu_file.Text = "File";
            // 
            // menu_file_exit
            // 
            this.menu_file_exit.Image = global::SCADA.Properties.Resources.icons_exit;
            this.menu_file_exit.Name = "menu_file_exit";
            this.menu_file_exit.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.F4)));
            this.menu_file_exit.ShowShortcutKeys = false;
            this.menu_file_exit.Size = new System.Drawing.Size(90, 26);
            this.menu_file_exit.Text = "Exit";
            this.menu_file_exit.Click += new System.EventHandler(this.menu_file_exit_Click);
            // 
            // menu_setting
            // 
            this.menu_setting.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menu_tag,
            this.toolStripSeparator3,
            this.menu_label});
            this.menu_setting.Image = ((System.Drawing.Image)(resources.GetObject("menu_setting.Image")));
            this.menu_setting.Name = "menu_setting";
            this.menu_setting.Size = new System.Drawing.Size(76, 24);
            this.menu_setting.Text = "Setting";
            // 
            // menu_tag
            // 
            this.menu_tag.Image = ((System.Drawing.Image)(resources.GetObject("menu_tag.Image")));
            this.menu_tag.Name = "menu_tag";
            this.menu_tag.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.T)));
            this.menu_tag.ShowShortcutKeys = false;
            this.menu_tag.Size = new System.Drawing.Size(141, 26);
            this.menu_tag.Text = "TagIO";
            this.menu_tag.Click += new System.EventHandler(this.menu_tag_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(138, 6);
            // 
            // menu_label
            // 
            this.menu_label.Image = global::SCADA.Properties.Resources.iconsTagPrice;
            this.menu_label.Name = "menu_label";
            this.menu_label.Size = new System.Drawing.Size(141, 26);
            this.menu_label.Text = "Nama Label";
            this.menu_label.Click += new System.EventHandler(this.menu_label_Click);
            // 
            // menu_hmi
            // 
            this.menu_hmi.Image = global::SCADA.Properties.Resources.icons7Segment;
            this.menu_hmi.Name = "menu_hmi";
            this.menu_hmi.Size = new System.Drawing.Size(90, 24);
            this.menu_hmi.Text = "View HMI";
            this.menu_hmi.Click += new System.EventHandler(this.menu_hmi_Click);
            // 
            // menu_log
            // 
            this.menu_log.Image = global::SCADA.Properties.Resources.AddrSet;
            this.menu_log.Name = "menu_log";
            this.menu_log.Size = new System.Drawing.Size(87, 24);
            this.menu_log.Text = "View Log";
            this.menu_log.Click += new System.EventHandler(this.menu_log_Click);
            // 
            // menu_view_chart
            // 
            this.menu_view_chart.Image = global::SCADA.Properties.Resources.icons8_improvement_16;
            this.menu_view_chart.Name = "menu_view_chart";
            this.menu_view_chart.Size = new System.Drawing.Size(96, 24);
            this.menu_view_chart.Text = "View Chart";
            this.menu_view_chart.Click += new System.EventHandler(this.menu_view_chart_Click);
            // 
            // menu_exit
            // 
            this.menu_exit.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.menu_exit.Image = global::SCADA.Properties.Resources.icons_close;
            this.menu_exit.Name = "menu_exit";
            this.menu_exit.Size = new System.Drawing.Size(32, 24);
            this.menu_exit.Click += new System.EventHandler(this.menu_exit_Click);
            // 
            // menu_minimize
            // 
            this.menu_minimize.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.menu_minimize.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.menu_minimize.Image = global::SCADA.Properties.Resources.icons_minimize;
            this.menu_minimize.Name = "menu_minimize";
            this.menu_minimize.Size = new System.Drawing.Size(32, 24);
            this.menu_minimize.Click += new System.EventHandler(this.menu_minimize_Click);
            // 
            // menu_connect_opc
            // 
            this.menu_connect_opc.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.menu_connect_opc.Image = global::SCADA.Properties.Resources.icons8_disconnect;
            this.menu_connect_opc.Name = "menu_connect_opc";
            this.menu_connect_opc.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.L)));
            this.menu_connect_opc.ShowShortcutKeys = false;
            this.menu_connect_opc.Size = new System.Drawing.Size(98, 24);
            this.menu_connect_opc.Text = "Disconnect";
            this.menu_connect_opc.Click += new System.EventHandler(this.menu_connect_opc_Click);
            // 
            // form_main
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.AutoSize = true;
            this.ClientSize = new System.Drawing.Size(1243, 589);
            this.Controls.Add(this.panel_main);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.menuStrip1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "form_main";
            this.Text = "main form";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.Load += new System.EventHandler(this.form_main_Load);
            this.Shown += new System.EventHandler(this.form_main_Shown);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem iconsAppToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem menu_file;
        private System.Windows.Forms.ToolStripMenuItem menu_file_exit;
        private System.Windows.Forms.ToolStripMenuItem menu_setting;
        private System.Windows.Forms.ToolStripMenuItem menu_tag;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripMenuItem menu_label;
        private System.Windows.Forms.ToolStripMenuItem menu_log;
        private System.Windows.Forms.ToolStripMenuItem menu_exit;
        private System.Windows.Forms.ToolStripMenuItem menu_minimize;
        private System.Windows.Forms.ToolStripMenuItem menu_connect_opc;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ImageList imageList1;
        private System.Windows.Forms.ToolStripMenuItem menu_view_chart;
        private System.Windows.Forms.Panel panel_main;
        private System.Windows.Forms.ToolStripMenuItem menu_hmi;
        private System.Windows.Forms.Timer timer_handle_opc_tag1;
        private System.Windows.Forms.Timer timer_handle_opc_tag2;
        private System.Windows.Forms.Timer timer_delete_glg_popup;
        public System.Windows.Forms.ToolStripStatusLabel status_txt;
    }
}

