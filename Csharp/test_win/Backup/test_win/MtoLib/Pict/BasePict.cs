using System;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;

namespace MtoLib
{
    namespace Pict
    {
        /// <summary>
        /// 画像データのベースクラス
        /// 対応フォーマットを増やす場合はこれを継承して作成する
        /// </summary>
        class BasePict
        {
            protected byte[] m_pImage;	        // ピクセルデータ
            protected int m_ImageSize;	        // ピクセルデータサイズ
            protected ushort m_ImageBit;        // ピクセルデータビット数
 
            protected byte[] m_pPalette;    	// パレットデータ
            protected int m_PaletteSize;    	// パレットデータサイズ
            protected ushort m_PaletteBit;      // パレットデータビット数
            protected ushort m_PaletteColor;    // パレットデータの色数

            protected ushort m_ImageW;          // 画像の幅
            protected ushort m_ImageH;          // 画像の高さ
            protected TGA.LINE m_Line;          // ビット配列


            /**--------------------------------------------------------------------------
             * 公開
             *--------------------------------------------------------------------------*/
            /// <summary>
            /// コンストラクタ
            /// </summary>
            public BasePict()
            {
                m_pImage = null;
                m_ImageSize = 0;
                m_ImageBit = 0;

                m_pPalette = null;
                m_PaletteSize = 0;
                m_PaletteBit = 0;
                m_PaletteColor = 0;

                m_ImageW = 0;
                m_ImageH = 0;
                m_Line = TGA.LINE.IMAGE_LINE_LRUD;
            }

            public BasePict(BasePict pict)
            {
                m_pImage = null;
                m_ImageSize = pict.m_ImageSize;
                m_ImageBit = pict.m_ImageBit;

                m_pPalette = null;
                m_PaletteSize = pict.m_PaletteSize;
                m_PaletteBit = pict.m_PaletteBit;
                m_PaletteColor = pict.m_PaletteColor;

                m_ImageW = pict.m_ImageW;
                m_ImageH = pict.m_ImageH;
                m_Line = pict.m_Line;
            }

            /// <summary>
            /// ピクセルサイズの取得
            /// </summary>
            /// <returns>ピクセルサイズ</returns>
            public int getImageSize() { return m_ImageSize; }

            /// <summary>
            /// パレットサイズの取得
            /// </summary>
            /// <returns>パレットサイズ</returns>
            public int getPaletteSize() { return m_PaletteSize; }

            /// <summary>
            /// ピクセルのビット数を取得
            /// </summary>
            /// <returns>ビット数</returns>
            public ushort getImageBit() { return m_ImageBit; }

            /// <summary>
            /// パレットのビット数を取得
            /// </summary>
            /// <returns>ビット数</returns>
            public ushort getPaletteBit() { return m_PaletteBit; }

            /// <summary>
            /// パレットの色数を取得
            /// </summary>
            /// <returns>パレットの色数</returns>
            public ushort getPaletteColor() { return m_PaletteColor; }

            /// <summary>
            /// 画像の幅を取得
            /// </summary>
            /// <returns>幅</returns>
            public ushort getWidth() { return m_ImageW; }

            /// <summary>
            /// 画像の高さを取得
            /// </summary>
            /// <returns>高さ</returns>
            public ushort getHeight() { return m_ImageH; }

            /// <summary>
            /// ビット配列の取得
            /// </summary>
            /// <returns>ビット配列</returns>
            public TGA.LINE getLine() { return m_Line; }

            /// <summary>
            /// BGRA配列をRGBA配列に変更
            /// 備考：RGBAに変更したあと再度呼ぶとBGRA配列になります
            /// </summary>
            /// <returns>変換の成否</returns>
            public bool ConvertRGBA()
            {
                if (m_ImageBit <= 8)
                {
                    return ConvertRGBA(ref m_pPalette, m_PaletteSize, m_PaletteBit);
                }
                else
                {
                    return ConvertRGBA(ref m_pImage, m_ImageSize, m_ImageBit);
                }
            }

            /// <summary>
            /// 指定のビット配列に変換
            /// ※LINEはTGAクラスのものを使用
            /// </summary>
            /// <param name="line">変換するビット配列</param>
            /// <returns>変換の成否</returns>
            public bool ConvertBitType(TGA.LINE line)
            {
                if (line >= TGA.LINE.IMAGE_LINE_MAX) return false;
                if (m_pImage == null || m_ImageSize == 0) return false;

                byte srcDiscripter = (byte)((byte)m_Line & 0xf0);
                byte dstDiscripter = (byte)((byte)line & 0xf0);

                // 同じなら処理をしない
                if (srcDiscripter == dstDiscripter) return true;

                // 配列変換
                try
                {
                    int tx, ty;
                    int ofsSrc, ofsDst;
                    byte[] pImage = new byte[m_ImageSize];

                    ofsDst = 0;
                    for (int y = 0; y < m_ImageH; y++)
                    {
                        ty = y;
                        if ((srcDiscripter & 0x20) != (dstDiscripter & 0x20))
                        {
                            // お互いのY方向が一致しないなら反転
                            ty = m_ImageH - y - 1;
                        }

                        for (int x = 0; x < m_ImageW; x++)
                        {
                            tx = x;
                            if ((srcDiscripter & 0x10) != (dstDiscripter & 0x10))
                            {
                                // お互いのX方向が一致しないなら反転
                                tx = m_ImageW - x - 1;
                            }
                            ofsSrc = (ty * m_ImageW) + tx;

                            switch (m_ImageBit)
                            {
                                case 8:
                                    pImage[ofsDst++] = m_pImage[ofsSrc];
                                    break;

                                case 16:
                                    ofsSrc *= 2;
                                    byte[] src = BitConverter.GetBytes(BitConverter.ToUInt16(m_pImage, ofsSrc));
                                    pImage[ofsDst++] = src[0];
                                    pImage[ofsDst++] = src[1];
                                    break;

                                case 24:
                                    ofsSrc *= 3;
                                    pImage[ofsDst++] = m_pImage[ofsSrc++];
                                    pImage[ofsDst++] = m_pImage[ofsSrc++];
                                    pImage[ofsDst++] = m_pImage[ofsSrc++];
                                    break;

                                case 32:
                                    ofsSrc *= 4;
                                    pImage[ofsDst++] = m_pImage[ofsSrc++];
                                    pImage[ofsDst++] = m_pImage[ofsSrc++];
                                    pImage[ofsDst++] = m_pImage[ofsSrc++];
                                    pImage[ofsDst++] = m_pImage[ofsSrc++];
                                    break;
                            }
                        }
                    }

                    m_pImage = pImage;
                    m_Line = (TGA.LINE)(((byte)m_Line & 0x0f) + dstDiscripter);
                }
                catch
                {
                    return false;
                }

                return true;
            }

            /// <summary>
            /// Tim2Clutを通常の並びに変換
            /// 備考：通常の並びのパレットを渡すとTim2Clutの並びになります
            /// </summary>
            /// <returns>変換の成否</returns>
            public bool ConvertTim2Clut()
            {
                if (m_pPalette == null || m_PaletteSize == 0) return false;

                // 並びの変換
                try
                {
                    // メモリ確保
                    int bitcount = m_PaletteColor;
                    byte[] pltDat = new byte[m_PaletteSize];

                    // パレット変換
                    int bcnt = 0;
                    int clutOffset = 0;
                    int pltOffset = 0;

                    switch (m_PaletteBit)
                    {
                        case 16:
                            byte r, g, b, a;

                            for (int i = 0; i < bitcount; i++)
                            {
                                UInt16 rgba = BitConverter.ToUInt16(m_pPalette, clutOffset);
                                a = MASK.getA1(rgba);
                                r = MASK.getR5(rgba);
                                g = MASK.getG5(rgba);
                                b = MASK.getB5(rgba);

                                // 16Bit->8bit変換＆コピー
                                byte[] rgba16 = BitConverter.GetBytes(MASK.getRGBA5551(b, g, r, a));
                                pltDat[pltOffset++] = rgba16[0];
                                pltDat[pltOffset++] = rgba16[1];
                                clutOffset += 2;
                                bcnt += 2;

                                // 16色は変換なし
                                if (bitcount == 16) continue;

                                // 64バイト単位で16～31バイトと32～47バイトの領域が入れ替わる
                                switch (bcnt)
                                {
                                    case 16:
                                        clutOffset += 16;
                                        break;
                                    case 32:
                                        clutOffset -= 32;
                                        break;
                                    case 48:
                                        clutOffset += 16;
                                        break;
                                    case 64:
                                        bcnt = 0;
                                        break;
                                }
                            }
                            break;

                        case 24:
                            for (int i = 0; i < bitcount; i++)
                            {
                                pltDat[pltOffset++] = m_pPalette[clutOffset++];
                                pltDat[pltOffset++] = m_pPalette[clutOffset++];
                                pltDat[pltOffset++] = m_pPalette[clutOffset++];
                                bcnt += 3;

                                // 16色は変換なし
                                if (bitcount == 16) continue;

                                // 96バイト単位で24～47バイトと48～71バイトの領域が入れ替わる
                                switch (bcnt)
                                {
                                    case 24:
                                        clutOffset += 24;
                                        break;
                                    case 48:
                                        clutOffset -= 48;
                                        break;
                                    case 72:
                                        clutOffset += 24;
                                        break;
                                    case 96:
                                        bcnt = 0;
                                        break;
                                }
                            }
                            break;

                        case 32:
                            for (int i = 0; i < bitcount; i++)
                            {
                                pltDat[pltOffset++] = m_pPalette[clutOffset++];
                                pltDat[pltOffset++] = m_pPalette[clutOffset++];
                                pltDat[pltOffset++] = m_pPalette[clutOffset++];
                                pltDat[pltOffset++] = m_pPalette[clutOffset++];
                                bcnt += 4;

                                // 16色は変換なし
                                if (bitcount == 16) continue;

                                //128バイト単位で32～63バイトと64～95バイトの領域が入れ替わる
                                switch (bcnt)
                                {
                                    case 32:
                                        clutOffset += 32;
                                        break;
                                    case 64:
                                        clutOffset -= 64;
                                        break;
                                    case 96:
                                        clutOffset += 32;
                                        break;
                                    case 128:
                                        bcnt = 0;
                                        break;
                                }
                            }
                            break;
                    }

                    m_pPalette = pltDat;
                }
                catch
                {
                    System.Diagnostics.Debug.WriteLine("パレットへの変換に失敗");
                    return false;
                }

                return true;
            }

            /// <summary>
            /// Bitmapクラスへの変換
            /// </summary>
            /// <param name="destBmp">保存先</param>
            /// <returns>RESULTタイプ</returns>
            public RESULT ConvertBMP(ref Bitmap destBmp)
            {
                // 作成されていない or 読み込まれていない
                if (m_pImage == null || m_ImageSize == 0) return RESULT.ERROR_IMAGE;

                // パレットありなのに設定されていない
                if (m_PaletteColor != 0 && m_pPalette == null) return RESULT.ERROR_PALETTE;

                // ピクセルフォーマットの設定
                PixelFormat pixfmt;
                if (m_PaletteColor != 0)
                {
                    pixfmt = m_PaletteColor == 16 ? PixelFormat.Format4bppIndexed : PixelFormat.Format8bppIndexed;
                }
                else
                {
                    pixfmt = Utility.getPixelFormat(m_ImageBit);
                }
                
                // 配列が変わるのでコピーを作成
                BasePict bmpPict = new BasePict(this);
                try
                {
                    bmpPict.m_pImage = new byte[bmpPict.m_ImageSize];
                    Array.Copy(m_pImage, 0, bmpPict.m_pImage, 0, m_ImageSize);

                    if (bmpPict.m_PaletteColor != 0)
                    {
                        bmpPict.m_pPalette = new byte[bmpPict.m_PaletteSize];
                        Array.Copy(m_pPalette, 0, bmpPict.m_pPalette, 0, m_PaletteSize);
                    }
                }
                catch
                {
                    System.Diagnostics.Debug.WriteLine("出力用のメモリ確保に失敗");
                    return RESULT.ERROR_MEMORY;
                }

                // 左→右、下→上配列に変換
                if (!bmpPict.ConvertBitType(TGA.LINE.IMAGE_LINE_LRUD)) return RESULT.ERROR_OUTPUT;

                // BMPに変換
                try
                {
                    Bitmap bmp = new Bitmap(bmpPict.m_ImageW, bmpPict.m_ImageH, pixfmt);

                    // パレットあり？
                    if (bmpPict.m_PaletteColor != 0)
                    {
                        // パレットの設定
                        int offset = 0;
                        ColorPalette pal = bmp.Palette;

                        // RGBAが逆っぽい
                        if (bmpPict.m_PaletteBit == 24)
                        {
                            for (int i = 0; i < bmpPict.m_PaletteColor; i++)
                            {
                                pal.Entries[i] = Color.FromArgb(bmpPict.m_pPalette[offset + 2],
                                                                bmpPict.m_pPalette[offset + 1],
                                                                bmpPict.m_pPalette[offset + 0]);
                                offset += 3;
                            }
                        }
                        else if (bmpPict.m_PaletteBit == 32)
                        {
                            for (int i = 0; i < bmpPict.m_PaletteColor; i++)
                            {
                                pal.Entries[i] = Color.FromArgb(bmpPict.m_pPalette[offset + 3],
                                                                bmpPict.m_pPalette[offset + 2],
                                                                bmpPict.m_pPalette[offset + 1],
                                                                bmpPict.m_pPalette[offset + 0]);
                                offset += 4;
                            }
                        }
                        else if (bmpPict.m_PaletteBit == 16)
                        {
                            UInt16 rgba;
                            for (int i = 0; i < bmpPict.m_PaletteColor; i++)
                            {
                                rgba = BitConverter.ToUInt16(bmpPict.m_pPalette, offset);
                                offset += 2;

                                pal.Entries[i] = Color.FromArgb((MASK.getA1(rgba) * 0xff),
                                                                (MASK.getR5(rgba) << 3),
                                                                (MASK.getG5(rgba) << 3),
                                                                (MASK.getB5(rgba) << 3));
                            }
                        }
                        else
                        {
                            return RESULT.ERROR_PALETTE;
                        }

                        bmp.Palette = pal;
                    }

                    // イメージの設定
                    // ※LockBitsを使う以外方法がなっそうなのでそれで対応
                    Rectangle rect = new Rectangle(0, 0, bmpPict.m_ImageW, bmpPict.m_ImageH);
                    BitmapData bmpData = bmp.LockBits(rect, ImageLockMode.WriteOnly, pixfmt);
                    {
                        IntPtr bmpPtr = bmpData.Scan0;
                        int bmpStride = bmpData.Stride;
                        {
#if DEBUG
                            // DEBUG
                            int imageW = bmpPict.m_ImageW * (bmpPict.m_ImageBit >> 3);
                            System.Diagnostics.Debug.Assert((bmpStride == imageW),
                                "ConvertBMP:画像幅が一致しない:" + bmpStride + " == " + imageW + "\n");
#endif
                        }
                        System.Runtime.InteropServices.Marshal.Copy(bmpPict.m_pImage, 0, bmpPtr, bmpPict.m_ImageSize);
                    }
                    bmp.UnlockBits(bmpData);

                    // 変換できたインスタンスを渡す
                    destBmp = bmp;
                }
                catch
                {
                    return RESULT.ERROR_CONVERT;
                }

                return RESULT.ERROR_NONE;
            }

            /// <summary>
            /// BMPに変換して出力
            /// </summary>
            /// <param name="fileName">出力ファイル名</param>
            /// <returns>RESULTタイプ</returns>
            public RESULT OutputBMP(string fileName)
            {
                // 独自のBITMAPFILEHEADERとBITMAPINFOHEADERを使って出力します
                // ※Silverlightなどアンセーフを使えない環境用

                // 作成されていない or 読み込まれていない
                if (m_pImage == null || m_ImageSize == 0) return RESULT.ERROR_IMAGE;

                // パレットありなのに設定されていない
                if (m_PaletteColor != 0 && m_pPalette == null) return RESULT.ERROR_PALETTE;

                // 配列が変わるのでコピーを作成
                BasePict bmpPict = new BasePict(this);

                try
                {
                    bmpPict.m_pImage = new byte[bmpPict.m_ImageSize];
                    Array.Copy(m_pImage, 0, bmpPict.m_pImage, 0, m_ImageSize);

                    if (bmpPict.m_PaletteColor != 0)
                    {
                        bmpPict.m_pPalette = new byte[bmpPict.m_PaletteSize];
                        Array.Copy(m_pPalette, 0, bmpPict.m_pPalette, 0, m_PaletteSize);
                    }
                }
                catch
                {
                    System.Diagnostics.Debug.WriteLine("出力用のメモリ確保に失敗");
                    return RESULT.ERROR_MEMORY;
                }

                // 左→右、下→上配列に変換
                if (!bmpPict.ConvertBitType(TGA.LINE.IMAGE_LINE_LRDU)) return RESULT.ERROR_OUTPUT;

                try
                {
                    // BMPヘッダーの作成
                    BITMAPFILEHEADER bmHead = new BITMAPFILEHEADER();
                    BITMAPINFOHEADER bmInfo = new BITMAPINFOHEADER();

                    int paletteSize = bmpPict.m_PaletteColor << 2;
                    bmHead.bfSize += (UInt32)(bmpPict.m_ImageSize + paletteSize);
                    bmHead.bfOffBits += (UInt32)paletteSize;

                    bmInfo.biWidth = bmpPict.m_ImageW;
                    bmInfo.biHeight = bmpPict.m_ImageH;
                    bmInfo.biBitCount = (UInt16)bmpPict.m_ImageBit;

                    // BMP出力
                    using (FileStream fs = new FileStream(fileName, FileMode.Create, FileAccess.Write))
                    {
                        using (BinaryWriter bwrite = new BinaryWriter(fs))
                        {
                            // ヘッダー
                            bmHead.WriteHeader(bwrite);
                            bmInfo.WriteHeader(bwrite);

                            // パレット
                            if (bmpPict.m_pPalette != null)
                            {
                                if (bmpPict.m_PaletteBit == 32) 
                                {
                                    bwrite.Write(bmpPict.m_pPalette, 0, bmpPict.m_PaletteSize);
                                }
                                else if (bmpPict.m_PaletteBit == 24)
                                {
                                    // αを追加する
                                    int ofs = 0;
                                    const byte A = 0xff; // 見た目的に0x00の方がいい？
                                    for (int i = 0; i < bmpPict.m_PaletteColor; i++)
                                    {
                                        bwrite.Write(bmpPict.m_pPalette[ofs++]);
                                        bwrite.Write(bmpPict.m_pPalette[ofs++]);
                                        bwrite.Write(bmpPict.m_pPalette[ofs++]);
                                        bwrite.Write(A);
                                    }
                                }
                                else if (bmpPict.m_PaletteBit == 16)
                                {
                                    int offset = 0;
                                    byte r, g, b, a;
                                    UInt16 rgba;

                                    for (int i = 0; i < bmpPict.m_PaletteColor; i++)
                                    {
                                        rgba = BitConverter.ToUInt16(bmpPict.m_pPalette, offset);
                                        offset += 2;

                                        a = (byte)(MASK.getA1(rgba) * 0xff);
                                        r = (byte)(MASK.getR5(rgba) << 3);
                                        g = (byte)(MASK.getG5(rgba) << 3);
                                        b = (byte)(MASK.getB5(rgba) << 3);

                                        byte[] plt = BitConverter.GetBytes(MASK.getRGBA8888(r, g, b, a));
                                        bwrite.Write(plt[2]);
                                        bwrite.Write(plt[1]);
                                        bwrite.Write(plt[0]);
                                        bwrite.Write(plt[3]);
                                    }
                                }
                                else
                                {
                                    System.Diagnostics.Debug.WriteLine("対応していないビット数のパレット");
                                    return RESULT.ERROR_PALETTE;
                                }
                            }

                            // イメージ
                            bwrite.Write(bmpPict.m_pImage, 0, bmpPict.m_ImageSize);

                            /*
                            // 4バイトサイズに調整
                            UInt32 diffSize = Utility.BOUND((int)bmHead.bfSize, 4) - bmHead.bfSize;
                            if (0 < diffSize)
                            {
                                byte[] padd = new byte[diffSize];
                                bwrite.Write(padd, 0, padd.Length);
                                bmHead.bfSize += diffSize;
                            }
                            */
                        }
                    }
                }
                catch
                {
                    return RESULT.ERROR_OUTPUT;
                }

                return RESULT.ERROR_NONE;
            }


            /**--------------------------------------------------------------------------
             * 非公開
             *--------------------------------------------------------------------------*/
            /// <summary>
            /// BGRA配列をRGBA配列に変更
            /// 備考：RGBAに変更したあと再度呼ぶとBGRA配列になります
            /// </summary>
            /// <param name="pDat">Image/Paletteデータ</param>
            /// <param name="datSize">データサイズ</param>
            /// <param name="datBit">データのビット数</param>
            /// <returns>変換の成否</returns>
            private bool ConvertRGBA(ref byte[] pDat, int datSize, int datBit)
            {
                if (pDat == null || datSize == 0) return false;

                int offset;
                int byteSize = datBit >> 3;
                byte r, g, b, a;

                if (datBit == 24 || datBit == 32)
                {
                    for (offset = 0; offset < datSize; offset += byteSize)
                    {
                        r = pDat[offset + 2];
                        b = pDat[offset + 0];
                        pDat[offset + 2] = b;
                        pDat[offset + 0] = r;
                    }
                }
                else if (datBit == 16)
                {
                    UInt16 rgba;
                    byte[] rgba16;

                    // RGBA:5551
                    for (offset = 0; offset < datSize; offset += byteSize)
                    {
                        rgba = BitConverter.ToUInt16(pDat, offset);
                        a = MASK.getA1(rgba);
                        r = MASK.getR5(rgba);
                        g = MASK.getG5(rgba);
                        b = MASK.getB5(rgba);

                        // 16Bit->8bit変換＆コピー
                        rgba16 = BitConverter.GetBytes(MASK.getRGBA5551(b, g, r, a));
                        pDat[offset + 0] = rgba16[0];
                        pDat[offset + 1] = rgba16[1];
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("非対応のビット数です");
                    return false;
                }

                return true;
            }
        }
    }
}
