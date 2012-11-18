using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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

        // slicno kao dvaOpt, ali mijenjamo mjesta za 3 vrha (tako da nijedan od njih ne bude gdje je bio)
        public Obilazak triOpt(double dopustenaCijena, double ulaznaDuljina, Dictionary<Vrh, List<Vrh>> najbliziVrhovi = null)
        {
            if (put == null) return null;
            Obilazak obilazak;
            Obilazak izlazniObilazak = null;
            List<Obilazak> listaObilazaka = new List<Obilazak>();

            for (int i = 1; i < put.Count(); i++)
            {
                for (int j = i; j < put.Count(); j++)
                {
                    for (int k = j; k < put.Count(); k++)
                    {                        
                        obilazak = new Obilazak();
                        foreach (var vrh in put) obilazak.dodajVrh(vrh);
                        Vrh temp = obilazak.put[i];
                        obilazak.put[i] = obilazak.put[k];
                        obilazak.put[k] = obilazak.put[j];
                        obilazak.put[j] = temp;
                        if (obilazak.dopustiv(dopustenaCijena)) listaObilazaka.Add(obilazak);
                        obilazak = new Obilazak();
                        foreach (var vrh in put) obilazak.put.Add(vrh);
                        temp = obilazak.put[i];
                        obilazak.put[i] = obilazak.put[j];
                        obilazak.put[j] = obilazak.put[k];
                        obilazak.put[k] = temp;
                        if (obilazak.dopustiv(dopustenaCijena)) listaObilazaka.Add(obilazak);
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

    }
}
