using SqlKata;
using SqlKata.Execution;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing; 
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using ClosedXML.Excel;
using System.Reflection;
using DocumentFormat.OpenXml.Math;
using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;

namespace SCADA.UserControls
{
    public partial class uc_log : UserControl
    {
        private form_main form;
        internal BindingList<data_log_entry> data_log_db = new BindingList<data_log_entry>();
        public uc_log(form_main mainForm)
        {
            InitializeComponent();
            form = mainForm;
        }
        private async void uc_log_Load(object sender, EventArgs e)
        {
            date_stop.Value = DateTime.Now;
            date_start.Value = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 0, 0, 0);
            await Task.Delay(5000);
            btn_search_Click(null, null);
        }
        private void add_item_list()
        {
            string[] item_Fl_header = { string.Empty, Properties.Settings.Default.fl1_label_tf5, Properties.Settings.Default.fl2_header };
            cb_flow_meter.Items.AddRange(item_Fl_header);
            string[] item_mode = { string.Empty, "Auto", "Manual" };
            cb_mode.Items.AddRange(item_mode);
            string[] item_transfer_to =
            {string.Empty,
                    Properties.Settings.Default.fl1_label_tf1, Properties.Settings.Default.fl1_label_tf2, Properties.Settings.Default.fl1_label_tf3, Properties.Settings.Default.fl1_label_tf4,
                    Properties.Settings.Default.fl2_label_tf1, Properties.Settings.Default.fl2_label_tf2, Properties.Settings.Default.fl2_label_tf3, Properties.Settings.Default.fl2_label_tf4,
                };
            cb_transfer_to.Items.AddRange(item_transfer_to);
            string[] item_form_source =
            {string.Empty,
                    Properties.Settings.Default.fl1_label_batch1, Properties.Settings.Default.fl1_label_batch2, Properties.Settings.Default.fl1_label_batch3, Properties.Settings.Default.fl1_label_batch4,
                    Properties.Settings.Default.fl1_label_batch5, Properties.Settings.Default.fl1_label_batch6, Properties.Settings.Default.fl1_label_batch7, Properties.Settings.Default.fl1_label_batch8,
                    Properties.Settings.Default.fl1_label_batch9, Properties.Settings.Default.fl1_label_batch10, Properties.Settings.Default.fl2_label_batch1, Properties.Settings.Default.fl2_label_batch2,
                    Properties.Settings.Default.fl2_label_batch3, Properties.Settings.Default.fl2_label_batch4, Properties.Settings.Default.fl2_label_batch5, Properties.Settings.Default.fl2_label_batch6,
                    Properties.Settings.Default.fl2_label_batch7, Properties.Settings.Default.fl2_label_batch8, Properties.Settings.Default.fl2_label_batch9, Properties.Settings.Default.fl2_label_batch10
                };
            cb_from_source.Items.AddRange(item_form_source);
        }
        private void check_b_all_CheckedChanged(object sender, EventArgs e)
        {
            if (check_b_all.Checked)
            {
                cb_flow_meter.Items.Clear(); cb_flow_meter.Text = string.Empty;
                cb_mode.Items.Clear(); cb_mode.Text = string.Empty;
                cb_transfer_to.Items.Clear(); cb_transfer_to.Text = string.Empty;
                cb_from_source.Items.Clear(); cb_from_source.Text = string.Empty;
                return;
            }
            add_item_list();
        }
        private void btn_search_Click(object sender, EventArgs e)
        {
            var db = DbDataAccess.Db();
            if (db == null) { form.ShowErrorMessage("Koneksi database gagal. Periksa pengaturan koneksi atau hubungi administrator"); return; }
            Query query = db.Query(Properties.Settings.Default.tabel_db_flowmeter_1_2);
            //Query query = db.Query("log_data");
            if (query == null) { form.ShowErrorMessage("Objek query null. Cek pengaturan koneksi atau hubungi administrator"); return; }
            if (date_start.Value != DateTime.MinValue && date_stop.Value != DateTime.MinValue)
                query = query.WhereBetween("date_time", date_start.Value.Date, date_stop.Value.Date.AddHours(23).AddMinutes(59).AddSeconds(59));
            if (!check_b_all.Checked)
            {
                if (cb_flow_meter.SelectedItem != null || !string.IsNullOrEmpty(cb_flow_meter.Text)) query = query.WhereContains("flow_meter", cb_flow_meter.SelectedItem.ToString());
                if (cb_mode.SelectedItem != null || !string.IsNullOrEmpty(cb_mode.Text)) query = query.WhereContains("mode", cb_mode.SelectedItem.ToString());
                if (cb_transfer_to.SelectedItem != null || !string.IsNullOrEmpty(cb_transfer_to.Text)) query = query.WhereContains("transfer_to", cb_transfer_to.SelectedItem.ToString());
                if (cb_from_source.SelectedItem != null || !string.IsNullOrEmpty(cb_from_source.Text)) query = query.WhereContains("batch", cb_from_source.SelectedItem.ToString());
            }
            try
            {
                IEnumerable<data_log_entry> result = query.Get<data_log_entry>();
                if (result != null)
                {
                    result = result.OrderByDescending(entry => entry.id).ToList();
                    data_log_db = new BindingList<data_log_entry>((IList<data_log_entry>)result);
                    dataGridViewDataLog.DataSource = data_log_db;
                    form.uc_x_hmi.dataGridViewDataLog_hmi_fl1_fl2.DataSource = data_log_db;
                    label_total_liter.Text = $"Total Liter : {data_log_db.Sum(entry => entry.liter)} L";
                }
            }
            catch (Exception ex)
            {
                form.ShowErrorMessage("Terjadi kesalahan: " + ex.Message);
            }
        }
        private void btn_export_Click(object sender, EventArgs e)
        {
            if (!data_log_db.Any()) { form.ShowErrorMessage("Data tidak ditemukan."); return; }
            DataTable dataTable = ConvertToDataTable(data_log_db);
            ExportToExcel(dataTable);
        }
        private DataTable ConvertToDataTable(BindingList<data_log_entry> dataList)
        {
            DataTable dataTable = new DataTable();
            Dictionary<string, string> columnMappings = new Dictionary<string, string>();

            foreach (var prop in typeof(data_log_entry).GetProperties())
            {
                var headerAttribute = prop.GetCustomAttribute<HeaderColumnAttribute>();
                string columnName = headerAttribute != null ? headerAttribute.HeaderText : prop.Name;
                columnMappings.Add(prop.Name, columnName);
                dataTable.Columns.Add(columnName, prop.PropertyType);
            }
            foreach (var data in dataList)
            {
                DataRow row = dataTable.NewRow();
                foreach (var prop in typeof(data_log_entry).GetProperties())
                {
                    string columnName = columnMappings[prop.Name];
                    row[columnName] = prop.GetValue(data);
                }
                dataTable.Rows.Add(row);
            }
            return dataTable;
        }
        private void ExportToExcel(DataTable dataTable)
        {
            using (SaveFileDialog sfd = new SaveFileDialog() { Filter = "Excel Workbook|*.xlsx" })
            {
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        using (XLWorkbook workbook = new XLWorkbook())
                        {
                            var worksheet = workbook.Worksheets.Add(dataTable, "logdata");
                            int i = dataTable.Rows.Count + 2;
                            var label_total = worksheet.Range(i, 1, i, 4).Merge();
                            label_total.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                            worksheet.Cell(i, 1).Value = "Total";
                            worksheet.Cell(i, 5).Value = data_log_db.Sum(entry => entry.liter);
                            worksheet.Cell(i, 6).Value = "Liter";
                            worksheet.Columns().AdjustToContents();
                            workbook.SaveAs(sfd.FileName);
                            MessageBox.Show("Data berhasil diekspor ke excel", "INFORMASI", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(("exspor data excel :") + ex.Message.ToString(), "EXPORT DATA", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }
        private void timer_refresh_db_Tick(object sender, EventArgs e)
        {
            if(OPCStatus1.IsLogData)
            {
                OPCStatus1.IsLogData = false;
                btn_search_Click(null, null);
            }
        }
    }
}
