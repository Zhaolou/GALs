//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//namespace GALs
//{

//    public class DecimalComplex
//    {
//        public decimal real;
//        public decimal image;
//        public decimal Real
//        {
//            get { return real; }
//            set { real = value; }
//        }
//        public decimal Image
//        {
//            get { return image; }
//            set { image = value; }
//        }
//        public DecimalComplex(decimal real, decimal image)
//        {
//            this.real = real;
//            this.image = image;
//        }
//        public DecimalComplex()
//        {
//            real = 0;
//            image = 0;
//        }
//        public DecimalComplex Conjugate()
//        {
//            return new DecimalComplex(this.real, -this.image);
//        }
//        public static DecimalComplex operator +(DecimalComplex C, DecimalComplex c)
//        {
//            return new DecimalComplex(c.real + C.real, C.image + c.image);
//        }
//        public DecimalComplex Add(params DecimalComplex[] complexs)
//        {
//            if (complexs.Length == 0)
//            {
//                throw new Exception("The input parameter cannot be empty！");
//            }
//            DecimalComplex com = new DecimalComplex();
//            foreach (DecimalComplex c in complexs)
//            {
//                com = com + c;
//            }
//            return com;
//        }

//        public static DecimalComplex operator -(DecimalComplex C, DecimalComplex c)
//        {
//            return new DecimalComplex(C.real - c.real, C.image - c.Image);
//        }
//        public static bool operator ==(DecimalComplex C, DecimalComplex c)
//        {
//            return (C.real == c.real && C.image == c.image);
//        }
//        public static bool operator !=(DecimalComplex C, DecimalComplex c)
//        {
//            return (C.real != c.real || C.image != c.image);
//        }
//        public DecimalComplex Minus(params DecimalComplex[] complexs)
//        {
//            if (complexs.Length == 0)
//            {
//                throw new Exception("The input parameter cannot be empty！");
//            }
//            DecimalComplex com = complexs[0];
//            for (int i = 1; i < complexs.Length; i++)
//            {
//                com = com - complexs[i];
//            }
//            return com;
//        }
//        public static DecimalComplex operator *(DecimalComplex c, DecimalComplex C)
//        {
//            return new DecimalComplex(c.real * C.real - c.image * C.image, c.real * C.image + c.image * C.real);
//        }
//        public static DecimalComplex operator *(DecimalComplex c, decimal d)
//        {
//            return new DecimalComplex(c.real * d, c.image * d);
//        }
//        public static DecimalComplex operator *(decimal d, DecimalComplex c)
//        {
//            return new DecimalComplex(c.real * d, c.image * d);
//        }


//        public DecimalComplex Multiplicative(params DecimalComplex[] complexs)
//        {
//            if (complexs.Length == 0)
//            {
//                throw new Exception("The input parameter cannot be empty！");
//            }
//            DecimalComplex com = complexs[0];
//            for (int i = 1; i < complexs.Length; i++)
//            {
//                com += complexs[i];
//            }
//            return null;
//        }
//        public static DecimalComplex operator /(DecimalComplex C, DecimalComplex c)
//        {
//            if (c.real == 0 && c.image == 0)
//            {
//                throw new Exception("The divisor cannot be zero.");
//            }
//            decimal a, b, cc, d;
//            a = C.real; b = C.image; cc = c.real; d = c.image;

//            decimal real = (a * cc + b * d) / (cc * cc + d * d);
//            decimal image = (-a * d + b * cc) / (cc * cc + d * d);
//            return new DecimalComplex(real, image);
//        }

//        public static DecimalComplex Pow(DecimalComplex C, decimal a)
//        {
//            decimal mod, angle;
//            mod = C.Mod();
//            angle = C.GetAngle();
//            mod = Convert.ToDecimal(Math.Pow(Convert.ToDouble(mod), Convert.ToDouble(a)));
//            angle = angle * a;
//            return new DecimalComplex(mod * Convert.ToDecimal(Math.Cos(Convert.ToDouble(angle))), mod * Convert.ToDecimal(Math.Sin(Convert.ToDouble(angle))));
//        }
//        public static DecimalComplex Exp(DecimalComplex C)
//        {
//            decimal re, ia;
//            re = C.real;
//            ia = C.image;
//            return new Complex(Convert.ToDecimal(Math.Exp(Convert.ToDouble(re)) * Math.Cos(/cia), Math.Exp(re) * Math.Sin(ia));
//        }

//        public static Complex Sin(Complex C)
//        {
//            Complex a1, a2;
//            a1 = Complex.Exp(new Complex(0, 1) * C);
//            a2 = Complex.Exp(new Complex(0, -1) * C);
//            return (a1 - a2) / new Complex(0, 2);
//        }


//        public static Complex Cos(Complex C)
//        {
//            Complex a1, a2;
//            a1 = Complex.Exp(new Complex(0, 1) * C);
//            a2 = Complex.Exp(new Complex(0, -1) * C);
//            return (a1 + a2) / new Complex(2, 0);
//        }



//        public Complex Divison(params Complex[] complexs)
//        {
//            if (complexs.Length == 0)
//            {
//                throw new Exception("The input parameter cannot be empty！");
//            }
//            foreach (Complex com in complexs)
//            {
//                if (com.image == 0 && com.real == 0)
//                {
//                    throw new Exception("The input parameter cannot be empty！");
//                }
//            }
//            Complex COM = new Complex();
//            COM = complexs[0];
//            for (int i = 1; i < complexs.Length; i++)
//            {
//                COM = COM / complexs[i];
//            }
//            return COM;
//        }
//        public decimal Mod()
//        {
//            return Math.Sqrt(real * real + image * image);
//        }
//        public override bool Equals(object obj)
//        {
//            if (obj is Complex)
//            {
//                Complex com = (Complex)obj;
//                return (com.real == this.real && com.image == this.image);
//            }
//            return false;
//        }
//        public static decimal GetAngle(Complex c)
//        {
//            return Math.Atan2(c.image, c.real);
//        }
//        public decimal GetAngle()
//        {
//            return Math.Atan2(image, real);
//        }
//        public override string ToString()
//        {
//            return string.Format("<{0} , {1}>", this.real, this.image);
//        }
//    }

//}
