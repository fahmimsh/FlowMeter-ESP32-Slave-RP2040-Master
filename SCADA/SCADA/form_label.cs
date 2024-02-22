using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SCADA
{
    public partial class form_label : Form
    {
        private static form_label instance;
        private readonly form_main form;
        public form_label(form_main form)
        {
            InitializeComponent();
            this.form = form;
        }
        public static form_label GetInstance(form_main form) => instance == null || instance.IsDisposed ? instance = new form_label(form) : instance;
    }
}
