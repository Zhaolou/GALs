using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
namespace GALs
{

    public class Complex
    {
        public double real;
        public double image;
        public double Real
        {
            get { return real; }
            set { real = value; }
        }
        public double Image
        {
            get { return image; }
            set { image = value; }
        }
        public Complex(double real, double image)
        {
            this.real = real;
            this.image = image;
        }
        public Complex()
        {
            real = 0;
            image = 0;
        }
        public Complex Conjugate()
        {
            return new Complex(this.real, -this.image);
        }
        public static Complex operator +(Complex C, Complex c)
        {
            return new Complex(c.real + C.real, C.image + c.image);
        }
        public Complex Add(params Complex[] complexs)
        {
            if (complexs.Length == 0)
            {
                throw new Exception("The input parameter cannot be empty！");
            }
            Complex com = new Complex();
            foreach (Complex c in complexs)
            {
                com = com + c;
            }
            return com;
        }

        public static Complex operator -(Complex C, Complex c)
        {
            return new Complex(C.real - c.real, C.image - c.Image);
        }
        public static bool operator ==(Complex C, Complex c)
        {
            return (C.real == c.real && C.image == c.image);
        }
        public static bool operator !=(Complex C, Complex c)
        {
            return (C.real != c.real || C.image != c.image);
        }
        public Complex Minus(params Complex[] complexs)
        {
            if (complexs.Length == 0)
            {
                throw new Exception("The input parameter cannot be empty！");
            }
            Complex com = complexs[0];
            for (int i = 1; i < complexs.Length; i++)
            {
                com = com - complexs[i];
            }
            return com;
        }
        public static Complex operator *(Complex c, Complex C)
        {
            return new Complex(c.real * C.real - c.image * C.image, c.real * C.image + c.image * C.real);
        }
        public static Complex operator *(Complex c, double d)
        {
            return new Complex(c.real * d,  c.image * d);
        }
        public static Complex operator *(double d, Complex c)
        {
            return new Complex(c.real * d, c.image * d);
        }


        public Complex Multiplicative(params Complex[] complexs)
        {
            if (complexs.Length == 0)
            {
                throw new Exception("The input parameter cannot be empty！");
            }
            Complex com = complexs[0];
            for (int i = 1; i < complexs.Length; i++)
            {
                com += complexs[i];
            }
            return null;
        }
        public static Complex operator /(Complex C, Complex c)
        {
            if (c.real == 0 && c.image == 0)
            {
                throw new Exception("The divisor cannot be zero.");
            }
            double a, b, cc, d;
            a = C.real; b = C.image; cc = c.real; d = c.image;

            double real = (a * cc + b * d) / (cc * cc + d * d);
            double image = (-a * d + b * cc) / (cc * cc + d * d);
            return new Complex(real, image);
        }

        public static Complex Pow(Complex C, double a)
        {
            double mod, angle;
            mod = C.Mod();
            angle = C.GetAngle();
            mod = Math.Pow(mod, a);
            angle = angle * a;
            return new Complex(mod * Math.Cos(angle), mod * Math.Sin(angle));
        }
        public static Complex Exp(Complex C)
        {
            double re,ia;
            re = C.real;
            ia = C.image;
            return new Complex(Math.Exp(re)*Math.Cos(ia), Math.Exp(re)*Math.Sin(ia));
        }

        public static Complex Sin(Complex C)
        {
            Complex a1, a2;
            a1 = Complex.Exp(new Complex(0, 1) * C);
            a2 = Complex.Exp(new Complex(0, -1) * C);
            return (a1 - a2) / new Complex(0, 2);
        }


        public static Complex Cos(Complex C)
        {
            Complex a1, a2;
            a1 = Complex.Exp(new Complex(0, 1) * C);
            a2 = Complex.Exp(new Complex(0, -1) * C);
            return (a1 + a2) / new Complex(2, 0);
        }



        public Complex Divison(params Complex[] complexs)
        {
            if (complexs.Length == 0)
            {
                throw new Exception("The input parameter cannot be empty！");
            }
            foreach (Complex com in complexs)
            {
                if (com.image == 0 && com.real == 0)
                {
                    throw new Exception("The input parameter cannot be empty！");
                }
            }
            Complex COM = new Complex();
            COM = complexs[0];
            for (int i = 1; i < complexs.Length; i++)
            {
                COM = COM / complexs[i];
            }
            return COM;
        }
        public double Mod()
        {
            return Math.Sqrt(real * real + image * image);
        }
        public override bool Equals(object obj)
        {
            if (obj is Complex)
            {
                Complex com = (Complex)obj;
                return (com.real == this.real && com.image == this.image);
            }
            return false;
        }
        public static double GetAngle(Complex c)
        {
            return Math.Atan2(c.image, c.real);
        }
        public double GetAngle()
        {
            return Math.Atan2(image, real);
        }
        public override string ToString()
        {
            return string.Format("<{0} , {1}>", this.real, this.image);
        }
    }

}
