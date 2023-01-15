# Program na spravov�n� fotek

Program na snadn� spravov�n� rodinn�ch fotek - import, t��d�n� do slo�ek, p�ejmenov�n�, backup, kontrola duplik�t� atd.

Na �ten� EXIF dat byla pou�ita n�sleduj�c� knihovna: https://drewnoakes.com/code/exif/

## Pl�n

- [ ] Jednoduch� importov�n� fotek (seznam fotek, c�lov� adres��)
- [ ] Automatick� vyhled�v�n� zdroje (�ablony fo��kov�ch SD karet)
- [x] Data z EXIFu
- [x] Template jm�na soubor� a adres��ov�ho syst�mu
- [ ] Config soubory
- [ ] Posunov�n� �asu p�i importu, a tak� ji� importovan�ch soubor� - ~~p�eps�n� EXIFu~~ a p�ejmenov�n� souboru
- [ ] Indexovac� soubory
- [ ] Po��t�n� hash� - kontrola duplik�t�
- [ ] Maz�n� - ko�
- [ ] Undo - historie zm�n
- [ ] Backup
- [ ] Select, export - vyb�r�n� fotek spl�uj�c� ur��t� vlastnosti a jejich export, pop�. jin� operace na nich
- [ ] Interaktivn� m�d

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
