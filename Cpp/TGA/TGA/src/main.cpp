#include "mto_common.h"
#include "tga.h"


int main(int argc, char* argv[])
{
	SET_CRTDBG();

	// �����`�F�b�N
	if (argc <= 1) return 1;

	// �g���q�`�F�b�N
	const char *pStr = strrchr(argv[1], '.');
	if (pStr == NULL) return 1;

	char ext[_MAX_EXT];
	strcpy(ext, pStr);
	if (strcmp(ext, ".tga") != 0 && strcmp(ext, ".TGA") != 0) return 1;

	// TGA�쐬
	CTga tga;
	int ret = tga.Create(argv[1]);

	if (ret < 0) {
		switch (ret) {
			case CTga::ERROR_OPEN:
				printf("File open error!!\n");
				break;
			case CTga::ERROR_MEMORY:
				printf("Memory alloc error!!\n");
				break;
			case CTga::ERROR_HEADER:
				printf("Not support error!!\n");
				break;
			case CTga::ERROR_PALETTE:
				printf("Not support palette data\n");
				break;
			case CTga::ERROR_IMAGE:
				printf("Not support image data\n");
				break;
			case CTga::ERROR_OUTPUT:
				printf("Output error!!\n");
				break;
		}
	}

	// TGA�o��
	tga.ConvertType(CTga::IMAGE_LINE_RLDU);
	ret = tga.Output("output.tga");
	if (ret < 0) {
		switch (ret) {
			case CTga::ERROR_OPEN:
				printf("File open error!!\n");
				break;
			case CTga::ERROR_OUTPUT:
				printf("Output error!!\n");
				break;
		}
	}

	// BMP�o��
//	tga.ConvertRGBA(); // BGR->RGB
	ret = tga.OutputBMP("output.bmp");
	if (ret < 0) {
		switch (ret) {
			case CTga::ERROR_OPEN:
				printf("File open error!!\n");
				break;
			case CTga::ERROR_OUTPUT:
				printf("Output error!!\n");
				break;
		}
	}

	return 0;
}

