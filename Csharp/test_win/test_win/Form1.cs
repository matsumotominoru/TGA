using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace test_win
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Test(ref byte[] pDest)
        {
            pDest = new byte[4];
            pDest[0] = 1;
            pDest[1] = 2;
            pDest[2] = 3;
            pDest[3] = 4;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
#if NOP
            // TGA test
            try
            {
                MtoLib.Pict.RESULT ret;
                MtoLib.Pict.TGA tga = new MtoLib.Pict.TGA();

                //ret = tga.Create("../../dat/pen1_ico32a.tga");
                ret = tga.Create("../../dat/pen1_ico256.tga");
                Console.WriteLine("Create result:" + ret.ToString());
                if (ret != MtoLib.Pict.RESULT.ERROR_NONE) return;

                /*
                tga.ConvertBitType(MtoLib.Pict.TGA.LINE.IMAGE_LINE_LRDU);
                ret = tga.OutputBMP("output32.bmp");
                Console.WriteLine("OutputBMP result:" + ret.ToString());
                */

                // ConvertBMP->Set BackgroundImage
                Bitmap bmp = null;
                ret = tga.ConvertBMP(ref bmp);
                Console.WriteLine("ConvertBMP result:" + ret.ToString());
                this.BackgroundImage = bmp;
            }
            catch
            {
                Console.WriteLine("ファイルがないよ");
            }
#endif

#if DEBUG
            // Tim2 test
            try {
                MtoLib.Pict.RESULT ret;
                MtoLib.Pict.Tim2 tim2 = new MtoLib.Pict.Tim2();

                //string fileName = "pen1_ico16";
                //string fileName = "pen1_ico24";
                //string fileName = "pen1_ico32";
                //string fileName = "pen1_ico256";
                string fileName = "pen1_ico256_16";
                //string fileName = "pen1_ico256_24";
                
                ret = tim2.Create("../../dat/" + fileName + ".tm2");
                Console.WriteLine("Create result:" + ret.ToString());
                if (ret != MtoLib.Pict.RESULT.ERROR_NONE) return;

                // BITMAP配列に変換
                tim2.ConvertTim2Clut();
                tim2.ConvertRGBA();

                ret = tim2.OutputBMP(fileName + ".bmp");
                Console.WriteLine("OutputBMP result:" + ret.ToString());

                Bitmap bmp = null;
                ret = tim2.ConvertBMP(ref bmp);
                Console.WriteLine("ConvertBMP result:" + ret.ToString());
                this.BackgroundImage = bmp;
            }
            catch
            {
                Console.WriteLine("ファイルがないよ");
            }
#endif
        }
    }
}
