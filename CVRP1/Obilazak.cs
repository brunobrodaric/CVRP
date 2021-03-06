﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace CVRP1
{
    class Obilazak
    {
        public List<Vrh> put;

        public Obilazak()
        {
            put = new List<Vrh>();
        }

        public Obilazak(List<Vrh> listaVrhova)
        {
            put = listaVrhova;
        }

        public void dodajVrh(Vrh vrh)
        {
            put.Add(vrh);        
        }

        public bool jeLiJednak(Obilazak o)
        {
            if (o == null && this == null) return true;
            if (o == null && this != null) return false;
            if (o != null && this == null) return false;
            int brojCvorova1 = o.put.Count();
            int brojCvorova2 = this.put.Count();
            if (brojCvorova1 != brojCvorova2) return false;
            else 
            {
                for (int i = 0; i < brojCvorova1; ++i)
                {
                    if (o.put[i] != this.put[i]) return false;
                }
                return true;
            }
            
        }

        public void ispisi()
        {
            if (put == null) return;
            foreach (var vrh in put)
            {
                Console.Write(vrh.oznaka + " ");
            }
            Console.WriteLine();
        }

        public double duljinaObilaska(int smijeNula = 1)
        {
            double duljina = 0;
            if (put == null) return 0;
            for (int i = 1; i < put.Count(); i++)
            {
                duljina += put[i - 1].udaljenost(put[i], smijeNula);
            }
            return duljina;
        }

        public bool dopustiv(double dopustenaCijena)
        {
            if (put[0].oznaka != 1) return false;
            if (put[put.Count() - 1].oznaka != 1) return false;
            double cijena = 0;
            foreach (var vrh in put)
            {
                cijena += vrh.potraznja;
                if (cijena > dopustenaCijena) return false;
                if (vrh.oznaka == 1) cijena = 0;
            }
            return true;
        }

        // nije doslovce 2-opt, tj. prikladniji naziv bi mozda bio kvaziDvaOpt... ipak, trebalo bi raditi *vise* od pravog dvaOpta (ali sporije)
        // pokusa zamijeniti *svaka* dva vrha u obilasku, cak ako su isti ili zamjena nije dopustiva itd. a onda gleda je li ta zamjena dopustiva
        // pa na kraju medju svim dopustivim zamjenama vraca onu koja najvise poboljsava rjesenje AKO ga ijedna poboljsava.
        // moze se poboljsati tako da ima manje *potpuno* nepotrebnih operacija...
        // treci parametar za sada neiskoristen, moze se iskoristiti za to da gledamo zamjene samo najblizih vrhova, ali
        // to moze dovesti do toga da funkcija ne nadje neko bolje rjesenje koje je mogla. prednost bi bila povecanje brzine, ali za sada je
        // ipak kvaliteta rjesenja prioritet...
        public Obilazak dvaOpt(double dopustenaCijena, double ulaznaDuljina, Dictionary<Vrh, List<Vrh>> najbliziVrhovi = null)
        {
            if (put == null) return null;

            Obilazak obilazak;
            Obilazak izlazniObilazak = null;
            List<Obilazak> listaObilazaka = new List<Obilazak>();

            for (int i = 1; i < put.Count(); i++)
            {
                for (int j = i; j < put.Count(); j++)
                {
                    {
                        obilazak = new Obilazak();
                        foreach (var vrh in put) obilazak.dodajVrh(vrh);
                        Vrh temp = obilazak.put[i];
                        obilazak.put[i] = obilazak.put[j];
                        obilazak.put[j] = temp;
                        if (obilazak.dopustiv(dopustenaCijena))
                            listaObilazaka.Add(obilazak);
                    }
                }
            }

            double najboljaDuljina = ulaznaDuljina;
            foreach (var ob in listaObilazaka)
            {
                if (ob.duljinaObilaska() < najboljaDuljina)
                {
                    najboljaDuljina = ob.duljinaObilaska();
                    izlazniObilazak = ob;
                }
            }

            if (najboljaDuljina < ulaznaDuljina)
                return izlazniObilazak; 
            else return null;
        }

        // funkcija za crtanje rjesenja... potrebno imati instaliran graphviz
        // uglavnom, skoro sve je podesivo, guglati npr. graphviz attributes
        // generira sliku rjesenja "nacrtaj.png" u bin\debug folderu projekta
        public void nacrtaj()
        {
            System.IO.StreamWriter file = new System.IO.StreamWriter("nacrtaj.txt");

            file.WriteLine("graph{");
            foreach (var cvor in this.put)
            {
                file.WriteLine("resolution=200;");
                file.WriteLine(cvor.oznaka + "[");
                file.WriteLine("label = " + cvor.oznaka);
                file.WriteLine("pos = \"" + cvor.x * 6 + "," + cvor.y * 6 + "!\"");
                file.WriteLine("width = 0.15");
                file.WriteLine("height = 0.15");
                file.WriteLine("fixedsize=true");
                file.WriteLine("fontsize = 8");
                if (cvor.oznaka == 1) file.WriteLine("shape = box"); //else file.WriteLine("shape = point");
                file.WriteLine("]");
            }

            string[] boje = { "red", "navy", "green", "brown", "yellow", "tomato", "dark blue", "deep pink", "teal", "black", "gray", "crimson" };

            int brojBoje = 0;
            for (int i = 1; i < this.put.Count(); ++i)
            {
                file.WriteLine(this.put[i - 1].oznaka + " -- " + this.put[i].oznaka + "[color=\"" + boje[brojBoje] + "\"]");
                if (this.put[i].oznaka == 1) brojBoje++;
            }

            file.WriteLine("}");
            file.Close();
            ProcessStartInfo startInfo = new ProcessStartInfo("dot.exe");
            startInfo.Arguments = "-Kneato -Goverlap=scaling -Tpng nacrtaj.txt -o nacrtaj.png";
            Process.Start(startInfo);  
        }

    }
}
