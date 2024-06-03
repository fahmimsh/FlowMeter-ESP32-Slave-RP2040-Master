using System;
using System.Drawing;
using System.Windows.Forms;

namespace SCADA
{
    internal static class Prompt
    {
        public static string PasswordSetting(bool IssetOrAct, string passwordIn, out bool PromptCancle)
        {
            Form prompt = new Form()
            {
                Name = "PasswordSetting",
                Width = 250,
                Height = 90,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                Text = IssetOrAct ? "New Password" : "Password",
                StartPosition = FormStartPosition.CenterScreen,
                BackColor = Color.FromArgb(213, 204, 204),
                ForeColor = Color.White,
                MinimizeBox = false,
                MaximizeBox = false,
            };
            TextBox textBox = new TextBox()
            {
                Left = 10,
                Top = 10,
                Width = 150,
                BackColor = Color.FromArgb(255, 255, 254),
                ForeColor = Color.Black,
                Text = IssetOrAct ? passwordIn : "",
                UseSystemPasswordChar = !IssetOrAct
            };
            Button confirmation = new Button()
            {
                Text = IssetOrAct ? "Set" : "OK",
                Left = 170,
                Width = 50,
                Top = 10,
                DialogResult = DialogResult.OK,
                BackColor = Color.FromArgb(117, 147, 247),
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.White
            };

            confirmation.FlatAppearance.BorderSize = 0;
            confirmation.FlatAppearance.MouseOverBackColor = Color.FromArgb(0, 104, 218);

            confirmation.Click += (sender, e) => { prompt.Close(); };
            prompt.Controls.Add(textBox);
            prompt.Controls.Add(confirmation);
            prompt.AcceptButton = confirmation;
            if (prompt.ShowDialog() == DialogResult.OK)
            {
                PromptCancle = false;
                return textBox.Text.Trim();
            }
            else
            {
                PromptCancle = true;
                return IssetOrAct ? passwordIn : "";
            }
        }
        public static double set_value_(string Header, string unit, double value, out bool PromptCancle)
        {
            Form prompt = new Form()
            {
                Name = "set_value_",
                Width = 100,
                Height = 100,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                Text = Header,
                StartPosition = FormStartPosition.CenterScreen,
                BackColor = Color.FromArgb(213, 204, 204),
                ForeColor = Color.White,
                MinimizeBox = false,
                MaximizeBox = false,
            };
            Label label_unit = new Label()
            {
                Left = 80,
                Top = 7,
                Width = 50,
                Text = unit,
                ForeColor = Color.Black,
            };
            NumericUpDown numeric = new NumericUpDown()
            {
                Left = 5,
                Top = 5,
                Width = 70,
                BackColor = Color.FromArgb(255, 255, 254),
                ForeColor = Color.Black,
                Maximum = 100000,
                DecimalPlaces = 4,
                Value = (decimal) value,
            };
            Button confirmation = new Button()
            {
                Text = "Set",
                Left = 30,
                Width = 50,
                Top = 30,
                DialogResult = DialogResult.OK,
                BackColor = Color.FromArgb(117, 147, 247),
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.White
            };

            confirmation.FlatAppearance.BorderSize = 0;
            confirmation.FlatAppearance.MouseOverBackColor = Color.FromArgb(0, 104, 218);

            confirmation.Click += (sender, e) => { prompt.Close(); };
            prompt.Controls.Add(numeric);
            prompt.Controls.Add(label_unit);
            prompt.Controls.Add(confirmation);
            prompt.AcceptButton = confirmation;
            PromptCancle = (prompt.ShowDialog() == DialogResult.OK);
            return (double)numeric.Value;
        }
        public static int set_label_(string Header, string[] list_option, out bool PromptCancle)
        {
            Form prompt = new Form()
            {
                Name = "set_label_",
                Width = 100,
                Height = 100,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                Text = Header,
                StartPosition = FormStartPosition.CenterScreen,
                BackColor = Color.FromArgb(213, 204, 204),
                ForeColor = Color.White,
                MinimizeBox = false,
                MaximizeBox = false,
            };
            ComboBox combo_box = new ComboBox()
            {
                Left = 5,
                Top = 5,
                Width = 90,
                BackColor = Color.FromArgb(255, 255, 254),
                ForeColor = Color.Black,
                DropDownStyle = ComboBoxStyle.DropDownList,
            };
            Button confirmation = new Button()
            {
                Text = "Set",
                Left = 30,
                Width = 50,
                Top = 30,
                DialogResult = DialogResult.OK,
                BackColor = Color.FromArgb(117, 147, 247),
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.White
            };

            confirmation.FlatAppearance.BorderSize = 0;
            confirmation.FlatAppearance.MouseOverBackColor = Color.FromArgb(0, 104, 218);

            confirmation.Click += (sender, e) => { prompt.Close(); };
            combo_box.Items.AddRange(list_option);
            prompt.Controls.Add(combo_box);
            prompt.Controls.Add(confirmation);
            prompt.AcceptButton = confirmation;
            PromptCancle = (prompt.ShowDialog() == DialogResult.OK);
            return combo_box.SelectedIndex;
        }
        public static Tuple<string, string> SelectServer(string[] servers, string OPChost_)
        {
            var openFileDialog = new OpenFileDialog
            {
                Title = "Select Server",
                CheckFileExists = false,
                CheckPathExists = false,
                FileName = "Choose a server",
                Filter = "All Files|*.*",
                FilterIndex = 1,
                RestoreDirectory = true
            };

            var listBox = new ListBox
            {
                SelectionMode = SelectionMode.One,
                Dock = DockStyle.Fill
            };

            var selectButton = new Button
            {
                Text = "Select",
                Dock = DockStyle.Bottom,
                Enabled = false
            };

            var labelOPChost = new Label
            {
                Text = "OPChost:",
                TextAlign = ContentAlignment.MiddleRight,
                Width = 60, // Set the width to adjust as needed
                Dock = DockStyle.Fill
            };
            var flowLayoutPanel = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.LeftToRight,
                Dock = DockStyle.Bottom,
                Height = 35
            };
            var textBoxOPChost = new TextBox
            {
                Text = OPChost_,
                ReadOnly = true,
                Dock = DockStyle.Bottom
            };
            flowLayoutPanel.Controls.Add(labelOPChost);
            flowLayoutPanel.Controls.Add(textBoxOPChost);
            flowLayoutPanel.Controls.Add(selectButton);

            listBox.Items.AddRange(servers);
            listBox.SelectedIndexChanged += (sender, e) => { textBoxOPChost.ReadOnly = !(selectButton.Enabled = listBox.SelectedIndex != -1); };

            var form = new Form
            {
                Text = "Select Server",
                Size = new Size(280, 250),
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false
            };
            form.Controls.Add(listBox);
            form.Controls.Add(flowLayoutPanel);

            selectButton.Click += (sender, e) =>
            {
                form.DialogResult = DialogResult.OK;
                form.Close();
            };
            if (form.ShowDialog() == DialogResult.OK)
            {
                return Tuple.Create(listBox.SelectedItem?.ToString().Trim(), textBoxOPChost.Text.Trim());
            }
            return null;
        }
    }
}
