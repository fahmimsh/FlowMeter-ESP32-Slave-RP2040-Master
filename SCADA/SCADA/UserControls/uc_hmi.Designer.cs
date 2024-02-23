namespace SCADA.UserControls
{
    partial class uc_hmi
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
            this.panels_hmi1 = new System.Windows.Forms.TableLayoutPanel();
            this.panel_hmi1 = new System.Windows.Forms.Panel();
            this.glgControl_hmi1 = new GenLogic.GlgControl();
            this.panel_header_hmi1 = new System.Windows.Forms.Panel();
            this.label_header_hmi1 = new System.Windows.Forms.Label();
            this.tabControl_hmi = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.panels_hmi1.SuspendLayout();
            this.panel_hmi1.SuspendLayout();
            this.panel_header_hmi1.SuspendLayout();
            this.tabControl_hmi.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panels_hmi1
            // 
            this.panels_hmi1.ColumnCount = 2;
            this.panels_hmi1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.panels_hmi1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.panels_hmi1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.panels_hmi1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.panels_hmi1.Controls.Add(this.panel_hmi1, 0, 0);
            this.panels_hmi1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panels_hmi1.Location = new System.Drawing.Point(3, 3);
            this.panels_hmi1.Name = "panels_hmi1";
            this.panels_hmi1.RowCount = 1;
            this.panels_hmi1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.panels_hmi1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.panels_hmi1.Size = new System.Drawing.Size(1229, 507);
            this.panels_hmi1.TabIndex = 0;
            // 
            // panel_hmi1
            // 
            this.panel_hmi1.Controls.Add(this.glgControl_hmi1);
            this.panel_hmi1.Controls.Add(this.panel_header_hmi1);
            this.panel_hmi1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel_hmi1.Location = new System.Drawing.Point(3, 3);
            this.panel_hmi1.Name = "panel_hmi1";
            this.panel_hmi1.Size = new System.Drawing.Size(608, 501);
            this.panel_hmi1.TabIndex = 0;
            // 
            // glgControl_hmi1
            // 
            this.glgControl_hmi1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.glgControl_hmi1.DrawingFile = "";
            this.glgControl_hmi1.DrawingObject = null;
            this.glgControl_hmi1.DrawingURL = "";
            this.glgControl_hmi1.HierarchyEnabled = true;
            this.glgControl_hmi1.Location = new System.Drawing.Point(0, 25);
            this.glgControl_hmi1.Margin = new System.Windows.Forms.Padding(20);
            this.glgControl_hmi1.MinimumSize = new System.Drawing.Size(5, 5);
            this.glgControl_hmi1.Name = "glgControl_hmi1";
            this.glgControl_hmi1.Padding = new System.Windows.Forms.Padding(2);
            this.glgControl_hmi1.SelectEnabled = true;
            this.glgControl_hmi1.Size = new System.Drawing.Size(608, 476);
            this.glgControl_hmi1.TabIndex = 1;
            this.glgControl_hmi1.Text = "glgControl1";
            this.glgControl_hmi1.Trace2Enabled = false;
            this.glgControl_hmi1.TraceEnabled = false;
            // 
            // panel_header_hmi1
            // 
            this.panel_header_hmi1.BackColor = System.Drawing.Color.LightSteelBlue;
            this.panel_header_hmi1.Controls.Add(this.label_header_hmi1);
            this.panel_header_hmi1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel_header_hmi1.Location = new System.Drawing.Point(0, 0);
            this.panel_header_hmi1.Margin = new System.Windows.Forms.Padding(5);
            this.panel_header_hmi1.Name = "panel_header_hmi1";
            this.panel_header_hmi1.Padding = new System.Windows.Forms.Padding(10);
            this.panel_header_hmi1.Size = new System.Drawing.Size(608, 25);
            this.panel_header_hmi1.TabIndex = 0;
            // 
            // label_header_hmi1
            // 
            this.label_header_hmi1.AutoSize = true;
            this.label_header_hmi1.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label_header_hmi1.Location = new System.Drawing.Point(3, 3);
            this.label_header_hmi1.Name = "label_header_hmi1";
            this.label_header_hmi1.Size = new System.Drawing.Size(94, 18);
            this.label_header_hmi1.TabIndex = 0;
            this.label_header_hmi1.Text = "Flow Meter 1";
            // 
            // tabControl_hmi
            // 
            this.tabControl_hmi.Controls.Add(this.tabPage1);
            this.tabControl_hmi.Controls.Add(this.tabPage2);
            this.tabControl_hmi.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl_hmi.Location = new System.Drawing.Point(0, 0);
            this.tabControl_hmi.Name = "tabControl_hmi";
            this.tabControl_hmi.SelectedIndex = 0;
            this.tabControl_hmi.Size = new System.Drawing.Size(1243, 539);
            this.tabControl_hmi.TabIndex = 1;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.panels_hmi1);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(1235, 513);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "tabPage1";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // tabPage2
            // 
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(1235, 513);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "tabPage2";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // uc_hmi
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tabControl_hmi);
            this.Name = "uc_hmi";
            this.Size = new System.Drawing.Size(1243, 539);
            this.Load += new System.EventHandler(this.uc_hmi_Load);
            this.panels_hmi1.ResumeLayout(false);
            this.panel_hmi1.ResumeLayout(false);
            this.panel_header_hmi1.ResumeLayout(false);
            this.panel_header_hmi1.PerformLayout();
            this.tabControl_hmi.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.TabControl tabControl_hmi;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.Panel panel_hmi1;
        private System.Windows.Forms.Panel panel_header_hmi1;
        private System.Windows.Forms.Label label_header_hmi1;
        public System.Windows.Forms.TableLayoutPanel panels_hmi1;
        public GenLogic.GlgControl glgControl_hmi1;
    }
}
