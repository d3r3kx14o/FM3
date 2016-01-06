using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LargeGraphLayout.Algorithms
{
    public class Complex
    {
        public double Real;
        public double Imaginary;
        public Complex(double real, double imaginary)
        {
            this.Real = real;
            this.Imaginary = imaginary;
        }

        public Complex()
        {
        }

        public Complex Add(Complex c)
        {
            Real = Real + c.Real;
            Imaginary = Imaginary + c.Imaginary;
            return this;
        }

        public static Complex Add(Complex a, Complex b)
        {
            return new Complex
            {
                Real = a.Real + b.Real,
                Imaginary = a.Imaginary + b.Imaginary
            };
        }

        public Complex Subtract(Complex c)
        {
            return new Complex
            {
                Real = Real - c.Real,
                Imaginary = Imaginary - c.Imaginary
            };
        }
        public static Complex Subtract(Complex a, Complex b)
        {
            return new Complex
            {
                Real = a.Real - b.Real,
                Imaginary = a.Imaginary - b.Imaginary
            };
        }

        public Complex Multiply(double r)
        {
            this.Real *= r;
            this.Imaginary *= r;
            return this;
        }

        public Complex Multiply(Complex c)
        {
            Real = Real*c.Real - Imaginary*c.Imaginary;
            Imaginary = Real*c.Imaginary + Imaginary*c.Real;
            return this;
        }

        public static Complex Multiply(Complex a, Complex b)
        {
            return new Complex
            {
                Real = a.Real * b.Real - a.Imaginary * b.Imaginary,
                Imaginary = a.Real * b.Imaginary + a.Imaginary * b.Real
            };
        }

        public Complex Divide(double r)
        {
            if (r.Equals(0))
                throw new ArithmeticException("0 as Denom");
            this.Real /= r;
            this.Imaginary /= r;
            return this;
        }

        public Complex Divide(Complex c)
        {
            var denom = c.NormSquare();
            if (denom.Equals(0))
                throw new ArithmeticException("0 as Denom");
            return new Complex
            {
                Real = (Real * c.Real + Imaginary * c.Imaginary) / denom,
                Imaginary = (Imaginary * c.Real - Real * c.Imaginary) / denom
            };
        }

        public static Complex Divide(Complex a, Complex b)
        {
            var denom = b.NormSquare();
            if (denom.Equals(0))
                throw new ArithmeticException("0 as Denom");
            return new Complex
            {
                Real = (a.Real * b.Real + a.Imaginary * b.Imaginary) / denom,
                Imaginary = (a.Imaginary * b.Real - a.Real * b.Imaginary) / denom
            };
        }

        public double Norm1()
        {
            if (Real.Equals(0)) return Math.Abs(Imaginary);
            
            if (Imaginary.Equals(0)) return 0;
            var ratio = Imaginary / Real;
            return Math.Sqrt(1 + ratio * ratio) * Real;
        }

        public double Norm()
        {
            double a = Real, b = Imaginary;
            if (Math.Abs(a) < Math.Abs(b))
            {
                var t = a;
                a = b;
                b = t;
            }
            if (b.Equals(0)) return Math.Abs(a);
            var ratio = b / a;
            var res = Math.Abs(a);
            if (!double.IsNaN(ratio))
                res *= Math.Sqrt(1 + ratio * ratio);
            return res;
        }

        public Complex Trim(double scale = 1000)
        {
            var norm = this.Norm();
            if (norm.Equals(0) || scale.Equals(0)) return this;
            double factor = 1000000000000;

            if (norm > factor * scale)
            {
                return new Complex
                {
                    Real = Real / norm / factor  * scale,
                    Imaginary = Imaginary / norm / factor * scale
                };
            }
            else
            {
                return new Complex
                {
                    Real = Real / factor ,
                    Imaginary = Imaginary / factor 
                };
            }
        }

        public Complex Normalize(double scale = 1)
        {
            var norm = this.Norm();
            if (norm.Equals(0) || scale.Equals(0)) return this;
            norm /= scale;
            return new Complex
            {
                Real = Real / norm,
                Imaginary = Imaginary / norm
            };
        }

        public Complex AddTo(Complex a)
        {
            a.Real += Real;
            a.Imaginary += Imaginary;
            return a;
        }

        public double NormSquare()
        {
            return Math.Pow(Real, 2) + Math.Pow(Imaginary, 2);
        }

        public Complex Log()
        {
            var norm = this.Norm();
            if (norm.Equals(0))
                throw new ArithmeticException("0 as Denom");
            return new Complex
            {
                Real = Math.Log(norm),
                Imaginary = Math.Atan2(this.Imaginary, this.Real)
            };
        }

        public Complex Clone()
        {
            return new Complex(Real, Imaginary);
        }

        
        public override string ToString()
        {
            return $"{Real}\t{Imaginary}";
        }
        //return Math.Sqrt(Real*Real + Imaginary*Imaginary);
    }
}