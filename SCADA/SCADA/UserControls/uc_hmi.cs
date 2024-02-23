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
        internal GlgSetTag glgSetTag1;
        private form_main form;
        public uc_hmi(form_main mainForm)
        {
            InitializeComponent();
            form = mainForm;
            glgSetTag1 = new GlgSetTag();
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
            tabControl_hmi.TabPages[0].Text = $"{Properties.Settings.Default.fl1_header} & {Properties.Settings.Default.fl2_header}";
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
        public void ShowWarningMessage(string message) => form.ShowWarningMessage(message);
    }
}
