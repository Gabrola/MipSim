using System;
using System.Drawing.Design;
using System.Windows.Forms;

namespace MipSim.UI
{
    public sealed partial class ChangeValueDialog : Form
    {
        public int ReturnValue { get; private set; }

        public ChangeValueDialog(GUI.CustomListItem listItem, string prefix, string title)
        {
            InitializeComponent();

            label1.Text = prefix + listItem.Idx + " Value";
            Text = title;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            int retVal;
            if (int.TryParse(textBox1.Text, out retVal))
            {
                ReturnValue = retVal;
                DialogResult = DialogResult.OK;
                Close();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
