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
using System.Text.RegularExpressions;

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

            stepToolStripMenuItem.Enabled = false;
            runToolStripMenuItem.Enabled = false;

            listView1.Enabled = true;
            listView2.Enabled = true;

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
                selectedItem.Value = diag.ReturnValue;
                _cpu.RegWrite(selectedItem.Idx, selectedItem.Value);
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
                selectedItem.Value = diag.ReturnValue;
                _cpu.Store(selectedItem.Idx << 2, selectedItem.Value);
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

        private void beginToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Initialize();
        }

        private void stepToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!Step())
                MessageBox.Show("Execution finished in " + _cpu.ClockCycle + " clock cycles");
        }

        private bool Step()
        {
            toolStripStatusLabel4.Text = _cpu.GetPC().ToString();

            listBox1.SelectedIndex = _cpu.GetArrayPC() >= listBox1.Items.Count ? listBox1.Items.Count - 1 : _cpu.GetArrayPC();

            if (_cpu.RunClock())
            {
                var exec = _cpu.ExecutionRecords[_cpu.ExecutionRecords.Count - 1];

                int clockCycle = _cpu.ClockCycle;

                if (clockCycle == 1)
                {
                    listView1.Enabled = false;
                    listView2.Enabled = false;
                }

                for (int i = 0; i < 5; i++)
                    listView3.Items[i].SubItems[1].Text = "Idle";

                foreach (var execRecord in exec)
                {
                    var btn = new Button { Location = new Point(3, 3), Size = new Size(50, 50) };

                    switch (execRecord.Type)
                    {
                        case ExecutionType.Fetch:
                            btn.Text = "IF";

                            var lbl = new Label();
                            lbl.Size = new System.Drawing.Size(50, 50);
                            lbl.TextAlign = ContentAlignment.MiddleCenter;
                            lbl.Text = "Inst. #" + execRecord.Instruction.InstructionNumber;
                            AddControl(lbl, clockCycle - 1, execRecord.ExecutionNumber);

                            listView3.Items[0].SubItems[1].Text = string.Format("Instruction {0}: {1}", execRecord.Instruction.InstructionNumber, execRecord.Value);

                            break;
                        case ExecutionType.Decode:
                            btn.Text = "ID";

                            listView3.Items[1].SubItems[1].Text = string.Format("Instruction {0}: {1}", execRecord.Instruction.InstructionNumber, execRecord.Value);
                            break;
                        case ExecutionType.Execute:
                            btn.Text = "EX";

                            listView3.Items[2].SubItems[1].Text = string.Format("Instruction {0}: {1}", execRecord.Instruction.InstructionNumber, execRecord.Value);
                            break;
                        case ExecutionType.Memory:
                            btn.Text = "MEM";

                            listView3.Items[3].SubItems[1].Text = string.Format("Instruction {0}: {1}", execRecord.Instruction.InstructionNumber, execRecord.Value);

                            var regex2 = new Regex(@"^Memory #([0-9]+) <= (-?[0-9]+)$", RegexOptions.IgnoreCase);
                            var match2 = regex2.Match(execRecord.Value);

                            if (match2.Success)
                            {
                                int memIndex = int.Parse(match2.Groups[1].Value);
                                int memValue = int.Parse(match2.Groups[2].Value);

                                listView2.Items[memIndex].SubItems[1].Text = memValue.ToString();
                                listView2.SelectedIndices.Clear();
                                listView2.SelectedIndices.Add(memIndex);
                            }

                            break;
                        case ExecutionType.Writeback:
                            btn.Text = "WB";

                            listView3.Items[4].SubItems[1].Text = string.Format("Instruction {0}: {1}", execRecord.Instruction.InstructionNumber, execRecord.Value);

                            var regex = new Regex(@"^Register \$([0-9]+) <= (-?[0-9]+)$", RegexOptions.IgnoreCase);
                            var match = regex.Match(execRecord.Value);

                            if (match.Success)
                            {
                                int regNum = int.Parse(match.Groups[1].Value);
                                int regValue = int.Parse(match.Groups[2].Value);

                                listView1.Items[regNum].SubItems[1].Text = regValue.ToString();
                                listView1.SelectedIndices.Clear();
                                listView1.SelectedIndices.Add(regNum);
                            }

                            break;
                    }

                    btn.UseVisualStyleBackColor = true;
                    btn.Click += (o, ea) =>
                    {
                        MessageBox.Show(Enum.GetName(typeof(ExecutionType), execRecord.Type) + ": " + execRecord.Value);
                    };

                    AddControl(btn, clockCycle, execRecord.ExecutionNumber);
                }

                toolStripStatusLabel2.Text = _cpu.ClockCycle.ToString();

                return true;
            }

            return false;
        }

        private void AddControl(Control control, int column, int row)
        {
            CheckAndAddColumn(column);
            CheckAndAddRow(row);

            tableLayoutPanel1.Controls.Add(control, column, row);
        }

        private void CheckAndAddRow(int row)
        {
            if (tableLayoutPanel1.RowCount >= row)
            {
                //tableLayoutPanel1.RowCount++;
                tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 56F));
            }
        }

        private void CheckAndAddColumn(int column)
        {
            if (tableLayoutPanel1.ColumnCount >= column)
            {
                //tableLayoutPanel1.ColumnCount++;
                tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 56F));
            }
        }

        private void runToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int counter = 1;
            while (Step() && counter < 100) 
                counter++;

            if (counter == 100)
                MessageBox.Show("Execution stopped after 100 clock cycles limit.");
            else
                MessageBox.Show("Execution finished in " + _cpu.ClockCycle + " clock cycles");
        }
    }
}
