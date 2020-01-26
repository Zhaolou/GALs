// GPUMieScattering.cpp : 定义控制台应用程序的入口点。
//
#include "stdafx.h"
#include <cuda_runtime.h>
#include <helper_cuda.h>
#include <device_launch_parameters.h>
#include <helper_functions.h>
#include <device_functions.h>
#include <time.h>
#include "GPUMieScatteringDll.h"
#define PI 3.141592654



BOOL APIENTRY DllMain( HMODULE hModule,
                       DWORD  ul_reason_for_call,
                       LPVOID lpReserved
					 )
{
	switch (ul_reason_for_call)
	{
	case DLL_PROCESS_ATTACH:
	case DLL_THREAD_ATTACH:
	case DLL_THREAD_DETACH:
	case DLL_PROCESS_DETACH:
		break;
	}
	return TRUE;
}

__global__ void kernel(float mr, float mi, float x, float *u_data, int N, int nmax, float* abr_data, 
	float* abi_data,float* s1r_data, float* s1i_data, float* s2r_data, float* s2i_data, float* p, float *t)
{

    unsigned int tid = threadIdx.x + threadIdx.y * blockDim.x + blockIdx.x * blockDim.x * blockDim.y;
    float u = u_data[tid];
	if(tid < N)
	{
		p[0 + tid * nmax] = 1; t[0 + tid * nmax] = u;
		p[1 + tid * nmax] = 3 * u; t[1 + tid * nmax] = 3 * cos(2 * acos(u));
		float p1, p2, t1, t2;
		int n1;
		for (n1 = 3; n1 <= nmax; n1++)
		{
			p1 = (2 * n1 - 1.0) / (n1 - 1) * p[n1 - 2 + tid * nmax] * u;
			p2 = n1 * 1.0 / (n1 - 1) * p[n1 - 3 + tid * nmax];
			p[n1 - 1 + tid * nmax] = p1 - p2;
			t1 = n1 * u * p[n1 - 1 + tid * nmax];
			t2 = (n1 + 1) * p[n1 - 2 + tid * nmax];
			t[n1 - 1 + tid * nmax] = t1 - t2;
		}
		float n2;
		int n;
		s1r_data[tid] = 0; s1i_data[tid] = 0; s2r_data[tid] = 0; s2i_data[tid] = 0;
		for(n = 1; n <= nmax; n++)
		{
			n2 = (2 * n + 1.0) / (n * (n + 1));
			s1r_data[tid] = s1r_data[tid] + (abr_data[0*nmax + n - 1] * p[n - 1 + tid * nmax] + abr_data[1*nmax+n-1] * t[n-1 + tid * nmax])*n2;
			s1i_data[tid] = s1i_data[tid] + (abi_data[0*nmax + n - 1] * p[n - 1 + tid * nmax] + abi_data[1*nmax+n-1] * t[n-1 + tid * nmax])*n2;
	
			s2r_data[tid] = s2r_data[tid] + (abr_data[0*nmax + n - 1] * t[n - 1 + tid * nmax] + abr_data[1*nmax+n-1] * p[n-1 + tid * nmax])*n2;
			s2i_data[tid] = s2i_data[tid] + (abi_data[0*nmax + n - 1] * t[n - 1 + tid * nmax] + abi_data[1*nmax+n-1] * p[n-1 + tid * nmax])*n2;
		}
	}
	
}
//__global__ void ScatteringSuperposition(int integrationStepNumber, int lightsheetScatteringAngleNumber, float* lightsheetScatteringAngle_data, float*  lightsheetScatteringAmplitudeReal_data, float* lightsheetScatteringAmplitudeImage_data,
//		int planewaveScatteringAngleNumber,  float* planewaveScatteringAngle_data,  float* planewaveScatteringAmplitudeReal_data, float* planewaveScatteringAmplitudeImage_data,
//		int spectrumSampleNumber,  float* planewaveSpectrumAngle_data,  float* planewaveSpectrumReal_data, float* planewaveSpectrumImage_data)
//{
//
//}

float *u_data;
float *abr_data;
float *abi_data;
float *s1r_data;
float *s1i_data;
float *s2r_data;
float *s2i_data;
float *p_data;
float *t_data;
//float* lightsheetScatteringAngle_data;
//float *lightsheetScatteringAmplitudeReal_data;
//float *lightsheetScatteringAmplitudeImage_data; // results of light sheet scattering								//
//float* planewaveScatteringAngle_data;
//float *planewaveScatteringAmplitudeReal_data;
//float *planewaveScatteringAmplitudeImage_data;  //results of plane wave scattering
//float* planewaveSpectrumAngle_data;
//float *planewaveSpectrumReal_data;
//float *planewaveSpectrumImage_data;


cudaEvent_t start, stop;  
float processingTime;
extern "C" _declspec( dllexport ) int Mie_S12(float mr, float mi, float x, int N, int nmax, float* s1r, float *s1i, float *s2r, float *s2i)
{
	//FILE* f;
	//f = fopen("D:\\ff.txt", "w+");
	const unsigned int num_threads = N;
	cudaEventRecord(start, 0);
	
    dim3 grid(N/192+1, 1, 1);
    dim3 block(12, 16, 1);
	//fprintf(f,"gridx %d\n", N/192+1); 
	kernel<<< grid, block >>>(mr, mi, x, (float *) u_data, N, nmax, abr_data, abi_data,s1r_data, s1i_data, s2r_data, s2i_data, p_data, t_data);
	
	//fprintf(f,"After kernel\n");
	cudaEventRecord(stop, 0);  
	cudaEventSynchronize(stop);  
  
	cudaEventElapsedTime(&processingTime, start, stop);  

	
	//fprintf(f,"processingTime %f\n", processingTime);
	checkCudaErrors(cudaMemcpy(s1r, s1r_data, N*sizeof(float), cudaMemcpyDeviceToHost));
    checkCudaErrors(cudaMemcpy(s1i, s1i_data, N*sizeof(float), cudaMemcpyDeviceToHost));
    checkCudaErrors(cudaMemcpy(s2r, s2r_data, N*sizeof(float), cudaMemcpyDeviceToHost));
    checkCudaErrors(cudaMemcpy(s2i, s2i_data, N*sizeof(float), cudaMemcpyDeviceToHost));
	//fclose(f);
	return true;
}




extern "C" _declspec( dllexport ) int GPUDeInitialization()
{
	cudaEventDestroy(start);  
	cudaEventDestroy(stop);
	checkCudaErrors(cudaFree(s1r_data));
	checkCudaErrors(cudaFree(s1i_data));
	checkCudaErrors(cudaFree(s2r_data));
	checkCudaErrors(cudaFree(s2i_data));
	checkCudaErrors(cudaFree(u_data));
	checkCudaErrors(cudaFree(abr_data));
	checkCudaErrors(cudaFree(abi_data));
	checkCudaErrors(cudaFree(p_data));
	checkCudaErrors(cudaFree(t_data));

	//checkCudaErrors(cudaFree(lightsheetScatteringAngle_data));
 //   checkCudaErrors(cudaFree(lightsheetScatteringAmplitudeReal_data));
 //   checkCudaErrors(cudaFree(lightsheetScatteringAmplitudeImage_data));
 //   checkCudaErrors(cudaFree(planewaveScatteringAngle_data));
 //   checkCudaErrors(cudaFree(planewaveScatteringAmplitudeReal_data));
 //   checkCudaErrors(cudaFree(planewaveScatteringAmplitudeImage_data));
 //   checkCudaErrors(cudaFree(planewaveSpectrumAngle_data));
 //   checkCudaErrors(cudaFree(planewaveSpectrumReal_data));
 //   checkCudaErrors(cudaFree(planewaveSpectrumImage_data));
    
	
	
	return true;
}



extern "C" _declspec( dllexport ) int GPUInitialization(int N, int nmax, float *u, float *abr, float *abi)
{
	int devID = -5;
	devID = findCudaDevice(0, NULL);
	const unsigned int num_threads = N;
    checkCudaErrors(cudaMalloc((void **) &u_data, N*sizeof(float)));
    checkCudaErrors(cudaMalloc((void **) &abr_data, 4*nmax*sizeof(float)));
    checkCudaErrors(cudaMalloc((void **) &abi_data, 4*nmax*sizeof(float)));
    checkCudaErrors(cudaMalloc((void **) &s1r_data, N*sizeof(float)));
    checkCudaErrors(cudaMalloc((void **) &s1i_data, N*sizeof(float)));
    checkCudaErrors(cudaMalloc((void **) &s2r_data, N*sizeof(float)));
    checkCudaErrors(cudaMalloc((void **) &s2i_data, N*sizeof(float)));
    checkCudaErrors(cudaMalloc((void **) &p_data, N*nmax*sizeof(float)));
    checkCudaErrors(cudaMalloc((void **) &t_data, N*nmax*sizeof(float)));
    


	//checkCudaErrors(cudaMalloc((void **) &lightsheetScatteringAngle_data, 100000*sizeof(float)));
 //   checkCudaErrors(cudaMalloc((void **) &lightsheetScatteringAmplitudeReal_data, 100000*sizeof(float)));
 //   checkCudaErrors(cudaMalloc((void **) &lightsheetScatteringAmplitudeImage_data, 100000*sizeof(float)));
 //   checkCudaErrors(cudaMalloc((void **) &planewaveScatteringAngle_data, 100000*sizeof(float)));
 //   checkCudaErrors(cudaMalloc((void **) &planewaveScatteringAmplitudeReal_data, 100000*sizeof(float)));
 //   checkCudaErrors(cudaMalloc((void **) &planewaveScatteringAmplitudeImage_data, 100000*sizeof(float)));
 //   checkCudaErrors(cudaMalloc((void **) &planewaveSpectrumReal_data, 100000*sizeof(float)));
 //   checkCudaErrors(cudaMalloc((void **) &planewaveSpectrumImage_data, 100000*sizeof(float)));
 //   checkCudaErrors(cudaMalloc((void **) &planewaveSpectrumAngle_data, 100000*sizeof(float)));
    
	

	checkCudaErrors(cudaMemcpy(u_data, u, N*sizeof(float), cudaMemcpyHostToDevice));
    checkCudaErrors(cudaMemcpy(abr_data, abr, 4*nmax*sizeof(float), cudaMemcpyHostToDevice));
    checkCudaErrors(cudaMemcpy(abi_data, abi, 4*nmax*sizeof(float), cudaMemcpyHostToDevice));
	
	float time;  
	cudaEventCreate(&start);  
	cudaEventCreate(&stop); 
	return devID;
}

//extern "C" int MieScatteringSuperposition(int integrationStepNumber, 
//	int lightsheetScatteringAngleNumber, float* lightsheetScatteringAngle, float *lightsheetScatteringAmplitudeReal, float *lightsheetScatteringAmplitudeImage, // results of light sheet scattering								//
//	int planewaveScatteringAngleNumber, float* planewaveScatteringAngle, float *planewaveScatteringAmplitudeReal, float *planewaveScatteringAmplitudeImage,  //results of plane wave scattering
//	int spectrumSampleNumber, float* planewaveSpectrumAngle, float *planewaveSpectrumReal, float *planewaveSpectrumImage)			//plane wave spectrum of a light sheet
//{
//	const unsigned int num_threads = lightsheetScatteringAngleNumber;
//	cudaEventRecord(start, 0);
//
//
//	checkCudaErrors(cudaMemcpy(lightsheetScatteringAngle_data, lightsheetScatteringAngle, lightsheetScatteringAngleNumber*sizeof(float), cudaMemcpyHostToDevice));
//
//	checkCudaErrors(cudaMemcpy(planewaveScatteringAmplitudeReal_data, planewaveScatteringAmplitudeReal, planewaveScatteringAngleNumber*sizeof(float), cudaMemcpyHostToDevice));
//	checkCudaErrors(cudaMemcpy(planewaveScatteringAmplitudeImage_data, planewaveScatteringAmplitudeImage, planewaveScatteringAngleNumber*sizeof(float), cudaMemcpyHostToDevice));
//	checkCudaErrors(cudaMemcpy(planewaveScatteringAngle_data, planewaveScatteringAngle, planewaveScatteringAngleNumber*sizeof(float), cudaMemcpyHostToDevice));
//	checkCudaErrors(cudaMemcpy(planewaveSpectrumReal_data, planewaveSpectrumReal, spectrumSampleNumber*sizeof(float), cudaMemcpyHostToDevice));
//	checkCudaErrors(cudaMemcpy(planewaveSpectrumImage_data, planewaveSpectrumImage, spectrumSampleNumber*sizeof(float), cudaMemcpyHostToDevice));
//	checkCudaErrors(cudaMemcpy(planewaveSpectrumAngle_data, planewaveSpectrumAngle, spectrumSampleNumber*sizeof(float), cudaMemcpyHostToDevice));
//	
//
//
//
//    dim3 grid(num_threads/192+1, 1, 1);
//    dim3 block(12, 16, 1);
//	//fprintf(f,"gridx %d\n", N/192+1); 
//	ScatteringSuperposition<<< grid, block >>>(integrationStepNumber, lightsheetScatteringAngleNumber, lightsheetScatteringAngle_data, lightsheetScatteringAmplitudeReal_data, lightsheetScatteringAmplitudeImage_data,
//		planewaveScatteringAngleNumber, planewaveScatteringAngle_data, planewaveScatteringAmplitudeReal_data, planewaveScatteringAmplitudeImage_data,
//		spectrumSampleNumber, planewaveSpectrumAngle_data, planewaveSpectrumReal_data, planewaveSpectrumImage_data);
//	
//	//fprintf(f,"After kernel\n");
//	cudaEventRecord(stop, 0);  
//	cudaEventSynchronize(stop);  
//  
//	cudaEventElapsedTime(&processingTime, start, stop);  
//
//	
//	//fprintf(f,"processingTime %f\n", processingTime);
//	checkCudaErrors(cudaMemcpy(lightsheetScatteringAmplitudeReal, lightsheetScatteringAmplitudeReal_data, lightsheetScatteringAngleNumber*sizeof(float), cudaMemcpyDeviceToHost));
//	checkCudaErrors(cudaMemcpy(lightsheetScatteringAmplitudeImage, lightsheetScatteringAmplitudeImage_data, lightsheetScatteringAngleNumber*sizeof(float), cudaMemcpyDeviceToHost));
//
//	//fclose(f);
//	return true;
//	
//}

//extern "C" _declspec( dllexport ) int fadd2add(int a, int b)
//{
//	return a+b;
//}
extern "C" _declspec( dllexport ) float GetProcessingTime()
{
	return processingTime;
}