using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SCADA.UserControls
{
    public partial class uc_hmi_suhu : UserControl
    {
        private form_main form;
        internal GlgSetTag glgSetTag_suhu;
        public uc_hmi_suhu(form_main mainForm)
        {
            InitializeComponent();
            form = mainForm;
            glgSetTag_suhu = new GlgSetTag();
        }

        private void uc_hmi_suhu_Load(object sender, EventArgs e)
        {
            glgSetTag_suhu.Initialize(this, glgControl_suhu, Path.Combine(Application.StartupPath, "GUI_SUHU.g"));
        }
    }
}
