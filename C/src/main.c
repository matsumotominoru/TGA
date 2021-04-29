#include "mto_common.h"
#include "tga.h"


int main(int argc, char* argv[])
{
	SET_CRTDBG();

	// 引数チェック
	if (argc <= 1) return 1;

	// 拡張子チェック
	const char *pStr = strrchr(argv[1], '.');
	if (pStr == NULL) return 1;

	char ext[8] = {0};
	strcpy(ext, pStr);
	if (strcmp(ext, ".tga") != 0 && strcmp(ext, ".TGA") != 0) return 1;

	// TGA作成
	struct TGA tga;
	int ret = tgaCreateFile(&tga, argv[1]);

	if (ret < 0) {
		switch (ret) {
			case TGA_ERROR_OPEN:
				printf("File open error!!\n");
				break;
			case TGA_ERROR_MEMORY:
				printf("Memory alloc error!!\n");
				break;
			case TGA_ERROR_HEADER:
				printf("Not support error!!\n");
				break;
			case TGA_ERROR_PALETTE:
				printf("Not support palette data\n");
				break;
			case TGA_ERROR_IMAGE:
				printf("Not support image data\n");
				break;
			case TGA_ERROR_OUTPUT:
				printf("Output error!!\n");
				break;
		}
	}

	// TGA出力
	tgaConvertType(&tga, TGA_IMAGE_LINE_RLDU);
	ret = tgaOutput(&tga, "output.tga");
	if (ret < 0) {
		switch (ret) {
			case TGA_ERROR_OPEN:
				printf("File open error!!\n");
				break;
			case TGA_ERROR_OUTPUT:
				printf("Output error!!\n");
				break;
		}
	}

	// BMP出力
//	tga.ConvertRGBA(); // BGR->RGB
	ret = tgaOutputBMP(&tga, "output.bmp");
	if (ret < 0) {
		switch (ret) {
			case TGA_ERROR_OPEN:
				printf("File open error!!\n");
				break;
			case TGA_ERROR_OUTPUT:
				printf("Output error!!\n");
				break;
		}
	}

	return 0;
}

