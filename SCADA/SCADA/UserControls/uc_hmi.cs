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
using GenLogic;

namespace SCADA.UserControls
{
    public partial class uc_hmi : UserControl
    {
        internal GlgSetTag glgSetTag1, glgSetTag2, glgSetTag3, glgSetTag4, glgSetTag5;
        private form_main form;
        public uc_hmi(form_main mainForm)
        {
            InitializeComponent();
            form = mainForm;
            glgSetTag1 = new GlgSetTag();
            glgSetTag2 = new GlgSetTag();
            glgSetTag3 = new GlgSetTag();
            glgSetTag4 = new GlgSetTag();
            glgSetTag5 = new GlgSetTag();
        }
        private void uc_hmi_Load(object sender, EventArgs e)
        {
            glgSetTag1.Initialize(this, glgControl_hmi1, Path.Combine(Application.StartupPath, "GUI_FL1_FL2.g"));
            glgSetTag1.set_batch += GlgSetTag1_set_batch;
            glgSetTag1.set_transfer += GlgSetTag1_set_transfer;
            glgSetTag1.set_value += GlgSetTag1_set_value;
            glgSetTag1.set_on_off += GlgSetTag1_set_on_off;
            label_header_hmi1.Text = $"{Properties.Settings.Default.fl1_header}      ";
            flow_meter1.label_transfer = Properties.Settings.Default.fl1_label_tf1;
            glgSetTag1.SetSRsc(glgControl_hmi1, "text_transfer", flow_meter1.label_transfer);
            flow_meter1.label_batch = Properties.Settings.Default.fl1_label_batch1;
            glgSetTag1.SetSRsc(glgControl_hmi1, "text_batch", flow_meter1.label_batch);

            glgSetTag2.Initialize(this, glgControl_hmi2, Path.Combine(Application.StartupPath, "GUI_FL1_FL2.g"));
            glgSetTag2.set_batch += GlgSetTag2_set_batch;
            glgSetTag2.set_transfer += GlgSetTag2_set_transfer;
            glgSetTag2.set_value += GlgSetTag2_set_value;
            glgSetTag2.set_on_off += GlgSetTag2_set_on_off;
            label_header_hmi2.Text = $"{Properties.Settings.Default.fl2_header}      ";
            flow_meter2.label_transfer = Properties.Settings.Default.fl2_label_tf1;
            glgSetTag2.SetSRsc(glgControl_hmi2, "text_transfer", flow_meter2.label_transfer);
            flow_meter2.label_batch = Properties.Settings.Default.fl2_label_batch1;
            glgSetTag2.SetSRsc(glgControl_hmi2, "text_batch", flow_meter2.label_batch);

            glgSetTag3.Initialize(this, glgControl_hmi3, Path.Combine(Application.StartupPath, "GUI_FL3_FL4_FL5.g"));
            glgSetTag3.set_produk += GlgSetTag3_set_produk;
            glgSetTag3.set_batch += GlgSetTag3_set_batch;
            glgSetTag3.set_transfer += GlgSetTag3_set_transfer;
            glgSetTag3.set_value += GlgSetTag3_set_value;
            glgSetTag3.set_on_off += GlgSetTag3_set_on_off;
            glgSetTag3.set_proses_mesin += GlgSetTag3_set_proses_mesin;
            label_header_hmi3.Text = $"{Properties.Settings.Default.fl3_header}      ";
            flow_meter3.label_transfer = Properties.Settings.Default.fl3_label_tf1;
            glgSetTag3.SetSRsc(glgControl_hmi3, "transfer_to/String", flow_meter3.label_transfer);
            flow_meter3.label_batch = Properties.Settings.Default.fl1_label_batch1;
            glgSetTag3.SetSRsc(glgControl_hmi3, "batch/String", flow_meter3.label_batch);
            flow_meter3.label_proses_mesin = Properties.Settings.Default.fl3_label_pm1;
            glgSetTag3.SetSRsc(glgControl_hmi3, "proses_mesin/String", flow_meter3.label_proses_mesin);

            glgSetTag4.Initialize(this, glgControl_hmi4, Path.Combine(Application.StartupPath, "GUI_FL3_FL4_FL5.g"));
            glgSetTag4.set_produk += GlgSetTag4_set_produk;
            glgSetTag4.set_batch += GlgSetTag4_set_batch;
            glgSetTag4.set_transfer += GlgSetTag4_set_transfer;
            glgSetTag4.set_value += GlgSetTag4_set_value;
            glgSetTag4.set_on_off += GlgSetTag4_set_on_off;
            glgSetTag4.set_proses_mesin += GlgSetTag4_set_proses_mesin;
            label_header_hmi4.Text = $"{Properties.Settings.Default.fl4_header}      ";
            flow_meter4.label_transfer = Properties.Settings.Default.fl2_label_tf1;
            glgSetTag4.SetSRsc(glgControl_hmi4, "transfer_to/String", flow_meter4.label_transfer);
            flow_meter4.label_batch = Properties.Settings.Default.fl1_label_batch1;
            glgSetTag4.SetSRsc(glgControl_hmi4, "batch/String", flow_meter4.label_batch);
            flow_meter4.label_proses_mesin = Properties.Settings.Default.fl4_label_pm1;
            glgSetTag4.SetSRsc(glgControl_hmi4, "proses_mesin/String", flow_meter4.label_proses_mesin);

            glgSetTag5.Initialize(this, glgControl_hmi5, Path.Combine(Application.StartupPath, "GUI_FL3_FL4_FL5.g"));
            glgSetTag5.set_produk += GlgSetTag5_set_produk;
            glgSetTag5.set_batch += GlgSetTag5_set_batch;
            glgSetTag5.set_transfer += GlgSetTag5_set_transfer;
            glgSetTag5.set_value += GlgSetTag5_set_value;
            glgSetTag5.set_on_off += GlgSetTag5_set_on_off;
            glgSetTag5.set_proses_mesin += GlgSetTag5_set_proses_mesin;
            label_header_hmi5.Text = $"{Properties.Settings.Default.fl5_header}      ";
            flow_meter5.label_transfer = Properties.Settings.Default.fl2_label_tf1;
            glgSetTag5.SetSRsc(glgControl_hmi5, "transfer_to/String", flow_meter5.label_transfer);
            flow_meter5.label_batch = Properties.Settings.Default.fl1_label_batch1;
            glgSetTag5.SetSRsc(glgControl_hmi5, "batch/String", flow_meter5.label_batch);
            flow_meter5.label_proses_mesin = Properties.Settings.Default.fl5_label_pm1;
            glgSetTag5.SetSRsc(glgControl_hmi5, "proses_mesin/String", flow_meter5.label_proses_mesin);
        }
        //=================================================================================================================================================================================
        private void GlgSetTag1_set_on_off(object sender, GlgObject e)
        {
            if (!OPCStatus1.Connected) return;
            form.OPCWriteAsync1(5, !form.GetOPCDataValue<bool>(5));
        }
        private void GlgSetTag1_set_value(object sender, string e)
        {
            if (!OPCStatus1.Connected) return;
            bool isCancle;
            if(e.Equals("set_setliter"))
            {
                double set_value = Prompt.set_value_("Set Liter", "Liter", form.GetOPCDataValue<double>(42), out isCancle);
                if (!isCancle) return;
                form.OPCWriteAsync1(42, set_value);
            } 
            else if (e.Equals("set_k-factor"))
            {
                double set_value = Prompt.set_value_("Set K-Factor", "%", form.GetOPCDataValue<double>(41), out isCancle);
                if (!isCancle) return;
                form.OPCWriteAsync1(41, set_value);
            }
            else if (e.Equals("set_f-kurang"))
            {
                double set_value = Prompt.set_value_("Set F-Kurang", "%/L", form.GetOPCDataValue<double>(43), out isCancle);
                if (!isCancle) return;
                form.OPCWriteAsync1(43, set_value);
            }
        }
        private void GlgSetTag1_set_transfer(object sender, GlgObject e)
        {
            if (!OPCStatus1.Connected) return;
            string[] list_transfer = { Properties.Settings.Default.fl1_label_tf1, Properties.Settings.Default.fl1_label_tf2, Properties.Settings.Default.fl1_label_tf3, Properties.Settings.Default.fl1_label_tf4, Properties.Settings.Default.fl1_label_tf5 };
            bool isCancle;
            int set_index = Prompt.set_label_("Set Transfer", list_transfer, out isCancle);
            if (!isCancle) return;
            form.OPCWriteAsync1(9 + set_index, true);
        }
        private void GlgSetTag1_set_batch(object sender, GlgObject e)
        {
            if (!OPCStatus1.Connected) return;
            string[] list_transfer = { 
                Properties.Settings.Default.fl1_label_batch1, Properties.Settings.Default.fl1_label_batch2, Properties.Settings.Default.fl1_label_batch3, Properties.Settings.Default.fl1_label_batch4,
                Properties.Settings.Default.fl1_label_batch5, Properties.Settings.Default.fl1_label_batch6, Properties.Settings.Default.fl1_label_batch7, Properties.Settings.Default.fl1_label_batch8,
                Properties.Settings.Default.fl1_label_batch9, Properties.Settings.Default.fl1_label_batch10
            };
            bool isCancle;
            int set_index = Prompt.set_label_("Set Batch", list_transfer, out isCancle);
            if (!isCancle) return;
            form.OPCWriteAsync1(18 + set_index, true);
        }
        //=================================================================================================================================================================================
        private void GlgSetTag2_set_on_off(object sender, GlgObject e)
        {
            if (!OPCStatus1.Connected) return;
            form.OPCWriteAsync1(7, !form.GetOPCDataValue<bool>(7));
        }
        private void GlgSetTag2_set_value(object sender, string e)
        {
            if (!OPCStatus1.Connected) return;
            bool isCancle;
            if (e.Equals("set_setliter"))
            {
                double set_value = Prompt.set_value_("Set Liter", "Liter", form.GetOPCDataValue<double>(47), out isCancle);
                if (!isCancle) return;
                form.OPCWriteAsync1(47, set_value);
            }
            else if (e.Equals("set_k-factor"))
            {
                double set_value = Prompt.set_value_("Set K-Factor", "%", form.GetOPCDataValue<double>(46), out isCancle);
                if (!isCancle) return;
                form.OPCWriteAsync1(46, set_value);
            }
            else if (e.Equals("set_f-kurang"))
            {
                double set_value = Prompt.set_value_("Set F-Kurang", "%/L", form.GetOPCDataValue<double>(48), out isCancle);
                if (!isCancle) return;
                form.OPCWriteAsync1(48, set_value);
            }
        }
        private void GlgSetTag2_set_transfer(object sender, GlgObject e)
        {
            if (!OPCStatus1.Connected) return;
            string[] list_transfer = { Properties.Settings.Default.fl2_label_tf1, Properties.Settings.Default.fl2_label_tf2, Properties.Settings.Default.fl2_label_tf3, Properties.Settings.Default.fl2_label_tf4 };
            bool isCancle;
            int set_index = Prompt.set_label_("Set Transfer", list_transfer, out isCancle);
            if (!isCancle) return;
            form.OPCWriteAsync1(14 + set_index, true);
        }
        private void GlgSetTag2_set_batch(object sender, GlgObject e)
        {
            if (!OPCStatus1.Connected) return;
            string[] list_transfer = {
                Properties.Settings.Default.fl2_label_batch1, Properties.Settings.Default.fl2_label_batch2, Properties.Settings.Default.fl2_label_batch3, Properties.Settings.Default.fl2_label_batch4,
                Properties.Settings.Default.fl2_label_batch5, Properties.Settings.Default.fl2_label_batch6, Properties.Settings.Default.fl2_label_batch7, Properties.Settings.Default.fl2_label_batch8,
                Properties.Settings.Default.fl2_label_batch9, Properties.Settings.Default.fl2_label_batch10
            };
            bool isCancle;
            int set_index = Prompt.set_label_("Set Batch", list_transfer, out isCancle);
            if (!isCancle) return;
            form.OPCWriteAsync1(28 + set_index, true);
        }
        //=================================================================================================================================================================================
        private void GlgSetTag3_set_on_off(object sender, GlgObject e)
        {
            if (!OPCStatus1.Connected) return;
            form.OPCWriteAsync1(58, !form.GetOPCDataValue<bool>(58));
        }
        private void GlgSetTag3_set_proses_mesin(object sender, GlgObject e)
        {
            if (!OPCStatus1.Connected) return;
            string[] list_proses_mesin = { Properties.Settings.Default.fl3_label_pm1, Properties.Settings.Default.fl3_label_pm2 };
            bool isCancle;
            int set_index = Prompt.set_label_("Set Proses Mesin", list_proses_mesin, out isCancle);
            if (!isCancle) return;
            form.OPCWriteAsync1(66 + set_index, true);
        }
        private void GlgSetTag3_set_value(object sender, string e)
        {
            if (!OPCStatus1.Connected) return;
            bool isCancle;
            if (e.Equals("set_k-factor"))
            {
                double set_value = Prompt.set_value_("Set K-Factor", "%", form.GetOPCDataValue<double>(76), out isCancle);
                if (!isCancle) return;
                form.OPCWriteAsync1(76, set_value);
            }
        }
        private void GlgSetTag3_set_transfer(object sender, GlgObject e)
        {
            if (!OPCStatus1.Connected) return;
            string[] list_transfer = { Properties.Settings.Default.fl3_label_tf1, Properties.Settings.Default.fl3_label_tf2, Properties.Settings.Default.fl3_label_tf3 };
            bool isCancle;
            int set_index = Prompt.set_label_("Set Transfer To MF", list_transfer, out isCancle);
            if (!isCancle) return;
            form.OPCWriteAsync1(63 + set_index, true);
        }
        private void GlgSetTag3_set_batch(object sender, GlgObject e)
        {
            if (!OPCStatus1.Connected) return;
            string[] list_transfer = {
                Properties.Settings.Default.fl1_label_batch1, Properties.Settings.Default.fl1_label_batch2, Properties.Settings.Default.fl1_label_batch3, Properties.Settings.Default.fl1_label_batch4,
                Properties.Settings.Default.fl1_label_batch5, Properties.Settings.Default.fl1_label_batch6, Properties.Settings.Default.fl1_label_batch7, Properties.Settings.Default.fl1_label_batch8,
                Properties.Settings.Default.fl1_label_batch9, Properties.Settings.Default.fl1_label_batch10
            };
            bool isCancle;
            int set_index = Prompt.set_label_("Set Batch", list_transfer, out isCancle);
            if (!isCancle) return;
            form.OPCWriteAsync1(82, 1 + set_index);
        }
        private void GlgSetTag3_set_produk(object sender, GlgObject e)
        {
            if (!OPCStatus1.Connected) return;
            string[] list_produk = {
                Properties.Settings.Default.fl3_produk1, Properties.Settings.Default.fl3_produk2, Properties.Settings.Default.fl3_produk3, Properties.Settings.Default.fl3_produk4,
                Properties.Settings.Default.fl3_produk5, Properties.Settings.Default.fl3_produk6, Properties.Settings.Default.fl3_produk7, Properties.Settings.Default.fl3_produk8,
                Properties.Settings.Default.fl3_produk9, Properties.Settings.Default.fl3_produk10, Properties.Settings.Default.fl3_produk11
            };
            bool isCancle;
            int set_index = Prompt.set_label_("Set Produk", list_produk, out isCancle);
            if (!isCancle) return;
            form.OPCWriteAsync1(98, set_index);
        }
        //=================================================================================================================================================================================
        private void GlgSetTag4_set_on_off(object sender, GlgObject e)
        {
            if (!OPCStatus1.Connected) return;
            form.OPCWriteAsync1(61, !form.GetOPCDataValue<bool>(61));
        }
        private void GlgSetTag4_set_proses_mesin(object sender, GlgObject e)
        {
            if (!OPCStatus1.Connected) return;
            string[] list_proses_mesin = { Properties.Settings.Default.fl4_label_pm1, Properties.Settings.Default.fl4_label_pm2 };
            bool isCancle;
            int set_index = Prompt.set_label_("Set Proses Mesin", list_proses_mesin, out isCancle);
            if (!isCancle) return;
            form.OPCWriteAsync1(71 + set_index, true);
        }
        private void GlgSetTag4_set_value(object sender, string e)
        {
            if (!OPCStatus1.Connected) return;
            bool isCancle;
            if (e.Equals("set_k-factor"))
            {
                double set_value = Prompt.set_value_("Set K-Factor", "%", form.GetOPCDataValue<double>(79), out isCancle);
                if (!isCancle) return;
                form.OPCWriteAsync1(79, set_value);
            }
        }
        private void GlgSetTag4_set_transfer(object sender, GlgObject e)
        {
            if (!OPCStatus1.Connected) return;
            string[] list_transfer = { Properties.Settings.Default.fl4_label_tf1, Properties.Settings.Default.fl4_label_tf2, Properties.Settings.Default.fl4_label_tf3 };
            bool isCancle;
            int set_index = Prompt.set_label_("Set Transfer To MF", list_transfer, out isCancle);
            if (!isCancle) return;
            form.OPCWriteAsync1(68 + set_index, true);
        }
        private void GlgSetTag4_set_batch(object sender, GlgObject e)
        {
            if (!OPCStatus1.Connected) return;
            string[] list_transfer = {
                Properties.Settings.Default.fl2_label_batch1, Properties.Settings.Default.fl2_label_batch2, Properties.Settings.Default.fl2_label_batch3, Properties.Settings.Default.fl2_label_batch4,
                Properties.Settings.Default.fl2_label_batch5, Properties.Settings.Default.fl2_label_batch6, Properties.Settings.Default.fl2_label_batch7, Properties.Settings.Default.fl2_label_batch8,
                Properties.Settings.Default.fl2_label_batch9, Properties.Settings.Default.fl2_label_batch10
            };
            bool isCancle;
            int set_index = Prompt.set_label_("Set Batch", list_transfer, out isCancle);
            if (!isCancle) return;
            form.OPCWriteAsync1(83, 1 + set_index);
        }
        private void GlgSetTag4_set_produk(object sender, GlgObject e)
        {
            if (!OPCStatus1.Connected) return;
            string[] list_produk = {
                Properties.Settings.Default.fl4_produk1, Properties.Settings.Default.fl4_produk2, Properties.Settings.Default.fl4_produk3, Properties.Settings.Default.fl4_produk4,
                Properties.Settings.Default.fl4_produk5, Properties.Settings.Default.fl4_produk6, Properties.Settings.Default.fl4_produk7, Properties.Settings.Default.fl4_produk8,
                Properties.Settings.Default.fl4_produk9, Properties.Settings.Default.fl4_produk10, Properties.Settings.Default.fl4_produk11
            };
            bool isCancle;
            int set_index = Prompt.set_label_("Set Produk", list_produk, out isCancle);
            if (!isCancle) return;
            form.OPCWriteAsync1(99, set_index);
        }
        //=================================================================================================================================================================================
        private void GlgSetTag5_set_on_off(object sender, GlgObject e)
        {
            if (!OPCStatus1.Connected) return;
            form.OPCWriteAsync1(85, !form.GetOPCDataValue<bool>(85));
        }
        private void GlgSetTag5_set_proses_mesin(object sender, GlgObject e)
        {
            if (!OPCStatus1.Connected) return;
            string[] list_proses_mesin = { Properties.Settings.Default.fl5_label_pm1, Properties.Settings.Default.fl5_label_pm2 };
            bool isCancle;
            int set_index = Prompt.set_label_("Set Proses Mesin", list_proses_mesin, out isCancle);
            if (!isCancle) return;
            form.OPCWriteAsync1(90 + set_index, true);
        }
        private void GlgSetTag5_set_value(object sender, string e)
        {
            if (!OPCStatus1.Connected) return;
            bool isCancle;
            if (e.Equals("set_k-factor"))
            {
                double set_value = Prompt.set_value_("Set K-Factor", "%", form.GetOPCDataValue<double>(94), out isCancle);
                if (!isCancle) return;
                form.OPCWriteAsync1(94, set_value);
            }
        }
        private void GlgSetTag5_set_transfer(object sender, GlgObject e)
        {
            if (!OPCStatus1.Connected) return;
            string[] list_transfer = { Properties.Settings.Default.fl5_label_tf1, Properties.Settings.Default.fl5_label_tf2, Properties.Settings.Default.fl5_label_tf3 };
            bool isCancle;
            int set_index = Prompt.set_label_("Set Transfer To MF", list_transfer, out isCancle);
            if (!isCancle) return;
            form.OPCWriteAsync1(87 + set_index, true);
        }
        private void GlgSetTag5_set_batch(object sender, GlgObject e)
        {
            if (!OPCStatus1.Connected) return;
            string[] list_transfer = {
                Properties.Settings.Default.fl2_label_batch1, Properties.Settings.Default.fl2_label_batch2, Properties.Settings.Default.fl2_label_batch3, Properties.Settings.Default.fl2_label_batch4,
                Properties.Settings.Default.fl2_label_batch5, Properties.Settings.Default.fl2_label_batch6, Properties.Settings.Default.fl2_label_batch7, Properties.Settings.Default.fl2_label_batch8,
                Properties.Settings.Default.fl2_label_batch9, Properties.Settings.Default.fl2_label_batch10
            };
            bool isCancle;
            int set_index = Prompt.set_label_("Set Batch", list_transfer, out isCancle);
            if (!isCancle) return;
            form.OPCWriteAsync1(97, 1 + set_index);
        }
        private void GlgSetTag5_set_produk(object sender, GlgObject e)
        {
            if (!OPCStatus1.Connected) return;
            string[] list_produk = {
                Properties.Settings.Default.fl5_produk1, Properties.Settings.Default.fl5_produk2, Properties.Settings.Default.fl5_produk3, Properties.Settings.Default.fl5_produk4,
                Properties.Settings.Default.fl5_produk5, Properties.Settings.Default.fl5_produk6, Properties.Settings.Default.fl5_produk7, Properties.Settings.Default.fl5_produk8,
                Properties.Settings.Default.fl5_produk9, Properties.Settings.Default.fl5_produk10, Properties.Settings.Default.fl5_produk11
            };
            bool isCancle;
            int set_index = Prompt.set_label_("Set Produk", list_produk, out isCancle);
            if (!isCancle) return;
            form.OPCWriteAsync1(100, set_index);
        }
    }
}
