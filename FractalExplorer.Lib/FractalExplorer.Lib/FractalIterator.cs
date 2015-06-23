using System.Drawing;
using System.Numerics;

namespace FractalExplorer.Lib
{
    public class FractalIterator
    {
        private FractalIterator()
        {
        }

        private static object existsLockObject = new object();

        private static object createLockObject = new object();

        private static FractalIterator singletonInstance;

        public static FractalIterator GetSingletonInstance()
        {
            lock (existsLockObject)
            {
                if (singletonInstance == null)
                {
                    lock (createLockObject)
                    {
                        singletonInstance = new FractalIterator();
                    }
                }
            }
            return singletonInstance;
        }

        public int IterateMandelbrotPoint(PointF point, int maxIterations, double bailout, ComplexFunctions.ComplexFn func)
        {
            return IterateMandelbrotPoint(new Complex(point.X, point.Y), maxIterations, bailout, func);
        }

        public int IterateMandelbrotPoint(Complex c, int maxIterations, double bailout, ComplexFunctions.ComplexFn func)
        {
            //Iterate a single point through a complex function, returning number of iterations (up to the max)
            //it takes to escape off beyond the bailout limit
            int iterations = 0;
            Complex Z = new Complex(0, 0);
            while (Complex.Abs(Z) < bailout && iterations < maxIterations)
            {
                Z = func(Z) + c;
                iterations++;
            }
            return iterations;
        }
    }
}