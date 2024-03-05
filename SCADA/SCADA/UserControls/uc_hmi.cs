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
        internal GlgSetTag glgSetTag1, glgSetTag2, glgSetTag3, glgSetTag4;
        private form_main form;
        public uc_hmi(form_main mainForm)
        {
            InitializeComponent();
            form = mainForm;
            glgSetTag1 = new GlgSetTag();
            glgSetTag2 = new GlgSetTag();
            glgSetTag3 = new GlgSetTag();
            glgSetTag4 = new GlgSetTag();
        }
        private void uc_hmi_Load(object sender, EventArgs e)
        {
            glgSetTag1.Initialize(this, glgControl_hmi1, Path.Combine(Application.StartupPath, "GUI.g"));
            glgSetTag1.set_sumber += GlgSetTag1_set_sumber;
            glgSetTag1.set_transfer += GlgSetTag1_set_transfer;
            glgSetTag1.set_value += GlgSetTag1_set_value;
            glgSetTag1.set_on_off += GlgSetTag1_set_on_off;
            label_header_hmi1.Text = Properties.Settings.Default.fl1_header;
            flow_meter1.label_sumber = Properties.Settings.Default.fl1_label_sumber1;
            glgSetTag1.SetSRsc(glgControl_hmi1, "text_sumber", Properties.Settings.Default.fl1_label_sumber1);

            glgSetTag2.Initialize(this, glgControl_hmi2, Path.Combine(Application.StartupPath, "GUI.g"));
            glgSetTag2.set_sumber += GlgSetTag2_set_sumber;
            glgSetTag2.set_transfer += GlgSetTag2_set_transfer;
            glgSetTag2.set_value += GlgSetTag2_set_value;
            glgSetTag2.set_on_off += GlgSetTag2_set_on_off;
            label_header_hmi2.Text = Properties.Settings.Default.fl2_header;
            flow_meter1.label_sumber = Properties.Settings.Default.fl2_label_sumber1;
            glgSetTag2.SetSRsc(glgControl_hmi2, "text_sumber", Properties.Settings.Default.fl2_label_sumber1);

            glgSetTag3.Initialize(this, glgControl_hmi3, Path.Combine(Application.StartupPath, "GUI.g"));
            glgSetTag3.set_sumber += GlgSetTag3_set_sumber;
            glgSetTag3.set_transfer += GlgSetTag3_set_transfer;
            glgSetTag3.set_value += GlgSetTag3_set_value;
            glgSetTag3.set_on_off += GlgSetTag3_set_on_off;
            label_header_hmi3.Text = Properties.Settings.Default.fl3_header;

            glgSetTag4.Initialize(this, glgControl_hmi4, Path.Combine(Application.StartupPath, "GUI.g"));
            glgSetTag4.set_sumber += GlgSetTag4_set_sumber;
            glgSetTag4.set_transfer += GlgSetTag4_set_transfer;
            glgSetTag4.set_value += GlgSetTag4_set_value;
            glgSetTag4.set_on_off += GlgSetTag4_set_on_off;
            label_header_hmi4.Text = Properties.Settings.Default.fl4_header;
        }
        private void GlgSetTag1_set_on_off(object sender, GlgObject e)
        {
            if (!OPCStatus1.Connected) return;
            form.OPCWriteAsync1(3, !form.GetOPCDataValue<bool>(3));
        }
        private void GlgSetTag1_set_value(object sender, string e)
        {
            if (!OPCStatus1.Connected) return;
            bool isCancle;
            if(e.Equals("set_setliter"))
            {
                double set_value = Prompt.set_value_("Set Liter", "Liter", form.GetOPCDataValue<double>(7), out isCancle);
                if (!isCancle) return;
                form.OPCWriteAsync1(7, set_value);
            } 
            else if (e.Equals("set_k-factor"))
            {
                double set_value = Prompt.set_value_("Set K-Factor", "%", form.GetOPCDataValue<double>(6), out isCancle);
                if (!isCancle) return;
                form.OPCWriteAsync1(6, set_value);
            }
            else if (e.Equals("set_f-kurang"))
            {
                double set_value = Prompt.set_value_("Set F-Kurang", "Liter", form.GetOPCDataValue<double>(8), out isCancle);
                if (!isCancle) return;
                form.OPCWriteAsync1(8, set_value);
            }
        }
        private void GlgSetTag1_set_transfer(object sender, GlgObject e)
        {
            if (!OPCStatus1.Connected) return;
            string[] list_transfer = { Properties.Settings.Default.fl1_label_tf1, Properties.Settings.Default.fl1_label_tf2, Properties.Settings.Default.fl1_label_tf3, Properties.Settings.Default.fl1_label_tf4 };
            bool isCancle;
            int set_index = Prompt.set_label_("Set Transfer", list_transfer, out isCancle);
            if (!isCancle) return;
            form.OPCWriteAsync1(21 + set_index, true);
        }
        private void GlgSetTag1_set_sumber(object sender, GlgObject e)
        {
            if (!OPCStatus1.Connected) return;
            string[] list_transfer = { Properties.Settings.Default.fl1_label_sumber1 };
            bool isCancle;
            Prompt.set_label_("Set Sumber", list_transfer, out isCancle);
        }
        private void GlgSetTag2_set_on_off(object sender, GlgObject e)
        {
            if (!OPCStatus1.Connected) return;
            form.OPCWriteAsync1(13, !form.GetOPCDataValue<bool>(13));
        }
        private void GlgSetTag2_set_value(object sender, string e)
        {
            if (!OPCStatus1.Connected) return;
            bool isCancle;
            if (e.Equals("set_setliter"))
            {
                double set_value = Prompt.set_value_("Set Liter", "Liter", form.GetOPCDataValue<double>(17), out isCancle);
                if (!isCancle) return;
                form.OPCWriteAsync1(17, set_value);
            }
            else if (e.Equals("set_k-factor"))
            {
                double set_value = Prompt.set_value_("Set K-Factor", "%", form.GetOPCDataValue<double>(16), out isCancle);
                if (!isCancle) return;
                form.OPCWriteAsync1(16, set_value);
            }
            else if (e.Equals("set_f-kurang"))
            {
                double set_value = Prompt.set_value_("Set F-Kurang", "Liter", form.GetOPCDataValue<double>(18), out isCancle);
                if (!isCancle) return;
                form.OPCWriteAsync1(18, set_value);
            }
        }
        private void GlgSetTag2_set_transfer(object sender, GlgObject e)
        {
            if (!OPCStatus1.Connected) return;
            string[] list_transfer = { Properties.Settings.Default.fl2_label_tf1, Properties.Settings.Default.fl2_label_tf2, Properties.Settings.Default.fl2_label_tf3, Properties.Settings.Default.fl2_label_tf4 };
            bool isCancle;
            int set_index = Prompt.set_label_("Set Transfer", list_transfer, out isCancle);
            if (!isCancle) return;
            form.OPCWriteAsync1(25 + set_index, true);
        }
        private void GlgSetTag2_set_sumber(object sender, GlgObject e)
        {
            if (!OPCStatus1.Connected) return;
            string[] list_transfer = { Properties.Settings.Default.fl2_label_sumber1 };
            bool isCancle;
            Prompt.set_label_("Set Sumber", list_transfer, out isCancle);
        }



        private void GlgSetTag3_set_on_off(object sender, GlgObject e)
        {
            if (!OPCStatus1.Connected) return;
            form.OPCWriteAsync1(31, !form.GetOPCDataValue<bool>(31));
        }
        private void GlgSetTag3_set_value(object sender, string e)
        {
            if (!OPCStatus1.Connected) return;
            bool isCancle;
            if (e.Equals("set_setliter"))
            {
                double set_value = Prompt.set_value_("Set Liter", "Liter", form.GetOPCDataValue<double>(35), out isCancle);
                if (!isCancle) return;
                form.OPCWriteAsync1(35, set_value);
            }
            else if (e.Equals("set_k-factor"))
            {
                double set_value = Prompt.set_value_("Set K-Factor", "%", form.GetOPCDataValue<double>(34), out isCancle);
                if (!isCancle) return;
                form.OPCWriteAsync1(34, set_value);
            }
            else if (e.Equals("set_f-kurang"))
            {
                double set_value = Prompt.set_value_("Set F-Kurang", "Liter", form.GetOPCDataValue<double>(36), out isCancle);
                if (!isCancle) return;
                form.OPCWriteAsync1(36, set_value);
            }
        }
        private void GlgSetTag3_set_transfer(object sender, GlgObject e)
        {
            if (!OPCStatus1.Connected) return;
            string[] list_transfer = { Properties.Settings.Default.fl3_label_tf1, Properties.Settings.Default.fl3_label_tf2, Properties.Settings.Default.fl3_label_tf3, Properties.Settings.Default.fl3_label_tf4 };
            bool isCancle;
            int set_index = Prompt.set_label_("Set Transfer", list_transfer, out isCancle);
            if (!isCancle) return;
            form.OPCWriteAsync1(49 + set_index, true);
        }
        private void GlgSetTag3_set_sumber(object sender, GlgObject e)
        {
            if (!OPCStatus1.Connected) return;
            string[] list_transfer = { Properties.Settings.Default.fl3_label_sumber1, Properties.Settings.Default.fl3_label_sumber2 };
            bool isCancle;
            int set_index = Prompt.set_label_("Set Sumber", list_transfer, out isCancle);
            if (!isCancle) return;
            form.OPCWriteAsync1(53 + set_index, true);
        }
        private void GlgSetTag4_set_on_off(object sender, GlgObject e)
        {
            if (!OPCStatus1.Connected) return;
            form.OPCWriteAsync1(41, !form.GetOPCDataValue<bool>(41));
        }

        private void GlgSetTag4_set_value(object sender, string e)
        {
            if (!OPCStatus1.Connected) return;
            bool isCancle;
            if (e.Equals("set_setliter"))
            {
                double set_value = Prompt.set_value_("Set Liter", "Liter", form.GetOPCDataValue<double>(45), out isCancle);
                if (!isCancle) return;
                form.OPCWriteAsync1(45, set_value);
            }
            else if (e.Equals("set_k-factor"))
            {
                double set_value = Prompt.set_value_("Set K-Factor", "%", form.GetOPCDataValue<double>(44), out isCancle);
                if (!isCancle) return;
                form.OPCWriteAsync1(44, set_value);
            }
            else if (e.Equals("set_f-kurang"))
            {
                double set_value = Prompt.set_value_("Set F-Kurang", "Liter", form.GetOPCDataValue<double>(46), out isCancle);
                if (!isCancle) return;
                form.OPCWriteAsync1(46, set_value);
            }
        }
        private void GlgSetTag4_set_transfer(object sender, GlgObject e)
        {
            if (!OPCStatus1.Connected) return;
            string[] list_transfer = { Properties.Settings.Default.fl4_label_tf1, Properties.Settings.Default.fl4_label_tf2, Properties.Settings.Default.fl4_label_tf3, Properties.Settings.Default.fl4_label_tf4 };
            bool isCancle;
            int set_index = Prompt.set_label_("Set Transfer", list_transfer, out isCancle);
            if (!isCancle) return;
            form.OPCWriteAsync1(55 + set_index, true);
        }
        private void GlgSetTag4_set_sumber(object sender, GlgObject e)
        {
            if (!OPCStatus1.Connected) return;
            string[] list_transfer = { Properties.Settings.Default.fl4_label_sumber1, Properties.Settings.Default.fl4_label_sumber2 };
            bool isCancle;
            int set_index = Prompt.set_label_("Set Sumber", list_transfer, out isCancle);
            if (!isCancle) return;
            form.OPCWriteAsync1(59 + set_index, true);
        }
    }
}
