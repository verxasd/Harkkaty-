using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harkkatyö
{
    interface Mittaus
    {
        double PalautaDouble(string nimi);
        int PalautaInt(string nimi);
        bool PalautaBool(string nimi);
    }
}
