﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnifiedAutomation.UaBase;
using UnifiedAutomation.UaClient;

namespace Harkkatyö
{
    class Prosessi
    {
        /* Sekalaista yritystä saada yhteys ohjelman ja simulaattorin välille
         * 
         * var client = new UnifiedAutomation.UaClient.("opc.tcp://127.0.0.1:8087")
        {

        }*/

        // Säiliöiden pinnankorkeudet
        private int LI200;
        
        // Pumppujen tilat
        private int P100;
        private int P200;

        // Mitattavat lämpötilat
        private int TI100;
        private int TI300;

        // Säätöventtiilien tilat
        private int V102;
        private int V104;

        // Virtaus ennen P100
        private double FI100;

        // T100 pintahälytys
        private bool LA100;

        // Sulkuventtiilien tilat
        private bool V101;
        private bool V103;

        private bool V201;
        private bool V202;
        private bool V203;
        private bool V204;

        private bool V301;
        private bool V302;
        private bool V303;
        private bool V304;

        private bool V401;
        private bool V402;
        private bool V403;
        private bool V404;

    }
}
