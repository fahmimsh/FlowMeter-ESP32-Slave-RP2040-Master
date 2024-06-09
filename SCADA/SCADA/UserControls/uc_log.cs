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
        internal BindingList<data_log_entry2> data_log_db2 = new BindingList<data_log_entry2>();
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
        private void check_b_all_CheckedChanged(object sender, EventArgs e)
        {
            if (!(check_b_all.Checked)) return;
            cb_flow_meter.Items.Clear(); cb_flow_meter.Text = string.Empty;
            cb_transfer_to_fl12.Items.Clear(); cb_transfer_to_fl12.Text = string.Empty;
            cb_batch_fl_1_2.Items.Clear(); cb_batch_fl_1_2.Text = string.Empty;
            cb_proses_mesin.Items.Clear(); cb_proses_mesin.Text = string.Empty;
            cb_transfer_to_fl345.Items.Clear(); cb_transfer_to_fl345.Text = string.Empty;
            cb_batch_fl345.Items.Clear(); cb_batch_fl_1_2.Text = string.Empty;
        }
        private void btn_search_Click(object sender, EventArgs e)
        {
            db_search_fl_1_2();
            db_search_fl_3_4_5();
        }
        private void db_search_fl_1_2()
        {
            var db = DbDataAccess.Db();
            if (db == null) { form.ShowErrorMessage("Koneksi database gagal. Periksa pengaturan koneksi atau hubungi administrator"); return; }
            Query query = db.Query(Properties.Settings.Default.tabel_db_flowmeter_1_2);
            if (query == null) { form.ShowErrorMessage("Objek query null. Cek pengaturan koneksi atau hubungi administrator"); return; }
            if (date_start.Value != DateTime.MinValue && date_stop.Value != DateTime.MinValue)
                query = query.WhereBetween("date_time", date_start.Value.Date, date_stop.Value.Date.AddHours(23).AddMinutes(59).AddSeconds(59));
            if (!check_b_all.Checked)
            {
                if (cb_flow_meter.SelectedItem != null || !string.IsNullOrEmpty(cb_flow_meter.Text)) query = query.WhereContains("flow_meter", cb_flow_meter.SelectedItem.ToString());
                if (cb_transfer_to_fl12.SelectedItem != null || !string.IsNullOrEmpty(cb_transfer_to_fl12.Text)) query = query.WhereContains("transfer_to", cb_transfer_to_fl12.SelectedItem.ToString());
                if (cb_batch_fl_1_2.SelectedItem != null || !string.IsNullOrEmpty(cb_batch_fl_1_2.Text)) query = query.WhereContains("batch", cb_batch_fl_1_2.SelectedItem.ToString());
            }
            try
            {
                IEnumerable<data_log_entry> result = query.Get<data_log_entry>();
                if (result != null)
                {
                    result = result.OrderByDescending(entry => entry.id).ToList();
                    data_log_db = new BindingList<data_log_entry>((IList<data_log_entry>)result);
                    dataGridViewDataLog_fl12.DataSource = data_log_db;
                    form.uc_x_hmi.dataGridViewDataLog_hmi_fl1_fl2.DataSource = data_log_db;
                    label_total_liter_fl12.Text = $"Total Liter : {data_log_db.Sum(entry => entry.liter)} L";
                }
            }
            catch (Exception ex)
            {
                form.ShowErrorMessage("Terjadi kesalahan: " + ex.Message);
            }
        }
        private void db_search_fl_3_4_5()
        {
            var db = DbDataAccess.Db();
            if (db == null) { form.ShowErrorMessage("Koneksi database gagal. Periksa pengaturan koneksi atau hubungi administrator"); return; }
            Query query = db.Query(Properties.Settings.Default.tabel_db_flowmeter_3_4_5);
            if (query == null) { form.ShowErrorMessage("Objek query null. Cek pengaturan koneksi atau hubungi administrator"); return; }
            if (date_start.Value != DateTime.MinValue && date_stop.Value != DateTime.MinValue)
                query = query.WhereBetween("date_time", date_start.Value.Date, date_stop.Value.Date.AddHours(23).AddMinutes(59).AddSeconds(59));
            if (!check_b_all.Checked)
            {
                if (cb_proses_mesin.SelectedItem != null || !string.IsNullOrEmpty(cb_proses_mesin.Text)) query = query.WhereContains("proses_mesin", cb_proses_mesin.SelectedItem.ToString());
                if (cb_transfer_to_fl345.SelectedItem != null || !string.IsNullOrEmpty(cb_transfer_to_fl345.Text)) query = query.WhereContains("transfer_to", cb_transfer_to_fl345.SelectedItem.ToString());
                if (cb_batch_fl345.SelectedItem != null || !string.IsNullOrEmpty(cb_batch_fl345.Text)) query = query.WhereContains("batch", cb_batch_fl345.SelectedItem.ToString());
            }
            try
            {
                IEnumerable<data_log_entry2> result = query.Get<data_log_entry2>();
                if (result != null)
                {
                    result = result.OrderByDescending(entry => entry.id).ToList();
                    data_log_db2 = new BindingList<data_log_entry2>((IList<data_log_entry2>)result);
                    dataGridViewDataLog_fl345.DataSource = data_log_db2;
                    form.uc_x_hmi.dataGridViewDataLog_hmi_fl3_fl4_fl5.DataSource = data_log_db2;
                    label_total_liter_fl345.Text = $"Total Liter : {data_log_db2.Sum(entry => entry.liter)} L";
                }
            }
            catch (Exception ex)
            {
                form.ShowErrorMessage("Terjadi kesalahan: " + ex.Message);
            }
        }
        private void btn_export_Click(object sender, EventArgs e)
        {
            /*            if (!data_log_db.Any()) { form.ShowErrorMessage("Data tidak ditemukan."); return; }
                        DataTable dataTable = ConvertToDataTable(data_log_db);
                        ExportToExcel(dataTable);
                        if (!data_log_db2.Any()) { form.ShowErrorMessage("Data tidak ditemukan."); return; }
                        DataTable dataTable2 = ConvertToDataTable2(data_log_db2);
                        ExportToExcel2(dataTable2);*/

            if (!data_log_db.Any() && !data_log_db2.Any()) { form.ShowErrorMessage("Data tidak ditemukan."); return; }

            DataTable dataTable1 = ConvertToDataTable(data_log_db);
            DataTable dataTable2 = ConvertToDataTable(data_log_db2);

            ExportToExcel(dataTable1, dataTable2);
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

        private DataTable ConvertToDataTable(BindingList<data_log_entry2> dataList)
        {
            DataTable dataTable = new DataTable();
            Dictionary<string, string> columnMappings = new Dictionary<string, string>();

            foreach (var prop in typeof(data_log_entry2).GetProperties())
            {
                var headerAttribute = prop.GetCustomAttribute<HeaderColumnAttribute>();
                string columnName = headerAttribute != null ? headerAttribute.HeaderText : prop.Name;
                columnMappings.Add(prop.Name, columnName);
                dataTable.Columns.Add(columnName, prop.PropertyType);
            }
            foreach (var data in dataList)
            {
                DataRow row = dataTable.NewRow();
                foreach (var prop in typeof(data_log_entry2).GetProperties())
                {
                    string columnName = columnMappings[prop.Name];
                    row[columnName] = prop.GetValue(data);
                }
                dataTable.Rows.Add(row);
            }
            return dataTable;
        }
        private void ExportToExcel(DataTable dataTable1, DataTable dataTable2)
        {
            using (SaveFileDialog sfd = new SaveFileDialog() { Filter = "Excel Workbook|*.xlsx" })
            {
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        using (XLWorkbook workbook = new XLWorkbook())
                        {
                            var worksheet = workbook.Worksheets.Add("logdata");

                            // Adding and merging the header for the first table
                            var header1 = worksheet.Range(1, 1, 1, 9).Merge();
                            header1.Value = $"Log Data hasil {Properties.Settings.Default.fl1_header} dan {Properties.Settings.Default.fl2_header}";
                            header1.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                            header1.Style.Fill.BackgroundColor = XLColor.Yellow;

                            // Inserting the first table
                            var table1 = worksheet.Cell(2, 1).InsertTable(dataTable1);

                            // Align all data rows in the first table to center
                            foreach (var cell in worksheet.Range(3, 1, 2 + dataTable1.Rows.Count, dataTable1.Columns.Count).Cells())
                            {
                                cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                            }

                            int rowCount1 = dataTable1.Rows.Count + 3;

                            // Adding and merging the total row for the first table
                            var total1 = worksheet.Range(rowCount1, 1, rowCount1, 4).Merge();
                            total1.Value = "Total";
                            total1.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                            worksheet.Cell(rowCount1, 5).Value = data_log_db.Sum(entry => entry.liter);
                            worksheet.Cell(rowCount1, 5).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                            worksheet.Cell(rowCount1, 6).Value = "Liter";

                            // Adding and merging the header for the second table
                            int startRow = 1;
                            var header2 = worksheet.Range(startRow, 10, startRow, 17).Merge();
                            header2.Value = $"Log Data hasil {Properties.Settings.Default.fl3_header}, {Properties.Settings.Default.fl4_header} dan {Properties.Settings.Default.fl5_header}";
                            header2.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                            header2.Style.Fill.BackgroundColor = XLColor.Green;

                            // Inserting the second table
                            var table2 = worksheet.Cell(startRow + 1, 10).InsertTable(dataTable2);

                            // Align all data rows in the second table to center
                            foreach (var cell in worksheet.Range(startRow + 2, 10, startRow + 1 + dataTable2.Rows.Count, 10 + dataTable2.Columns.Count - 1).Cells())
                            {
                                cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                            }

                            int rowCount2 = startRow + dataTable2.Rows.Count + 2;

                            // Adding and merging the total row for the second table
                            var total2 = worksheet.Range(rowCount2, 10, rowCount2, 14).Merge();
                            total2.Value = "Total";
                            total2.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                            worksheet.Cell(rowCount2, 15).Value = data_log_db2.Sum(entry => entry.liter);
                            worksheet.Cell(rowCount2, 15).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                            worksheet.Cell(rowCount2, 16).Value = "Liter";

                            worksheet.Columns().AdjustToContents();

                            workbook.SaveAs(sfd.FileName);
                            MessageBox.Show("Data berhasil diekspor ke excel", "INFORMASI", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(("Ekspor data ke Excel gagal: ") + ex.Message, "EXPORT DATA", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }


        /*        private void ExportToExcel(DataTable dataTable1, DataTable dataTable2)
                {
                    using (SaveFileDialog sfd = new SaveFileDialog() { Filter = "Excel Workbook|*.xlsx" })
                    {
                        if (sfd.ShowDialog() == DialogResult.OK)
                        {
                            try
                            {
                                using (XLWorkbook workbook = new XLWorkbook())
                                {
                                    var worksheet = workbook.Worksheets.Add("logdata");

                                    // Adding and merging the header for the first table
                                    var header1 = worksheet.Range(1, 1, 1, 9).Merge();
                                    header1.Value = $"Log Data hasil {Properties.Settings.Default.fl1_header} dan {Properties.Settings.Default.fl2_header}";
                                    header1.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                                    header1.Style.Fill.BackgroundColor = XLColor.Yellow;

                                    // Inserting the first table
                                    var table1 = worksheet.Cell(2, 1).InsertTable(dataTable1);
                                    table1.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                                    int rowCount1 = dataTable1.Rows.Count + 3;

                                    // Adding and merging the total row for the first table
                                    var total1 = worksheet.Range(rowCount1, 1, rowCount1, 4).Merge();
                                    total1.Value = "Total";
                                    total1.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                                    worksheet.Cell(rowCount1, 5).Value = data_log_db.Sum(entry => entry.liter);
                                    worksheet.Cell(rowCount1, 5).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                                    worksheet.Cell(rowCount1, 6).Value = "Liter";

                                    // Adding and merging the header for the second table
                                    int startRow = 1;
                                    var header2 = worksheet.Range(startRow, 10, startRow, 16).Merge();
                                    header2.Value = $"Log Data hasil {Properties.Settings.Default.fl3_header}, {Properties.Settings.Default.fl4_header} dan {Properties.Settings.Default.fl5_header}";
                                    header2.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                                    header2.Style.Fill.BackgroundColor = XLColor.Green;

                                    // Inserting the second table
                                    var table2 = worksheet.Cell(startRow + 1, 10).InsertTable(dataTable2);
                                    table2.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                                    int rowCount2 = startRow + dataTable2.Rows.Count + 2;

                                    // Adding and merging the total row for the second table
                                    var total2 = worksheet.Range(rowCount2, 10, rowCount2, 14).Merge();
                                    total2.Value = "Total";
                                    total2.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                                    worksheet.Cell(rowCount2, 15).Value = data_log_db2.Sum(entry => entry.liter);
                                    worksheet.Cell(rowCount2, 15).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                                    worksheet.Cell(rowCount2, 16).Value = "Liter";

                                    worksheet.Columns().AdjustToContents();

                                    workbook.SaveAs(sfd.FileName);
                                    MessageBox.Show("Data berhasil diekspor ke excel", "INFORMASI", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                }
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show(("Ekspor data ke Excel gagal: ") + ex.Message, "EXPORT DATA", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                    }
                }*/

        /*        private void ExportToExcel(DataTable dataTable1, DataTable dataTable2)
                {
                    using (SaveFileDialog sfd = new SaveFileDialog() { Filter = "Excel Workbook|*.xlsx" })
                    {
                        if (sfd.ShowDialog() == DialogResult.OK)
                        {
                            try
                            {
                                using (XLWorkbook workbook = new XLWorkbook())
                                {
                                    var worksheet = workbook.Worksheets.Add("logdata");

                                    // Adding and merging the header for the first table
                                    var header1 = worksheet.Range(1, 1, 1, 9).Merge();
                                    header1.Value = "Log Data hasil Flow Meter 1 dan Flow Meter 2";
                                    header1.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                                    header1.Style.Fill.BackgroundColor = XLColor.Yellow;

                                    // Inserting the first table
                                    worksheet.Cell(2, 1).InsertTable(dataTable1);
                                    int rowCount1 = dataTable1.Rows.Count + 3;

                                    // Adding and merging the total row for the first table
                                    var total1 = worksheet.Range(rowCount1, 1, rowCount1, 4).Merge();
                                    total1.Value = "Total";
                                    total1.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                                    worksheet.Cell(rowCount1, 5).Value = data_log_db.Sum(entry => entry.liter);
                                    worksheet.Cell(rowCount1, 6).Value = "Liter";

                                    // Adding and merging the header for the second table
                                    int startRow = 1;
                                    var header2 = worksheet.Range(startRow, 10, startRow, 16).Merge();
                                    header2.Value = "Log Data hasil Flow Meter 3, Flow Meter 4 dan Flow Meter 2";
                                    header2.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                                    header2.Style.Fill.BackgroundColor = XLColor.Green;

                                    // Inserting the second table
                                    worksheet.Cell(startRow + 1, 10).InsertTable(dataTable2);
                                    int rowCount2 = startRow + dataTable2.Rows.Count + 2;

                                    // Adding and merging the total row for the second table
                                    var total2 = worksheet.Range(rowCount2, 10, rowCount2, 14).Merge();
                                    total2.Value = "Total";
                                    total2.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                                    worksheet.Cell(rowCount2, 15).Value = data_log_db2.Sum(entry => entry.liter);
                                    worksheet.Cell(rowCount2, 16).Value = "Liter";

                                    worksheet.Columns().AdjustToContents();

                                    workbook.SaveAs(sfd.FileName);
                                    MessageBox.Show("Data berhasil diekspor ke excel", "INFORMASI", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                }
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show(("Ekspor data ke Excel gagal: ") + ex.Message, "EXPORT DATA", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                    }
                }*/

        /*        private DataTable ConvertToDataTable(BindingList<data_log_entry> dataList)
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
                private DataTable ConvertToDataTable2(BindingList<data_log_entry2> dataList)
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
                private void ExportToExcel2(DataTable dataTable)
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
                                    worksheet.Cell(i, 5).Value = data_log_db2.Sum(entry => entry.liter);
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
                }*/
        private void timer_refresh_db_Tick(object sender, EventArgs e)
        {
            if(OPCStatus1.IsLogData)
            {
                OPCStatus1.IsLogData = false;
                btn_search_Click(null, null);
            }
        }
        private void cb_flow_meter_DropDown(object sender, EventArgs e)
        {
            if (check_b_all.Checked) return;
            cb_flow_meter.Items.Clear(); cb_flow_meter.Text = string.Empty;
            string[] item_Fl_header = { string.Empty, Properties.Settings.Default.fl1_header, Properties.Settings.Default.fl2_header };
            cb_flow_meter.Items.AddRange(item_Fl_header);
        }

        private void cb_transfer_to_DropDown(object sender, EventArgs e)
        {
            if (check_b_all.Checked) return;
            cb_transfer_to_fl12.Items.Clear(); cb_transfer_to_fl12.Text = string.Empty;
            string[] item_transfer_to =
                {string.Empty,
                    Properties.Settings.Default.fl1_label_tf1, Properties.Settings.Default.fl1_label_tf2, Properties.Settings.Default.fl1_label_tf3, Properties.Settings.Default.fl1_label_tf4,
                    Properties.Settings.Default.fl2_label_tf1, Properties.Settings.Default.fl2_label_tf2, Properties.Settings.Default.fl2_label_tf3, Properties.Settings.Default.fl2_label_tf4,
                };
            cb_transfer_to_fl12.Items.AddRange(item_transfer_to);
        }

        private void cb_batch_fl_1_2_DropDown(object sender, EventArgs e)
        {
            if (check_b_all.Checked) return;
            cb_batch_fl_1_2.Items.Clear(); cb_batch_fl_1_2.Text = string.Empty;
            string[] item_form_source =
                {string.Empty,
                    Properties.Settings.Default.fl1_label_batch1, Properties.Settings.Default.fl1_label_batch2, Properties.Settings.Default.fl1_label_batch3, Properties.Settings.Default.fl1_label_batch4,
                    Properties.Settings.Default.fl1_label_batch5, Properties.Settings.Default.fl1_label_batch6, Properties.Settings.Default.fl1_label_batch7, Properties.Settings.Default.fl1_label_batch8,
                    Properties.Settings.Default.fl1_label_batch9, Properties.Settings.Default.fl1_label_batch10, Properties.Settings.Default.fl2_label_batch1, Properties.Settings.Default.fl2_label_batch2,
                    Properties.Settings.Default.fl2_label_batch3, Properties.Settings.Default.fl2_label_batch4, Properties.Settings.Default.fl2_label_batch5, Properties.Settings.Default.fl2_label_batch6,
                    Properties.Settings.Default.fl2_label_batch7, Properties.Settings.Default.fl2_label_batch8, Properties.Settings.Default.fl2_label_batch9, Properties.Settings.Default.fl2_label_batch10
                };
            cb_batch_fl_1_2.Items.AddRange(item_form_source);
        }

        private void cb_proses_mesin_DropDown(object sender, EventArgs e)
        {
            if (check_b_all.Checked) return;
            cb_proses_mesin.Items.Clear(); cb_proses_mesin.Text = string.Empty;
            string[] item_form_proses_mesin = {string.Empty, Properties.Settings.Default.fl3_label_pm1, Properties.Settings.Default.fl3_label_pm2, Properties.Settings.Default.fl4_label_pm1, Properties.Settings.Default.fl4_label_pm2, Properties.Settings.Default.fl5_label_pm1, Properties.Settings.Default.fl5_label_pm2 };
            cb_proses_mesin.Items.AddRange(item_form_proses_mesin);
        }

        private void cb_transfer_to_fl345_DropDown(object sender, EventArgs e)
        {
            if (check_b_all.Checked) return;
            cb_transfer_to_fl345.Items.Clear(); cb_transfer_to_fl345.Text = string.Empty;
            string[] item_form_transfer_to = { string.Empty, 
                Properties.Settings.Default.fl3_label_tf1, Properties.Settings.Default.fl3_label_tf2, Properties.Settings.Default.fl3_label_tf3,
                Properties.Settings.Default.fl4_label_tf1, Properties.Settings.Default.fl4_label_tf2, Properties.Settings.Default.fl4_label_tf3,
                Properties.Settings.Default.fl5_label_tf1, Properties.Settings.Default.fl5_label_tf2, Properties.Settings.Default.fl5_label_tf3
            };
            cb_transfer_to_fl345.Items.AddRange(item_form_transfer_to);
        }

        private void cb_batch_fl345_DropDown(object sender, EventArgs e)
        {
            if (check_b_all.Checked) return;
            cb_batch_fl345.Items.Clear(); cb_batch_fl_1_2.Text = string.Empty;
            string[] item_form_source =
                {string.Empty,
                    Properties.Settings.Default.fl1_label_batch1, Properties.Settings.Default.fl1_label_batch2, Properties.Settings.Default.fl1_label_batch3, Properties.Settings.Default.fl1_label_batch4,
                    Properties.Settings.Default.fl1_label_batch5, Properties.Settings.Default.fl1_label_batch6, Properties.Settings.Default.fl1_label_batch7, Properties.Settings.Default.fl1_label_batch8,
                    Properties.Settings.Default.fl1_label_batch9, Properties.Settings.Default.fl1_label_batch10
                };
            cb_batch_fl345.Items.AddRange(item_form_source);
        }
    }
}
