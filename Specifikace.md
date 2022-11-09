# Program na ukládání fotek

## Nápad
Program, který bude umět importovat fotky z foťáků a mobilů a ukládat do společného adresáře (alba) - dobré pro spravování rodinných fotek.
Program bude napsaný v **C#** a poběží pod **Windows**, jelikož tam ho budeme hlavně používat.

Program bude muset být dostatečně rychlý při velkém množství fotek v albu (řádově 200 000 fotek, aneb 1 TB, neboť tolik máme aktuálně dohromady fotek).

Program bude fotky řadit podle data a času, které získá z EXIFu, popř. z metadat souborů (template bude konfiguravatelný a bude umožňovat uložení více fotek se stejným časem). Fotky budou ukládány do adresářů podle roků a v nich do adresářů podle měsíců. Občas se stane, že dva různé foťaky mají jinak nastavený čas. Když se toto stane, program bude umět fotkám hromadně posunout čas, aby se usnadnila ruční oprava.

Dále bude program umět kontrolovat duplikáty (pomocí hashů souborů), tj. jestli jsme omylem nenahráli nějakou fotku dvakrát. Dále bude umět nastavovat fotkám příznaky (např. kdo je vyfotil, při jaké příležitosti). Podle všech informací pak bude umět fotky filtrovat, třeba zkopírovat do nějaké složky všechny vyfocené v červnu 2017 jedním konkrétním člověkem.

Bude podporovat i mazání fotek, ale všechny operace bude možné vrátit zpět. Takže mazání bude vlastně přesouvání do koše, který pak můžeme vysypat, až si budeme jistí, že jsme nesmazali nic, co jsme nechtěli. Dále bude umět celé album zazálohovat na nějaký externí disk.

## Použití
Samotný program bude napsán jako knihovna, aby šlo snadno udělat konzolovou i grafickou aplikaci.

Konzolová aplikace se bude používat podobně jako `git`. Například příkaz `album import [path] [--time-shift=<time>]` (nebo tak nějak) naimportuje fotky ze zadané cesty, popřípadě program sám vyhledá, jestli není připojená nějaká SD karta s fotkami (resp. jakékoli externí úložistě), a možná jim upraví čas.

(Možná někdy později přidělám i nějaké GUI, ale teď bych to spíš nedělal - uvidím kolik budu mít času.)
