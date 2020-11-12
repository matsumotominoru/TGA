#ifndef _TGA_H_
#define _TGA_H_

class CTga {
public:
	// イメージタイプ
	enum {
		IMAGE_TYPE_NONE = 0,		// イメージなし
		IMAGE_TYPE_INDEX,			// 256色
		IMAGE_TYPE_FULL,			// フルカラー
		IMAGE_TYPE_GRAY,			// 白黒
		IMAGE_TYPE_MAX,
		IMAGE_TYPE_INDEX_RLE = 9,	// 256色(RLE圧縮)
		IMAGE_TYPE_FULL_RLE,		// フルカラー(RLE圧縮)
		IMAGE_TYPE_GRAY_RLE,		// 白黒(RLE圧縮)
		IMAGE_TYPE_RLE_MAX
	};

	// ピクセルの並び
	enum {
		IMAGE_LINE_LRDU = 0x00,		// 左→右、下→上
		IMAGE_LINE_RLDU = 0x10,		// 右→左、下→上
		IMAGE_LINE_LRUD = 0x20,		// 左→右、上→下
		IMAGE_LINE_RLUD = 0x30,		// 右→左、上→下
		IMAGE_LINE_MAX
	};

	enum {
		HEADER_SIZE = 0x12,			// ヘッダーサイズ
		FOOTER_SIZE = 0x1a			// フッターサイズ
	};

	// エラータイプ
	enum {
		ERROR_OPEN    = -1,			// ファイルオープン失敗
		ERROR_MEMORY  = -2,			// メモリ確保失敗
		ERROR_HEADER  = -3,			// サポートしていない形式
		ERROR_PALETTE = -4,			// パレットデータが不正
		ERROR_IMAGE   = -5,			// イメージデータが不正
		ERROR_OUTPUT  = -6,			// 出力エラー
		ERROR_NONE    =  1,			// エラーなし
		ERROR_MAX
	};

	struct TGAHeader {
		uint8	IDField;			// IDフィールドのサイズ
		uint8	usePalette;			// パレット使用？
		uint8	imageType;			// イメージ形式
		uint16	paletteIndex;		// パレットIndex
		uint16	paletteColor;		// パレットの色数
		uint8	paletteBit;			// 1パレットのビット数
		uint16	imageX;				// イメージX原点
		uint16	imageY;				// イメージY原点
		uint16	imageW;				// イメージ幅
		uint16	imageH;				// イメージ高さ
		uint8	imageBit;			// イメージビット数
		uint8	discripter;			// イメージ記述子
	};

	struct TGAFooter {
		uint32	filePos;			// ファイルの位置(エクステンションエリアの位置?)
		uint32	fileDev;			// developer directory ファイル位置
		uint8	version[18];		// ”TRUEVISION-TARGA”の文字（version[17]==0x00）
	};

private:
	TGAHeader	m_Header;
	TGAFooter	m_Footer;

	uint8		*m_pImage;			// ピクセルデータ
	uint8		*m_pPalette;		// パレットデータ
	uint32		m_ImageSize;		// ピクセルデータサイズ
	uint32		m_PaletteSize;		// パレットデータサイズ

private:
	void   Clear(void);
	bool   CheckSupport(const TGAHeader &header);
	bool   ReadHeader(const uint8 *pSrc);
	void   ReadFooter(const uint8 *pSrc, const uint32 offset);
	bool   CalcSize(const bool bFlg);
	bool   ReadImage(const uint8 *pSrc, const uint32 size, uint32 *pOffset);
	bool   ReadPalette(const uint8 *pSrc);
	uint32 UnpackRLE(uint8 *pDst, const uint8 *pSrc, const uint32 size);

public:
	CTga(void);
	virtual ~CTga(void);

	uint8 *getImage(void)        const {return m_pImage;}
	uint32 getImageSize(void)    const {return m_ImageSize;}
	uint8  getImageBit(void)     const {return m_Header.imageBit;}

	uint8 *getPalette(void)      const {return m_pPalette;}
	uint32 getPaletteSize(void)  const {return m_PaletteSize;}
	uint16 getPaletteColor(void) const {return m_Header.paletteColor;}
	uint8  getPaletteBit(void)   const {return m_Header.paletteBit;}

	uint16 getWidth(void)        const {return m_Header.imageW;}
	uint16 getHeight(void)       const {return m_Header.imageH;}

	TGAHeader getHeader(void)   const {return m_Header;}
	TGAFooter getFooter(void)   const {return m_Footer;}

	void setFilePos(const uint32 filePos) {m_Footer.filePos = filePos;}
	void setFileDev(const uint32 fileDev) {m_Footer.fileDev = fileDev;}

	int  Create(const char *pFileName);
	int  Create(const void *pSrc, const uint32 size);
	int  Create(const TGAHeader &header, uint8 *pImage, const uint32 imageSize, uint8 *pPalette, const uint32 paletteSize);
	int  Output(const char *pFileName);
	int  OutputBMP(const char *pFileName);
	bool ConvertRGBA(void);
	bool ConvertType(const sint32 type);

	bool WriteHeader(FILE *fp);
	bool WriteHeader(FILE *fp, TGAHeader *pHeader);
	bool WriteFooter(FILE *fp);
	bool WriteFooter(FILE *fp, TGAFooter *pHeader);
};

#endif
