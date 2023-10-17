using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Prism
{
    public partial class HelpForm : Form
    {
        private string HelpFolder = @"\\sev-los-fs1\application data$\Prism\Help";
        private HelpNode RootNode;

        public HelpForm(string name, string parent, ExternalService.WebService1 service)
        {
            InitializeComponent();
            CenterToScreen();
            TopMost = true;

            TreeView.SelectedImageIndex = 2;

            CreateHelpFromRootFolder(9, service);

            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(parent))
                Browser.Navigate(HelpFolder + @"\Index.mht");
            else
                BrowseTo(name, parent);
        }

        private void BrowseTo(string name, string parent)
        {
            foreach (TreeNode node in TreeView.Nodes)
                BrowseTo(name, parent, node);
        }

        private void BrowseTo(string name, string parent, TreeNode startNode)
        {
            foreach (TreeNode node in startNode.Nodes)
            {
                if (node.Text == name)
                {
                    if (node.Parent.Text == parent)
                    {
                        TreeView.SelectedNode = node;
                        Browser.Navigate(node.Tag.ToString());
                        break;
                    }
                }
                
                BrowseTo(name, parent, node);
            }
        } 

        private void CreateHelpFromRootFolder(int filePathLine, ExternalService.WebService1 service)
        {
            if (!service.DirectoryExists(filePathLine, "")) return;

            RootNode = new HelpNode("Index", service.GetDirectoryName(filePathLine, @"\Index.mht"));
            AddNodes(ref RootNode, 9, service);

            var root = new TreeNode("Index");
            root.ImageIndex = 0;
            root.Tag = RootNode.Filepath;
            root.Expand();
            TreeView.Nodes.Add(root);
            BuildTree(RootNode, root);
        }
       
        private void AddNodes(ref HelpNode root, int filePathLine, ExternalService.WebService1 service)
        {
            var directories = service.GetDirectories(filePathLine, "");// Directory.GetDirectories(folderpath);
            foreach (var d in directories)
            {
                var name = new DirectoryInfo(Path.GetDirectoryName(d + "\\")).Name;
                if (name.Contains("-")) name = name.Split('-')[1];

                var filepath = d + "\\" + name + ".mht";
                var files = service.DirectoryGetFiles(90, "*.mht", d);
                if (!files.Contains(filepath)) break;
                var newNode = new HelpNode(name, filepath);
                foreach (var dir in service.GetDirectories(90, d))
                {
                    AddNodes(ref newNode, 9, service);
                }

                foreach (var file in files)
                {
                    if (file != filepath)
                    {
                        var subname = Path.GetFileNameWithoutExtension(file);
                        if (subname.Contains("-")) subname = subname.Split('-')[1];
                        var fileNode = new HelpNode(subname, file);
                        newNode.NodeList.Add(fileNode);
                    }
                }

                root.NodeList.Add(newNode);
            }
        }

        private class HelpNode
        {
            public string Name;
            public string Filepath;
            public List<HelpNode> NodeList = new List<HelpNode>();

            public HelpNode(string name, string filepath)
            {
                Name = name;
                Filepath = filepath;
            }
        }

        private void BuildTree(HelpNode helpNode, TreeNode node)
        {
            foreach (var hn in helpNode.NodeList)
            {
                var newNode = new TreeNode(hn.Name);
                if (hn.NodeList.Count > 0)
                    newNode.ImageIndex = 0;
                else
                    newNode.ImageIndex = 1;

                newNode.Tag = hn.Filepath;
                node.Nodes.Add(newNode);
                BuildTree(hn, newNode);
            }
        }

        private void TreeView_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            Browser.Navigate(e.Node.Tag.ToString());
            TreeView.SelectedNode = e.Node;
        }
    }
}
