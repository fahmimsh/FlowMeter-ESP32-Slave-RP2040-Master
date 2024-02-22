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
    public partial class uc_chart : UserControl
    {
        private form_main form;
        public uc_chart(form_main mainForm)
        {
            InitializeComponent();
            form = mainForm;
        }
        private void uc_chart_Load(object sender, EventArgs e)
        {

        }
    }
}
