using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace CVRP1
{    
    class Program
    {
            
        /**********************************************************************************************************************
         * glavna funkcija, trazi najkrace rjesenje.                                         
         * vraca najbolje rjesenje koje je nasla.                                                                  
         * brojMrava je broj mrava koji u svakoj iteraciji idu konstruirati rjesenje.
         * alfa i beta su parametri iz formule pomocu koje odredjujemo vjerojatnost odabira pojednog vrha kao iduceg;
         * sto manji alfa, to veci efekt udaljenosti gradova. sto manji beta, to veci efekt feromonski tragova;
         * parametarEvaporacije je izmedju 0 i 1. vrijednost 1 bi znacila da feromoni potpuno isparavaju nakon svake iteracije.
         * kolikoIteracija je broj iteracija koje ce funkcija izvrsiti. ako je postavljen na nulu, broj iteracija je beskonacno;
         * ako je postavljen na negativan broj, program se vrti sve dok -kolikoIteracija iteracija ne nadjemo bolje rjesenje.
         *************************************************************************************************************************/
        static Obilazak nadjiRjesenje(int brojMrava = 25, double alfa = 1, double beta = 2, double parametarEvaporacije = 0.2,
                                    long kolikoIteracija = -100)
        {
            // ucitavanje testnih podataka
            TestniPodaci podaci = new TestniPodaci(@"test\B-n34-k5.vrp");

            Dictionary<Obilazak, Obilazak> poznataPoboljsanja = new Dictionary<Obilazak, Obilazak>();
                     
            int brojVrhova = podaci.brojVrhova;
            Vrh[] vrhovi = new Vrh[brojVrhova+1];
            double kapacitetVozila = podaci.kapacitetVozila;
            for (int i = 0; i <= brojVrhova; ++i)
            {
                vrhovi[i] = podaci.vrhovi[i];
            }
            
            double[,] feromoni = new double[brojVrhova + 1, brojVrhova + 1];
            double[,] eta = new double[brojVrhova + 1, brojVrhova + 1];

            /* svakom vrhu pridruzujemo listu 15 najblizih vrhova i spremamo te podatke u rjecnik
             * najbliziVrhovi. moze se iskoristiti za ubrzavanja lokalnog pretrazivanja i sl. (vidi treci parametar u dvaOpt i triOpt).
             * za sad iskoristeno samo kod trazenja mogucih vrhova, vidi popunjavanje liste moguciVrhovi
             */
            Dictionary<Vrh, List<Vrh>> najbliziVrhovi = new Dictionary<Vrh, List<Vrh>>();
            foreach (var vrh in vrhovi)
            {
                najbliziVrhovi[vrh] = vrh.vratiNajblize(vrhovi.ToList(), 15);
            }

            for (int i = 0; i <= brojVrhova; i++)
            {
                for (int j = 0; j <= brojVrhova; j++)
                {
                    feromoni[i, j] = 1;
                    feromoni[j, i] = 1;
                    eta[i, j] = 1 / (vrhovi[i].udaljenost(vrhovi[j], 0));
                }
            }

            Random random = new Random();

            int brojIteracije = 1;
            int boljeRjesenjePrijeKoliko = 0;
            Obilazak globalniNajboljiPut = new Obilazak();
            double globalnaMinDuljina = 1000000;

            // svaki prolazak kroz sljedecu petlju predstavlja jednu iteraciju algoritma
            while (brojIteracije-1 < kolikoIteracija || kolikoIteracija < 0)
            {               
                double[] ukupniPut = new double[brojMrava];
                Obilazak[] prijedeniPut = new Obilazak[brojMrava];

                // svaki prolazak kroz sljedecu petlju izvrsava posao jednog mrava
                for (int mrav = 0; mrav < brojMrava; ++mrav)
                {
                    List<Vrh> neposjeceniVrhovi = new List<Vrh>();
                    prijedeniPut[mrav] = new Obilazak();
                    int brojPotpunoSlucajnih = 0;

                    foreach (var vrh in vrhovi)
                    {
                        if (vrh.oznaka != 0) neposjeceniVrhovi.Add(vrh);
                    }
                    neposjeceniVrhovi.Remove(vrhovi[1]);

                    prijedeniPut[mrav].dodajVrh(vrhovi[1]);  // vrhovi[1] = skladiste, odatle pocinje svaki obilazak

                    // slijedi konstrukcija kompletnog rjesenja... (to svaki mrav radi)
                    velikaPetlja: while (neposjeceniVrhovi.Count() > 0)
                    {
                        int brojPocetnogVrha = random.Next(0, neposjeceniVrhovi.Count());
                    
                        Vrh pocetniVrh = neposjeceniVrhovi[brojPocetnogVrha];
                        Vrh prosliVrh = pocetniVrh;
                        double preostaliKapacitet = kapacitetVozila - prosliVrh.potraznja;
                        prijedeniPut[mrav].dodajVrh(pocetniVrh);
                        neposjeceniVrhovi.Remove(pocetniVrh);
                    
                        while (true)
                        {
                            List<Vrh> moguciVrhovi = new List<Vrh>();

                            foreach (var vrh in neposjeceniVrhovi)
                            {
                                if (preostaliKapacitet - vrh.potraznja >= 0 && vrh.oznaka != prosliVrh.oznaka &&
                                    najbliziVrhovi[prosliVrh].Contains(vrh)) 
                                    moguciVrhovi.Add(vrh);
                            }

                            if (moguciVrhovi.Count() == 0)
                            {                               
                                foreach (var vrh in neposjeceniVrhovi)
                                {
                                    if (preostaliKapacitet - vrh.potraznja >= 0 && vrh.oznaka != prosliVrh.oznaka) 
                                      moguciVrhovi.Add(vrh);
                                 }
                            
                            }

                                                     
                            double qParametar = random.NextDouble();
                            double q = 0.6;
                            double q2 = 1 / ( ( brojPotpunoSlucajnih * 5 )/brojVrhova + 6);
                           
                            // ako mrav vise ne moze prijeci ni u jedan vrh a da ne dostavi vise nego sto ima, krece opet iz skladista
                            if (moguciVrhovi.Count() == 0)   
                            {
                                prijedeniPut[mrav].dodajVrh(vrhovi[1]);
                                goto velikaPetlja;
                            };
                            Vrh sljedeciVrh = null;
                            // imamo dva moguca nacina za izbor sljedeceg vrha, tj. po dvije razlicite formule
                            // (vidi ANT COLONY SYSTEM, npr. u stuzle-99) i slucajno biramo na koji cemo od ta dva nacina
                            // (za to sluze qParametar i q)
                            // UPDATE: dodan treci nacin: potpuno slucajan izbor! (za potrebe toga dodan parametar q2)
                            if (qParametar > q)
                            {
                                double r = random.NextDouble();

                                double[] vjerojatnosti = new double[brojVrhova + 1];
                                double[] rasponOd = new double[brojVrhova + 1];
                                double[] rasponDo = new double[brojVrhova + 1];

                                double suma = 0;
                                
                                foreach (var vrh in moguciVrhovi)
                                {
                                    suma += (Math.Pow(feromoni[prosliVrh.oznaka, vrh.oznaka], alfa) * Math.Pow(eta[prosliVrh.oznaka, vrh.oznaka], beta));
                                }

                                foreach (var vrh in moguciVrhovi)
                                {
                                    vjerojatnosti[vrh.oznaka] =
                                        (Math.Pow(feromoni[prosliVrh.oznaka, vrh.oznaka], alfa) *
                                        Math.Pow(eta[prosliVrh.oznaka, vrh.oznaka], beta)) / suma;
                                }

                                double rasponDoProsli = 0;

                                foreach (var vrh in moguciVrhovi)
                                {
                                    rasponOd[vrh.oznaka] = rasponDoProsli;
                                    rasponDo[vrh.oznaka] = rasponOd[vrh.oznaka] + vjerojatnosti[vrh.oznaka];
                                    rasponDoProsli = rasponDo[vrh.oznaka];
                                }

                                double randomDouble = random.NextDouble();

                                foreach (var vrh in moguciVrhovi)
                                {

                                    if (randomDouble >= rasponOd[vrh.oznaka] && randomDouble < rasponDo[vrh.oznaka])
                                    {
                                        sljedeciVrh = vrh;
                                        break;
                                    }
                                }
                            }
                            else if (qParametar > q2)
                            {
                                double najvecaVrijednost = 0;
                                foreach (var vrh in moguciVrhovi)
                                {
                                    double vrijednostZaOvajVrh =
                                        feromoni[prosliVrh.oznaka, vrh.oznaka] * Math.Pow(eta[prosliVrh.oznaka, vrh.oznaka], beta);
                                    if (vrijednostZaOvajVrh >= najvecaVrijednost)
                                    {
                                        najvecaVrijednost = vrijednostZaOvajVrh;
                                        sljedeciVrh = vrh;
                                    }

                                }
                            }
                            else
                            {
                                brojPotpunoSlucajnih++;
                                int indeksSljedeceg = random.Next(0, moguciVrhovi.Count());
                                sljedeciVrh = moguciVrhovi[indeksSljedeceg];
                            }
                            preostaliKapacitet -= sljedeciVrh.potraznja;
                            prijedeniPut[mrav].dodajVrh(sljedeciVrh);
                            neposjeceniVrhovi.Remove(sljedeciVrh);
                        }
                    }

                    // mrav je konstruirao neko rjesenje, slijedi dosta agresivno i vremenski skupo lokalno pretrazivanje
                    // stavljeno je da se vrsi samo u svakoj desetoj iteraciji jer bi inace bilo jos puno sporije
                    // ako se zakomentira taj dio koda, ide puno brze, ali i rezultati su losiji
                    // EDIT: dodano da program pamti do sada poznata poboljsanja i onda ne pretrazuje ako smo za neki put vec prije vrsili 
                    // dvaOpt ili triOpt... medutim, cini se da to ne ubrzava program (mozda nesto nije dobro napravljeno?)... ali ga ni ne
                    // usporava... znaci, to sad nije toliko bitno, prouciti kasnije.
                    
                    if (brojIteracije % 10 == 9  &&  poznataPoboljsanja.ContainsKey(prijedeniPut[mrav]) == false)
                    {
                        bool nasliSmoBoljiPut = false;
                        Obilazak ulazniObilazak = new Obilazak();
                        foreach (var vrh in prijedeniPut[mrav].put)
                        {
		                    ulazniObilazak.dodajVrh(vrh);
	                    }
                        int p = 1;
                        while (p == 1)
                        {
                            p = 0;
                            Obilazak mozdaBoljiPut1 = prijedeniPut[mrav].dvaOpt(kapacitetVozila, prijedeniPut[mrav].duljinaObilaska(), najbliziVrhovi);

                            if (mozdaBoljiPut1 != null)
                            {
                                p = 1;
                                prijedeniPut[mrav] = mozdaBoljiPut1;
                                nasliSmoBoljiPut = true;
                            }
                        }

                      /*  p = 1;
                        while (p == 1)
                        {
                            p = 0;
                            Obilazak mozdaBoljiPut2 = prijedeniPut[mrav].triOpt(kapacitetVozila, prijedeniPut[mrav].duljinaObilaska(), najbliziVrhovi);

                            if (mozdaBoljiPut2 != null)
                            {
                                p = 1;
                                prijedeniPut[mrav] = mozdaBoljiPut2;
                                nasliSmoBoljiPut = true;
                            }
                        }
                        */
                        if (nasliSmoBoljiPut) 
                            poznataPoboljsanja.Add(ulazniObilazak, prijedeniPut[mrav]);
                        else
                            poznataPoboljsanja.Add(ulazniObilazak, null);
                    }
                    else
                    {
                        if (poznataPoboljsanja.ContainsKey(prijedeniPut[mrav]) && poznataPoboljsanja[prijedeniPut[mrav]] != null)
                        {                          
                            prijedeniPut[mrav] = poznataPoboljsanja[prijedeniPut[mrav]];                          
                        }
                    }

                    // stutzle-99, 7. str. skroz dolje, local pheromone update: (znaci, put koji mrav izabere gubi dio svojih feromona,
                    // na taj nacin poticemo istrazivanje novih puteva...

                    ukupniPut[mrav] = prijedeniPut[mrav].duljinaObilaska();
                    double ksi = 0.8;
                    double tau0 = 0.0000000002;   // jos nije sigurno da je ovo dobra vrijednost za tau0

                    int prosli = 1;
                    for (int i = 1; i < prijedeniPut[mrav].put.Count(); i++)
                    {
                        feromoni[prijedeniPut[mrav].put[i].oznaka, prosli] = (1 - ksi) * (feromoni[prijedeniPut[mrav].put[i].oznaka, prosli]) + ksi*tau0;
                        feromoni[prosli, prijedeniPut[mrav].put[i].oznaka] = (1 - ksi) * (feromoni[prosli, prijedeniPut[mrav].put[i].oznaka]) + ksi * tau0;
                        prosli = prijedeniPut[mrav].put[i].oznaka;
                    }
                }

                // iteracija je zavrsila i trazimo najbolje rjesenje iz te iteracije, a zatim ga usporedujemo s najboljim koje do sada znamo

                double ukupniPutIteracijskiMin = ukupniPut[0];
                int indeksMin = 0;
                for (int i = 1; i < brojMrava; i++)
                {
                    if (ukupniPut[i] < ukupniPutIteracijskiMin)
                    {
                        ukupniPutIteracijskiMin = ukupniPut[i];
                        indeksMin = i;
                    }
                }

                if (ukupniPutIteracijskiMin < globalnaMinDuljina)
                {
                    boljeRjesenjePrijeKoliko = 0;
                    globalniNajboljiPut = new Obilazak();

                    foreach (var vrh in prijedeniPut[indeksMin].put)
                    {
                        globalniNajboljiPut.dodajVrh(vrh);
                    }
                    
                    globalnaMinDuljina = ukupniPutIteracijskiMin;
                    
                }

                // azuriramo feromone samo po najboljem do sada poznatom rjesenju, za iteracijsko bi islo ovako nekako:
                //   double feromonskiDelta = 1 / ukupniPutIteracijskiMin;
                // ... plus morale bi se promijeniti jos neke stvari malo nize  
                // u nekom od onih PDF-ova pise da je najbolje prvo azurirati najbolji iteracijski put a onda postepeno sve cesce samo globalni
                // najbolji put... a inace je, ako biramo samo jedno od tog dvoje, bolje stalno azurirati samo globalni. za sada stoji tako,
                // radi jednostavnosti.

                double feromonskiDelta = 1 / globalnaMinDuljina;

                for (int i = 1; i <= brojVrhova; i++)
                {
                    for (int j = 1; j <= brojVrhova; j++)
                    {
                        feromoni[i, j] *= 1 - parametarEvaporacije;
                        feromoni[j, i] *= 1 - parametarEvaporacije;                        
                    }
                }

                int prosli2 = 1;
                for (int i = 1; i < globalniNajboljiPut.put.Count(); i++)
                {
                    feromoni[globalniNajboljiPut.put[i].oznaka, prosli2] += feromonskiDelta;
                    feromoni[prosli2, globalniNajboljiPut.put[i].oznaka] += feromonskiDelta;
                    prosli2 = globalniNajboljiPut.put[i].oznaka;
                }
                boljeRjesenjePrijeKoliko++;
                if (kolikoIteracija < 0  && boljeRjesenjePrijeKoliko == Math.Abs(kolikoIteracija)) return globalniNajboljiPut;
                if (kolikoIteracija != 0) brojIteracije++;
            }
            return globalniNajboljiPut;
        }

        static void Main(string[] args)
        {
            
            double najboljeRjesenje = 1000000;

            Random rand = new Random();

            // stalno pozivamo glavnu funkciju (to je kao da stalno ispocetka pokrecemo program) i, kad nam nadje bolje rjesenje od najboljeg do sada,
            // to rjesenje se ispisuje, skupa s duljinom puta za to rjesenje
            while (true)
            {
                // ovi parametri trenutno nemaju smisla jer pozivamo program s fiksiranim parametrima, ali ovaj dio programa zapravo
                // generira neke kvazislucajne parametre, tj. tako nesto se moze iskoristiti kad nesto bitno promijenimo u programu
                // i nismo sigurni s kojim parametrima novi program najbolje radi, pa ga pustimo da isprobava razne...
                double beta = rand.NextDouble() * 1 + 1;
                double evap = rand.NextDouble() * 0.1 + 0.8;
                int brojMrava = rand.Next(20, 30);
                Console.WriteLine(DateTime.Now);
                Obilazak rjesenje = nadjiRjesenje(25, 1, 2, 0.2, 1000);
                
                double duljinaObilaskaRjesenja = rjesenje.duljinaObilaska();
                if (duljinaObilaskaRjesenja < najboljeRjesenje)
                {
                    najboljeRjesenje = duljinaObilaskaRjesenja;
                    rjesenje.ispisi();                   
                    Console.WriteLine(rjesenje.duljinaObilaska());
                 // ispis parametara koji slijedi ima smisla samo ako su parametri slucajno generirani...
                 /* Console.Write("alfa = " + alfa + ", beta = " + beta + ", evap = " + evap);
                    Console.WriteLine();
                    Console.Write("broj mrava = " + brojMrava); */
                    Console.WriteLine(); 
                }
            }
        }
    }
}

