using System;
using Harkkatyö;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SaadinTesti
{
    [TestClass]
    public class UnitTest1
    {
        Saadin testattavaSaadin = new Saadin(false, 10);
        [TestMethod]
        public void SaadinAsetusarvoTesti()
        {
            Saadin testattavaSaadin = new Saadin(false, 10);
            int ohjaus = testattavaSaadin.PalautaOhjaus(200, 50, 50);
            Assert.AreEqual(100,ohjaus);
        }
        [TestMethod]
        public void SaadinMittausTesti()
        {
            Saadin testattavaSaadin = new Saadin(false, 10);
            int ohjaus = testattavaSaadin.PalautaOhjaus(50, 200, 50);
            Assert.AreEqual(0,ohjaus);
        }
        [TestMethod]
        public void SaadinNollalinjaTesti()
        {
            Saadin testattavaSaadin = new Saadin(false, 10);
            int ohjaus = testattavaSaadin.PalautaOhjaus(60, 50, 100);
            Assert.AreEqual(100,ohjaus);
        }
        public void SaadinAsetusarvoTestiIso()
        {
            Saadin testattavaSaadin = new Saadin(false, 10);
            int ohjaus = testattavaSaadin.PalautaOhjaus(1000000, 50, 50);
            Assert.AreEqual(0, ohjaus);
        }
        [TestMethod]
        public void SaadinMittausTestiIso()
        {
            Saadin testattavaSaadin = new Saadin(false, 10);
            int ohjaus = testattavaSaadin.PalautaOhjaus(50, 1000000, 50);
            Assert.AreEqual(0, ohjaus);
        }
        [TestMethod]
        public void SaadinNollalinjaTestiIso()
        {
            Saadin testattavaSaadin = new Saadin(false, 10);
            int ohjaus = testattavaSaadin.PalautaOhjaus(60, 50, 1000000);
            Assert.AreEqual(1000, ohjaus);
        }
    }
}
