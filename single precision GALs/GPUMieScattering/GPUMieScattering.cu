// GPUMieScattering.cpp : 定义控制台应用程序的入口点。
//
#include "stdafx.h"
#include <cuda_runtime.h>
#include <helper_cuda.h>
#include <device_launch_parameters.h>
#include <helper_functions.h>
#include <device_functions.h>
#include <time.h>
__global__ void kernel2(int n, float *g_data)
{
	printf("threadID is %d", threadIdx.x);
    const unsigned int tid = threadIdx.x;
    int data = g_data[tid];
	if(tid < n)
    g_data[tid] = g_data[tid] + 1;
}


//__global__ void kernel(float mr, float mi, float x, float *u_data, int N)
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


//extern "C" int GPUInitialization()
//{
//	int devID = findCudaDevice(0, NULL);
//	return devID;
//	float mr, mi;				//real and image components of m
//	float x;
//	int N = 10001;
//	float *u = (float*)malloc(N*sizeof(float));
//	int nmax = 665;
//	float* abr = (float*)malloc(4*nmax*sizeof(float));
//	float* abi = (float*)malloc(4*nmax*sizeof(float));			//real and image components of ab
//	float *s1r, *s1i, *s2r, *s2i;	
//	s1r = (float*)malloc(N*sizeof(float));
//	s1i = (float*)malloc(N*sizeof(float));
//	
//	s2r = (float*)malloc(N*sizeof(float));
//	s2i = (float*)malloc(N*sizeof(float));
//	for(int i = 0; i < N; i++)
//	{
//		s2r[i] = i;
//		s2i[i] = -i;
//	}
//
//	const unsigned int num_threads = N;
//	 
//	float *u_data;
//	float *abr_data;
//	float *abi_data;
//	float *s1r_data;
//	float *s1i_data;
//	float *s2r_data;
//	float *s2i_data;
//	float *p_data;
//	float *t_data; 
//	FILE* file;
//	file = fopen("D:\\ff.txt", "r");
//	float a0, a1, a2, a3;
//	for(int i = 0; i < nmax; i++)
//	{
//		fscanf(file, "%lf %lf %lf %lf", &a0, &a1, &a2, &a3);
//		abr[i] = a0;
//		abr[nmax + i] = a2;
//		abi[i] = a1;
//		abi[nmax+i] = a3;
//	}
//
//	fclose(file);
//	mr = 1.5; mi = 0; x = 628.31853; 
//	for(int i =0; i < N; i++)
//		u[i] = cos(3.1415927/(N-1)*i);
//    checkCudaErrors(cudaMalloc((void **) &u_data, N*sizeof(float)));
//    checkCudaErrors(cudaMalloc((void **) &abr_data, 4*nmax*sizeof(float)));
//    checkCudaErrors(cudaMalloc((void **) &abi_data, 4*nmax*sizeof(float)));
//    checkCudaErrors(cudaMalloc((void **) &s1r_data, N*sizeof(float)));
//    checkCudaErrors(cudaMalloc((void **) &s1i_data, N*sizeof(float)));
//    checkCudaErrors(cudaMalloc((void **) &s2r_data, N*sizeof(float)));
//    checkCudaErrors(cudaMalloc((void **) &s2i_data, N*sizeof(float)));
//    checkCudaErrors(cudaMalloc((void **) &p_data, N*nmax*sizeof(float)));
//    checkCudaErrors(cudaMalloc((void **) &t_data, N*nmax*sizeof(float)));
//    // copy host memory to device
//    checkCudaErrors(cudaMemcpy(u_data, u, N*sizeof(float), cudaMemcpyHostToDevice));
//
//	cudaEvent_t start, stop;  
//	float time;  
//	cudaEventCreate(&start);  
//	cudaEventCreate(&stop); 
//	
//	cudaEventRecord(start, 0);
//
//    checkCudaErrors(cudaMemcpy(abr_data, abr, 4*nmax*sizeof(float), cudaMemcpyHostToDevice));
//    checkCudaErrors(cudaMemcpy(abi_data, abi, 4*nmax*sizeof(float), cudaMemcpyHostToDevice));
//    checkCudaErrors(cudaMemcpy(s2r_data, s2r, N*sizeof(float), cudaMemcpyHostToDevice));
//    checkCudaErrors(cudaMemcpy(s2i_data, s2i, N*sizeof(float), cudaMemcpyHostToDevice));
//
//    dim3 grid(N/192+1, 1, 1);
//    dim3 block(12, 16, 1);
//
//	//for(int i = 0; i < 20; i++)
//		kernel<<< grid, block >>>(mr, mi, x, (float *) u_data, N, nmax, abr_data, abi_data,s1r_data, s1i_data, s2r_data, s2i_data, p_data, t_data);
//
//	cudaEventRecord(stop, 0);  
//	cudaEventSynchronize(stop);  
//  
//	cudaEventElapsedTime(&time, start, stop);  
//	cudaEventDestroy(start);  
//	cudaEventDestroy(stop);
//
//	
//    checkCudaErrors(cudaMemcpy(u, u_data, N*sizeof(float),cudaMemcpyDeviceToHost));
//
//
//
//
//    getLastCudaError("Kernel execution failed");
//	
//    checkCudaErrors(cudaMemcpy(s1r, s1r_data, N*sizeof(float), cudaMemcpyDeviceToHost));
//    checkCudaErrors(cudaMemcpy(s1i, s1i_data, N*sizeof(float), cudaMemcpyDeviceToHost));
//    checkCudaErrors(cudaMemcpy(s2r, s2r_data, N*sizeof(float), cudaMemcpyDeviceToHost));
//    checkCudaErrors(cudaMemcpy(s2i, s2i_data, N*sizeof(float), cudaMemcpyDeviceToHost));
//	
//	checkCudaErrors(cudaFree(s1r_data));
//	checkCudaErrors(cudaFree(s1i_data));
//	checkCudaErrors(cudaFree(s2r_data));
//	checkCudaErrors(cudaFree(s2i_data));
//	checkCudaErrors(cudaFree(u_data));
//	checkCudaErrors(cudaFree(abr_data));
//	checkCudaErrors(cudaFree(abi_data));
//
//	free(u);
//    free(abr);
//    free(abi);
//    free(s1r);
//    free(s1i);
//    free(s2r);
//    free(s2i);
//	return true;
//}
//


extern "C" int Mie_S12()
{
	int devID = findCudaDevice(0, NULL);
	float mr, mi;				//real and image components of m
	float x;
	int N = 10001;
	float *u = (float*)malloc(N*sizeof(float));
	int nmax = 665;
	float* abr = (float*)malloc(4*nmax*sizeof(float));
	float* abi = (float*)malloc(4*nmax*sizeof(float));			//real and image components of ab
	float *s1r, *s1i, *s2r, *s2i;	
	s1r = (float*)malloc(N*sizeof(float));
	s1i = (float*)malloc(N*sizeof(float));
	
	s2r = (float*)malloc(N*sizeof(float));
	s2i = (float*)malloc(N*sizeof(float));
	for(int i = 0; i < N; i++)
	{
		s2r[i] = i;
		s2i[i] = -i;
	}

	const unsigned int num_threads = N;
	 
	float *u_data;
	float *abr_data;
	float *abi_data;
	float *s1r_data;
	float *s1i_data;
	float *s2r_data;
	float *s2i_data;
	float *p_data;
	float *t_data; 
	FILE* file;
	file = fopen("D:\\ff.txt", "r");
	float a0, a1, a2, a3;
	for(int i = 0; i < nmax; i++)
	{
		fscanf(file, "%lf %lf %lf %lf", &a0, &a1, &a2, &a3);
		abr[i] = a0;
		abr[nmax + i] = a2;
		abi[i] = a1;
		abi[nmax+i] = a3;
	}

	fclose(file);
	mr = 1.5; mi = 0; x = 628.31853; 
	for(int i =0; i < N; i++)
		u[i] = cos(3.1415927/(N-1)*i);
    checkCudaErrors(cudaMalloc((void **) &u_data, N*sizeof(float)));
    checkCudaErrors(cudaMalloc((void **) &abr_data, 4*nmax*sizeof(float)));
    checkCudaErrors(cudaMalloc((void **) &abi_data, 4*nmax*sizeof(float)));
    checkCudaErrors(cudaMalloc((void **) &s1r_data, N*sizeof(float)));
    checkCudaErrors(cudaMalloc((void **) &s1i_data, N*sizeof(float)));
    checkCudaErrors(cudaMalloc((void **) &s2r_data, N*sizeof(float)));
    checkCudaErrors(cudaMalloc((void **) &s2i_data, N*sizeof(float)));
    checkCudaErrors(cudaMalloc((void **) &p_data, N*nmax*sizeof(float)));
    checkCudaErrors(cudaMalloc((void **) &t_data, N*nmax*sizeof(float)));
    // copy host memory to device
    checkCudaErrors(cudaMemcpy(u_data, u, N*sizeof(float), cudaMemcpyHostToDevice));

	cudaEvent_t start, stop;  
	float time;  
	cudaEventCreate(&start);  
	cudaEventCreate(&stop); 
	
	cudaEventRecord(start, 0);

    checkCudaErrors(cudaMemcpy(abr_data, abr, 4*nmax*sizeof(float), cudaMemcpyHostToDevice));
    checkCudaErrors(cudaMemcpy(abi_data, abi, 4*nmax*sizeof(float), cudaMemcpyHostToDevice));
    checkCudaErrors(cudaMemcpy(s2r_data, s2r, N*sizeof(float), cudaMemcpyHostToDevice));
    checkCudaErrors(cudaMemcpy(s2i_data, s2i, N*sizeof(float), cudaMemcpyHostToDevice));

    dim3 grid(N/192+1, 1, 1);
    dim3 block(12, 16, 1);

	//for(int i = 0; i < 20; i++)
		kernel<<< grid, block >>>(mr, mi, x, (float *) u_data, N, nmax, abr_data, abi_data,s1r_data, s1i_data, s2r_data, s2i_data, p_data, t_data);

	cudaEventRecord(stop, 0);  
	cudaEventSynchronize(stop);  
  
	cudaEventElapsedTime(&time, start, stop);  
	cudaEventDestroy(start);  
	cudaEventDestroy(stop);

	
    checkCudaErrors(cudaMemcpy(u, u_data, N*sizeof(float),cudaMemcpyDeviceToHost));




    getLastCudaError("Kernel execution failed");
	
    checkCudaErrors(cudaMemcpy(s1r, s1r_data, N*sizeof(float), cudaMemcpyDeviceToHost));
    checkCudaErrors(cudaMemcpy(s1i, s1i_data, N*sizeof(float), cudaMemcpyDeviceToHost));
    checkCudaErrors(cudaMemcpy(s2r, s2r_data, N*sizeof(float), cudaMemcpyDeviceToHost));
    checkCudaErrors(cudaMemcpy(s2i, s2i_data, N*sizeof(float), cudaMemcpyDeviceToHost));
	
	checkCudaErrors(cudaFree(s1r_data));
	checkCudaErrors(cudaFree(s1i_data));
	checkCudaErrors(cudaFree(s2r_data));
	checkCudaErrors(cudaFree(s2i_data));
	checkCudaErrors(cudaFree(u_data));
	checkCudaErrors(cudaFree(abr_data));
	checkCudaErrors(cudaFree(abi_data));

	free(u);
    free(abr);
    free(abi);
    free(s1r);
    free(s1i);
    free(s2r);
    free(s2i);
	return true;
}

BOOL _tmain(int argc, _TCHAR* argv[])
{
	if(Mie_S12() < 0)
		return false;
//    CPUMieS12(m, x, Math.Cos(calculationParameter.planewaveScatteringAngle[i]),nmax,ab);
	return 0;
}

