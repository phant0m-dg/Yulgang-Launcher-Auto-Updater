namespace GameLauncher
{
    partial class frmLauncher
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmLauncher));
            this.lblStatusText = new System.Windows.Forms.Label();
            this.btnStartGame = new System.Windows.Forms.PictureBox();
            this.btnClose = new System.Windows.Forms.PictureBox();
            this.ctrlProgressBar = new ColorProgressBar.ColorProgressBar();
            this.newsWebbrowser = new System.Windows.Forms.WebBrowser();
            this.lblDownloadSpeed = new System.Windows.Forms.Label();
            this.btnOptions = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.btnStartGame)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnClose)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnOptions)).BeginInit();
            this.SuspendLayout();
            // 
            // lblStatusText
            // 
            this.lblStatusText.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(74)))), ((int)(((byte)(139)))), ((int)(((byte)(194)))));
            this.lblStatusText.Font = new System.Drawing.Font("Calibri", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblStatusText.ForeColor = System.Drawing.Color.White;
            this.lblStatusText.Location = new System.Drawing.Point(97, 413);
            this.lblStatusText.Name = "lblStatusText";
            this.lblStatusText.Size = new System.Drawing.Size(370, 22);
            this.lblStatusText.TabIndex = 3;
            // 
            // btnStartGame
            // 
            this.btnStartGame.BackColor = System.Drawing.Color.Transparent;
            this.btnStartGame.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnStartGame.BackgroundImage")));
            this.btnStartGame.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.btnStartGame.Enabled = false;
            this.btnStartGame.Image = ((System.Drawing.Image)(resources.GetObject("btnStartGame.Image")));
            this.btnStartGame.Location = new System.Drawing.Point(555, 385);
            this.btnStartGame.Name = "btnStartGame";
            this.btnStartGame.Size = new System.Drawing.Size(165, 72);
            this.btnStartGame.TabIndex = 2;
            this.btnStartGame.TabStop = false;
            this.btnStartGame.Visible = false;
            this.btnStartGame.Click += new System.EventHandler(this.btnStartGame_Click);
            this.btnStartGame.MouseEnter += new System.EventHandler(this.btnStartGame_MouseEnter);
            this.btnStartGame.MouseLeave += new System.EventHandler(this.btnStartGame_MouseLeave);
            // 
            // btnClose
            // 
            this.btnClose.BackColor = System.Drawing.Color.Transparent;
            this.btnClose.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnClose.BackgroundImage")));
            this.btnClose.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.btnClose.Image = ((System.Drawing.Image)(resources.GetObject("btnClose.Image")));
            this.btnClose.Location = new System.Drawing.Point(703, 6);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(24, 24);
            this.btnClose.TabIndex = 1;
            this.btnClose.TabStop = false;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            this.btnClose.MouseEnter += new System.EventHandler(this.picBoxClose_MouseEnter);
            this.btnClose.MouseLeave += new System.EventHandler(this.picBoxClose_MouseLeave);
            // 
            // ctrlProgressBar
            // 
            this.ctrlProgressBar.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(43)))), ((int)(((byte)(97)))), ((int)(((byte)(142)))));
            this.ctrlProgressBar.BarColor = System.Drawing.Color.FromArgb(((int)(((byte)(26)))), ((int)(((byte)(66)))), ((int)(((byte)(99)))));
            this.ctrlProgressBar.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(43)))), ((int)(((byte)(97)))), ((int)(((byte)(142)))));
            this.ctrlProgressBar.FillStyle = ColorProgressBar.ColorProgressBar.FillStyles.Solid;
            this.ctrlProgressBar.Location = new System.Drawing.Point(100, 438);
            this.ctrlProgressBar.Maximum = 100;
            this.ctrlProgressBar.Minimum = 0;
            this.ctrlProgressBar.Name = "ctrlProgressBar";
            this.ctrlProgressBar.Size = new System.Drawing.Size(440, 20);
            this.ctrlProgressBar.Step = 10;
            this.ctrlProgressBar.TabIndex = 4;
            this.ctrlProgressBar.Value = 0;
            // 
            // newsWebbrowser
            // 
            this.newsWebbrowser.Location = new System.Drawing.Point(26, 69);
            this.newsWebbrowser.MinimumSize = new System.Drawing.Size(20, 20);
            this.newsWebbrowser.Name = "newsWebbrowser";
            this.newsWebbrowser.Size = new System.Drawing.Size(341, 199);
            this.newsWebbrowser.TabIndex = 5;
            // 
            // lblDownloadSpeed
            // 
            this.lblDownloadSpeed.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(74)))), ((int)(((byte)(139)))), ((int)(((byte)(194)))));
            this.lblDownloadSpeed.Font = new System.Drawing.Font("Calibri", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblDownloadSpeed.ForeColor = System.Drawing.Color.White;
            this.lblDownloadSpeed.Location = new System.Drawing.Point(473, 413);
            this.lblDownloadSpeed.Name = "lblDownloadSpeed";
            this.lblDownloadSpeed.Size = new System.Drawing.Size(67, 22);
            this.lblDownloadSpeed.TabIndex = 6;
            this.lblDownloadSpeed.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // btnOptions
            // 
            this.btnOptions.BackColor = System.Drawing.Color.Transparent;
            this.btnOptions.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnOptions.BackgroundImage")));
            this.btnOptions.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.btnOptions.Image = ((System.Drawing.Image)(resources.GetObject("btnOptions.Image")));
            this.btnOptions.Location = new System.Drawing.Point(632, 341);
            this.btnOptions.Name = "btnOptions";
            this.btnOptions.Size = new System.Drawing.Size(88, 38);
            this.btnOptions.TabIndex = 7;
            this.btnOptions.TabStop = false;
            this.btnOptions.Click += new System.EventHandler(this.btnOptions_Click);
            this.btnOptions.MouseEnter += new System.EventHandler(this.btnOptions_MouseEnter);
            this.btnOptions.MouseLeave += new System.EventHandler(this.btnOptions_MouseLeave);
            // 
            // frmLauncher
            // 
            this.AccessibleName = "";
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Fuchsia;
            this.BlendedBackground = ((System.Drawing.Bitmap)(resources.GetObject("$this.BlendedBackground")));
            this.ClientSize = new System.Drawing.Size(734, 474);
            this.Controls.Add(this.btnOptions);
            this.Controls.Add(this.lblDownloadSpeed);
            this.Controls.Add(this.newsWebbrowser);
            this.Controls.Add(this.ctrlProgressBar);
            this.Controls.Add(this.lblStatusText);
            this.Controls.Add(this.btnStartGame);
            this.Controls.Add(this.btnClose);
            this.DrawControlBackgrounds = true;
            this.EnhancedRendering = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "frmLauncher";
            this.SizeMode = GameLauncher.AlphaForm.SizeModes.Clip;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Yulgang Launcher";
            this.TransparencyKey = System.Drawing.Color.Fuchsia;
            this.Load += new System.EventHandler(this.GameLauncher_Load);
            ((System.ComponentModel.ISupportInitialize)(this.btnStartGame)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnClose)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnOptions)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox btnClose;
        public System.Windows.Forms.PictureBox btnStartGame;
        public System.Windows.Forms.Label lblStatusText;
        public ColorProgressBar.ColorProgressBar ctrlProgressBar;
        public System.Windows.Forms.WebBrowser newsWebbrowser;
        public System.Windows.Forms.Label lblDownloadSpeed;
        public System.Windows.Forms.PictureBox btnOptions;
    }
}

