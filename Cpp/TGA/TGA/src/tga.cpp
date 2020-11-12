#include "mto_common.h"
#include "tga.h"


/*=======================================================================
【機能】
 =======================================================================*/
CTga::CTga(void)
{
	memset(&m_Header, 0, sizeof(m_Header));
	memset(&m_Footer, 0, sizeof(m_Footer));
	m_pImage   = NULL;
	m_pPalette = NULL;

	m_ImageSize   = 0;
	m_PaletteSize = 0;
}

/*=======================================================================
【機能】
 =======================================================================*/
CTga::~CTga(void)
{
	SAFE_DELETES(m_pImage);
	SAFE_DELETES(m_pPalette);
}


/*=======================================================================
【機能】ファイル読み込み
【引数】pFileName：ファイル名
 =======================================================================*/
int CTga::Create(const char *pFileName)
{
	FILE *fp;
	uint8 *mem;
	uint32 size;

#ifndef NDEBUG
	_ASSERT(pFileName != NULL);
#else
	if (pFileName == NULL) return ERROR_OPEN;
#endif

	if ((fp = fopen(pFileName, "rb")) == NULL) {
		DBG_PRINT("file not found!\n");
		return ERROR_OPEN;
	}

	// get file size
	fseek(fp, 0, SEEK_END);
	size = ftell(fp);
	fseek(fp, 0, SEEK_SET);

	// read file to memory
	if ((mem = new uint8[size]) == NULL) {
		fclose(fp);
		return ERROR_MEMORY;
	}
	fread(mem, size, 1, fp);
	fclose(fp);

	// 読み込み
	int ret = this->Create(mem, size);
	SAFE_DELETES(mem);

	return ret;
}

/*=======================================================================
【機能】メモリから作成
【引数】pSrc：画像データアドレス
        size：画像データサイズ
 =======================================================================*/
int CTga::Create(const void *pSrc, const uint32 size)
{
	uint32 offset;

#ifndef NDEBUG
	_ASSERT(pSrc != NULL);
	_ASSERT(size);
#else
	if (pSrc == NULL || size == 0) return ERROR_HEADER;
#endif

	// 既に作成しているなら削除
	if (m_pImage != NULL) {
		this->Clear();
	}

	// ヘッダー読み込み
	if (!this->ReadHeader(static_cast<const uint8*>(pSrc))) {
		return ERROR_HEADER;
	}

	// ImageとPaletteのサイズを求める
	if (!this->CalcSize(true)) {
		this->Clear();
		return ERROR_MEMORY;
	}

	// パレット読み込み
	if (!this->ReadPalette(static_cast<const uint8*>(pSrc))) {
		this->Clear();
		return ERROR_PALETTE;
	}

	// イメージ読み込み
	if (!this->ReadImage(static_cast<const uint8*>(pSrc), size, &offset)) {
		this->Clear();
		return ERROR_IMAGE;
	}

	// フッター読み込み
	offset += HEADER_SIZE + m_Header.IDField + m_PaletteSize;
	if ((size - offset) >= FOOTER_SIZE) {
		this->ReadFooter(static_cast<const uint8*>(pSrc), offset);
	}

	return ERROR_NONE;
}

/*=======================================================================
【機能】指定データから作成
【引数】header     ：TGAヘッダー
        pImage     ：イメージデータアドレス
        imageSize  ：イメージデータサイズ
        pPalette   ：パレットデータアドレス
        paletteSize：パレットデータサイズ
 =======================================================================*/
int CTga::Create(const TGAHeader &header, uint8 *pImage, const uint32 imageSize, uint8 *pPalette, const uint32 paletteSize)
{
	// 引数チェック
	if (pImage == NULL || imageSize == 0) return ERROR_IMAGE;
	if (pPalette != NULL && paletteSize == 0) return ERROR_PALETTE;
	if (pPalette == NULL && paletteSize != 0) return ERROR_PALETTE;

	// ヘッダーチェック
	if (!this->CheckSupport(header)) return ERROR_HEADER;

	// 既に作成しているなら削除
	if (m_pImage != NULL) {
		this->Clear();
	}

	m_Header      = header;
	m_pImage      = pImage;
	m_ImageSize   = imageSize;
	m_pPalette    = pPalette;
	m_PaletteSize = paletteSize;

	return ERROR_NONE;
}

/*=======================================================================
【機能】ファイル出力
【引数】pFileName：出力ファイル名
 =======================================================================*/
int CTga::Output(const char *pFileName)
{
#ifndef NDEBUG
	_ASSERT(pFileName != NULL);
#else
	if (pFileName == NULL) return ERROR_OUTPUT;
#endif

	// 読み込まれていない？
	if (m_pImage == NULL) return ERROR_NONE;

	// 出力ファイルオープン
	FILE *fp;
	if ((fp = fopen(pFileName, "wb")) == NULL) {
		DBG_PRINT("file can't open!\n");
		return ERROR_OPEN;
	}

	// ヘッダー出力
	this->WriteHeader(fp);

	// パレット出力
	if (m_pPalette != NULL) {
		fwrite(m_pPalette, m_PaletteSize, 1, fp);
	}

	// イメージ出力
	fwrite(m_pImage, m_ImageSize, 1, fp);

	// フッター出力
	this->WriteFooter(fp);

	fclose(fp);

	return ERROR_NONE;
}

/*=======================================================================
【機能】BMP出力
【引数】pFileName：出力ファイル名
 =======================================================================*/
int CTga::OutputBMP(const char *pFileName)
{
#ifndef NDEBUG
	_ASSERT(pFileName != NULL);
#else
	if (pFileName == NULL) return ERROR_OUTPUT;
#endif

	// 読み込まれていない？
	if (m_pImage == NULL) return ERROR_NONE;

	// 出力ファイルオープン
	FILE *fp;
	if ((fp = fopen(pFileName, "wb")) == NULL) {
		DBG_PRINT("file can't open!\n");
		return ERROR_OPEN;
	}

	// BMPヘッダー作成
	uint32 hsize, isize, psize;
	BITMAPFILEHEADER bmHead;
	BITMAPINFOHEADER bmInfo;

	memset(&bmHead, 0, sizeof(bmHead));
	memset(&bmInfo, 0, sizeof(bmInfo));

	hsize = sizeof(BITMAPFILEHEADER);
	isize = sizeof(BITMAPINFOHEADER);
	psize = sizeof(RGBQUAD) * m_Header.paletteColor;

	bmHead.bfType = 0x4D42; //BM
	bmHead.bfSize = m_ImageSize + hsize + isize + psize;
	bmHead.bfReserved1 = 0;
	bmHead.bfReserved2 = 0;
	bmHead.bfOffBits   = hsize + isize + psize;

	bmInfo.biSize   = isize;
	bmInfo.biWidth  = m_Header.imageW;
	bmInfo.biHeight = m_Header.imageH;
	bmInfo.biPlanes = 1;
	bmInfo.biBitCount = m_Header.imageBit;


	// ヘッダー出力
	fwrite(&bmHead, hsize, 1, fp);
	fwrite(&bmInfo, isize, 1, fp);

	// パレット出力
	if (m_pPalette != NULL) {
		if (m_Header.paletteBit == 24) {
			uint8 *pPalette = m_pPalette;
			uint8 alpha = 0x00;

			for (int i = 0; i < m_Header.paletteColor; i++) {
				fwrite(pPalette++, sizeof(uint8), 1, fp);
				fwrite(pPalette++, sizeof(uint8), 1, fp);
				fwrite(pPalette++, sizeof(uint8), 1, fp);
				fwrite(&alpha    , sizeof(uint8), 1, fp);
			}
		} else {
			fwrite(m_pPalette, m_PaletteSize, 1, fp);
		}
	}

	// イメージ出力（幅が4で割れない画像への対応はしていません）
	this->ConvertType(IMAGE_LINE_LRDU); // 左→右、下→上配列に変換
	fwrite(m_pImage, m_ImageSize, 1, fp);

	fclose(fp);

	return ERROR_NONE;
}

/*=======================================================================
【機能】BGRA配列をRGBA配列に変更
【備考】RGBAに変更したあと再度呼ぶとBGRA配列になります。
 =======================================================================*/
bool CTga::ConvertRGBA(void)
{
	if (m_pImage == NULL) return false;

	uint8 r, b;
	uint8 byte;
	uint32 offset;

	// パレット
	if (m_pPalette) {
		byte = m_Header.paletteBit >> 3;

		for (offset = 0; offset < m_PaletteSize; offset += byte) {
			r = m_pPalette[offset + 2];
			b = m_pPalette[offset + 0];
			m_pPalette[offset + 2] = b;
			m_pPalette[offset + 0] = r;
		}
	}

	// イメージ
	byte = m_Header.imageBit >> 3;

	if (m_Header.imageBit <= 8) {
		// IndexColorなら処理しない
		return true;
	} else if (m_Header.imageBit == 16) {
		uint16 r, g, b, a;
		uint16 *pImage = reinterpret_cast<uint16*>(m_pImage);

		// RGBA:5551
		for (offset = 0; offset < m_ImageSize; offset += byte) {
			a = (*pImage & 0x8000);
			r = (*pImage & 0x7c00) >> 10;
			g = (*pImage & 0x03e0);
			b = (*pImage & 0x001f) << 10;
			*pImage++ = (a | r | g | b);
		}
	} else {
		for (offset = 0; offset < m_ImageSize; offset += byte) {
			r = m_pImage[offset + 2];
			b = m_pImage[offset + 0];
			m_pImage[offset + 2] = b;
			m_pImage[offset + 0] = r;
		}
	}

	return true;
}

/*=======================================================================
【機能】指定のビット配列に変換
【引数】type：ラインタイプ
 =======================================================================*/
bool CTga::ConvertType(const sint32 type)
{
	if (type >= IMAGE_LINE_MAX) return false;
	if (m_pImage == NULL) return false;

	// 一緒なら処理なし
	if ((m_Header.discripter & 0xf0) == type) return true;

	uint8 *pImage;

	// 変換用のメモリ確保
	if ((pImage = new uint8[m_ImageSize]) == NULL) {
		return false;
	}

	// 配列変換
	uint16 tx, ty;
	uint32 ofsSrc, ofsDst;

	ofsDst = 0;
	for (int y = 0; y < m_Header.imageH; y++) {
		ty = y;
		if ((m_Header.discripter & 0x20) != (type & 0x20)) {
			// お互いのY方向が一致しないなら反転
			ty = m_Header.imageH - y - 1;
		}

		for (int x = 0; x < m_Header.imageW; x++) {
			tx = x;
			if ((m_Header.discripter & 0x10) != (type & 0x10)) {
				// お互いのX方向が一致しないなら反転
				tx = m_Header.imageW - x - 1;
			}
			ofsSrc = (ty * m_Header.imageW) + tx;

			if (m_Header.imageBit == 8) {
				pImage[ofsDst++] = m_pImage[ofsSrc];

			} else if (m_Header.imageBit == 16) {
				uint16 *pSrc = reinterpret_cast<uint16*>(m_pImage);
				uint16 *pDst = reinterpret_cast<uint16*>(pImage);
				pDst[ofsDst++] = pSrc[ofsSrc];

			} else if (m_Header.imageBit == 24) {
				ofsSrc *= 3;
				pImage[ofsDst++] = m_pImage[ofsSrc++];
				pImage[ofsDst++] = m_pImage[ofsSrc++];
				pImage[ofsDst++] = m_pImage[ofsSrc++];

			} else if (m_Header.imageBit == 32) {
				ofsSrc *= 4;
				pImage[ofsDst++] = m_pImage[ofsSrc++];
				pImage[ofsDst++] = m_pImage[ofsSrc++];
				pImage[ofsDst++] = m_pImage[ofsSrc++];
				pImage[ofsDst++] = m_pImage[ofsSrc++];
			}
		}
	}

	// 元を破棄して変換後のデータを保持
	SAFE_DELETES(m_pImage);
	m_pImage = pImage;

	// イメージ記述子を変更
	m_Header.discripter = type;

	return true;
}

/*=======================================================================
【機能】情報クリア
【備考】非公開
 =======================================================================*/
void CTga::Clear(void)
{
	SAFE_DELETES(m_pImage);
	SAFE_DELETES(m_pPalette);

	memset(&m_Header, 0, sizeof(m_Header));
	memset(&m_Footer, 0, sizeof(m_Footer));

	m_ImageSize   = 0;
	m_PaletteSize = 0;
}

/*=======================================================================
【機能】対応チェック
【引数】header：TGAヘッダー
【備考】非公開
 =======================================================================*/
bool CTga::CheckSupport(const TGAHeader &header)
{
	// 原点が0ではない？
	if (header.imageX != 0 && header.imageY != 0) return false;

	// 対応していないイメージタイプ？
	if (!((IMAGE_TYPE_NONE < header.imageType && header.imageType < IMAGE_TYPE_MAX) ||
		  (IMAGE_TYPE_INDEX_RLE <= header.imageType && header.imageType < IMAGE_TYPE_RLE_MAX))) {
		return false;
	}

	// 対応ビット？
	if (header.imageBit !=  8 && header.imageBit != 16 &&
		header.imageBit != 24 && header.imageBit != 32) {
		return false;
	}

	// 対応パレット？
	if (header.usePalette) {
		if (header.paletteIndex != 0) return false;
		if (header.paletteBit != 24 && header.paletteBit != 32) return false;
	}

	return true;
}

/*=======================================================================
【機能】TGAヘッダー読み込み
【引数】pSrc：画像データアドレス
【備考】非公開
 =======================================================================*/
bool CTga::ReadHeader(const uint8 *pSrc)
{
#ifndef NDEBUG
	_ASSERT(pSrc != NULL);
#else
	if (pSrc == NULL) return false;
#endif

	uint32 offset = 0;

	m_Header.IDField      = pSrc[offset++];
	m_Header.usePalette   = pSrc[offset++];
	m_Header.imageType    = pSrc[offset++];
	m_Header.paletteIndex = (pSrc[offset] | (pSrc[offset + 1] << 8)); offset += 2;
	m_Header.paletteColor = (pSrc[offset] | (pSrc[offset + 1] << 8)); offset += 2;
	m_Header.paletteBit   = pSrc[offset++];
	m_Header.imageX       = (pSrc[offset] | (pSrc[offset + 1] << 8)); offset += 2;
	m_Header.imageY       = (pSrc[offset] | (pSrc[offset + 1] << 8)); offset += 2;
	m_Header.imageW       = (pSrc[offset] | (pSrc[offset + 1] << 8)); offset += 2;
	m_Header.imageH       = (pSrc[offset] | (pSrc[offset + 1] << 8)); offset += 2;
	m_Header.imageBit     = pSrc[offset++];
	m_Header.discripter   = pSrc[offset++];

	_ASSERT(offset == HEADER_SIZE);

	return this->CheckSupport(m_Header);
}

/*=======================================================================
【機能】TGAフッター読み込み
【引数】pSrc　：画像データアドレス
        offset：フッターまでのオフセット
【備考】非公開
 =======================================================================*/
void CTga::ReadFooter(const uint8 *pSrc, const uint32 offset)
{
#ifndef NDEBUG
	_ASSERT(pSrc != NULL);
#else
	if (pSrc == NULL) return;
#endif

	uint32 offs = offset;

	memcpy(&m_Footer.filePos, &pSrc[offs], sizeof(m_Footer.filePos)); offs += sizeof(m_Footer.filePos);
	memcpy(&m_Footer.fileDev, &pSrc[offs], sizeof(m_Footer.fileDev)); offs += sizeof(m_Footer.fileDev);
	memcpy(m_Footer.version,  &pSrc[offs], sizeof(m_Footer.version)); offs += sizeof(m_Footer.version);

	_ASSERT((offs - offset) == FOOTER_SIZE);
}

/*=======================================================================
【機能】Image/Paletteサイズを求める
【引数】bFlg：メモリ確保を行う？
【備考】非公開
 =======================================================================*/
bool CTga::CalcSize(const bool bFlg)
{
	m_ImageSize   = m_Header.imageW * m_Header.imageH * (m_Header.imageBit >> 3);
	m_PaletteSize = m_Header.usePalette * m_Header.paletteColor * (m_Header.paletteBit >> 3);

	if (bFlg) {
		if ((m_pImage = new uint8[m_ImageSize]) == NULL) return false;

		// パレットありならメモリ確保
		if (m_Header.usePalette) {
			if ((m_pPalette = new uint8[m_PaletteSize]) == NULL) return false;
		}
	}

	return true;
}

/*=======================================================================
【機能】Image読み込み
【引数】pSrc  ：画像データアドレス
        size  ：画像データサイズ
        offset：保存先元イメージサイズ（RLEの場合は圧縮時のサイズ）
【備考】非公開
 =======================================================================*/
bool CTga::ReadImage(const uint8 *pSrc, const uint32 size, uint32 *pOffset)
{
#ifndef NDEBUG
	_ASSERT(pSrc != NULL);
	_ASSERT(m_pImage != NULL);
#else
	if (pSrc == NULL || m_pImage == NULL) return false;
#endif

	uint8 *pWork  = const_cast<uint8*>(pSrc) + HEADER_SIZE + m_Header.IDField + m_PaletteSize;
	uint8 *pImage = m_pImage;
	uint32 offset = 0;

	if (IMAGE_TYPE_INDEX_RLE <= m_Header.imageType && m_Header.imageType < IMAGE_TYPE_RLE_MAX) {
		// RLE圧縮
		offset = this->UnpackRLE(m_pImage, pWork, size);
		if (offset == static_cast<uint32>(-1)) return false;
	} else {
		// 非圧縮
		memcpy(pImage, pWork, m_ImageSize);
		offset = m_ImageSize;
	}

	if (pOffset != NULL) *pOffset = offset;

	return true;
}

/*=======================================================================
【機能】Palette読み込み
【引数】pSrc：画像データアドレス
【備考】非公開
 =======================================================================*/
bool CTga::ReadPalette(const uint8 *pSrc)
{
#ifndef NDEBUG
	_ASSERT(pSrc != NULL);
#else
	if (pSrc == NULL) return false;
#endif

	// IndexColor?
	if (m_Header.imageType != IMAGE_TYPE_INDEX && m_Header.imageType != IMAGE_TYPE_INDEX_RLE) {
		return true;
	}

	// Palette OK?
	if (m_pPalette == NULL) return false;

	uint8 *pWork = const_cast<uint8*>(pSrc) + HEADER_SIZE + m_Header.IDField;
	uint8 *pPalette = m_pPalette;
	uint16 i; // VC6.0ではコンパイルが通らないので外に出す

	switch (m_Header.paletteBit) {
	case 24:
		for (i = 0; i < m_Header.paletteColor; i++) {
			*pPalette++ = *pWork++;
			*pPalette++ = *pWork++;
			*pPalette++ = *pWork++;
		}
		break;
	case 32:
		for (i = 0; i < m_Header.paletteColor; i++) {
			*pPalette++ = *pWork++;
			*pPalette++ = *pWork++;
			*pPalette++ = *pWork++;
			*pPalette++ = *pWork++;
		}
		break;
	default:
		DBG_PRINT("not support palette data!!\n");
		return false;
	}

	return true;
}

/*=======================================================================
【機能】RLE圧縮解凍
【引数】pDst：展開先
        pSrc：画像データアドレス
        size：画像データサイズ
【備考】非公開
 =======================================================================*/
uint32 CTga::UnpackRLE(uint8 *pDst, const uint8 *pSrc, const uint32 size)
{
#ifndef NDEBUG
	_ASSERT(pSrc != NULL);
	_ASSERT(pDst != NULL);
#else
	if (pSrc == NULL || pDst == NULL) return false;
#endif

	bool bFlg;
	uint16 loop;
	uint32 offset  = 0;
//	uint32 endAddr = reinterpret_cast<uint32>(pDst) + m_ImageSize;
	uint32 count   = 0;

	uint8 byte = m_Header.imageBit >> 3; // バイトサイズ

//	while (reinterpret_cast<uint32>(pDst) < endAddr) {
	while (count < m_ImageSize) {
		bFlg = (pSrc[offset] & 0x80) ? false : true; // 上位ビットが0ならリテラルグループ
		loop = (pSrc[offset] & 0x7f) + 1;
		offset++;

		if (bFlg) {
			// リテラルグループ
			// 制御バイトの後ろ（pSrc[offset] & 0x7f)+1個のデータ（ピクセルバイト単位）をコピーする
			for (uint16 i = 0; i < loop; i++) {
				for (uint32 j = 0; j < byte; j++) {
//					*pDst++ = pSrc[offset++];
					pDst[count++] = pSrc[offset++];
				}
			}
		} else {
			// 反復
			// 次に続くデータバイト（ピクセルバイト単位）を（pSrc[offset] & 0x7f)+1回繰り返す
			for (uint16 i = 0; i < loop; i++) {
				for (uint32 j = 0; j < byte; j++) {
//					*pDst++ = pSrc[offset + j];
					pDst[count++] = pSrc[offset + j];
				}
			}
			offset += byte;
		}

		// 解凍のしすぎチェック
		if (offset > size) {
			DBG_PRINT("UnpackRLE error!!\n");
			_ASSERT(0);
			return -1;
		}
	}

	_ASSERT(count == m_ImageSize);

	return offset;
}

/*=======================================================================
【機能】TGAヘッダー出力
【引数】fp     ：FILEポインタ
        pHeader：出力するTGAヘッダー
【備考】アライメントに沿っていないので個別出力。
 =======================================================================*/
bool CTga::WriteHeader(FILE *fp)
{
	return this->WriteHeader(fp, &m_Header);
}

bool CTga::WriteHeader(FILE *fp, TGAHeader *pHeader)
{
#ifndef NDEBUG
	_ASSERT(fp != NULL);
	_ASSERT(pHeader != NULL);
#else
	if (fp == NULL || pHeader == NULL) return false;
#endif

	// TODO:RLE圧縮出力対応
	//      2008/04/01時点では非対応
	if (IMAGE_TYPE_INDEX_RLE <= pHeader->imageType && pHeader->imageType < IMAGE_TYPE_RLE_MAX) {
		pHeader->imageType -= 8; // 非圧縮にする
	}

	fwrite(&pHeader->IDField     , sizeof(pHeader->IDField)     , 1, fp);
	fwrite(&pHeader->usePalette  , sizeof(pHeader->usePalette)  , 1, fp);
	fwrite(&pHeader->imageType   , sizeof(pHeader->imageType)   , 1, fp);
	fwrite(&pHeader->paletteIndex, sizeof(pHeader->paletteIndex), 1, fp);
	fwrite(&pHeader->paletteColor, sizeof(pHeader->paletteColor), 1, fp);
	fwrite(&pHeader->paletteBit  , sizeof(pHeader->paletteBit)  , 1, fp);
	fwrite(&pHeader->imageX      , sizeof(pHeader->imageX)      , 1, fp);
	fwrite(&pHeader->imageY      , sizeof(pHeader->imageY)      , 1, fp);
	fwrite(&pHeader->imageW      , sizeof(pHeader->imageW)      , 1, fp);
	fwrite(&pHeader->imageH      , sizeof(pHeader->imageH)      , 1, fp);
	fwrite(&pHeader->imageBit    , sizeof(pHeader->imageBit)    , 1, fp);
	fwrite(&pHeader->discripter  , sizeof(pHeader->discripter)  , 1, fp);

	return true;
}

/*=======================================================================
【機能】TGAフッター出力
【引数】fp     ：FILEポインタ
        pHeader：出力するTGAフッター
【備考】アライメントに沿っていないので個別出力。
 =======================================================================*/
bool CTga::WriteFooter(FILE *fp)
{
	return this->WriteFooter(fp, &m_Footer);
}

bool CTga::WriteFooter(FILE *fp, TGAFooter *pFooter)
{
#ifndef NDEBUG
	_ASSERT(fp != NULL);
	_ASSERT(pFooter != NULL);
#else
	if (fp == NULL || pFooter == NULL) return false;
#endif

	// 元画像にフッターが付いていたかチェック
	int ret = 0;
	for (int i = 0; i < 18; i++) {
		ret += pFooter->version[i];
	}

	fwrite(&pFooter->filePos, sizeof(pFooter->filePos), 1, fp);
	fwrite(&pFooter->fileDev, sizeof(pFooter->fileDev), 1, fp);

	if (ret == 0) {
		strcpy(reinterpret_cast<char*>(pFooter->version), "TRUEVISION-TARGA");
	}
	fwrite(pFooter->version, sizeof(pFooter->version), 1, fp);

	return true;
}
