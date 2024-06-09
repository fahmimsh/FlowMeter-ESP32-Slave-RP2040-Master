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
            check_b_auto_connect_opc.Checked = Properties.Settings.Default.auto_connect_opc;

            tb_fl1_header.Text = Properties.Settings.Default.fl1_header;
            tb_fl2_header.Text = Properties.Settings.Default.fl2_header;

            tb_fl1_transfer_1.Text = Properties.Settings.Default.fl1_label_tf1;
            tb_fl1_transfer_2.Text = Properties.Settings.Default.fl1_label_tf2;
            tb_fl1_transfer_3.Text = Properties.Settings.Default.fl1_label_tf3;
            tb_fl1_transfer_4.Text = Properties.Settings.Default.fl1_label_tf4;
            tb_fl1_transfer_5.Text = Properties.Settings.Default.fl1_label_tf5;

            tb_fl2_transfer_1.Text = Properties.Settings.Default.fl2_label_tf1;
            tb_fl2_transfer_2.Text = Properties.Settings.Default.fl2_label_tf2;
            tb_fl2_transfer_3.Text = Properties.Settings.Default.fl2_label_tf3;
            tb_fl2_transfer_4.Text = Properties.Settings.Default.fl2_label_tf4;

            tb_fl1_batch_1.Text = Properties.Settings.Default.fl1_label_batch1;
            tb_fl1_batch_2.Text = Properties.Settings.Default.fl1_label_batch2;
            tb_fl1_batch_3.Text = Properties.Settings.Default.fl1_label_batch3;
            tb_fl1_batch_4.Text = Properties.Settings.Default.fl1_label_batch4;
            tb_fl1_batch_5.Text = Properties.Settings.Default.fl1_label_batch5;
            tb_fl1_batch_6.Text = Properties.Settings.Default.fl1_label_batch6;
            tb_fl1_batch_7.Text = Properties.Settings.Default.fl1_label_batch7;
            tb_fl1_batch_8.Text = Properties.Settings.Default.fl1_label_batch8;
            tb_fl1_batch_9.Text = Properties.Settings.Default.fl1_label_batch9;
            tb_fl1_batch_10.Text = Properties.Settings.Default.fl1_label_batch10;

            tb_fl2_batch_1.Text = Properties.Settings.Default.fl2_label_batch1;
            tb_fl2_batch_2.Text = Properties.Settings.Default.fl2_label_batch2;
            tb_fl2_batch_3.Text = Properties.Settings.Default.fl2_label_batch3;
            tb_fl2_batch_4.Text = Properties.Settings.Default.fl2_label_batch4;
            tb_fl2_batch_5.Text = Properties.Settings.Default.fl2_label_batch5;
            tb_fl2_batch_6.Text = Properties.Settings.Default.fl2_label_batch6;
            tb_fl2_batch_7.Text = Properties.Settings.Default.fl2_label_batch7;
            tb_fl2_batch_8.Text = Properties.Settings.Default.fl2_label_batch8;
            tb_fl2_batch_9.Text = Properties.Settings.Default.fl2_label_batch9;
            tb_fl2_batch_10.Text = Properties.Settings.Default.fl2_label_batch10;

            tb_fl3_header.Text = Properties.Settings.Default.fl3_header;
            tb_fl3_proses_mesin_1.Text = Properties.Settings.Default.fl3_label_pm1;
            tb_fl3_proses_mesin_2.Text = Properties.Settings.Default.fl3_label_pm2;
            tb_fl3_transfer_1.Text = Properties.Settings.Default.fl3_label_tf1;
            tb_fl3_transfer_2.Text = Properties.Settings.Default.fl3_label_tf2;
            tb_fl3_transfer_3.Text = Properties.Settings.Default.fl3_label_tf3;

            tb_fl4_header.Text = Properties.Settings.Default.fl4_header;
            tb_fl4_proses_mesin_1.Text = Properties.Settings.Default.fl4_label_pm1;
            tb_fl4_proses_mesin_2.Text = Properties.Settings.Default.fl4_label_pm2;
            tb_fl4_transfer_1.Text = Properties.Settings.Default.fl4_label_tf1;
            tb_fl4_transfer_2.Text = Properties.Settings.Default.fl4_label_tf2;
            tb_fl4_transfer_3.Text = Properties.Settings.Default.fl4_label_tf3;

            tb_fl5_header.Text = Properties.Settings.Default.fl5_header;
            tb_fl5_proses_mesin_1.Text = Properties.Settings.Default.fl5_label_pm1;
            tb_fl5_proses_mesin_2.Text = Properties.Settings.Default.fl5_label_pm2;
            tb_fl5_transfer_1.Text = Properties.Settings.Default.fl5_label_tf1;
            tb_fl5_transfer_2.Text = Properties.Settings.Default.fl5_label_tf2;
            tb_fl5_transfer_3.Text = Properties.Settings.Default.fl5_label_tf3;

            tb_fl3_produk1.Text = Properties.Settings.Default.fl3_produk1;
            tb_fl3_produk2.Text = Properties.Settings.Default.fl3_produk2;
            tb_fl3_produk3.Text = Properties.Settings.Default.fl3_produk3;
            tb_fl3_produk4.Text = Properties.Settings.Default.fl3_produk4;
            tb_fl3_produk5.Text = Properties.Settings.Default.fl3_produk5;
            tb_fl3_produk6.Text = Properties.Settings.Default.fl3_produk6;
            tb_fl3_produk7.Text = Properties.Settings.Default.fl3_produk7;
            tb_fl3_produk8.Text = Properties.Settings.Default.fl3_produk8;
            tb_fl3_produk9.Text = Properties.Settings.Default.fl3_produk9;
            tb_fl3_produk10.Text = Properties.Settings.Default.fl3_produk10;
            tb_fl3_produk11.Text = Properties.Settings.Default.fl3_produk11;

            tb_fl4_produk1.Text = Properties.Settings.Default.fl4_produk1;
            tb_fl4_produk2.Text = Properties.Settings.Default.fl4_produk2;
            tb_fl4_produk3.Text = Properties.Settings.Default.fl4_produk3;
            tb_fl4_produk4.Text = Properties.Settings.Default.fl4_produk4;
            tb_fl4_produk5.Text = Properties.Settings.Default.fl4_produk5;
            tb_fl4_produk6.Text = Properties.Settings.Default.fl4_produk6;
            tb_fl4_produk7.Text = Properties.Settings.Default.fl4_produk7;
            tb_fl4_produk8.Text = Properties.Settings.Default.fl4_produk8;
            tb_fl4_produk9.Text = Properties.Settings.Default.fl4_produk9;
            tb_fl4_produk10.Text = Properties.Settings.Default.fl4_produk10;
            tb_fl4_produk11.Text = Properties.Settings.Default.fl4_produk11;

            tb_fl5_produk1.Text = Properties.Settings.Default.fl5_produk1;
            tb_fl5_produk2.Text = Properties.Settings.Default.fl5_produk2;
            tb_fl5_produk3.Text = Properties.Settings.Default.fl5_produk3;
            tb_fl5_produk4.Text = Properties.Settings.Default.fl5_produk4;
            tb_fl5_produk5.Text = Properties.Settings.Default.fl5_produk5;
            tb_fl5_produk6.Text = Properties.Settings.Default.fl5_produk6;
            tb_fl5_produk7.Text = Properties.Settings.Default.fl5_produk7;
            tb_fl5_produk8.Text = Properties.Settings.Default.fl5_produk8;
            tb_fl5_produk9.Text = Properties.Settings.Default.fl5_produk9;
            tb_fl5_produk10.Text = Properties.Settings.Default.fl5_produk10;
            tb_fl5_produk11.Text = Properties.Settings.Default.fl5_produk11;

        }
        private void btn_cancle_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        private void btn_save_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.auto_connect_opc = check_b_auto_connect_opc.Checked;
            Properties.Settings.Default.fl1_header = tb_fl1_header.Text;
            Properties.Settings.Default.fl1_label_tf1 = tb_fl1_transfer_1.Text;
            Properties.Settings.Default.fl1_label_tf2 = tb_fl1_transfer_2.Text;
            Properties.Settings.Default.fl1_label_tf3 = tb_fl1_transfer_3.Text;
            Properties.Settings.Default.fl1_label_tf4 = tb_fl1_transfer_4.Text;
            Properties.Settings.Default.fl1_label_tf5 = tb_fl1_transfer_5.Text;

            Properties.Settings.Default.fl2_header = tb_fl2_header.Text;
            Properties.Settings.Default.fl2_label_tf1 = tb_fl2_transfer_1.Text;
            Properties.Settings.Default.fl2_label_tf2 = tb_fl2_transfer_2.Text;
            Properties.Settings.Default.fl2_label_tf3 = tb_fl2_transfer_3.Text;
            Properties.Settings.Default.fl2_label_tf4 = tb_fl2_transfer_4.Text;

            Properties.Settings.Default.fl1_label_batch1 = tb_fl1_batch_1.Text;
            Properties.Settings.Default.fl1_label_batch2 = tb_fl1_batch_2.Text;
            Properties.Settings.Default.fl1_label_batch3 = tb_fl1_batch_3.Text;
            Properties.Settings.Default.fl1_label_batch4 = tb_fl1_batch_4.Text;
            Properties.Settings.Default.fl1_label_batch5 = tb_fl1_batch_5.Text;
            Properties.Settings.Default.fl1_label_batch6 = tb_fl1_batch_6.Text;
            Properties.Settings.Default.fl1_label_batch7 = tb_fl1_batch_7.Text;
            Properties.Settings.Default.fl1_label_batch8 = tb_fl1_batch_8.Text;
            Properties.Settings.Default.fl1_label_batch9 = tb_fl1_batch_9.Text;
            Properties.Settings.Default.fl1_label_batch10 = tb_fl1_batch_10.Text;

            Properties.Settings.Default.fl2_label_batch1 = tb_fl2_batch_1.Text;
            Properties.Settings.Default.fl2_label_batch2 = tb_fl2_batch_2.Text;
            Properties.Settings.Default.fl2_label_batch3 = tb_fl2_batch_3.Text;
            Properties.Settings.Default.fl2_label_batch4 = tb_fl2_batch_4.Text;
            Properties.Settings.Default.fl2_label_batch5 = tb_fl2_batch_5.Text;
            Properties.Settings.Default.fl2_label_batch6 = tb_fl2_batch_6.Text;
            Properties.Settings.Default.fl2_label_batch7 = tb_fl2_batch_7.Text;
            Properties.Settings.Default.fl2_label_batch8 = tb_fl2_batch_8.Text;
            Properties.Settings.Default.fl2_label_batch9 = tb_fl2_batch_9.Text;
            Properties.Settings.Default.fl2_label_batch10 = tb_fl2_batch_10.Text;

            Properties.Settings.Default.fl3_header = tb_fl3_header.Text;
            Properties.Settings.Default.fl3_label_pm1 = tb_fl3_proses_mesin_1.Text;
            Properties.Settings.Default.fl3_label_pm2 = tb_fl3_proses_mesin_2.Text;
            Properties.Settings.Default.fl3_label_tf1 = tb_fl3_transfer_1.Text;
            Properties.Settings.Default.fl3_label_tf2 = tb_fl3_transfer_2.Text;
            Properties.Settings.Default.fl3_label_tf3 = tb_fl3_transfer_3.Text;

            Properties.Settings.Default.fl4_header = tb_fl4_header.Text;
            Properties.Settings.Default.fl4_label_pm1 = tb_fl4_proses_mesin_1.Text;
            Properties.Settings.Default.fl4_label_pm2 = tb_fl4_proses_mesin_2.Text;
            Properties.Settings.Default.fl4_label_tf1 = tb_fl4_transfer_1.Text;
            Properties.Settings.Default.fl4_label_tf2 = tb_fl4_transfer_2.Text;
            Properties.Settings.Default.fl4_label_tf3 = tb_fl4_transfer_3.Text;

            Properties.Settings.Default.fl5_header = tb_fl5_header.Text;
            Properties.Settings.Default.fl5_label_pm1 = tb_fl5_proses_mesin_1.Text;
            Properties.Settings.Default.fl5_label_pm2 = tb_fl5_proses_mesin_2.Text;
            Properties.Settings.Default.fl5_label_tf1 = tb_fl5_transfer_1.Text;
            Properties.Settings.Default.fl5_label_tf2 = tb_fl5_transfer_2.Text;
            Properties.Settings.Default.fl5_label_tf3 = tb_fl5_transfer_3.Text;

            Properties.Settings.Default.fl3_produk1 = tb_fl3_produk1.Text;
            Properties.Settings.Default.fl3_produk2 = tb_fl3_produk2.Text;
            Properties.Settings.Default.fl3_produk3 = tb_fl3_produk3.Text;
            Properties.Settings.Default.fl3_produk4 = tb_fl3_produk4.Text;
            Properties.Settings.Default.fl3_produk5 = tb_fl3_produk5.Text;
            Properties.Settings.Default.fl3_produk6 = tb_fl3_produk6.Text;
            Properties.Settings.Default.fl3_produk7 = tb_fl3_produk7.Text;
            Properties.Settings.Default.fl3_produk8 = tb_fl3_produk8.Text;
            Properties.Settings.Default.fl3_produk9 = tb_fl3_produk9.Text;
            Properties.Settings.Default.fl3_produk10 = tb_fl3_produk10.Text;
            Properties.Settings.Default.fl3_produk11 = tb_fl3_produk11.Text;

            Properties.Settings.Default.fl4_produk1 = tb_fl4_produk1.Text;
            Properties.Settings.Default.fl4_produk2 = tb_fl4_produk2.Text;
            Properties.Settings.Default.fl4_produk3 = tb_fl4_produk3.Text;
            Properties.Settings.Default.fl4_produk4 = tb_fl4_produk4.Text;
            Properties.Settings.Default.fl4_produk5 = tb_fl4_produk5.Text;
            Properties.Settings.Default.fl4_produk6 = tb_fl4_produk6.Text;
            Properties.Settings.Default.fl4_produk7 = tb_fl4_produk7.Text;
            Properties.Settings.Default.fl4_produk8 = tb_fl4_produk8.Text;
            Properties.Settings.Default.fl4_produk9 = tb_fl4_produk9.Text;
            Properties.Settings.Default.fl4_produk10 = tb_fl4_produk10.Text;
            Properties.Settings.Default.fl4_produk11 = tb_fl4_produk11.Text;

            Properties.Settings.Default.fl5_produk1 = tb_fl5_produk1.Text;
            Properties.Settings.Default.fl5_produk2 = tb_fl5_produk2.Text;
            Properties.Settings.Default.fl5_produk3 = tb_fl5_produk3.Text;
            Properties.Settings.Default.fl5_produk4 = tb_fl5_produk4.Text;
            Properties.Settings.Default.fl5_produk5 = tb_fl5_produk5.Text;
            Properties.Settings.Default.fl5_produk6 = tb_fl5_produk6.Text;
            Properties.Settings.Default.fl5_produk7 = tb_fl5_produk7.Text;
            Properties.Settings.Default.fl5_produk8 = tb_fl5_produk8.Text;
            Properties.Settings.Default.fl5_produk9 = tb_fl5_produk9.Text;
            Properties.Settings.Default.fl5_produk10 = tb_fl5_produk10.Text;
            Properties.Settings.Default.fl5_produk11 = tb_fl5_produk11.Text;

            form.uc_x_hmi.label_header_hmi1.Text = $"{Properties.Settings.Default.fl1_header}      ";
            form.uc_x_hmi.label_header_hmi2.Text = $"{Properties.Settings.Default.fl2_header}      ";
            form.uc_x_hmi.label_header_hmi3.Text = $"{Properties.Settings.Default.fl3_header}      ";
            form.uc_x_hmi.label_header_hmi4.Text = $"{Properties.Settings.Default.fl4_header}      ";
            form.uc_x_hmi.label_header_hmi5.Text = $"{Properties.Settings.Default.fl5_header}      ";

            Properties.Settings.Default.Save();
            form.ShowMessage("Data Berhasil di Save", "DB Save", MessageBoxIcon.Information);
            this.Close();
        }
    }
}
