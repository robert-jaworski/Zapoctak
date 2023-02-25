# Program na spravování fotek

Program na snadné spravování rodinnıch fotek - import, tøídìní do sloek, pøejmenování, backup, kontrola duplikátù atd.

Na ètení EXIF dat byla pouita následující knihovna: https://drewnoakes.com/code/exif/

## Dokumentace
Uivatelská dokumentace je pøístupná pomocí pøíkazu `help`. Programátorská dokumentace je v soubory `Dokumentace.md` a dále v komentáøích v programu.

## Plán

- [x] Jednoduché importování fotek (seznam fotek, cílovı adresáø)
- [x] Automatické vyhledávání zdroje (šablony foákovıch SD karet) - pøes config soubory
- [x] Data z EXIFu
- [x] Template jména souborù a adresáøového systému
- [x] Config soubory
- [x] Posunování èasu pøi importu, a také ji importovanıch souborù - ~~pøepsání EXIFu~~ a pøejmenování souboru
- [x] Indexovací soubory
- [x] Poèítání hashù - kontrola duplikátù
- [x] Mazání - koš
- [x] `undo` - historie zmìn
- [x] `backup`
- [x] ~~select~~, `export` - vybírání fotek splòující urèíté vlastnosti a jejich export, popø. jiné operace na nich (`change`)
- [ ] Interaktivní mód
- [ ] Hashe souborù v indexovacím souboru
- [ ] Tagy

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
