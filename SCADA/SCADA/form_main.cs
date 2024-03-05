using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using SqlKata.Execution;
using GenLogic;
using MQTTnet;
using MQTTnet.Client;
using Newtonsoft.Json;
using GodSharp.Opc.Da;
using GodSharp.Opc.Da.Options;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using SCADA.UserControls;
using MySql.Data.MySqlClient;
using SqlKata;
using SqlKata.Compilers;
using System.Diagnostics;
using static Org.BouncyCastle.Crypto.Engines.SM2Engine;

namespace SCADA
{
    public partial class form_main : Form
    {
        internal IServerDiscovery OPCdiscovery;
        internal IOpcDaClient OPCclient1;
        internal IMqttClient MqttClient;
        internal BindingList<OPCData1> OPCData_1 = new BindingList<OPCData1>();
        internal ObservableCollection<TagData1> tagData1 = new ObservableCollection<TagData1>();
        public uc_hmi uc_x_hmi;
        public uc_log uc_x_log;
        public form_main()
        {
            InitializeComponent();
            this.MaximumSize = new System.Drawing.Size(Screen.PrimaryScreen.WorkingArea.Width, Screen.PrimaryScreen.WorkingArea.Height);
            uc_x_hmi = new uc_hmi(this);
            uc_x_log = new uc_log(this);
            OPCdiscovery = DaClientFactory.Instance.CreateOpcNetApiServerDiscovery();
        }
        private void form_main_Load(object sender, EventArgs e)
        {
            XamppOpen();
            add_to_main_panel(uc_x_log);
            if(Properties.Settings.Default.auto_connect_opc) OPC1Connect_or_Disconnect(false);
            add_to_main_panel(uc_x_hmi);
        }
        private void form_main_Shown(object sender, EventArgs e)
        {
            timer_handle_opc_tag1.Enabled = true;
            timer_handle_opc_tag2.Enabled = true;
        }
        internal bool OPC1Connect_or_Disconnect(bool flag_is_set)
        {
            if (flag_is_set)
            {
                if (OPCclient1?.Connected == true) OPCclient1.Disconnect();
                OPCclient1?.Dispose();
                GC.Collect();
                OPCStatus1.Connected = false;
                menu_connect_opc.Image = Properties.Resources.icons8_disconnect;
                menu_connect_opc.Text = "Disconnect";
                return true;
            }
            if (string.IsNullOrWhiteSpace(Properties.Settings.Default.OPCServer1)) return false;
            if (OPCclient1?.Connected == true) { OPCclient1.Disconnect(); OPCclient1.Dispose(); GC.Collect(); }
            Func<Action<DaClientOptions>, IOpcDaClient> factory;
            factory = (Action<DaClientOptions> action) => DaClientFactory.Instance.CreateOpcNetApiClient(action);
            OPCclient1 = factory(x =>
            {
                x.Data = new ServerData { Host = Properties.Settings.Default.OPCHost, ProgId = Properties.Settings.Default.OPCServer1, Name = Properties.Settings.Default.OPCServer1 };
                x.OnDataChangedHandler += OPC1_OnDataChangedHandler;
                x.OnServerShutdownHandler += OPC1_OnServerShutdownHandler;
                x.OnAsyncReadCompletedHandler += OPC1_OnAsyncReadCompletedHandler;
                x.OnAsyncWriteCompletedHandler += OPC1_OnAsyncWriteCompletedHandler;
            });
            if (!File.Exists("opcTag.js")) { ShowErrorMessage($"FIle Tidak Tersedia"); return false; }
            string json = File.ReadAllText("opcTag.js");
            tagData1 = new ObservableCollection<TagData1>(JsonConvert.DeserializeObject<List<TagData1>>(json) ?? new List<TagData1>());
            OPCclient1.Connect();
            OPCclient1.Add(new Group { Name = "default", UpdateRate = 10, IsSubscribed = true });
            OPCData_1.Clear();
            foreach (var data in tagData1.OrderBy(data => data.Id))
            {
                if (OPCclient1.Current.Tags?.ContainsKey(data.Tag) != true)
                {
                    var tag = new Tag(data.Tag, data.Id);
                    try
                    {
                        OPCclient1.Current.Add(tag);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"OPC Gagal Add Tag:{data.Tag} Client id: {data.Id} Message: {ex.Message}", @"OPC Gagal Add Tag", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                    }
                    OPCData_1.Add(new OPCData1(data.Tag, data.Id));
                }
            }
            OPCReadAsync1(1);
            OPCStatus1.Connected = true;
            menu_connect_opc.Image = Properties.Resources.icons8_connect;
            menu_connect_opc.Text = "Connected";
            return OPCclient1.Connected;
        }
        private void OPC1_OnAsyncWriteCompletedHandler(AsyncWriteCompletedOutput output)
        {
            if (!output.Data.Ok) return;
            var tag = OPCData_1.FirstOrDefault(a => a.ClientHandle == output.Data.Result.ClientHandle);
            if (tag == null) return;
            tag.ItemName = output.Data.Result.ItemName;
            OPCclient1.Current.ReadsAsync(tag.ItemName);
        }
        private void OPC1_OnAsyncReadCompletedHandler(AsyncReadCompletedOutput output)
        {
            if (!output.Data.Ok) return;
            var tag = OPCData_1.FirstOrDefault(a => a.ClientHandle == output.Data.Result.ClientHandle);
            if (tag == null) return;
            tag.ItemName = output.Data.Result.ItemName;
            tag.Value = output.Data.Result.Value;
            tag.Quality = output.Data.Result.Quality?.ToString();
            tag.Counter += 1;
            tag.Flag = true;
            tag.Timestamp = output.Data.Result.Timestamp?.ToString();
            tag.TipeReq = output.Data.Result.RequestType;
        }
        private void OPC1_OnServerShutdownHandler(Server server, string arg2)
        {
            OPCclient1?.Disconnect();
            OPCStatus1.Connected = false;
            menu_connect_opc.Image = Properties.Resources.icons8_disconnect;
            menu_connect_opc.Text = "Disconnect";
            ShowErrorMessage($"Server {server} Shutdown karena {arg2}");
        }
        private void OPC1_OnDataChangedHandler(DataChangedOutput output)
        {
            var tag = OPCData_1.FirstOrDefault(a => a.ClientHandle == output.Data.ClientHandle);
            if (tag == null) return;
            tag.ItemName = output.Data.ItemName;
            tag.Value = output.Data.Value;
            tag.Quality = output.Data.Quality?.ToString();
            tag.Counter += 1;
            tag.Flag = true;
            tag.Timestamp = output.Data.Timestamp?.ToString();
            tag.TipeReq = output.Data.RequestType;
        }
        public void OPCWriteAsync1(int itm_no, object val)
        {
            try
            {
                var tag = OPCData_1.FirstOrDefault(a => a.ClientHandle == itm_no);
                if (tag == null) return;
                var result = OPCclient1.Current.WriteAsync(tag.ItemName, val);
                if (result.Ok) return;
                MessageBox.Show($"Write value failed.", @"Opc Da Browser", MessageBoxButtons.OK, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Write value failed:{ex.Message}", @"Opc Da Browser", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
            }
        }
        public void OPCReadAsync1(int itm_no)
        {
            try
            {
                var tag = OPCData_1.FirstOrDefault(a => a.ClientHandle == itm_no);
                if (tag == null) return;
                var result = OPCclient1.Current.ReadAsync(tag.ItemName);
                if (result.Ok) return;
                MessageBox.Show($"ReadsAsync value failed.", @"Opc Da Browser", MessageBoxButtons.OK, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Read value failed:{ex.Message}", @"Opc Da Browser", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
            }
        }
        public TipeOfData GetOPCDataValue<TipeOfData>(int clientHandle, TipeOfData defaultValue = default)
        {
            var matchingData = OPCData_1.FirstOrDefault(dataItem => dataItem.ClientHandle == clientHandle);
            if (matchingData == null) return defaultValue;
            try
            {
                return (TipeOfData)Convert.ChangeType(matchingData.Value, typeof(TipeOfData));
            }
            catch (InvalidCastException)
            {
                return defaultValue;
            }
        }
        private void timer_handle_opc_tag1_Tick(object sender, EventArgs e)
        {
            Dictionary<int, Action> tagMapping = new Dictionary<int, Action> // Dictionary untuk menyimpan informasi terkait indeks dan fungsi delegate yang sesuai
            {
                { 1, () => BeginInvoke((MethodInvoker)delegate { uc_x_hmi.glgSetTag1.SetDRsc(uc_x_hmi.glgControl_hmi1, uc_x_hmi.glgSetTag1.TagMaps["valve_flow"], GetOPCDataValue<bool>(1)); }) },
                { 2, () => BeginInvoke((MethodInvoker)delegate {
                    uc_x_hmi.glgSetTag1.SetDRsc(uc_x_hmi.glgControl_hmi1, uc_x_hmi.glgSetTag1.TagMaps["pump_flow"], GetOPCDataValue<bool>(2));
                    uc_x_hmi.glgSetTag1.SetDRsc(uc_x_hmi.glgControl_hmi1, uc_x_hmi.glgSetTag1.TagMaps["sensor_flow"], GetOPCDataValue<bool>(2));
                }) },
                { 3, () => BeginInvoke((MethodInvoker)delegate { uc_x_hmi.glgSetTag1.BtnGlgSet(uc_x_hmi.glgControl_hmi1, "set_on_off", GetOPCDataValue<bool>(3), "ON", "OFF", 0.0, 0.725475, 0.0, 0.945892, 0.0, 0.0); }) },
                { 4, () => BeginInvoke((MethodInvoker)delegate {
                    if(GetOPCDataValue<bool>(4) && !GetOPCDataValue<bool>(1)) {
                        if(log_to_db(Properties.Settings.Default.fl1_header, GetOPCDataValue<bool>(5) ? "Auto" : "Manual", GetOPCDataValue<double>(7), GetOPCDataValue<double>(9), GetOPCDataValue<double>(6), flow_meter1.label_sumber, flow_meter1.label_transfer))
                        {  OPCWriteAsync1(4, false);
                            OPCStatus1.IsLogData = true;
                        }
                        else
                        {   log_to_db(Properties.Settings.Default.fl1_header, GetOPCDataValue<bool>(5) ? "Auto" : "Manual", GetOPCDataValue<double>(7), GetOPCDataValue<double>(9), GetOPCDataValue<double>(6), flow_meter1.label_sumber, flow_meter1.label_transfer);
                            OPCWriteAsync1(4, false);
                            OPCStatus1.IsLogData = true;
                        }
                    }
                }) },
                { 5, () => BeginInvoke((MethodInvoker)delegate { uc_x_hmi.glgSetTag1.SetSRsc(uc_x_hmi.glgControl_hmi1, "text_mode", GetOPCDataValue<bool>(5) ? "Auto" : "Manual"); }) },
                { 6, () => BeginInvoke((MethodInvoker)delegate { uc_x_hmi.glgSetTag1.SetDRsc(uc_x_hmi.glgControl_hmi1, "val_k-factor/Value", GetOPCDataValue<double>(6)); }) },
                { 7, () => BeginInvoke((MethodInvoker)delegate { uc_x_hmi.glgSetTag1.SetDRsc(uc_x_hmi.glgControl_hmi1, uc_x_hmi.glgSetTag1.TagMaps["val_setliter"], GetOPCDataValue<double>(7)); }) },
                { 8, () => BeginInvoke((MethodInvoker)delegate { uc_x_hmi.glgSetTag1.SetDRsc(uc_x_hmi.glgControl_hmi1, "val_f-kurang/Value", GetOPCDataValue<double>(8)); }) },
                { 9, () => BeginInvoke((MethodInvoker)delegate {
                    uc_x_hmi.glgSetTag1.SetDRsc(uc_x_hmi.glgControl_hmi1, "val_liter/Value", GetOPCDataValue<double>(9));
                    uc_x_hmi.glgSetTag1.SetDRsc(uc_x_hmi.glgControl_hmi1, "level_liter/Value", GetOPCDataValue<double>(9), GetOPCDataValue<double>(7));
                    uc_x_hmi.glgSetTag1.SetDRsc(uc_x_hmi.glgControl_hmi1, "level_liter/LowLevel/LevelLow", GetOPCDataValue<double>(9), GetOPCDataValue<double>(7));
                }) },
                { 10, () => BeginInvoke((MethodInvoker)delegate { uc_x_hmi.glgSetTag1.SetDRsc(uc_x_hmi.glgControl_hmi1, "val_lpm/Value", GetOPCDataValue<double>(10)); }) },
                { 21, () => BeginInvoke((MethodInvoker)delegate { if(GetOPCDataValue<bool>(21)){ uc_x_hmi.glgSetTag1.SetSRsc(uc_x_hmi.glgControl_hmi1, "text_transfer", Properties.Settings.Default.fl1_label_tf1); flow_meter1.label_transfer = Properties.Settings.Default.fl1_label_tf1; } }) },
                { 22, () => BeginInvoke((MethodInvoker)delegate { if(GetOPCDataValue<bool>(22)){ uc_x_hmi.glgSetTag1.SetSRsc(uc_x_hmi.glgControl_hmi1, "text_transfer", Properties.Settings.Default.fl1_label_tf2); flow_meter1.label_transfer = Properties.Settings.Default.fl1_label_tf2; } }) },
                { 23, () => BeginInvoke((MethodInvoker)delegate { if(GetOPCDataValue<bool>(23)){ uc_x_hmi.glgSetTag1.SetSRsc(uc_x_hmi.glgControl_hmi1, "text_transfer", Properties.Settings.Default.fl1_label_tf3); flow_meter1.label_transfer = Properties.Settings.Default.fl1_label_tf3; } }) },
                { 24, () => BeginInvoke((MethodInvoker)delegate { if(GetOPCDataValue<bool>(24)){ uc_x_hmi.glgSetTag1.SetSRsc(uc_x_hmi.glgControl_hmi1, "text_transfer", Properties.Settings.Default.fl1_label_tf4); flow_meter1.label_transfer = Properties.Settings.Default.fl1_label_tf4; } }) },

                { 11, () => BeginInvoke((MethodInvoker)delegate { uc_x_hmi.glgSetTag2.SetDRsc(uc_x_hmi.glgControl_hmi2, uc_x_hmi.glgSetTag2.TagMaps["valve_flow"], GetOPCDataValue<bool>(11)); }) },
                { 12, () => BeginInvoke((MethodInvoker)delegate {
                    uc_x_hmi.glgSetTag2.SetDRsc(uc_x_hmi.glgControl_hmi2, uc_x_hmi.glgSetTag2.TagMaps["pump_flow"], GetOPCDataValue<bool>(12));
                    uc_x_hmi.glgSetTag2.SetDRsc(uc_x_hmi.glgControl_hmi2, uc_x_hmi.glgSetTag2.TagMaps["sensor_flow"], GetOPCDataValue<bool>(12));
                }) },
                { 13, () => BeginInvoke((MethodInvoker)delegate { uc_x_hmi.glgSetTag2.BtnGlgSet(uc_x_hmi.glgControl_hmi2, "set_on_off", GetOPCDataValue<bool>(13), "ON", "OFF", 0.0, 0.725475, 0.0, 0.945892, 0.0, 0.0); }) },
                { 14, () => BeginInvoke((MethodInvoker)delegate {
                    if(GetOPCDataValue<bool>(14) && !GetOPCDataValue<bool>(11)) {
                        if(log_to_db(Properties.Settings.Default.fl2_header, GetOPCDataValue<bool>(15) ? "Auto" : "Manual", GetOPCDataValue<double>(17), GetOPCDataValue<double>(19), GetOPCDataValue<double>(16), flow_meter2.label_sumber, flow_meter2.label_transfer))
                        {   OPCWriteAsync1(14, false);
                            OPCStatus1.IsLogData = true;
                        }
                        else
                        {   log_to_db(Properties.Settings.Default.fl2_header, GetOPCDataValue<bool>(15) ? "Auto" : "Manual", GetOPCDataValue<double>(17), GetOPCDataValue<double>(19), GetOPCDataValue<double>(16), flow_meter2.label_sumber, flow_meter2.label_transfer);
                            OPCWriteAsync1(14, false);
                            OPCStatus1.IsLogData = true;
                        }
                    }
                }) },
                { 15, () => BeginInvoke((MethodInvoker)delegate { uc_x_hmi.glgSetTag2.SetSRsc(uc_x_hmi.glgControl_hmi2, "text_mode", GetOPCDataValue<bool>(15) ? "Auto" : "Manual"); }) },
                { 16, () => BeginInvoke((MethodInvoker)delegate { uc_x_hmi.glgSetTag2.SetDRsc(uc_x_hmi.glgControl_hmi2, "val_k-factor/Value", GetOPCDataValue<double>(16)); }) },
                { 17, () => BeginInvoke((MethodInvoker)delegate { uc_x_hmi.glgSetTag2.SetDRsc(uc_x_hmi.glgControl_hmi2, uc_x_hmi.glgSetTag2.TagMaps["val_setliter"], GetOPCDataValue<double>(17)); }) },
                { 18, () => BeginInvoke((MethodInvoker)delegate { uc_x_hmi.glgSetTag2.SetDRsc(uc_x_hmi.glgControl_hmi2, "val_f-kurang/Value", GetOPCDataValue<double>(18)); }) },
                { 19, () => BeginInvoke((MethodInvoker)delegate {
                    uc_x_hmi.glgSetTag2.SetDRsc(uc_x_hmi.glgControl_hmi2, "val_liter/Value", GetOPCDataValue<double>(19));
                    uc_x_hmi.glgSetTag2.SetDRsc(uc_x_hmi.glgControl_hmi2, "level_liter/Value", GetOPCDataValue<double>(19), GetOPCDataValue<double>(17));
                    uc_x_hmi.glgSetTag2.SetDRsc(uc_x_hmi.glgControl_hmi2, "level_liter/LowLevel/LevelLow", GetOPCDataValue<double>(19), GetOPCDataValue<double>(17));
                }) },
                { 20, () => BeginInvoke((MethodInvoker)delegate { uc_x_hmi.glgSetTag2.SetDRsc(uc_x_hmi.glgControl_hmi2, "val_lpm/Value", GetOPCDataValue<double>(20)); }) },
                { 25, () => BeginInvoke((MethodInvoker)delegate { if(GetOPCDataValue<bool>(25)){ uc_x_hmi.glgSetTag2.SetSRsc(uc_x_hmi.glgControl_hmi2, "text_transfer", Properties.Settings.Default.fl2_label_tf1); flow_meter2.label_transfer = Properties.Settings.Default.fl2_label_tf1; } }) },
                { 26, () => BeginInvoke((MethodInvoker)delegate { if(GetOPCDataValue<bool>(26)){ uc_x_hmi.glgSetTag2.SetSRsc(uc_x_hmi.glgControl_hmi2, "text_transfer", Properties.Settings.Default.fl2_label_tf2); flow_meter2.label_transfer = Properties.Settings.Default.fl2_label_tf2; } }) },
                { 27, () => BeginInvoke((MethodInvoker)delegate { if(GetOPCDataValue<bool>(27)){ uc_x_hmi.glgSetTag2.SetSRsc(uc_x_hmi.glgControl_hmi2, "text_transfer", Properties.Settings.Default.fl2_label_tf3); flow_meter2.label_transfer = Properties.Settings.Default.fl2_label_tf3; } }) },
                { 28, () => BeginInvoke((MethodInvoker)delegate { if(GetOPCDataValue<bool>(28)){ uc_x_hmi.glgSetTag2.SetSRsc(uc_x_hmi.glgControl_hmi2, "text_transfer", Properties.Settings.Default.fl2_label_tf4); flow_meter2.label_transfer = Properties.Settings.Default.fl2_label_tf4; } }) },
            };
            foreach (var kvp in tagMapping.OrderBy(x => GetOPCDataValue<bool>(x.Key)))
            {
                Action action = kvp.Value; int clientHandle = kvp.Key;
                if (OPCData_1.Any(data => data.ClientHandle == clientHandle && data.Flag))
                {
                    action.Invoke();
                    OPCData_1.First(data => data.ClientHandle == clientHandle).Flag = false;
                }
            }
        }
        private void timer_handle_opc_tag2_Tick(object sender, EventArgs e)
        {
            Dictionary<int, Action> tagMapping = new Dictionary<int, Action> // Dictionary untuk menyimpan informasi terkait indeks dan fungsi delegate yang sesuai
            {
                { 29, () => BeginInvoke((MethodInvoker)delegate { uc_x_hmi.glgSetTag3.SetDRsc(uc_x_hmi.glgControl_hmi3, uc_x_hmi.glgSetTag3.TagMaps["valve_flow"], GetOPCDataValue<bool>(29)); }) },
                { 30, () => BeginInvoke((MethodInvoker)delegate {
                    uc_x_hmi.glgSetTag3.SetDRsc(uc_x_hmi.glgControl_hmi3, uc_x_hmi.glgSetTag3.TagMaps["pump_flow"], GetOPCDataValue<bool>(30));
                    uc_x_hmi.glgSetTag3.SetDRsc(uc_x_hmi.glgControl_hmi3, uc_x_hmi.glgSetTag3.TagMaps["sensor_flow"], GetOPCDataValue<bool>(30));
                }) },
                { 31, () => BeginInvoke((MethodInvoker)delegate { uc_x_hmi.glgSetTag3.BtnGlgSet(uc_x_hmi.glgControl_hmi3, "set_on_off", GetOPCDataValue<bool>(31), "ON", "OFF", 0.0, 0.725475, 0.0, 0.945892, 0.0, 0.0); }) },
                { 32, () => BeginInvoke((MethodInvoker)delegate {
                    if(GetOPCDataValue<bool>(32) && !GetOPCDataValue<bool>(29)) {
                        if(log_to_db(Properties.Settings.Default.fl3_header, GetOPCDataValue<bool>(33) ? "Auto" : "Manual", GetOPCDataValue<double>(35), GetOPCDataValue<double>(37), GetOPCDataValue<double>(34), flow_meter3.label_sumber, flow_meter3.label_transfer))
                        { OPCWriteAsync1(32, false);
                            OPCStatus1.IsLogData = true;
                        }
                        else
                        {   log_to_db(Properties.Settings.Default.fl3_header, GetOPCDataValue<bool>(33) ? "Auto" : "Manual", GetOPCDataValue<double>(35), GetOPCDataValue<double>(37), GetOPCDataValue<double>(34), flow_meter3.label_sumber, flow_meter3.label_transfer);
                            OPCWriteAsync1(32, false);
                            OPCStatus1.IsLogData = true;
                        }
                    }
                }) },
                { 33, () => BeginInvoke((MethodInvoker)delegate { uc_x_hmi.glgSetTag3.SetSRsc(uc_x_hmi.glgControl_hmi3, "text_mode", GetOPCDataValue<bool>(33) ? "Auto" : "Manual"); }) },
                { 34, () => BeginInvoke((MethodInvoker)delegate { uc_x_hmi.glgSetTag3.SetDRsc(uc_x_hmi.glgControl_hmi3, "val_k-factor/Value", GetOPCDataValue<double>(34)); }) },
                { 35, () => BeginInvoke((MethodInvoker)delegate { uc_x_hmi.glgSetTag3.SetDRsc(uc_x_hmi.glgControl_hmi3, uc_x_hmi.glgSetTag3.TagMaps["val_setliter"], GetOPCDataValue<double>(35)); }) },
                { 36, () => BeginInvoke((MethodInvoker)delegate { uc_x_hmi.glgSetTag3.SetDRsc(uc_x_hmi.glgControl_hmi3, "val_f-kurang/Value", GetOPCDataValue<double>(36)); }) },
                { 37, () => BeginInvoke((MethodInvoker)delegate {
                    uc_x_hmi.glgSetTag3.SetDRsc(uc_x_hmi.glgControl_hmi3, "val_liter/Value", GetOPCDataValue<double>(37));
                    uc_x_hmi.glgSetTag3.SetDRsc(uc_x_hmi.glgControl_hmi3, "level_liter/Value", GetOPCDataValue<double>(37), GetOPCDataValue<double>(35));
                    uc_x_hmi.glgSetTag3.SetDRsc(uc_x_hmi.glgControl_hmi3, "level_liter/LowLevel/LevelLow", GetOPCDataValue<double>(37), GetOPCDataValue<double>(35));
                }) },
                { 38, () => BeginInvoke((MethodInvoker)delegate { uc_x_hmi.glgSetTag3.SetDRsc(uc_x_hmi.glgControl_hmi3, "val_lpm/Value", GetOPCDataValue<double>(38)); }) },
                { 49, () => BeginInvoke((MethodInvoker)delegate { if(GetOPCDataValue<bool>(49)){ uc_x_hmi.glgSetTag3.SetSRsc(uc_x_hmi.glgControl_hmi3, "text_transfer", Properties.Settings.Default.fl3_label_tf1); flow_meter3.label_transfer = Properties.Settings.Default.fl3_label_tf1; } }) },
                { 50, () => BeginInvoke((MethodInvoker)delegate { if(GetOPCDataValue<bool>(50)){ uc_x_hmi.glgSetTag3.SetSRsc(uc_x_hmi.glgControl_hmi3, "text_transfer", Properties.Settings.Default.fl3_label_tf2); flow_meter3.label_transfer = Properties.Settings.Default.fl3_label_tf2; } }) },
                { 51, () => BeginInvoke((MethodInvoker)delegate { if(GetOPCDataValue<bool>(51)){ uc_x_hmi.glgSetTag3.SetSRsc(uc_x_hmi.glgControl_hmi3, "text_transfer", Properties.Settings.Default.fl3_label_tf3); flow_meter3.label_transfer = Properties.Settings.Default.fl3_label_tf3; } }) },
                { 52, () => BeginInvoke((MethodInvoker)delegate { if(GetOPCDataValue<bool>(52)){ uc_x_hmi.glgSetTag3.SetSRsc(uc_x_hmi.glgControl_hmi3, "text_transfer", Properties.Settings.Default.fl3_label_tf4); flow_meter3.label_transfer = Properties.Settings.Default.fl3_label_tf4; } }) },
                { 53, () => BeginInvoke((MethodInvoker)delegate { if(GetOPCDataValue<bool>(53)){ uc_x_hmi.glgSetTag3.SetSRsc(uc_x_hmi.glgControl_hmi3, "text_sumber", Properties.Settings.Default.fl3_label_sumber1); flow_meter3.label_sumber = Properties.Settings.Default.fl3_label_sumber1; } }) },
                { 54, () => BeginInvoke((MethodInvoker)delegate { if(GetOPCDataValue<bool>(54)){ uc_x_hmi.glgSetTag3.SetSRsc(uc_x_hmi.glgControl_hmi3, "text_sumber", Properties.Settings.Default.fl3_label_sumber2); flow_meter3.label_sumber = Properties.Settings.Default.fl3_label_sumber2; } }) },


                { 39, () => BeginInvoke((MethodInvoker)delegate { uc_x_hmi.glgSetTag4.SetDRsc(uc_x_hmi.glgControl_hmi4, uc_x_hmi.glgSetTag4.TagMaps["valve_flow"], GetOPCDataValue<bool>(39)); }) },
                { 40, () => BeginInvoke((MethodInvoker)delegate {
                    uc_x_hmi.glgSetTag4.SetDRsc(uc_x_hmi.glgControl_hmi4, uc_x_hmi.glgSetTag4.TagMaps["pump_flow"], GetOPCDataValue<bool>(40));
                    uc_x_hmi.glgSetTag4.SetDRsc(uc_x_hmi.glgControl_hmi4, uc_x_hmi.glgSetTag4.TagMaps["sensor_flow"], GetOPCDataValue<bool>(40));
                }) },
                { 41, () => BeginInvoke((MethodInvoker)delegate { uc_x_hmi.glgSetTag4.BtnGlgSet(uc_x_hmi.glgControl_hmi4, "set_on_off", GetOPCDataValue<bool>(41), "ON", "OFF", 0.0, 0.725475, 0.0, 0.945892, 0.0, 0.0); }) },
                { 42, () => BeginInvoke((MethodInvoker)delegate {
                    if(GetOPCDataValue<bool>(42) && !GetOPCDataValue<bool>(39)) {
                        if(log_to_db(Properties.Settings.Default.fl4_header, GetOPCDataValue<bool>(43) ? "Auto" : "Manual", GetOPCDataValue<double>(45), GetOPCDataValue<double>(47), GetOPCDataValue<double>(44), flow_meter4.label_sumber, flow_meter4.label_transfer))
                        {
                            OPCWriteAsync1(42, false);
                            OPCStatus1.IsLogData = true;
                        }
                            
                        else
                        {
                            log_to_db(Properties.Settings.Default.fl4_header, GetOPCDataValue<bool>(43) ? "Auto" : "Manual", GetOPCDataValue<double>(45), GetOPCDataValue<double>(47), GetOPCDataValue<double>(44), flow_meter4.label_sumber, flow_meter4.label_transfer);
                            OPCWriteAsync1(42, false);
                            OPCStatus1.IsLogData = true;
                        }
                    }
                }) },
                { 43, () => BeginInvoke((MethodInvoker)delegate { uc_x_hmi.glgSetTag4.SetSRsc(uc_x_hmi.glgControl_hmi4, "text_mode", GetOPCDataValue<bool>(43) ? "Auto" : "Manual"); }) },
                { 44, () => BeginInvoke((MethodInvoker)delegate { uc_x_hmi.glgSetTag4.SetDRsc(uc_x_hmi.glgControl_hmi4, "val_k-factor/Value", GetOPCDataValue<double>(44)); }) },
                { 45, () => BeginInvoke((MethodInvoker)delegate { uc_x_hmi.glgSetTag4.SetDRsc(uc_x_hmi.glgControl_hmi4, uc_x_hmi.glgSetTag4.TagMaps["val_setliter"], GetOPCDataValue<double>(45)); }) },
                { 46, () => BeginInvoke((MethodInvoker)delegate { uc_x_hmi.glgSetTag4.SetDRsc(uc_x_hmi.glgControl_hmi4, "val_f-kurang/Value", GetOPCDataValue<double>(46)); }) },
                { 47, () => BeginInvoke((MethodInvoker)delegate {
                    uc_x_hmi.glgSetTag4.SetDRsc(uc_x_hmi.glgControl_hmi4, "val_liter/Value", GetOPCDataValue<double>(47));
                    uc_x_hmi.glgSetTag4.SetDRsc(uc_x_hmi.glgControl_hmi4, "level_liter/Value", GetOPCDataValue<double>(47), GetOPCDataValue<double>(45));
                    uc_x_hmi.glgSetTag4.SetDRsc(uc_x_hmi.glgControl_hmi4, "level_liter/LowLevel/LevelLow", GetOPCDataValue<double>(47), GetOPCDataValue<double>(45));
                }) },
                { 48, () => BeginInvoke((MethodInvoker)delegate { uc_x_hmi.glgSetTag4.SetDRsc(uc_x_hmi.glgControl_hmi4, "val_lpm/Value", GetOPCDataValue<double>(48)); }) },
                { 55, () => BeginInvoke((MethodInvoker)delegate { if(GetOPCDataValue<bool>(55)){ uc_x_hmi.glgSetTag4.SetSRsc(uc_x_hmi.glgControl_hmi4, "text_transfer", Properties.Settings.Default.fl4_label_tf1); flow_meter4.label_transfer = Properties.Settings.Default.fl4_label_tf1; } }) },
                { 56, () => BeginInvoke((MethodInvoker)delegate { if(GetOPCDataValue<bool>(56)){ uc_x_hmi.glgSetTag4.SetSRsc(uc_x_hmi.glgControl_hmi4, "text_transfer", Properties.Settings.Default.fl4_label_tf2); flow_meter4.label_transfer = Properties.Settings.Default.fl4_label_tf2; } }) },
                { 57, () => BeginInvoke((MethodInvoker)delegate { if(GetOPCDataValue<bool>(57)){ uc_x_hmi.glgSetTag4.SetSRsc(uc_x_hmi.glgControl_hmi4, "text_transfer", Properties.Settings.Default.fl4_label_tf3); flow_meter4.label_transfer = Properties.Settings.Default.fl4_label_tf3; } }) },
                { 58, () => BeginInvoke((MethodInvoker)delegate { if(GetOPCDataValue<bool>(58)){ uc_x_hmi.glgSetTag4.SetSRsc(uc_x_hmi.glgControl_hmi4, "text_transfer", Properties.Settings.Default.fl4_label_tf4); flow_meter4.label_transfer = Properties.Settings.Default.fl4_label_tf4; } }) },
                { 59, () => BeginInvoke((MethodInvoker)delegate { if(GetOPCDataValue<bool>(59)){ uc_x_hmi.glgSetTag4.SetSRsc(uc_x_hmi.glgControl_hmi4, "text_sumber", Properties.Settings.Default.fl4_label_sumber1); flow_meter4.label_sumber = Properties.Settings.Default.fl4_label_sumber1; } }) },
                { 60, () => BeginInvoke((MethodInvoker)delegate { if(GetOPCDataValue<bool>(60)){ uc_x_hmi.glgSetTag4.SetSRsc(uc_x_hmi.glgControl_hmi4, "text_sumber", Properties.Settings.Default.fl4_label_sumber2); flow_meter4.label_sumber = Properties.Settings.Default.fl4_label_sumber2; } }) },
            };
            foreach (var kvp in tagMapping.OrderBy(x => GetOPCDataValue<bool>(x.Key)))
            {
                Action action = kvp.Value; int clientHandle = kvp.Key;
                if (OPCData_1.Any(data => data.ClientHandle == clientHandle && data.Flag))
                {
                    action.Invoke();
                    OPCData_1.First(data => data.ClientHandle == clientHandle).Flag = false;
                }
            }
        }
        private bool log_to_db(string _flow_meter, string _mode, double _set_liter, double _liter, double _k_factor, string _from_source, string _transfer_to)
        {
            var db = DbDataAccess.Db();
            if (db == null) { ShowErrorMessage("Koneksi database gagal. Periksa pengaturan koneksi atau hubungi administrator"); return false; }
            var log_fl_data = new { flow_meter = _flow_meter, mode = _mode, set_liter = Math.Round(_set_liter, 2), liter = Math.Round(_liter, 2), k_factor = Math.Round(_k_factor, 3), from_source = _from_source, transfer_to = _transfer_to, date_time = DateTime.Now, };
            Query query = db.Query(Properties.Settings.Default.tabel_db_flowmeter);
            try
            {
                int affected = query.Insert(log_fl_data);
                var lastInsertIdQuery = db.Query(Properties.Settings.Default.tabel_db_flowmeter).OrderByDesc("id").Limit(1);
                var lastInsertIdResult = lastInsertIdQuery.First();
                //add new tabel view
            }
            catch (Exception ex) { ShowErrorMessage(ex is NullReferenceException ? $"Insert DB Terjadi kesalahan null reference: {ex.Message}" : $"Insert DB Terjadi kesalahan: {ex.Message}"); return false; }

            return true;
        }
        private void timer_delete_glg_popup_Tick(object sender, EventArgs e)
        {
            var foundForm = Application.OpenForms.Cast<Form>().FirstOrDefault(form => form.Text.Contains("GLG Toolkit"));
            if (foundForm != null) foundForm.Close();
        }
        private void menu_tag_Click(object sender, EventArgs e)
        {
            string password =  Prompt.PasswordSetting(false, "", out bool cancle_); if (cancle_) return;
            if (password != Properties.Settings.Default.password) { ShowWarningMessage("Password salah"); return; }
            form_tag _form_tag = form_tag.GetInstance(this);
            _form_tag.Show();
            _form_tag.BringToFront();
        }
        private void menu_label_Click(object sender, EventArgs e)
        {
            form_label _form_label = form_label.GetInstance(this);
            _form_label.Show();
            _form_label.BringToFront();
        }
        private void XamppOpen()
        {
            Process[] processes = Process.GetProcessesByName("xampp-control");
            if (processes.Length > 0) return;
            try
            {
                BeginInvoke((MethodInvoker)delegate
                {
                    ProcessStartInfo startInfo = new ProcessStartInfo { FileName = @"C:\xampp\xampp-control.exe", WindowStyle = ProcessWindowStyle.Minimized };
                    Process.Start(startInfo);
                });
            }
            catch (System.ComponentModel.Win32Exception ex)
            {
                ShowErrorMessage($"Terjadi kesalahan saat membuka XAMPP Control Panel: {ex.Message}");
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Terjadi kesalahan: {ex.Message}");
            }
        }
        private void menu_hmi_Click(object sender, EventArgs e) => show_to_main_panel(uc_x_hmi);
        private void menu_log_Click(object sender, EventArgs e) => show_to_main_panel(uc_x_log);
        private void add_to_main_panel(UserControl uc_x) { uc_x.Dock = DockStyle.Fill; panel_main.Controls.Add(uc_x); uc_x.BringToFront(); }
        private void show_to_main_panel(UserControl uc_x) => uc_x.BringToFront();
        private void menu_connect_opc_Click(object sender, EventArgs e) => OPC1Connect_or_Disconnect(OPCStatus1.Connected);
        public void ShowWarningMessage(string message) => ShowMessage(message, "Warning", MessageBoxIcon.Warning);
        public void ShowErrorMessage(string message) => ShowMessage(message, "Error", MessageBoxIcon.Error);
        private void ShowMessage(string message, string caption, MessageBoxIcon icon) { if (!IsHandleCreated) return; BeginInvoke((MethodInvoker)delegate { MessageBox.Show(this, message, caption, MessageBoxButtons.OK, icon); }); }
        private void menu_minimize_Click(object sender, EventArgs e) => WindowState = FormWindowState.Minimized;
        private void menu_exit_Click(object sender, EventArgs e) => OnClosed(EventArgs.Empty);
        private void menu_file_exit_Click(object sender, EventArgs e) => OnClosed(EventArgs.Empty);
        protected override void OnClosed(EventArgs args)
        {
            OPC1Connect_or_Disconnect(true);
            Application.Exit();
        }
    }
    public static class DbDataAccess
    {
        private static MySqlConnection connection = null;

        public static MySqlConnection GetConnection()
        {
            if (connection == null)
            {
                try
                {
                    connection = new MySqlConnection($"Server=localhost;Database=flowmeter_db;Uid=root;");
                    connection.Open();
                }
                catch
                {
                    connection?.Close();
                    connection = null;
                }
            }
            return connection;
        }

        private static QueryFactory db = null;

        public static QueryFactory Db()
        {
            if (db == null)
            {
                try
                {
                    var compiler = new MySqlCompiler();
                    db = new QueryFactory(GetConnection(), compiler);
                }
                catch
                {
                    db = null;
                }
            }
            return db;
        }
    }

}
