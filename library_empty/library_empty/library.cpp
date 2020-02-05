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
* Temat: Program zmieniaj�cy rozdzielczo�� wielu zdj�� 
*		 do wybranego rozmiaru
*/
///////////////////////////////////////////////////////////

/**
*	Skaluje obraz u�ywaj�c interpolacji dwuliniowej
*	
*	@param char* src - Wska�nik na tablic� zawieraj�c� piksele zdj�cia �r�d�owego (format BGRA)
*	@param char* dst - Wska�nik na tablic� zawieraj�c� piksele zdj�cia docelowego (format BGRA)
*	@param int width - Szeroko�� �r�d�owego obrazu
*	@param int height - Wysoko�� �r�d�owego obrazu
*	@param int newWidth - Szeroko�� docelowego zdj�cia
*	@param int newHeight - Wysoko�� docelowego zdj�cia
*	@return int - Je�eli skalowanie dobiegnie ko�ca zwraca 0
*/
int interpolateC(char* src, char* dst, int width, int height, int newWidth, int newHeight) {
	/*
	*	Unia opisuj�ca sk�adowe piksela (32bit BGRA)
	*/
	union pixel {
		/*
		*	Warto�� piksela jako 32bit zmienna ca�kowita
		*/
		uint32_t value;
		/*
		*	Dekompozycja piksela na sk�adowe
		*/
		struct {
			uint8_t b : 8;
			uint8_t g : 8;
			uint8_t r : 8;
			uint8_t a : 8;
		} color;
	};
	//Wska�niki z 32 bitowym przesuni�ciem (1 pe�ny piksel) dla
	//tablicy �r�d�owej i docelowej
	uint32_t *ptrDst = (uint32_t*)dst;
	uint32_t *ptrSrc = (uint32_t*)src;

	//Przesuni�cie wska�nik�w w taki spos�b, �eby pierwszy piksel
	//dost�pny by� pod adresem (1,1) zamiast (0,0)
	ptrSrc = ptrSrc - width - 1;
	ptrDst = ptrDst - newWidth - 1;

	//Obliczenie skali - przelicznika, wed�ug kt�rego piksele 
	//z docelowego obrazu b�d� mapowane na punkty w obrazie �r�d�owym
	float scaleW = float(width) / newWidth;
	float scaleH = float(height) / newHeight;

	//Dla ka�dego wiersza obrazu docelowego wykonujemy obliczenia
	for (int row = 1; row <= newHeight; ++row) {
		//Obliczamy odpowiadaj�cy wiersz w obrazie �r�d�owym
		float refRowPos = row * scaleH;

		//Zaokr�glamy w d� do ca�o�ci
		int refRow = (int)refRowPos;
		//wiersza numerujemy w zakresie <1;height>
		//je�eli wiersz < 1, ustaw go na 1
		if (refRow < 1) {
			refRow = 1;
		}
		//je�eli wiersz > height, ustaw go na height
		else if (refRow > height) {
			refRow = height;
		}

		//Warto�� delta - o ile obliczony wiersz pokrywa si� z pozycj� na obrazie
		float deltaRow = refRowPos - refRow;
		//warto�� delta musi zawiera� si� w przedziale <0;1>
		//Je�eli jest mniejsza od 0, ustaw 0
		if (deltaRow < 0) {
			deltaRow = 0;
		}
		//Dla ka�dej kolumny numerowanej od 1 do newWidth
		for (int col = 1; col <= newWidth; ++col) {
			//Obliczenie odpowiadaj�cej kolumny w obrazie docelowym
			float refColPos = col * scaleW;
			//Zaokr�glenie w d� do cz�ci ca�kowitej
			int refCol = (int)refColPos;
			//Kolumna referencyjna zawiera si� w przedziale <1;width>
			//Je�eli kolumna < 1, ustaw na 1
			if (refCol < 1) {
				refCol = 1;
			}
			//Je�eli kolumna > width, ustaw na width
			else if (refCol > width) {
				refCol = width;
			}
			//Warto�� delta - o ile obliczona kolumna pokrywa si�
			//z pozycj� na obrazie
			float deltaCol = refColPos - refCol;
			if (deltaCol < 0) {
				deltaCol = 0;
			}

			//Cztery s�siednie piksele �r�d�owe i piksel docelowy
			pixel src1, src2, src3, src4, dst;
			//Obliczenie warto�ci s�siednich pikseli
			//S�siednie piksele to b�d� piksele po prawej stronie(kolumna+1)
			//i wiersz ni�ej (wiersz + 1). Poniewa� obliczany jest te� ostatni
			//wiersz i ostatnia kolumna, nale�y si� zabezpieczy� przed wyj�ciem
			//poza zakres obrazka.
			//Obliczenie wiersza i kolumny s�siad�w
			int nextRefCol = refCol < width ? refCol + 1 : refCol;
			int nextRefRow = refRow < height ? refRow + 1 : refRow;

			//Za�adowanie warto�ci pikseli �r�d�owych
			src1.value = ptrSrc[refCol + width * refRow];
			src2.value = ptrSrc[nextRefCol + width * refRow];
			src3.value = ptrSrc[refCol + width * nextRefRow];
			src4.value = ptrSrc[nextRefCol + width * nextRefRow];
			
			
			
			//Obliczenie warto�ci sk�adowej Red jako sumy warto�ci poszczeg�lnych
			//pikseli pomno�onych przez wagi odpowiadaj�ce pozycji danego
			//piksela wzgl�dem pr�bkowanego punktu
			float value = src1.color.r * (1 - deltaRow) * (1 - deltaCol) +
				src2.color.r * (1 - deltaRow) * deltaCol +
				src3.color.r * deltaRow * (1 - deltaCol) +
				src4.color.r * deltaRow * deltaCol;
			//Wpisanie obliczonej warto�ci sk�adowej Red do piksela docelowego
			dst.color.r = value;
			//Obliczenie warto�ci sk�adowej Green jako sumy warto�ci poszczeg�lnych
			//pikseli pomno�onych przez wagi odpowiadaj�ce pozycji danego
			//piksela wzgl�dem pr�bkowanego punktu
			value = src1.color.g * (1 - deltaRow) * (1 - deltaCol) +
				src2.color.g * (1 - deltaRow) * deltaCol +
				src3.color.g * deltaRow * (1 - deltaCol) +
				src4.color.g * deltaRow * deltaCol;
			//Wpisanie obliczonej warto�ci sk�adowej Green do piksela docelowego
			dst.color.g = value;
			//Obliczenie warto�ci sk�adowej Blue jako sumy warto�ci poszczeg�lnych
			//pikseli pomno�onych przez wagi odpowiadaj�ce pozycji danego
			//piksela wzgl�dem pr�bkowanego punktu
			value = src1.color.b * (1 - deltaRow) * (1 - deltaCol) +
				src2.color.b * (1 - deltaRow) * deltaCol +
				src3.color.b * deltaRow * (1 - deltaCol) +
				src4.color.b * deltaRow * deltaCol;
			//Wpisanie obliczonej warto�ci sk�adowej Blue do piksela docelowego
			dst.color.b = value;
			//Obliczenie warto�ci sk�adowej Alpha jako sumy warto�ci poszczeg�lnych
			//pikseli pomno�onych przez wagi odpowiadaj�ce pozycji danego
			//piksela wzgl�dem pr�bkowanego punktu i wpisanie do piksela docelowego
			dst.color.a = src1.color.a * (1 - deltaRow) * (1 - deltaCol) +
				src2.color.a * (1 - deltaRow) * deltaCol +
				src3.color.a * deltaRow * (1 - deltaCol) +
				src4.color.a * deltaRow * deltaCol;
			//Za�adowanie obliczonego piksela docelowego do obrazu docelowego
			ptrDst[col + newWidth * row] = dst.value;

		}
	}
	//Na koniec zwr�� 0
	return 0;
}