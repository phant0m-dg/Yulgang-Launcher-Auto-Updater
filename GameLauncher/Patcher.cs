using Ionic.Zip;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GameLauncher
{
    class Patcher
    {
        private frmLauncher GameLauncher = null;
        
        private Settings oSettings = new Settings();

        private bool force_start = false;
        private String patch_url;

        private String launcherLocalVerFile;
        private String launcherLastVer;
        private String launcherFile;

        private String client_path;
        private String client_parameters;
        
        private String local_patch_list_path;
        private FileStream local_patch_list_stream;

        private Stack<String> patch_list = new Stack<String>();
        private Stack<String> reversedPatchListStack = new Stack<String>();
        private String patch;

        private int patch_count = 0;
        private ArrayList local_patch_list = new ArrayList();

        private int totalPatches = 0;

        public Patcher(frmLauncher oGameLauncher)
        {
            this.GameLauncher = oGameLauncher;
            this.startDownloadingSettings();
        }

        public void startDownloadingSettings()
        {
            WebClient clientPatchlist = new WebClient();
            clientPatchlist.DownloadDataCompleted += new DownloadDataCompletedEventHandler(client_DownloadSettingsCompleted);
            clientPatchlist.DownloadDataAsync(new Uri("http://sarasa.com.ar/main.ini"));
        }

        private async void client_DownloadSettingsCompleted(object sender, DownloadDataCompletedEventArgs e)
        {
            if (!e.Cancelled && e.Error == null)
            {
                if (this.loadIniData(System.Text.Encoding.UTF8.GetString(e.Result)) && this.checkPatchlist())
                {
                    string fileContents = File.ReadAllText(launcherLocalVerFile);

                    // Compare the file contents to a string
                    string comparisonString = launcherLastVer;
                    bool isEqual = fileContents.Equals(comparisonString);

                    if (!isEqual)
                    {
                        FileInfo file = new FileInfo(System.Reflection.Assembly.GetExecutingAssembly().Location);
                        File.Move(file.FullName, file.DirectoryName + "\\" + file.Name.Replace(file.Extension, ".DELETE"));

                        await this.startDownloadingLauncher();

                        string assemblyPath = Assembly.GetEntryAssembly().Location;

                        // Sleep thread to allow for file unzip
                        WaitForm waitForm = new WaitForm();
                        waitForm.Show();
                        waitForm.Update();
                        Thread.Sleep(5000);

                        Process.Start(assemblyPath);
                        Environment.Exit(0);
                    }
                    
                    if (this.reversedPatchListStack.Count > 0)
                    {
                        this.startDownloadingPatches();
                    }
                    else
                    {
                        this.patchesCompleted("Game is on latest version. You may now start the game.");
                    }
                }
            }
            else
            {
                this.setStatusText(e.Error.Message);
            }
        }

        public String getClientPath()
        {
            return client_path;
        }

        public String getClientParameters()
        {
            return client_parameters;
        }

        private bool loadIniData(String iniData)
        {
            try
            {
                this.oSettings.Load(iniData);
                this.force_start = this.oSettings.GetSetting("General", "force_Start").Contains("true") ? true : false;
                this.patch_url = this.oSettings.GetSetting("General", "patch_url");
                GameLauncher.newsWebbrowser.Navigate(this.oSettings.GetSetting("General", "news_url"));

                this.client_path = this.oSettings.GetSetting("Client", "client_path");
                this.client_parameters = this.oSettings.GetSetting("Client", "client_parameters");

                this.patch_count = Int32.Parse(this.oSettings.GetSetting("Patches", "patchcount"));
                this.local_patch_list_path = this.oSettings.GetSetting("Patches", "local_list");

                this.launcherLastVer = this.oSettings.GetSetting("LaucherVersion", "launcher_ver");
                this.launcherLocalVerFile = this.oSettings.GetSetting("LaucherVersion", "local_ver");
                this.launcherFile = this.oSettings.GetSetting("LaucherVersion", "patch");

                return true;
            }
            catch (Exception ex)
            {
                this.setStatusText(ex.Message);
                return false;
            }
        }

        private bool checkPatchlist()
        {
            try
            {
                String path_list = AppDomain.CurrentDomain.SetupInformation.ApplicationBase + this.local_patch_list_path;
                string directoryName = Path.GetDirectoryName(path_list);
                if ((directoryName.Length > 0) && (!Directory.Exists(directoryName)))
                {
                    Directory.CreateDirectory(directoryName);
                }
                using (this.local_patch_list_stream = new FileStream(path_list, FileMode.OpenOrCreate))
                {
                    using (StreamReader sr = new StreamReader(this.local_patch_list_stream))
                    {
                        while (!sr.EndOfStream)
                        {
                            local_patch_list.Add(sr.ReadLine());
                        }
                    }
                }

                for (int i = 1; i <= this.patch_count; i++)
                {
                    this.patch = this.oSettings.GetSetting("Patches", "patch" + i.ToString());
                    if (!local_patch_list.Contains(patch) && !reversedPatchListStack.Contains(patch))
                    {
                        this.patch_list.Push(patch);
                    }
                }

                pushStackAndReverse(patch);

                totalPatches = reversedPatchListStack.Count;
                return true;
            } catch(Exception ex)
            {
                this.setStatusText(ex.Message);
                return false;
            }
        }

        private Stopwatch sw = new Stopwatch();

        private WebClient clientPatches = new WebClient();
        private String currentDownload;
        private void pushStackAndReverse(string patch)
        {
            // Initiate new Stack using previous Stack data
            this.reversedPatchListStack = new Stack<string>(this.patch_list);
        }
        private void startDownloadingPatches()
        {
            clientPatches.DownloadProgressChanged += new DownloadProgressChangedEventHandler(client_DownloadProgressChanged);
            clientPatches.DownloadDataCompleted += new DownloadDataCompletedEventHandler(client_DownloadDataCompleted);
            this.currentDownload = patch_url + reversedPatchListStack.Pop();
            this.sw.Start();
            clientPatches.DownloadDataAsync(new Uri(this.currentDownload));
        }

        private async Task startDownloadingLauncher()
        {
            clientPatches.DownloadProgressChanged += new DownloadProgressChangedEventHandler(client_DownloadProgressChanged);
            clientPatches.DownloadDataCompleted += new DownloadDataCompletedEventHandler(client_DownloadDataCompleted);
            this.currentDownload = patch_url + launcherFile;
            this.sw.Start();

            Task.Run(() => { clientPatches.DownloadDataAsync(new Uri(this.currentDownload)); } );

        }

        private void client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            // Calculate download speed
            String downloadSpeed = Patcher.BytesToString(e.BytesReceived / this.sw.Elapsed.TotalSeconds);

            // Show the percentage on our label.
            int percentage = e.ProgressPercentage;

            // Update the label with how much data have been downloaded so far and the total size of the file we are currently downloading
            String totalReceived = string.Format("{0} MB / {1} MB", (e.BytesReceived / 1024d / 1024d).ToString("0.00"), (e.TotalBytesToReceive / 1024d / 1024d).ToString("0.00"));

            this.setStatusText(string.Format("{0} Downloading: {1} {2} ", this.currentFileProcessing(), Path.GetFileNameWithoutExtension(this.currentDownload), totalReceived), downloadSpeed);
            this.setProgressbar(percentage);
        }

        private String currentFileProcessing()
        {
            return "["+ (String)(this.totalPatches - this.reversedPatchListStack.Count).ToString()+"/"+this.totalPatches.ToString()+"]";
        }

        private static String BytesToString(double byteCount)
        {
            string[] suf = { "B/s", "KB/s", "MB/s", "GB/s" }; //Longs run out around EB
            if (byteCount == 0)
                return "0" + suf[0];
            long bytes = Math.Abs((long)byteCount);
            int place = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
            double num = Math.Round(bytes / Math.Pow(1024, place), 1);
            return (Math.Sign(byteCount) * num).ToString() + " "+ suf[place];
        }
        private void client_DownloadDataCompleted(object sender, DownloadDataCompletedEventArgs e)
        {
            // Reset the stopwatch.
            this.sw.Reset();
            if (!e.Cancelled && e.Error == null)
            {
                this.extractDownload(e.Result);
            } else
            {
                this.setStatusText(e.Error.Message);
            }
        }

        private int percentComplete;

        public void ExtractProgress(object sender, ExtractProgressEventArgs e)
        {
            if (e.EventType == ZipProgressEventType.Extracting_BeforeExtractEntry)
            {
                this.setStatusText(String.Format("{0} Extracting: {1}", this.currentFileProcessing(), e.CurrentEntry.FileName));
                this.setProgressbar(percentComplete);
            }
        }

        private void extractDownload(byte[] download)
        {
            new Thread(() =>
            {
                Stream stream = new MemoryStream(download);
                try
                {
                    using (ZipFile zip = ZipFile.Read(stream))
                    {
                        zip.ExtractProgress += ExtractProgress;

                        int step = 0;
                        int totalFiles = zip.Count;
                        this.percentComplete = 0;
                        foreach (ZipEntry file in zip)
                        {
                            step++;
                            file.Extract(AppDomain.CurrentDomain.SetupInformation.ApplicationBase, ExtractExistingFileAction.OverwriteSilently);

                            this.percentComplete = (int)Math.Round((double)(100 * step) / totalFiles);
                        }
                        this.addToPatchList(Path.GetFileName(this.currentDownload));
                        if (reversedPatchListStack.Count > 0)
                        {
                            this.currentDownload = patch_url + reversedPatchListStack.Pop();
                            this.sw.Start();
                            clientPatches.DownloadDataAsync(new Uri(this.currentDownload));
                        }
                        else
                            this.patchesCompleted("Game succesfully updated. You may now start the game.");
                    }
                }
                catch (Exception ex)
                {
                    this.setStatusText(ex.Message);
                }
            }).Start();
        }

        private void addToPatchList(String patch_added)
        {
            try
            {
                String path_list = AppDomain.CurrentDomain.SetupInformation.ApplicationBase + this.local_patch_list_path;
                string directoryName = Path.GetDirectoryName(path_list);
                if ((directoryName.Length > 0) && (!Directory.Exists(directoryName)))
                {
                    Directory.CreateDirectory(directoryName);
                }
                using (this.local_patch_list_stream = new FileStream(path_list, FileMode.Append))
                {
                    using (StreamWriter sr = new StreamWriter(this.local_patch_list_stream))
                    {
                        sr.WriteLine(patch_added);
                    }
                }
            }
            catch (Exception ex)
            {
                this.setStatusText(ex.Message);
            }
        }
        public void setStatusText(String text)
        {
            if (GameLauncher.IsDisposed)
                return;
            if (GameLauncher.InvokeRequired)
            {
                GameLauncher.BeginInvoke((MethodInvoker)delegate
                {
                    GameLauncher.lblStatusText.Text = text;
                    GameLauncher.lblStatusText.Refresh();
                    GameLauncher.lblDownloadSpeed.Text = "";
                    GameLauncher.lblDownloadSpeed.Refresh();
                });
            }
            else
            {
                GameLauncher.lblStatusText.Text = text;
                GameLauncher.lblStatusText.Refresh();
                GameLauncher.lblDownloadSpeed.Text = "";
                GameLauncher.lblDownloadSpeed.Refresh();
            }
        }

        private void setStatusText(String text, String downloadSpeed)
        {
            if (GameLauncher.IsDisposed)
                return;
            if (GameLauncher.InvokeRequired)
            { 
                GameLauncher.BeginInvoke((MethodInvoker)delegate
                {
                    GameLauncher.lblStatusText.Text = text;
                    GameLauncher.lblStatusText.Refresh();
                    GameLauncher.lblDownloadSpeed.Text = downloadSpeed;
                    GameLauncher.lblDownloadSpeed.Refresh();
                });
            }
            else
            {
                GameLauncher.lblStatusText.Text = text;
                GameLauncher.lblStatusText.Refresh();
                GameLauncher.lblDownloadSpeed.Text = downloadSpeed;
                GameLauncher.lblDownloadSpeed.Refresh();
            }
        }

        private void setProgressbar(int value)
        {
            if (GameLauncher.IsDisposed)
                return;
            if (GameLauncher.InvokeRequired)
            {
                GameLauncher.BeginInvoke((MethodInvoker)delegate
                {
                    GameLauncher.ctrlProgressBar.Value = value;
                    GameLauncher.ctrlProgressBar.Refresh();
                });
            }
            else
            {
                GameLauncher.ctrlProgressBar.Value = value;
                GameLauncher.ctrlProgressBar.Refresh();
            }
        }

        private void enableStartGameButton()
        {
            if (GameLauncher.IsDisposed)
                return;
            if (GameLauncher.InvokeRequired)
            {
                GameLauncher.BeginInvoke((MethodInvoker)delegate
                {
                    this.GameLauncher.btnStartGame.Visible = true;
                    this.GameLauncher.btnStartGame.Enabled = true;
                });
            }
            else
            {
                this.GameLauncher.btnStartGame.Visible = true;
                this.GameLauncher.btnStartGame.Enabled = true;
            }
        }

        private void patchesCompleted(String text)
        {
            this.enableStartGameButton();
            this.setProgressbar(100);
            this.setStatusText(text);
        }
    }
}
