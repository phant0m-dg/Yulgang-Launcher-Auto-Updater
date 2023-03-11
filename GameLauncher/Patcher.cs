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
using System.Text;

namespace GameLauncher
{
    public class Patcher
    {
        private readonly frmLauncher GameLauncher = null;
        
        private readonly Settings oSettings = new Settings();
        private readonly Settings oSettingsLauncherUpdate = new Settings();
        private readonly Uri configURL = new Uri("http://sarasa.com.ar/main.ini");

        private FileStream local_patch_list_stream;

        private readonly Stack<string> patch_list = new Stack<string>();
        private Stack<string> reversedPatchListStack = new Stack<string>();

        private readonly ArrayList local_patch_list = new ArrayList();

        private readonly Stopwatch sw = new Stopwatch();

        private readonly WebClient clientPatches = new WebClient();
        private readonly WebClient launcherPatches = new WebClient();

        private readonly string localVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();
        private string patch_url;

        private string launcherLastVer;
        private string launcherFile;

        private string client_path;
        private string client_parameters;

        private string currentDownload;
        private string patch;

        private string local_patch_list_path;

        private int patch_count = 0;
        private int totalPatches = 0;

        public Patcher(frmLauncher oGameLauncher)
        {
            GameLauncher = oGameLauncher;
            startDownloadingSettingsLauncher();
        }

        public void startDownloadingSettings()
        {
            WebClient clientPatchlist = new WebClient();
            clientPatchlist.DownloadDataCompleted += new DownloadDataCompletedEventHandler(client_DownloadSettingsCompleted);
            clientPatchlist.DownloadDataAsync(configURL);
        }

        public void startDownloadingSettingsLauncher()
        {
            WebClient launcherPatchlist = new WebClient();
            launcherPatchlist.DownloadDataCompleted += new DownloadDataCompletedEventHandler(launcher_DownloadSettingsCompleted);
            launcherPatchlist.DownloadDataAsync(configURL);
        }

        private async void launcher_DownloadSettingsCompleted(object sender, DownloadDataCompletedEventArgs e)
        {
            if (!e.Cancelled && e.Error == null)
            {
                if (loadIniDataLauncherUpdate(Encoding.UTF8.GetString(e.Result)))
                {
                    if (launcherLastVer != localVersion)
                    {
                        FileInfo file = new FileInfo(Assembly.GetExecutingAssembly().Location);
                        File.Move(file.FullName, file.DirectoryName + "\\" + file.Name.Replace(file.Extension, ".DELETE"));

                        await startDownloadingLauncher();

                        // Wait Splashscreen
                        WaitForm waitForm = new WaitForm();
                        waitForm.Show();
                        waitForm.Update();

                        try
                        {
                            Thread.Sleep(5000);

                            Application.ExitThread();
                            Application.Exit();
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.ToString());
                        }
                    }
                    else // If launcher is updated continue to client update
                    {
                        startDownloadingSettings();
                    }
                }
            }
            else
            {
                setStatusText(e.Error.Message);
            }
        }

        private void client_DownloadSettingsCompleted(object sender, DownloadDataCompletedEventArgs e)
        {
            if (!e.Cancelled && e.Error == null)
            {
                if (loadIniData(Encoding.UTF8.GetString(e.Result)) && checkPatchlist())
                {
                    if (reversedPatchListStack.Count > 0)
                    {
                        startDownloadingPatches();
                    }
                    else
                    {
                        patchesCompleted("Game is on latest version. You may now start the game.");
                    }
                }
            }
            else
            {
                setStatusText(e.Error.Message);
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
                oSettings.Load(iniData);
                patch_url = oSettings.GetSetting("General", "patch_url");
                GameLauncher.newsWebbrowser.Navigate(oSettings.GetSetting("General", "news_url"));

                client_path = oSettings.GetSetting("Client", "client_path");
                client_parameters = oSettings.GetSetting("Client", "client_parameters");

                patch_count = Int32.Parse(oSettings.GetSetting("Patches", "patchcount"));
                local_patch_list_path = oSettings.GetSetting("Patches", "local_list");

                return true;
            }
            catch (Exception ex)
            {
                setStatusText(ex.Message);
                return false;
            }
        }

        private bool loadIniDataLauncherUpdate(String iniData)
        {
            try
            {
                oSettingsLauncherUpdate.Load(iniData);
                patch_url = oSettingsLauncherUpdate.GetSetting("General", "patch_url");
                launcherLastVer = oSettingsLauncherUpdate.GetSetting("LaucherVersion", "launcher_ver");
                launcherFile = oSettingsLauncherUpdate.GetSetting("LaucherVersion", "patch");

                return true;
            }
            catch (Exception ex)
            {
                setStatusText(ex.Message);
                return false;
            }
        }

        private bool checkPatchlist()
        {
            try
            {
                String path_list = AppDomain.CurrentDomain.SetupInformation.ApplicationBase + local_patch_list_path;
                string directoryName = Path.GetDirectoryName(path_list);
                if ((directoryName.Length > 0) && (!Directory.Exists(directoryName)))
                {
                    Directory.CreateDirectory(directoryName);
                }
                using (local_patch_list_stream = new FileStream(path_list, FileMode.OpenOrCreate))
                {
                    using (StreamReader sr = new StreamReader(local_patch_list_stream))
                    {
                        while (!sr.EndOfStream)
                        {
                            local_patch_list.Add(sr.ReadLine());
                        }
                    }
                }

                for (int i = 1; i <= patch_count; i++)
                {
                    patch = oSettings.GetSetting("Patches", "patch" + i.ToString());
                    if (!local_patch_list.Contains(patch) && !reversedPatchListStack.Contains(patch))
                    {
                        patch_list.Push(patch);
                    }
                }

                pushStackAndReverse(patch);

                totalPatches = reversedPatchListStack.Count;
                return true;
            } catch(Exception ex)
            {
                setStatusText(ex.Message);
                return false;
            }
        }
        private void pushStackAndReverse(string patch)
        {
            // Initiate new Stack using previous Stack data
            reversedPatchListStack = new Stack<string>(patch_list);
        }
        private void startDownloadingPatches()
        {
            clientPatches.DownloadProgressChanged += new DownloadProgressChangedEventHandler(client_DownloadProgressChanged);
            clientPatches.DownloadDataCompleted += new DownloadDataCompletedEventHandler(client_DownloadDataCompleted);
            currentDownload = patch_url + reversedPatchListStack.Pop();
            sw.Start();
            clientPatches.DownloadDataAsync(new Uri(currentDownload));
        }

        private async Task startDownloadingLauncher()
        {
            launcherPatches.DownloadProgressChanged += new DownloadProgressChangedEventHandler(client_DownloadProgressChanged);
            launcherPatches.DownloadDataCompleted += new DownloadDataCompletedEventHandler(client_DownloadDataCompleted);
            //currentDownload = "http://sarasa.com.ar/" + launcherFile;
            currentDownload = patch_url + launcherFile;
            sw.Start();

            await Task.Run(() => { launcherPatches.DownloadDataAsync(new Uri(currentDownload)); } );
        }

        private void client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            // Calculate download speed
            String downloadSpeed = Patcher.BytesToString(e.BytesReceived / sw.Elapsed.TotalSeconds);

            // Show the percentage on our label.
            int percentage = e.ProgressPercentage;

            // Update the label with how much data have been downloaded so far and the total size of the file we are currently downloading
            String totalReceived = string.Format("{0} MB / {1} MB", (e.BytesReceived / 1024d / 1024d).ToString("0.00"), (e.TotalBytesToReceive / 1024d / 1024d).ToString("0.00"));

            setStatusText(string.Format("{0} Downloading: {1} {2} ", currentFileProcessing(), Path.GetFileNameWithoutExtension(currentDownload), totalReceived), downloadSpeed);
            setProgressbar(percentage);
        }

        private String currentFileProcessing()
        {
            return "["+ (String)(totalPatches - reversedPatchListStack.Count).ToString()+"/"+totalPatches.ToString()+"]";
        }

        private static String BytesToString(double byteCount)
        {
            string[] suf = { "B/s", "KB/s", "MB/s", "GB/s" };
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
            sw.Reset();
            if (!e.Cancelled && e.Error == null)
            {
                extractDownload(e.Result);
            } else
            {
                setStatusText(e.Error.Message);
            }
        }

        private int percentComplete;

        public void ExtractProgress(object sender, ExtractProgressEventArgs e)
        {
            if (e.EventType == ZipProgressEventType.Extracting_BeforeExtractEntry)
            {
                setStatusText(String.Format("{0} Extracting: {1}", currentFileProcessing(), e.CurrentEntry.FileName));
                setProgressbar(percentComplete);
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
                        percentComplete = 0;
                        foreach (ZipEntry file in zip)
                        {
                            step++;
                            file.Extract(AppDomain.CurrentDomain.SetupInformation.ApplicationBase, ExtractExistingFileAction.OverwriteSilently);

                            percentComplete = (int)Math.Round((double)(100 * step) / totalFiles);
                        }
                        addToPatchList(Path.GetFileName(currentDownload));
                        if (reversedPatchListStack.Count > 0)
                        {
                            currentDownload = patch_url + reversedPatchListStack.Pop();
                            sw.Start();
                            clientPatches.DownloadDataAsync(new Uri(currentDownload));
                        }
                        else
                            this.patchesCompleted("Game succesfully updated. You may now start the game.");
                    }
                }
                catch (Exception ex)
                {
                    setStatusText(ex.Message);
                }
            }).Start();
        }

        private void addToPatchList(String patch_added)
        {
            try
            {
                String path_list = AppDomain.CurrentDomain.SetupInformation.ApplicationBase + local_patch_list_path;
                string directoryName = Path.GetDirectoryName(path_list);
                if ((directoryName.Length > 0) && (!Directory.Exists(directoryName)))
                {
                    Directory.CreateDirectory(directoryName);
                }
                using (local_patch_list_stream = new FileStream(path_list, FileMode.Append))
                {
                    using (StreamWriter sr = new StreamWriter(local_patch_list_stream))
                    {
                        sr.WriteLine(patch_added);
                    }
                }
            }
            catch (Exception ex)
            {
                setStatusText(ex.Message);
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
