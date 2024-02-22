using GodSharp.Opc.Da;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SCADA
{
    public partial class form_tag : Form
    {
        private static form_tag instance;
        private readonly form_main form;
        public form_tag(form_main form)
        {
            InitializeComponent();
            this.form = form;
            dataGridViewTagIo.DataSource = form.OPCData_1;
        }
        public static form_tag GetInstance(form_main form) => instance == null || instance.IsDisposed ? instance = new form_tag(form) : instance;

        private void form_tag_Load(object sender, EventArgs e)
        {
            toolStripTextBoxHost.Text = Properties.Settings.Default.OPCHost;
            toolStripTextBoxServer.Text = Properties.Settings.Default.OPCServer1;
            if (!OPCStatus1.Connected)
            {
                ToolStripMenuItemConnectOPC.Enabled = true; return;
            }
            treeViewOPC();
        }
        private void treeViewOPC()
        {
            treeView1.Nodes.Clear();
            var root = new TreeNode(Properties.Settings.Default.OPCServer1) { ImageIndex = 4, SelectedImageIndex = 5 };
            treeView1.Nodes.Add(root);
            var list = form.OPCclient1.BrowseNodeTree();
            if (list == null) return;
            Dictionary<string, TreeNode> tree = new Dictionary<string, TreeNode>();
            NodeTree(list, tree);
            var nodes = tree.Where(x => list.Any(a => a.Name == x.Key)).Select(x => x.Value).ToArray();
            root.Nodes.AddRange(nodes);

        }
        private void NodeTree(IEnumerable<BrowseNode> nodes, Dictionary<string, TreeNode> tree, string parent = null)
        {
            foreach (var item in nodes)
            {
                TreeNode node = CreateTreeNode(item);
                tree.Add(item.Full, node);
                if (!item.IsLeaf && item.Childs != null && item.Childs.Any()) NodeTree(item.Childs, tree, item.Full);
                if (parent != null && node != null) tree[parent].Nodes.Add(node);
            }
        }
        private TreeNode CreateTreeNode(BrowseNode item)
        {
            int leafImageIndex = item.Branch == null ? (item.IsLeaf ? 0 : 2) : (item.IsLeaf ? 0 : 2);

            return new TreeNode(item.Name)
            {
                Name = item.Name,
                Tag = item.Full,
                ImageIndex = leafImageIndex,
                SelectedImageIndex = leafImageIndex + 1
            };
        }
        private void treeView1_AfterCollapse(object sender, TreeViewEventArgs e)
        {
            if (e.Node.Nodes.Count == 0) return;
            e.Node.ImageIndex = 2;
            e.Node.SelectedImageIndex = 3;
        }

        private void treeView1_AfterExpand(object sender, TreeViewEventArgs e)
        {
            if (e.Node.Nodes.Count == 0) return;
            e.Node.ImageIndex = 4;
            e.Node.SelectedImageIndex = 5;
        }
        private void ToolStripMenuItemsetOPCServer_Click(object sender, EventArgs e)
        {
            try
            {
                var servers = form.OPCdiscovery.GetServers(ServerSpecification.DA30, Properties.Settings.Default.OPCHost);
                if (servers == null || servers.Length == 0) { return; };
                var select = Prompt.SelectServer(servers, Properties.Settings.Default.OPCHost);
                if (select == null) return;
                Properties.Settings.Default.OPCHost = select.Item2;
                Properties.Settings.Default.OPCServer1 = select.Item1;
                Properties.Settings.Default.Save();
            }
            catch (Exception exception)
            {
                MessageBox.Show($@"Enumerate opc server throw exception:{exception.Message}\r\n{exception.StackTrace}");
            }
        }
        private void ToolStripMenuItemAboutOpc_Click(object sender, EventArgs e) => System.Diagnostics.Process.Start("https://github.com/godsharp/GodSharp.OpcDa");

        private void ToolStripMenuItemConnectOPC_Click(object sender, EventArgs e)
        {
            form.OPC1Connect_or_Disconnect(false);
            if (OPCStatus1.Connected) treeViewOPC();
        }
    }
}
