# Program na spravov�n� fotek

Program na snadn� spravov�n� rodinn�ch fotek - import, t��d�n� do slo�ek, p�ejmenov�n�, backup, kontrola duplik�t� atd.

Na �ten� EXIF dat byla pou�ita n�sleduj�c� knihovna: https://drewnoakes.com/code/exif/

## Dokumentace
U�ivatelsk� dokumentace je p��stupn� pomoc� p��kazu `help`. Program�torsk� dokumentace je v soubory `Dokumentace.md` a d�le v koment���ch v programu.

## Pl�n

- [x] Jednoduch� importov�n� fotek (seznam fotek, c�lov� adres��)
- [x] Automatick� vyhled�v�n� zdroje (�ablony fo��kov�ch SD karet) - p�es config soubory
- [x] Data z EXIFu
- [x] Template jm�na soubor� a adres��ov�ho syst�mu
- [x] Config soubory
- [x] Posunov�n� �asu p�i importu, a tak� ji� importovan�ch soubor� - ~~p�eps�n� EXIFu~~ a p�ejmenov�n� souboru
- [x] Indexovac� soubory
- [x] Po��t�n� hash� - kontrola duplik�t�
- [x] Maz�n� - ko�
- [x] `undo` - historie zm�n
- [x] `backup`
- [x] ~~select~~, `export` - vyb�r�n� fotek spl�uj�c� ur��t� vlastnosti a jejich export, pop�. jin� operace na nich (`change`)
- [ ] Interaktivn� m�d
- [ ] Hashe soubor� v indexovac�m souboru
- [ ] Tagy

## Slo�kov� syst�m projektu

- README.md - p�ehled projektu, pl�n
- Specifikace.md - high level n�pad projektu
- Pou�it�.md - jak program pou��vat

### AlbumLibrary

Soubory t�kaj�c� se samotn� logiky programu

### AlbumConsole

Soubory t�kaj�c� se konzolov� aplikace

### AlbumTest

Testy
