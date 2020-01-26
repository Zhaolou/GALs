using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;
using System.Diagnostics;
using System.Runtime.InteropServices;
namespace GALs
{
    public class ScatteringProcessing
    {
        [DllImport("GPUMieScatteringDLL.dll", EntryPoint = "GPUDeInitialization", CallingConvention = CallingConvention.Cdecl)]
        public static extern int GPUDeInitialization();

        [DllImport("GPUMieScatteringDLL.dll", EntryPoint = "GPUInitialization", CallingConvention = CallingConvention.Cdecl)]
        public static extern int GPUInitialization(int N, int nmax, float[] u, float[] abr, float[] abi);

        [DllImport("GPUMieScatteringDLL.dll", EntryPoint = "Mie_S12", CallingConvention = CallingConvention.Cdecl)]
        public static extern int GPUMie_S12(float mr, float mi, float x, int N, int nmax, float[] s1r, float[] s1i, float[] s2r, float[] s2i);


        //[DllImport("GPUMieScatteringDLL.dll", EntryPoint = "MieScatteringSuperposition", CallingConvention = CallingConvention.Cdecl)]
        //public static extern int MieScatteringSuperposition(int integrationStepNumber,
        //    int lightsheetScatteringAngleNumber, float[] lightsheetScatteringAngle, float[] lightsheetScatteringAmplitudeReal, float[] lightsheetScatteringAmplitudeImage, // results of light sheet scattering								//
        //    int planewaveScatteringAngleNumber, float[] planewaveScatteringAngle, float[] planewaveScatteringAmplitudeReal, float[] planewaveScatteringAmplitudeImage,  //results of plane wave scattering
        //    int spectrumSampleNumber, float[] planewaveSpectrumAngle, float[] planewaveSpectrumReal, float[] planewaveSpectrumImage);		//plane wave spectrum of a light sheet


        [DllImport("GPUMieScatteringDLL.dll", EntryPoint = "GetProcessingTime", CallingConvention = CallingConvention.Cdecl)]
        public static extern float GetProcessingTime();
        private int lightSheetType = 0;              // = 0, Gaussian; = 1, Airy; = 2, Customized.
        public int LightSheetType 
        {
            get { return lightSheetType; }
            set { lightSheetType = value; }
        }
        public Complex[] scatteringLightAmplitude;
        public double wavelength = 0.5e-6;
        public double deg2rad(double a)
        {
            return a * Math.PI / 180.0;
        }
        public class GaussianLightSheet
        {
            public double wavelength = 0.5e-6;
            public double waistRadius = 1e-6;
            public Complex GetAmplitudeAtFrequency(double nx)
            {
                double k0 = 2 * Math.PI / wavelength;
                double real = k0 * waistRadius / Math.Sqrt(Math.PI) * Math.Exp(-waistRadius * waistRadius * k0 * k0 * nx * nx);
                return new Complex(real, 0);
            }
        }

        public class AiryLightSheet
        {
            public double wavelength = 0.5e-6;
            public double xr = 1e-6;
            public double a0 = 0.1;
            public Complex GetAmplitudeAtFrequency(double nx)
            {
                double k0 = 2 * Math.PI / wavelength;
                double temp = 0.5 * Math.Exp(a0 * a0 * a0 / 3 - a0 * xr * xr * k0 * k0 * nx * nx);
                double real = temp * Math.Cos(Math.Pow(xr * k0 * nx, 3) / 3.0 - a0 * a0 * xr * k0 * nx);
                double image = temp * Math.Sin(Math.Pow(xr * k0 * nx, 3) / 3.0 - a0 * a0 * xr * k0 * nx);
                return new Complex(real, image);
            }        
        }

        public class CustomizedLightSheet
        {
            public double wavelength = 0.5e-6;
            public List<PlaneWaveSpectrum> planeWaveSpectrum = new List<PlaneWaveSpectrum>();
            public Complex Interp(double nx)
            { 
                if(planeWaveSpectrum.Count == 0)
                    return new Complex(0,0);

                if (nx <= planeWaveSpectrum[0].nx)
                    return new Complex(planeWaveSpectrum[0].real, planeWaveSpectrum[0].image);
                if (nx >= planeWaveSpectrum[planeWaveSpectrum.Count-1].nx)
                    return new Complex(planeWaveSpectrum[planeWaveSpectrum.Count - 1].real, planeWaveSpectrum[planeWaveSpectrum.Count-1].image);

                for (int i = 1; i < planeWaveSpectrum.Count; i++)
                {
                    if (nx < planeWaveSpectrum[i].nx)
                    {
                        double r0 = planeWaveSpectrum[i - 1].real;
                        double r1 = planeWaveSpectrum[i].real;
                        double i0 = planeWaveSpectrum[i - 1].image;
                        double i1 = planeWaveSpectrum[i].image;
                        double n0 = planeWaveSpectrum[i - 1].nx;
                        double n1 = planeWaveSpectrum[i].nx;
                        double real = (nx - n0) / (n1 - n0) * (r1 - r0) + r0;
                        double image = (nx - n0) / (n1 - n0) * (i1 - i0) + i0;
                        return new Complex(real, image);  
                    }
                }
                return new Complex(0, 0);
            }
            public Complex GetAmplitudeAtFrequency(double nx)
            {
                return Interp(nx);
            }   
        }
        public GaussianLightSheet gl = new GaussianLightSheet();
        public AiryLightSheet al = new AiryLightSheet();
        public CustomizedLightSheet cl = new CustomizedLightSheet();
        public Complex GetAmplitudeAtFrequency(double nx, int lightSheetType)
        {
            Complex c;
            double kx, kz;
            if (lightSheetType == 0)
            {
                kx = 2*Math.PI/gl.wavelength*nx;
                kz = 2*Math.PI/gl.wavelength*Math.Sqrt(1-nx*nx);
                c = gl.GetAmplitudeAtFrequency(nx);
                c = new Complex(Math.Cos(kx*sphereParameter.xPos + kz*sphereParameter.yPos),Math.Sin(kx*sphereParameter.xPos + kz*sphereParameter.yPos)) * c;
                return c;
            }
            else if (lightSheetType == 1)
            {
                kx = 2*Math.PI/al.wavelength*nx;
                kz = 2*Math.PI/al.wavelength*Math.Sqrt(1-nx*nx);
                c = al.GetAmplitudeAtFrequency(nx);
                c = new Complex(Math.Cos(kx*sphereParameter.xPos + kz*sphereParameter.yPos),Math.Sin(kx*sphereParameter.xPos + kz*sphereParameter.yPos)) * c;
                return c;
            }
            else           
            {
                kx = 2*Math.PI/cl.wavelength*nx;
                kz = 2*Math.PI/cl.wavelength*Math.Sqrt(1-nx*nx);
                c = cl.GetAmplitudeAtFrequency(nx);
                c = new Complex(Math.Cos(kx*sphereParameter.xPos + kz*sphereParameter.yPos),Math.Sin(kx*sphereParameter.xPos + kz*sphereParameter.yPos)) * c;
                return c;
            }
        }
        public class PlaneWaveSpectrum
        {
            public double nx;
            public double real;
            public double image;
            public PlaneWaveSpectrum(double n, double r, double i)
            {
                nx = n; real = r; image = i;
            }
        }
        public class CalculationParameter
        {
            public int seriesThreshold = 0;
            public int processingUnitType = 0;      // = 0, GPU; = 1, CPU;
            public List<double> scatteringAngle = new List<double>();
            public List<Complex> lightsheetScatteringAmplitude = new List<Complex>();
            public int angleStep = 10001;
            public float[] planewaveScatteringAngle;
            public Complex[] planewaveScatteringAmplitudeThetaPolarized;
            public Complex[] planewaveScatteringAmplitudePhiPolarized;
            public double[] spectrumNx = new double[2001];
            public double[] spectrumMag = new double[2001];
            public double[] spectrumReal = new double[2001];
            public double[] spectrumImage = new double[2001];
            public double[] spectrumPhase = new double[2001];




        }
        public CalculationParameter calculationParameter = new CalculationParameter();
        public class SphereParameter
        {
            public double radius = 50e-6;
            public double refractiveIndex = 1.5;
            public double refractiveIndexImage = 0;
            public double xPos = 0;
            public double yPos = 0;
            public void Copy(double ra, double rir, double rii, double xp, double yp)
            {
                radius = ra; refractiveIndex = rir;
                refractiveIndexImage = rii;
                xPos = xp; yPos = yp;
            }
        }
        public SphereParameter sphereParameter = new SphereParameter();
        public void ReadFile(string fileName)
        {
            cl.planeWaveSpectrum.Clear();
            FileStream fs = new FileStream(fileName, FileMode.Open);
            StreamReader sr = new StreamReader(fs);
            string line;
            double nx, real, image;
            try
            {
                while ((line = sr.ReadLine()) != null)
                {
                    string[] str = line.Split(new char[2] { ' ', '\t' }, options: StringSplitOptions.RemoveEmptyEntries);
                    if (str.Length < 3) 
                        break;
                    nx = Convert.ToDouble(str[0]);
                    real = Convert.ToDouble(str[1]);
                    image = Convert.ToDouble(str[2]);
                    PlaneWaveSpectrum pw = new PlaneWaveSpectrum(nx, real, image);
                    cl.planeWaveSpectrum.Add(pw);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            sr.Close();
            fs.Close();   
        }
        public Complex GetPlanewaveScatteringAmplitude(double angle)
        {
            double wrappedAngle = angle;
            double real = 0, image = 0;
            double r0, i0, r1, i1, a0, a1;
            int sign = 1;
            if(angle < 0) 
            {
                wrappedAngle = Convert.ToSingle(-angle);
            }
            if(angle > Math.PI)
            {
                wrappedAngle = Convert.ToSingle(2*Math.PI - angle);
            }
            int i = Convert.ToInt32(wrappedAngle / (Math.PI) * (calculationParameter.angleStep-1));
            if (i == calculationParameter.angleStep - 1) 
                i = calculationParameter.angleStep - 2;

            double cosWrappedAngle = Math.Cos(wrappedAngle);
            r0 = calculationParameter.planewaveScatteringAmplitudeThetaPolarized[i].real;
            r1 = calculationParameter.planewaveScatteringAmplitudeThetaPolarized[i+1].real;
            i0 = calculationParameter.planewaveScatteringAmplitudeThetaPolarized[i].image;
            i1 = calculationParameter.planewaveScatteringAmplitudeThetaPolarized[i+1].image;
            a0 = i * Math.PI / (calculationParameter.angleStep - 1);
            a1 = (i+1) * Math.PI / (calculationParameter.angleStep - 1);
            real = ((r1-r0)/(a1-a0)*(wrappedAngle - a0) + r0);
            image = ((i1-i0)/(a1-a0)*(wrappedAngle - a0) + i0);

            return new Complex(real,image);
        }
        public void Superposition()
        { 
	        int i, n;
	        double fplanewaveAngle;
            double flightsheetAngle;
            Complex fplanewaveAmplitude;
            Complex fplanewaveScatteringAmplitude;
            double fplanewaveScatteringAngle;
            double scaleFactor;
            for (n = 0; n < calculationParameter.scatteringAngle.Count; n++)
            { 
                calculationParameter.lightsheetScatteringAmplitude[n] = new Complex(0, 0);
                flightsheetAngle = calculationParameter.scatteringAngle[n];
		        for( i = 0; i < calculationParameter.angleStep; i++)				// Every plane wave contributes to the 
		        {
			        fplanewaveAngle = (i - (calculationParameter.angleStep - 1)/2.0) * Math.PI/(calculationParameter.angleStep - 1);
			        scaleFactor =  Math.Cos(fplanewaveAngle) * Math.Cos(fplanewaveAngle);
			        fplanewaveAmplitude = GetAmplitudeAtFrequency(Math.Sin(fplanewaveAngle), lightSheetType);
                    if (fplanewaveAmplitude.Mod() < 1e-3)
                        continue;
                    fplanewaveScatteringAngle = flightsheetAngle - fplanewaveAngle;
                    scaleFactor = Math.Cos(fplanewaveAngle) * Math.Cos(fplanewaveAngle);
                    fplanewaveScatteringAmplitude = GetPlanewaveScatteringAmplitude(fplanewaveScatteringAngle);



			        calculationParameter.lightsheetScatteringAmplitude[n] = calculationParameter.lightsheetScatteringAmplitude[n] 
                        + fplanewaveAmplitude * fplanewaveScatteringAmplitude * scaleFactor * (Math.PI/(calculationParameter.angleStep - 1.0));
		        }
	        }      
        }
        public void RunSimulation(int type, int superpositionType = 0)
        {

            if (type == 0)
                GPUSimulation();
            else
                CPUSimulation();
            Stopwatch sw = new Stopwatch();
            sw.Start();
                Superposition();
                sw.Stop();
        }
        public Complex[,] CPUMieABCD(Complex m, double x, int nmax)
        {
            Complex[,] abcd = new Complex[4, nmax];
            double[] n = new double[nmax];
            double[] nu = new double[nmax];

            Complex[] bx = new Complex[nmax];
            Complex[] bz = new Complex[nmax];
            Complex[] yx = new Complex[nmax];
            Complex[] hx = new Complex[nmax];

            Complex[] ax = new Complex[nmax];
            Complex[] az = new Complex[nmax];
            Complex[] ahx = new Complex[nmax];


            Complex z = m * new Complex(x, 0);
            Complex m2 = m * m;
            double sqx = Math.Sqrt(0.5 * Math.PI / x);
            Complex sqz = Complex.Pow(new Complex(0.5 * Math.PI, 0) / z, 0.5);
            double bxp05 = Math.Sin(x) * Math.Sqrt(2 / Math.PI / x);
            double bxm05 = Math.Cos(x) * Math.Sqrt(2 / Math.PI / x);
            double yxp05 = bxp05;
            double yxm05 = bxm05;
            Complex bzp05 = Complex.Sin(z) * Complex.Pow(new Complex(2 / Math.PI, 0) / z, 0.5);
            Complex bzm05 = Complex.Cos(z) * Complex.Pow(new Complex(2 / Math.PI, 0) / z, 0.5);
            double dt;
            Complex ct;
            for (int i = 0; i < nmax; i++)
            {
                dt = 2.0 / x * (i + 0.5) * bxp05 - bxm05;
                bxm05 = bxp05;
                bxp05 = dt;
                bx[i] = new Complex(bxp05 * sqx,0);
                ct = new Complex(2*i + 1.0, 0) / z * bzp05 - bzm05;
                bzm05 = bzp05;
                bzp05 = ct;
                bz[i] = bzp05 * sqz;
                dt = 2.0 / x * (-i - 0.5) * yxm05 - yxp05;
                yxp05 = yxm05;
                yxm05 = dt;
                yx[i] = new Complex((bxp05 * Math.Cos((i + 1.5)*Math.PI)-yxm05)/Math.Sin((i+1.5) * Math.PI)*sqx,0);
                hx[i] = bx[i] + new Complex(0, 1) * yx[i];
            }

            ax[0] = new Complex(Math.Sin(x),0) - bx[0];
            az[0] = Complex.Sin(z) - bz[0];
            ahx[0] = new Complex(Math.Sin(x), -Math.Cos(x)) - hx[0];
            for (int i = 1; i < nmax; i++)
            {
                ax[i] = new Complex(x, 0) * bx[i - 1] - new Complex((i + 1), 0) * bx[i];
                az[i] = z * bz[i - 1] - new Complex((i + 1), 0) * bz[i];
                ahx[i] = new Complex(x, 0) * hx[i - 1] - new Complex((i + 1), 0) * hx[i];
            }
            for (int i = 0; i < nmax; i++)
            {
                abcd[0, i] = (m2 * bz[i] * ax[i] - bx[i] * az[i]) / (m2 * bz[i] * ahx[i] - hx[i] * az[i]);
                abcd[1, i] = (bz[i] * ax[i] - bx[i] * az[i]) / (bz[i] * ahx[i] - hx[i] * az[i]);
                abcd[2, i] = (bx[i] * ahx[i] - hx[i] * ax[i]) / (bz[i] * ahx[i] - hx[i] * az[i]);
                abcd[3, i] = m * (bx[i] * ahx[i] - hx[i] * ax[i]) / (m2 * bz[i] * ahx[i] - hx[i] * az[i]);         
            }
            return abcd;
        }

        public double[,] CPUMiePT(double u, int nmax)
        {
            double[] p = new double[nmax];
            double[] t = new double[nmax];
            double [,] r = new double[2,nmax];
            p[0] = 1; t[0] = u;
            p[1] = 3 * u; t[1] = 3 * Math.Cos(2 * Math.Acos(u));
            double p1, p2, t1, t2;
            for (int n1 = 3; n1 <= nmax; n1++)
            {
                p1 = (2 * n1 - 1.0) / (n1 - 1) * p[n1 - 2] * u;
                p2 = n1 * 1.0 / (n1 - 1) * p[n1 - 3];
                p[n1 - 1] = p1 - p2;
                t1 = n1 * u * p[n1 - 1];
                t2 = (n1 + 1) * p[n1 - 2];
                t[n1 - 1] = t1 - t2;
            }
            for (int n1 = 0; n1 < nmax; n1++)
            {
                r[0, n1] = p[n1];
                r[1, n1] = t[n1];
            }
            return r;
        }

        public Complex[] CPUMieS12(Complex m, double x, double cosTheta, int nmax, Complex[,] ab)
        {
            double[,] pt = CPUMiePT(cosTheta, nmax);
            Complex[]s = new Complex[2];
            s[0] = new Complex(); s[1] = new Complex();
            double n2;
            for (int n = 1; n <= nmax; n++)
            {
                n2 = (2 * n + 1.0) / (n * (n + 1));
                s[0] = s[0] + (ab[0, n - 1] * pt[0, n - 1] + ab[1, n - 1] * pt[1, n - 1]) * n2;
                s[1] = s[1] + (ab[0, n - 1] * pt[1, n - 1] + ab[1, n - 1] * pt[0, n - 1]) * n2; 
            }
            return s;
        }
        public void SaveAB(Complex[,] ab, int nmax)
        {
            FileStream fs = new FileStream("D:\\ff.txt", FileMode.OpenOrCreate);
            StreamWriter sw = new StreamWriter(fs);
            for (int i = 0; i < nmax; i++)
            { 
                sw.WriteLine(ab[0,i].real.ToString() + " " + ab[0,i].image.ToString() + " "
                    + ab[1,i].real.ToString() + " " + ab[1,i].image.ToString());
            }
            sw.Close();
            fs.Close();
        }
        public void CPUInitialization()
        {
            Stopwatch sw = new Stopwatch();         
            calculationParameter.planewaveScatteringAngle = new float[calculationParameter.angleStep];
            calculationParameter.planewaveScatteringAmplitudeThetaPolarized = new Complex[calculationParameter.angleStep];
            calculationParameter.planewaveScatteringAmplitudePhiPolarized = new Complex[calculationParameter.angleStep];
            Complex m; double x;
            m = new Complex(sphereParameter.refractiveIndex, sphereParameter.refractiveIndexImage);
            
            
            
            x = 2 * Math.PI / wavelength * sphereParameter.radius;
            int nmax = (int)(calculationParameter.seriesThreshold == 0 ? Math.Round(2 + x + 4 * Math.Pow(x, 1.0 / 3)) : calculationParameter.seriesThreshold);

            Complex[,] ab = CPUMieABCD(m, x, nmax);

            SaveAB(ab, nmax);

            sw.Start(); 
            for(int j = 0; j < 10; j++)
            for (int i = 0; i < calculationParameter.angleStep; i++)
            {
                calculationParameter.planewaveScatteringAngle[i] = Convert.ToSingle(Math.Cos(deg2rad(180.0 / (calculationParameter.angleStep - 1) * i)));
                Complex[] c = CPUMieS12(m, x, calculationParameter.planewaveScatteringAngle[i],nmax,ab);
                calculationParameter.planewaveScatteringAmplitudeThetaPolarized[i] = c[1];
                calculationParameter.planewaveScatteringAmplitudePhiPolarized[i] = new Complex(0,0);                            //For future extension to out-of-E plane
            }
            sw.Stop();
            MessageBox.Show(sw.ElapsedMilliseconds.ToString());
        }
        public void CPUSimulation()
        {
            CPUInitialization();
            Output();
//            CPUIntegration();
        }
        public void Output()
        {
            FileStream fs = new FileStream("output.txt", FileMode.Create);
            StreamWriter swr = new StreamWriter(fs);
            for (int i = 0; i < calculationParameter.angleStep; i++)
            {
                swr.WriteLine((10 * Math.Log10(Math.Pow(calculationParameter.planewaveScatteringAmplitudeThetaPolarized[i].Mod(), 2) / Math.PI)).ToString());
            }
            swr.Close();
            fs.Close();        
        }
        public void GPUSimulation()
        {
            Stopwatch sw = new Stopwatch();
            calculationParameter.planewaveScatteringAngle = new float[calculationParameter.angleStep];
            calculationParameter.planewaveScatteringAmplitudeThetaPolarized = new Complex[calculationParameter.angleStep];
            calculationParameter.planewaveScatteringAmplitudePhiPolarized = new Complex[calculationParameter.angleStep];
            Complex m; double x;
            m = new Complex(sphereParameter.refractiveIndex, sphereParameter.refractiveIndexImage); x = 2 * Math.PI / wavelength * sphereParameter.radius;
            int nmax = (int)(calculationParameter.seriesThreshold == 0 ? Math.Round(2 + x + 4 * Math.Pow(x, 1.0 / 3)) : calculationParameter.seriesThreshold);
            for (int i = 0; i < calculationParameter.angleStep; i++)
            {
                calculationParameter.planewaveScatteringAngle[i] = Convert.ToSingle(Math.Cos(deg2rad(180.0 / (calculationParameter.angleStep - 1) * i)));
            }

            Complex[,] ab = CPUMieABCD(m, x, nmax);
            float[] abr = new float[2 * nmax];
            float[] abi = new float[2 * nmax];
            for (int i = 0; i < nmax; i++)
            {
                abr[i] = Convert.ToSingle(ab[0, i].real);
                abr[i + nmax] = Convert.ToSingle(ab[1, i].real);
                abi[i] = Convert.ToSingle(ab[0, i].image);
                abi[i + nmax] = Convert.ToSingle(ab[1, i].image);
            }
            int dev = -2;
            float[] s1r = new float[calculationParameter.angleStep];
            float[] s2r = new float[calculationParameter.angleStep];
            float[] s1i = new float[calculationParameter.angleStep];
            float[] s2i = new float[calculationParameter.angleStep];
            s1r[0] = 1;
            if ((dev = GPUInitialization(calculationParameter.angleStep, nmax,calculationParameter.planewaveScatteringAngle,abr, abi))  < 0)
            {
                MessageBox.Show("No available CUDA device can be found!");
            }
            GPUMie_S12(Convert.ToSingle(m.real), Convert.ToSingle(m.image), Convert.ToSingle(x), calculationParameter.angleStep, nmax, s1r, s1i, s2r, s2i);
            float processingTime = GetProcessingTime();
            for (int i = 0; i < calculationParameter.angleStep; i++)
            {
                calculationParameter.planewaveScatteringAmplitudeThetaPolarized[i] = new Complex(s2r[i],s2i[i]);
                calculationParameter.planewaveScatteringAmplitudePhiPolarized[i] = new Complex(0, 0);                            //For future extension to out-of-E plane
            }
            GPUDeInitialization();
            MessageBox.Show("Processing time is " + processingTime.ToString("f2"));
            Output();
        }
        public bool GPUInitialization()
        {
            return true;
        }

    }
}
