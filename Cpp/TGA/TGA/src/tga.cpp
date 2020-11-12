#include "mto_common.h"
#include "tga.h"


/*=======================================================================
�y�@�\�z
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
�y�@�\�z
 =======================================================================*/
CTga::~CTga(void)
{
	SAFE_DELETES(m_pImage);
	SAFE_DELETES(m_pPalette);
}


/*=======================================================================
�y�@�\�z�t�@�C���ǂݍ���
�y�����zpFileName�F�t�@�C����
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

	// �ǂݍ���
	int ret = this->Create(mem, size);
	SAFE_DELETES(mem);

	return ret;
}

/*=======================================================================
�y�@�\�z����������쐬
�y�����zpSrc�F�摜�f�[�^�A�h���X
        size�F�摜�f�[�^�T�C�Y
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

	// ���ɍ쐬���Ă���Ȃ�폜
	if (m_pImage != NULL) {
		this->Clear();
	}

	// �w�b�_�[�ǂݍ���
	if (!this->ReadHeader(static_cast<const uint8*>(pSrc))) {
		return ERROR_HEADER;
	}

	// Image��Palette�̃T�C�Y�����߂�
	if (!this->CalcSize(true)) {
		this->Clear();
		return ERROR_MEMORY;
	}

	// �p���b�g�ǂݍ���
	if (!this->ReadPalette(static_cast<const uint8*>(pSrc))) {
		this->Clear();
		return ERROR_PALETTE;
	}

	// �C���[�W�ǂݍ���
	if (!this->ReadImage(static_cast<const uint8*>(pSrc), size, &offset)) {
		this->Clear();
		return ERROR_IMAGE;
	}

	// �t�b�^�[�ǂݍ���
	offset += HEADER_SIZE + m_Header.IDField + m_PaletteSize;
	if ((size - offset) >= FOOTER_SIZE) {
		this->ReadFooter(static_cast<const uint8*>(pSrc), offset);
	}

	return ERROR_NONE;
}

/*=======================================================================
�y�@�\�z�w��f�[�^����쐬
�y�����zheader     �FTGA�w�b�_�[
        pImage     �F�C���[�W�f�[�^�A�h���X
        imageSize  �F�C���[�W�f�[�^�T�C�Y
        pPalette   �F�p���b�g�f�[�^�A�h���X
        paletteSize�F�p���b�g�f�[�^�T�C�Y
 =======================================================================*/
int CTga::Create(const TGAHeader &header, uint8 *pImage, const uint32 imageSize, uint8 *pPalette, const uint32 paletteSize)
{
	// �����`�F�b�N
	if (pImage == NULL || imageSize == 0) return ERROR_IMAGE;
	if (pPalette != NULL && paletteSize == 0) return ERROR_PALETTE;
	if (pPalette == NULL && paletteSize != 0) return ERROR_PALETTE;

	// �w�b�_�[�`�F�b�N
	if (!this->CheckSupport(header)) return ERROR_HEADER;

	// ���ɍ쐬���Ă���Ȃ�폜
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
�y�@�\�z�t�@�C���o��
�y�����zpFileName�F�o�̓t�@�C����
 =======================================================================*/
int CTga::Output(const char *pFileName)
{
#ifndef NDEBUG
	_ASSERT(pFileName != NULL);
#else
	if (pFileName == NULL) return ERROR_OUTPUT;
#endif

	// �ǂݍ��܂�Ă��Ȃ��H
	if (m_pImage == NULL) return ERROR_NONE;

	// �o�̓t�@�C���I�[�v��
	FILE *fp;
	if ((fp = fopen(pFileName, "wb")) == NULL) {
		DBG_PRINT("file can't open!\n");
		return ERROR_OPEN;
	}

	// �w�b�_�[�o��
	this->WriteHeader(fp);

	// �p���b�g�o��
	if (m_pPalette != NULL) {
		fwrite(m_pPalette, m_PaletteSize, 1, fp);
	}

	// �C���[�W�o��
	fwrite(m_pImage, m_ImageSize, 1, fp);

	// �t�b�^�[�o��
	this->WriteFooter(fp);

	fclose(fp);

	return ERROR_NONE;
}

/*=======================================================================
�y�@�\�zBMP�o��
�y�����zpFileName�F�o�̓t�@�C����
 =======================================================================*/
int CTga::OutputBMP(const char *pFileName)
{
#ifndef NDEBUG
	_ASSERT(pFileName != NULL);
#else
	if (pFileName == NULL) return ERROR_OUTPUT;
#endif

	// �ǂݍ��܂�Ă��Ȃ��H
	if (m_pImage == NULL) return ERROR_NONE;

	// �o�̓t�@�C���I�[�v��
	FILE *fp;
	if ((fp = fopen(pFileName, "wb")) == NULL) {
		DBG_PRINT("file can't open!\n");
		return ERROR_OPEN;
	}

	// BMP�w�b�_�[�쐬
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


	// �w�b�_�[�o��
	fwrite(&bmHead, hsize, 1, fp);
	fwrite(&bmInfo, isize, 1, fp);

	// �p���b�g�o��
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

	// �C���[�W�o�́i����4�Ŋ���Ȃ��摜�ւ̑Ή��͂��Ă��܂���j
	this->ConvertType(IMAGE_LINE_LRDU); // �����E�A������z��ɕϊ�
	fwrite(m_pImage, m_ImageSize, 1, fp);

	fclose(fp);

	return ERROR_NONE;
}

/*=======================================================================
�y�@�\�zBGRA�z���RGBA�z��ɕύX
�y���l�zRGBA�ɕύX�������ƍēx�ĂԂ�BGRA�z��ɂȂ�܂��B
 =======================================================================*/
bool CTga::ConvertRGBA(void)
{
	if (m_pImage == NULL) return false;

	uint8 r, b;
	uint8 byte;
	uint32 offset;

	// �p���b�g
	if (m_pPalette) {
		byte = m_Header.paletteBit >> 3;

		for (offset = 0; offset < m_PaletteSize; offset += byte) {
			r = m_pPalette[offset + 2];
			b = m_pPalette[offset + 0];
			m_pPalette[offset + 2] = b;
			m_pPalette[offset + 0] = r;
		}
	}

	// �C���[�W
	byte = m_Header.imageBit >> 3;

	if (m_Header.imageBit <= 8) {
		// IndexColor�Ȃ珈�����Ȃ�
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
�y�@�\�z�w��̃r�b�g�z��ɕϊ�
�y�����ztype�F���C���^�C�v
 =======================================================================*/
bool CTga::ConvertType(const sint32 type)
{
	if (type >= IMAGE_LINE_MAX) return false;
	if (m_pImage == NULL) return false;

	// �ꏏ�Ȃ珈���Ȃ�
	if ((m_Header.discripter & 0xf0) == type) return true;

	uint8 *pImage;

	// �ϊ��p�̃������m��
	if ((pImage = new uint8[m_ImageSize]) == NULL) {
		return false;
	}

	// �z��ϊ�
	uint16 tx, ty;
	uint32 ofsSrc, ofsDst;

	ofsDst = 0;
	for (int y = 0; y < m_Header.imageH; y++) {
		ty = y;
		if ((m_Header.discripter & 0x20) != (type & 0x20)) {
			// ���݂���Y��������v���Ȃ��Ȃ甽�]
			ty = m_Header.imageH - y - 1;
		}

		for (int x = 0; x < m_Header.imageW; x++) {
			tx = x;
			if ((m_Header.discripter & 0x10) != (type & 0x10)) {
				// ���݂���X��������v���Ȃ��Ȃ甽�]
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

	// ����j�����ĕϊ���̃f�[�^��ێ�
	SAFE_DELETES(m_pImage);
	m_pImage = pImage;

	// �C���[�W�L�q�q��ύX
	m_Header.discripter = type;

	return true;
}

/*=======================================================================
�y�@�\�z���N���A
�y���l�z����J
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
�y�@�\�z�Ή��`�F�b�N
�y�����zheader�FTGA�w�b�_�[
�y���l�z����J
 =======================================================================*/
bool CTga::CheckSupport(const TGAHeader &header)
{
	// ���_��0�ł͂Ȃ��H
	if (header.imageX != 0 && header.imageY != 0) return false;

	// �Ή����Ă��Ȃ��C���[�W�^�C�v�H
	if (!((IMAGE_TYPE_NONE < header.imageType && header.imageType < IMAGE_TYPE_MAX) ||
		  (IMAGE_TYPE_INDEX_RLE <= header.imageType && header.imageType < IMAGE_TYPE_RLE_MAX))) {
		return false;
	}

	// �Ή��r�b�g�H
	if (header.imageBit !=  8 && header.imageBit != 16 &&
		header.imageBit != 24 && header.imageBit != 32) {
		return false;
	}

	// �Ή��p���b�g�H
	if (header.usePalette) {
		if (header.paletteIndex != 0) return false;
		if (header.paletteBit != 24 && header.paletteBit != 32) return false;
	}

	return true;
}

/*=======================================================================
�y�@�\�zTGA�w�b�_�[�ǂݍ���
�y�����zpSrc�F�摜�f�[�^�A�h���X
�y���l�z����J
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
�y�@�\�zTGA�t�b�^�[�ǂݍ���
�y�����zpSrc�@�F�摜�f�[�^�A�h���X
        offset�F�t�b�^�[�܂ł̃I�t�Z�b�g
�y���l�z����J
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
�y�@�\�zImage/Palette�T�C�Y�����߂�
�y�����zbFlg�F�������m�ۂ��s���H
�y���l�z����J
 =======================================================================*/
bool CTga::CalcSize(const bool bFlg)
{
	m_ImageSize   = m_Header.imageW * m_Header.imageH * (m_Header.imageBit >> 3);
	m_PaletteSize = m_Header.usePalette * m_Header.paletteColor * (m_Header.paletteBit >> 3);

	if (bFlg) {
		if ((m_pImage = new uint8[m_ImageSize]) == NULL) return false;

		// �p���b�g����Ȃ烁�����m��
		if (m_Header.usePalette) {
			if ((m_pPalette = new uint8[m_PaletteSize]) == NULL) return false;
		}
	}

	return true;
}

/*=======================================================================
�y�@�\�zImage�ǂݍ���
�y�����zpSrc  �F�摜�f�[�^�A�h���X
        size  �F�摜�f�[�^�T�C�Y
        offset�F�ۑ��挳�C���[�W�T�C�Y�iRLE�̏ꍇ�͈��k���̃T�C�Y�j
�y���l�z����J
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
		// RLE���k
		offset = this->UnpackRLE(m_pImage, pWork, size);
		if (offset == static_cast<uint32>(-1)) return false;
	} else {
		// �񈳏k
		memcpy(pImage, pWork, m_ImageSize);
		offset = m_ImageSize;
	}

	if (pOffset != NULL) *pOffset = offset;

	return true;
}

/*=======================================================================
�y�@�\�zPalette�ǂݍ���
�y�����zpSrc�F�摜�f�[�^�A�h���X
�y���l�z����J
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
	uint16 i; // VC6.0�ł̓R���p�C�����ʂ�Ȃ��̂ŊO�ɏo��

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
�y�@�\�zRLE���k��
�y�����zpDst�F�W�J��
        pSrc�F�摜�f�[�^�A�h���X
        size�F�摜�f�[�^�T�C�Y
�y���l�z����J
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

	uint8 byte = m_Header.imageBit >> 3; // �o�C�g�T�C�Y

//	while (reinterpret_cast<uint32>(pDst) < endAddr) {
	while (count < m_ImageSize) {
		bFlg = (pSrc[offset] & 0x80) ? false : true; // ��ʃr�b�g��0�Ȃ烊�e�����O���[�v
		loop = (pSrc[offset] & 0x7f) + 1;
		offset++;

		if (bFlg) {
			// ���e�����O���[�v
			// ����o�C�g�̌��ipSrc[offset] & 0x7f)+1�̃f�[�^�i�s�N�Z���o�C�g�P�ʁj���R�s�[����
			for (uint16 i = 0; i < loop; i++) {
				for (uint32 j = 0; j < byte; j++) {
//					*pDst++ = pSrc[offset++];
					pDst[count++] = pSrc[offset++];
				}
			}
		} else {
			// ����
			// ���ɑ����f�[�^�o�C�g�i�s�N�Z���o�C�g�P�ʁj���ipSrc[offset] & 0x7f)+1��J��Ԃ�
			for (uint16 i = 0; i < loop; i++) {
				for (uint32 j = 0; j < byte; j++) {
//					*pDst++ = pSrc[offset + j];
					pDst[count++] = pSrc[offset + j];
				}
			}
			offset += byte;
		}

		// �𓀂̂������`�F�b�N
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
�y�@�\�zTGA�w�b�_�[�o��
�y�����zfp     �FFILE�|�C���^
        pHeader�F�o�͂���TGA�w�b�_�[
�y���l�z�A���C�����g�ɉ����Ă��Ȃ��̂Ōʏo�́B
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

	// TODO:RLE���k�o�͑Ή�
	//      2008/04/01���_�ł͔�Ή�
	if (IMAGE_TYPE_INDEX_RLE <= pHeader->imageType && pHeader->imageType < IMAGE_TYPE_RLE_MAX) {
		pHeader->imageType -= 8; // �񈳏k�ɂ���
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
�y�@�\�zTGA�t�b�^�[�o��
�y�����zfp     �FFILE�|�C���^
        pHeader�F�o�͂���TGA�t�b�^�[
�y���l�z�A���C�����g�ɉ����Ă��Ȃ��̂Ōʏo�́B
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

	// ���摜�Ƀt�b�^�[���t���Ă������`�F�b�N
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
