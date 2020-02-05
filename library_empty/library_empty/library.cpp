#include <Windows.h>
#include <stdint.h>
#include "library.h"

///////////////////////////////////////////////////////////
/*
* Autor: Jakub Klimek
* Informatyka
* Semestr: 5
* Grupa dziekanska: 1-2
*
* Temat: Program zmieniaj¹cy rozdzielczoœæ wielu zdjêæ 
*		 do wybranego rozmiaru
*/
///////////////////////////////////////////////////////////

/**
*	Skaluje obraz u¿ywaj¹c interpolacji dwuliniowej
*	
*	@param char* src - WskaŸnik na tablicê zawieraj¹c¹ piksele zdjêcia Ÿród³owego (format BGRA)
*	@param char* dst - WskaŸnik na tablicê zawieraj¹c¹ piksele zdjêcia docelowego (format BGRA)
*	@param int width - Szerokoœæ Ÿród³owego obrazu
*	@param int height - Wysokoœæ Ÿród³owego obrazu
*	@param int newWidth - Szerokoœæ docelowego zdjêcia
*	@param int newHeight - Wysokoœæ docelowego zdjêcia
*	@return int - Je¿eli skalowanie dobiegnie koñca zwraca 0
*/
int interpolateC(char* src, char* dst, int width, int height, int newWidth, int newHeight) {
	/*
	*	Unia opisuj¹ca sk³adowe piksela (32bit BGRA)
	*/
	union pixel {
		/*
		*	Wartoœæ piksela jako 32bit zmienna ca³kowita
		*/
		uint32_t value;
		/*
		*	Dekompozycja piksela na sk³adowe
		*/
		struct {
			uint8_t b : 8;
			uint8_t g : 8;
			uint8_t r : 8;
			uint8_t a : 8;
		} color;
	};
	//WskaŸniki z 32 bitowym przesuniêciem (1 pe³ny piksel) dla
	//tablicy Ÿród³owej i docelowej
	uint32_t *ptrDst = (uint32_t*)dst;
	uint32_t *ptrSrc = (uint32_t*)src;

	//Przesuniêcie wskaŸników w taki sposób, ¿eby pierwszy piksel
	//dostêpny by³ pod adresem (1,1) zamiast (0,0)
	ptrSrc = ptrSrc - width - 1;
	ptrDst = ptrDst - newWidth - 1;

	//Obliczenie skali - przelicznika, wed³ug którego piksele 
	//z docelowego obrazu bêd¹ mapowane na punkty w obrazie Ÿród³owym
	float scaleW = float(width) / newWidth;
	float scaleH = float(height) / newHeight;

	//Dla ka¿dego wiersza obrazu docelowego wykonujemy obliczenia
	for (int row = 1; row <= newHeight; ++row) {
		//Obliczamy odpowiadaj¹cy wiersz w obrazie Ÿród³owym
		float refRowPos = row * scaleH;

		//Zaokr¹glamy w dó³ do ca³oœci
		int refRow = (int)refRowPos;
		//wiersza numerujemy w zakresie <1;height>
		//je¿eli wiersz < 1, ustaw go na 1
		if (refRow < 1) {
			refRow = 1;
		}
		//je¿eli wiersz > height, ustaw go na height
		else if (refRow > height) {
			refRow = height;
		}

		//Wartoœæ delta - o ile obliczony wiersz pokrywa siê z pozycj¹ na obrazie
		float deltaRow = refRowPos - refRow;
		//wartoœæ delta musi zawieraæ siê w przedziale <0;1>
		//Je¿eli jest mniejsza od 0, ustaw 0
		if (deltaRow < 0) {
			deltaRow = 0;
		}
		//Dla ka¿dej kolumny numerowanej od 1 do newWidth
		for (int col = 1; col <= newWidth; ++col) {
			//Obliczenie odpowiadaj¹cej kolumny w obrazie docelowym
			float refColPos = col * scaleW;
			//Zaokr¹glenie w dó³ do czêœci ca³kowitej
			int refCol = (int)refColPos;
			//Kolumna referencyjna zawiera siê w przedziale <1;width>
			//Je¿eli kolumna < 1, ustaw na 1
			if (refCol < 1) {
				refCol = 1;
			}
			//Je¿eli kolumna > width, ustaw na width
			else if (refCol > width) {
				refCol = width;
			}
			//Wartoœæ delta - o ile obliczona kolumna pokrywa siê
			//z pozycj¹ na obrazie
			float deltaCol = refColPos - refCol;
			if (deltaCol < 0) {
				deltaCol = 0;
			}

			//Cztery s¹siednie piksele Ÿród³owe i piksel docelowy
			pixel src1, src2, src3, src4, dst;
			//Obliczenie wartoœci s¹siednich pikseli
			//S¹siednie piksele to bêd¹ piksele po prawej stronie(kolumna+1)
			//i wiersz ni¿ej (wiersz + 1). Poniewa¿ obliczany jest te¿ ostatni
			//wiersz i ostatnia kolumna, nale¿y siê zabezpieczyæ przed wyjœciem
			//poza zakres obrazka.
			//Obliczenie wiersza i kolumny s¹siadów
			int nextRefCol = refCol < width ? refCol + 1 : refCol;
			int nextRefRow = refRow < height ? refRow + 1 : refRow;

			//Za³adowanie wartoœci pikseli Ÿród³owych
			src1.value = ptrSrc[refCol + width * refRow];
			src2.value = ptrSrc[nextRefCol + width * refRow];
			src3.value = ptrSrc[refCol + width * nextRefRow];
			src4.value = ptrSrc[nextRefCol + width * nextRefRow];
			
			
			
			//Obliczenie wartoœci sk³adowej Red jako sumy wartoœci poszczególnych
			//pikseli pomno¿onych przez wagi odpowiadaj¹ce pozycji danego
			//piksela wzglêdem próbkowanego punktu
			float value = src1.color.r * (1 - deltaRow) * (1 - deltaCol) +
				src2.color.r * (1 - deltaRow) * deltaCol +
				src3.color.r * deltaRow * (1 - deltaCol) +
				src4.color.r * deltaRow * deltaCol;
			//Wpisanie obliczonej wartoœci sk³adowej Red do piksela docelowego
			dst.color.r = value;
			//Obliczenie wartoœci sk³adowej Green jako sumy wartoœci poszczególnych
			//pikseli pomno¿onych przez wagi odpowiadaj¹ce pozycji danego
			//piksela wzglêdem próbkowanego punktu
			value = src1.color.g * (1 - deltaRow) * (1 - deltaCol) +
				src2.color.g * (1 - deltaRow) * deltaCol +
				src3.color.g * deltaRow * (1 - deltaCol) +
				src4.color.g * deltaRow * deltaCol;
			//Wpisanie obliczonej wartoœci sk³adowej Green do piksela docelowego
			dst.color.g = value;
			//Obliczenie wartoœci sk³adowej Blue jako sumy wartoœci poszczególnych
			//pikseli pomno¿onych przez wagi odpowiadaj¹ce pozycji danego
			//piksela wzglêdem próbkowanego punktu
			value = src1.color.b * (1 - deltaRow) * (1 - deltaCol) +
				src2.color.b * (1 - deltaRow) * deltaCol +
				src3.color.b * deltaRow * (1 - deltaCol) +
				src4.color.b * deltaRow * deltaCol;
			//Wpisanie obliczonej wartoœci sk³adowej Blue do piksela docelowego
			dst.color.b = value;
			//Obliczenie wartoœci sk³adowej Alpha jako sumy wartoœci poszczególnych
			//pikseli pomno¿onych przez wagi odpowiadaj¹ce pozycji danego
			//piksela wzglêdem próbkowanego punktu i wpisanie do piksela docelowego
			dst.color.a = src1.color.a * (1 - deltaRow) * (1 - deltaCol) +
				src2.color.a * (1 - deltaRow) * deltaCol +
				src3.color.a * deltaRow * (1 - deltaCol) +
				src4.color.a * deltaRow * deltaCol;
			//Za³adowanie obliczonego piksela docelowego do obrazu docelowego
			ptrDst[col + newWidth * row] = dst.value;

		}
	}
	//Na koniec zwróæ 0
	return 0;
}