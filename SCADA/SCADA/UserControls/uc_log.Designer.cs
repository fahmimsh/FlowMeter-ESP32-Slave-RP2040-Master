namespace SCADA.UserControls
{
    partial class uc_log
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

        #region Component Designer generated code

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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle7 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle8 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle9 = new System.Windows.Forms.DataGridViewCellStyle();
            this.splitContainer_log_data = new System.Windows.Forms.SplitContainer();
            this.panel_filter = new System.Windows.Forms.Panel();
            this.label_total_liter = new System.Windows.Forms.Label();
            this.btn_search = new System.Windows.Forms.Button();
            this.date_stop = new System.Windows.Forms.DateTimePicker();
            this.date_start = new System.Windows.Forms.DateTimePicker();
            this.btn_export = new System.Windows.Forms.Button();
            this.check_b_all = new System.Windows.Forms.CheckBox();
            this.label6 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.cb_from_source = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.cb_transfer_to = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.cb_flow_meter = new System.Windows.Forms.ComboBox();
            this.cb_mode = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.panel_filter_data = new System.Windows.Forms.Panel();
            this.label_header_filter = new System.Windows.Forms.Label();
            this.panel_log_data = new System.Windows.Forms.Panel();
            this.dataGridViewDataLog = new System.Windows.Forms.DataGridView();
            this.Id = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.flow_meter = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.mode = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.set_liter = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.liter = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.k_factor = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.from_source = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.transfer_to = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.date_time = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.panel_header_log = new System.Windows.Forms.Panel();
            this.label_header_log = new System.Windows.Forms.Label();
            this.timer_refresh_db = new System.Windows.Forms.Timer(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer_log_data)).BeginInit();
            this.splitContainer_log_data.Panel1.SuspendLayout();
            this.splitContainer_log_data.Panel2.SuspendLayout();
            this.splitContainer_log_data.SuspendLayout();
            this.panel_filter.SuspendLayout();
            this.panel_filter_data.SuspendLayout();
            this.panel_log_data.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewDataLog)).BeginInit();
            this.panel_header_log.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer_log_data
            // 
            this.splitContainer_log_data.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer_log_data.Location = new System.Drawing.Point(0, 0);
            this.splitContainer_log_data.Name = "splitContainer_log_data";
            // 
            // splitContainer_log_data.Panel1
            // 
            this.splitContainer_log_data.Panel1.Controls.Add(this.panel_filter);
            this.splitContainer_log_data.Panel1.Controls.Add(this.panel_filter_data);
            // 
            // splitContainer_log_data.Panel2
            // 
            this.splitContainer_log_data.Panel2.Controls.Add(this.panel_log_data);
            this.splitContainer_log_data.Panel2.Controls.Add(this.panel_header_log);
            this.splitContainer_log_data.Size = new System.Drawing.Size(1243, 539);
            this.splitContainer_log_data.SplitterDistance = 215;
            this.splitContainer_log_data.TabIndex = 0;
            // 
            // panel_filter
            // 
            this.panel_filter.BackColor = System.Drawing.SystemColors.GradientActiveCaption;
            this.panel_filter.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel_filter.Controls.Add(this.label_total_liter);
            this.panel_filter.Controls.Add(this.btn_search);
            this.panel_filter.Controls.Add(this.date_stop);
            this.panel_filter.Controls.Add(this.date_start);
            this.panel_filter.Controls.Add(this.btn_export);
            this.panel_filter.Controls.Add(this.check_b_all);
            this.panel_filter.Controls.Add(this.label6);
            this.panel_filter.Controls.Add(this.label5);
            this.panel_filter.Controls.Add(this.cb_from_source);
            this.panel_filter.Controls.Add(this.label4);
            this.panel_filter.Controls.Add(this.cb_transfer_to);
            this.panel_filter.Controls.Add(this.label3);
            this.panel_filter.Controls.Add(this.cb_flow_meter);
            this.panel_filter.Controls.Add(this.cb_mode);
            this.panel_filter.Controls.Add(this.label1);
            this.panel_filter.Controls.Add(this.label2);
            this.panel_filter.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel_filter.Location = new System.Drawing.Point(0, 28);
            this.panel_filter.Name = "panel_filter";
            this.panel_filter.Size = new System.Drawing.Size(215, 511);
            this.panel_filter.TabIndex = 3;
            // 
            // label_total_liter
            // 
            this.label_total_liter.AutoSize = true;
            this.label_total_liter.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label_total_liter.Location = new System.Drawing.Point(7, 231);
            this.label_total_liter.Name = "label_total_liter";
            this.label_total_liter.Size = new System.Drawing.Size(104, 15);
            this.label_total_liter.TabIndex = 2;
            this.label_total_liter.Text = "Total Liter : 0.00 L";
            // 
            // btn_search
            // 
            this.btn_search.Image = global::SCADA.Properties.Resources.icons8_search_16;
            this.btn_search.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btn_search.Location = new System.Drawing.Point(112, 269);
            this.btn_search.Name = "btn_search";
            this.btn_search.Size = new System.Drawing.Size(86, 31);
            this.btn_search.TabIndex = 12;
            this.btn_search.Text = "Search";
            this.btn_search.UseVisualStyleBackColor = true;
            this.btn_search.Click += new System.EventHandler(this.btn_search_Click);
            // 
            // date_stop
            // 
            this.date_stop.CalendarMonthBackground = System.Drawing.SystemColors.ControlDarkDark;
            this.date_stop.CustomFormat = "  dd/MM/yyyy";
            this.date_stop.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.date_stop.Location = new System.Drawing.Point(78, 171);
            this.date_stop.Margin = new System.Windows.Forms.Padding(2);
            this.date_stop.Name = "date_stop";
            this.date_stop.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.date_stop.Size = new System.Drawing.Size(121, 20);
            this.date_stop.TabIndex = 11;
            // 
            // date_start
            // 
            this.date_start.CalendarMonthBackground = System.Drawing.SystemColors.ControlDarkDark;
            this.date_start.CustomFormat = "  dd/MM/yyyy";
            this.date_start.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.date_start.Location = new System.Drawing.Point(78, 139);
            this.date_start.Margin = new System.Windows.Forms.Padding(2);
            this.date_start.Name = "date_start";
            this.date_start.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.date_start.Size = new System.Drawing.Size(120, 20);
            this.date_start.TabIndex = 10;
            // 
            // btn_export
            // 
            this.btn_export.Image = global::SCADA.Properties.Resources.icons8_excel_16;
            this.btn_export.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btn_export.Location = new System.Drawing.Point(9, 269);
            this.btn_export.Name = "btn_export";
            this.btn_export.Size = new System.Drawing.Size(87, 31);
            this.btn_export.TabIndex = 3;
            this.btn_export.Text = "Export";
            this.btn_export.UseVisualStyleBackColor = true;
            this.btn_export.Click += new System.EventHandler(this.btn_export_Click);
            // 
            // check_b_all
            // 
            this.check_b_all.AutoSize = true;
            this.check_b_all.Checked = true;
            this.check_b_all.CheckState = System.Windows.Forms.CheckState.Checked;
            this.check_b_all.Location = new System.Drawing.Point(63, 204);
            this.check_b_all.Name = "check_b_all";
            this.check_b_all.Size = new System.Drawing.Size(136, 17);
            this.check_b_all.TabIndex = 2;
            this.check_b_all.Text = "Select All Data by Date";
            this.check_b_all.UseVisualStyleBackColor = true;
            this.check_b_all.CheckedChanged += new System.EventHandler(this.check_b_all_CheckedChanged);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(7, 175);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(46, 13);
            this.label6.TabIndex = 9;
            this.label6.Text = "Date To";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(7, 143);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(56, 13);
            this.label5.TabIndex = 8;
            this.label5.Text = "Date From";
            // 
            // cb_from_source
            // 
            this.cb_from_source.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cb_from_source.FormattingEnabled = true;
            this.cb_from_source.Location = new System.Drawing.Point(78, 106);
            this.cb_from_source.Name = "cb_from_source";
            this.cb_from_source.Size = new System.Drawing.Size(121, 21);
            this.cb_from_source.TabIndex = 7;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(7, 110);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(67, 13);
            this.label4.TabIndex = 6;
            this.label4.Text = "From Source";
            // 
            // cb_transfer_to
            // 
            this.cb_transfer_to.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cb_transfer_to.FormattingEnabled = true;
            this.cb_transfer_to.Location = new System.Drawing.Point(78, 73);
            this.cb_transfer_to.Name = "cb_transfer_to";
            this.cb_transfer_to.Size = new System.Drawing.Size(121, 21);
            this.cb_transfer_to.TabIndex = 5;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(7, 77);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(62, 13);
            this.label3.TabIndex = 4;
            this.label3.Text = "Transfer To";
            // 
            // cb_flow_meter
            // 
            this.cb_flow_meter.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cb_flow_meter.FormattingEnabled = true;
            this.cb_flow_meter.Location = new System.Drawing.Point(78, 9);
            this.cb_flow_meter.Name = "cb_flow_meter";
            this.cb_flow_meter.Size = new System.Drawing.Size(121, 21);
            this.cb_flow_meter.TabIndex = 2;
            // 
            // cb_mode
            // 
            this.cb_mode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cb_mode.FormattingEnabled = true;
            this.cb_mode.Location = new System.Drawing.Point(78, 41);
            this.cb_mode.Name = "cb_mode";
            this.cb_mode.Size = new System.Drawing.Size(121, 21);
            this.cb_mode.TabIndex = 3;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(7, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(59, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Flow Meter";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(7, 45);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(34, 13);
            this.label2.TabIndex = 0;
            this.label2.Text = "Mode";
            // 
            // panel_filter_data
            // 
            this.panel_filter_data.BackColor = System.Drawing.Color.Gold;
            this.panel_filter_data.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel_filter_data.Controls.Add(this.label_header_filter);
            this.panel_filter_data.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel_filter_data.Location = new System.Drawing.Point(0, 0);
            this.panel_filter_data.Name = "panel_filter_data";
            this.panel_filter_data.Size = new System.Drawing.Size(215, 28);
            this.panel_filter_data.TabIndex = 3;
            // 
            // label_header_filter
            // 
            this.label_header_filter.AutoSize = true;
            this.label_header_filter.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label_header_filter.Location = new System.Drawing.Point(6, 5);
            this.label_header_filter.Name = "label_header_filter";
            this.label_header_filter.Size = new System.Drawing.Size(75, 18);
            this.label_header_filter.TabIndex = 1;
            this.label_header_filter.Text = "Filter Data";
            // 
            // panel_log_data
            // 
            this.panel_log_data.Controls.Add(this.dataGridViewDataLog);
            this.panel_log_data.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel_log_data.Location = new System.Drawing.Point(0, 28);
            this.panel_log_data.Name = "panel_log_data";
            this.panel_log_data.Size = new System.Drawing.Size(1024, 511);
            this.panel_log_data.TabIndex = 4;
            // 
            // dataGridViewDataLog
            // 
            this.dataGridViewDataLog.AllowUserToAddRows = false;
            this.dataGridViewDataLog.AllowUserToDeleteRows = false;
            this.dataGridViewDataLog.AllowUserToResizeRows = false;
            this.dataGridViewDataLog.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dataGridViewDataLog.BackgroundColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.InactiveCaption;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.InactiveCaption;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dataGridViewDataLog.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.dataGridViewDataLog.ColumnHeadersHeight = 35;
            this.dataGridViewDataLog.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            this.dataGridViewDataLog.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Id,
            this.flow_meter,
            this.mode,
            this.set_liter,
            this.liter,
            this.k_factor,
            this.from_source,
            this.transfer_to,
            this.date_time});
            this.dataGridViewDataLog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridViewDataLog.EnableHeadersVisualStyles = false;
            this.dataGridViewDataLog.Location = new System.Drawing.Point(0, 0);
            this.dataGridViewDataLog.Margin = new System.Windows.Forms.Padding(2);
            this.dataGridViewDataLog.Name = "dataGridViewDataLog";
            this.dataGridViewDataLog.RowHeadersVisible = false;
            this.dataGridViewDataLog.RowHeadersWidth = 51;
            this.dataGridViewDataLog.RowTemplate.Height = 24;
            this.dataGridViewDataLog.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dataGridViewDataLog.Size = new System.Drawing.Size(1024, 511);
            this.dataGridViewDataLog.TabIndex = 1;
            // 
            // Id
            // 
            this.Id.DataPropertyName = "id";
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            this.Id.DefaultCellStyle = dataGridViewCellStyle2;
            this.Id.FillWeight = 25F;
            this.Id.HeaderText = "Id";
            this.Id.Name = "Id";
            this.Id.ReadOnly = true;
            // 
            // flow_meter
            // 
            this.flow_meter.DataPropertyName = "flow_meter";
            this.flow_meter.HeaderText = "Nama FlowMeter";
            this.flow_meter.MinimumWidth = 6;
            this.flow_meter.Name = "flow_meter";
            this.flow_meter.ReadOnly = true;
            // 
            // mode
            // 
            this.mode.DataPropertyName = "mode";
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            this.mode.DefaultCellStyle = dataGridViewCellStyle3;
            this.mode.FillWeight = 40F;
            this.mode.HeaderText = "Mode";
            this.mode.MinimumWidth = 6;
            this.mode.Name = "mode";
            this.mode.ReadOnly = true;
            // 
            // set_liter
            // 
            this.set_liter.DataPropertyName = "setLiter";
            dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle4.Format = "N2";
            dataGridViewCellStyle4.NullValue = "0.00";
            this.set_liter.DefaultCellStyle = dataGridViewCellStyle4;
            this.set_liter.FillWeight = 50F;
            this.set_liter.HeaderText = "Set Liter";
            this.set_liter.MinimumWidth = 6;
            this.set_liter.Name = "set_liter";
            this.set_liter.ReadOnly = true;
            // 
            // liter
            // 
            this.liter.DataPropertyName = "liter";
            dataGridViewCellStyle5.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle5.Format = "N2";
            dataGridViewCellStyle5.NullValue = "0.00";
            this.liter.DefaultCellStyle = dataGridViewCellStyle5;
            this.liter.FillWeight = 50F;
            this.liter.HeaderText = "Liter";
            this.liter.MinimumWidth = 6;
            this.liter.Name = "liter";
            this.liter.ReadOnly = true;
            // 
            // k_factor
            // 
            this.k_factor.DataPropertyName = "k_factor";
            dataGridViewCellStyle6.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle6.NullValue = "0";
            this.k_factor.DefaultCellStyle = dataGridViewCellStyle6;
            this.k_factor.FillWeight = 50F;
            this.k_factor.HeaderText = "K-Factor";
            this.k_factor.MinimumWidth = 6;
            this.k_factor.Name = "k_factor";
            this.k_factor.ReadOnly = true;
            // 
            // from_source
            // 
            this.from_source.DataPropertyName = "from_source";
            dataGridViewCellStyle7.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            this.from_source.DefaultCellStyle = dataGridViewCellStyle7;
            this.from_source.FillWeight = 70F;
            this.from_source.HeaderText = "From Source";
            this.from_source.Name = "from_source";
            this.from_source.ReadOnly = true;
            // 
            // transfer_to
            // 
            this.transfer_to.DataPropertyName = "transfer_to";
            dataGridViewCellStyle8.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            this.transfer_to.DefaultCellStyle = dataGridViewCellStyle8;
            this.transfer_to.FillWeight = 34F;
            this.transfer_to.HeaderText = "Transfer To";
            this.transfer_to.Name = "transfer_to";
            this.transfer_to.ReadOnly = true;
            // 
            // date_time
            // 
            this.date_time.DataPropertyName = "date_time";
            dataGridViewCellStyle9.Format = "G";
            dataGridViewCellStyle9.NullValue = null;
            this.date_time.DefaultCellStyle = dataGridViewCellStyle9;
            this.date_time.FillWeight = 80F;
            this.date_time.HeaderText = "Date Time";
            this.date_time.MinimumWidth = 6;
            this.date_time.Name = "date_time";
            this.date_time.ReadOnly = true;
            // 
            // panel_header_log
            // 
            this.panel_header_log.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(192)))), ((int)(((byte)(0)))));
            this.panel_header_log.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel_header_log.Controls.Add(this.label_header_log);
            this.panel_header_log.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel_header_log.Location = new System.Drawing.Point(0, 0);
            this.panel_header_log.Name = "panel_header_log";
            this.panel_header_log.Size = new System.Drawing.Size(1024, 28);
            this.panel_header_log.TabIndex = 5;
            // 
            // label_header_log
            // 
            this.label_header_log.AutoSize = true;
            this.label_header_log.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label_header_log.Location = new System.Drawing.Point(5, 5);
            this.label_header_log.Name = "label_header_log";
            this.label_header_log.Size = new System.Drawing.Size(68, 18);
            this.label_header_log.TabIndex = 3;
            this.label_header_log.Text = "Log Data";
            // 
            // timer_refresh_db
            // 
            this.timer_refresh_db.Enabled = true;
            this.timer_refresh_db.Tick += new System.EventHandler(this.timer_refresh_db_Tick);
            // 
            // uc_log
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.splitContainer_log_data);
            this.Name = "uc_log";
            this.Size = new System.Drawing.Size(1243, 539);
            this.Load += new System.EventHandler(this.uc_log_Load);
            this.splitContainer_log_data.Panel1.ResumeLayout(false);
            this.splitContainer_log_data.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer_log_data)).EndInit();
            this.splitContainer_log_data.ResumeLayout(false);
            this.panel_filter.ResumeLayout(false);
            this.panel_filter.PerformLayout();
            this.panel_filter_data.ResumeLayout(false);
            this.panel_filter_data.PerformLayout();
            this.panel_log_data.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewDataLog)).EndInit();
            this.panel_header_log.ResumeLayout(false);
            this.panel_header_log.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.SplitContainer splitContainer_log_data;
        private System.Windows.Forms.Label label_header_filter;
        private System.Windows.Forms.Panel panel_filter;
        private System.Windows.Forms.Panel panel_filter_data;
        private System.Windows.Forms.Panel panel_log_data;
        private System.Windows.Forms.Label label_header_log;
        private System.Windows.Forms.Panel panel_header_log;
        private System.Windows.Forms.DataGridView dataGridViewDataLog;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.ComboBox cb_from_source;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ComboBox cb_transfer_to;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox cb_flow_meter;
        private System.Windows.Forms.ComboBox cb_mode;
        private System.Windows.Forms.Button btn_export;
        private System.Windows.Forms.CheckBox check_b_all;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.DateTimePicker date_start;
        private System.Windows.Forms.DateTimePicker date_stop;
        private System.Windows.Forms.Button btn_search;
        private System.Windows.Forms.Label label_total_liter;
        private System.Windows.Forms.DataGridViewTextBoxColumn Id;
        private System.Windows.Forms.DataGridViewTextBoxColumn flow_meter;
        private System.Windows.Forms.DataGridViewTextBoxColumn mode;
        private System.Windows.Forms.DataGridViewTextBoxColumn set_liter;
        private System.Windows.Forms.DataGridViewTextBoxColumn liter;
        private System.Windows.Forms.DataGridViewTextBoxColumn k_factor;
        private System.Windows.Forms.DataGridViewTextBoxColumn from_source;
        private System.Windows.Forms.DataGridViewTextBoxColumn transfer_to;
        private System.Windows.Forms.DataGridViewTextBoxColumn date_time;
        private System.Windows.Forms.Timer timer_refresh_db;
    }
}
