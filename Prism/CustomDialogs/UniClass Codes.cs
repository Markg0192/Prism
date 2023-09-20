using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace Prism.CustomDialogs
{
    public partial class UniClass_Codes : Form
    {
        string fileLocation;

        public UniClass_Codes(string jobName, ExternalService.WebService1 service)
        {
            InitializeComponent();
            fileLocation = Constants.ModelProjectInforLocation(jobName);
            TopMost = true;
            CenterToScreen();
            PopulateBoxes(fileLocation, service);
        }

        private void btn_UCApply_Click(object sender, EventArgs e)
        {
            TableData tableData = BuildTables();
            PrintTableData(tableData, fileLocation);
            lbl_UCApplySuccess.Visible = true;
        }

        TableData BuildTables()
        {
            TableData td = new TableData();
            td.Rows.Add(new TableRow(txt_UCFilter1.Text, txt_UCCode1.Text, txt_UCTitle1.Text));
            td.Rows.Add(new TableRow(txt_UCFilter2.Text, txt_UCCode2.Text, txt_UCTitle2.Text));
            td.Rows.Add(new TableRow(txt_UCFilter3.Text, txt_UCCode3.Text, txt_UCTitle3.Text));
            td.Rows.Add(new TableRow(txt_UCFilter4.Text, txt_UCCode4.Text, txt_UCTitle4.Text));
            td.Rows.Add(new TableRow(txt_UCFilter5.Text, txt_UCCode5.Text, txt_UCTitle5.Text));
            td.Rows.Add(new TableRow(txt_UCFilter6.Text, txt_UCCode6.Text, txt_UCTitle6.Text));
            td.Rows.Add(new TableRow(txt_UCFilter7.Text, txt_UCCode7.Text, txt_UCTitle7.Text));
            td.Rows.Add(new TableRow(txt_UCFilter8.Text, txt_UCCode8.Text, txt_UCTitle8.Text));
            return td;
        }

        private void PopulateBoxes(string filePath, ExternalService.WebService1 service)
        {
            TableData td = ReadTableData(filePath, service);
            txt_UCFilter1.Text = td.Rows[0].Filter;
            txt_UCCode1.Text = td.Rows[0].Code;
            txt_UCTitle1.Text = td.Rows[0].Title;
            txt_UCFilter2.Text = td.Rows[1].Filter;
            txt_UCCode2.Text = td.Rows[1].Code;
            txt_UCTitle2.Text = td.Rows[1].Title;
            txt_UCFilter3.Text = td.Rows[2].Filter;
            txt_UCCode3.Text = td.Rows[2].Code;
            txt_UCTitle3.Text = td.Rows[2].Title;
            txt_UCFilter4.Text = td.Rows[3].Filter;
            txt_UCCode4.Text = td.Rows[3].Code;
            txt_UCTitle4.Text = td.Rows[3].Title;
            txt_UCFilter5.Text = td.Rows[4].Filter;
            txt_UCCode5.Text = td.Rows[4].Code;
            txt_UCTitle5.Text = td.Rows[4].Title;
            txt_UCFilter6.Text = td.Rows[5].Filter;
            txt_UCCode6.Text = td.Rows[5].Code;
            txt_UCTitle6.Text = td.Rows[5].Title;
            txt_UCFilter7.Text = td.Rows[6].Filter;
            txt_UCCode7.Text = td.Rows[6].Code;
            txt_UCTitle7.Text = td.Rows[6].Title;
            txt_UCFilter8.Text = td.Rows[7].Filter;
            txt_UCCode8.Text = td.Rows[7].Code;
            txt_UCTitle8.Text = td.Rows[7].Title;
        }

        public static TableData ReadTableData(string filePath, ExternalService.WebService1 service)
        {
            bool startReading = false;
            TableData tableData = new TableData();

            foreach (string line in service.ReadAllLinesIntoArray(Constants.PrismDataLogLocation, Constants.ModelProjectInforLocation(filePath)))
            {
                if (line.Contains("---Filter------------Code----------------Title"))
                {
                    startReading = true;
                    continue;
                }

                if (startReading)
                {
                    string[] parts = line.Split(new[] { '*' }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length >= 3)
                    {
                        string filter = parts[0].Trim();
                        string code = parts[1].Trim();
                        string title = parts[2].Trim();

                        tableData.Rows.Add(new TableRow(filter, code, title));
                    }
                    if (parts.Length < 3)
                    {
                        tableData.Rows.Add(new TableRow("", "", ""));
                    }
                }
            }

            return tableData;
        }

        static void PrintTableData(TableData tableData, string filePath)
        {
            StreamReader read = new StreamReader(filePath);
            string line1 = read.ReadLine();
            string line2 = read.ReadLine();
            string line3 = read.ReadLine();
            string line4 = read.ReadLine();
            string line5 = read.ReadLine();
            string line6 = read.ReadLine();
            string line7 = read.ReadLine();
            string line8 = tableData.Rows[0].Filter + "****" + tableData.Rows[0].Code + "****" + tableData.Rows[0].Title;
            string line9 = tableData.Rows[1].Filter + "****" + tableData.Rows[1].Code + "****" + tableData.Rows[1].Title;
            string line10 = tableData.Rows[2].Filter + "****" + tableData.Rows[2].Code + "****" + tableData.Rows[2].Title;
            string line11 = tableData.Rows[3].Filter + "****" + tableData.Rows[3].Code + "****" + tableData.Rows[3].Title;
            string line12 = tableData.Rows[4].Filter + "****" + tableData.Rows[4].Code + "****" + tableData.Rows[4].Title;
            string line13 = tableData.Rows[5].Filter + "****" + tableData.Rows[5].Code + "****" + tableData.Rows[5].Title;
            string line14 = tableData.Rows[6].Filter + "****" + tableData.Rows[6].Code + "****" + tableData.Rows[6].Title;
            string line15 = tableData.Rows[7].Filter + "****" + tableData.Rows[7].Code + "****" + tableData.Rows[7].Title;
            read.Close();
            StreamWriter writer = new StreamWriter(filePath);
            writer.WriteLine(line1);
            writer.WriteLine(line2);
            writer.WriteLine(line3);
            writer.WriteLine(line4);
            writer.WriteLine(line5);
            writer.WriteLine(line6);
            writer.WriteLine(line7);
            writer.WriteLine(line8);
            writer.WriteLine(line9);
            writer.WriteLine(line10);
            writer.WriteLine(line11);
            writer.WriteLine(line12);
            writer.WriteLine(line13);
            writer.WriteLine(line14);
            writer.WriteLine(line15);
            writer.Close();
        }

        private void btn_UCCLose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}

public class TableData
{
    public List<TableRow> Rows { get; }

    public TableData()
    {
        Rows = new List<TableRow>();
    }
}

public class TableRow
{
    public string Filter { get; }
    public string Code { get; }
    public string Title { get; }

    public TableRow(string filter, string code, string title)
    {
        Filter = filter;
        Code = code;
        Title = title;
    }
}