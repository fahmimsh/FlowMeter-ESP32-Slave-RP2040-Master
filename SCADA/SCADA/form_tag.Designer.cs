namespace SCADA
{
    partial class form_tag
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle5 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle6 = new System.Windows.Forms.DataGridViewCellStyle();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(form_tag));
            this.toolStripTextBoxServer = new System.Windows.Forms.ToolStripTextBox();
            this.toolStripTextBoxHost = new System.Windows.Forms.ToolStripTextBox();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.PropertiesOpc1 = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolStripMenuItemsetOPCServer = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolStripMenuItemConnectOPC = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolStripMenuItemAboutOpc = new System.Windows.Forms.ToolStripMenuItem();
            this.btn_database = new System.Windows.Forms.ToolStripMenuItem();
            this.panel1 = new System.Windows.Forms.Panel();
            this.dataGridViewTagIo = new System.Windows.Forms.DataGridView();
            this.ColumnNo = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ColumnNameTag = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ColumnValueTag = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ColumnQuality = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ColumnTimeStamp = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.count = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.FlagOpc = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.type = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.treeView1 = new System.Windows.Forms.TreeView();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.panel2 = new System.Windows.Forms.Panel();
            this.dbServerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.dbTabelFl12ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.dbTabelFl345ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tb_db_server = new System.Windows.Forms.ToolStripTextBox();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.btn_save_db = new System.Windows.Forms.ToolStripMenuItem();
            this.tb_tabel_1_2 = new System.Windows.Forms.ToolStripTextBox();
            this.tb_tabel_3_4_5 = new System.Windows.Forms.ToolStripTextBox();
            this.dbNameToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tb_db_name = new System.Windows.Forms.ToolStripTextBox();
            this.menuStrip1.SuspendLayout();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewTagIo)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // toolStripTextBoxServer
            // 
            this.toolStripTextBoxServer.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.toolStripTextBoxServer.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.toolStripTextBoxServer.Name = "toolStripTextBoxServer";
            this.toolStripTextBoxServer.ReadOnly = true;
            this.toolStripTextBoxServer.Size = new System.Drawing.Size(110, 24);
            // 
            // toolStripTextBoxHost
            // 
            this.toolStripTextBoxHost.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.toolStripTextBoxHost.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.toolStripTextBoxHost.Name = "toolStripTextBoxHost";
            this.toolStripTextBoxHost.ReadOnly = true;
            this.toolStripTextBoxHost.Size = new System.Drawing.Size(70, 24);
            // 
            // menuStrip1
            // 
            this.menuStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Visible;
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripTextBoxHost,
            this.toolStripTextBoxServer,
            this.PropertiesOpc1,
            this.btn_database});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Padding = new System.Windows.Forms.Padding(2, 2, 0, 2);
            this.menuStrip1.Size = new System.Drawing.Size(831, 28);
            this.menuStrip1.TabIndex = 2;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // PropertiesOpc1
            // 
            this.PropertiesOpc1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ToolStripMenuItemsetOPCServer,
            this.ToolStripMenuItemConnectOPC,
            this.ToolStripMenuItemAboutOpc});
            this.PropertiesOpc1.Image = global::SCADA.Properties.Resources.server;
            this.PropertiesOpc1.Name = "PropertiesOpc1";
            this.PropertiesOpc1.Size = new System.Drawing.Size(92, 24);
            this.PropertiesOpc1.Text = "Properties";
            // 
            // ToolStripMenuItemsetOPCServer
            // 
            this.ToolStripMenuItemsetOPCServer.Image = global::SCADA.Properties.Resources.server;
            this.ToolStripMenuItemsetOPCServer.Name = "ToolStripMenuItemsetOPCServer";
            this.ToolStripMenuItemsetOPCServer.Size = new System.Drawing.Size(184, 26);
            this.ToolStripMenuItemsetOPCServer.Text = "Set OPC Server";
            this.ToolStripMenuItemsetOPCServer.Click += new System.EventHandler(this.ToolStripMenuItemsetOPCServer_Click);
            // 
            // ToolStripMenuItemConnectOPC
            // 
            this.ToolStripMenuItemConnectOPC.Enabled = false;
            this.ToolStripMenuItemConnectOPC.Image = global::SCADA.Properties.Resources.icons8_connect;
            this.ToolStripMenuItemConnectOPC.Name = "ToolStripMenuItemConnectOPC";
            this.ToolStripMenuItemConnectOPC.Size = new System.Drawing.Size(184, 26);
            this.ToolStripMenuItemConnectOPC.Text = "Connect OPC";
            this.ToolStripMenuItemConnectOPC.Click += new System.EventHandler(this.ToolStripMenuItemConnectOPC_Click);
            // 
            // ToolStripMenuItemAboutOpc
            // 
            this.ToolStripMenuItemAboutOpc.Image = global::SCADA.Properties.Resources.about;
            this.ToolStripMenuItemAboutOpc.Name = "ToolStripMenuItemAboutOpc";
            this.ToolStripMenuItemAboutOpc.Size = new System.Drawing.Size(184, 26);
            this.ToolStripMenuItemAboutOpc.Text = "About Opc";
            this.ToolStripMenuItemAboutOpc.Click += new System.EventHandler(this.ToolStripMenuItemAboutOpc_Click);
            // 
            // btn_database
            // 
            this.btn_database.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.dbServerToolStripMenuItem,
            this.dbNameToolStripMenuItem,
            this.dbTabelFl12ToolStripMenuItem,
            this.dbTabelFl345ToolStripMenuItem,
            this.toolStripSeparator1,
            this.btn_save_db});
            this.btn_database.Image = global::SCADA.Properties.Resources.icons8_sync_16;
            this.btn_database.Name = "btn_database";
            this.btn_database.Size = new System.Drawing.Size(90, 24);
            this.btn_database.Text = "Data Base";
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.SystemColors.Control;
            this.panel1.Controls.Add(this.menuStrip1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(831, 32);
            this.panel1.TabIndex = 4;
            // 
            // dataGridViewTagIo
            // 
            this.dataGridViewTagIo.AllowUserToAddRows = false;
            this.dataGridViewTagIo.AllowUserToDeleteRows = false;
            this.dataGridViewTagIo.AllowUserToOrderColumns = true;
            this.dataGridViewTagIo.AllowUserToResizeRows = false;
            this.dataGridViewTagIo.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.Bisque;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.Color.Bisque;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dataGridViewTagIo.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.dataGridViewTagIo.ColumnHeadersHeight = 25;
            this.dataGridViewTagIo.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            this.dataGridViewTagIo.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.ColumnNo,
            this.ColumnNameTag,
            this.ColumnValueTag,
            this.ColumnQuality,
            this.ColumnTimeStamp,
            this.count,
            this.FlagOpc,
            this.type});
            this.dataGridViewTagIo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridViewTagIo.EnableHeadersVisualStyles = false;
            this.dataGridViewTagIo.Location = new System.Drawing.Point(0, 0);
            this.dataGridViewTagIo.Name = "dataGridViewTagIo";
            this.dataGridViewTagIo.ReadOnly = true;
            this.dataGridViewTagIo.RowHeadersVisible = false;
            this.dataGridViewTagIo.RowHeadersWidth = 51;
            this.dataGridViewTagIo.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dataGridViewTagIo.Size = new System.Drawing.Size(587, 477);
            this.dataGridViewTagIo.TabIndex = 0;
            // 
            // ColumnNo
            // 
            this.ColumnNo.DataPropertyName = "ClientHandle";
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            this.ColumnNo.DefaultCellStyle = dataGridViewCellStyle2;
            this.ColumnNo.FillWeight = 25F;
            this.ColumnNo.HeaderText = "No";
            this.ColumnNo.MinimumWidth = 6;
            this.ColumnNo.Name = "ColumnNo";
            this.ColumnNo.ReadOnly = true;
            // 
            // ColumnNameTag
            // 
            this.ColumnNameTag.DataPropertyName = "ItemName";
            this.ColumnNameTag.FillWeight = 141.2462F;
            this.ColumnNameTag.HeaderText = "Name Tag";
            this.ColumnNameTag.MinimumWidth = 6;
            this.ColumnNameTag.Name = "ColumnNameTag";
            this.ColumnNameTag.ReadOnly = true;
            // 
            // ColumnValueTag
            // 
            this.ColumnValueTag.DataPropertyName = "Value";
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle3.NullValue = null;
            this.ColumnValueTag.DefaultCellStyle = dataGridViewCellStyle3;
            this.ColumnValueTag.FillWeight = 54.32545F;
            this.ColumnValueTag.HeaderText = "Value";
            this.ColumnValueTag.MinimumWidth = 6;
            this.ColumnValueTag.Name = "ColumnValueTag";
            this.ColumnValueTag.ReadOnly = true;
            // 
            // ColumnQuality
            // 
            this.ColumnQuality.DataPropertyName = "Quality";
            dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            this.ColumnQuality.DefaultCellStyle = dataGridViewCellStyle4;
            this.ColumnQuality.FillWeight = 54.32545F;
            this.ColumnQuality.HeaderText = "Quality";
            this.ColumnQuality.MinimumWidth = 6;
            this.ColumnQuality.Name = "ColumnQuality";
            this.ColumnQuality.ReadOnly = true;
            // 
            // ColumnTimeStamp
            // 
            this.ColumnTimeStamp.DataPropertyName = "Timestamp";
            dataGridViewCellStyle5.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            this.ColumnTimeStamp.DefaultCellStyle = dataGridViewCellStyle5;
            this.ColumnTimeStamp.FillWeight = 108.6509F;
            this.ColumnTimeStamp.HeaderText = "Time Stamp";
            this.ColumnTimeStamp.MinimumWidth = 6;
            this.ColumnTimeStamp.Name = "ColumnTimeStamp";
            this.ColumnTimeStamp.ReadOnly = true;
            // 
            // count
            // 
            this.count.DataPropertyName = "Counter";
            dataGridViewCellStyle6.Alignment = System.Windows.Forms.DataGridViewContentAlignment.TopCenter;
            this.count.DefaultCellStyle = dataGridViewCellStyle6;
            this.count.FillWeight = 30F;
            this.count.HeaderText = "count";
            this.count.Name = "count";
            this.count.ReadOnly = true;
            // 
            // FlagOpc
            // 
            this.FlagOpc.DataPropertyName = "Flag";
            this.FlagOpc.FillWeight = 30F;
            this.FlagOpc.HeaderText = "Flag";
            this.FlagOpc.Name = "FlagOpc";
            this.FlagOpc.ReadOnly = true;
            // 
            // type
            // 
            this.type.DataPropertyName = "TipeReq";
            this.type.HeaderText = "Type";
            this.type.Name = "type";
            this.type.ReadOnly = true;
            this.type.Visible = false;
            // 
            // imageList1
            // 
            this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList1.Images.SetKeyName(0, "tag1.png");
            this.imageList1.Images.SetKeyName(1, "tag2.png");
            this.imageList1.Images.SetKeyName(2, "folder-0.png");
            this.imageList1.Images.SetKeyName(3, "folder-1.png");
            this.imageList1.Images.SetKeyName(4, "folder-open-0.png");
            this.imageList1.Images.SetKeyName(5, "folder-open-1.png");
            // 
            // treeView1
            // 
            this.treeView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeView1.FullRowSelect = true;
            this.treeView1.ImageIndex = 0;
            this.treeView1.ImageList = this.imageList1;
            this.treeView1.Location = new System.Drawing.Point(0, 0);
            this.treeView1.Margin = new System.Windows.Forms.Padding(0);
            this.treeView1.Name = "treeView1";
            this.treeView1.PathSeparator = ".";
            this.treeView1.SelectedImageIndex = 0;
            this.treeView1.ShowNodeToolTips = true;
            this.treeView1.Size = new System.Drawing.Size(232, 477);
            this.treeView1.TabIndex = 2;
            this.treeView1.AfterCollapse += new System.Windows.Forms.TreeViewEventHandler(this.treeView1_AfterCollapse);
            this.treeView1.AfterExpand += new System.Windows.Forms.TreeViewEventHandler(this.treeView1_AfterExpand);
            // 
            // splitContainer1
            // 
            this.splitContainer1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.treeView1);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.dataGridViewTagIo);
            this.splitContainer1.Size = new System.Drawing.Size(831, 481);
            this.splitContainer1.SplitterDistance = 236;
            this.splitContainer1.TabIndex = 7;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.splitContainer1);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Location = new System.Drawing.Point(0, 32);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(831, 481);
            this.panel2.TabIndex = 5;
            // 
            // dbServerToolStripMenuItem
            // 
            this.dbServerToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tb_db_server});
            this.dbServerToolStripMenuItem.Image = global::SCADA.Properties.Resources.icons8_sync_16;
            this.dbServerToolStripMenuItem.Name = "dbServerToolStripMenuItem";
            this.dbServerToolStripMenuItem.Size = new System.Drawing.Size(184, 26);
            this.dbServerToolStripMenuItem.Text = "Db Server";
            // 
            // dbTabelFl12ToolStripMenuItem
            // 
            this.dbTabelFl12ToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tb_tabel_1_2});
            this.dbTabelFl12ToolStripMenuItem.Image = global::SCADA.Properties.Resources.icons8_sync_16;
            this.dbTabelFl12ToolStripMenuItem.Name = "dbTabelFl12ToolStripMenuItem";
            this.dbTabelFl12ToolStripMenuItem.Size = new System.Drawing.Size(184, 26);
            this.dbTabelFl12ToolStripMenuItem.Text = "Db Tabel fl 1 2";
            // 
            // dbTabelFl345ToolStripMenuItem
            // 
            this.dbTabelFl345ToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tb_tabel_3_4_5});
            this.dbTabelFl345ToolStripMenuItem.Image = global::SCADA.Properties.Resources.icons8_sync_16;
            this.dbTabelFl345ToolStripMenuItem.Name = "dbTabelFl345ToolStripMenuItem";
            this.dbTabelFl345ToolStripMenuItem.Size = new System.Drawing.Size(184, 26);
            this.dbTabelFl345ToolStripMenuItem.Text = "Db Tabel fl 3 4 5";
            // 
            // tb_db_server
            // 
            this.tb_db_server.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.tb_db_server.Name = "tb_db_server";
            this.tb_db_server.Size = new System.Drawing.Size(100, 23);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(181, 6);
            // 
            // btn_save_db
            // 
            this.btn_save_db.Image = global::SCADA.Properties.Resources.icons8_improvement_16;
            this.btn_save_db.Name = "btn_save_db";
            this.btn_save_db.Size = new System.Drawing.Size(184, 26);
            this.btn_save_db.Text = "Save";
            this.btn_save_db.Click += new System.EventHandler(this.btn_save_db_Click);
            // 
            // tb_tabel_1_2
            // 
            this.tb_tabel_1_2.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.tb_tabel_1_2.Name = "tb_tabel_1_2";
            this.tb_tabel_1_2.Size = new System.Drawing.Size(100, 23);
            // 
            // tb_tabel_3_4_5
            // 
            this.tb_tabel_3_4_5.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.tb_tabel_3_4_5.Name = "tb_tabel_3_4_5";
            this.tb_tabel_3_4_5.Size = new System.Drawing.Size(100, 23);
            // 
            // dbNameToolStripMenuItem
            // 
            this.dbNameToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tb_db_name});
            this.dbNameToolStripMenuItem.Image = global::SCADA.Properties.Resources.icons8_sync_16;
            this.dbNameToolStripMenuItem.Name = "dbNameToolStripMenuItem";
            this.dbNameToolStripMenuItem.Size = new System.Drawing.Size(184, 26);
            this.dbNameToolStripMenuItem.Text = "Db_Name";
            // 
            // tb_db_name
            // 
            this.tb_db_name.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.tb_db_name.Name = "tb_db_name";
            this.tb_db_name.Size = new System.Drawing.Size(100, 23);
            // 
            // form_tag
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(831, 513);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "form_tag";
            this.Text = "TAG";
            this.Load += new System.EventHandler(this.form_tag_Load);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewTagIo)).EndInit();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItemAboutOpc;
        private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItemConnectOPC;
        private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItemsetOPCServer;
        private System.Windows.Forms.ToolStripMenuItem PropertiesOpc1;
        private System.Windows.Forms.ToolStripTextBox toolStripTextBoxServer;
        private System.Windows.Forms.ToolStripTextBox toolStripTextBoxHost;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.DataGridView dataGridViewTagIo;
        private System.Windows.Forms.ImageList imageList1;
        private System.Windows.Forms.TreeView treeView1;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColumnNo;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColumnNameTag;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColumnValueTag;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColumnQuality;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColumnTimeStamp;
        private System.Windows.Forms.DataGridViewTextBoxColumn count;
        private System.Windows.Forms.DataGridViewTextBoxColumn FlagOpc;
        private System.Windows.Forms.DataGridViewTextBoxColumn type;
        private System.Windows.Forms.ToolStripMenuItem btn_database;
        private System.Windows.Forms.ToolStripMenuItem dbServerToolStripMenuItem;
        private System.Windows.Forms.ToolStripTextBox tb_db_server;
        private System.Windows.Forms.ToolStripMenuItem dbTabelFl12ToolStripMenuItem;
        private System.Windows.Forms.ToolStripTextBox tb_tabel_1_2;
        private System.Windows.Forms.ToolStripMenuItem dbTabelFl345ToolStripMenuItem;
        private System.Windows.Forms.ToolStripTextBox tb_tabel_3_4_5;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem btn_save_db;
        private System.Windows.Forms.ToolStripMenuItem dbNameToolStripMenuItem;
        private System.Windows.Forms.ToolStripTextBox tb_db_name;
    }
}