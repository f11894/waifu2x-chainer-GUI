using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Globalization;
using System.IO;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Win32;
using System.ComponentModel; // CancelEventArgs

using Forms = System.Windows.Forms;
namespace waifu2x_chainer_gui
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            System.Environment.CurrentDirectory = System.AppDomain.CurrentDomain.BaseDirectory;
            var dirInfo = new DirectoryInfo(App.directory);
            var langlist = dirInfo.GetFiles("UILang.*.xaml");
            string[] langcodelist = new string[langlist.Length];
            for (int i = 0; i < langlist.Length; i++)
            {
                var fn_parts = langlist[i].ToString().Split('.');
                langcodelist[i] = fn_parts[1];
            }

            foreach (var langcode in langcodelist)
            {
                MenuItem mi = new MenuItem();
                mi.Tag = langcode;
                mi.Header = langcode;
                mi.Click += new RoutedEventHandler(MenuItem_Style_Click);
                menuLang.Items.Add(mi);
            }
            foreach (MenuItem item in menuLang.Items)
            {
                if (item.Tag.ToString().Equals(CultureInfo.CurrentUICulture.Name))
                {
                    item.IsChecked = true;
                }
            }
            // 設定をファイルから読み込む

            if (Properties.Settings.Default.output_dir != "null")
            { txtDstPath.Text = Properties.Settings.Default.output_dir; }

            txtWaifu2x_chainerPath.Text = Properties.Settings.Default.waifu2x_chainer_dir;

            if (System.Text.RegularExpressions.Regex.IsMatch(
                Properties.Settings.Default.Device_ID,
                @"^\d+$",
                System.Text.RegularExpressions.RegexOptions.ECMAScript))
            {
                txtDevice.Text = Properties.Settings.Default.Device_ID;
            }

            btn128.IsChecked = true;

            if (Properties.Settings.Default.block_size == "256")
            { btn256.IsChecked = true; }
            if (Properties.Settings.Default.block_size == "128")
            { btn128.IsChecked = true; }
            if (Properties.Settings.Default.block_size == "64")
            { btn64.IsChecked = true; }
            if (Properties.Settings.Default.block_size == "32")
            { btn32.IsChecked = true; }

            btnBatch16.IsChecked = true;

            if (Properties.Settings.Default.batch_size == "32")
            { btnBatch32.IsChecked = true; }
            if (Properties.Settings.Default.batch_size == "16")
            { btnBatch16.IsChecked = true; }
            if (Properties.Settings.Default.batch_size == "8")
            { btnBatch8.IsChecked = true; }
            if (Properties.Settings.Default.batch_size == "4")
            { btnBatch4.IsChecked = true; }

            //btnCUDA.IsChecked = true;
            btnDenoise0.IsChecked = true;

            if (Properties.Settings.Default.noise_level == "3")
            { btnDenoise3.IsChecked = true; }
            if (Properties.Settings.Default.noise_level == "2")
            { btnDenoise2.IsChecked = true; }
            if (Properties.Settings.Default.noise_level == "1")
            { btnDenoise1.IsChecked = true; }
            if (Properties.Settings.Default.noise_level == "0")
            { btnDenoise0.IsChecked = true; }

            btnUpResNet10.IsChecked = true;

            if (Properties.Settings.Default.Arch == "VGG7")
            { btnVGG7.IsChecked = true; }
            if (Properties.Settings.Default.Arch == "UpConv7")
            { btnUpConv7.IsChecked = true; }
            if (Properties.Settings.Default.Arch == "ResNet10")
            { btnResNet10.IsChecked = true; }
            if (Properties.Settings.Default.Arch == "UpResNet10")
            { btnUpResNet10.IsChecked = true; }
            
            btnModeScale.IsChecked = true;

            if (Properties.Settings.Default.mode == "scale")
            { btnModeScale.IsChecked = true; }
            if (Properties.Settings.Default.mode == "noise_scale")
            { btnModeNoiseScale.IsChecked = true; }
            if (Properties.Settings.Default.mode == "noise")
            { btnModeNoise.IsChecked = true; }

            output_width.Clear();

            if (Properties.Settings.Default.output_width != 0)
            { output_width.Text = Properties.Settings.Default.output_width.ToString(); }

            output_height.Clear();

            if (Properties.Settings.Default.output_height != 0)
            { output_height.Text = Properties.Settings.Default.output_height.ToString(); }

            if (this.output_width.Text.Trim() != "")
            {
                slider_zoom.IsEnabled = false;
                slider_value.IsEnabled = false;
            }

            if (this.output_height.Text.Trim() != "")
            {
                slider_zoom.IsEnabled = false;
                slider_value.IsEnabled = false;
            }

            cbTTA.IsChecked = Properties.Settings.Default.TTAmode;
            ComboTTA_level.SelectedValue = Properties.Settings.Default.TTA_level;

            ComboColor_Mode.SelectedValue = Properties.Settings.Default.color_mode;

            checkSoundBeep.IsChecked = Properties.Settings.Default.SoundBeep;
            checkStore_output_dir.IsChecked = Properties.Settings.Default.store_output_dir;

            slider_value.Text = Properties.Settings.Default.scale_ratio;
            slider_zoom.Value = double.Parse(Properties.Settings.Default.scale_ratio);

            //cbTTA.IsChecked = false;

        }

        public static StringBuilder param_src= new StringBuilder("");
        public static StringBuilder param_dst = new StringBuilder("");
        public static StringBuilder param_informat = new StringBuilder("*.jpg *.jpeg *.png *.bmp *.tif *.tiff");
        //public static StringBuilder param_outformat = new StringBuilder("png");
        public static StringBuilder param_mag = new StringBuilder("2");
        public static StringBuilder param_denoise = new StringBuilder("");
        public static StringBuilder param_denoise2 = new StringBuilder("");
        public static StringBuilder param_arch = new StringBuilder("-a 3");
        public static StringBuilder param_model = new StringBuilder("");
        public static StringBuilder param_color = new StringBuilder("RGB");
        public static StringBuilder param_block = new StringBuilder("-l 128");
        public static StringBuilder param_batch = new StringBuilder("-b 16");
        public static StringBuilder param_mode = new StringBuilder("noise_scale");
        public static StringBuilder param_device = new StringBuilder("");
        public static StringBuilder param_tta = new StringBuilder("");
        public static StringBuilder param_WorkingDirectory = new StringBuilder("");
        public static StringBuilder param_outformat = new StringBuilder(".png");
        public static StringBuilder param_width = new StringBuilder("");
        public static StringBuilder param_height = new StringBuilder("");

        public static StringBuilder param_waifu2x_chainer_path = new StringBuilder("C:\\waifu2x-chainer");

        public static bool EventHandler_Flag = false;

        //public static StringBuilder param_tta = new StringBuilder("-t 0");
        public static Process pHandle = new Process();
        public static ProcessStartInfo psinfo = new ProcessStartInfo();

        public static StringBuilder console_buffer = new StringBuilder();

        public static bool flagAbort = false;

        public static bool queueFlag = false;

        void MainWindow_Closing(object sender, CancelEventArgs e)
        {
                
            // 設定を保存

            // 前回出力したパスを記憶する
            if (checkStore_output_dir.IsChecked == true)
            {
                if (txtDstPath.Text.Trim() != "")
                {
                    Properties.Settings.Default.output_dir = txtDstPath.Text;
                } else
                {
                    Properties.Settings.Default.output_dir = "null";
                }

            }
            else
            {
                Properties.Settings.Default.output_dir = "null";
            }


            if (Directory.Exists(this.txtWaifu2x_chainerPath.Text))
            {
                Properties.Settings.Default.waifu2x_chainer_dir = txtWaifu2x_chainerPath.Text;
            }
            else
            {
                Properties.Settings.Default.waifu2x_chainer_dir = "C:\\waifu2x-chainer";
            }

            // Properties.Settings.Default.Device_ID = txtDevice.Text;

            if (System.Text.RegularExpressions.Regex.IsMatch(
                txtDevice.Text,
                @"^\d+$",
                System.Text.RegularExpressions.RegexOptions.ECMAScript))
            {
               Properties.Settings.Default.Device_ID = txtDevice.Text;
            } else 
            {
               Properties.Settings.Default.Device_ID = "Unspecified";
            }

            Properties.Settings.Default.block_size = param_block.ToString().Replace("-l ", "");

            Properties.Settings.Default.batch_size = param_batch.ToString().Replace("-b ", "");

            Properties.Settings.Default.noise_level = param_denoise.ToString().Replace("-n ", "");

            if (param_arch.ToString().Trim() == "-a 0")
            {Properties.Settings.Default.Arch = "VGG7";}
            if (param_arch.ToString().Trim() == "-a 1")
            {Properties.Settings.Default.Arch = "UpConv7";}
            if (param_arch.ToString().Trim() == "-a 2")
            {Properties.Settings.Default.Arch = "ResNet10";}
            if (param_arch.ToString().Trim() == "-a 3")
            {Properties.Settings.Default.Arch = "UpResNet10";}
            
            Properties.Settings.Default.mode = param_mode.ToString().Replace("-m ", "");

            Properties.Settings.Default.TTAmode = Convert.ToBoolean(cbTTA.IsChecked);
            Properties.Settings.Default.TTA_level = Convert.ToDouble(ComboTTA_level.SelectedValue);
            Properties.Settings.Default.color_mode = ComboColor_Mode.SelectedValue.ToString();
            Properties.Settings.Default.SoundBeep = Convert.ToBoolean(checkSoundBeep.IsChecked);
            Properties.Settings.Default.store_output_dir = Convert.ToBoolean(checkStore_output_dir.IsChecked);


            if (System.Text.RegularExpressions.Regex.IsMatch(
                slider_value.Text,
                @"^\d+(\.\d+)?$",
                System.Text.RegularExpressions.RegexOptions.ECMAScript))
            {
               Properties.Settings.Default.scale_ratio = slider_value.Text;
            } else 
            {
               Properties.Settings.Default.scale_ratio = "2";
            }

            if (System.Text.RegularExpressions.Regex.IsMatch(
                output_width.Text,
                @"^\d+$",
                System.Text.RegularExpressions.RegexOptions.ECMAScript))
            {
                Properties.Settings.Default.output_width = double.Parse(output_width.Text);
            }
            else
            {
                Properties.Settings.Default.output_width = 0;
            }


            if (System.Text.RegularExpressions.Regex.IsMatch(
                output_height.Text,
                @"^\d+$",
                System.Text.RegularExpressions.RegexOptions.ECMAScript))
            {
                Properties.Settings.Default.output_height = double.Parse(output_height.Text);
            }
            else
            {
                Properties.Settings.Default.output_height = 0;
            }

            Properties.Settings.Default.Save();

            try
            {
                pHandle.Kill();
            }
            catch (Exception) { /*Nothing*/ }
        }

        private void OnMenuHelpClick(object sender, RoutedEventArgs e)
        {
            string msg =
                "This is a multilingual graphical user-interface\n" +
                "for the waifu2x-chainer commandline program.\n" +
                "You need a working copy of waifu2x-chainer first\n" +
                "then copy everything from the GUI archive to\n" +
                "waifu2x-chainer folder.\n" +
                "DO NOT rename any subdirectories inside waifu2x-chainer folder\n" +
                "To make a translation, copy one of the bundled xaml file\n" +
                "then edit the copy with a text editor.\n" +
                "Whenever you see a language code like en-US, change it to\n" +
                "the target language code like zh-TW, ja-JP.\n" +
                "The filename needs to be changed too.";
            MessageBox.Show(msg);
        }

        private void OnMenuVersionClick(object sender, RoutedEventArgs e)
        {
            string msg =
                "Multilingual GUI for waifu2x-chainer\n" +
                "f11894 (2018)\n" +
                "Version 1.2.1\n" +
                "BuildDate: 2 Nov,2018\n" +
                "License: Do What the Fuck You Want License";
            MessageBox.Show(msg);
        }

        private void OnBtnSrc(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fdlg= new OpenFileDialog();
            if (fdlg.ShowDialog() == true)
            {
                this.txtSrcPath.Text = fdlg.FileName;
            }
        }

        private void OnSrcClear(object sender, RoutedEventArgs e)
        {
            this.txtSrcPath.Clear();
        }

        private void OnBtnDst(object sender, RoutedEventArgs e)
        {
            SaveFileDialog fdlg = new SaveFileDialog();
            fdlg.Filter = "PNG Image | *.png";
            fdlg.DefaultExt = "png";
            if (fdlg.ShowDialog() == true)
            {
                this.txtDstPath.Text = fdlg.FileName;
            }
        }

        private void OnDstClear(object sender, RoutedEventArgs e)
        {
            this.txtDstPath.Clear();
        }

        private void OnWaifu2x_chainerPath(object sender, RoutedEventArgs e)
        {
            // フォルダー参照ダイアログのインスタンスを生成
            var dlg = new Forms.FolderBrowserDialog();

            // 説明文を設定
            // dlg.Description = "フォルダーを選択してください。";

            // ダイアログを表示
            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                // 選択されたフォルダーパスをメッセージボックスに表示
                this.txtWaifu2x_chainerPath.Text = dlg.SelectedPath;
            }
        }

        private void MenuItem_Style_Click(object sender, RoutedEventArgs e)
        {
            foreach(MenuItem item in menuLang.Items)
            {
                item.IsChecked = false;
            }
            MenuItem mi = (MenuItem)sender;
            mi.IsChecked = true;
            App.Instance.SwitchLanguage(mi.Tag.ToString());
        }

        private void On_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.All;
            }
            else
            {
                e.Effects= DragDropEffects.None;
            }
            
            e.Handled = true;
        }

        private void On_SrcDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] fn = (string[])e.Data.GetData(DataFormats.FileDrop);
                this.txtSrcPath.Text = fn[0];
            }
        }

        private void On_DstDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] fn = (string[])e.Data.GetData(DataFormats.FileDrop);
                this.txtDstPath.Text = fn[0];
            }
        }

        private void On_waifu2x_chainerDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] fn = (string[])e.Data.GetData(DataFormats.FileDrop);
                this.txtWaifu2x_chainerPath.Text = fn[0];
            }
        }

        private void OnSetModeChecked(object sender, RoutedEventArgs e)
        {
            gpDenoise.IsEnabled = true;
            if (btnModeNoise.IsChecked == false) 
            {
               output_width.IsEnabled = true;
               output_height.IsEnabled = true;
               if (this.output_height.Text.Trim() == "") if (this.output_width.Text.Trim() == "") 
               {
                  slider_zoom.IsEnabled = true;
                  slider_value.IsEnabled = true;
               }
            }

            param_mode.Clear();
            RadioButton optsrc = sender as RadioButton;
            param_mode.Append(optsrc.Tag.ToString());
            if (btnModeScale.IsChecked == true)
            { gpDenoise.IsEnabled = false;}
            
            if (btnModeNoise.IsChecked == true)
            { 
              output_width.IsEnabled = false;
              output_height.IsEnabled = false;
              slider_zoom.IsEnabled = false;
              slider_value.IsEnabled = false;
            }
        }

        private void OutputSize_TextChanged(object sender, EventArgs e)
        {

            if (btnModeNoise.IsChecked == false) if (this.output_width.Text.Trim() != "")
                {
                slider_zoom.IsEnabled = false;
                slider_value.IsEnabled = false;
            }

            if (btnModeNoise.IsChecked == false) if (this.output_height.Text.Trim() != "")
            {
                slider_zoom.IsEnabled = false;
                slider_value.IsEnabled = false;
            }
            if (btnModeNoise.IsChecked == false) if (this.output_height.Text.Trim() == "") if (this.output_width.Text.Trim() == "")
                {
                    slider_zoom.IsEnabled = true;
                    slider_value.IsEnabled = true;
                }
        }

        private void OnDenoiseChecked(object sender, RoutedEventArgs e)
        {
            param_denoise.Clear();
            RadioButton optsrc= sender as RadioButton;
            param_denoise.Append(optsrc.Tag.ToString());
        }

        private void OnArchChecked(object sender, RoutedEventArgs e)
        {
            param_arch.Clear();
            RadioButton optsrc= sender as RadioButton;
            param_arch.Append(optsrc.Tag.ToString());
        }

        /*private void OnDeviceChecked(object sender, RoutedEventArgs e)
        {
            param_device.Clear();
            RadioButton optsrc= sender as RadioButton;
            param_device.Append(optsrc.Tag.ToString());
        }
        */

        private void OnBlockChecked(object sender, RoutedEventArgs e)
        {
            param_block.Clear();
            RadioButton optsrc= sender as RadioButton;
            param_block.Append(optsrc.Tag.ToString());
        }

        private void OnBatchChecked(object sender, RoutedEventArgs e)
        {
            param_batch.Clear();
            RadioButton optsrc = sender as RadioButton;
            param_batch.Append(optsrc.Tag.ToString());
        }

        private void OnTTAChecked(object sender, RoutedEventArgs e)
        {
            param_tta.Clear();
            CheckBox optsrc= sender as CheckBox;
            if (optsrc.IsChecked.Value)
            {
                param_tta.Append(optsrc.Tag.ToString());
            }
            
        }
        
        private void OnConsoleDataRecv(object sender, DataReceivedEventArgs e)
        {
            if (!String.IsNullOrEmpty(e.Data))
            {

                console_buffer.Append(e.Data);
                console_buffer.Append(Environment.NewLine);
                // if (queueFlag) return;
                // queueFlag = true;
                Dispatcher.BeginInvoke(new Action(delegate
                {
                    // queueFlag = false;
                    CLIOutput.Focus();
                    this.CLIOutput.AppendText(e.Data);
                    this.CLIOutput.AppendText(Environment.NewLine);
                    CLIOutput.Select(CLIOutput.Text.Length, 0);
                }), System.Windows.Threading.DispatcherPriority.ApplicationIdle, null);
            }
            
        }
        
        private void OnProcessExit(object sender, EventArgs e)
        {
            try
            {
                pHandle.CancelOutputRead();
                pHandle.CancelErrorRead();
            }
            catch (Exception)
            {
                //No need to throw
                //throw;
            }

            pHandle.Close();
            Dispatcher.BeginInvoke(new Action(delegate
            {
                if (checkSoundBeep.IsChecked == true) if (this.btnRun.IsEnabled == false)
                { System.Media.SystemSounds.Beep.Play(); }
                
                this.btnAbort.IsEnabled = false;
                this.btnRun.IsEnabled = true;
                //this.CLIOutput.Text = console_buffer.ToString();

            }), System.Windows.Threading.DispatcherPriority.ApplicationIdle, null);
            flagAbort = false;
        }

        private void OnAbort(object sender, RoutedEventArgs e)
        {
            this.btnAbort.IsEnabled = false;
            try
            {
                pHandle.CancelOutputRead();
                pHandle.CancelErrorRead();
            }
            catch (Exception) { /*Nothing*/ }

            if (!pHandle.HasExited)
            {
                pHandle.Kill();

                flagAbort = true;
                this.CLIOutput.Clear();
            }
        }
        
        public void Errormessage(string x)
        { 
          System.Media.SystemSounds.Beep.Play();
          MessageBox.Show(@x);
          btnAbort.IsEnabled = false;
          btnRun.IsEnabled = true;
          return; 
        }

        private void OnRun(object sender, RoutedEventArgs e)
        {

            // Sets Source
            // The source must be a file or folder that exists
            if (File.Exists(this.txtSrcPath.Text) || Directory.Exists(this.txtSrcPath.Text))
            {
                if (this.txtSrcPath.Text.Trim() == "") //When source path is empty, replace with current folder
                {
                    param_src.Clear();
                    param_src.Append("-i ");
                    param_src.Append("\"");
                    param_src.Append(App.directory);
                    param_src.Append("\"");

                }
                else
                {
                    param_src.Clear();
                    param_src.Append("-i ");
                    param_src.Append("\"");
                    param_src.Append(this.txtSrcPath.Text);
                    param_src.Append("\"");
                    param_WorkingDirectory.Clear();
                    if (Directory.Exists(this.txtSrcPath.Text))
                    {
                        param_WorkingDirectory.Append(this.txtSrcPath.Text);
                    }
                    if (File.Exists(this.txtSrcPath.Text))
                    {
                        System.IO.DirectoryInfo hDirInfo = System.IO.Directory.GetParent(this.txtSrcPath.Text);
                        param_WorkingDirectory.Append(hDirInfo);
                    }

                }
            }
            else
            {
                Errormessage(@"The source folder or file does not exists!");
                return;
            }


            // waifu2x-chainerのパスを設定する
            param_waifu2x_chainer_path.Clear();
            if (File.Exists(this.txtWaifu2x_chainerPath.Text + "\\waifu2x.py"))
            {
                param_waifu2x_chainer_path.Append("\"" + this.txtWaifu2x_chainerPath.Text + "\\waifu2x.py\"");
            }
            else
            {
                Errormessage("waifu2x-chainer folder is missing!");
                return;
            }

            // モデルフォルダを決定する
            param_model.Clear();
            if (param_arch.ToString().Trim() == "-a 0")
            { param_model.Append("-d \"" + this.txtWaifu2x_chainerPath.Text + "\\models\\vgg7\""); }
            if (param_arch.ToString().Trim() == "-a 1")
            { param_model.Append("-d \"" + this.txtWaifu2x_chainerPath.Text + "\\models\\upconv7\""); }
            if (param_arch.ToString().Trim() == "-a 2")
            { param_model.Append("-d \"" + this.txtWaifu2x_chainerPath.Text + "\\models\\resnet10\""); }
            if (param_arch.ToString().Trim() == "-a 3")
            { param_model.Append("-d \"" + this.txtWaifu2x_chainerPath.Text + "\\models\\upresnet10\""); }
            
            param_color.Clear();
            param_color.Append("-c " + ComboColor_Mode.SelectedValue.ToString().ToLowerInvariant());

            param_mag.Clear();
            param_mag.Append("-s ");
            param_mag.Append(this.slider_value.Text);

            // 数字が入力されてなかったらクリアする

            param_width.Clear();
            if (!System.Text.RegularExpressions.Regex.IsMatch(
            output_width.Text,
            @"^\d+$",
            System.Text.RegularExpressions.RegexOptions.ECMAScript))
            {
                output_width.Clear();

            } else
            {
                param_mag.Clear();
                param_width.Append("-W " + this.output_width.Text);
            }

            param_height.Clear();
            if (!System.Text.RegularExpressions.Regex.IsMatch(
                output_height.Text,
                @"^\d+$",
                System.Text.RegularExpressions.RegexOptions.ECMAScript))
            {
                output_height.Clear();
            } else
            {
                param_mag.Clear();
                param_height.Append("-H " + this.output_height.Text);
            }

            if (System.Text.RegularExpressions.Regex.IsMatch(
                txtDevice.Text,
                @"^(\d+)$",
                System.Text.RegularExpressions.RegexOptions.ECMAScript))
            {
                param_device.Clear();
                param_device.Append("-g ");
                param_device.Append(txtDevice.Text);
            }
            else
            {
                param_device.Clear();
            }

            param_tta.Clear();
            if (cbTTA.IsChecked == true)
            {
                param_tta.Append("-t -T ");
                param_tta.Append(ComboTTA_level.SelectedValue.ToString());
            }

            // logをクリアする
            this.CLIOutput.Clear();

            // Set Destination
            if (this.txtDstPath.Text.Trim() == "")
            {
                param_dst.Clear();
            }
            else
            {
                param_dst.Clear();
                param_dst.Append("-o ");
                param_dst.Append("\"");
                param_dst.Append(this.txtDstPath.Text);
                param_dst.Append("\"");
            }

            // Set input format
            // param_informat.Clear();
            //param_informat.Append("-l ");
            // param_informat.Append(this.txtExt.Text);
            //param_informat.Append(@":");
            //param_informat.Append(this.txtExt.Text.ToUpper());

            // Set output format
            //param_outformat.Clear();
            //param_outformat.Append("-e ");
            //param_outformat.Append(this.txtOExt.Text);

            // Set scale ratio

            param_denoise2.Clear();
            param_denoise2.Append(param_denoise.ToString());

            // Set mode
            if (param_mode.ToString() == "-m noise")
            {
                param_width.Clear();
                param_height.Clear();
                param_mag.Clear();
            }
            if (param_mode.ToString() == "-m scale")
            {
                param_denoise2.Clear();
            }

            this.btnRun.IsEnabled = false;
            this.btnAbort.IsEnabled = true;

            // Assemble parameters
            string full_param = String.Join(" ", param_waifu2x_chainer_path.ToString(),
                param_src.ToString(),
                param_dst.ToString(),
                // param_informat.ToString(),
                // param_outformat.ToString(),
                param_mode.ToString(),
                param_mag.ToString(),
                param_width.ToString(),
                param_height.ToString(),
                param_denoise2.ToString(),
                param_arch.ToString(),
                param_model.ToString(),
                param_color.ToString(),
                param_block.ToString(),
                param_batch.ToString(),
                param_device.ToString(),
                param_tta.ToString());
            // Setup ProcessStartInfo
            psinfo.FileName = "python";
            psinfo.Arguments = full_param;
            psinfo.RedirectStandardError = true;
            psinfo.RedirectStandardOutput = true;
            psinfo.UseShellExecute = false;
            psinfo.WorkingDirectory = param_WorkingDirectory.ToString();
            psinfo.CreateNoWindow = true;
            psinfo.WindowStyle = ProcessWindowStyle.Hidden;
            pHandle.StartInfo = psinfo;
            pHandle.EnableRaisingEvents = true;
            if (EventHandler_Flag == false)
            {
                 pHandle.OutputDataReceived += new DataReceivedEventHandler(OnConsoleDataRecv);
                 pHandle.ErrorDataReceived += new DataReceivedEventHandler(OnConsoleDataRecv);
                 pHandle.Exited += new EventHandler(OnProcessExit);
                 EventHandler_Flag = true;
             }

            // Starts working
            console_buffer.Clear();

            try
            {
                //MessageBox.Show(full_param);
                bool pState = pHandle.Start();
            }
            catch (Exception)
            {
                try
                {
                    pHandle.Kill();
                }
                catch (Exception) { /*Nothing*/ }
                Errormessage("Failed to start Python.");
                //throw;
            }

            Dispatcher.BeginInvoke(new Action(delegate
            {
                CLIOutput.Focus();
                this.CLIOutput.AppendText("WorkingDirectory " + param_WorkingDirectory.ToString());
                this.CLIOutput.AppendText(Environment.NewLine);
                this.CLIOutput.AppendText("python " + full_param.ToString());
                this.CLIOutput.AppendText(Environment.NewLine);
                this.CLIOutput.AppendText(Environment.NewLine);
                CLIOutput.Select(CLIOutput.Text.Length, 0);
            }), System.Windows.Threading.DispatcherPriority.ApplicationIdle, null);

            try
            {
                pHandle.BeginOutputReadLine();
            }
            catch (Exception)
            {
                this.CLIOutput.Clear();
                this.CLIOutput.Text = "BeginOutputReadLine crashed...\n";
            }

            try
            {
                pHandle.BeginErrorReadLine();
            }
            catch (Exception)
            {
                this.CLIOutput.Clear();
                this.CLIOutput.Text = "BeginErrorReadLine crashed...\n";
            }

            //pHandle.BeginErrorReadLine();
            //MessageBox.Show("Some parameters do not mix well and crashed...");

            //pHandle.WaitForExit();
            /*
            pHandle.CancelOutputRead();
            pHandle.Close();
            this.btnAbort.IsEnabled = false;
            this.btnRun.IsEnabled = true;
            this.CLIOutput.Text = console_buffer.ToString();
            */

        }
    }
}
