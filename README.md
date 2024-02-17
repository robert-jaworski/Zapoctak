# Program na spravování fotek

Program na snadné spravování rodinných fotek - import, třídění do složek, přejmenování, backup, kontrola duplikátů atd.

Na čtení EXIF dat byla použita následující knihovna: https://drewnoakes.com/code/exif/

## Dokumentace
Uživatelská dokumentace je přístupná pomocí příkazu `help`. Programátorská dokumentace je v soubory `Dokumentace.md` a dále v komentářích v programu.

## Plán

- [x] Jednoduché importování fotek (seznam fotek, cílový adresář)
- [x] Automatické vyhledávání zdroje (šablony foťákových SD karet) - přes config soubory
- [x] Data z EXIFu
- [x] Template jména souborů a adresářového systému
- [x] Config soubory
- [x] Posunování času při importu, a také již importovaných souborů - ~~přepsání EXIFu~~ a přejmenování souboru
- [x] Indexovací soubory
- [x] Počítání hashů - kontrola duplikátů
- [x] Mazání - koš
- [x] `undo` - historie změn
- [x] `backup`
- [x] ~~select~~, `export` - vybírání fotek splňující určíté vlastnosti a jejich export, popř. jiné operace na nich (`change`)
- [ ] Interaktivní mód
- [ ] Hashe souborů v indexovacím souboru
- [ ] Tagy

## Složkový systém projektu

- README.md - přehled projektu, plán
- Specifikace.md - high level nápad projektu
- Použití.md - jak program používat

### AlbumLibrary

Soubory týkající se samotné logiky programu

### AlbumConsole

Soubory týkající se konzolové aplikace

### AlbumTest

Testy
