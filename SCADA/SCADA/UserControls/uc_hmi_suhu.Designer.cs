namespace SCADA.UserControls
{
    partial class uc_hmi_suhu
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
            this.glgControl_suhu = new GenLogic.GlgControl();
            this.SuspendLayout();
            // 
            // glgControl_suhu
            // 
            this.glgControl_suhu.BackColor = System.Drawing.Color.YellowGreen;
            this.glgControl_suhu.Dock = System.Windows.Forms.DockStyle.Fill;
            this.glgControl_suhu.DrawingFile = "";
            this.glgControl_suhu.DrawingObject = null;
            this.glgControl_suhu.DrawingURL = "";
            this.glgControl_suhu.HierarchyEnabled = true;
            this.glgControl_suhu.Location = new System.Drawing.Point(0, 0);
            this.glgControl_suhu.MinimumSize = new System.Drawing.Size(5, 5);
            this.glgControl_suhu.Name = "glgControl_suhu";
            this.glgControl_suhu.SelectEnabled = true;
            this.glgControl_suhu.Size = new System.Drawing.Size(1243, 562);
            this.glgControl_suhu.TabIndex = 0;
            this.glgControl_suhu.Text = "glgControl1";
            this.glgControl_suhu.Trace2Enabled = false;
            this.glgControl_suhu.TraceEnabled = false;
            // 
            // uc_hmi_suhu
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.glgControl_suhu);
            this.Name = "uc_hmi_suhu";
            this.Size = new System.Drawing.Size(1243, 562);
            this.Load += new System.EventHandler(this.uc_hmi_suhu_Load);
            this.ResumeLayout(false);

        }

        #endregion

        public GenLogic.GlgControl glgControl_suhu;
    }
}
