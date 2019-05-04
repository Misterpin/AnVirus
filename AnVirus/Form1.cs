using MetroFramework.Forms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Media;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using VirusTotalNET;
using VirusTotalNET.ResponseCodes;
using VirusTotalNET.Results;

namespace WindowsFormsApp2
{
    public partial class Form1 : MetroForm
    {
        public Form1()
        {
            InitializeComponent();
        }

        private async void DoLogicString(string text)
        {
            listView1.Items.Clear();
            byte[] virus = Encoding.ASCII.GetBytes(text);
            FileReport fr = null;
            try
            {
                fr = await ScanBytesAsync(virus);
            }
            catch (Exception e)
            {
                metroLabel3.Text = "Лимит скана - 4 раза в минуту!";
                Console.Beep();
                return;
            }
            ReportStructure structure = null;
            if (fr != null)
            {
                structure = ReportStructure.FromReport(fr);
            }
            if (structure != null)
            {
                int i = 0;
                foreach (KeyValuePair<string, bool> pair in structure.ScanResults)
                {
                    ListViewItem item = new ListViewItem(pair.Key);
                    item.SubItems.Add(pair.Value ? "Имеются" : "Нет вирусов");
                    item.SubItems.Add(structure.ScanDate.ToString());
                    item.SubItems.Add(structure.VirNames[i]);
                    listView1.Items.Add(item);
                    i++;
                }
                int YesCount = 0;
                int NoCount = 0;
                foreach(bool vir in structure.ScanResults.Values)
                {
                    if(vir)
                    {
                        YesCount++;
                    } else
                    {
                        NoCount++;
                    }
                }
                if(YesCount > NoCount)
                {
                    metroLabel3.ForeColor = Color.Red;
                    metroLabel3.Text = "вирус!";
                    Console.Beep();
                }
            }
        }

        private async void DoLogicAsync(string path)
        {
            listView1.Items.Clear();
            FileReport fr = null;
            try
            {
                fr = await ScanFileAsync(path);
            } catch(Exception e)
            {
                metroLabel3.Text = "Лимит скана - 4 раза в минуту!";
                Console.Beep();
                return;
            }
            ReportStructure structure = null;
            if (fr != null) {
                structure = ReportStructure.FromReport(fr);
            }
            if (structure != null) {
                int i = 0;
                foreach(KeyValuePair<string, bool> pair in structure.ScanResults)
                {
                    ListViewItem item = new ListViewItem(pair.Key);
                    item.SubItems.Add(pair.Value ? "Да" : "Нет");
                    item.SubItems.Add(structure.ScanDate.ToString());
                    item.SubItems.Add(structure.VirNames[i]);
                    listView1.Items.Add(item);
                    i++;
                }
                int YesCount = 0;
                int NoCount = 0;
                foreach (bool vir in structure.ScanResults.Values)
                {
                    if (vir)
                    {
                        YesCount++;
                    }
                    else
                    {
                        NoCount++;
                    }
                }
                if (YesCount > NoCount)
                {
                    metroLabel3.ForeColor = Color.Red;
                    metroLabel3.Text = "вирус!";
                    Console.Beep();
                }
            }
            
        }

        private async Task<FileReport> ScanBytesAsync(byte[] bytes)
        {
            VirusTotal virusTotal = new VirusTotal("571adb9ec9c3d0614f1cf16ef8da0429b901eca6aeed8c84653b0e7f6ddf5da4");
            virusTotal.UseTLS = true;
            FileReport report = await virusTotal.GetFileReportAsync(bytes);
            bool hasFileBeenScannedBefore = report.ResponseCode == FileReportResponseCode.Present;
            if (hasFileBeenScannedBefore)
            {
                metroLabel3.ForeColor = Color.Green;
                metroLabel3.Text = "успешно";
                return report;
            }
            else
            {
                ScanResult fileResult = await virusTotal.ScanFileAsync(bytes, "Eicar.txt");
                metroLabel3.ForeColor = Color.Green;
                metroLabel3.Text = "идет сканирование. Подождите приблизительно 2 минуты и сканируйте снова!";
                report = null;
                return report;
            }
        }

        private async Task<FileReport> ScanFileAsync(string path)
        {
            FileInfo info = new FileInfo(path);
            VirusTotal vt = new VirusTotal("571adb9ec9c3d0614f1cf16ef8da0429b901eca6aeed8c84653b0e7f6ddf5da4");
            vt.UseTLS = true;
            FileReport fileReport = await vt.GetFileReportAsync(info);
            bool hasFileBeenScannedBefore = fileReport.ResponseCode == FileReportResponseCode.Present;
            if (!hasFileBeenScannedBefore)
            {
                ScanResult fileResult = await vt.ScanFileAsync(info);
                metroLabel3.ForeColor = Color.Green;
                metroLabel3.Text = "идет сканирование. Подождите приблизительно 2 минуты и сканируйте снова!";
                return null;
            }
            else
            {
                metroLabel3.ForeColor = Color.Green;
                metroLabel3.Text = "успешно";
                return fileReport;
            }
        }

        private void metroButton1_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.InitialDirectory = "c:\\";
            openFileDialog1.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
            openFileDialog1.FilterIndex = 2;
            openFileDialog1.RestoreDirectory = true;
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                string path = openFileDialog1.FileName;
                metroTextBox1.Text = path;
            }
        }

        private void kryptonButton1_Click(object sender, EventArgs e)
        {
            if(string.IsNullOrEmpty(metroTextBox1.Text) || string.IsNullOrEmpty(metroTextBox1.Text))
            {
                metroLabel3.ForeColor = Color.Red;
                metroLabel3.Text = "путь файла не может быть пустым!";
                return;
            }
            if(!File.Exists(metroTextBox1.Text))
            {
                metroLabel3.ForeColor = Color.Red;
                metroLabel3.Text = "указанный файл не существует!";
                return;
            }
            if (!CheckInternetConnection())
            {
                metroLabel3.ForeColor = Color.Red;
                metroLabel3.Text = "нет соединения!";
                Console.Beep();
                return;
            }
            DoLogicAsync(metroTextBox1.Text);
        }

        private void kryptonButton2_Click(object sender, EventArgs e)
        {
            if(string.IsNullOrEmpty(metroTextBox2.Text) || string.IsNullOrEmpty(metroTextBox2.Text))
            {
                metroLabel3.ForeColor = Color.Red;
                metroLabel3.Text = "строка не может быть пустой!";
                return;
            }
            if(!CheckInternetConnection())
            {
                metroLabel3.ForeColor = Color.Red;
                metroLabel3.Text = "нет соединения!";
                Console.Beep();
                return;
            }
            DoLogicString(metroTextBox2.Text);
        }

        private void kryptonButton3_Click(object sender, EventArgs e)
        {
            metroTextBox2.Text = @"X5O!P%@AP[4\PZX54(P^)7CC)7}$EICAR-STANDARD-ANTIVIRUS-TEST-FILE!$H+H*";
        }

        private bool CheckInternetConnection()
        {
            try
            {
                using (var client = new WebClient())
                using (var stream = client.OpenRead("http://www.google.com"))
                {
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }
    }
}
