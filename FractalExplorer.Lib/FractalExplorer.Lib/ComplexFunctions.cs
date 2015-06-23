using System.Numerics;

namespace FractalExplorer.Lib
{
    public class ComplexFunctions
    {
        private ComplexFunctions()
        {
        }

        private static object existsLockObject = new object();

        private static object createLockObject = new object();

        private static ComplexFunctions singletonInstance;

        public static ComplexFunctions GetSingletonInstance()
        {
            lock (existsLockObject)
            {
                if (singletonInstance == null)
                {
                    lock (createLockObject)
                    {
                        singletonInstance = new ComplexFunctions();
                    }
                }
            }
            return singletonInstance;
        }

        public delegate Complex ComplexFn(Complex c);

        public Complex Pow(Complex c, Complex pow)
        {
            return Complex.Pow(c, pow);
        }

        public Complex Pow(Complex c, Complex pow, ComplexFn func)
        {
            if (func == null)
                return Pow(c, pow);

            Complex resultOfFunc = func.Invoke(c);
            return Pow(resultOfFunc, pow);
        }

        public Complex RealPow(Complex c, double realPower)
        {
            return Pow(c, new Complex(realPower, 1));
        }

        public Complex ImaginaryPow(Complex c, double imaginaryPower)
        {
            return Pow(c, new Complex(imaginaryPower, 1));
        }

        public Complex Exp(Complex c)
        {
            return Complex.Exp(c);
        }

        public Complex Exp(Complex c, ComplexFn func)
        {
            if (func == null)
                return Exp(c);

            Complex resultOfFunc = func.Invoke(c);
            return Exp(resultOfFunc);
        }
    }
}