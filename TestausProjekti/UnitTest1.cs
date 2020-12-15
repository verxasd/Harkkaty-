using Harkkatyö;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestausProjekti
{
    class UnitTest1
    {
        [TestMethod]
        public void SaadinAsetusarvoTesti()
        {
            Saadin testattavaSaadin = new Saadin(false, 10);
            int ohjaus = testattavaSaadin.PalautaOhjaus(200, 50, 50);
            // Ohjauksen tulisi olla 100
            Assert.AreEqual(100, ohjaus);
        }
        [TestMethod]
        public void SaadinMittausTesti()
        {
            Saadin testattavaSaadin = new Saadin(false, 10);
            int ohjaus = testattavaSaadin.PalautaOhjaus(50, 200, 50);
        }
        [TestMethod]
        public void SaadinNollalinjaTesti()
        {
            Saadin testattavaSaadin = new Saadin(false, 10);
            int ohjaus = testattavaSaadin.PalautaOhjaus(60, 50, 1000);
        }
    }
}
