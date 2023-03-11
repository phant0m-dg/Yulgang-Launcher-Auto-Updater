using System;
using System.Drawing;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace GameLauncher
{
	public partial class frmLauncher : AlphaForm
	{
        public frmLauncher()
		{
            // Get the directory containing the currently executing assembly
            string assemblyPath = Assembly.GetExecutingAssembly().Location;
            string assemblyDirectory = Path.GetDirectoryName(assemblyPath);

            // Search for files with the ".DELETE" extension in the assembly directory
            string[] deleteFiles = Directory.GetFiles(assemblyDirectory, "*.DELETE");

            // Delete the found files
            foreach (string file in deleteFiles)
            {
                File.Delete(file);
            }

            if (!File.Exists("launcher.dat"))
            {
                using (FileStream fs = File.Create("launcher.dat"));
            }

            InitializeComponent();
		}

        private Image m_closeBtnOff;
        private Image m_closeBtnOn;

        private Image m_BtnGameStartOff;
        private Image m_BtnGameStartOn;

        private Image m_BtnOptOff;
        private Image m_BtnOptOn;

        private Patcher oPatcher;

        #region Game Launcher Load
        private void GameLauncher_Load(object sender, EventArgs e)
        {
            DrawControlBackground(this.btnClose, false);
            DrawControlBackground(this.btnStartGame, false);
            DrawControlBackground(this.lblStatusText, false);
            UpdateLayeredBackground();

            //For convenience when switching between the on / off images
            m_closeBtnOff = this.btnClose.Image;
            m_closeBtnOn = this.btnClose.BackgroundImage;

            m_BtnGameStartOff = this.btnStartGame.Image;
            m_BtnGameStartOn = this.btnStartGame.BackgroundImage;

            m_BtnOptOff = this.btnOptions.Image;
            m_BtnOptOn = this.btnOptions.BackgroundImage;

            this.oPatcher = new Patcher(this);
        } 
        #endregion

        #region Close Button Functions
        private void picBoxClose_MouseEnter(object sender, EventArgs e)
        {
            this.btnClose.Image = m_closeBtnOn;
            this.btnClose.BackgroundImage = m_closeBtnOff;
            this.btnClose.Cursor = Cursors.Hand;
        }

        private void picBoxClose_MouseLeave(object sender, EventArgs e)
        {
            this.btnClose.BackgroundImage = m_closeBtnOn;
            this.btnClose.Image = m_closeBtnOff;
            this.btnClose.Cursor = Cursors.Default;
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Application.Exit();
        } 
        #endregion

        #region Start Game Button Functions
        private void btnStartGame_MouseEnter(object sender, EventArgs e)
        {
            this.btnStartGame.Image = m_BtnGameStartOn;
            this.btnStartGame.BackgroundImage = m_BtnGameStartOff;
            this.btnStartGame.Cursor = Cursors.Hand;
        }

        private void btnStartGame_MouseLeave(object sender, EventArgs e)
        {
            this.btnStartGame.BackgroundImage = m_BtnGameStartOn;
            this.btnStartGame.Image = m_BtnGameStartOff;
            this.btnStartGame.Cursor = Cursors.Default;
        }

        private void btnStartGame_Click(object sender, EventArgs e)
        {
            try
            {
                // Save environment path
                string currentDirectory = Environment.CurrentDirectory;

                string execute = string.Concat(AppDomain.CurrentDomain.SetupInformation.ApplicationBase, "\\", this.oPatcher.getClientPath());
                string execute_path = System.IO.Path.GetDirectoryName(execute);

                Environment.CurrentDirectory = execute_path;

                ProcessStartInfo startInfo = new ProcessStartInfo(execute);
                startInfo.Arguments = this.oPatcher.getClientParameters();
                startInfo.UseShellExecute = true;
                Process.Start(startInfo);

                // Restore the selected directory locally.   
                Environment.CurrentDirectory = currentDirectory;
                this.Close();
            }
            catch (Exception ex)
            {
                this.oPatcher.setStatusText(ex.Message);
            }
        } 
        #endregion

        #region Options Button Functions
        private void btnOptions_MouseEnter(object sender, EventArgs e)
        {
            this.btnOptions.Image = m_BtnOptOn;
            this.btnOptions.BackgroundImage = m_BtnOptOff;
            this.btnOptions.Cursor = Cursors.Hand;
        }

        private void btnOptions_MouseLeave(object sender, EventArgs e)
        {
            this.btnOptions.BackgroundImage = m_BtnOptOn;
            this.btnOptions.Image = m_BtnOptOff;
            this.btnOptions.Cursor = Cursors.Default;
        }

        private void btnOptions_Click(object sender, EventArgs e)
        {
            if (Application.OpenForms["Options"] == null)
            {
                var optForm = new Options();
                optForm.Show();
            }
        } 
        #endregion
    }
}
