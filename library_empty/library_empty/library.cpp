#include <Windows.h>
#include <stdint.h>
#include "library.h"

int returnNumber() {
	return 5;
}

int interpolate(char* ptr) {
	union pixel {
		uint32_t value;
		struct {
			char b : 8;
			char g : 8;
			char r : 8;
			char a : 8;
		} color;
	};
	int i = 0;
	uint32_t *ptrI = (uint32_t*)ptr;
	pixel color;
	color.color.a = 0xff;
	color.color.r = 0xff;
	color.color.b = color.color.g = 0x00;

	*ptrI = color.value;
	color.color.b = 0xff;
	color.color.r = 0x00;
	color.color.g = 0x00;
	ptrI[1] = color.value;
	color.color.b = 0x00;
	color.color.r = 0x00;
	color.color.g = 0xff;
	ptrI[2] = color.value;
	color.color.b = 0xff;
	color.color.r = 0x00;
	color.color.g = 0x00;
	ptrI[3] = color.value;
	while (i < 136 * 120) {
		ptrI[i++] = color.value;
	}

	return 12;
}

int interpolateC(char* src, char* dst, int width, int height, int newWidth, int newHeight) {
	union pixel {
		uint32_t value;
		struct {
			uint8_t b : 8;
			uint8_t g : 8;
			uint8_t r : 8;
			uint8_t a : 8;
		} color;
	};

	uint32_t *ptrDst = (uint32_t*)dst;
	uint32_t *ptrSrc = (uint32_t*)src;
	ptrSrc = ptrSrc - width - 1;
	ptrDst = ptrDst - newWidth - 1;

	float scaleW = float(width) / newWidth;
	float scaleH = float(height) / newHeight;

	for (int row = 1; row <= newHeight; ++row) {
		float refRowPos = row * scaleH;

		int refRow = (int)refRowPos;
		if (refRow < 1) {
			refRow = 1;
		}
		else if (refRow > height) {
			refRow = height;
		}

		float deltaRow = refRowPos - refRow;
		if (deltaRow < 0) {
			deltaRow = 0;
		}

		for (int col = 1; col <= newWidth; ++col) {
			float refColPos = col * scaleW;

			int refCol = (int)refColPos;
			if (refCol < 1) {
				refCol = 1;
			}
			else if (refCol > width) {
				refCol = width;
			}

			float deltaCol = refColPos - refCol;
			if (deltaCol < 0) {
				deltaCol = 0;
			}


			pixel src1, src2, src3, src4, dst;
			int nextRefCol = refCol < width ? refCol + 1 : refCol;
			int nextRefRow = refRow < height ? refRow + 1 : refRow;
			src1.value = ptrSrc[refCol + width * refRow];
			src2.value = ptrSrc[nextRefCol + width * refRow];
			src3.value = ptrSrc[refCol + width * nextRefRow];
			src4.value = ptrSrc[nextRefCol + width * nextRefRow];
			
			
			

			float value = src1.color.r * (1 - deltaRow) * (1 - deltaCol) +
				src2.color.r * (1 - deltaRow) * deltaCol +
				src3.color.r * deltaRow * (1 - deltaCol) +
				src4.color.r * deltaRow * deltaCol;
			dst.color.r = value;
			value = src1.color.g * (1 - deltaRow) * (1 - deltaCol) +
				src2.color.g * (1 - deltaRow) * deltaCol +
				src3.color.g * deltaRow * (1 - deltaCol) +
				src4.color.g * deltaRow * deltaCol;
			dst.color.g = value;
			value = src1.color.b * (1 - deltaRow) * (1 - deltaCol) +
				src2.color.b * (1 - deltaRow) * deltaCol +
				src3.color.b * deltaRow * (1 - deltaCol) +
				src4.color.b * deltaRow * deltaCol;
			dst.color.b = value;
			dst.color.a = src1.color.a * (1 - deltaRow) * (1 - deltaCol) +
				src2.color.a * (1 - deltaRow) * deltaCol +
				src3.color.a * deltaRow * (1 - deltaCol) +
				src4.color.a * deltaRow * deltaCol;
			ptrDst[col + newWidth * row] = dst.value;

		}
	}
	return 0;
}