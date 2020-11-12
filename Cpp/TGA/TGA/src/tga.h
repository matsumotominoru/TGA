#ifndef _TGA_H_
#define _TGA_H_

class CTga {
public:
	// �C���[�W�^�C�v
	enum {
		IMAGE_TYPE_NONE = 0,		// �C���[�W�Ȃ�
		IMAGE_TYPE_INDEX,			// 256�F
		IMAGE_TYPE_FULL,			// �t���J���[
		IMAGE_TYPE_GRAY,			// ����
		IMAGE_TYPE_MAX,
		IMAGE_TYPE_INDEX_RLE = 9,	// 256�F(RLE���k)
		IMAGE_TYPE_FULL_RLE,		// �t���J���[(RLE���k)
		IMAGE_TYPE_GRAY_RLE,		// ����(RLE���k)
		IMAGE_TYPE_RLE_MAX
	};

	// �s�N�Z���̕���
	enum {
		IMAGE_LINE_LRDU = 0x00,		// �����E�A������
		IMAGE_LINE_RLDU = 0x10,		// �E�����A������
		IMAGE_LINE_LRUD = 0x20,		// �����E�A�と��
		IMAGE_LINE_RLUD = 0x30,		// �E�����A�と��
		IMAGE_LINE_MAX
	};

	enum {
		HEADER_SIZE = 0x12,			// �w�b�_�[�T�C�Y
		FOOTER_SIZE = 0x1a			// �t�b�^�[�T�C�Y
	};

	// �G���[�^�C�v
	enum {
		ERROR_OPEN    = -1,			// �t�@�C���I�[�v�����s
		ERROR_MEMORY  = -2,			// �������m�ێ��s
		ERROR_HEADER  = -3,			// �T�|�[�g���Ă��Ȃ��`��
		ERROR_PALETTE = -4,			// �p���b�g�f�[�^���s��
		ERROR_IMAGE   = -5,			// �C���[�W�f�[�^���s��
		ERROR_OUTPUT  = -6,			// �o�̓G���[
		ERROR_NONE    =  1,			// �G���[�Ȃ�
		ERROR_MAX
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

private:
	TGAHeader	m_Header;
	TGAFooter	m_Footer;

	uint8		*m_pImage;			// �s�N�Z���f�[�^
	uint8		*m_pPalette;		// �p���b�g�f�[�^
	uint32		m_ImageSize;		// �s�N�Z���f�[�^�T�C�Y
	uint32		m_PaletteSize;		// �p���b�g�f�[�^�T�C�Y

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
