// AvsInfo.cpp : Definiert den Einstiegspunkt für die Konsolenanwendung.
//

#include "stdafx.h"
#include <Windows.h>
#include <string.h>
#include "internal.h"
#include <iostream>

int main(int argc, char* argv[])
{
	if (argc != 2) {
        return -1;
    }
    if (argc == 0) {
        std::cerr << "usage: avsInfo \"Path to the .avs file\"" << std::endl;
        return 0;
    }
    const char* infile = argv[1];
    const char *dot = strrchr(infile, '.');
    if (!dot || strcmp(".avs", dot)) {
        std::cerr << infile << " is no .avs file" << std::endl;
    }
    try {
        HMODULE avsdll = LoadLibrary(TEXT("avisynth.dll"));
        if (!avsdll) {
            std::cerr << "Couldn't find avisynth.dll!" << std::endl;
            return -1;
        }
		IScriptEnvironment* (__stdcall *CreateEnv)(int);
		IScriptEnvironment *env;

		CreateEnv = (IScriptEnvironment *(__stdcall *)(int))GetProcAddress(avsdll, 
                                 "CreateScriptEnvironment"); 

        //IScriptEnvironment* (* CreateScriptEnvironment)(int version)
        //              = (IScriptEnvironment*(*)(int)) GetProcAddress(avsdll, "CreateScriptEnvironment");
		if (!CreateEnv) {
        //if (!CreateScriptEnvironment) {
            std::cerr << "Couldn't create script environment!" << std::endl;
            return -1;
        }

        //IScriptEnvironment* env = CreateScriptEnvironment(AVISYNTH_INTERFACE_VERSION);
		env = CreateEnv(AVISYNTH_INTERFACE_VERSION);
        if (!env) {
            std::cerr << "Couldn't create IScriptEnvironment!" << std::endl;
            return -1;
        }

        AVSValue arg(infile);
        AVSValue res = env->Invoke("Import", AVSValue(&arg, 1));
        if (!res.IsClip()) {
            std::cerr << "Found no valid avisynth clip!" << std::endl;
            return -1;
        }

        PClip clip = res.AsClip();
        
        VideoInfo inf = clip->GetVideoInfo();
        
        if (!inf.HasVideo()) {
            std::cerr << "Found no video info in current script!" << std::endl;
            return -1;
        }

		std::cerr << "<avsinfo>" << std::endl;
		std::cerr << "  <video>" << std::endl;

        std::cerr << "      <colorspace>";
                if (inf.IsYV12()) {
                  std::cerr << "Yv12";
                } else if (inf.IsRGB()) {
                  std::cerr << "RGB";
                } else if (inf.IsRGB24()) {
                  std::cerr << "RGB24";
                } else if (inf.IsRGB32()) {
                  std::cerr << "RGB32";
                } else if (inf.IsYUY2()) {
                  std::cerr << "YUY2";
                } else if (inf.IsYUV()) {
                  std::cerr << "YUV";
                } else {
                  std::cerr << "unknown";
                }
        std::cerr << "</colorspace>" << std::endl;

		std::cerr << "          <resolutionx>" << inf.width << "</resolutionx>" << std::endl;
		std::cerr << "          <resolutiony>" <<  inf.height << "</resolutiony>" << std::endl;

		std::cerr << "          <fps_num>" << inf.fps_numerator << "</fps_num>" << std::endl;
		std::cerr << "          <fps_denom>" << inf.fps_denominator << "</fps_denom>" << std::endl;

		std::cerr << "          <lengthf>" << inf.num_frames << "</lengthf>" << std::endl;
		
		std::cerr << "          <scan_type>";

        if(inf.IsBFF()){
            std::cerr << "BFF";
        } else if(inf.IsTFF()){
            std::cerr << "TFF";
        } else {
            std::cerr << "PRO";
        }

		std::cerr << "</scan_type>" << std::endl;
		std::cerr << "  </video>" << std::endl;

        if (inf.HasAudio()) {
            int sampleRate = inf.audio_samples_per_second;
            if (sampleRate != 0) {
				std::cerr << "  <audio>" << std::endl;
				std::cerr << "      <samplerate>" << sampleRate << "</samplerate>" << std::endl;
				std::cerr << "      <channelcount>" << inf.nchannels << "</channelcount>" << std::endl;
                std::cerr << "      <numsamples>" << inf.num_audio_samples << "</numsamples>" << std::endl;
				std::cerr << "  </audio>" << std::endl;
            }
        }
		std::cerr << "</avsinfo>" << std::endl;
    } catch(AvisynthError err) {
        std::cerr << "Avisynth error: " << err.msg << std::endl;
        return -1;
    }

    return 0;
}

bool WriteBitmap(LPTSTR szFile, HBITMAP hbitmap, HDC memdc) 
{ 
BITMAP  bmp; 
  if(GetObject(hbitmap, sizeof(BITMAP), &bmp)) 
  { 
    BITMAPINFOHEADER BmpInfoHdr;  //Struktur für Bitmap-Infoheader 
    BITMAPFILEHEADER BmpFileHdr;  //Struktur für Bitmap-Dateiheader 
    BmpInfoHdr.biSize = sizeof(BITMAPINFOHEADER); 
    BmpInfoHdr.biWidth = bmp.bmWidth; 
    BmpInfoHdr.biHeight = bmp.bmHeight; 
    BmpInfoHdr.biPlanes = bmp.bmPlanes; 
    BmpInfoHdr.biBitCount = 24; 
    BmpInfoHdr.biCompression    = BI_RGB; 
    BmpInfoHdr.biSizeImage        = bmp.bmWidth*bmp.bmHeight*3; 
    BmpFileHdr.bfType        = 0x4d42; 
    BmpFileHdr.bfReserved1        = 0; 
    BmpFileHdr.bfReserved2        = 0; 
    BmpFileHdr.bfOffBits = sizeof(BITMAPFILEHEADER)+sizeof(BITMAPINFOHEADER); 
    BmpFileHdr.bfSize = BmpFileHdr.bfOffBits+BmpInfoHdr.biSizeImage; 
  
    bmp.bmBits = (void*)GlobalAlloc(GMEM_FIXED, BmpInfoHdr.biSizeImage); 
  
    if(GetDIBits(memdc, hbitmap, 0, BmpInfoHdr.biHeight, bmp.bmBits, 
        (BITMAPINFO*)&BmpInfoHdr, DIB_RGB_COLORS) == BmpInfoHdr.biHeight) 
    { 
    HANDLE hFile = CreateFile(szFile, GENERIC_READ | GENERIC_WRITE, 0, NULL, CREATE_ALWAYS, FILE_ATTRIBUTE_NORMAL,NULL); 
      if(hFile != INVALID_HANDLE_VALUE)  { 
        DWORD dwTmp; 
        WriteFile(hFile, &BmpFileHdr, sizeof(BITMAPFILEHEADER), &dwTmp, NULL); 
        WriteFile(hFile, &BmpInfoHdr, sizeof(BITMAPINFOHEADER), &dwTmp, NULL); 
        WriteFile(hFile, bmp.bmBits,  BmpInfoHdr.biSizeImage,   &dwTmp, NULL); 
        } 
      CloseHandle(hFile); 
    } 
    GlobalFree(bmp.bmBits); 
    return TRUE; 
  } 
  return FALSE; 
}