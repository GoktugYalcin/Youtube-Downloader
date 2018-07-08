using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using YoutubeExtractor;
using cs_ffmpeg_mp3_converter;
using System.IO;

namespace Youtube_Player
{
    public partial class Form1 : Form
    {
        public Form1()
        {

            InitializeComponent();
            comboBox1.Items.Add("1080");
            comboBox1.Items.Add("720");
            comboBox1.Items.Add("480");
            comboBox1.Items.Add("360");
        }
        public string giris;
        public string cikis;
        private void Form1_Load(object sender, EventArgs e)
        {
            comboBox1.SelectedIndex = 0;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            progressBar1.Minimum = 0;
            progressBar1.Maximum = 100;
            IEnumerable<VideoInfo> videolar = DownloadUrlResolver.GetDownloadUrls(textBox1.Text);
            var allowedResolutions = new List<int>() { 1080, 720, 480, 360 };
            VideoInfo video = videolar.OrderByDescending(info => info.Resolution)
                              .Where(info => allowedResolutions.Contains(info.Resolution))
                              .First(info => info.VideoType == VideoType.Mp4);
            if (video.RequiresDecryption)
                DownloadUrlResolver.DecryptDownloadUrl(video);
            string yol = "";
            SaveFileDialog kaydetme = new SaveFileDialog();
            kaydetme.Filter = "Video Dosyası |*.mp4";
            kaydetme.Title = "Kaydedilecek yeri seçin.";
            kaydetme.ShowDialog();
            string KaydetmeDosyaYolu = kaydetme.FileName;
            VideoDownloader indirici = new VideoDownloader(video, KaydetmeDosyaYolu);
            MessageBox.Show(Path.GetDirectoryName(KaydetmeDosyaYolu) + "\\" + Path.GetFileNameWithoutExtension(KaydetmeDosyaYolu) + ".mp3");
            indirici.DownloadProgressChanged += Downloader_DownloadProgressChanged;
            indirici.DownloadFinished += Downloader_DownloadFinished;
            Thread thread = new Thread(() => { indirici.Execute(); }) { IsBackground = true };
            giris = KaydetmeDosyaYolu;
            cikis = KaydetmeDosyaYolu.Substring(0, giris.IndexOf("."));
            thread.Start();
        }

        private void Downloader_DownloadFinished(object sender, EventArgs e)
        {
            MessageBox.Show("İndirme Başarıyla Tamamlandı!", "Bildirim");
        }

        private void Downloader_DownloadProgressChanged(object sender, ProgressEventArgs e)
        {
            Invoke(new MethodInvoker(delegate ()
            {
                progressBar1.Value = (int)e.ProgressPercentage;
                lblPercentage.Text = $"{string.Format("{0:0.##}", e.ProgressPercentage)}%";
                progressBar1.Update();
            }));
        }

        private void button2_Click(object sender, EventArgs e)
        {
            var videoConv = new NReco.VideoConverter.FFMpegConverter();
            videoConv.ConvertMedia(giris, cikis + ".mp3", "mp3");
            File.Delete(giris);
        }
    }
}
