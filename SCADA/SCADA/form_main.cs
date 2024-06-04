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
            timer_handle_opc_tag3.Enabled = true;
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
                { 3, () => BeginInvoke((MethodInvoker)delegate { uc_x_hmi.glgSetTag2.SetDRsc(uc_x_hmi.glgControl_hmi2, uc_x_hmi.glgSetTag2.TagMaps["valve_flow"], GetOPCDataValue<bool>(3)); }) },
                { 4, () => BeginInvoke((MethodInvoker)delegate {
                    uc_x_hmi.glgSetTag2.SetDRsc(uc_x_hmi.glgControl_hmi2, uc_x_hmi.glgSetTag2.TagMaps["pump_flow"], GetOPCDataValue<bool>(4));
                    uc_x_hmi.glgSetTag2.SetDRsc(uc_x_hmi.glgControl_hmi2, uc_x_hmi.glgSetTag2.TagMaps["sensor_flow"], GetOPCDataValue<bool>(4));
                }) },
                { 5, () => BeginInvoke((MethodInvoker)delegate { uc_x_hmi.glgSetTag1.BtnGlgSet(uc_x_hmi.glgControl_hmi1, "set_on_off", GetOPCDataValue<bool>(5), "ON", "OFF", 0.0, 0.725475, 0.0, 0.945892, 0.0, 0.0); }) },
                { 6, () => BeginInvoke((MethodInvoker)delegate {
                    if(GetOPCDataValue<bool>(6) && !GetOPCDataValue<bool>(1)) {
                        if(!log_to_db(Properties.Settings.Default.fl1_header, GetOPCDataValue<bool>(38) ? "Auto" : "Manual", GetOPCDataValue<double>(42), GetOPCDataValue<double>(44), GetOPCDataValue<double>(41), flow_meter1.label_batch, flow_meter1.label_transfer))
                            log_to_db(Properties.Settings.Default.fl1_header, GetOPCDataValue<bool>(38) ? "Auto" : "Manual", GetOPCDataValue<double>(42), GetOPCDataValue<double>(44), GetOPCDataValue<double>(41), flow_meter1.label_batch, flow_meter1.label_transfer);
                        OPCWriteAsync1(6, false);
                        OPCStatus1.IsLogData = true;
                    }
                }) },
                { 7, () => BeginInvoke((MethodInvoker)delegate { uc_x_hmi.glgSetTag2.BtnGlgSet(uc_x_hmi.glgControl_hmi2, "set_on_off", GetOPCDataValue<bool>(7), "ON", "OFF", 0.0, 0.725475, 0.0, 0.945892, 0.0, 0.0); }) },
                { 8, () => BeginInvoke((MethodInvoker)delegate {
                    if(GetOPCDataValue<bool>(8) && !GetOPCDataValue<bool>(3)) {
                        if(!log_to_db(Properties.Settings.Default.fl2_header, GetOPCDataValue<bool>(39) ? "Auto" : "Manual", GetOPCDataValue<double>(47), GetOPCDataValue<double>(49), GetOPCDataValue<double>(46), flow_meter2.label_batch, flow_meter2.label_transfer))
                            log_to_db(Properties.Settings.Default.fl2_header, GetOPCDataValue<bool>(39) ? "Auto" : "Manual", GetOPCDataValue<double>(47), GetOPCDataValue<double>(49), GetOPCDataValue<double>(46), flow_meter2.label_batch, flow_meter2.label_transfer);
                        OPCWriteAsync1(8, false);
                        OPCStatus1.IsLogData = true;
                    }
                }) },
                { 9, () => BeginInvoke((MethodInvoker)delegate { if(GetOPCDataValue<bool>(9)){ uc_x_hmi.glgSetTag1.SetSRsc(uc_x_hmi.glgControl_hmi1, "text_transfer", Properties.Settings.Default.fl1_label_tf1); flow_meter1.label_transfer = Properties.Settings.Default.fl1_label_tf1; } }) },
                { 10, () => BeginInvoke((MethodInvoker)delegate { if(GetOPCDataValue<bool>(10)){ uc_x_hmi.glgSetTag1.SetSRsc(uc_x_hmi.glgControl_hmi1, "text_transfer", Properties.Settings.Default.fl1_label_tf2); flow_meter1.label_transfer = Properties.Settings.Default.fl1_label_tf2; } }) },
                { 11, () => BeginInvoke((MethodInvoker)delegate { if(GetOPCDataValue<bool>(11)){ uc_x_hmi.glgSetTag1.SetSRsc(uc_x_hmi.glgControl_hmi1, "text_transfer", Properties.Settings.Default.fl1_label_tf3); flow_meter1.label_transfer = Properties.Settings.Default.fl1_label_tf3; } }) },
                { 12, () => BeginInvoke((MethodInvoker)delegate { if(GetOPCDataValue<bool>(12)){ uc_x_hmi.glgSetTag1.SetSRsc(uc_x_hmi.glgControl_hmi1, "text_transfer", Properties.Settings.Default.fl1_label_tf4); flow_meter1.label_transfer = Properties.Settings.Default.fl1_label_tf4; } }) },
                { 13, () => BeginInvoke((MethodInvoker)delegate { if(GetOPCDataValue<bool>(13)){ uc_x_hmi.glgSetTag1.SetSRsc(uc_x_hmi.glgControl_hmi1, "text_transfer", Properties.Settings.Default.fl1_label_tf5); flow_meter1.label_transfer = Properties.Settings.Default.fl1_label_tf5; } }) },

                { 14, () => BeginInvoke((MethodInvoker)delegate { if(GetOPCDataValue<bool>(14)){ uc_x_hmi.glgSetTag2.SetSRsc(uc_x_hmi.glgControl_hmi2, "text_transfer", Properties.Settings.Default.fl2_label_tf1); flow_meter2.label_transfer = Properties.Settings.Default.fl2_label_tf1; } }) },
                { 15, () => BeginInvoke((MethodInvoker)delegate { if(GetOPCDataValue<bool>(15)){ uc_x_hmi.glgSetTag2.SetSRsc(uc_x_hmi.glgControl_hmi2, "text_transfer", Properties.Settings.Default.fl2_label_tf2); flow_meter2.label_transfer = Properties.Settings.Default.fl2_label_tf2; } }) },
                { 16, () => BeginInvoke((MethodInvoker)delegate { if(GetOPCDataValue<bool>(16)){ uc_x_hmi.glgSetTag2.SetSRsc(uc_x_hmi.glgControl_hmi2, "text_transfer", Properties.Settings.Default.fl2_label_tf3); flow_meter2.label_transfer = Properties.Settings.Default.fl2_label_tf3; } }) },
                { 17, () => BeginInvoke((MethodInvoker)delegate { if(GetOPCDataValue<bool>(17)){ uc_x_hmi.glgSetTag2.SetSRsc(uc_x_hmi.glgControl_hmi2, "text_transfer", Properties.Settings.Default.fl2_label_tf4); flow_meter2.label_transfer = Properties.Settings.Default.fl2_label_tf4; } }) },

                { 18, () => BeginInvoke((MethodInvoker)delegate { if(GetOPCDataValue<bool>(18)){     flow_meter1.label_batch = Properties.Settings.Default.fl1_label_batch1; } }) },
                { 19, () => BeginInvoke((MethodInvoker)delegate { if(GetOPCDataValue<bool>(19)){ uc_x_hmi.glgSetTag1.SetSRsc(uc_x_hmi.glgControl_hmi1, "text_batch", Properties.Settings.Default.fl1_label_batch2); flow_meter1.label_batch = Properties.Settings.Default.fl1_label_batch2; } }) },
                { 20, () => BeginInvoke((MethodInvoker)delegate { if(GetOPCDataValue<bool>(20)){ uc_x_hmi.glgSetTag1.SetSRsc(uc_x_hmi.glgControl_hmi1, "text_batch", Properties.Settings.Default.fl1_label_batch3); flow_meter1.label_batch = Properties.Settings.Default.fl1_label_batch3; } }) },
                { 21, () => BeginInvoke((MethodInvoker)delegate { if(GetOPCDataValue<bool>(21)){ uc_x_hmi.glgSetTag1.SetSRsc(uc_x_hmi.glgControl_hmi1, "text_batch", Properties.Settings.Default.fl1_label_batch4); flow_meter1.label_batch = Properties.Settings.Default.fl1_label_batch4; } }) },
                { 22, () => BeginInvoke((MethodInvoker)delegate { if(GetOPCDataValue<bool>(22)){ uc_x_hmi.glgSetTag1.SetSRsc(uc_x_hmi.glgControl_hmi1, "text_batch", Properties.Settings.Default.fl1_label_batch5); flow_meter1.label_batch = Properties.Settings.Default.fl1_label_batch5; } }) },
                { 23, () => BeginInvoke((MethodInvoker)delegate { if(GetOPCDataValue<bool>(23)){ uc_x_hmi.glgSetTag1.SetSRsc(uc_x_hmi.glgControl_hmi1, "text_batch", Properties.Settings.Default.fl1_label_batch6); flow_meter1.label_batch = Properties.Settings.Default.fl1_label_batch6; } }) },
                { 24, () => BeginInvoke((MethodInvoker)delegate { if(GetOPCDataValue<bool>(24)){ uc_x_hmi.glgSetTag1.SetSRsc(uc_x_hmi.glgControl_hmi1, "text_batch", Properties.Settings.Default.fl1_label_batch7); flow_meter1.label_batch = Properties.Settings.Default.fl1_label_batch7; } }) },
                { 25, () => BeginInvoke((MethodInvoker)delegate { if(GetOPCDataValue<bool>(25)){ uc_x_hmi.glgSetTag1.SetSRsc(uc_x_hmi.glgControl_hmi1, "text_batch", Properties.Settings.Default.fl1_label_batch8); flow_meter1.label_batch = Properties.Settings.Default.fl1_label_batch8; } }) },
                { 26, () => BeginInvoke((MethodInvoker)delegate { if(GetOPCDataValue<bool>(26)){ uc_x_hmi.glgSetTag1.SetSRsc(uc_x_hmi.glgControl_hmi1, "text_batch", Properties.Settings.Default.fl1_label_batch9); flow_meter1.label_batch = Properties.Settings.Default.fl1_label_batch9; } }) },
                { 27, () => BeginInvoke((MethodInvoker)delegate { if(GetOPCDataValue<bool>(27)){ uc_x_hmi.glgSetTag1.SetSRsc(uc_x_hmi.glgControl_hmi1, "text_batch", Properties.Settings.Default.fl1_label_batch10); flow_meter1.label_batch = Properties.Settings.Default.fl1_label_batch10; } }) },

                { 28, () => BeginInvoke((MethodInvoker)delegate { if(GetOPCDataValue<bool>(28)){ uc_x_hmi.glgSetTag2.SetSRsc(uc_x_hmi.glgControl_hmi2, "text_batch", Properties.Settings.Default.fl2_label_batch1); flow_meter2.label_batch = Properties.Settings.Default.fl2_label_batch1; } }) },
                { 29, () => BeginInvoke((MethodInvoker)delegate { if(GetOPCDataValue<bool>(29)){ uc_x_hmi.glgSetTag2.SetSRsc(uc_x_hmi.glgControl_hmi2, "text_batch", Properties.Settings.Default.fl2_label_batch2); flow_meter2.label_batch = Properties.Settings.Default.fl2_label_batch2; } }) },
                { 30, () => BeginInvoke((MethodInvoker)delegate { if(GetOPCDataValue<bool>(30)){ uc_x_hmi.glgSetTag2.SetSRsc(uc_x_hmi.glgControl_hmi2, "text_batch", Properties.Settings.Default.fl2_label_batch3); flow_meter2.label_batch = Properties.Settings.Default.fl2_label_batch3; } }) },
                { 31, () => BeginInvoke((MethodInvoker)delegate { if(GetOPCDataValue<bool>(31)){ uc_x_hmi.glgSetTag2.SetSRsc(uc_x_hmi.glgControl_hmi2, "text_batch", Properties.Settings.Default.fl2_label_batch4); flow_meter2.label_batch = Properties.Settings.Default.fl2_label_batch4; } }) },
                { 32, () => BeginInvoke((MethodInvoker)delegate { if(GetOPCDataValue<bool>(32)){ uc_x_hmi.glgSetTag2.SetSRsc(uc_x_hmi.glgControl_hmi2, "text_batch", Properties.Settings.Default.fl2_label_batch5); flow_meter2.label_batch = Properties.Settings.Default.fl2_label_batch5; } }) },
                { 33, () => BeginInvoke((MethodInvoker)delegate { if(GetOPCDataValue<bool>(33)){ uc_x_hmi.glgSetTag2.SetSRsc(uc_x_hmi.glgControl_hmi2, "text_batch", Properties.Settings.Default.fl2_label_batch6); flow_meter2.label_batch = Properties.Settings.Default.fl2_label_batch6; } }) },
                { 34, () => BeginInvoke((MethodInvoker)delegate { if(GetOPCDataValue<bool>(34)){ uc_x_hmi.glgSetTag2.SetSRsc(uc_x_hmi.glgControl_hmi2, "text_batch", Properties.Settings.Default.fl2_label_batch7); flow_meter2.label_batch = Properties.Settings.Default.fl2_label_batch7; } }) },
                { 35, () => BeginInvoke((MethodInvoker)delegate { if(GetOPCDataValue<bool>(35)){ uc_x_hmi.glgSetTag2.SetSRsc(uc_x_hmi.glgControl_hmi2, "text_batch", Properties.Settings.Default.fl2_label_batch8); flow_meter2.label_batch = Properties.Settings.Default.fl2_label_batch8; } }) },
                { 36, () => BeginInvoke((MethodInvoker)delegate { if(GetOPCDataValue<bool>(36)){ uc_x_hmi.glgSetTag2.SetSRsc(uc_x_hmi.glgControl_hmi2, "text_batch", Properties.Settings.Default.fl2_label_batch9); flow_meter2.label_batch = Properties.Settings.Default.fl2_label_batch9; } }) },
                { 37, () => BeginInvoke((MethodInvoker)delegate { if(GetOPCDataValue<bool>(37)){ uc_x_hmi.glgSetTag2.SetSRsc(uc_x_hmi.glgControl_hmi2, "text_batch", Properties.Settings.Default.fl2_label_batch10); flow_meter2.label_batch = Properties.Settings.Default.fl2_label_batch10; } }) },

                { 38, () => BeginInvoke((MethodInvoker)delegate { uc_x_hmi.glgSetTag1.SetSRsc(uc_x_hmi.glgControl_hmi1, "text_mode", GetOPCDataValue<bool>(38) ? "Auto" : "Manual"); }) },
                { 39, () => BeginInvoke((MethodInvoker)delegate { uc_x_hmi.glgSetTag2.SetSRsc(uc_x_hmi.glgControl_hmi2, "text_mode", GetOPCDataValue<bool>(39) ? "Auto" : "Manual"); }) },
                { 40, () => BeginInvoke((MethodInvoker)delegate { uc_x_hmi.label_header_hmi2.Image = uc_x_hmi.label_header_hmi1.Image = GetOPCDataValue<bool>(40) ? Properties.Resources.icons8_connect : Properties.Resources.icons8_disconnect; }) },

                { 41, () => BeginInvoke((MethodInvoker)delegate { uc_x_hmi.glgSetTag1.SetDRsc(uc_x_hmi.glgControl_hmi1, "val_k-factor/Value", GetOPCDataValue<double>(41)); }) },
                { 42, () => BeginInvoke((MethodInvoker)delegate { uc_x_hmi.glgSetTag1.SetDRsc(uc_x_hmi.glgControl_hmi1, uc_x_hmi.glgSetTag1.TagMaps["val_setliter"], GetOPCDataValue<double>(42)); }) },
                { 43, () => BeginInvoke((MethodInvoker)delegate { uc_x_hmi.glgSetTag1.SetDRsc(uc_x_hmi.glgControl_hmi1, "val_f-kurang/Value", GetOPCDataValue<double>(43)); }) },
                { 44, () => BeginInvoke((MethodInvoker)delegate {
                    uc_x_hmi.glgSetTag1.SetDRsc(uc_x_hmi.glgControl_hmi1, "val_liter/Value", GetOPCDataValue<double>(44));
                    uc_x_hmi.glgSetTag1.SetDRsc(uc_x_hmi.glgControl_hmi1, "level_liter/Value", GetOPCDataValue<double>(44), GetOPCDataValue<double>(42));
                    uc_x_hmi.glgSetTag1.SetDRsc(uc_x_hmi.glgControl_hmi1, "level_liter/LowLevel/LevelLow", GetOPCDataValue<double>(44), GetOPCDataValue<double>(42));
                }) },
                { 45, () => BeginInvoke((MethodInvoker)delegate { uc_x_hmi.glgSetTag1.SetDRsc(uc_x_hmi.glgControl_hmi1, "val_lpm/Value", GetOPCDataValue<double>(45)); }) },
 
                { 46, () => BeginInvoke((MethodInvoker)delegate { uc_x_hmi.glgSetTag2.SetDRsc(uc_x_hmi.glgControl_hmi2, "val_k-factor/Value", GetOPCDataValue<double>(46)); }) },
                { 47, () => BeginInvoke((MethodInvoker)delegate { uc_x_hmi.glgSetTag2.SetDRsc(uc_x_hmi.glgControl_hmi2, uc_x_hmi.glgSetTag2.TagMaps["val_setliter"], GetOPCDataValue<double>(47)); }) },
                { 48, () => BeginInvoke((MethodInvoker)delegate { uc_x_hmi.glgSetTag2.SetDRsc(uc_x_hmi.glgControl_hmi2, "val_f-kurang/Value", GetOPCDataValue<double>(48)); }) },
                { 49, () => BeginInvoke((MethodInvoker)delegate {
                    uc_x_hmi.glgSetTag2.SetDRsc(uc_x_hmi.glgControl_hmi2, "val_liter/Value", GetOPCDataValue<double>(49));
                    uc_x_hmi.glgSetTag2.SetDRsc(uc_x_hmi.glgControl_hmi2, "level_liter/Value", GetOPCDataValue<double>(49), GetOPCDataValue<double>(47));
                    uc_x_hmi.glgSetTag2.SetDRsc(uc_x_hmi.glgControl_hmi2, "level_liter/LowLevel/LevelLow", GetOPCDataValue<double>(49), GetOPCDataValue<double>(47));
                }) },
                { 50, () => BeginInvoke((MethodInvoker)delegate { uc_x_hmi.glgSetTag2.SetDRsc(uc_x_hmi.glgControl_hmi2, "val_lpm/Value", GetOPCDataValue<double>(50)); }) },
                { 51, () => BeginInvoke((MethodInvoker)delegate { if(GetOPCDataValue<int>(51) != DateTime.Now.Day) OPCWriteAsync1(51, DateTime.Now.Day); }) },
                { 52, () => BeginInvoke((MethodInvoker)delegate { if(GetOPCDataValue<int>(52) != DateTime.Now.Month) OPCWriteAsync1(52, DateTime.Now.Month); }) },
                { 53, () => BeginInvoke((MethodInvoker)delegate { if(GetOPCDataValue<int>(53) != DateTime.Now.Year) OPCWriteAsync1(53, DateTime.Now.Year); }) },
                { 54, () => BeginInvoke((MethodInvoker)delegate { if(GetOPCDataValue<int>(54) != DateTime.Now.Hour) OPCWriteAsync1(54, DateTime.Now.Hour); }) },
                { 55, () => BeginInvoke((MethodInvoker)delegate { if(GetOPCDataValue<int>(55) != DateTime.Now.Minute){ OPCWriteAsync1(55, DateTime.Now.Minute); OPCWriteAsync1(56, DateTime.Now.Second); } }) },
                { 56, () => BeginInvoke((MethodInvoker)delegate {  }) }
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
                { 57, () => BeginInvoke((MethodInvoker)delegate { uc_x_hmi.glgSetTag3.SetDRsc(uc_x_hmi.glgControl_hmi3, uc_x_hmi.glgSetTag3.TagMaps["sensor_flow"], GetOPCDataValue<bool>(57)); }) },
                { 58, () => BeginInvoke((MethodInvoker)delegate { uc_x_hmi.glgSetTag3.BtnGlgSet(uc_x_hmi.glgControl_hmi3, "set_on_off", GetOPCDataValue<bool>(58), "STOP PRODUKSI", "PRODUKSI", 0.945892, 0.0, 0.0, 0.0, 0.725475, 0.0); }) },
                { 59, () => BeginInvoke((MethodInvoker)delegate {
                   if(GetOPCDataValue<bool>(59)) {
                        if(!log_to_db2(flow_meter3.label_proses_mesin, flow_meter3.label_batch, flow_meter3.label_transfer, GetOPCDataValue<double>(77), GetOPCDataValue<double>(76)))
                            log_to_db2(flow_meter3.label_proses_mesin, flow_meter3.label_batch, flow_meter3.label_transfer, GetOPCDataValue<double>(77), GetOPCDataValue<double>(76));
                        OPCWriteAsync1(59, false);
                        OPCStatus1.IsLogData = true;
                    }
                }) },
                { 60, () => BeginInvoke((MethodInvoker)delegate { uc_x_hmi.glgSetTag4.SetDRsc(uc_x_hmi.glgControl_hmi4, uc_x_hmi.glgSetTag4.TagMaps["sensor_flow"], GetOPCDataValue<bool>(60)); }) },
                { 61, () => BeginInvoke((MethodInvoker)delegate { uc_x_hmi.glgSetTag4.BtnGlgSet(uc_x_hmi.glgControl_hmi4, "set_on_off", GetOPCDataValue<bool>(61), "STOP PRODUKSI", "PRODUKSI", 0.945892, 0.0, 0.0, 0.0, 0.725475, 0.0); }) },
                { 62, () => BeginInvoke((MethodInvoker)delegate {
                    if(GetOPCDataValue<bool>(62)) {
                        if(!log_to_db2(flow_meter4.label_proses_mesin, flow_meter4.label_batch, flow_meter4.label_transfer, GetOPCDataValue<double>(80), GetOPCDataValue<double>(79)))
                            log_to_db2(flow_meter4.label_proses_mesin, flow_meter4.label_batch, flow_meter4.label_transfer, GetOPCDataValue<double>(80), GetOPCDataValue<double>(79));
                        OPCWriteAsync1(62, false);
                        OPCStatus1.IsLogData = true;
                    }
                }) },
                { 63, () => BeginInvoke((MethodInvoker)delegate { if(GetOPCDataValue<bool>(63)){ uc_x_hmi.glgSetTag3.SetSRsc(uc_x_hmi.glgControl_hmi3, "transfer_to/String", Properties.Settings.Default.fl3_label_tf1); flow_meter3.label_transfer = Properties.Settings.Default.fl3_label_tf1; } }) },
                { 64, () => BeginInvoke((MethodInvoker)delegate { if(GetOPCDataValue<bool>(64)){ uc_x_hmi.glgSetTag3.SetSRsc(uc_x_hmi.glgControl_hmi3, "transfer_to/String", Properties.Settings.Default.fl3_label_tf2); flow_meter3.label_transfer = Properties.Settings.Default.fl3_label_tf2; } }) },
                { 65, () => BeginInvoke((MethodInvoker)delegate { if(GetOPCDataValue<bool>(65)){ uc_x_hmi.glgSetTag3.SetSRsc(uc_x_hmi.glgControl_hmi3, "transfer_to/String", Properties.Settings.Default.fl3_label_tf3); flow_meter3.label_transfer = Properties.Settings.Default.fl3_label_tf3; } }) },
                { 66, () => BeginInvoke((MethodInvoker)delegate { if(GetOPCDataValue<bool>(66)){ uc_x_hmi.glgSetTag3.SetSRsc(uc_x_hmi.glgControl_hmi3, "proses_mesin/String", Properties.Settings.Default.fl3_label_pm1); flow_meter3.label_proses_mesin = Properties.Settings.Default.fl3_label_pm1; } }) },
                { 67, () => BeginInvoke((MethodInvoker)delegate { if(GetOPCDataValue<bool>(67)){ uc_x_hmi.glgSetTag3.SetSRsc(uc_x_hmi.glgControl_hmi3, "proses_mesin/String", Properties.Settings.Default.fl3_label_pm2); flow_meter3.label_proses_mesin = Properties.Settings.Default.fl3_label_pm2; } }) },

                { 68, () => BeginInvoke((MethodInvoker)delegate { if(GetOPCDataValue<bool>(68)){ uc_x_hmi.glgSetTag4.SetSRsc(uc_x_hmi.glgControl_hmi4, "transfer_to/String", Properties.Settings.Default.fl4_label_tf1); flow_meter4.label_transfer = Properties.Settings.Default.fl4_label_tf1; } }) },
                { 69, () => BeginInvoke((MethodInvoker)delegate { if(GetOPCDataValue<bool>(69)){ uc_x_hmi.glgSetTag4.SetSRsc(uc_x_hmi.glgControl_hmi4, "transfer_to/String", Properties.Settings.Default.fl4_label_tf2); flow_meter4.label_transfer = Properties.Settings.Default.fl4_label_tf2; } }) },
                { 70, () => BeginInvoke((MethodInvoker)delegate { if(GetOPCDataValue<bool>(70)){ uc_x_hmi.glgSetTag4.SetSRsc(uc_x_hmi.glgControl_hmi4, "transfer_to/String", Properties.Settings.Default.fl4_label_tf3); flow_meter4.label_transfer = Properties.Settings.Default.fl4_label_tf3; } }) },
                { 71, () => BeginInvoke((MethodInvoker)delegate { if(GetOPCDataValue<bool>(71)){ uc_x_hmi.glgSetTag4.SetSRsc(uc_x_hmi.glgControl_hmi4, "proses_mesin/String", Properties.Settings.Default.fl4_label_pm1); flow_meter4.label_proses_mesin = Properties.Settings.Default.fl4_label_pm1; } }) },
                { 72, () => BeginInvoke((MethodInvoker)delegate { if(GetOPCDataValue<bool>(72)){ uc_x_hmi.glgSetTag4.SetSRsc(uc_x_hmi.glgControl_hmi4, "proses_mesin/String", Properties.Settings.Default.fl4_label_pm2); flow_meter4.label_proses_mesin = Properties.Settings.Default.fl4_label_pm2; } }) },
                { 73, () => BeginInvoke((MethodInvoker)delegate { uc_x_hmi.glgSetTag3.SetDRsc(uc_x_hmi.glgControl_hmi3, uc_x_hmi.glgSetTag3.TagMaps["valve_pipe"], GetOPCDataValue<bool>(73)); }) },
                { 74, () => BeginInvoke((MethodInvoker)delegate { uc_x_hmi.glgSetTag4.SetDRsc(uc_x_hmi.glgControl_hmi4, uc_x_hmi.glgSetTag4.TagMaps["valve_pipe"], GetOPCDataValue<bool>(74)); }) },
                { 75, () => BeginInvoke((MethodInvoker)delegate { uc_x_hmi.label_header_hmi4.Image = uc_x_hmi.label_header_hmi3.Image = GetOPCDataValue<bool>(75) ? Properties.Resources.icons8_connect : Properties.Resources.icons8_disconnect; }) },
                { 76, () => BeginInvoke((MethodInvoker)delegate { uc_x_hmi.glgSetTag3.SetDRsc(uc_x_hmi.glgControl_hmi3, "val_k-factor/Value", GetOPCDataValue<double>(76)); }) },
                { 77, () => BeginInvoke((MethodInvoker)delegate { uc_x_hmi.glgSetTag3.SetDRsc(uc_x_hmi.glgControl_hmi3, "val_liter/Value", GetOPCDataValue<double>(77)); }) },
                { 78, () => BeginInvoke((MethodInvoker)delegate { uc_x_hmi.glgSetTag3.SetDRsc(uc_x_hmi.glgControl_hmi3, "val_lpm/Value", GetOPCDataValue<double>(78)); }) },
                { 79, () => BeginInvoke((MethodInvoker)delegate { uc_x_hmi.glgSetTag4.SetDRsc(uc_x_hmi.glgControl_hmi4, "val_k-factor/Value", GetOPCDataValue<double>(79)); }) },
                { 80, () => BeginInvoke((MethodInvoker)delegate { uc_x_hmi.glgSetTag4.SetDRsc(uc_x_hmi.glgControl_hmi4, "val_liter/Value", GetOPCDataValue<double>(80)); }) },
                { 81, () => BeginInvoke((MethodInvoker)delegate { uc_x_hmi.glgSetTag4.SetDRsc(uc_x_hmi.glgControl_hmi4, "val_lpm/Value", GetOPCDataValue<double>(81)); }) },
                { 82, () => BeginInvoke((MethodInvoker)delegate { flow_meter3.label_batch = $"Batch {(GetOPCDataValue<int>(82) < 10 ? "0" : "")}{GetOPCDataValue<int>(82)}"; uc_x_hmi.glgSetTag3.SetSRsc(uc_x_hmi.glgControl_hmi3, "batch/String", flow_meter3.label_batch); }) },
                { 83, () => BeginInvoke((MethodInvoker)delegate { flow_meter4.label_batch = $"Batch {(GetOPCDataValue<int>(83) < 10 ? "0" : "")}{GetOPCDataValue<int>(83)}"; uc_x_hmi.glgSetTag4.SetSRsc(uc_x_hmi.glgControl_hmi4, "batch/String", flow_meter4.label_batch); }) }
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
        private void timer_handle_opc_tag3_Tick(object sender, EventArgs e)
        {
            Dictionary<int, Action> tagMapping = new Dictionary<int, Action> // Dictionary untuk menyimpan informasi terkait indeks dan fungsi delegate yang sesuai
            {
                { 84, () => BeginInvoke((MethodInvoker)delegate { uc_x_hmi.glgSetTag5.SetDRsc(uc_x_hmi.glgControl_hmi5, uc_x_hmi.glgSetTag5.TagMaps["sensor_flow"], GetOPCDataValue<bool>(84)); }) },
                { 85, () => BeginInvoke((MethodInvoker)delegate { uc_x_hmi.glgSetTag5.BtnGlgSet(uc_x_hmi.glgControl_hmi5, "set_on_off", GetOPCDataValue<bool>(85), "STOP PRODUKSI", "PRODUKSI", 0.945892, 0.0, 0.0, 0.0, 0.725475, 0.0); }) },
                { 86, () => BeginInvoke((MethodInvoker)delegate {
                    if(GetOPCDataValue<bool>(86)) {
                        if(!log_to_db2(flow_meter5.label_proses_mesin, flow_meter5.label_batch, flow_meter5.label_transfer, GetOPCDataValue<double>(95), GetOPCDataValue<double>(94)))
                            log_to_db2(flow_meter5.label_proses_mesin, flow_meter5.label_batch, flow_meter5.label_transfer, GetOPCDataValue<double>(95), GetOPCDataValue<double>(94));
                        OPCWriteAsync1(86, false);
                        OPCStatus1.IsLogData = true;
                    }
                }) },
                { 87, () => BeginInvoke((MethodInvoker)delegate { if(GetOPCDataValue<bool>(87)){ uc_x_hmi.glgSetTag5.SetSRsc(uc_x_hmi.glgControl_hmi5, "transfer_to/String", Properties.Settings.Default.fl5_label_tf1); flow_meter5.label_transfer = Properties.Settings.Default.fl5_label_tf1; } }) },
                { 88, () => BeginInvoke((MethodInvoker)delegate { if(GetOPCDataValue<bool>(88)){ uc_x_hmi.glgSetTag5.SetSRsc(uc_x_hmi.glgControl_hmi5, "transfer_to/String", Properties.Settings.Default.fl5_label_tf2); flow_meter5.label_transfer = Properties.Settings.Default.fl5_label_tf2; } }) },
                { 89, () => BeginInvoke((MethodInvoker)delegate { if(GetOPCDataValue<bool>(89)){ uc_x_hmi.glgSetTag5.SetSRsc(uc_x_hmi.glgControl_hmi5, "transfer_to/String", Properties.Settings.Default.fl5_label_tf3); flow_meter5.label_transfer = Properties.Settings.Default.fl5_label_tf3; } }) },
                { 90, () => BeginInvoke((MethodInvoker)delegate { if(GetOPCDataValue<bool>(90)){ uc_x_hmi.glgSetTag5.SetSRsc(uc_x_hmi.glgControl_hmi5, "proses_mesin/String", Properties.Settings.Default.fl5_label_pm1); flow_meter5.label_proses_mesin = Properties.Settings.Default.fl5_label_pm1; } }) },
                { 91, () => BeginInvoke((MethodInvoker)delegate { if(GetOPCDataValue<bool>(91)){ uc_x_hmi.glgSetTag5.SetSRsc(uc_x_hmi.glgControl_hmi5, "proses_mesin/String", Properties.Settings.Default.fl5_label_pm2); flow_meter5.label_proses_mesin = Properties.Settings.Default.fl5_label_pm2; } }) },
                { 92, () => BeginInvoke((MethodInvoker)delegate { uc_x_hmi.glgSetTag5.SetDRsc(uc_x_hmi.glgControl_hmi5, uc_x_hmi.glgSetTag5.TagMaps["valve_pipe"], GetOPCDataValue<bool>(92)); }) },
                { 93, () => BeginInvoke((MethodInvoker)delegate { uc_x_hmi.label_header_hmi5.Image = GetOPCDataValue<bool>(93) ? Properties.Resources.icons8_connect : Properties.Resources.icons8_disconnect; }) },
                { 94, () => BeginInvoke((MethodInvoker)delegate { uc_x_hmi.glgSetTag5.SetDRsc(uc_x_hmi.glgControl_hmi5, "val_k-factor/Value", GetOPCDataValue<double>(94)); }) },
                { 95, () => BeginInvoke((MethodInvoker)delegate { uc_x_hmi.glgSetTag5.SetDRsc(uc_x_hmi.glgControl_hmi5, "val_liter/Value", GetOPCDataValue<double>(95)); }) },
                { 96, () => BeginInvoke((MethodInvoker)delegate { uc_x_hmi.glgSetTag5.SetDRsc(uc_x_hmi.glgControl_hmi5, "val_lpm/Value", GetOPCDataValue<double>(96)); }) },
                { 97, () => BeginInvoke((MethodInvoker)delegate { flow_meter5.label_batch = $"Batch {(GetOPCDataValue<int>(97) < 10 ? "0" : "")}{GetOPCDataValue<int>(97)}"; uc_x_hmi.glgSetTag5.SetSRsc(uc_x_hmi.glgControl_hmi5, "batch/String", flow_meter5.label_batch); }) }
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
        private bool log_to_db(string _flow_meter, string _mode, double _set_liter, double _liter, double _k_factor, string _batch, string _transfer_to)
        {
            var db = DbDataAccess.Db();
            if (db == null) { ShowErrorMessage("Koneksi database gagal. Periksa pengaturan koneksi atau hubungi administrator"); return false; }
            var log_fl_data = new { flow_meter = _flow_meter, mode = _mode, set_liter = Math.Round(_set_liter, 2), liter = Math.Round(_liter, 2), k_factor = Math.Round(_k_factor, 3), batch = _batch, transfer_to = _transfer_to, date_time = DateTime.Now, };
            Query query = db.Query(Properties.Settings.Default.tabel_db_flowmeter_1_2);
            try
            {
                int affected = query.Insert(log_fl_data);
                var lastInsertIdQuery = db.Query(Properties.Settings.Default.tabel_db_flowmeter_1_2).OrderByDesc("id").Limit(1);
                var lastInsertIdResult = lastInsertIdQuery.First();
                //add new tabel view
            }
            catch (Exception ex) { ShowErrorMessage(ex is NullReferenceException ? $"Insert DB Terjadi kesalahan null reference: {ex.Message}" : $"Insert DB Terjadi kesalahan: {ex.Message}"); return false; }

            return true;
        }
        private bool log_to_db2(string _proses_mesin, string _batch, string _transfer_to, double _liter, double _k_factor)
        {
            var db = DbDataAccess.Db();
            if (db == null) { ShowErrorMessage("Koneksi database gagal. Periksa pengaturan koneksi atau hubungi administrator"); return false; }
            var log_fl_data = new { proses_mesin = _proses_mesin, batch = _batch, transfer_to = _transfer_to, liter = Math.Round(_liter, 2), k_factor = Math.Round(_k_factor, 3), date_time = DateTime.Now, };
            Query query = db.Query(Properties.Settings.Default.tabel_db_flowmeter_3_4_5);
            try
            {
                int affected = query.Insert(log_fl_data);
                var lastInsertIdQuery = db.Query(Properties.Settings.Default.tabel_db_flowmeter_3_4_5).OrderByDesc("id").Limit(1);
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
        public void ShowMessage(string message, string caption, MessageBoxIcon icon) { if (!IsHandleCreated) return; BeginInvoke((MethodInvoker)delegate { MessageBox.Show(this, message, caption, MessageBoxButtons.OK, icon); }); }
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
                    string connection_str = $@"Server=localhost;Database={Properties.Settings.Default.DatabaseName};Uid=root;";
                    connection = new MySqlConnection(connection_str);
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
