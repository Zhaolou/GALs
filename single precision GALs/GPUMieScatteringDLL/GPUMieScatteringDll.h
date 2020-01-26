#ifndef _GPUMIESCATTERINGDLL_H_  
#define _GPUMIESCATTERINGDLL_H_  
 
 
extern "C" _declspec( dllexport ) int GPUInitialization(int N, int nmax, float *u, float *abr, float *abi);
extern "C" _declspec( dllexport ) int GPUDeInitialization();
	
extern "C" _declspec( dllexport ) int Mie_S12(float mr, float mi, float x, int N, int nmax, float* s1r, float *s1i, float *s2r, float *s2i);
#endif  