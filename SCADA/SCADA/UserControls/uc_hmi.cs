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
        public GlgSetTag glgSetTag1;
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
            throw new NotImplementedException();
        }
        private void GlgSetTag1_set_sumber(object sender, GlgObject e)
        {
            throw new NotImplementedException();
        }
        public void ShowWarningMessage(string message) => form.ShowWarningMessage(message);
    }
}
