using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.MemoryMappedFiles;
using System.IO;

namespace CVRP1
{
    class TestniPodaci
    {
        public Vrh[] vrhovi;
        public int brojVrhova;
        public double kapacitetVozila;
        string nazivDatoteke; 

        public TestniPodaci(string nazivDatoteke)
        {
            this.nazivDatoteke = nazivDatoteke;
            using (var mappedFile1 = MemoryMappedFile.CreateFromFile(nazivDatoteke))
            {
                using (Stream mmStream = mappedFile1.CreateViewStream())
                {
                    using (StreamReader sr = new StreamReader(mmStream, Encoding.Default))
                    {
                        sr.ReadLine();
                        sr.ReadLine();
                        sr.ReadLine();
                        var line = sr.ReadLine();
                        var lineWords = line.Split(' ');
                        brojVrhova = Convert.ToInt32(lineWords[2]);
                        sr.ReadLine();
                        line = sr.ReadLine();
                        lineWords = line.Split(' ');
                        if (lineWords[0] == "CAPACITY") kapacitetVozila = Convert.ToInt32(lineWords[2]);
                        vrhovi = new Vrh[brojVrhova + 1];
                        for (int i = 0; i < brojVrhova + 1; i++)
                        {
                            vrhovi[i] = new Vrh();
                        }

                        while (true)
                        {
                            line = sr.ReadLine();
                            lineWords = line.Split(' ');

                            if (lineWords[0] == "NODE_COORD_SECTION")
                            {
                                for (int i = 1; i <= brojVrhova; i++)
                                {
                                    var line2 = sr.ReadLine();
                                    var lineWords2 = line2.Split(' ');
                                    vrhovi[i].x = Convert.ToInt32(lineWords2[2]);
                                    vrhovi[i].y = Convert.ToInt32(lineWords2[3]);
                                }
                            }

                            if (lineWords[0] == "DEMAND_SECTION")
                            {
                                for (int i = 1; i <= brojVrhova; i++)
                                {
                                    line = sr.ReadLine();
                                    lineWords = line.Split(' ');
                                    vrhovi[i].potraznja = Convert.ToInt32(lineWords[1]);
                                    vrhovi[i].oznaka = i;
                                }
                                vrhovi[0].potraznja = 0;
                                vrhovi[0].oznaka = 0;
                            }

                            if (lineWords[0] == "DEPOT_SECTION")
                            {
                                var lineX = sr.ReadLine();
                                var lineWordsX = lineX.Split(' ');
                                vrhovi[0].x = Convert.ToInt32(lineWordsX[1]);
                                var lineY = sr.ReadLine();
                                var lineWordsY = lineY.Split(' ');
                                vrhovi[0].y = Convert.ToInt32(lineWordsY[1]);
                            }

                            if (lineWords[0] == "EOF") break;
                        }
                    }
                }
            }
        }
    }
}
