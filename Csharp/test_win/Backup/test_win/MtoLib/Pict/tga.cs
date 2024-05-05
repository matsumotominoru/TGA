using System;
using System.Text;
using System.IO;

namespace MtoLib
{
    namespace Pict
    {
        /// <summary>
        /// TGAヘッダークラス
        /// </summary>
        class TGAHeader
        {
            public byte IDField;		    // IDフィールドのサイズ
            public byte usePalette;			// パレット使用？
            public byte imageType;			// イメージ形式
            public ushort paletteIndex;		// パレットIndex
            public ushort paletteColor;		// パレットの色数
            public byte paletteBit;			// 1パレットのビット数
            public ushort imageX;			// イメージX原点
            public ushort imageY;			// イメージY原点
            public ushort imageW;			// イメージ幅
            public ushort imageH;			// イメージ高さ
            public byte imageBit;			// イメージビット数
            public byte discripter;			// イメージ記述子


            /// <summary>
            /// TGAHeaderサイズ
            /// </summary>
            public const int HEADER_SIZE = 0x12;

            /// <summary>
            /// ピクセルサイズの取得
            /// </summary>
            /// <returns>ピクセルサイズ</returns>
            public int getImageSize()
            {
                return imageW * imageH * (imageBit >> 3);
            }

            /// <summary>
            /// パレットサイズの取得
            /// </summary>
            /// <returns>パレットサイズ(0:パレットなし)</returns>
            public int getPaletteSize()
            {
                return usePalette * paletteColor * (paletteBit >> 3);
            }

            /// <summary>
            /// TGAヘッダーの読み込み
            /// </summary>
            /// <param name="pSrc">画像データ</param>
            /// <param name="offset">ヘッダーまでのオフセット</param>
            /// <returns>true:読み込み成功</returns>
            public bool ReadHeader(byte[] pSrc, int offset)
            {
                if (pSrc == null) return false;

                int ofs = offset;
                IDField = pSrc[ofs++];
                usePalette = pSrc[ofs++];
                imageType = pSrc[ofs++];
                paletteIndex = BitConverter.ToUInt16(pSrc, ofs); ofs += sizeof(ushort);
                paletteColor = BitConverter.ToUInt16(pSrc, ofs); ofs += sizeof(ushort);
                paletteBit = pSrc[ofs++];
                imageX = BitConverter.ToUInt16(pSrc, ofs); ofs += sizeof(ushort);
                imageY = BitConverter.ToUInt16(pSrc, ofs); ofs += sizeof(ushort);
                imageW = BitConverter.ToUInt16(pSrc, ofs); ofs += sizeof(ushort);
                imageH = BitConverter.ToUInt16(pSrc, ofs); ofs += sizeof(ushort);
                imageBit = pSrc[ofs++];
                discripter = pSrc[ofs++];

                // DEBUG:サイズチェック
                string str = "TGAヘッダーサイズエラー" + ofs + " == " + HEADER_SIZE + "\n";
                System.Diagnostics.Debug.Assert(ofs == HEADER_SIZE, str);

                return TGA.CheckSupport(this);
            }

            /// <summary>
            /// TGAヘッダーの書き込み
            /// </summary>
            /// <param name="bwrite">BinaryWriterインスタンス</param>
            public void WriteHeader(BinaryWriter bwrite)
            {
                bwrite.Write(IDField);
                bwrite.Write(usePalette);
                bwrite.Write(imageType);
                bwrite.Write(paletteIndex);
                bwrite.Write(paletteColor);
                bwrite.Write(paletteBit);
                bwrite.Write(imageX);
                bwrite.Write(imageY);
                bwrite.Write(imageW);
                bwrite.Write(imageH);
                bwrite.Write(imageBit);
                bwrite.Write(discripter);
            }
        }


        /// <summary>
        /// TGAフッタークラス
        /// </summary>
        class TGAFooter
        {
            public uint filePos;			// ファイルの位置(エクステンションエリアの位置?)
            public uint fileDev;			// developer directory ファイル位置
            public byte[] version;	    	// "TRUEVISION-TARGA"の文字（version[17]==0x00）

            /// <summary>
            /// TGAFooterサイズ
            /// </summary>
            public const int FOOTER_SIZE = 0x1a;

            /// <summary>
            /// フッターのバージョン配列サイズ
            /// </summary>
            public const int FOTTER_VER_SIZE = 18;


            /// <summary>
            /// コンストラクタ
            /// </summary>
            public TGAFooter()
            {
                string ver = "TRUEVISION-TARGA";
                version = new byte[FOTTER_VER_SIZE];
                
                // ASCIIへのエンコーディングはUTF8経由推奨みたい
                Array.Copy(Encoding.UTF8.GetBytes(ver), version, ver.Length);
                version[FOTTER_VER_SIZE - 1] = 0;
            }

            /// <summary>
            /// TGAフッター読み込み
            /// </summary>
            /// <param name="pSrc">画像データ</param>
            /// <param name="offset">フッターまでのオフセット</param>
            /// <returns>true:読み込み成功</returns>
            public bool ReadFooter(byte[] pSrc, int offset)
            {
                if (pSrc == null) return false;

                int ofs = offset;
                filePos = BitConverter.ToUInt32(pSrc, ofs); ofs += sizeof(UInt32);
                fileDev = BitConverter.ToUInt32(pSrc, ofs); ofs += sizeof(UInt32);
                Array.Copy(pSrc, ofs, version, 0, FOTTER_VER_SIZE); ofs += FOTTER_VER_SIZE;

                // DEBUG:サイズチェック
                string str = "TGAフッターサイズエラー" + (ofs - offset) + " == " + FOOTER_SIZE + "\n";
                System.Diagnostics.Debug.Assert((ofs - offset) == FOOTER_SIZE);

                return true;
            }

            /// <summary>
            /// TGAフッターの書き込み
            /// </summary>
            /// <param name="bwrite">BinaryWriterインスタンス</param>
            public void WriteFooter(BinaryWriter bwrite)
            {
                bwrite.Write(filePos);
                bwrite.Write(fileDev);
                bwrite.Write(version, 0, FOTTER_VER_SIZE);
            }
        }


        /// <summary>
        /// TGAメインクラス
        /// </summary>
        class TGA : BasePict
        {
            /// <summary>
            /// イメージ形式
            /// </summary>
            public enum TYPE
            {
                /// <summary>
                /// イメージなし
                /// </summary>
                IMAGE_TYPE_NONE = 0,

                /// <summary>
                /// 256色
                /// </summary>
                IMAGE_TYPE_INDEX,

                /// <summary>
                /// フルカラー
                /// </summary>
                IMAGE_TYPE_FULL,

                /// <summary>
                /// 白黒
                /// </summary>
                IMAGE_TYPE_GRAY,
                IMAGE_TYPE_MAX,

                /// <summary>
                /// 256色(RLE圧縮)
                /// </summary>
                IMAGE_TYPE_INDEX_RLE = 9,

                /// <summary>
                /// フルカラー(RLE圧縮)
                /// </summary>
                IMAGE_TYPE_FULL_RLE,

                /// <summary>
                /// 白黒(RLE圧縮)
                /// </summary>
                IMAGE_TYPE_GRAY_RLE,
                IMAGE_TYPE_RLE_MAX
            }

            /// <summary>
            /// イメージ記述子
            /// </summary>
            public enum LINE
            {
                /// <summary>
                /// 左→右、下→上
                /// </summary>
                IMAGE_LINE_LRDU = 0x00,

                /// <summary>
                /// 右→左、下→上
                /// </summary>
                IMAGE_LINE_RLDU = 0x10,

                /// <summary>
                /// 左→右、上→下
                /// </summary>
                IMAGE_LINE_LRUD = 0x20,

                /// <summary>
                /// 右→左、上→下
                /// </summary>
                IMAGE_LINE_RLUD = 0x30,

                IMAGE_LINE_MAX
            }

            /// <summary>
            /// プライベートメンバ変数
            /// </summary>
            private TGAHeader m_Header;
            private TGAFooter m_Footer;


            /**--------------------------------------------------------------------------
             * 公開
             *--------------------------------------------------------------------------*/
            /// <summary>
            /// コンストラクタ
            /// </summary>
            public TGA()
            {
                m_Header = new TGAHeader();
                m_Footer = new TGAFooter();
            }

            /// <summary>
            /// TGAヘッダーの取得
            /// </summary>
            /// <returns>TGAヘッダー</returns>
            public TGAHeader getHeader() { return m_Header; }

            /// <summary>
            /// TGAフッターの取得
            /// </summary>
            /// <returns>TGAフッター</returns>
            public TGAFooter getFotter() { return m_Footer; }

            /// <summary>
            /// フッターのファイルの位置設定
            /// </summary>
            /// <param name="filePos">ファイルの位置</param>
            public void setFilePos(UInt32 filePos) { m_Footer.filePos = filePos; }

            /// <summary>
            /// フッターのdeveloper directory ファイル位置設定
            /// </summary>
            /// <param name="fileDev">developer directory ファイル位置</param>
            public void setFileDev(UInt32 fileDev) { m_Footer.fileDev = fileDev; }

            /// <summary>
            /// ファイルから読み込み
            /// </summary>
            /// <param name="fileName">TGAファイル名</param>
            /// <returns>RESULTタイプ</returns>
            public RESULT Create(string fileName)
            {
                RESULT ret;

                try
                {
                    using (FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read))
                    {
                        int size = (int)fs.Length;
                        byte[] buf = new byte[size];

                        // ファイルの読み込み
                        fs.Read(buf, 0, size);

                        // メモリから作成
                        ret = this.Create(buf, size);
                    }
                }
                catch (FileNotFoundException)
                {
                    return RESULT.ERROR_OPEN;
                }
                catch
                {
                    return RESULT.ERROR_MEMORY;
                }

                return ret;
            }

            /// <summary>
            /// メモリから作成
            /// </summary>
            /// <param name="pSrc">画像データ</param>
            /// <param name="size">データサイズ</param>
            /// <returns>RESULTタイプ</returns>
            public RESULT Create(byte[] pSrc, int size)
            {
                int offset = 0;

                // 既に作成されているならクリア
                if (m_ImageSize != 0) this.Clear();

                // データが存在する？
                if (pSrc == null || size == 0)
                {
                    return RESULT.ERROR_HEADER;
                }

                // ヘッダーの読み込み
                if (!m_Header.ReadHeader(pSrc, offset)) return RESULT.ERROR_HEADER;

                // ImageとPaletteのサイズを求める
                if (!this.CalcSize(true))
                {
                    return RESULT.ERROR_MEMORY;
                }
                offset += TGAHeader.HEADER_SIZE + m_Header.IDField;

                // パレットの読み込み
                if (!this.ReadPalette(pSrc, size, ref offset))
                {
                    return RESULT.ERROR_PALETTE;
                }

                // イメージの読み込み
                if (!this.ReadImage(pSrc, size, ref offset))
                {
                    return RESULT.ERROR_IMAGE;
                }

                // フッターの読み込み
                if ((size - offset) >= TGAFooter.FOOTER_SIZE)
                {
                    m_Footer.ReadFooter(pSrc, offset);
                }

                return RESULT.ERROR_NONE;
            }

            /// <summary>
            /// 指定データから作成
            /// </summary>
            /// <param name="header">TGAヘッダー</param>
            /// <param name="pImage">イメージデータ</param>
            /// <param name="pPalette">パレットデータ</param>
            /// <returns>RESULTタイプ</returns>
            public RESULT Create(TGAHeader header, byte[] pImage, byte[] pPalette)
            {
                // 引数チェック
                if (pImage == null || header.getImageSize() == 0) return RESULT.ERROR_IMAGE;
                if (pPalette != null && header.getPaletteSize() == 0) return RESULT.ERROR_PALETTE;
                if (pPalette == null && header.getPaletteSize() != 0) return RESULT.ERROR_PALETTE;

                // ヘッダーチェック
                if (!CheckSupport(header)) return RESULT.ERROR_HEADER;

                // 情報の設定
                this.CalcSize(false);

                return RESULT.ERROR_NONE;
            }

            /// <summary>
            /// 指定のビット配列に変換
            /// </summary>
            /// <param name="line">ラインタイプ</param>
            /// <returns>配列変換の成否</returns>
            public new bool ConvertBitType(LINE line)
            {
                // MEMO:ヘッダーを書き換えるためにオーバーライドしています

                // 配列変換
                if (base.ConvertBitType(line))
                {
                    // イメージ記述子を変更
                    m_Header.discripter = (byte)m_Line;
                    return true;
                }

                return false;
            }

            /// <summary>
            /// ファイル出力
            /// </summary>
            /// <param name="fileName">出力ファイル名</param>
            /// <returns>RESULTタイプ</returns>
            public RESULT Output(string fileName)
            {
                // 作成されていない or 読み込まれていない
                if (m_pImage == null || m_ImageSize == 0) return RESULT.ERROR_NONE;

                // DEBUG:output filename
                System.Diagnostics.Debug.Write("TGA Output:" + fileName + "\n");

                try
                {
                    using (FileStream fs = new FileStream(fileName, FileMode.Create, FileAccess.Write))
                    {
                        // TODO:RLE圧縮出力の対応
                        //      2010/12/20時点では非対応
                        if ((byte)TYPE.IMAGE_TYPE_INDEX_RLE <= m_Header.imageType && m_Header.imageType < (byte)TYPE.IMAGE_TYPE_RLE_MAX)
                        {
                            m_Header.imageType -= 8; // 非圧縮にする
                        }

                        using (BinaryWriter bwrite = new BinaryWriter(fs))
                        {
                            // ヘッダー出力
                            m_Header.WriteHeader(bwrite);

                            // パレット出力
                            if (m_pPalette != null)
                            {
                                bwrite.Write(m_pPalette, 0, m_PaletteSize);
                            }

                            // イメージ出力
                            bwrite.Write(m_pImage, 0, m_ImageSize);

                            // フッターの出力
                            m_Footer.WriteFooter(bwrite);
                        }
                    }
                }
                catch (FileNotFoundException)
                {
                    System.Diagnostics.Debug.Write("           ...failed\n");
                    return RESULT.ERROR_OPEN;
                }
                catch
                {
                    System.Diagnostics.Debug.Write("           ...failed\n");
                    return RESULT.ERROR_OUTPUT;
                }

                // DEBUG:Result ok
                System.Diagnostics.Debug.Write("           ...sucsecc\n");

                return RESULT.ERROR_NONE;
            }


            /**--------------------------------------------------------------------------
             * 非公開
             *--------------------------------------------------------------------------*/
            /// <summary>
            /// データのクリア
            /// </summary>
            private void Clear()
            {
                m_pImage = null;
                m_pPalette = null;
                m_ImageSize = 0;
                m_PaletteSize = 0;
            }

            /// <summary>
            /// Image/Paletteサイズを求める
            /// </summary>
            /// <param name="bFlg">メモリ確保をする？</param>
            /// <returns></returns>
            private bool CalcSize(bool bFlg)
            {
                m_ImageSize = m_Header.getImageSize();
                m_PaletteSize = m_Header.getPaletteSize();

                if (bFlg)
                {
                    try
                    {
                        // イメージのメモリ確保
                        m_pImage = new byte[m_ImageSize];

                        // パレットがありならメモリ確保
                        if (m_PaletteSize > 0)
                        {
                            m_pPalette = new byte[m_PaletteSize];
                        }
                    }
                    catch
                    {
                        m_ImageSize = 0;
                        m_PaletteSize = 0;
                        return false;
                    }
                }

                // その他の情報設定
                m_ImageBit = m_Header.imageBit;
                m_PaletteBit = m_Header.paletteBit;
                m_PaletteColor = m_Header.paletteColor;
                m_ImageW = m_Header.imageW;
                m_ImageH = m_Header.imageH;
                m_Line = (LINE)m_Header.discripter;

                return true;
            }

            /// <summary>
            /// パレットの読み込み
            /// </summary>
            /// <param name="pSrc">画像データ</param>
            /// <param name="size">ファイルサイズ</param>
            /// <param name="offset">パレットまでのオフセット</param>
            /// <returns>読み込みの成否</returns>
            private bool ReadPalette(byte[] pSrc, int size, ref int offset)
            {
                // IndexColor?
                if (m_Header.imageType != (byte)TYPE.IMAGE_TYPE_INDEX && m_Header.imageType != (byte)TYPE.IMAGE_TYPE_INDEX_RLE)
                {
                    return true;
                }

                // Palette ok?
                if (m_pPalette == null) return false;
                if ((offset + m_PaletteSize) > size) return false;

                // パレットコピー
                if (m_Header.paletteBit == 24 || m_Header.paletteBit == 32)
                {
                    Array.Copy(pSrc, offset, m_pPalette, 0, m_PaletteSize);
                    offset += (int)m_PaletteSize;
                }
                else
                {
                    System.Diagnostics.Debug.Write("サポートされていないパレット形式:" + m_Header.paletteBit + "\n");
                    return false;
                }

                return true;
            }

            /// <summary>
            /// イメージの読み込み
            /// </summary>
            /// <param name="pSrc">画像データ</param>
            /// <param name="size">ファイルサイズ</param>
            /// <param name="offset">ピクセルまでのオフセット</param>
            /// <returns>読み込みの成否</returns>
            private bool ReadImage(byte[] pSrc, int size, ref int offset)
            {
                if ((byte)TYPE.IMAGE_TYPE_INDEX_RLE <= m_Header.imageType && m_Header.imageType < (byte)TYPE.IMAGE_TYPE_RLE_MAX)
                {
                    // RLE圧縮
                    if (!this.UnpackRLE(m_pImage, pSrc, size, ref offset))
                    {
                        return false;
                    }
                }
                else
                {
                    // 非圧縮
                    if ((offset + m_ImageSize) > size) return false;
                    Array.Copy(pSrc, offset, m_pImage, 0, m_ImageSize);
                    offset += (int)m_ImageSize;
                }
                return true;
            }

            /// <summary>
            /// RLE圧縮解凍
            /// </summary>
            /// <param name="pDst">展開先</param>
            /// <param name="pSrc">画像データ</param>
            /// <param name="size">ファイルサイズ</param>
            /// <param name="offset">ピクセルまでのオフセット</param>
            /// <returns>RLE圧縮解凍の成否</returns>
            private bool UnpackRLE(byte[] pDst, byte[] pSrc, int size, ref int offset)
            {
                bool bFlg;
                int i, j, loop;
                int count = 0;

                // バイトサイズ
                int byteSize = m_Header.imageBit >> 3;

                while (count < m_ImageSize)
                {
                    bFlg = ((pSrc[offset] & 0x80) != 0) ? false : true; // 上位ビットが0ならリテラルグループ
                    loop = (pSrc[offset] & 0x7f) + 1;
                    offset++;

                    if (bFlg)
                    {
                        // リテラルグループ
                        // 制御バイトの後ろ（pSrc[offset] & 0x7f)+1個のデータ（ピクセルバイト単位）をコピーする
                        for (i = 0; i < loop; i++)
                        {
                            for (j = 0; j < byteSize; j++)
                            {
                                pDst[count++] = pSrc[offset++];
                            }
                        }
                    }
                    else
                    {
                        // 反復
                        // 次に続くデータバイト（ピクセルバイト単位）を（pSrc[offset] & 0x7f)+1回繰り返す
                        for (i = 0; i < loop; i++)
                        {
                            for (j = 0; j < byteSize; j++)
                            {
                                pDst[count++] = pSrc[offset + j];
                            }
                        }
                        offset += byteSize;
                    }

                    // 解凍のしすぎをチェック
                    if (offset > size)
                    {
                        System.Diagnostics.Debug.WriteLine("解凍失敗(" + offset + " > " + size);
                        return false;
                    }
                }

                System.Diagnostics.Debug.Assert(count == m_ImageSize, "解凍処理が不正(" + count + " == " + m_ImageSize + "\n");

                return true;
            }


            /**--------------------------------------------------------------------------
             * 静的メソッド
             *--------------------------------------------------------------------------*/
            /// <summary>
            /// 対応チェック
            /// </summary>
            /// <param name="header">TGAヘッダー構造体</param>
            /// <returns>true:サポートしている形式</returns>
            public static bool CheckSupport(TGAHeader header)
            {
                // 原点が0ではない？
                if (header.imageX != 0 && header.imageY != 0) return false;

                // 対応していないイメージタイプ
                TYPE type = (TYPE)header.imageType;
                if (!((TYPE.IMAGE_TYPE_NONE < type && type < TYPE.IMAGE_TYPE_MAX) ||
                      (TYPE.IMAGE_TYPE_INDEX_RLE <= type && type <= TYPE.IMAGE_TYPE_RLE_MAX)))
                {
                    return false;
                }

                // 対応ビット？
                if (header.imageBit !=  8 && header.imageBit != 16 &&
                    header.imageBit != 24 && header.imageBit != 32)
                {
                    return false;
                }

                // 対応パレット？
                if (header.usePalette == 1)
                {
                    if (header.paletteIndex != 0) return false;
                    if (header.paletteBit != 24 && header.paletteBit != 32) return false;
                }

                return true;
            }
        }
    }
 }
