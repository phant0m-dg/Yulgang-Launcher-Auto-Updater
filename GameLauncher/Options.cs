using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace GameLauncher
{

    public partial class Options : Form
    {
        public Options()
        {
            InitializeComponent();
            comboBox1.SelectedIndex = 0;
        }
        public frmLauncher launcherFormRef;

        private readonly int offsetResolution = 0x0E; // Values: 00 800x600, 01 1024x768, 02 1280x1024
        private readonly int offsetViewRange = 0x06; // Values: 00 Far, 01 Medium, 02 Close
        private readonly int offsetDetails = 0x04; // Values: 01 Low, 00 High
        private readonly int offsetLights = 0x02; // Values: 01 Low, 00 High
        private readonly int offsetShadows = 0x03; // Values: 01 Low, 00 High
        private readonly int offsetWindowMode = 0x3B; // Values: 01 Low, 00 High
        private readonly string configFile = "Client/YBopt.cfg";
        //YBopt.ybi v20 file
        private byte[] YBopt = { 0x1C, 0x00, 0x01, 0x01, 0x01, 0x00, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x64, 0x00, 0x00, 0x00, 0x64, 0x00, 0x00, 0x00, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x01, 0x00, 0x00, 0x64, 0x00, 0x00, 0x00, 0x01, 0x01, 0x01, 0x00, 0x00, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x00, 0x00, 0x00, 0x00, 0x01, 0x01, 0x00, 0x00, 0xF4, 0x01, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x01, 0x00, 0x00, 0xFA, 0x01, 0x00, 0x00, 0xF7, 0x01, 0x00, 0x00, 0x01, 0x01, 0x00, 0x00 };

        private byte resolutionValue = 0x00;
        private byte viewRangeValue = 0x00;
        private byte detailsValue = 0x00;
        private byte lightsValue = 0x00;
        private byte shadowValue = 0x00;
        private byte windowModeValue = 0x00;



        private void PatchFile(string originalFile)
        {
            if (radioButton7.Checked)   { viewRangeValue = 0x02; }
            if (radioButton8.Checked)   { viewRangeValue = 0x01; }
            if (radioButton9.Checked)   { viewRangeValue = 0x00; }

            if (radioButton1.Checked)   { detailsValue = 0x00; }
            if (radioButton2.Checked)   { detailsValue = 0x01; }

            if (radioButton3.Checked)   { lightsValue = 0x00; }
            if (radioButton4.Checked)   { lightsValue = 0x01; }

            if (radioButton5.Checked)   { shadowValue = 0x00; }
            if (radioButton6.Checked)   { shadowValue = 0x01; }

            if (checkBox1.Checked) { windowModeValue = 0x01; }
            
            if (comboBox1.SelectedItem.ToString() == "800x600") { resolutionValue = 0x00; }
            if (comboBox1.SelectedItem.ToString() == "1024x768") { resolutionValue = 0x01; }
            if (comboBox1.SelectedItem.ToString() == "1280x1024") { resolutionValue = 0x02; }

            using (var stream = File.Open(originalFile, FileMode.Open))
            {
                stream.Seek(offsetResolution, SeekOrigin.Begin);
                stream.WriteByte(resolutionValue);

                stream.Seek(offsetViewRange, SeekOrigin.Begin);
                stream.WriteByte(viewRangeValue);

                stream.Seek(offsetDetails, SeekOrigin.Begin);
                stream.WriteByte(detailsValue);

                stream.Seek(offsetLights, SeekOrigin.Begin);
                stream.WriteByte(lightsValue);

                stream.Seek(offsetShadows, SeekOrigin.Begin);
                stream.WriteByte(shadowValue);

                stream.Seek(offsetWindowMode, SeekOrigin.Begin);
                stream.WriteByte(windowModeValue);
            }
        }

        private void saveOptBtn_Click(object sender, EventArgs e)
        {
            try
            {
                if (!File.Exists(configFile))
                {
                    FileStream fs = File.Create(configFile);
                    fs.Write(YBopt, 0, YBopt.Length);
                    fs.Dispose();
                }

                // Run Patching code; Format (input)
                PatchFile(configFile);
            }
            // If patching fails, display error and exit
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                Environment.Exit(0);
            }
            this.Close();
        }
    }
}
