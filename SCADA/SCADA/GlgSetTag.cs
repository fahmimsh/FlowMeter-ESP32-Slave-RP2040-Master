using GenLogic;
using SCADA.UserControls;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace SCADA
{
    public class GlgSetTag
    {
        private GlgCallback callback;
        public event EventHandler<GlgObject> set_batch;
        public event EventHandler<GlgObject> set_transfer;
        public event EventHandler<string> set_value;
        public event EventHandler<GlgObject> set_on_off;
        public event EventHandler<GlgObject> set_proses_mesin;
        public void Initialize(uc_hmi form_p, dynamic glg_object, string Path_p)
        {
            glg_object.SetDResource("$config/GlgPickResolution", 1600.0);
            glg_object.DrawingFile = Path_p;
            callback = new GlgCallback(form_p);
            glg_object.AddListener(GlgCallbackType.INPUT_CB, callback);
            callback.set_batch += Callback_set_batch;
            callback.set_transfer += Callback_set_transfer;
            callback.set_value += Callback_set_value;
            callback.set_on_off += Callback_set_on_off;
            callback.set_proses_mesin += Callback_set_proses_mesin;
        }
        public Dictionary<string, string[]> TagMaps = new Dictionary<string, string[]>
        {
            {"valve_flow", new string[] { "valve_flow/State", "valve_flow/handle/Visibility" }},
            {"sensor_flow", new string[] { "sensor_flow/State", "sensor_flow/turbine/Enabled" }},
            {"pump_flow", new string[] { "pump_flow/Enabled", "pipe_flow/Flow/Visibility"}},
            {"val_setliter", new string[] { "val_setliter/Value", "level_liter/high_level", "level_liter/HighLevel/LevelHigh"}},

            {"valve_pipe", new string[] { "valve_flow/State", "valve_flow/handle/Visibility", "pipe_flow/Flow/Visibility" }},
        };
        public void SetDRsc(dynamic glg_object, string tag, bool flag) => glg_object.SetDResource(tag, flag ? 1.0 : 0.0);
        public void SetDRsc(dynamic glg_object, string tag, double currentValue) => glg_object.SetDResource(tag, currentValue);
        public void SetDRsc(dynamic glg_object, string[] tag, double currentValue)
        {
            foreach (string glg in tag) glg_object.SetDResource(glg, currentValue);
        }
        public void SetDRsc(dynamic glg_object, string tag, double currentValue, double MaxValue)
        {
            if (currentValue > MaxValue) currentValue = MaxValue;
            glg_object.SetDResource(tag, currentValue);
        }
        public void SetDRsc(dynamic glg_object, string[] tag, bool flag)
        {
            foreach (string glg in tag) { glg_object.SetDResource(glg, flag ? 1.0 : 0.0); }
        }
        public void BtnGlgSet(dynamic glg_object, string glgTag, bool state_, string label0, string label1, double color00_, double color01_, double color02_, double color10_, double color11_, double color12_)
        {
            glg_object.SetGResource($"{glgTag}/BodyColor", !state_ ? color00_ : color10_, !state_ ? color01_ : color11_, !state_ ? color02_ : color12_); //hijau:merah
            glg_object.SetSResource($"{glgTag}/Label/String", !state_ ? label0 : label1);
        }
        public void SetSRsc(dynamic glg_object, string tag, string arg) => glg_object.SetSResource($"{tag}/String", arg);
        private void Callback_set_batch(object sender, GlgObject e) => set_batch?.Invoke(sender, e);
        private void Callback_set_on_off(object sender, GlgObject e) => set_on_off?.Invoke(sender, e);
        private void Callback_set_value(object sender, string e) => set_value?.Invoke(sender, e);
        private void Callback_set_transfer(object sender, GlgObject e) => set_transfer?.Invoke(sender, e);
        private void Callback_set_proses_mesin(object sender, GlgObject e) => set_proses_mesin?.Invoke(sender, e);
    }
    public class GlgCallback : GlgInputListener
    {
        uc_hmi form;
        public GlgCallback(uc_hmi Form1_p) { form = Form1_p; }
        public event EventHandler<GlgObject> set_batch;
        public event EventHandler<GlgObject> set_transfer;
        public event EventHandler<string> set_value;
        public event EventHandler<GlgObject> set_on_off;
        public event EventHandler<GlgObject> set_proses_mesin;
        public void InputCallback(GlgObject glg_object, GlgObject message)
        {
            string origin = message.GetSResource("Origin"); //name off obj
            string format = message.GetSResource("Format"); //tipe
            string action = message.GetSResource("Action"); //aksi saat mouse     
            if (!format.Equals("Button")) return; //string subAction = message_obj.GetSResource("SubAction");\
            if (!action.Equals("Activate"))
                return;
            Dictionary<string, Action> originMethodMap = new Dictionary<string, Action>
                {
                    {"set_batch", () => set_batch?.Invoke(this, glg_object) },
                    {"set_transfer", () => set_transfer?.Invoke(this, glg_object) },
                    {"set_setliter", () => set_value?.Invoke(this, "set_setliter") },
                    {"set_k-factor", () => set_value?.Invoke(this, "set_k-factor")  },
                    {"set_f-kurang", () => set_value?.Invoke(this, "set_f-kurang")   },
                    {"set_on_off", () => set_on_off?.Invoke(this, glg_object) },
                    {"set_proses_mesin", () => set_proses_mesin?.Invoke(this, glg_object) }
                };
            if (originMethodMap.ContainsKey(origin)) form.BeginInvoke((MethodInvoker)delegate { originMethodMap[origin].Invoke(); });
        }
    }
}
