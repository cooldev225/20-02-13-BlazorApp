using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TwainScanning;
using TwainScanning.Collectors;
using TwainScanning.NativeStructs;

namespace TwainUI
{
    public partial class Form1 : Form
    {
        public TwainScanning.DataSourceManager _asyncDSM = null;
        public TwainScanning.DataSource _asyncDS = null;
        public Form1()
        {
            InitializeComponent();
            bool isValid = TwainScanning.GlobalConfig.SetLicenseKey("CompanyName", "CompanyKey");
            OpenDSM();//Open data source manager method.
                      // MessageBox.Show("created form");

            HttpClient clint = new HttpClient();
            //clint.BaseAddress = new Uri("http://localhost:61027/");
            //HttpResponseMessage response = clint.GetAsync("/").Result;
            //MessageBox.Show(response.ToString());

        }
        private void OpenDSM()
        {
            TwainScanning.AppInfo info = new TwainScanning.AppInfo();//Create instance of AppInfo
            info.name = "Terminal";//Set app name
            info.manufacturer = "terminalworks";//Set manufacture name
            _asyncDSM = new TwainScanning.DataSourceManager(this.Handle, info); //Open DataSourceManager
        }

        private void btnOpenSource_Click(object sender, EventArgs e)
        {
            try
            {
                TwIdentity scanner = _asyncDSM.SelectDefaultSourceDlg();//Call twain Source dialog.
                if (scanner.Id == 0)
                {
                    MessageBox.Show("Please select scanner");
                }
                _asyncDS = _asyncDSM.OpenSource(scanner);
            }
            catch (BadRcTwainException ex)
            {
                MessageBox.Show("Bad twain return code: " + ex.ReturnCode.ToString() + "\nCondition code: " + ex.ConditionCode.ToString() + "\n" + ex.Message);
            }
        }

        private void btnAcquire_Click(object sender, EventArgs e)
        {
            try
            {
                btnAcquire.Enabled = false;

                if (String.IsNullOrEmpty(textBox1.Text))//If device name is null or empty return.
                {
                    btnAcquire.Enabled = true;
                    MessageBox.Show("Add save path");
                    return;
                }


                //  _asyncDS = _asyncDSM.OpenSource();

                _asyncDS.OnSingleImageAcquired += _asyncDS_ImageAcquired;//Event image acquired.
                ImageCollector col = new ImageCollector();
                bool showUI = false;
                _asyncDS.AcquireAsync(asyncDS_OnScanFinished, showUI, false, TwainScanning.NativeStructs.TwSX.Native, -1);//Acquire image async.       

            }
            catch (TwainScanning.BadRcTwainException ex)
            {
                btnAcquire.Enabled = true;
                MessageBox.Show("Bad twain return code: " + ex.ReturnCode.ToString() + "\nCondition code: " + ex.ConditionCode.ToString() + "\n" + ex.Message);
            }
            CloseDs();
            btnAcquire.Enabled = true;

        }
        //Close dataSource
        private void CloseDs()
        {
            if (_asyncDS != null)
            {
                _asyncDS.Dispose();
                _asyncDS = null;
            }
        }

        void asyncDS_OnScanFinished(ImageCollector collector)
        {
            btnAcquire.Enabled = true;
            if (String.IsNullOrEmpty(textBox1.Text))
                return;//If device name is null or empty return.
            if (collector != null)
            {
                string pathPDF = "";
                string pathTIFF = "";
                int i = 0;
                do
                {
                    string dir = Path.GetDirectoryName(textBox1.Text);
                    string noExtension = Path.GetFileNameWithoutExtension(textBox1.Text);
                    pathPDF = dir + "\\" + noExtension + i + ".pdf";
                    pathTIFF = dir + "\\" + noExtension + i + ".tiff";
                    i++;
                }
                while (File.Exists(pathPDF));
                collector.SaveAllToMultipagePdf(pathPDF);
                collector.SaveAllToMultipageTiff(pathTIFF);
                collector.Dispose();
            }
            CloseDs();
        }

        private void _asyncDS_ImageAcquired(object sender, DataSource.SingleImageAcquiredEventArgs e)
        {
            var imgOld = pictureBox1.Image;
            if (e.Image == null)//Check if image is null
                return;
            Bitmap img = new Bitmap((Image)e.Image);//scat image to bitmap type
            pictureBox1.Image = img;//Set image to picturebox
            pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;//Set image zoom 
            if (imgOld != null)
                imgOld.Dispose();
        }

        private void btnImgSavePath_Click(object sender, EventArgs e)
        {

            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.RestoreDirectory = true;
            saveFileDialog1.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                textBox1.Text = saveFileDialog1.FileName;
            }
        }

        private void Simple_Load(object sender, EventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}
