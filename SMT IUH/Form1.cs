using System;
using System.Data;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using ActUtlTypeLib;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using Emgu.Util;
using Emgu.CV.UI;
using Emgu.CV.XFeatures2D;  
using Emgu.CV.Cuda;
using Emgu.CV.XPhoto;   
using IronXL;


namespace SMT_IUH
{
    public partial class Form1 : Form
    {
        public ActUtlType plc = new ActUtlType();
        ushort val1, val2;

        float jog_xy_speed = 2000;
        float jog_z_speed = 1000;

        float run_xy_speed = 9000;
        float run_z_speed = 4000;

        float buoc_di_chuyen = 0.25f;

        float vi_buoc_z = 0.05f;
        float vi_tri_x;
        float vi_tri_y;
        float vi_tri_z;

        float vi_tri_feeder_1_x;
        float vi_tri_feeder_1_y;
        float vi_tri_feeder_1_z;

        float vi_tri_feeder_2_x;
        float vi_tri_feeder_2_y;
        float vi_tri_feeder_2_z;

        float vi_tri_feeder_3_x;
        float vi_tri_feeder_3_y;
        float vi_tri_feeder_3_z;

        float vi_tri_feeder_4_x;
        float vi_tri_feeder_4_y;
        float vi_tri_feeder_4_z;

        float vi_tri_camera_x;
        float vi_tri_camera_y;
        float vi_tri_camera_z;

        float vi_tri_goc_pcb_x;
        float vi_tri_goc_pcb_y;
        float vi_tri_goc_pcb_z;

        float toa_do_hien_tai_x;
        float toa_do_hien_tai_y;
        float toa_do_hien_tai_z;

        float khoang_cach_2_dau_x;
        float khoang_cach_2_dau_y;
        float khoang_cach_2_dau_z;

        float[] toa_do_chip_x;
        float[] toa_do_chip_y;
        float[] toa_do_pad_x;
        float[] toa_do_pad_y;
        int[] loai_chip;
        string[] ten_chip;
        int tong_so_chip;
        int tong_so_pad;

        int buoc_chay = 0;
        int dem_chip = 0;
        int dem_pad = 0;

        int steps = 0; // step 0 - bom keo, step 1 pick and place

        string fileName = "";

        int goc_xoay_chip_1;
        int goc_xoay_chip_2;
        int goc_xoay_chip_3;
        int goc_xoay_chip_4;

        static int fileIndex = 0;

        public Form1()
        {
            InitializeComponent();
        }

        private void buttonKetNoi_Click(object sender, EventArgs e)
        {
            plc.ActLogicalStationNumber = 1;

            var error = plc.Open();

            if (error == 0)
            {
                labelStatus.Text = "Đã kết nối";
            }
            else
            {
                labelStatus.Text = "Chưa kết nối";
            }

            timerStartStop.Start();
        }

        private void buttonNgat_Click(object sender, EventArgs e)
        {
            if (plc != null)
            {
                plc.SetDevice("M0", 0);
                plc.Close();
                labelStatus.Text = "Chưa kết nối";
            }

            buttonMoveX.Enabled = false;
            buttonMoveY.Enabled = false;
            buttonMoveZ.Enabled = false;
            buttonXtang.Enabled = false;
            buttonYtang.Enabled = false;
            buttonZtang.Enabled = false;
            buttonXgiam.Enabled = false;
            buttonYgiam.Enabled = false;
            buttonZgiam.Enabled = false;
        }

        private void buttonStartPLC_Click(object sender, EventArgs e)
        {
            if (plc != null)
            {
                Image tempImage = Image.FromFile(@"D:\XLA C#\PICTURE\Pic_2024_12_09_165604_1.bmp");
                Bitmap tempBitmap = new Bitmap(tempImage);
                pictureBoxCam.Image = tempBitmap;
                tempImage.Dispose();
                plc.SetDevice("M0", 1);
                labelStatus.Text = "Máy chạy";

                FloatConvertToTwoInt16(jog_xy_speed); // toc do
                plc.SetDevice("D120", val1);
                plc.SetDevice("D121", val2);
                plc.SetDevice("D130", val1);
                plc.SetDevice("D131", val2);
                plc.SetDevice("D204", 2000);
                timerStartStop.Start();
            }

        }

        private void buttonStopPLC_Click(object sender, EventArgs e)
        {
            timerStartStop.Stop();
            if (plc != null)
            {
                plc.SetDevice("M0", 0);
                plc.SetDevice("Y6", 0);
                plc.SetDevice("Y7", 0);
                plc.SetDevice("Y10", 0);
                plc.SetDevice("Y11", 0);
                plc.SetDevice("Y12", 0);
                plc.SetDevice("Y13", 0);
                labelStatus.Text = "Máy tắt";
            }

            buttonMoveX.Enabled = false;
            buttonMoveY.Enabled = false;
            buttonMoveZ.Enabled = false;
            buttonXtang.Enabled = false;
            buttonYtang.Enabled = false;
            buttonZtang.Enabled = false;
            buttonXgiam.Enabled = false;
            buttonYgiam.Enabled = false;
            buttonZgiam.Enabled = false;

            steps = 0;
            dem_chip = 0;
            dem_pad = 0;

            timerRun.Stop();
        }

        void goToHome()
        {
            plc.SetDevice("M20", 1);
            plc.SetDevice("M30", 1);
            plc.SetDevice("D202", 500);
            plc.SetDevice("D200", 1200);
            plc.SetDevice("M80", 1);

            Thread.Sleep(600);
            plc.SetDevice("M20", 0);
            plc.SetDevice("M30", 0);
            Thread.Sleep(100);

            plc.SetDevice("D200", -6000);
            plc.SetDevice("M80", 1);
            plc.SetDevice("M40", 1);
            plc.SetDevice("M50", 1);
            Thread.Sleep(100);
            plc.SetDevice("M40", 0);
            plc.SetDevice("M50", 0);

            int x21 = 1, x22 = 1;

            while (x21 == 1 || x22 == 1)
            {
                plc.GetDevice("X21", out x21);
                plc.GetDevice("X22", out x22);
                Thread.Sleep(50);
            }

            Thread.Sleep(1000);

            FloatConvertToTwoInt16(run_xy_speed); // toc do
            plc.SetDevice("D160", val1);
            plc.SetDevice("D161", val2);
            plc.SetDevice("D170", val1);
            plc.SetDevice("D171", val2);

            FloatConvertToTwoInt16(5000); // vi tri zero
            plc.SetDevice("D162", val1);
            plc.SetDevice("D163", val2);
            plc.SetDevice("D172", val1);
            plc.SetDevice("D173", val2);

            plc.SetDevice("M60", 1);
            plc.SetDevice("M70", 1);
            Thread.Sleep(1000);
            plc.SetDevice("M60", 0);
            plc.SetDevice("M70", 0);

            plc.SetDevice("D202", 3200);

            vi_tri_x = 5;
            vi_tri_y = 5;
            vi_tri_z = 0;

            textBoxViTriX.Text = vi_tri_x.ToString("0.000");
            textBoxViTriY.Text = vi_tri_y.ToString("0.000");
            textBoxViTriZ.Text = vi_tri_z.ToString("0.000");

            buttonMoveX.Enabled = true;
            buttonMoveY.Enabled = true;
            buttonMoveZ.Enabled = true;
            buttonXtang.Enabled = true;
            buttonYtang.Enabled = true;
            buttonZtang.Enabled = true;
            buttonXgiam.Enabled = true;
            buttonYgiam.Enabled = true;
            buttonZgiam.Enabled = true;

            toa_do_hien_tai_x = 5;
            toa_do_hien_tai_y = 5;
            toa_do_hien_tai_z = 0;

            if (steps == 1)
            {
                Thread.Sleep(1000);
                timerRun.Start();
            }
        }

        private void buttonGoToHome_Click(object sender, EventArgs e)
        {
            if (plc != null)
            {
                goToHome();
            }
        }

        private void buttonFD1_Click(object sender, EventArgs e)
        {
            int temp;
            plc.GetDevice("Y10", out temp);
            if (temp == 0)
            {
                plc.SetDevice("Y10", 1);
            }
            else
            {
                plc.SetDevice("Y10", 0);
            }
            timerStartStop.Start();
        }

        private void buttonFD2_Click(object sender, EventArgs e)
        {
            int temp;
            plc.GetDevice("Y11", out temp);
            if (temp == 0)
            {
                plc.SetDevice("Y11", 1);
            }
            else
            {
                plc.SetDevice("Y11", 0);
            }
            timerStartStop.Start();
        }

        private void buttonFD3_Click(object sender, EventArgs e)
        {
            int temp;
            plc.GetDevice("Y12", out temp);
            if (temp == 0)
            {
                plc.SetDevice("Y12", 1);
            }
            else
            {
                plc.SetDevice("Y12", 0);
            }
            timerStartStop.Start();
        }

        private void buttonFD4_Click(object sender, EventArgs e)
        {
            int temp;
            plc.GetDevice("Y13", out temp);
            if (temp == 0)
            {
                plc.SetDevice("Y13", 1);
            }
            else
            {
                plc.SetDevice("Y13", 0);
            }
            timerStartStop.Start();
        }

        private void buttonVanHut_Click(object sender, EventArgs e)
        {
            int temp;
            plc.GetDevice("Y7", out temp);
            if (temp == 0)
            {
                plc.SetDevice("Y7", 1);
            }
            else
            {
                plc.SetDevice("Y7", 0);
            }
            timerStartStop.Start();
        }

        private void buttonBomKeo_Click(object sender, EventArgs e)
        {
            int temp;
            plc.GetDevice("Y6", out temp);
            if (temp == 0)
            {
                plc.SetDevice("Y6", 1);
            }
            else
            {
                plc.SetDevice("Y6", 0);
            }
            timerStartStop.Start();
        }

        private void buttonZtang_Click(object sender, EventArgs e)
        {
            if (plc != null)
            {
                int temp;
                plc.GetDevice("X20", out temp);

                if (temp == 1)
                {
                    vi_tri_z -= buoc_di_chuyen;
                    int so_buoc;
                    so_buoc = (int)(buoc_di_chuyen / vi_buoc_z);
                    plc.SetDevice("D200", -so_buoc);
                    plc.SetDevice("M81", 1);
                    Thread.Sleep(250);
                }
            }
            textBoxViTriZ.Text = vi_tri_z.ToString("0.000");
            timerStartStop.Start();
        }

        private void buttonGiamBuoc_Click(object sender, EventArgs e)
        {
            if (buoc_di_chuyen > 0.25f)
            {
                buoc_di_chuyen -= 0.25f;
            }
            labelBuoc.Text = "Bước di chuyển : " + buoc_di_chuyen.ToString("0.00") + "mm";
            Thread.Sleep(250);
            timerStartStop.Start();
        }

        private void buttonTangBuoc_Click(object sender, EventArgs e)
        {
            if (buoc_di_chuyen < 5.0f)
            {
                buoc_di_chuyen += 0.25f;
            }
            labelBuoc.Text = "Bước di chuyển : " + buoc_di_chuyen.ToString("0.00") + "mm";
            Thread.Sleep(250);
            timerStartStop.Start();
        }

        private void buttonZgiam_Click(object sender, EventArgs e)
        {
            if (plc != null)
            {
                vi_tri_z += buoc_di_chuyen;
                int so_buoc;
                so_buoc = (int)(buoc_di_chuyen / vi_buoc_z);
                plc.SetDevice("D200", so_buoc);
                plc.SetDevice("M80", 1);
                Thread.Sleep(250);
            }
            textBoxViTriZ.Text = vi_tri_z.ToString("0.000");
            timerStartStop.Start();
        }

        private void buttonXtang_Click(object sender, EventArgs e)
        {
            if (plc != null)
            {
                vi_tri_x += buoc_di_chuyen;
                textBoxViTriX.Text = vi_tri_x.ToString("0.000");

                FloatConvertToTwoInt16(buoc_di_chuyen * 1000);
                plc.SetDevice("D162", val1);
                plc.SetDevice("D163", val2);
                plc.SetDevice("M60", 1);
                Thread.Sleep(500);
                plc.SetDevice("M60", 0);
            }
            timerStartStop.Start();
        }

        private void buttonXgiam_Click(object sender, EventArgs e)
        {
            if (plc != null)
            {
                int temp;
                plc.GetDevice("X21", out temp);

                if (temp == 1)
                {
                    vi_tri_x -= buoc_di_chuyen;

                    FloatConvertToTwoInt16(-(buoc_di_chuyen * 1000));
                    plc.SetDevice("D162", val1);
                    plc.SetDevice("D163", val2);
                    plc.SetDevice("M60", 1);
                    Thread.Sleep(500);
                    plc.SetDevice("M60", 0);
                }
            }
            textBoxViTriX.Text = vi_tri_x.ToString("0.000");
            timerStartStop.Start();
        }

        private void buttonYgiam_Click(object sender, EventArgs e)
        {
            if (plc != null)
            {
                int temp;
                plc.GetDevice("X22", out temp);

                if (temp == 1)
                {
                    vi_tri_y -= buoc_di_chuyen;

                    FloatConvertToTwoInt16(-(buoc_di_chuyen * 1000));
                    plc.SetDevice("D172", val1);
                    plc.SetDevice("D173", val2);
                    plc.SetDevice("M70", 1);
                    Thread.Sleep(500);
                    plc.SetDevice("M70", 0);
                }
            }

            textBoxViTriY.Text = vi_tri_y.ToString("0.000");
            timerStartStop.Start();
        }

        private void buttonYtang_Click(object sender, EventArgs e)
        {
            if (plc != null)
            {
                vi_tri_y += buoc_di_chuyen;
                textBoxViTriY.Text = vi_tri_y.ToString("0.000");

                FloatConvertToTwoInt16(buoc_di_chuyen * 1000);
                plc.SetDevice("D172", val1);
                plc.SetDevice("D173", val2);
                plc.SetDevice("M70", 1);
                Thread.Sleep(500);
                plc.SetDevice("M70", 0);
            }
            timerStartStop.Start();
        }

        private void buttonMoveX_Click(object sender, EventArgs e)
        {
            float vi_tri_moi_x;
            vi_tri_moi_x = float.Parse(textBoxViTriX.Text);

            if (vi_tri_moi_x >= 5)
            {
                FloatConvertToTwoInt16((vi_tri_moi_x - vi_tri_x) * 1000);
                plc.SetDevice("D162", val1);
                plc.SetDevice("D163", val2);
                plc.SetDevice("M60", 1);
                Thread.Sleep(200);

                int done = 0;

                while (done == 0)
                {
                    plc.GetDevice("M61", out done);
                    Thread.Sleep(50);
                }
                plc.SetDevice("M60", 0);

                vi_tri_x = vi_tri_moi_x;

            }
            textBoxViTriX.Text = vi_tri_x.ToString("0.000");
            timerStartStop.Start();
        }

        private void buttonMoveY_Click(object sender, EventArgs e)
        {
            float vi_tri_moi_y;
            vi_tri_moi_y = float.Parse(textBoxViTriY.Text);

            if (vi_tri_moi_y >= 5)
            {
                FloatConvertToTwoInt16((vi_tri_moi_y - vi_tri_y) * 1000);
                plc.SetDevice("D172", val1);
                plc.SetDevice("D173", val2);
                plc.SetDevice("M70", 1);
                Thread.Sleep(200);

                int done = 0;

                while (done == 0)
                {
                    plc.GetDevice("M71", out done);
                    Thread.Sleep(50);
                }
                plc.SetDevice("M70", 0);

                vi_tri_y = vi_tri_moi_y;
            }
            textBoxViTriY.Text = vi_tri_y.ToString("0.000");
            timerStartStop.Start();
        }

        private void buttonMoveZ_Click(object sender, EventArgs e)
        {
            float vi_tri_moi_z;
            vi_tri_moi_z = float.Parse(textBoxViTriZ.Text);

            if (vi_tri_moi_z == 0)
            {
                plc.SetDevice("M90", 1);
                vi_tri_z = vi_tri_moi_z;
            }

            if (vi_tri_moi_z > 0 && vi_tri_moi_z <= 20)
            {
                float khoang_di_chuyen = vi_tri_moi_z - vi_tri_z;
                int so_buoc;
                so_buoc = (int)(khoang_di_chuyen / vi_buoc_z);
                plc.SetDevice("D202", so_buoc);
                plc.SetDevice("M91", 1);
                vi_tri_z = vi_tri_moi_z;
            }
            Thread.Sleep(250);
            textBoxViTriZ.Text = vi_tri_z.ToString("0.000");
            timerStartStop.Start();
        }

        private void buttonImport_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();

            //openFileDialog1.Filter = "excel files (*.xlsx)|*.xlsx|txt files (*.txt)|*.txt|All files (*.*)|*.*";
            openFileDialog1.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
            openFileDialog1.RestoreDirectory = true;
            openFileDialog1.Title = "Browse TXT Files";
            //openFileDialog1.InitialDirectory = @"D:\SMT\";

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                fileName = openFileDialog1.FileName;

                StreamReader Textfile = new StreamReader(fileName);
                string line;

                toa_do_chip_x = new float[3000];
                toa_do_chip_y = new float[3000];
                toa_do_pad_x = new float[9000];
                toa_do_pad_y = new float[9000];
                loai_chip = new int[3000];

                tong_so_chip = 0;
                tong_so_pad = 0;

                int counter = 0;
                int dem_ten = 0;

                ten_chip = new string[4];

                while ((line = Textfile.ReadLine()) != null)
                {
                    counter++;
                    if (counter > 1)
                    {
                        string[] cols = line.Split('\t');
                        string ten = cols[3];
                        var result = Array.Find(ten_chip, element => element == ten);

                        if (dem_ten < 4)
                        {
                            if (result == null)
                            {

                                ten_chip[dem_ten] = ten;
                                Console.WriteLine(ten_chip[dem_ten]);
                                dem_ten++;
                            }
                        }
                    }
                }

                Textfile.Close();
                Textfile = new StreamReader(fileName);
                counter = 0;

                while ((line = Textfile.ReadLine()) != null)
                {
                    counter++;
                    if (counter > 1)
                    {
                        string[] cols = line.Split('\t');
                        string ten = cols[3];
                        var result = Array.Find(ten_chip, element => element == ten);

                        toa_do_chip_x[tong_so_chip] = float.Parse(cols[4]);
                        toa_do_chip_y[tong_so_chip] = float.Parse(cols[5]);

                        for (int i = 0; i < 4; i++)
                        {
                            if (cols[3].Contains(ten_chip[i]))
                            {
                                loai_chip[tong_so_chip] = i;

                                if (ten_chip[loai_chip[tong_so_chip]].Contains("1206"))
                                {
                                    toa_do_pad_x[tong_so_pad] = toa_do_chip_x[tong_so_chip];
                                    toa_do_pad_y[tong_so_pad++] = toa_do_chip_y[tong_so_chip] + 1.42f;
                                    toa_do_pad_x[tong_so_pad] = toa_do_chip_x[tong_so_chip];
                                    toa_do_pad_y[tong_so_pad++] = toa_do_chip_y[tong_so_chip] - 1.42f;
                                }

                                if (ten_chip[loai_chip[tong_so_chip]].Contains("TU"))
                                {
                                    toa_do_pad_x[tong_so_pad] = toa_do_chip_x[tong_so_chip] + 1.42f;
                                    toa_do_pad_y[tong_so_pad++] = toa_do_chip_y[tong_so_chip];
                                    toa_do_pad_x[tong_so_pad] = toa_do_chip_x[tong_so_chip] - 1.42f;
                                    toa_do_pad_y[tong_so_pad++] = toa_do_chip_y[tong_so_chip];
                                }
                            }
                        }
                        tong_so_chip++;
                    }
                }
                Textfile.Close();
                textBoxTenChip1.Text = ten_chip[0];
                textBoxTenChip2.Text = ten_chip[1];
                textBoxTenChip3.Text = ten_chip[2];
                textBoxTenChip4.Text = ten_chip[3];
                labelTongSoChip.Text = "Tổng số chip : " + tong_so_chip.ToString();
            }
        }

        private void FloatConvertToTwoInt16(float value)
        {
            byte[] valueBytes = BitConverter.GetBytes(value);
            val1 = BitConverter.ToUInt16(valueBytes, 0);
            val2 = BitConverter.ToUInt16(valueBytes, 2);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            string file = @"config.txt";

            if (File.Exists(file))
            {
                StreamReader Textfile = new StreamReader(file);
                string line;

                while ((line = Textfile.ReadLine()) != null)
                {
                    if (line.Contains("vi_tri_feeder_1_x"))
                    {
                        string temp;
                        temp = line.Substring(line.IndexOf(":") + 1);
                        vi_tri_feeder_1_x = float.Parse(temp);
                    }
                    if (line.Contains("vi_tri_feeder_1_y"))
                    {
                        string temp;
                        temp = line.Substring(line.IndexOf(":") + 1);
                        vi_tri_feeder_1_y = float.Parse(temp);
                    }
                    if (line.Contains("vi_tri_feeder_1_z"))
                    {
                        string temp;
                        temp = line.Substring(line.IndexOf(":") + 1);
                        vi_tri_feeder_1_z = float.Parse(temp);
                    }

                    if (line.Contains("vi_tri_feeder_2_x"))
                    {
                        string temp;
                        temp = line.Substring(line.IndexOf(":") + 1);
                        vi_tri_feeder_2_x = float.Parse(temp);
                    }
                    if (line.Contains("vi_tri_feeder_2_y"))
                    {
                        string temp;
                        temp = line.Substring(line.IndexOf(":") + 1);
                        vi_tri_feeder_2_y = float.Parse(temp);
                    }
                    if (line.Contains("vi_tri_feeder_2_z"))
                    {
                        string temp;
                        temp = line.Substring(line.IndexOf(":") + 1);
                        vi_tri_feeder_2_z = float.Parse(temp);
                    }

                    if (line.Contains("vi_tri_feeder_3_x"))
                    {
                        string temp;
                        temp = line.Substring(line.IndexOf(":") + 1);
                        vi_tri_feeder_3_x = float.Parse(temp);
                    }
                    if (line.Contains("vi_tri_feeder_3_y"))
                    {
                        string temp;
                        temp = line.Substring(line.IndexOf(":") + 1);
                        vi_tri_feeder_3_y = float.Parse(temp);
                    }
                    if (line.Contains("vi_tri_feeder_3_z"))
                    {
                        string temp;
                        temp = line.Substring(line.IndexOf(":") + 1);
                        vi_tri_feeder_3_z = float.Parse(temp);
                    }

                    if (line.Contains("vi_tri_feeder_4_x"))
                    {
                        string temp;
                        temp = line.Substring(line.IndexOf(":") + 1);
                        vi_tri_feeder_4_x = float.Parse(temp);
                    }
                    if (line.Contains("vi_tri_feeder_4_y"))
                    {
                        string temp;
                        temp = line.Substring(line.IndexOf(":") + 1);
                        vi_tri_feeder_4_y = float.Parse(temp);
                    }
                    if (line.Contains("vi_tri_feeder_4_z"))
                    {
                        string temp;
                        temp = line.Substring(line.IndexOf(":") + 1);
                        vi_tri_feeder_4_z = float.Parse(temp);
                    }

                    if (line.Contains("vi_tri_camera_x"))
                    {
                        string temp;
                        temp = line.Substring(line.IndexOf(":") + 1);
                        vi_tri_camera_x = float.Parse(temp);
                    }
                    if (line.Contains("vi_tri_camera_y"))
                    {
                        string temp;
                        temp = line.Substring(line.IndexOf(":") + 1);
                        vi_tri_camera_y = float.Parse(temp);
                    }
                    if (line.Contains("vi_tri_camera_z"))
                    {
                        string temp;
                        temp = line.Substring(line.IndexOf(":") + 1);
                        vi_tri_camera_z = float.Parse(temp);
                    }

                    if (line.Contains("vi_tri_goc_pcb_x"))
                    {
                        string temp;
                        temp = line.Substring(line.IndexOf(":") + 1);
                        vi_tri_goc_pcb_x = float.Parse(temp);
                    }
                    if (line.Contains("vi_tri_goc_pcb_y"))
                    {
                        string temp;
                        temp = line.Substring(line.IndexOf(":") + 1);
                        vi_tri_goc_pcb_y = float.Parse(temp);
                    }
                    if (line.Contains("vi_tri_goc_pcb_z"))
                    {
                        string temp;
                        temp = line.Substring(line.IndexOf(":") + 1);
                        vi_tri_goc_pcb_z = float.Parse(temp);
                    }

                    if (line.Contains("khoang_cach_dau_bom_keo_x"))
                    {
                        string temp;
                        temp = line.Substring(line.IndexOf(":") + 1);
                        khoang_cach_2_dau_x = float.Parse(temp);
                    }
                    if (line.Contains("khoang_cach_dau_bom_keo_y"))
                    {
                        string temp;
                        temp = line.Substring(line.IndexOf(":") + 1);
                        khoang_cach_2_dau_y = float.Parse(temp);
                    }
                    if (line.Contains("khoang_cach_dau_bom_keo_z"))
                    {
                        string temp;
                        temp = line.Substring(line.IndexOf(":") + 1);
                        khoang_cach_2_dau_z = float.Parse(temp);
                    }

                    if (line.Contains("goc_xoay_chip_1"))
                    {
                        string temp;
                        temp = line.Substring(line.IndexOf(":") + 1);
                        goc_xoay_chip_1 = int.Parse(temp);
                    }
                    if (line.Contains("goc_xoay_chip_2"))
                    {
                        string temp;
                        temp = line.Substring(line.IndexOf(":") + 1);
                        goc_xoay_chip_2 = int.Parse(temp);
                    }
                    if (line.Contains("goc_xoay_chip_3"))
                    {
                        string temp;
                        temp = line.Substring(line.IndexOf(":") + 1);
                        goc_xoay_chip_3 = int.Parse(temp);
                    }
                    if (line.Contains("goc_xoay_chip_4"))
                    {
                        string temp;
                        temp = line.Substring(line.IndexOf(":") + 1);
                        goc_xoay_chip_4 = int.Parse(temp);
                    }
                }

                Textfile.Close();
            }
            hScrollBarXYspeed.Value = (int)(run_xy_speed / 1000);
            labelXYspeed.Text = "Tốc độ XY: " + run_xy_speed + " (mm / p)";
        }

        private void hScrollBarXYspeed_Scroll(object sender, ScrollEventArgs e)
        {
            run_xy_speed = e.NewValue * 1000;
            FloatConvertToTwoInt16(run_xy_speed); // toc do
            plc.SetDevice("D160", val1);
            plc.SetDevice("D161", val2);
            plc.SetDevice("D170", val1);
            plc.SetDevice("D171", val2);
            labelXYspeed.Text = "Tốc độ XY: " + run_xy_speed + " (mm / p)";
        }

        static void Xulyanh()
        {

            // Đọc hình ảnh đầu vào
            Mat img = CvInvoke.Imread(@"D:\tài liệu meca\Code mới\18122024\SMT IUH\bin\Debug\BitMaps\2.jpg");

            // Cắt hình ảnh
            Rectangle cropRect = new Rectangle(800, 700, 750, 850);
            Image<Bgr, Byte> bufferImg = img.ToImage<Bgr, Byte>();
            bufferImg.ROI = cropRect;
            Image<Bgr, Byte> croppedImg = bufferImg.Copy();
            Mat imgCrop = croppedImg.Mat;

            // Hàm để thực hiện phát hiện hình chữ nhật
            bool DetectRectangles(Mat inputFrame, Mat outputFrame, out int offsetY, double sigmaX)
            {
                offsetY = 30;
                bool found = false;

                // Áp dụng Gaussian Blur để làm mờ hình ảnh
                CvInvoke.GaussianBlur(inputFrame, inputFrame, new Size(15, 15), sigmaX);

                // Phát hiện các cạnh trong hình ảnh bằng thuật toán Canny
                Mat edges = new Mat();
                CvInvoke.Canny(inputFrame, edges, 50, 150);

                // Tìm các đường viền từ hình ảnh cạnh và vẽ chúng
                VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();
                Mat hierarchy = new Mat();
                CvInvoke.FindContours(edges, contours, hierarchy, RetrType.External, ChainApproxMethod.ChainApproxSimple);
                CvInvoke.DrawContours(outputFrame, contours, -1, new MCvScalar(0, 255, 0), 2); // Vẽ tất cả các đường viền

                for (int i = 0; i < contours.Size; i++)
                {
                    // Tính toán hình chữ nhật bao quanh nhỏ nhất (minimum area rectangle)
                    RotatedRect minAreaRect = CvInvoke.MinAreaRect(contours[i]);

                    // Tính toán chiều rộng và chiều cao của hình chữ nhật và diện tích
                    double width = minAreaRect.Size.Width;
                    double height = minAreaRect.Size.Height;
                    double minAreaRectangleArea = width * height;

                    // Kiểm tra điều kiện diện tích
                    if ((minAreaRectangleArea >= 30000
                        || (minAreaRectangleArea >= 18000 && minAreaRectangleArea < 30000))
                        && ((width / height) < 2)
                        && ((height / width) < 2)
                        && (Math.Abs(height - width) >= 20))
                    {
                        found = true;

                        // Điều chỉnh góc quay cho chính xác
                        double angle = minAreaRect.Angle;
                        if (width < height)
                        {
                            angle += 90;
                        }

                        //// Vẽ hình chữ nhật bao quanh nhỏ nhất
                        //PointF[] verticesF = minAreaRect.GetVertices();
                        //Point[] vertices = Array.ConvertAll(verticesF, point => Point.Round(point));
                        //for (int j = 0; j < 4; j++)
                        //{
                        //    CvInvoke.Line(outputFrame, vertices[j], vertices[(j + 1) % 4], new MCvScalar(255, 0, 0), 5);
                        //}

                        // Hiển thị diện tích của MinAreaRect và góc quay
                        string minAreaRectText = $"Angle: {angle:F2}, Area: {minAreaRectangleArea:F2}";
                        CvInvoke.PutText(outputFrame, minAreaRectText, new Point(10, offsetY), FontFace.HersheySimplex, 1.25, new MCvScalar(0, 0, 255), 3);

                        // Tăng vị trí y để không ghi đè văn bản
                        offsetY += 40;

                        // Thoát khỏi vòng lặp nếu điều kiện đầu tiên được thỏa mãn
                        break;
                    }

                }

                return found;
            }

            // Danh sách các giá trị sigmaX để thử
            double[] sigmaXValues = { 0,1,2,3,4,5};
            bool foundLargeRectangle = false;
            int textOffsetY = 30;

            // Thử phát hiện hình chữ nhật với các giá trị sigmaX khác nhau
            foreach (double sigmaX in sigmaXValues)
            {
                foundLargeRectangle = DetectRectangles(imgCrop, imgCrop, out textOffsetY, sigmaX);
                if (foundLargeRectangle)
                {
                    break;
                }
            }
            if (foundLargeRectangle || !foundLargeRectangle)
            {
                // Lưu ảnh vào thư mục
                Image saveImg = imgCrop.ToBitmap();
                saveImg.Save($@"D:\tài liệu meca\Code mới\18122024\SMT IUH\bin\Debug\BitMaps\KTL\(fileIndex).bmp");
                fileIndex++;
            }
             
            if (foundLargeRectangle)
            {
                // Lưu ảnh vào thư mục
                Image saveImg = imgCrop.ToBitmap();
                saveImg.Save($@"D:\tài liệu meca\Code mới\18122024\SMT IUH\bin\Debug\BitMaps\s.bmp");
            }
        }
    
        private void timerRun_Tick(object sender, EventArgs e)
        {
            int doneX = 0;
            int doneY = 0;
            int m80 = 0;
            int m81 = 0;
            int m60 = 0;
            int m70 = 0;
            int m100 = 0;
            int m101 = 0;
            int m102 = 0;

            plc.GetDevice("M60", out m60);
            plc.GetDevice("M70", out m70);
            plc.GetDevice("M61", out doneX);
            plc.GetDevice("M71", out doneY);
            plc.GetDevice("M80", out m80);
            plc.GetDevice("M81", out m81);
            plc.GetDevice("M100", out m100);
            plc.GetDevice("M101", out m101);
            plc.GetDevice("M102", out m102);

            if (m101 == 1)
            {
                plc.SetDevice("M101", 0);
                timerRun.Stop();
                timerStartStop.Start();
            }

            textBox1.Text = buoc_chay.ToString();

            if (doneX == 1)
            {
                plc.SetDevice("M60", 0);
            }

            if (doneY == 1)
            {
                plc.SetDevice("M70", 0);
            }

            if (m60 == 0 && m70 == 0 && doneX == 0 && doneY == 0 && m80 == 0 && m81 == 0)
            {
                if (steps == 0) // chay bom keo
                {
                    if (buoc_chay == 0)
                    {
                        labelDemChip.Text = "Đếm pad : " + (dem_pad + 1).ToString();

                        float toa_do_rel_x = 0;
                        float toa_do_rel_y = 0;

                        float vi_tri_chip_x; // toa do tuyet doi
                        float vi_tri_chip_y; // toa do tuyet doi

                        vi_tri_chip_x = vi_tri_goc_pcb_x + toa_do_pad_x[dem_pad] + khoang_cach_2_dau_x;
                        vi_tri_chip_y = vi_tri_goc_pcb_y + toa_do_pad_y[dem_pad] + khoang_cach_2_dau_y;

                        labelViTriChipX.Text = toa_do_pad_x[dem_pad].ToString("0.000");
                        labelViTriChipY.Text = toa_do_pad_y[dem_pad].ToString("0.000");

                        toa_do_rel_x = vi_tri_chip_x - toa_do_hien_tai_x;
                        toa_do_rel_y = vi_tri_chip_y - toa_do_hien_tai_y;

                        toa_do_hien_tai_x = vi_tri_chip_x;
                        toa_do_hien_tai_y = vi_tri_chip_y;

                        FloatConvertToTwoInt16(toa_do_rel_x * 1000);
                        plc.SetDevice("D162", val1);
                        plc.SetDevice("D163", val2);
                        FloatConvertToTwoInt16(toa_do_rel_y * 1000);
                        plc.SetDevice("D172", val1);
                        plc.SetDevice("D173", val2);

                        float absX = Math.Abs(toa_do_rel_x);
                        float absY = Math.Abs(toa_do_rel_y);

                        if (absX >= absY)
                        {
                            float ratio = absY / absX;

                            FloatConvertToTwoInt16(run_xy_speed);
                            plc.SetDevice("D160", val1);
                            plc.SetDevice("D161", val2);

                            FloatConvertToTwoInt16(run_xy_speed * ratio);
                            plc.SetDevice("D170", val1);
                            plc.SetDevice("D171", val2);
                        }
                        else
                        {
                            float ratio = absX / absY;

                            FloatConvertToTwoInt16(run_xy_speed);
                            plc.SetDevice("D170", val1);
                            plc.SetDevice("D171", val2);

                            FloatConvertToTwoInt16(run_xy_speed * ratio);
                            plc.SetDevice("D160", val1);
                            plc.SetDevice("D161", val2);
                        }

                        if (toa_do_rel_x != 0) plc.SetDevice("M60", 1);
                        if (toa_do_rel_y != 0) plc.SetDevice("M70", 1);
                    }

                    else if (buoc_chay == 1)
                    {
                        // Xuong z bom keo
                        int so_buoc = 0;
                        so_buoc = (int)((vi_tri_goc_pcb_z + khoang_cach_2_dau_z) / vi_buoc_z);
                        plc.SetDevice("D200", -so_buoc);
                        plc.SetDevice("M81", 1);
                        //timerRun.Stop();
                    }

                    else if (buoc_chay == 2)
                    {
                        if (dem_pad < tong_so_pad - 5) plc.SetDevice("Y6", 1); // bom keo
                        Thread.Sleep(20);
                        plc.SetDevice("Y6", 0);
                    }

                    //else if (buoc_chay == 3)
                    //{
                    //    Thread.Sleep(15);
                    //    plc.SetDevice("Y6", 0);
                    //}

                    else if (buoc_chay == 4)
                    {
                        plc.SetDevice("Y6", 0);
                        int so_buoc = 0;
                        so_buoc = (int)((vi_tri_goc_pcb_z + khoang_cach_2_dau_z) / vi_buoc_z);
                        plc.SetDevice("D200", so_buoc); // bom keo
                        plc.SetDevice("M80", 1);
                    }

                    else if (buoc_chay == 5)
                    {
                        dem_pad++;

                        labelDemChip.Text = "Đếm pad : " + (dem_pad + 1).ToString();
                        if (dem_pad >= tong_so_pad)
                        {
                            timerRun.Stop();
                            dem_chip = 0;
                            steps = 1;
                            buoc_chay = 0;
                            goToHome();
                        }

                    }

                    if (steps == 0)
                    {
                        if (buoc_chay < 5)
                        {
                            buoc_chay++;
                        }
                        else
                        {
                            buoc_chay = 0;
                        }
                    }
                }
                else
                {
                    if (buoc_chay == 0)
                    {
                        if (File.Exists(@"D:\tài liệu meca\Code mới\18122024\SMT IUH\bin\Debug\BitMaps\s.bmp"))
                        {
                            File.Delete(@"D:\tài liệu meca\Code mới\18122024\SMT IUH\bin\Debug\BitMaps\s.bmp");
                        }

                        labelDemChip.Text = "Đếm chip : " + (dem_chip + 1).ToString();

                        float toa_do_rel_x = 0;
                        float toa_do_rel_y = 0;

                        if (loai_chip[dem_chip] == 0)
                        {
                            toa_do_rel_x = vi_tri_feeder_1_x - toa_do_hien_tai_x;
                            toa_do_rel_y = vi_tri_feeder_1_y - toa_do_hien_tai_y;
                            toa_do_hien_tai_x = vi_tri_feeder_1_x;
                            toa_do_hien_tai_y = vi_tri_feeder_1_y;
                            plc.SetDevice("Y10", 1);
                            Thread.Sleep(1000);
                            plc.SetDevice("Y10", 0);
                        }
                        else if (loai_chip[dem_chip] == 1)
                        {
                            toa_do_rel_x = vi_tri_feeder_2_x - toa_do_hien_tai_x;
                            toa_do_rel_y = vi_tri_feeder_2_y - toa_do_hien_tai_y;
                            toa_do_hien_tai_x = vi_tri_feeder_2_x;
                            toa_do_hien_tai_y = vi_tri_feeder_2_y;
                            plc.SetDevice("Y11", 1);
                            Thread.Sleep(1000);
                            plc.SetDevice("Y11", 0);
                        }
                        else if (loai_chip[dem_chip] == 2)
                        {
                            toa_do_rel_x = vi_tri_feeder_3_x - toa_do_hien_tai_x;
                            toa_do_rel_y = vi_tri_feeder_3_y - toa_do_hien_tai_y;
                            toa_do_hien_tai_x = vi_tri_feeder_3_x;
                            toa_do_hien_tai_y = vi_tri_feeder_3_y;
                            plc.SetDevice("Y12", 1);
                            Thread.Sleep(1000);
                            plc.SetDevice("Y12", 0);
                        }
                        else if (loai_chip[dem_chip] == 3)
                        {
                            toa_do_rel_x = vi_tri_feeder_4_x - toa_do_hien_tai_x;
                            toa_do_rel_y = vi_tri_feeder_4_y - toa_do_hien_tai_y;
                            toa_do_hien_tai_x = vi_tri_feeder_4_x;
                            toa_do_hien_tai_y = vi_tri_feeder_4_y;
                            plc.SetDevice("Y13", 1);
                            Thread.Sleep(1000);
                            plc.SetDevice("Y13", 0);
                        }

                        FloatConvertToTwoInt16(toa_do_rel_x * 1000);
                        plc.SetDevice("D162", val1);
                        plc.SetDevice("D163", val2);
                        FloatConvertToTwoInt16(toa_do_rel_y * 1000);
                        plc.SetDevice("D172", val1);
                        plc.SetDevice("D173", val2);

                        float absX = Math.Abs(toa_do_rel_x);
                        float absY = Math.Abs(toa_do_rel_y);

                        if (absX >= absY)
                        {
                            float ratio = absY / absX;

                            FloatConvertToTwoInt16(run_xy_speed);
                            plc.SetDevice("D160", val1);
                            plc.SetDevice("D161", val2);

                            FloatConvertToTwoInt16(run_xy_speed * ratio);
                            plc.SetDevice("D170", val1);
                            plc.SetDevice("D171", val2);
                        }
                        else
                        {
                            float ratio = absX / absY;

                            FloatConvertToTwoInt16(run_xy_speed);
                            plc.SetDevice("D170", val1);
                            plc.SetDevice("D171", val2);

                            FloatConvertToTwoInt16(run_xy_speed * ratio);
                            plc.SetDevice("D160", val1);
                            plc.SetDevice("D161", val2);
                        }

                        if (toa_do_rel_x != 0) plc.SetDevice("M60", 1);
                        if (toa_do_rel_y != 0) plc.SetDevice("M70", 1);
                    }

                    else if (buoc_chay == 1)
                    {
                        plc.SetDevice("Y7", 1); // hut chip
                    }

                    else if (buoc_chay == 2)
                    {
                        int so_buoc = 0;

                        if (loai_chip[dem_chip] == 0)
                        {
                            so_buoc = (int)(vi_tri_feeder_1_z / vi_buoc_z);
                        }
                        else if (loai_chip[dem_chip] == 1)
                        {
                            so_buoc = (int)(vi_tri_feeder_2_z / vi_buoc_z);
                        }
                        else if (loai_chip[dem_chip] == 2)
                        {
                            so_buoc = (int)(vi_tri_feeder_3_z / vi_buoc_z);
                        }
                        else if (loai_chip[dem_chip] == 3)
                        {
                            so_buoc = (int)(vi_tri_feeder_4_z / vi_buoc_z);
                        }

                        plc.SetDevice("D200", so_buoc);
                        plc.SetDevice("M80", 1);
                    }

                    else if (buoc_chay == 4)
                    {
                        int so_buoc = 0;

                        if (loai_chip[dem_chip] == 0)
                        {
                            so_buoc = (int)(vi_tri_feeder_1_z / vi_buoc_z);
                        }
                        else if (loai_chip[dem_chip] == 1)
                        {
                            so_buoc = (int)(vi_tri_feeder_2_z / vi_buoc_z);
                        }
                        else if (loai_chip[dem_chip] == 2)
                        {
                            so_buoc = (int)(vi_tri_feeder_3_z / vi_buoc_z);
                        }
                        else if (loai_chip[dem_chip] == 3)
                        {
                            so_buoc = (int)(vi_tri_feeder_4_z / vi_buoc_z);
                        }
                        plc.SetDevice("D200", -so_buoc);
                        plc.SetDevice("M81", 1);
                    }

                    else if (buoc_chay == 5)
                    {
                        labelDemChip.Text = "Đếm chip : " + (dem_chip + 1).ToString();

                        float toa_do_rel_x = 0;
                        float toa_do_rel_y = 0;
                        float toa_do_rel_z = 0;

                        toa_do_rel_x = vi_tri_camera_x - toa_do_hien_tai_x;
                        toa_do_rel_y = vi_tri_camera_y - toa_do_hien_tai_y;
                        toa_do_rel_z = vi_tri_camera_z - toa_do_hien_tai_z;

                        toa_do_hien_tai_x = vi_tri_camera_x;
                        toa_do_hien_tai_y = vi_tri_camera_y;

                        FloatConvertToTwoInt16(toa_do_rel_x * 1000);
                        plc.SetDevice("D162", val1);
                        plc.SetDevice("D163", val2);
                        FloatConvertToTwoInt16(toa_do_rel_y * 1000);
                        plc.SetDevice("D172", val1);
                        plc.SetDevice("D173", val2);

                        float absX = Math.Abs(toa_do_rel_x);
                        float absY = Math.Abs(toa_do_rel_y);

                        if (absX >= absY)
                        {
                            float ratio = absY / absX;

                            FloatConvertToTwoInt16(run_xy_speed);
                            plc.SetDevice("D160", val1);
                            plc.SetDevice("D161", val2);

                            FloatConvertToTwoInt16(run_xy_speed * ratio);
                            plc.SetDevice("D170", val1);
                            plc.SetDevice("D171", val2);
                        }
                        else
                        {
                            float ratio = absX / absY;

                            FloatConvertToTwoInt16(run_xy_speed);
                            plc.SetDevice("D170", val1);
                            plc.SetDevice("D171", val2);

                            FloatConvertToTwoInt16(run_xy_speed * ratio);
                            plc.SetDevice("D160", val1);
                            plc.SetDevice("D161", val2);
                        }
                        if (toa_do_rel_x != 0) plc.SetDevice("M60", 1);
                        if (toa_do_rel_y != 0) plc.SetDevice("M70", 1);
                    }
                    else if (buoc_chay == 6)
                    {
                        int so_buoc = 0;
                        so_buoc = (int)(vi_tri_camera_z / vi_buoc_z);
                        plc.SetDevice("D200", so_buoc);
                        plc.SetDevice("M80", 1);
                    }
                    else if (buoc_chay == 7)
                    {
                        System.Diagnostics.Process.Start(@"D:\tài liệu meca\Code mới\18122024\SMT IUH\bin\Debug\SaveImage.exe");
                        Thread.Sleep(4000);
                    }
                    else if (buoc_chay == 8) 
                    {
                        Image tempImage = Image.FromFile(@"D:\tài liệu meca\Code mới\18122024\SMT IUH\bin\Debug\BitMaps\1.bmp");
                        Bitmap tempBitmap = new Bitmap(tempImage);
                        pictureBoxCam.Image = tempBitmap;
                        tempImage.Dispose();
                    }
                    else if (buoc_chay == 9)
                    {
                        //Thread.Sleep(2000);
                        Xulyanh();
                    }
                    if (buoc_chay == 10)
                    {
                        int so_buoc = 0;
                        so_buoc = (int)(-vi_tri_camera_z / vi_buoc_z);
                        plc.SetDevice("D200", so_buoc);
                        plc.SetDevice("M80", 1);
                        //Thread.Sleep(3000);

                        if (File.Exists(@"D:\tài liệu meca\Code mới\18122024\SMT IUH\bin\Debug\BitMaps\s.bmp"))
                        {
                            Image tempImage = Image.FromFile(@"D:\tài liệu meca\Code mới\18122024\SMT IUH\bin\Debug\BitMaps\s.bmp");
                            Bitmap tempBitmap = new Bitmap(tempImage); ;
                            pictureBoxCam.Image = tempBitmap;
                            tempImage.Dispose();
                        }
                        else
                        {
                            Image tempImage = Image.FromFile(@"D:\tài liệu meca\Code mới\18122024\SMT IUH\bin\Debug\BitMaps\2.jpg");
                            Bitmap tempBitmap = new Bitmap(tempImage);
                            pictureBoxCam.Image = tempBitmap;
                            tempImage.Dispose();
                            plc.SetDevice("Y7", 0);
                            dem_chip = dem_chip - 1;
                            buoc_chay = 16;
                        }
                    }

                    if (buoc_chay == 11) //86
                    {
                        labelDemChip.Text = "Đếm chip : " + (dem_chip + 1).ToString();

                        int goc_xoay_chip = 0;
                        if (loai_chip[dem_chip] == 0)
                        {
                            goc_xoay_chip = goc_xoay_chip_1;
                        }
                        else if (loai_chip[dem_chip] == 1)
                        {
                            goc_xoay_chip = goc_xoay_chip_2;
                        }
                        else if (loai_chip[dem_chip] == 2)
                        {
                            goc_xoay_chip = goc_xoay_chip_3;
                        }
                        else if (loai_chip[dem_chip] == 3)
                        {
                            goc_xoay_chip = goc_xoay_chip_4;
                        }

                        if (goc_xoay_chip < 0)
                        {
                            int so_xung_xoay = Math.Abs(goc_xoay_chip * 1600 / 360);
                            plc.SetDevice("D208", so_xung_xoay);
                            plc.SetDevice("M82", 1);
                        }
                        else if (goc_xoay_chip > 0)
                        {
                            int so_xung_xoay = Math.Abs(goc_xoay_chip * 1600 / 360);
                            plc.SetDevice("D208", so_xung_xoay);
                            plc.SetDevice("M83", 1);
                        }

                        float toa_do_rel_x = 0;
                        float toa_do_rel_y = 0;

                        float vi_tri_chip_x; // toa do tuyet doi
                        float vi_tri_chip_y; // toa do tuyet doi

                        vi_tri_chip_x = vi_tri_goc_pcb_x + toa_do_chip_x[dem_chip];
                        vi_tri_chip_y = vi_tri_goc_pcb_y + toa_do_chip_y[dem_chip];

                        labelViTriChipX.Text = toa_do_chip_x[dem_chip].ToString("0.000");
                        labelViTriChipY.Text = toa_do_chip_y[dem_chip].ToString("0.000");

                        toa_do_rel_x = vi_tri_chip_x - toa_do_hien_tai_x;
                        toa_do_rel_y = vi_tri_chip_y - toa_do_hien_tai_y;

                        toa_do_hien_tai_x = vi_tri_chip_x;
                        toa_do_hien_tai_y = vi_tri_chip_y;

                        FloatConvertToTwoInt16(toa_do_rel_x * 1000);
                        plc.SetDevice("D162", val1);
                        plc.SetDevice("D163", val2);
                        FloatConvertToTwoInt16(toa_do_rel_y * 1000);
                        plc.SetDevice("D172", val1);
                        plc.SetDevice("D173", val2);

                        float absX = Math.Abs(toa_do_rel_x);
                        float absY = Math.Abs(toa_do_rel_y);

                        if (absX >= absY)
                        {
                            float ratio = absY / absX;

                            FloatConvertToTwoInt16(run_xy_speed);
                            plc.SetDevice("D160", val1);
                            plc.SetDevice("D161", val2);

                            FloatConvertToTwoInt16(run_xy_speed * ratio);
                            plc.SetDevice("D170", val1);
                            plc.SetDevice("D171", val2);
                        }
                        else
                        {
                            float ratio = absX / absY;

                            FloatConvertToTwoInt16(run_xy_speed);
                            plc.SetDevice("D170", val1);
                            plc.SetDevice("D171", val2);

                            FloatConvertToTwoInt16(run_xy_speed * ratio);
                            plc.SetDevice("D160", val1);
                            plc.SetDevice("D161", val2);
                        }

                        if (toa_do_rel_x != 0) plc.SetDevice("M60", 1);
                        if (toa_do_rel_y != 0) plc.SetDevice("M70", 1);
                    }

                    else if (buoc_chay == 12) //87
                    {
                        int so_buoc = 0;
                        so_buoc = (int)(vi_tri_goc_pcb_z / vi_buoc_z);
                        plc.SetDevice("D200", so_buoc);
                        plc.SetDevice("M80", 1);
                    }

                    else if (buoc_chay == 13) //88
                    {
                        plc.SetDevice("Y7", 0);
                    }

                    else if (buoc_chay == 14) //89
                    {
                        int so_buoc = 0;
                        so_buoc = (int)(vi_tri_goc_pcb_z / vi_buoc_z);
                        plc.SetDevice("D200", -so_buoc);
                        plc.SetDevice("M81", 1);
                    }
                    if (buoc_chay == 15) //90
                    {
                        int goc_xoay_chip = 0;
                        if (loai_chip[dem_chip] == 0)
                        {
                            goc_xoay_chip = goc_xoay_chip_1;
                        }
                        else if (loai_chip[dem_chip] == 1)
                        {
                            goc_xoay_chip = goc_xoay_chip_2;
                        }
                        else if (loai_chip[dem_chip] == 2)
                        {
                            goc_xoay_chip = goc_xoay_chip_3;
                        }
                        else if (loai_chip[dem_chip] == 3)
                        {
                            goc_xoay_chip = goc_xoay_chip_4;
                        }

                        if (goc_xoay_chip > 0)
                        {
                            int so_xung_xoay = Math.Abs(goc_xoay_chip * 1600 / 360);
                            plc.SetDevice("D208", so_xung_xoay);
                            plc.SetDevice("M82", 1);
                        }
                        else if (goc_xoay_chip < 0)
                        {
                            int so_xung_xoay = Math.Abs(goc_xoay_chip * 1600 / 360);
                            plc.SetDevice("D208", so_xung_xoay);
                            plc.SetDevice("M83", 1);
                        }
                    }
                    else if (buoc_chay == 16) //91
                    {
                        if (File.Exists(@"D:\tài liệu meca\Code mới\18122024\SMT IUH\bin\Debug\BitMaps\1.bmp"))
                        {
                            File.Delete(@"D:\tài liệu meca\Code mới\18122024\SMT IUH\bin\Debug\BitMaps\1.bmp");
                        }
                        Image tempImage = Image.FromFile(@"D:\tài liệu meca\Code mới\18122024\SMT IUH\bin\Debug\BitMaps\2.jpg");
                        Bitmap tempBitmap = new Bitmap(tempImage);
                        pictureBoxCam.Image = tempBitmap;
                        tempImage.Dispose();
                        dem_chip++;
                        if (dem_chip == tong_so_chip)
                        {
                            //timerRun.Stop();
                            goToHome();
                            timerStartStop.Start();
                        }
                        labelDemChip.Text = "Đếm chip : " + (dem_chip + 1).ToString();
                    }

                    if (buoc_chay < 16) //91
                    {
                        buoc_chay++;
                    }
                    else
                    {
                        buoc_chay = 0;
                    }
                }
            }
        }

        private void buttonRun_Click(object sender, EventArgs e)
        {
            if (tong_so_chip > 0)
            {
                buoc_chay = 0;
                dem_chip = 0;
                dem_pad = 0;
                steps = 0;
                toa_do_hien_tai_x = 5;
                toa_do_hien_tai_y = 5;
                toa_do_hien_tai_z = 0;

                timerStartStop.Stop();

                plc.SetDevice("Y6", 1);
                Thread.Sleep(120);
                plc.SetDevice("Y6", 0);

                timerRun.Start();
            }
        }

        private void buttonPause_Click(object sender, EventArgs e)
        {
            timerRun.Stop();
        }

        private void buttonRunKeo_Click(object sender, EventArgs e)
        {

            if (tong_so_chip > 0)
            {
                buoc_chay = 0;
                dem_chip = 0;
                dem_pad = 0;
                steps = 0;
                toa_do_hien_tai_x = 5;
                toa_do_hien_tai_y = 5;
                toa_do_hien_tai_z = 0;
                timerRun.Start();
                timerStartStop.Stop();
            }
        }

        private void buttonCamera_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start(@"D:\tài liệu meca\Code mới\18122024\SMT IUH\bin\Debug\SaveImage.exe");
            timer1.Start();
        }

        private void buttonEnd_Click(object sender, EventArgs e)
        {
            timerRun.Stop();
            dem_chip = 0;
            dem_pad = 0;
        }
        private void timer1_Tick(object sender, EventArgs e)
        {
            Image tempImage = Image.FromFile(@"D:\tài liệu meca\Code mới\18122024\SMT IUH\bin\Debug\BitMaps\1.bmp");
            Bitmap tempBitmap = new Bitmap(tempImage);
            pictureBoxCam.Image = tempBitmap;
            tempImage.Dispose();
        }

        private void buttonGapTha_Click(object sender, EventArgs e)
        {
            if (tong_so_chip > 0)
            {
                buoc_chay = 0;
                dem_chip = 0;
                dem_pad = 0;
                steps = 1;
                toa_do_hien_tai_x = 5;
                toa_do_hien_tai_y = 5;
                toa_do_hien_tai_z = 0;
                timerRun.Start();
                timerStartStop.Stop();
            }
        }

        private void timerStartStop_Tick(object sender, EventArgs e)
        {
            int m100 = 0;
            int m101 = 0;
            int m102 = 0;

            plc.GetDevice("M100", out m100);
            plc.GetDevice("M101", out m101);
            plc.GetDevice("M102", out m102);

            if (m100 == 1)
            {
                plc.SetDevice("M100", 0);
                if (tong_so_chip > 0)
                {
                    buoc_chay = 0;
                    dem_chip = 0;
                    dem_pad = 0;
                    steps = 0;
                    toa_do_hien_tai_x = 5;
                    toa_do_hien_tai_y = 5;
                    toa_do_hien_tai_z = 0;

                    timerStartStop.Stop();

                   // plc.SetDevice("Y6", 1);
                   // Thread.Sleep(50);
                   // plc.SetDevice("Y6", 0);
                    timerRun.Start();
                }
            }
            else if (m102 == 1)
            {
                plc.SetDevice("M102", 0);
                goToHome();
            }
        }
    }
}