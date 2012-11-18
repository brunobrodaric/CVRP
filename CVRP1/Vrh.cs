using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CVRP1
{
    class Vrh
    {
        public double x;
        public double y;
        public double potraznja;
        public int oznaka;

        public Vrh(double x, double y, double potraznja, int oznaka)
        {
            this.x = x;
            this.y = y;
            this.potraznja = potraznja;
            this.oznaka = oznaka;
        }

        public Vrh()
        {
            this.x = 0;
            this.y = 0;
            this.potraznja = 0;
            this.oznaka = -1;
        }

        /* ako je smijeNula = 0, funkcija nikad ne vraca 0, sto nam ponekad treba jer ne smijemo dijeliti s nula i sl. */
        public double udaljenost(Vrh drugiVrh, int smijeNula = 1)
        {
            double udaljeni;
            udaljeni = Math.Ceiling(Math.Sqrt(Math.Pow(this.x - drugiVrh.x, 2) + Math.Pow(this.y - drugiVrh.y, 2)));
            if (udaljeni == 0 && smijeNula == 0) 
                return 0.000000001;
            else return udaljeni;
        }

        /* vrati kolikoNajblizih vrhova iz liste vrhova, ili sve vrhove ako ih ima manje */
        public List<Vrh> vratiNajblize(List<Vrh> listaVrhova, int kolikoNajblizih)
        {            
            List<Vrh> pomocnaLista = listaVrhova;
            List<Vrh> izlaznaLista = new List<Vrh>();

            if (listaVrhova.Count() < kolikoNajblizih) return listaVrhova;
            pomocnaLista = pomocnaLista.OrderBy(x => x.udaljenost(this)).ToList();            
            for (int i = 0; i < kolikoNajblizih; i++)
            {
                izlaznaLista.Add(pomocnaLista[i]);
            }
            return izlaznaLista;
        }

    }
}
