using System;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Prism.CustomDialogs
{
    public partial class ChangeMessage : Form
    {
        public int ContinueProgram = 0;

        public ChangeMessage()
        {
            InitializeComponent();

            richTextBox1.ReadOnly = true;
            richTextBox1.ScrollBars = RichTextBoxScrollBars.None; // Initially set to None
            richTextBox1.BackColor = Color.White;

            // Set initial size (minimum size)
            richTextBox1.Size = new Size(500, 100); // Adjust to your preferred starting size
            this.Size = new Size(520, 150); // Adjust to fit richTextBox plus any padding/borders

            // Subscribe to the TextChanged event
            richTextBox1.TextChanged += richTextBox1_TextChanged;
        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {
            // Height for 3 buttons and some padding above them
            int buttonsHeight = 3 * 23; // Adjust padding as needed

            // Temporarily disable AutoSize to manually adjust the form size
            AutoSize = false;

            // Use the TextRenderer to measure the text
            Size textSize = TextRenderer.MeasureText(richTextBox1.Text, richTextBox1.Font, new Size(richTextBox1.Width, int.MaxValue), TextFormatFlags.WordBreak | TextFormatFlags.TextBoxControl);

            // Desired size is text size plus a little padding
            int desiredHeight = textSize.Height + 10;

            // Check against maximum size, accounting for buttons
            if (desiredHeight + buttonsHeight > 800)
            {
                richTextBox1.ScrollBars = RichTextBoxScrollBars.Vertical; // Enable scrollbar
                                                                          // Set richTextBox height to max available space minus space for buttons
                richTextBox1.Height = 800 - buttonsHeight - 50; // Subtract form padding/border space
                this.Size = new Size(500, 800); // Set form to maximum size
            }
            else
            {
                richTextBox1.Height = desiredHeight;
                // Adjust form height based on richTextBox height, space for buttons, and some padding
                this.Height = richTextBox1.Height + buttonsHeight + 50;
                richTextBox1.ScrollBars = RichTextBoxScrollBars.None; // Disable scrollbar if content fits
            }
        }


        public void SetMessage(string message)
        {
            richTextBox1.Rtf = message;

        }

        private void btn_Cancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btn_ConfirmAndContinue_Click_1(object sender, EventArgs e)
        {
            ContinueProgram = 1;
            this.Close();
        }

        private void btn_StopAndReport_Click(object sender, EventArgs e)
        {
            ContinueProgram = 2;
            this.Close();
        }
    }
}