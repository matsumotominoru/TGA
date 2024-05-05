using System;
using System.Text;
using System.IO;

namespace MtoLib
{
    namespace Pict
    {
        /// <summary>
        /// Tim2FileHeaderクラス
        /// </summary>
        public class Tim2FileHeader
        {
            /// <summary>
            /// ファイルIDの種類
            /// </summary>
            public enum FILEID
            {
                /// <summary>
                /// TIM2
                /// </summary>
                TIM2 = 0,

                /// <summary>
                /// CLT2(※2010/12/24時点非対応)
                /// </summary>
                CLT2,

                /// <summary>
                /// 非対応
                /// </summary>
                NONE
            }

            public byte[] FileId;       // ファイルID('T','I','M','2'または'C','L','T','2')
            public byte FormatVersion;  // ファイルフォーマットバージョン番号
            public byte FormatId;       // フォーマットID
            public ushort Pictures;     // ピクチャデータの個数
            public ulong Reserved;      // パディング(0x00固定, 8byte)

            /// <summary>
            /// ファイルIDサイズ('T','I','M','2'または'C','L','T','2')
            /// </summary>
            public const int FILEID_SIZE = 4;

            /// <summary>
            /// Tim2FileHeaderサイズ
            /// </summary>
            public const int HEADER_SIZE = 16;

            /// <summary>
            /// 対応しているフォーマットバージョン
            /// </summary>
            public const int FORMAT_VER = 0x04;


            /// <summary>
            /// コンストラクタ
            /// </summary>
            public Tim2FileHeader()
            {
                // ファイルIDの設定
                string id = "TIM2";
                FileId = new byte[FILEID_SIZE];
                Array.Copy(Encoding.UTF8.GetBytes(id), FileId, FILEID_SIZE);

                FormatVersion = FORMAT_VER;
                FormatId = 0;
                Pictures = 0;
                Reserved = 0;
            }

            /// <summary>
            /// Tim2FileHeaderの読み込み
            /// </summary>
            /// <param name="pSrc">画像データ</param>
            /// <param name="offset">ヘッダーまでのオフセット</param>
            /// <returns>true:読み込み成功</returns>
            public bool ReadHeader(byte[] pSrc, int offset)
            {
                if (pSrc == null) return false;

                Array.Copy(pSrc, offset, FileId, 0, FILEID_SIZE);
                offset += FILEID_SIZE;

                FormatVersion = pSrc[offset++];
                FormatId = pSrc[offset++];
                Pictures = BitConverter.ToUInt16(pSrc, offset);

                return true;
            }

            /// <summary>
            /// Tim2FileHeaderの書き込み
            /// </summary>
            /// <param name="bwrite">BinaryWriterインスタンス</param>
            public void WriteHeader(BinaryWriter bwrite)
            {
                byte[] fileID = new byte[FILEID_SIZE];
                Array.Copy(FileId, fileID, FILEID_SIZE);
                bwrite.Write(fileID);
                bwrite.Write(FormatVersion);
                bwrite.Write(FormatId);
                bwrite.Write(Pictures);
                bwrite.Write(Reserved);
            }

            /// <summary>
            /// ファイルIDのチェック
            /// </summary>
            /// <returns>FILEIDの種類</returns>
            public FILEID CheckFileID()
            {
                string fileID = Encoding.UTF8.GetString(FileId);
                const string tim2 = "TIM2";
                const string clt2 = "CLT2";

                if (fileID == tim2)
                {
                    return FILEID.TIM2;
                }
                else if (fileID == clt2)
                {
                    return FILEID.CLT2;
                }

                return FILEID.NONE;
            }
        }

        /// <summary>
        /// Tim2PictureHeaderクラス
        /// </summary>
        class Tim2PictureHeader
        {
            public enum TYPE
            {
                IMAGE = 0,
                CLUT
            }

            public uint TotalSize;		    // ピクチャデータ全体のバイトサイズ
            public uint ClutSize;		    // CLUTデータ部のバイトサイズ
            public uint ImageSize;		    // イメージデータ部のバイトサイズ
            public ushort HeaderSize;		// ヘッダ部のバイトサイズ
            public ushort ClutColors;		// CLUTデータ部のトータル色数
            public byte PictFormat;		    // ピクチャ形式ID(0固定)
            public byte MipMapTextures;	    // MIPMAPテクスチャ枚数
            public byte ClutType;		    // CLUTデータ部種別
            public byte ImageType;		    // イメージデータ部種別
            public ushort ImageWidth;		// ピクチャ横サイズ
            public ushort ImageHeight;		// ピクチャ縦サイズ
            
            // PS2 GSレジスタデータは未使用
            public ulong GsTex0;			// GS TEX0レジスタデータ
            public long GsTex1;			    // GS TEX1レジスタデータ
            public uint GsRegs;			    // GS TEXA,FBA,PABEレジスタデータ
            public uint GsTexClut;		    // GS TEXCLUTレジスタデータ


            /// <summary>
            /// Tim2PictureHeaderサイズ
            /// </summary>
            public const int HEADER_SIZE = 48;

            /// <summary>
            /// 画像ビットテーブル
            /// </summary>
            private static ushort[] _bit = { 0, 16, 24, 32, 4, 8 };


            /// <summary>
            /// コンストラクタ
            /// </summary>
            public Tim2PictureHeader()
            {
                TotalSize = 0;
                ClutSize = 0;
                ImageSize = 0;
                HeaderSize = 0;
                ClutColors = 0;
                PictFormat = 0;
                MipMapTextures = 0;
                ClutType = 0;
                ImageType = 0;
                ImageWidth = 0;
                ImageHeight = 0;
                GsTex0 = 0;
                GsTex1 = 0;
                GsRegs = 0;
                GsTexClut = 0;		
            }

            /// <summary>
            /// Tim2PictureHeaderの読み込み
            /// </summary>
            /// <param name="pSrc">画像データ</param>
            /// <param name="offset">ヘッダーまでのオフセット</param>
            /// <returns>true:読み込み成功</returns>
            public bool ReadHeader(byte[] pSrc, int offset)
            {
                if (pSrc == null) return false;

                TotalSize = BitConverter.ToUInt32(pSrc, offset); offset += sizeof(UInt32);
                ClutSize = BitConverter.ToUInt32(pSrc, offset); offset += sizeof(UInt32);
                ImageSize = BitConverter.ToUInt32(pSrc, offset); offset += sizeof(UInt32);
                HeaderSize = BitConverter.ToUInt16(pSrc, offset); offset += sizeof(UInt16);
                ClutColors = BitConverter.ToUInt16(pSrc, offset); offset += sizeof(UInt16);
                PictFormat = pSrc[offset++];
                MipMapTextures = pSrc[offset++];
                ClutType = pSrc[offset++];
                ImageType = pSrc[offset++];
                ImageWidth = BitConverter.ToUInt16(pSrc, offset); offset += sizeof(UInt16);
                ImageHeight = BitConverter.ToUInt16(pSrc, offset); offset += sizeof(UInt16);

                return true;
            }

            /// <summary>
            /// Tim2PictureHeaderの書き込み
            /// </summary>
            /// <param name="bwrite">BinaryWriterインスタンス</param>
            public void WriteHeader(BinaryWriter bwrite)
            {
                bwrite.Write(TotalSize);
                bwrite.Write(ClutSize);
                bwrite.Write(ImageSize);
                bwrite.Write(HeaderSize);
                bwrite.Write(ClutColors);
                bwrite.Write(PictFormat);
                bwrite.Write(MipMapTextures);
                bwrite.Write(ClutType);
                bwrite.Write(ImageType);
                bwrite.Write(ImageWidth);
                bwrite.Write(ImageHeight);
                bwrite.Write(GsTex0);
                bwrite.Write(GsTex1);
                bwrite.Write(GsRegs);
                bwrite.Write(GsTexClut);
            }

            /// <summary>
            /// 画像ビット数の取得
            /// </summary>
            /// <param name="type">ImageType/ClutType</param>
            /// <returns>ビット数</returns>
            public ushort getBit(TYPE type)
            {
                byte idx;

                if (type == TYPE.CLUT)
                {
                    idx = (byte)(ClutType & 0x1f);
                }
                else
                {
                    idx = ImageType;
                }
                
                return _bit[idx];
            }
        }


        /// <summary>
        /// TIM2メインクラス
        /// </summary>
        class Tim2 : BasePict
        {
            private Tim2FileHeader m_FileHeader;
            private Tim2PictureHeader m_PictHeader;


            /// <summary>
            /// ファイルヘッダーの取得
            /// </summary>
            /// <returns>Tim2ファイルヘッダー</returns>
            public Tim2FileHeader getFileHeader() { return m_FileHeader; }

            /// <summary>
            /// イメージヘッダーの取得
            /// </summary>
            /// <returns>Tim2イメージヘッダー</returns>
            public Tim2PictureHeader getPictHeader() { return m_PictHeader; }


            /**--------------------------------------------------------------------------
             * 公開
             *--------------------------------------------------------------------------*/
            /// <summary>
            /// コンストラクタ
            /// </summary>
            public Tim2()
            {
                m_FileHeader = new Tim2FileHeader();
                m_PictHeader = new Tim2PictureHeader();
            }

            /// <summary>
            /// ファイルから読み込み
            /// </summary>
            /// <param name="fileName">TIM2ファイル名</param>
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
            /// <param name="size">画像サイズ</param>
            /// <returns></returns>
            public RESULT Create(byte[] pSrc, int size)
            {
                int offset = 0;

                // 既に作成されているならクリア
                if (m_pImage != null) this.Clear();

                // データがある？
                if (pSrc == null || size == 0) return RESULT.ERROR_HEADER;

                // ファイルヘッダーの読み込み
                if (!m_FileHeader.ReadHeader(pSrc, offset)) return RESULT.ERROR_HEADER;

                // サポート形式？
                if (!CheckSupport(m_FileHeader)) return RESULT.ERROR_HEADER;
                offset += Tim2FileHeader.HEADER_SIZE;

                // 画像ヘッダーの読み込み
                if (!m_PictHeader.ReadHeader(pSrc, offset)) return RESULT.ERROR_HEADER;
                offset += Tim2PictureHeader.HEADER_SIZE;

                // 情報の設定
                m_ImageW = m_PictHeader.ImageWidth;
                m_ImageH = m_PictHeader.ImageHeight;
                m_ImageBit = m_PictHeader.getBit(Tim2PictureHeader.TYPE.IMAGE);
                m_PaletteBit = m_PictHeader.getBit(Tim2PictureHeader.TYPE.CLUT);
                m_PaletteColor = m_PictHeader.ClutColors;
                m_Line = TGA.LINE.IMAGE_LINE_LRUD;

                // パレットの色数は16/256をサポート
                if (m_ImageBit <= 8 && m_PaletteColor > 256) return RESULT.ERROR_PALETTE;

                // イメージの読み込み
                if (!this.ReadImage(pSrc, size, ref offset))
                {
                    return RESULT.ERROR_IMAGE;
                }

                // Clutの読み込み
                if (!this.ReadClut(pSrc, size, ref offset))
                {
                    return RESULT.ERROR_PALETTE;
                }
                
                return RESULT.ERROR_NONE;
            }

            /// <summary>
            /// ファイル出力
            /// </summary>
            /// <param name="fileName">出力ファイル名</param>
            /// <returns></returns>
            public RESULT Output(string fileName)
            {
                // 作成されていない or 読み込まれていない
                if (m_pImage == null || m_ImageSize == 0) return RESULT.ERROR_NONE;

                // DEBUG:output filename
                System.Diagnostics.Debug.Write("TIM2 Output:" + fileName + "\n");

                try
                {
                    using (FileStream fs = new FileStream(fileName, FileMode.Create, FileAccess.Write))
                    {
                        using (BinaryWriter bwrite = new BinaryWriter(fs))
                        {
                            // ヘッダー出力
                            m_FileHeader.WriteHeader(bwrite);
                            m_PictHeader.WriteHeader(bwrite);

                            // イメージ出力
                            bwrite.Write(m_pImage, 0, m_ImageSize);

                            // パレット出力
                            if (m_pPalette != null)
                            {
                                bwrite.Write(m_pPalette, 0, m_PaletteSize);
                            }
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
            /// イメージの読み込み
            /// </summary>
            /// <param name="pSrc">画像データ</param>
            /// <param name="size">ファイルサイズ</param>
            /// <param name="offset">イメージデータまでの位置</param>
            /// <returns>読み込みの成否</returns>
            private bool ReadImage(byte[] pSrc, int size, ref int offset)
            {
                if ((offset + m_PictHeader.ImageSize) > size) return false;

                try
                {
                    m_pImage = new byte[m_PictHeader.ImageSize];
                    Array.Copy(pSrc, offset, m_pImage, 0, m_PictHeader.ImageSize);
                    offset += (int)m_PictHeader.ImageSize;
                    m_ImageSize = (int)m_PictHeader.ImageSize;

                    System.Diagnostics.Debug.Assert((offset <= size));
                }
                catch
                {
                    System.Diagnostics.Debug.WriteLine("イメージの読み込みに失敗");
                    return false;
                }

                return true;
            }

            /// <summary>
            /// Clutの読み込み
            /// </summary>
            /// <param name="pSrc">Clutデータ</param>
            /// <param name="size">ファイルサイズ</param>
            /// <param name="offset">Clutデータまでの位置</param>
            /// <returns>読み込みの成否</returns>
            private bool ReadClut(byte[] pSrc, int size, ref int offset)
            {
                // パレットなし？
                if (m_ImageBit > 8) return true;

                try {
                    m_pPalette = new byte[m_PictHeader.ClutSize];
                    Array.Copy(pSrc, offset, m_pPalette, 0, m_PictHeader.ClutSize);
                    offset += (int)m_PictHeader.ClutSize;
                    m_PaletteSize = (int)m_PictHeader.ClutSize;

                    System.Diagnostics.Debug.Assert((offset <= size));
                }
                catch
                {
                    System.Diagnostics.Debug.WriteLine("パレットの読み込みに失敗");
                    return false;
                }

                return true;
            }


            /**--------------------------------------------------------------------------
             * 静的メソッド
             *--------------------------------------------------------------------------*/
            /// <summary>
            /// 対応チェック
            /// </summary>
            /// <param name="fileHeader">ファイルヘッダー</param>
            /// <returns>true:サポートしている形式</returns>
            public static bool CheckSupport(Tim2FileHeader fileHeader)
            {
                // Tim2データ？
                if (fileHeader.CheckFileID() != Tim2FileHeader.FILEID.TIM2)
                {
                    return false;
                }

                // 対応バージョン？
                if (fileHeader.FormatVersion != Tim2FileHeader.FORMAT_VER)
                {
                    return false;
                }

                // ピクチャデータの個数 ※複数はサポートしていない
                if (fileHeader.Pictures != 1) return false;

                return true;
            }
        }
    }
}
