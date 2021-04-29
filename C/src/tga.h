#ifndef _TGA_H_
#define _TGA_H_

// イメージタイプ
enum {
	TGA_IMAGE_TYPE_NONE = 0,		// イメージなし
	TGA_IMAGE_TYPE_INDEX,			// 256色
	TGA_IMAGE_TYPE_FULL,			// フルカラー
	TGA_IMAGE_TYPE_GRAY,			// 白黒
	TGA_IMAGE_TYPE_MAX,
	TGA_IMAGE_TYPE_INDEX_RLE = 9,	// 256色(RLE圧縮)
	TGA_IMAGE_TYPE_FULL_RLE,		// フルカラー(RLE圧縮)
	TGA_IMAGE_TYPE_GRAY_RLE,		// 白黒(RLE圧縮)
	TGA_IMAGE_TYPE_RLE_MAX
};

// ピクセルの並び
enum {
	TGA_IMAGE_LINE_LRDU = 0x00,		// 左→右、下→上
	TGA_IMAGE_LINE_RLDU = 0x10,		// 右→左、下→上
	TGA_IMAGE_LINE_LRUD = 0x20,		// 左→右、上→下
	TGA_IMAGE_LINE_RLUD = 0x30,		// 右→左、上→下
	TGA_IMAGE_LINE_MAX
};

enum {
	TGA_HEADER_SIZE = 0x12,			// ヘッダーサイズ
	TGA_FOOTER_SIZE = 0x1a			// フッターサイズ
};

// エラータイプ
enum {
	TGA_ERROR_OPEN    = -1,			// ファイルオープン失敗
	TGA_ERROR_MEMORY  = -2,			// メモリ確保失敗
	TGA_ERROR_HEADER  = -3,			// サポートしていない形式
	TGA_ERROR_PALETTE = -4,			// パレットデータが不正
	TGA_ERROR_IMAGE   = -5,			// イメージデータが不正
	TGA_ERROR_OUTPUT  = -6,			// 出力エラー
	TGA_ERROR_NONE    =  1,			// エラーなし
	TGA_ERROR_MAX
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

struct TGA {
	struct TGAHeader	header;
	struct TGAFooter	footer;

	uint8	*pImage;			// ピクセルデータ
	uint8	*pPalette;			// パレットデータ
	uint32	imageSize;			// ピクセルデータサイズ
	uint32	paletteSize;		// パレットデータサイズ
};

static MTOINLINE uint8 *tgaGetImage(const struct TGA *pTga)
{
	return pTga->pImage;
}

static MTOINLINE uint32 tgaGetImageSize(const struct TGA *pTga)
{
	 return pTga->imageSize;
}

static MTOINLINE uint8 tgaGetImageBit(const struct TGA *pTga)
{
	return pTga->header.imageBit;
}

static MTOINLINE uint8 *tgaGetPalette(const struct TGA *pTga)
{
	return pTga->pPalette;
}

static MTOINLINE uint32 tgaGetPaletteSize(const struct TGA *pTga)
{
	return pTga->paletteSize;
}

static MTOINLINE uint16 tgaGetPaletteColor(const struct TGA *pTga)
{
	return pTga->header.paletteColor;
}

static MTOINLINE uint8 rgaGetPaletteBit(const struct TGA *pTga)
{
	return pTga->header.paletteBit;
}

static MTOINLINE uint16 tgaGetWidth(const struct TGA *pTga)
{
	return pTga->header.imageW;
}

static MTOINLINE uint16 tgaGetHeight(const struct TGA *pTga)
{
	return pTga->header.imageH;
}

static MTOINLINE struct TGAHeader tgaGetHeader(const struct TGA *pTga)
{
	return pTga->header;
}

static MTOINLINE struct TGAFooter tgaGetFooter(const struct TGA *pTga)
{
	return pTga->footer;
}

static MTOINLINE void tgaSetFilePos(struct TGA *pTga, const uint32 filePos)
{
	pTga->footer.filePos = filePos;
}

static MTOINLINE void tgaSetFileDev(struct TGA *pTga, const uint32 fileDev)
{
	pTga->footer.fileDev = fileDev;
}

int tgaCreateFile(struct TGA *pTga, const char *pFileName);
int tgaCreateMemory(struct TGA *pTga, const void *pSrc, const uint32 size);
int tgaCreateHeader(struct TGA *pTga, const struct TGAHeader *pHeader, uint8 *pImage, const uint32 imageSize, uint8 *pPalette, const uint32 paletteSize);
int tgaOutput(struct TGA *pTga, const char *pFileName);
int tgaOutputBMP(struct TGA *pTga, const char *pFileName);
bool tgaConvertRGBA(struct TGA *pTga);
bool tgaConvertType(struct TGA *pTga, const sint32 type);

void tgaRelease(struct TGA *pTga);

bool tgaWriteHeader(FILE *fp, struct TGAHeader *pHeader);
bool tgaWriteFooter(FILE *fp, struct TGAFooter *pFooter);

#endif
