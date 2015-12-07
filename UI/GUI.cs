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
using MipSim.UI;

namespace MipSim
{
    public partial class GUI : Form
    {
        private CPU _cpu;
        private string[] fileLines;

        public GUI()
        {
            fileLines = new string[0];

            InitializeComponent();
            Initialize();
        }

        private void Initialize()
        {
            _cpu = new CPU();

            listView1.Items.Clear();
            listView2.Items.Clear();
            listBox1.Items.Clear();;

            for (int i = 0; i < 16; i++)
            {
                listView1.Items.Add(new CustomListItem(i, 0, "$"));
                listView2.Items.Add(new CustomListItem(i, 0));
            }

            toolStripStatusLabel2.Text = "0";
            toolStripStatusLabel4.Text = "0";

            beginToolStripMenuItem.Enabled = false;
            stepToolStripMenuItem.Enabled = false;
            runToolStripMenuItem.Enabled = false;

            if (fileLines.Length > 0)
            {
                Dictionary<int, string> errors = _cpu.ParseCode(fileLines);
                
                if (errors.Count > 0)
                {
                    string errorMessage = errors.Aggregate("", (current, error) => current + ("Line " + error.Key + ": " + error.Value + Environment.NewLine));
                    MessageBox.Show(errorMessage.Trim(), "Parsing Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    beginToolStripMenuItem.Enabled = true;
                    stepToolStripMenuItem.Enabled = true;
                    runToolStripMenuItem.Enabled = true;

                    listBox1.Items.Clear();

                    foreach(string instruction in fileLines)
                    {
                        listBox1.Items.Add(instruction.Trim());
                    }
                }
            }

            tableLayoutPanel1.RowStyles.Clear();
            tableLayoutPanel1.ColumnStyles.Clear();
            tableLayoutPanel1.Controls.Clear();

            tableLayoutPanel1.RowCount = 0;
            tableLayoutPanel1.ColumnCount = 0;
        }

        private void openFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            if (File.Exists(openFileDialog1.FileName))
            {
                fileLines = File.ReadAllLines(openFileDialog1.FileName);

                //Ignore blank lines
                fileLines = fileLines.Where(line => line.Trim() != "").ToArray();

                Initialize();
            }
        }

        private void loadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openFileDialog1.ShowDialog();
        }

        private void listView_ColumnWidthChanging(object sender, ColumnWidthChangingEventArgs e)
        {
            e.NewWidth = ((ListView)sender).Columns[e.ColumnIndex].Width;
            e.Cancel = true;
        }

        private void listView1_DoubleClick(object sender, EventArgs e)
        {
            var selectedItem = listView1.SelectedItems[0] as CustomListItem;

            if (selectedItem == null || selectedItem.Idx == 0)
                return;

            var diag = new ChangeValueDialog(selectedItem, "Register $", "Change Register Value");
            diag.ShowDialog();

            if (diag.DialogResult == DialogResult.OK)
            {
                _cpu.RegWrite(selectedItem.Idx, selectedItem.Value);
                selectedItem.Value = diag.ReturnValue;
            }
        }

        private void listView2_DoubleClick(object sender, EventArgs e)
        {
            var selectedItem = listView2.SelectedItems[0] as CustomListItem;

            if (selectedItem == null)
                return;

            var diag = new ChangeValueDialog(selectedItem, "Memory Location ", "Change Memory Value");
            diag.ShowDialog();

            if (diag.DialogResult == DialogResult.OK)
            {
                _cpu.Store(selectedItem.Idx, selectedItem.Value);
                selectedItem.Value = diag.ReturnValue;
            }
        }

        public class CustomListItem : ListViewItem
        {
            private int _idx;
            private int _value;
            private readonly string _prefix;

            public int Idx
            {
                get { return _idx; }
                set
                {
                    _idx = value;
                    Text = _prefix + _idx;
                }
            }

            public int Value
            {
                get { return _value; }
                set
                {
                    _value = value;
                    SubItems[1].Text = value.ToString();
                }
            }

            public CustomListItem(int idx, int value, string prefix = "")
            {
                SubItems.Add("0");

                _prefix = prefix;
                Idx = idx;
                Value = value;
            }
        }

        private void testToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int clockCycle = tableLayoutPanel1.ColumnCount - 1;

            tableLayoutPanel1.ColumnCount++;
            tableLayoutPanel1.RowCount++;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 56F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 56F));

            for (int i = 0; i < 5; i++)
            {
                int row = clockCycle - i;

                if (row < 0)
                    break;

                var btn = new Button { Location = new Point(3, 3), Size = new Size(50, 50)};

                switch (i)
                {
                    case 0:
                        btn.Text = "IF";
                        break;
                    case 1:
                        btn.Text = "ID";
                        break;
                    case 2:
                        btn.Text = "EX";
                        break;
                    case 3:
                        btn.Text = "MEM";
                        break;
                    case 4:
                        btn.Text = "WB";
                        break;
                }
                
                btn.UseVisualStyleBackColor = true;

                tableLayoutPanel1.Controls.Add(btn, clockCycle, row);
            }
        }

        private void beginToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Initialize();
        }

        private void stepToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if(_cpu.RunClock())
            {
                var exec = _cpu.ExecutionRecords[_cpu.ExecutionRecords.Count - 1];

                int clockCycle = _cpu.ClockCycle - 1;

                foreach(var execRecord in exec)
                {
                    var btn = new Button { Location = new Point(3, 3), Size = new Size(50, 50) };

                    switch (execRecord.Type)
                    {
                        case ExecutionType.Fetch:
                            btn.Text = "IF";
                            break;
                        case ExecutionType.Decode:
                            btn.Text = "ID";
                            break;
                        case ExecutionType.Execute:
                            btn.Text = "EX";
                            break;
                        case ExecutionType.Memory:
                            btn.Text = "MEM";
                            break;
                        case ExecutionType.Writeback:
                            btn.Text = "WB";
                            break;
                    }

                    btn.UseVisualStyleBackColor = true;
                    btn.Click += (o, ea) =>
                    {
                        MessageBox.Show(Enum.GetName(typeof(ExecutionType), execRecord.Type) + ": " + execRecord.Value);
                    };

                    tableLayoutPanel1.Controls.Add(btn, clockCycle, execRecord.ExecutionNumber);
                }

                tableLayoutPanel1.ColumnCount++;
                tableLayoutPanel1.RowCount++;
                tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 56F));
                tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 56F));
            }
        }
    }
}
