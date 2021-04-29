#include "mto_common.h"
#include "tga.h"

/*=======================================================================
【機能】RLE圧縮解凍
【引数】pTga：TGA構造体のアドレス
        pDst：展開先
        pSrc：画像データアドレス
        size：画像データサイズ
【備考】非公開
 =======================================================================*/
uint32 _tgaUnpackRLE(struct TGA *pTga, uint8 *pDst, const uint8 *pSrc, const uint32 size)
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
	uint32 count   = 0;

	uint8 byte = pTga->header.imageBit >> 3; // バイトサイズ

	while (count < pTga->imageSize) {
		bFlg = (pSrc[offset] & 0x80) ? false : true; // 上位ビットが0ならリテラルグループ
		loop = (pSrc[offset] & 0x7f) + 1;
		offset++;

		if (bFlg) {
			// リテラルグループ
			// 制御バイトの後ろ（pSrc[offset] & 0x7f)+1個のデータ（ピクセルバイト単位）をコピーする
			for (uint16 i = 0; i < loop; i++) {
				for (uint32 j = 0; j < byte; j++) {
					pDst[count++] = pSrc[offset++];
				}
			}
		} else {
			// 反復
			// 次に続くデータバイト（ピクセルバイト単位）を（pSrc[offset] & 0x7f)+1回繰り返す
			for (uint16 i = 0; i < loop; i++) {
				for (uint32 j = 0; j < byte; j++) {
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

	_ASSERT(count == pTga->imageSize);

	return offset;
}

/*=======================================================================
【機能】対応チェック
【引数】pHeader：TGAヘッダーアドレス
【備考】非公開
 =======================================================================*/
bool _tgaCheckSupport(const struct TGAHeader *pHeader)
{
#ifndef NDEBUG
	_ASSERT(pHeader != NULL);
#else
	if (pHeader == NULL) return false;
#endif

	// 原点が0ではない？
	if (pHeader->imageX != 0 && pHeader->imageY != 0) return false;
	
	// 対応していないイメージタイプ？
	if (!((TGA_IMAGE_TYPE_NONE < pHeader->imageType && pHeader->imageType < TGA_IMAGE_TYPE_MAX) ||
		  (TGA_IMAGE_TYPE_INDEX_RLE <= pHeader->imageType && pHeader->imageType < TGA_IMAGE_TYPE_RLE_MAX))) {
		return false;
	}

	// 対応ビット？
	if (pHeader->imageBit !=  8 && pHeader->imageBit != 16 &&
		pHeader->imageBit != 24 && pHeader->imageBit != 32) {
		return false;
	}

	// 対応パレット？
	if (pHeader->usePalette) {
		if (pHeader->paletteIndex != 0) return false;
		if (pHeader->paletteBit != 24 && pHeader->paletteBit != 32) return false;
	}

	return true;
}

/*=======================================================================
【機能】TGAヘッダー読み込み
【引数】pSrc   ：画像データアドレス
        pHeader：TGAヘッダーアドレス
【備考】非公開
 =======================================================================*/
bool _tgaReadHeader(const uint8 *pSrc, struct TGAHeader *pHeader)
{
#ifndef NDEBUG
	_ASSERT(pSrc != NULL);
	_ASSERT(pHeader != NULL);
#else
	if (pSrc == NULL || pHeader == NULL) return false;
#endif

	uint32 offset = 0;

	pHeader->IDField      = pSrc[offset++];
	pHeader->usePalette   = pSrc[offset++];
	pHeader->imageType    = pSrc[offset++];
	pHeader->paletteIndex = (pSrc[offset] | (pSrc[offset + 1] << 8)); offset += 2;
	pHeader->paletteColor = (pSrc[offset] | (pSrc[offset + 1] << 8)); offset += 2;
	pHeader->paletteBit   = pSrc[offset++];
	pHeader->imageX       = (pSrc[offset] | (pSrc[offset + 1] << 8)); offset += 2;
	pHeader->imageY       = (pSrc[offset] | (pSrc[offset + 1] << 8)); offset += 2;
	pHeader->imageW       = (pSrc[offset] | (pSrc[offset + 1] << 8)); offset += 2;
	pHeader->imageH       = (pSrc[offset] | (pSrc[offset + 1] << 8)); offset += 2;
	pHeader->imageBit     = pSrc[offset++];
	pHeader->discripter   = pSrc[offset++];

	_ASSERT(offset == TGA_HEADER_SIZE);

	return _tgaCheckSupport(pHeader);
}

/*=======================================================================
【機能】TGAフッター読み込み
【引数】pSrc　 ：画像データアドレス
        offset ：フッターまでのオフセット
		pFooter：TGAフッターアドレス
【備考】非公開
 =======================================================================*/
void _tgaReadFooter(const uint8 *pSrc, const uint32 offset, struct TGAFooter *pFooter)
{
#ifndef NDEBUG
	_ASSERT(pSrc != NULL);
	_ASSERT(pFooter != NULL);
#else
	if (pSrc == NULL || pFooter == NULL) return;
#endif

	uint32 offs = offset;

	memcpy(&pFooter->filePos, &pSrc[offs], sizeof(pFooter->filePos)); offs += sizeof(pFooter->filePos);
	memcpy(&pFooter->fileDev, &pSrc[offs], sizeof(pFooter->fileDev)); offs += sizeof(pFooter->fileDev);
	memcpy(pFooter->version,  &pSrc[offs], sizeof(pFooter->version)); offs += sizeof(pFooter->version);

	_ASSERT((offs - offset) == TGA_FOOTER_SIZE);
}

/*=======================================================================
【機能】Image/Paletteサイズを求める
【引数】pTga：TGA構造体アドレス
        bFlg：メモリ確保を行う？
【備考】非公開
 =======================================================================*/
bool _tgaCalcSize(struct TGA *pTga, const bool bFlg)
{
	pTga->imageSize   = pTga->header.imageW * pTga->header.imageH * (pTga->header.imageBit >> 3);
	pTga->paletteSize = pTga->header.usePalette * pTga->header.paletteColor * (pTga->header.paletteBit >> 3);

	if (bFlg) {
		if ((pTga->pImage = (uint8*)malloc(pTga->imageSize)) == NULL) return false;

		// パレットありならメモリ確保
		if (pTga->header.usePalette) {
			if ((pTga->pPalette = (uint8*)malloc(pTga->paletteSize)) == NULL) return false;
		}
	}

	return true;
}

/*=======================================================================
【機能】Image読み込み
【引数】pTga  ：TGA構造体のアドレス
        pSrc  ：画像データアドレス
        size  ：画像データサイズ
        offset：保存先元イメージサイズ（RLEの場合は圧縮時のサイズ）
【備考】非公開
 =======================================================================*/
bool _tgaReadImage(struct TGA *pTga, const uint8 *pSrc, const uint32 size, uint32 *pOffset)
{
#ifndef NDEBUG
	_ASSERT(pSrc != NULL);
	_ASSERT(pTga != NULL);
	_ASSERT(pTga->pImage != NULL);
#else
	if (pSrc == NULL || pTga == NULL || pTga->pImage == NULL) return false;
#endif

	uint8 *pWork  = (uint8*)pSrc + TGA_HEADER_SIZE + pTga->header.IDField + pTga->paletteSize;
	uint8 *pImage = pTga->pImage;
	uint32 offset = 0;

	if (TGA_IMAGE_TYPE_INDEX_RLE <= pTga->header.imageType && pTga->header.imageType < TGA_IMAGE_TYPE_RLE_MAX) {
		// RLE圧縮
		offset = _tgaUnpackRLE(pTga, pImage, pWork, size);
		if (offset == (uint32)(-1)) return false;
	} else {
		// 非圧縮
		memcpy(pImage, pWork, pTga->imageSize);
		offset = pTga->imageSize;
	}

	if (pOffset != NULL) *pOffset = offset;

	return true;
}

/*=======================================================================
【機能】Palette読み込み
【引数】pTga：TGA構造体のアドレス
        pSrc：画像データアドレス
【備考】非公開
 =======================================================================*/
bool _tgaReadPalette(struct TGA *pTga, const uint8 *pSrc)
{
#ifndef NDEBUG
	_ASSERT(pSrc != NULL);
	_ASSERT(pTga != NULL);
#else
	if (pSrc == NULL || pTga == NULL) return false;
#endif

	// IndexColor?
	if (pTga->header.imageType != TGA_IMAGE_TYPE_INDEX && pTga->header.imageType != TGA_IMAGE_TYPE_INDEX_RLE) {
		return true;
	}

	// Palette OK?
	if (pTga->pPalette == NULL) return false;

	uint8 *pWork = (uint8*)pSrc + TGA_HEADER_SIZE + pTga->header.IDField;
	uint8 *pPalette = pTga->pPalette;
	uint16 i; // VC6.0ではコンパイルが通らないので外に出す

	switch (pTga->header.paletteBit) {
	case 24:
		for (i = 0; i < pTga->header.paletteColor; i++) {
			*pPalette++ = *pWork++;
			*pPalette++ = *pWork++;
			*pPalette++ = *pWork++;
		}
		break;
	case 32:
		for (i = 0; i < pTga->header.paletteColor; i++) {
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
【機能】ファイル読み込み
【引数】pTga     ： TGA構造体のアドレス
        pFileName：ファイル名
 =======================================================================*/
int tgaCreateFile(struct TGA *pTga, const char *pFileName)
{
	FILE *fp;
	uint8 *mem;
	uint32 size;

#ifndef NDEBUG
	_ASSERT(pTga != NULL);
	_ASSERT(pFileName != NULL);
#else
	if (pTga == NULL || pFileName == NULL) return TGA_ERROR_OPEN;
#endif

	if ((fp = fopen(pFileName, "rb")) == NULL) {
		DBG_PRINT("file not found!\n");
		return TGA_ERROR_OPEN;
	}

	// get file size
	fseek(fp, 0, SEEK_END);
	size = ftell(fp);
	fseek(fp, 0, SEEK_SET);

	// read file to memory
	if ((mem = (uint8*)malloc(size)) == NULL) {
		fclose(fp);
		return TGA_ERROR_MEMORY;
	}
	fread(mem, size, 1, fp);
	fclose(fp);

	// 読み込み
	int ret = tgaCreateMemory(pTga, mem, size);
	SAFE_FREE(mem);

	return ret;
}

/*=======================================================================
【機能】メモリから作成
【引数】pTga：TGA構造体のアドレス
        pSrc：画像データアドレス
        size：画像データサイズ
 =======================================================================*/
int tgaCreateMemory(struct TGA *pTga, const void *pSrc, const uint32 size)
{
	uint32 offset;

#ifndef NDEBUG
	_ASSERT(pTga != NULL);
	_ASSERT(pSrc != NULL);
	_ASSERT(size);
#else
	if (pTga == NULL || pSrc == NULL || size == 0) return TGA_ERROR_HEADER;
#endif

	// 既に作成しているなら削除
	if (pTga->pImage != NULL) {
		tgaRelease(pTga);
	}

	// ヘッダー読み込み
	if (!_tgaReadHeader((const uint8*)pSrc, &pTga->header)) {
		return TGA_ERROR_HEADER;
	}

	// ImageとPaletteのサイズを求める
	if (!_tgaCalcSize(pTga, true)) {
		tgaRelease(pTga);
		return TGA_ERROR_MEMORY;
	}

	// パレット読み込み
	if (!_tgaReadPalette(pTga, (const uint8*)pSrc)) {
		tgaRelease(pTga);
		return TGA_ERROR_PALETTE;
	}

	// イメージ読み込み
	if (!_tgaReadImage(pTga, (const uint8*)pSrc, size, &offset)) {
		tgaRelease(pTga);
		return TGA_ERROR_IMAGE;
	}

	// フッター読み込み
	offset += TGA_HEADER_SIZE + pTga->header.IDField + pTga->paletteSize;
	if ((size - offset) >= TGA_FOOTER_SIZE) {
		_tgaReadFooter((const uint8*)pSrc, offset, &pTga->footer);
	}

	return TGA_ERROR_NONE;
}

/*=======================================================================
【機能】指定データから作成
【引数】pTga       ：TGA構造体のアドレス
        pHeader    ：TGAヘッダーのアドレス
        pImage     ：イメージデータアドレス
        imageSize  ：イメージデータサイズ
        pPalette   ：パレットデータアドレス
        paletteSize：パレットデータサイズ
 =======================================================================*/
int tgaCreateHeader(struct TGA *pTga, const struct TGAHeader *pHeader, uint8 *pImage, const uint32 imageSize, uint8 *pPalette, const uint32 paletteSize)
{
	// 引数チェック
	if (pTga == NULL) return TGA_ERROR_IMAGE;
	if (pImage == NULL || imageSize == 0) return TGA_ERROR_IMAGE;
	if (pPalette != NULL && paletteSize == 0) return TGA_ERROR_PALETTE;
	if (pPalette == NULL && paletteSize != 0) return TGA_ERROR_PALETTE;

	// ヘッダーチェック
	if (!_tgaCheckSupport(pHeader)) return TGA_ERROR_HEADER;

	// 既に作成しているなら削除
	if (pTga->pImage != NULL) {
		tgaRelease(pTga);
	}

	memcpy(&pTga->header, pHeader, sizeof(pTga->header));
	pTga->pImage      = pImage;
	pTga->imageSize   = imageSize;
	pTga->pPalette    = pPalette;
	pTga->paletteSize = paletteSize;

	return TGA_ERROR_NONE;
}

/*=======================================================================
【機能】ファイル出力
【引数】pTga     ：TGA構造体のアドレス
        pFileName：出力ファイル名
 =======================================================================*/
int tgaOutput(struct TGA *pTga, const char *pFileName)
{
#ifndef NDEBUG
	_ASSERT(pTga != NULL);
	_ASSERT(pFileName != NULL);
#else
	if (pTga == NULL || pFileName == NULL) return TGA_ERROR_OUTPUT;
#endif

	// 読み込まれていない？
	if (pTga->pImage == NULL) return TGA_ERROR_NONE;

	// 出力ファイルオープン
	FILE *fp;
	if ((fp = fopen(pFileName, "wb")) == NULL) {
		DBG_PRINT("file can't open!\n");
		return TGA_ERROR_OPEN;
	}

	// ヘッダー出力
	tgaWriteHeader(fp, &pTga->header);

	// パレット出力
	if (pTga->pPalette != NULL) {
		fwrite(pTga->pPalette, pTga->paletteSize, 1, fp);
	}

	// イメージ出力
	fwrite(pTga->pImage, pTga->imageSize, 1, fp);

	// フッター出力
	tgaWriteFooter(fp, &pTga->footer);

	fclose(fp);

	return TGA_ERROR_NONE;
}

/*=======================================================================
【機能】BMP出力
【引数】pTga     ：TGA構造体のアドレス
        pFileName：出力ファイル名
 =======================================================================*/
int tgaOutputBMP(struct TGA *pTga, const char *pFileName)
{
#ifndef NDEBUG
	_ASSERT(pTga != NULL);
	_ASSERT(pFileName != NULL);
#else
	if (pTga == NULL) return TGA_ERROR_IMAGE;
	if (pFileName == NULL) return TGA_ERROR_OUTPUT;
#endif

	// 読み込まれていない？
	if (pTga->pImage == NULL) return TGA_ERROR_NONE;

	// 出力ファイルオープン
	FILE *fp;
	if ((fp = fopen(pFileName, "wb")) == NULL) {
		DBG_PRINT("file can't open!\n");
		return TGA_ERROR_OPEN;
	}

	// BMPヘッダー作成
	uint32 hsize, isize, psize;
	BITMAPFILEHEADER bmHead;
	BITMAPINFOHEADER bmInfo;

	memset(&bmHead, 0, sizeof(bmHead));
	memset(&bmInfo, 0, sizeof(bmInfo));

	hsize = sizeof(BITMAPFILEHEADER);
	isize = sizeof(BITMAPINFOHEADER);
	psize = sizeof(RGBQUAD) * pTga->header.paletteColor;

	bmHead.bfType = 0x4D42; //BM
	bmHead.bfSize = pTga->imageSize + hsize + isize + psize;
	bmHead.bfReserved1 = 0;
	bmHead.bfReserved2 = 0;
	bmHead.bfOffBits   = hsize + isize + psize;

	bmInfo.biSize   = isize;
	bmInfo.biWidth  = pTga->header.imageW;
	bmInfo.biHeight = pTga->header.imageH;
	bmInfo.biPlanes = 1;
	bmInfo.biBitCount = pTga->header.imageBit;


	// ヘッダー出力
	fwrite(&bmHead, hsize, 1, fp);
	fwrite(&bmInfo, isize, 1, fp);

	// パレット出力
	if (pTga->pPalette != NULL) {
		if (pTga->header.paletteBit == 24) {
			uint8 *pPalette = pTga->pPalette;
			uint8 alpha = 0x00;

			for (int i = 0; i < pTga->header.paletteColor; i++) {
				fwrite(pPalette++, sizeof(uint8), 1, fp);
				fwrite(pPalette++, sizeof(uint8), 1, fp);
				fwrite(pPalette++, sizeof(uint8), 1, fp);
				fwrite(&alpha    , sizeof(uint8), 1, fp);
			}
		} else {
			fwrite(pTga->pPalette, pTga->paletteSize, 1, fp);
		}
	}

	// イメージ出力（幅が4で割れない画像への対応はしていません）
	tgaConvertType(pTga, TGA_IMAGE_LINE_LRDU); // 左→右、下→上配列に変換
	fwrite(pTga->pImage, pTga->imageSize, 1, fp);

	fclose(fp);

	return TGA_ERROR_NONE;
}

/*=======================================================================
【機能】BGRA配列をRGBA配列に変更
【引数】pTga：TGA構造体のアドレス
【備考】RGBAに変更したあと再度呼ぶとBGRA配列になります。
 =======================================================================*/
bool tgaConvertRGBA(struct TGA *pTga)
{
	if (pTga == NULL || pTga->pImage == NULL) return false;

	uint8 r, b;
	uint8 byte;
	uint32 offset;

	// パレット
	if (pTga->pPalette) {
		byte = pTga->header.paletteBit >> 3;

		for (offset = 0; offset < pTga->paletteSize; offset += byte) {
			r = pTga->pPalette[offset + 2];
			b = pTga->pPalette[offset + 0];
			pTga->pPalette[offset + 2] = b;
			pTga->pPalette[offset + 0] = r;
		}
	}

	// イメージ
	byte = pTga->header.imageBit >> 3;

	if (pTga->header.imageBit <= 8) {
		// IndexColorなら処理しない
		return true;
	} else if (pTga->header.imageBit == 16) {
		uint16 r, g, b, a;
		uint16 *pImage = (uint16*)pTga->pImage;

		// RGBA:5551
		for (offset = 0; offset < pTga->imageSize; offset += byte) {
			a = (*pImage & 0x8000);
			r = (*pImage & 0x7c00) >> 10;
			g = (*pImage & 0x03e0);
			b = (*pImage & 0x001f) << 10;
			*pImage++ = (a | r | g | b);
		}
	} else {
		for (offset = 0; offset < pTga->imageSize; offset += byte) {
			r = pTga->pImage[offset + 2];
			b = pTga->pImage[offset + 0];
			pTga->pImage[offset + 2] = b;
			pTga->pImage[offset + 0] = r;
		}
	}

	return true;
}

/*=======================================================================
【機能】指定のビット配列に変換
【引数】pTga：TGA構造体のアドレス
        type：ラインタイプ
 =======================================================================*/
bool tgaConvertType(struct TGA *pTga, const sint32 type)
{
	if (type >= TGA_IMAGE_LINE_MAX) return false;
	if (pTga == NULL || pTga->pImage == NULL) return false;

	// 一緒なら処理なし
	if ((pTga->header.discripter & 0xf0) == type) return true;

	uint8 *pImage;

	// 変換用のメモリ確保
	if ((pImage = (uint8*)malloc(pTga->imageSize)) == NULL) {
		return false;
	}

	// 配列変換
	uint16 tx, ty;
	uint32 ofsSrc, ofsDst;

	ofsDst = 0;
	for (int y = 0; y < pTga->header.imageH; y++) {
		ty = y;
		if ((pTga->header.discripter & 0x20) != (type & 0x20)) {
			// お互いのY方向が一致しないなら反転
			ty = pTga->header.imageH - y - 1;
		}

		for (int x = 0; x < pTga->header.imageW; x++) {
			tx = x;
			if ((pTga->header.discripter & 0x10) != (type & 0x10)) {
				// お互いのX方向が一致しないなら反転
				tx = pTga->header.imageW - x - 1;
			}
			ofsSrc = (ty * pTga->header.imageW) + tx;

			if (pTga->header.imageBit == 8) {
				pImage[ofsDst++] = pTga->pImage[ofsSrc];

			} else if (pTga->header.imageBit == 16) {
				uint16 *pSrc = (uint16*)pTga->pImage;
				uint16 *pDst = (uint16*)pImage;
				pDst[ofsDst++] = pSrc[ofsSrc];

			} else if (pTga->header.imageBit == 24) {
				ofsSrc *= 3;
				pImage[ofsDst++] = pTga->pImage[ofsSrc++];
				pImage[ofsDst++] = pTga->pImage[ofsSrc++];
				pImage[ofsDst++] = pTga->pImage[ofsSrc++];

			} else if (pTga->header.imageBit == 32) {
				ofsSrc *= 4;
				pImage[ofsDst++] = pTga->pImage[ofsSrc++];
				pImage[ofsDst++] = pTga->pImage[ofsSrc++];
				pImage[ofsDst++] = pTga->pImage[ofsSrc++];
				pImage[ofsDst++] = pTga->pImage[ofsSrc++];
			}
		}
	}

	// 元を破棄して変換後のデータを保持
	SAFE_FREE(pTga->pImage);
	pTga->pImage = pImage;

	// イメージ記述子を変更
	pTga->header.discripter = type;

	return true;
}

/*=======================================================================
【機能】構造体情報の解放
【引数】pTga：TGA構造体のアドレス
 =======================================================================*/
void tgaRelease(struct TGA *pTga)
{
	SAFE_FREE(pTga->pImage);
	SAFE_FREE(pTga->pPalette);

	memset(&pTga->header, 0, sizeof(pTga->header));
	memset(&pTga->footer, 0, sizeof(pTga->footer));

	pTga->imageSize   = 0;
	pTga->paletteSize = 0;
}

/*=======================================================================
【機能】TGAヘッダー出力
【引数】fp     ：FILEポインタ
        pHeader：出力するTGAヘッダー
【備考】アライメントに沿っていないので個別出力。
 =======================================================================*/
bool tgaWriteHeader(FILE *fp, struct TGAHeader *pHeader)
{
#ifndef NDEBUG
	_ASSERT(fp != NULL);
	_ASSERT(pHeader != NULL);
#else
	if (fp == NULL || pHeader == NULL) return false;
#endif

	// TODO:RLE圧縮出力対応
	//      2008/04/01時点では非対応
	if (TGA_IMAGE_TYPE_INDEX_RLE <= pHeader->imageType && pHeader->imageType < TGA_IMAGE_TYPE_RLE_MAX) {
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
bool tgaWriteFooter(FILE *fp, struct TGAFooter *pFooter)
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
		strcpy((char*)pFooter->version, "TRUEVISION-TARGA");
	}
	fwrite(pFooter->version, sizeof(pFooter->version), 1, fp);

	return true;
}
