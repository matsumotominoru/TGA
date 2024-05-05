using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace test_win
{
    static class Program
    {
        /// <summary>
        /// アプリケーションのメイン エントリ ポイントです。
        /// </summary>
        [STAThread]
        static void Main()
        {
            MtoLib.Pict.RESULT ret;
            MtoLib.Pict.TGA tga = new MtoLib.Pict.TGA();

            var file_name = "card_yuzai_btn";
            ret = tga.Create(file_name + ".tga");
            Console.WriteLine("Create result:" + ret.ToString());
            if (ret != MtoLib.Pict.RESULT.ERROR_NONE) return;

            tga.ConvertBitType(MtoLib.Pict.TGA.LINE.IMAGE_LINE_LRDU);
            ret = tga.OutputBMP(file_name + ".bmp");
            Console.WriteLine("OutputBMP result:" + ret.ToString());
            //Application.EnableVisualStyles();
            //Application.SetCompatibleTextRenderingDefault(false);
            //Application.Run(new Form1());
        }
    }
}
