# Program na spravování fotek

Program na snadné spravování rodinnıch fotek - import, tøídìní do sloek, pøejmenování, backup, kontrola duplikátù atd.

Na ètení EXIF dat byla pouita následující knihovna: https://drewnoakes.com/code/exif/

## Plán

- [ ] Jednoduché importování fotek (seznam fotek, cílovı adresáø)
- [ ] Automatické vyhledávání zdroje (šablony foákovıch SD karet)
- [x] Data z EXIFu
- [x] Template jména souborù a adresáøového systému
- [ ] Config soubory
- [ ] Posunování èasu pøi importu, a také ji importovanıch souborù - ~~pøepsání EXIFu~~ a pøejmenování souboru
- [ ] Indexovací soubory
- [ ] Poèítání hashù - kontrola duplikátù
- [ ] Mazání - koš
- [ ] Undo - historie zmìn
- [ ] Backup
- [ ] Select, export - vybírání fotek splòující urèíté vlastnosti a jejich export, popø. jiné operace na nich
- [ ] Interaktivní mód

## Slokovı systém projektu

- README.md - pøehled projektu, plán
- Specifikace.md - high level nápad projektu
- Pouití.md - jak program pouívat

### AlbumLibrary

Soubory tıkající se samotné logiky programu

### AlbumConsole

Soubory tıkající se konzolové aplikace

### AlbumTest

Testy
