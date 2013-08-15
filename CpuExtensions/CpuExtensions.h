// Folgender ifdef-Block ist die Standardmethode zum Erstellen von Makros, die das Exportieren 
// aus einer DLL vereinfachen. Alle Dateien in dieser DLL werden mit dem CPUEXTENSIONS_EXPORTS-Symbol
// (in der Befehlszeile definiert) kompiliert. Dieses Symbol darf für kein Projekt definiert werden,
// das diese DLL verwendet. Alle anderen Projekte, deren Quelldateien diese Datei beinhalten, erkennen 
// CPUEXTENSIONS_API-Funktionen als aus einer DLL importiert, während die DLL
// mit diesem Makro definierte Symbole als exportiert ansieht.
#ifdef CPUEXTENSIONS_EXPORTS
#define CPUEXTENSIONS_API extern "C" __declspec(dllexport)
#else
#define CPUEXTENSIONS_API __declspec(dllimport)
#endif


typedef struct Extensions {
	int x64;
	int MMX;
	int SSE;
	int SSE2;
	int SSE3;
	int SSSE3;
	int SSE41;
	int SSE42;
	int SSE4a;
	int AVX;
	int AVX2;
	int XOP;
	int FMA3;
	int FMA4;
}*ExtensionsPtr;

CPUEXTENSIONS_API void GetExtensions(Extensions* result);

