using System;

namespace MtoLib
{
    /*-------------------------------------------------------------------
    【機能】汎用関数まとめクラス（継承禁止）
    -------------------------------------------------------------------*/
    sealed class Utility
    {
        /// <summary>
        /// インスタンスの生成を禁止する
        /// </summary>
        private Utility() { }

        
        /// <summary>
        /// 戻値の状態
        /// </summary>
        public enum STATE
        {
            ERROR = -1,
            BUSY = 0,
            OK,
            CANCEL
        }


        // プライベートメンバ変数
        private const float RAD = (float)(Math.PI / 180.0);
        private const float DEG = (float)(180.0 / Math.PI);

        /// <summary>
        /// 角度→ラジアン
        /// </summary>
        /// <param name="rot">角度</param>
        /// <returns>ラジアン</returns>
        public static float DEGtoRAD(float rot)
        {
            return (rot * RAD);
        }

        /// <summary>
        /// ラジアン→角度
        /// </summary>
        /// <param name="rot">ラジアン値</param>
        /// <returns>角度</returns>
        public static float RADtoDEG(float rot)
        {
            return (rot * DEG);
        }

        /// <summary>
        /// 値を指定のアライメントに合わせる
        /// </summary>
        /// <param name="num">値</param>
        /// <param name="aligin">アライメント</param>
        /// <returns>指定のアライメントに沿った値</returns>
        public static UInt32 BOUND(int num, int aligin)
        {
            return (UInt32)((num + aligin - 1) & ~(aligin - 1));
        }

        /// <summary>
        /// 16Bit変数のエンディアン入れ替え
        /// </summary>
        /// <param name="num">入れ替える値</param>
        /// <returns>上位と下位ビットを入れ替えた値</returns>
        public static UInt16 BitReverse16(UInt16 num)
        {
            return (UInt16)(((num & 0xff00) >> 8) | ((num & 0x00ff) << 8));
        }

        /// <summary>
        /// 32Bit変数のエンディアン入れ替え
        /// </summary>
        /// <param name="num">入れ替える値</param>
        /// <returns>上位と下位ビットを入れ替えた値</returns>
        public static UInt32 BitReverse32(UInt32 num)
        {
            return (((num & 0x000000ff) << 24) | ((num & 0x0000ff00) << 8) |
                    ((num & 0x00ff0000) >> 8) | ((num & 0xff000000) >> 24));
        }

        /// <summary>
        /// 64Bit変数のエンディアン入れ替え
        /// </summary>
        /// <param name="num">入れ替える値</param>
        /// <returns>上位と下位ビットを入れ替えた値</returns>
        public static UInt64 BitReverse64(UInt64 num)
        {
            return (((num & 0x00000000000000ffU) << 56) | ((num & 0x000000000000ff00U) << 40) |
                    ((num & 0x0000000000ff0000U) << 24) | ((num & 0x00000000ff000000U) <<  8) |
                    ((num & 0xff00000000000000U) >> 56) | ((num & 0x00ff000000000000U) >> 40) |
                    ((num & 0x0000ff0000000000U) >> 24) | ((num & 0x000000ff00000000U) >>  8));
        }

        /// <summary>
        /// 指定値の指定桁目の値を取得
        /// </summary>
        /// <param name="num">値</param>
        /// <param name="digit">取得する桁目</param>
        /// <returns>指定桁目の値</returns>
        public static UInt32 GetDigit(UInt32 num, int digit)
        {
            return (num / (UInt32)Math.Pow(10, digit) % 10);
        }

        /// <summary>
        /// ミリ秒取得
        /// </summary>
        /// <returns>システム起動後の経過時間</returns>
        public static UInt32 timeGetTime()
        {
            return (UInt32)Environment.TickCount;
        }

        /// <summary>
        /// 3方向ベクトルと4x4行列の積
        /// 備考:4行目の行列には1をかけます
        /// </summary>
        /// <param name="dv">出力先ベクトル</param>
        /// <param name="v">計算用ベクトル</param>
        /// <param name="m">計算用行列</param>
        public static void MatApply3(CVector3 dv, CVector3 v, CMatrix4 m)
        {
            dv.x = v.x * m.m[0, 0] + v.y * m.m[1, 0] + v.z * m.m[2, 0] + 1.0f * m.m[3, 0];
            dv.y = v.x * m.m[0, 1] + v.y * m.m[1, 1] + v.z * m.m[2, 1] + 1.0f * m.m[3, 1];
            dv.z = v.x * m.m[0, 2] + v.y * m.m[1, 2] + v.z * m.m[2, 2] + 1.0f * m.m[3, 2];
        }

        /// <summary>
        /// 4方向ベクトルと4x4行列の積
        /// </summary>
        /// <param name="dv">出力先ベクトル</param>
        /// <param name="v">計算用ベクトル</param>
        /// <param name="m">計算用行列</param>
        public static void MatApply4(CVector4 dv, CVector4 v, CMatrix4 m)
        {
            dv.x = v.x * m.m[0, 0] + v.y * m.m[1, 0] + v.z * m.m[2, 0] + v.w * m.m[3, 0];
            dv.y = v.x * m.m[0, 1] + v.y * m.m[1, 1] + v.z * m.m[2, 1] + v.w * m.m[3, 1];
            dv.z = v.x * m.m[0, 2] + v.y * m.m[1, 2] + v.z * m.m[2, 2] + v.w * m.m[3, 2];
            dv.z = v.x * m.m[0, 3] + v.y * m.m[1, 3] + v.z * m.m[2, 3] + v.w * m.m[3, 3];
        }

        // CRC16-CCITT
        private const byte UCHAR_MAX = 0xff;
        private const byte CHAR_BIT = 8;
        private const UInt32 CRCPOLY = 0x1021U;    // x^{16}+x^{12}+x^5+1
        private const UInt16 CRCINIT = 0xffff;     // 初期値
        private const UInt16 CRCXOR  = 0xffff;        // 出力XOR
        private static int[] crctbl = new int[UCHAR_MAX + 1];

        /// <summary>
        /// CRC16-CCITTテーブル作成
        /// 備考：使用前に1度だけ読んでください
        /// </summary>
        public static void CreateCRCTable()
        {
            UInt32 i, j, r;

            for (i = 0; i <= UCHAR_MAX; i++) {
                r = i << (16 - CHAR_BIT);

                for (j = 0; j < CHAR_BIT; j++) {
                    
                    if ((r & 0x8000U) != 0) {
                        r = (r << 1) ^ CRCPOLY;
                    } else {
                        r <<= 1;
                    }
                }

                crctbl[i] = (int)(r & 0xFFFFU);
            }
        }

        /// <summary>
        /// CRC16-CCITT計算 
        /// </summary>
        /// <param name="pData">データアドレス</param>
        /// <param name="size">データサイズ</param>
        /// <returns>CRC16の値</returns>
        public static UInt16 GetCRC16(byte[] pData, UInt32 size)
        {
            UInt32 i = 0;
            UInt32 r = CRCINIT;

            do {
                r = (UInt32)((r << CHAR_BIT) ^ crctbl[(byte)(r >> (16 - CHAR_BIT)) ^ pData[i]]);
            } while (++i < size);

            return (UInt16)(~r & CRCXOR);
        }

        /// <summary>
        /// ゆっくり止まる移動
        /// </summary>
        /// <param name="target">目標値</param>
        /// <param name="dx">変化する値</param>
        /// <param name="dir">加減算方向</param>
        /// <returns>true:目標値 false:移動中</returns>
        public static bool SmoothMmove(int target, ref int dx, int dir)
        {
            int spd = (target - dx) >> 3;

            // 指定スピード(dir)以下になったら…
            if ((dir > 0 && spd < dir) || (dir < 0 && spd > dir))
            {
                spd = dir;
            }

            dx += spd;
            if ((dir > 0 && dx >= target) || (dir < 0 && dx <= target))
            {
                dx = target;
                return true;
            }
            return false;
        }

        /// <summary>
        /// ゆっくり止まる移動
        /// </summary>
        /// <param name="target">目標値</param>
        /// <param name="dx">変化する値</param>
        /// <param name="dir">加減算方向</param>
        /// <returns>true:目標値 false:移動中</returns>
        public static bool SmoothMmove(float target, ref float dx, float dir)
        {
            float spd = (target - dx) / 8.0f;

            // 指定スピード(dir)以下になったら…
            if ((dir > 0 && spd < dir) || (dir < 0 && spd > dir))
            {
                spd = dir;
            }

            dx += spd;
            if ((dir > 0 && dx >= target) || (dir < 0 && dx <= target))
            {
                dx = target;
                return true;
            }
            return false;
        }

        /// <summary>
        /// ビット数からピクセルフォーマットを取得する
        /// </summary>
        /// <param name="bit">イメージ/パレットのビット数</param>
        /// <returns>ピクセルフォーマット</returns>
        public static System.Drawing.Imaging.PixelFormat getPixelFormat(int bit)
        {
            if (bit == 32)
            {
                return System.Drawing.Imaging.PixelFormat.Format32bppArgb;
            }
            else if (bit == 24)
            {
                return System.Drawing.Imaging.PixelFormat.Format24bppRgb;
            }
            else if (bit == 16)
            {
                return System.Drawing.Imaging.PixelFormat.Format16bppArgb1555;
            }

            return System.Drawing.Imaging.PixelFormat.Max;
        }
    }
}
