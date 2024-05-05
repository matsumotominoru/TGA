using System;
using System.IO;
using System.Collections.Generic;

namespace MtoLib
{
    /*-------------------------------------------------------------------
    【機能】汎用クラスをまとめるソース
    -------------------------------------------------------------------*/
    namespace Type
    {
        /// <summary>
        /// short型二次元ベクトル
        /// </summary>
        class DVECTOR
        {
            public short vx;
            public short vy;

            DVECTOR(short x, short y) { vx = x; vy = y; }
            DVECTOR(int x, int y) { vx = (short)x; vy = (short)y; }
            DVECTOR(DVECTOR v) { vx = v.vx; vy = v.vy; }

            // 符号変化
            public static DVECTOR operator +(DVECTOR v)
            {
                return new DVECTOR(v.vx, v.vy);
            }

            public static DVECTOR operator -(DVECTOR v)
            {
                return new DVECTOR(-v.vx, -v.vy);
            }

            // 演算
            public static DVECTOR operator +(DVECTOR v1, DVECTOR v2)
            {
                return new DVECTOR(v1.vx + v2.vx, v1.vy + v2.vy);
            }

            public static DVECTOR operator -(DVECTOR v1, DVECTOR v2)
            {
                return new DVECTOR(v1.vx - v2.vx, v1.vy - v2.vy);
            }

            public static DVECTOR operator +(DVECTOR v1, int num)
            {
                return new DVECTOR(v1.vx + num, v1.vy + num);
            }

            public static DVECTOR operator -(DVECTOR v1, int num)
            {
                return new DVECTOR(v1.vx - num, v1.vy - num);
            }

            public static DVECTOR operator *(DVECTOR v1, int num)
            {
                return new DVECTOR(v1.vx * num, v1.vy * num);
            }

            public static DVECTOR operator /(DVECTOR v1, int num)
            {
                if (num == 0)
                {
                    throw new DivideByZeroException("Zero Division");
                }
                return new DVECTOR(v1.vx / num, v1.vy / num);
            }

            // 評価
            public override bool Equals(object obj)
            {
                if (obj == null) return false;

                DVECTOR v = obj as DVECTOR;
                if ((object)v == null) return false;

                return (vx == v.vx) && (vy == v.vy);
            }

            public override int GetHashCode()
            {
                return base.GetHashCode();
            }

            public static bool operator ==(DVECTOR v1, DVECTOR v2)
            {
                return v1.Equals(v2);
            }

            public static bool operator !=(DVECTOR v1, DVECTOR v2)
            {
                return !v1.Equals(v2);
            }
        }
    }

    /*-------------------------------------------------------------------
    【機能】画像関係
    -------------------------------------------------------------------*/
    namespace Pict
    {
        /// <summary>
        /// 処理結果
        /// </summary>
        public enum RESULT
        {
            /// <summary>
            /// ファイルオープン失敗
            /// </summary>
            ERROR_OPEN = -1,
            
            /// <summary>
            /// メモリ確保失敗
            /// </summary>
            ERROR_MEMORY = -2,

            /// <summary>
            /// サポートしていない形式
            /// </summary>
            ERROR_HEADER = -3,

            /// <summary>
            /// パレットデータが不正
            /// </summary>
            ERROR_PALETTE = -4,

            /// <summary>
            /// ピクセルデータが不正
            /// </summary>
            ERROR_IMAGE = -5,

            /// <summary>
            /// 出力エラー
            /// </summary>
            ERROR_OUTPUT = -6,

            /// <summary>
            /// 変換できない
            /// </summary>
            ERROR_CONVERT = -7,

            /// <summary>
            /// エラーなし(正常終了)
            /// </summary>
            ERROR_NONE = 1,

            /// <summary>
            /// 結果最大値
            /// </summary>
            ERROR_MAX
        }

        /// <summary>
        /// RGBAマスククラス
        /// </summary>
        sealed class MASK
        {
            /// <summary>
            /// インスタンスの生成を禁止する
            /// </summary>
            private MASK() { }

            public const UInt16 R5 = 0x001f;
            public const UInt16 G5 = 0x03e0;
            public const UInt16 B5 = 0x7c00;
            public const UInt16 A1 = 0x8000;

            public const UInt32 R8 = 0x000000ff;
            public const UInt32 G8 = 0x0000ff00;
            public const UInt32 B8 = 0x00ff0000;
            public const UInt32 A8 = 0xff000000;

            /// <summary>
            /// RGBA5551からRを取得
            /// </summary>
            /// <param name="rgba">RGBA5551</param>
            /// <returns>R値</returns>
            public static byte getR5(UInt16 rgba) { return (byte)(rgba & R5); }

            /// <summary>
            /// RGBA5551からGを取得
            /// </summary>
            /// <param name="rgba">RGBA5551</param>
            /// <returns>G値</returns>
            public static byte getG5(UInt16 rgba) { return (byte)((rgba & G5) >> 5); }

            /// <summary>
            /// RGBA5551からBを取得
            /// </summary>
            /// <param name="rgba">RGBA5551</param>
            /// <returns>B値</returns>
            public static byte getB5(UInt16 rgba) { return (byte)((rgba & B5) >> 10); }

            /// <summary>
            /// RGBA5551からAを取得
            /// </summary>
            /// <param name="rgba">RGBA5551</param>
            /// <returns>A値</returns>
            public static byte getA1(UInt16 rgba) { return (byte)((rgba & A1) >> 15); }

            /// <summary>
            /// 各要素からRGBA5551を取得
            /// </summary>
            /// <param name="r">R値</param>
            /// <param name="g">G値</param>
            /// <param name="b">B値</param>
            /// <param name="a">A値</param>
            /// <returns>RGBA5551値</returns>
            public static UInt16 getRGBA5551(byte r, byte g, byte b, byte a)
            {
                return (UInt16)(r | (g << 5) | (b << 10) | (a << 15));
            }

            /// <summary>
            /// RGBA8888からRを取得
            /// </summary>
            /// <param name="rgba">RGBA8888</param>
            /// <returns>R値</returns>
            public static byte getR8(UInt32 rgba) { return (byte)(rgba & R8); }

            /// <summary>
            /// RGBA8888からGを取得
            /// </summary>
            /// <param name="rgba">RGBA8888</param>
            /// <returns>G値</returns>
            public static byte getG8(UInt32 rgba) { return (byte)((rgba & G8) >> 8); }

            /// <summary>
            /// RGBA8888からBを取得
            /// </summary>
            /// <param name="rgba">RGBA8888</param>
            /// <returns>B値</returns>
            public static byte getB8(UInt32 rgba) { return (byte)((rgba & B8) >> 16); }

            /// <summary>
            /// RGBA8888からAを取得
            /// </summary>
            /// <param name="rgba">RGBA8888</param>
            /// <returns>A値</returns>
            public static byte getA8(UInt32 rgba) { return (byte)((rgba & A8) >> 24); }

            /// <summary>
            /// 各要素からRGBA8888を取得
            /// </summary>
            /// <param name="r">R値</param>
            /// <param name="g">G値</param>
            /// <param name="b">B値</param>
            /// <param name="a">A値</param>
            /// <returns>RGBA8888値</returns>
            public static UInt32 getRGBA8888(byte r, byte g, byte b, byte a)
            {
                return (UInt32)(r | (g << 8) | (b << 16) | (a << 24));
            }
        }

        /// <summary>
        /// BITMAPFILEHEADERクラス
        /// 備考：2バイトアライメントにしてません
        /// </summary>
        class BITMAPFILEHEADER
        {
            public UInt16 bfType;
            public UInt32 bfSize;
            public UInt16 bfReserved1;
            public UInt16 bfReserved2;
            public UInt32 bfOffBits;

            /// <summary>
            /// BITMAPFILEHEADERサイズ
            /// </summary>
            public const int HEADER_SIZE = 14;

            public BITMAPFILEHEADER()
            {
                bfType = 0x4D42; // BM
                bfReserved1 = 0;
                bfReserved2 = 0;

                // ヘッダーサイズだけ設定する
                bfOffBits = BITMAPFILEHEADER.HEADER_SIZE + BITMAPINFOHEADER.HEADER_SIZE;
                bfSize = bfOffBits;
            }

            /// <summary>
            /// BITMAPFILEHEADERの書き込み
            /// </summary>
            /// <param name="bwrite">BinaryWriterインスタンス</param>
            public void WriteHeader(BinaryWriter bwrite)
            {
                bwrite.Write(bfType);
                bwrite.Write(bfSize);
                bwrite.Write(bfReserved1);
                bwrite.Write(bfReserved2);
                bwrite.Write(bfOffBits);
            }
        }

        /// <summary>
        /// BITMAPINFOHEADERクラス
        /// </summary>
        class BITMAPINFOHEADER
        {
            public UInt32 biSize;
            public UInt32 biWidth;
            public UInt32 biHeight;
            public UInt16 biPlanes;
            public UInt16 biBitCount;
            public UInt32 biCompression;
            public UInt32 biSizeImage;
            public UInt32 biXPelsPerMeter;
            public UInt32 biYPelsPerMeter;
            public UInt32 biClrUsed;
            public UInt32 biClrImportant;

            /// <summary>
            /// BITMAPINFOHEADERサイズ
            /// </summary>
            public const int HEADER_SIZE = 40;

            public BITMAPINFOHEADER()
            {
                biSize = HEADER_SIZE;
                biPlanes = 1;
                biWidth = 0;
                biHeight = 0;
                biPlanes = 0;
                biBitCount = 0;
                biCompression = 0;
                biSizeImage = 0;
                biXPelsPerMeter = 0;
                biYPelsPerMeter = 0;
                biClrUsed = 0;
                biClrImportant = 0;
            }

            /// <summary>
            /// 最低限必要な設定がされているかをチェック
            /// </summary>
            /// <returns></returns>
            public bool CheckHeader()
            {
                if (biWidth == 0 || biHeight == 0 || biBitCount == 0)
                {
                    return false;
                }

                return true;
            }

            /// <summary>
            /// BITMAPINFOHEADERの書き込み
            /// </summary>
            /// <param name="bwrite">BinaryWriterインスタンス</param>
            public void WriteHeader(BinaryWriter bwrite)
            {
                bwrite.Write(biSize);
                bwrite.Write(biWidth);
                bwrite.Write(biHeight);
                bwrite.Write(biPlanes);
                bwrite.Write(biBitCount);
                bwrite.Write(biCompression);
                bwrite.Write(biSizeImage);
                bwrite.Write(biXPelsPerMeter);
                bwrite.Write(biYPelsPerMeter);
                bwrite.Write(biClrUsed);
                bwrite.Write(biClrImportant);
            }
        }

        /// <summary>
        /// RGBAデータクラス(Byte)
        /// </summary>
        class RGBAQUAD
        {
            public byte r;
            public byte g;
            public byte b;
            public byte a;

            public RGBAQUAD()
            {
                r = g = b = a = 0;
            }

            public RGBAQUAD(byte r, byte g, byte b, byte a)
            {
                this.r = r;
                this.g = g;
                this.b = b;
                this.a = a;
            }

            public RGBAQUAD(UInt32 rgba)
            {
                r = MASK.getR8(rgba);
                g = MASK.getG8(rgba);
                b = MASK.getB8(rgba);
                a = MASK.getA8(rgba);
            }

            /// <summary>
            /// RGB取得
            /// </summary>
            /// <returns>RGB値</returns>
            public UInt32 GetRGB() { return MASK.getRGBA8888(r, g, b, 0xff);}

            /// <summary>
            /// RGBA取得
            /// </summary>
            /// <returns>RGBA値</returns>
            public UInt32 GetRGBA() { return MASK.getRGBA8888(r, g, b, a); }

            // 明示的なUInt32への変換
            public static explicit operator UInt32(RGBAQUAD rgba)
            {
                return rgba.GetRGBA();
            }
        }

        /// <summary>
        /// RGBAデータクラス(float)
        /// </summary>
        class RGBAQUADF
        {
            public float r;
            public float g;
            public float b;
            public float a;

            RGBAQUADF()
            {
                r = g = b = a = 0.0f;
            }

            RGBAQUADF(float r, float g, float b, float a)
            {
                this.r = r;
                this.g = g;
                this.b = b;
                this.a = a;
            }

            RGBAQUADF(UInt32 rgba)
            {
                r = (float)MASK.getR8(rgba) / 255.0f;
                g = (float)MASK.getG8(rgba) / 255.0f;
                b = (float)MASK.getB8(rgba) / 255.0f;
                a = (float)MASK.getA8(rgba) / 255.0f;
            }

            // 明示的なUInt32への変換
            public static explicit operator UInt32(RGBAQUADF rgbaf)
            {
                UInt32 rgba = 0;
                rgba |= ((UInt32)(rgbaf.r * 255.0f) & 0xff);
                rgba |= ((UInt32)(rgbaf.g * 255.0f) & 0xff) << 8;
                rgba |= ((UInt32)(rgbaf.b * 255.0f) & 0xff) << 16;
                rgba |= ((UInt32)(rgbaf.a * 255.0f) & 0xff) << 24;
                return rgba;
            }
        }
    }
}
