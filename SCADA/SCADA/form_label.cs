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
        private void form_label_Load(object sender, EventArgs e)
        {
            tb_fl1_header.Text = Properties.Settings.Default.fl1_header;
            tb_fl1_sumber_1.Text = Properties.Settings.Default.fl1_label_sumber1;
            tb_fl1_transfer_1.Text = Properties.Settings.Default.fl1_label_tf1;
            tb_fl1_transfer_2.Text = Properties.Settings.Default.fl1_label_tf2;
            tb_fl1_transfer_3.Text = Properties.Settings.Default.fl1_label_tf3;
            tb_fl1_transfer_4.Text = Properties.Settings.Default.fl1_label_tf4;
            tb_fl2_header.Text = Properties.Settings.Default.fl2_header;
            tb_fl2_sumber_1.Text = Properties.Settings.Default.fl2_label_sumber1;
            tb_fl2_transfer_1.Text = Properties.Settings.Default.fl2_label_tf1;
            tb_fl2_transfer_2.Text = Properties.Settings.Default.fl2_label_tf2;
            tb_fl2_transfer_3.Text = Properties.Settings.Default.fl2_label_tf3;
            tb_fl2_transfer_4.Text = Properties.Settings.Default.fl2_label_tf4;

            tb_fl3_header.Text = Properties.Settings.Default.fl3_header;
            tb_fl3_sumber_1.Text = Properties.Settings.Default.fl3_label_sumber1;
            tb_fl3_sumber_2.Text = Properties.Settings.Default.fl3_label_sumber2;
            tb_fl3_transfer_1.Text = Properties.Settings.Default.fl3_label_tf1;
            tb_fl3_transfer_2.Text = Properties.Settings.Default.fl3_label_tf2;
            tb_fl3_transfer_3.Text = Properties.Settings.Default.fl3_label_tf3;
            tb_fl3_transfer_4.Text = Properties.Settings.Default.fl3_label_tf4;
            tb_fl4_header.Text = Properties.Settings.Default.fl4_header;
            tb_fl4_sumber_1.Text = Properties.Settings.Default.fl4_label_sumber1;
            tb_fl4_sumber_2.Text = Properties.Settings.Default.fl4_label_sumber2;
            tb_fl4_transfer_1.Text = Properties.Settings.Default.fl4_label_tf1;
            tb_fl4_transfer_2.Text = Properties.Settings.Default.fl4_label_tf2;
            tb_fl4_transfer_3.Text = Properties.Settings.Default.fl4_label_tf3;
            tb_fl4_transfer_4.Text = Properties.Settings.Default.fl4_label_tf4;
        }
        private void btn_cancle_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        private void btn_save_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.fl1_header = tb_fl1_header.Text;
            Properties.Settings.Default.fl1_label_sumber1 = tb_fl1_sumber_1.Text;
            Properties.Settings.Default.fl1_label_tf1 = tb_fl1_transfer_1.Text;
            Properties.Settings.Default.fl1_label_tf2 = tb_fl1_transfer_2.Text;
            Properties.Settings.Default.fl1_label_tf3 = tb_fl1_transfer_3.Text;
            Properties.Settings.Default.fl1_label_tf4 = tb_fl1_transfer_4.Text;
            Properties.Settings.Default.fl2_header = tb_fl2_header.Text;
            Properties.Settings.Default.fl2_label_sumber1 = tb_fl2_sumber_1.Text;
            Properties.Settings.Default.fl2_label_tf1 = tb_fl2_transfer_1.Text;
            Properties.Settings.Default.fl2_label_tf2 = tb_fl2_transfer_2.Text;
            Properties.Settings.Default.fl2_label_tf3 = tb_fl2_transfer_3.Text;
            Properties.Settings.Default.fl2_label_tf4 = tb_fl2_transfer_4.Text;

            Properties.Settings.Default.fl3_header = tb_fl3_header.Text;
            Properties.Settings.Default.fl3_label_sumber1 = tb_fl3_sumber_1.Text;
            Properties.Settings.Default.fl3_label_sumber2 = tb_fl3_sumber_2.Text;
            Properties.Settings.Default.fl3_label_tf1 = tb_fl3_transfer_1.Text;
            Properties.Settings.Default.fl3_label_tf2 = tb_fl3_transfer_2.Text;
            Properties.Settings.Default.fl3_label_tf3 = tb_fl3_transfer_3.Text;
            Properties.Settings.Default.fl3_label_tf4 = tb_fl3_transfer_4.Text;
            Properties.Settings.Default.fl4_header = tb_fl4_header.Text;
            Properties.Settings.Default.fl4_label_sumber1 = tb_fl4_sumber_1.Text;
            Properties.Settings.Default.fl4_label_sumber2 = tb_fl4_sumber_2.Text;
            Properties.Settings.Default.fl4_label_tf1 = tb_fl4_transfer_1.Text;
            Properties.Settings.Default.fl4_label_tf2 = tb_fl4_transfer_2.Text;
            Properties.Settings.Default.fl4_label_tf3 = tb_fl4_transfer_3.Text;
            Properties.Settings.Default.fl4_label_tf4 = tb_fl4_transfer_4.Text;
            Properties.Settings.Default.Save();
            this.Close();
        }
    }
}
