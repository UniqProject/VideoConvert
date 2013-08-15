// CpuExtensions.cpp : Definiert die exportierten Funktionen für die DLL-Anwendung.
//

#include "stdafx.h"
#include "CpuExtensions.h"
#include <intrin.h>


#ifdef _WIN32
//  Windows
#define cpuid    __cpuid
#else
//  GCC Inline Assembly
void cpuid(int CPUInfo[4],int InfoType){
    __asm__ __volatile__ (
        "cpuid":
        "=a" (CPUInfo[0]),
        "=b" (CPUInfo[1]),
        "=c" (CPUInfo[2]),
        "=d" (CPUInfo[3]) :
        "a" (InfoType)
    );
}
#endif

CPUEXTENSIONS_API void GetExtensions(Extensions* result)
{
	int info[4];
	cpuid(info, 0);
	int nIds = info[0];

	cpuid(info, 0x80000000);
	int nExIds = info[0];

	//  Detect Instruction Set
	if (nIds >= 1){
		cpuid(info,0x00000001);
		result->MMX   = (info[3] & ((int)1 << 23)) != 0;
		result->SSE   = (info[3] & ((int)1 << 25)) != 0;
		result->SSE2  = (info[3] & ((int)1 << 26)) != 0;
		result->SSE3  = (info[2] & ((int)1 <<  0)) != 0;

		result->SSSE3 = (info[2] & ((int)1 <<  9)) != 0;
		result->SSE41 = (info[2] & ((int)1 << 19)) != 0;
		result->SSE42 = (info[2] & ((int)1 << 20)) != 0;

		result->AVX   = (info[2] & ((int)1 << 28)) != 0;
		result->FMA3  = (info[2] & ((int)1 << 12)) != 0;

		cpuid(info,0x00000007);
		result->AVX2  = (info[1] & ((int)1 << 5)) != 0;
	}

	if (nExIds >= 0x80000001){
		cpuid(info,0x80000001);
		result->x64   = (info[3] & ((int)1 << 29)) != 0;
		result->SSE4a = (info[2] & ((int)1 <<  6)) != 0;
		result->FMA4  = (info[2] & ((int)1 << 16)) != 0;
		result->XOP   = (info[2] & ((int)1 << 11)) != 0;
	}
}
