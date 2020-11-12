/*=============================================================================
 * �{���ɂ悭�g�����̂������W�߂��w�b�_�[�B
 * �c�[����e�X�g�v���O�����Ɏg���邱�Ƃ�z��B
 * C/C++�Ή��i�C�����C����Ή�C�R���p�C���͒m��Ȃ��j
 * 
 * Fast 2010/08/24
 * Last 2010/11/11  Ver1.03                                             (c)T.A
=============================================================================*/
#ifndef _MTO_COMMON_H_
#define _MTO_COMMON_H_

/*---------------------------------------------------------------------------
 * �I�v�V����
 *--------------------------------------------------------------------------*/
#if defined(__GNUC__) || defined(_MSC_VER) && (_MSC_VER >= 1400) // 1400:VC++2005
#define _USE_INT64				// int64���g�p����H
#endif

#if defined(_MSC_VER)
#define _USE_CRTDBG				// ���������[�N�`�F�b�N(VC++����)
#endif

#if _MSC_VER >= 1400
#pragma warning(disable:4201)	// anonymous unions warning
#pragma warning(disable:4996)	// 'function' ���Â��`���Ƃ��Đ錾����܂����B
#pragma warning(disable:4127)	// ���������萔�ł��B
#endif

#if defined(__GNUC__)
#define _USE_INLINE				// C��inline���g�p����H
#endif


/*---------------------------------------------------------------------------
 * �S�̂Ŏg�p����W�����C�u����
 *--------------------------------------------------------------------------*/
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <assert.h>
#include <math.h>


/*---------------------------------------------------------------------------
 * �悭�g�p����񋓌^
 *--------------------------------------------------------------------------*/
enum {X = 0, Y, Z, W};
enum {R = 0, G, B, A};

enum {
	off = 0,
	on
};

enum {
	eDIR_N = 0,
	eDIR_U,
	eDIR_D,
	eDIR_L,
	eDIR_R,
	eDIR_MAX
};

enum {
	eLP_ERROR = -1,
	eLP_BUSY  = 0,
	eLP_TRUE,
	eLP_CANCEL
};


/*---------------------------------------------------------------------------
 * �悭�g�p����}�N��
 *--------------------------------------------------------------------------*/
#ifndef SAFE_FREE
#define SAFE_FREE(p)			{if(p) {free(p);p = NULL;}}
#endif
#ifndef SAFE_DELETE
#define SAFE_DELETE(p)			{if(p) {delete (p);(p) = NULL;}}
#endif
#ifndef SAFE_DELETES
#define SAFE_DELETES(p)			{if(p) {delete[] (p);(p) = NULL;}}
#endif
#ifndef SAFE_RELEASE
#define SAFE_RELEASE(p)			{if(p) {(p)->Release();(p) = NULL;}}
#endif

#ifndef BOUND
#define BOUND(n, a)				(((uint32)(n) + (a) - 1) & ~((a) - 1))
#endif

#ifndef NOTHING
#define NOTHING(arg)			((void)(arg))
#endif

#ifndef memcls
#define memcls(dest, size)		memset(dest, 0, size)
#endif

#ifndef TRUE
#define TRUE					(1)
#endif
#ifndef FALSE
#define FALSE					(0)
#endif

#define PI						3.141592654f
#define RAD						(PI / 180.0f)
#define DEG						(180.0f / PI)
#define DEGtoRAD(x)				((x) * RAD)
#define RADtoDEG(x)				((x) * DEG)

#define A_MAX					0xff
#define RGB_MAX					0xffffff
#define RGBA_MAX				0xffffffff

// �f�o�b�O�֌W
#ifndef NDEBUG
#if defined(_WINDOWS_)
#define DBG_PRINT				OutputDebugString
#define _ASSERT(x)				assert(x)
#else
#define DBG_PRINT				printf
#define _ASSERT(x)				assert(x)
#endif // _WINDOWS_
#else
#define DBG_PRINT				
#define _ASSERT(x)				
#endif // NDEBUG

// �f�o�b�O�p�̃������`�F�b�N
#if defined(_USE_CRTDBG) && !defined(NDEBUG)
#include <crtdbg.h>

#define SET_CRTDBG()			_CrtSetDbgFlag(_CRTDBG_ALLOC_MEM_DF  | \
											   _CRTDBG_LEAK_CHECK_DF | \
											   _CRTDBG_DELAY_FREE_MEM_DF)
#else
#define SET_CRTDBG()			
#endif


/*---------------------------------------------------------------------------
 * �݊��p�ϐ�
 *--------------------------------------------------------------------------*/
#ifdef _USE_INT64
typedef unsigned long long		uint64;
typedef signed long long		sint64;
#endif //_USE_INT64

typedef unsigned int			uint32;
typedef signed int				sint32;
typedef unsigned short			uint16;
typedef signed short			sint16;
typedef unsigned char			uint8;
typedef signed char				sint8;

typedef float					FVector3[3];
typedef float					FVector4[4];
typedef unsigned char			Color3[3];
typedef unsigned char			Color4[4];


/*---------------------------------------------------------------------------
 * �ėp�\����
 *--------------------------------------------------------------------------*/
typedef struct tagDVECTOR {
	sint16 vx;
	sint16 vy;
} DVECTOR;

typedef struct tagMultiPOINT {
	union {
		sint32 p[2];

		struct {
			sint32 x;
			sint32 y;
		};
		struct {
			sint32 w;
			sint32 h;
		};
		struct {
			sint32 u;
			sint32 v;
		};
		struct {
			sint32 p0;
			sint32 p1;
		};
	};
} MPOINT;

typedef struct tagRGBAQUAD {
	union {
		uint32 RGBA;

		struct {
			uint8 r;
			uint8 g;
			uint8 b;
			uint8 a;
		};
	};
} RGBAQUAD;

typedef struct tagRGBAQUADF { 
	float r;
	float g;
	float b;
	float a;
} RGBAQUADF;

#ifndef _WINDOWS_
/*
  MEMO:windows.h���g�p���Ȃ��ꍇ�̌݊��p�ł��B
*/
#pragma pack(2)
typedef struct tagBITMAPFILEHEADER {
	uint16   bfType;
	uint32   bfSize;
	uint16   bfReserved1;
	uint16   bfReserved2;
	uint32   bfOffBits;
} BITMAPFILEHEADER;
#pragma pack()

typedef struct tagBITMAPINFOHEADER {
	uint32   biSize;
	uint32   biWidth;
	uint32   biHeight;
	uint16   biPlanes;
	uint16   biBitCount;
	uint32   biCompression;
	uint32   biSizeImage;
	uint32   biXPelsPerMeter;
	uint32   biYPelsPerMeter;
	uint32   biClrUsed;
	uint32   biClrImportant;
} BITMAPINFOHEADER;

typedef struct tagRGBQUAD { 
	uint8	rgbBlue;
	uint8	rgbGreen;
	uint8	rgbRed;
	uint8	 pad;
} RGBQUAD;
#endif


/*---------------------------------------------------------------------------
 * �ėpinline�֐�
 *--------------------------------------------------------------------------*/
#if defined(_LANGUAGE_C_PLUS_PLUS) || defined(__cplusplus) || defined(c_plusplus) || defined(_USE_INLINE)
#define MTOINLINE inline
#else
#define MTOINLINE _inline
#endif

static MTOINLINE uint16 MtoBitReverse16(const uint16 num)
{
	return (uint16)(((num & 0xff00) >> 8) | ((num & 0x00ff) << 8));
}

static MTOINLINE uint32 MtoBitReverse32(const uint32 num)
{
	return (uint32)(((num & 0x000000ff) << 24) | ((num & 0x0000ff00) << 8) |
					((num & 0x00ff0000) >> 8) | ((num & 0xff000000) >> 24));
}

#ifdef _USE_INT64
static MTOINLINE uint64 MtoBitReverse64(const uint64 num)
{
	return (uint64)(((num & 0x00000000000000ffLL) << 56) | ((num & 0x000000000000ff00LL) << 40) |
					((num & 0x0000000000ff0000LL) << 24) | ((num & 0x00000000ff000000LL) <<  8) |
					((num & 0xff00000000000000LL) >> 56) | ((num & 0x00ff000000000000LL) >> 40) |
					((num & 0x0000ff0000000000LL) >> 24) | ((num & 0x000000ff00000000LL) >>  8));
}
#endif


/*=======================================================================
�y�@�\�z�t�@�C���I�[�v��
�y�����zfp�@ �F�t�@�C���|�C���^
�@�@�@�@fname�F�t�@�C����
�@�@�@�@opt  �F�t�@�C���I�v�V����
�y�ߒl�z�t�@�C���T�C�Y(0:���s)
 =======================================================================*/
static MTOINLINE uint32 MtoFileOpen(FILE **fp, const char *fname, const char *opt)
{
	uint32 size;

#ifndef NDEBUG
	_ASSERT(fp != NULL);
	_ASSERT(fname != NULL);
	_ASSERT(opt != NULL);
#endif

	// files open
	if ((*fp = fopen(fname, opt)) == NULL) {
		return 0;
	}

	// get file size
	fseek(*fp, 0, SEEK_END);
	size = ftell(*fp);
	fseek(*fp, 0, SEEK_SET);

	return size;
}

/*=======================================================================
�y�@�\�z�󔒃`�F�b�N
�y�����zc�F����
 =======================================================================*/
static MTOINLINE sint32 MtoIsSpace(const char c)
{
	if ((0x09 <= c && c <= 0x0D) || c == 0x20) {
		return TRUE;
	}
	return FALSE;
}

/*=======================================================================
�y�@�\�z����(0-9)�`�F�b�N
�y�����zc�F����
 =======================================================================*/
static MTOINLINE sint32 MtoIsNumber(const char c)
{
	if (0x30 <= c && c <= 0x39) {
		return TRUE;
	}
	return FALSE;
}

#endif
