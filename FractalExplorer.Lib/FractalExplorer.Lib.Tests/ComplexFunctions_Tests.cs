using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Numerics;

namespace FractalExplorer.Lib.Tests
{
    [TestClass]
    public class ComplexFunctions_Tests
    {
        [TestMethod]
        public void GetSingletonInstance()
        {
            ComplexFunctions cf = ComplexFunctions.GetSingletonInstance();
            Assert.IsNotNull(cf);
        }

        [TestMethod]
        public void Pow_Func()
        {
            ComplexFunctions cf = ComplexFunctions.GetSingletonInstance();

            Complex c = new Complex(50, 50);
            Complex pow = new Complex(10, 1);
            Complex expected = Complex.Pow(c, pow);
            Complex actual = cf.Pow(c, pow, (x) => x);
            Assert.AreEqual(expected, actual);

            expected = Complex.Pow(Complex.Exp(c), pow);
            actual = cf.Pow(c, pow, (x) => Complex.Exp(x));
            Assert.AreEqual(expected, actual);

            expected = Complex.Exp(Complex.Pow(c, pow));
            actual = cf.Exp(c, (x) => cf.Pow(c, pow));
            Assert.AreEqual(expected, actual);
        }
    }
}