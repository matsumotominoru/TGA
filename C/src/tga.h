#ifndef _TGA_H_
#define _TGA_H_

// �C���[�W�^�C�v
enum {
	TGA_IMAGE_TYPE_NONE = 0,		// �C���[�W�Ȃ�
	TGA_IMAGE_TYPE_INDEX,			// 256�F
	TGA_IMAGE_TYPE_FULL,			// �t���J���[
	TGA_IMAGE_TYPE_GRAY,			// ����
	TGA_IMAGE_TYPE_MAX,
	TGA_IMAGE_TYPE_INDEX_RLE = 9,	// 256�F(RLE���k)
	TGA_IMAGE_TYPE_FULL_RLE,		// �t���J���[(RLE���k)
	TGA_IMAGE_TYPE_GRAY_RLE,		// ����(RLE���k)
	TGA_IMAGE_TYPE_RLE_MAX
};

// �s�N�Z���̕���
enum {
	TGA_IMAGE_LINE_LRDU = 0x00,		// �����E�A������
	TGA_IMAGE_LINE_RLDU = 0x10,		// �E�����A������
	TGA_IMAGE_LINE_LRUD = 0x20,		// �����E�A�と��
	TGA_IMAGE_LINE_RLUD = 0x30,		// �E�����A�と��
	TGA_IMAGE_LINE_MAX
};

enum {
	TGA_HEADER_SIZE = 0x12,			// �w�b�_�[�T�C�Y
	TGA_FOOTER_SIZE = 0x1a			// �t�b�^�[�T�C�Y
};

// �G���[�^�C�v
enum {
	TGA_ERROR_OPEN    = -1,			// �t�@�C���I�[�v�����s
	TGA_ERROR_MEMORY  = -2,			// �������m�ێ��s
	TGA_ERROR_HEADER  = -3,			// �T�|�[�g���Ă��Ȃ��`��
	TGA_ERROR_PALETTE = -4,			// �p���b�g�f�[�^���s��
	TGA_ERROR_IMAGE   = -5,			// �C���[�W�f�[�^���s��
	TGA_ERROR_OUTPUT  = -6,			// �o�̓G���[
	TGA_ERROR_NONE    =  1,			// �G���[�Ȃ�
	TGA_ERROR_MAX
};

struct TGAHeader {
	uint8	IDField;			// ID�t�B�[���h�̃T�C�Y
	uint8	usePalette;			// �p���b�g�g�p�H
	uint8	imageType;			// �C���[�W�`��
	uint16	paletteIndex;		// �p���b�gIndex
	uint16	paletteColor;		// �p���b�g�̐F��
	uint8	paletteBit;			// 1�p���b�g�̃r�b�g��
	uint16	imageX;				// �C���[�WX���_
	uint16	imageY;				// �C���[�WY���_
	uint16	imageW;				// �C���[�W��
	uint16	imageH;				// �C���[�W����
	uint8	imageBit;			// �C���[�W�r�b�g��
	uint8	discripter;			// �C���[�W�L�q�q
};

struct TGAFooter {
	uint32	filePos;			// �t�@�C���̈ʒu(�G�N�X�e���V�����G���A�̈ʒu?)
	uint32	fileDev;			// developer directory �t�@�C���ʒu
	uint8	version[18];		// �hTRUEVISION-TARGA�h�̕����iversion[17]==0x00�j
};

struct TGA {
	struct TGAHeader	header;
	struct TGAFooter	footer;

	uint8	*pImage;			// �s�N�Z���f�[�^
	uint8	*pPalette;			// �p���b�g�f�[�^
	uint32	imageSize;			// �s�N�Z���f�[�^�T�C�Y
	uint32	paletteSize;		// �p���b�g�f�[�^�T�C�Y
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
int tgaOutput(const struct TGA *pTga, const char *pFileName);
int tgaOutputBMP(const struct TGA *pTga, const char *pFileName);
bool tgaConvertRGBA(struct TGA *pTga);
bool tgaConvertType(struct TGA *pTga, const sint32 type);

bool tgaWriteHeader(FILE *fp, TGAHeader *pHeader);
bool tgaWriteFooter(FILE *fp, TGAFooter *pHeader);

#endif
