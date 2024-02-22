using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SCADA.UserControls
{
    public partial class uc_log : UserControl
    {
        private form_main form;
        public uc_log(form_main mainForm)
        {
            InitializeComponent();
            form = mainForm;
        }
        private void uc_log_Load(object sender, EventArgs e)
        {

        }
    }
}
