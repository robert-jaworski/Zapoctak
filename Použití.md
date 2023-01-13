# Pou�it�
`album <command> [parameters]`

Ka�d� p��kaz m� svoje parametry a nastaven�, n�kter� nastaven� jdou pou��t na v�echny p��kazy.

## Parametry
Parametry maj� dlouhou (nap�. `--album-dir`, `--verbose`) a kr�tkou verzi (`-d`, `-v`). N�kter� parametry po���aduj� dal�� hodnotu (t�eba n�zev souboru nebo ��slo), kter� n�sleduje hned po n�zvu parametru. Kr�tk� verze paramentr� jdou spojovat (`-vd`). Pokud spoj�me kr�tk� verze parametr�, kter� pot�ebuj� dodate�nou hodnotu, mus� tyto hodnoty n�sledovat ve stejn�m po�ad�.

## Typy parametr�
Parametry jsou �ty� typ�:
- `flag` - bu� je nastaven nebo ne, nap�. `--verbose`
- `number` - ��slo
- `string` - text, nap�. `--album-dir`
- `files` - seznam soubor�, ukon�eno jakmile se naraz� na dal�� parametr, nap�. `--import-files abc.jpg def.jpg -x xyz.jpg` bude odpov�dat soubor�m `abc.jpg def.jpg`
    - seznam soubor� m��e obsahovat n�zev slo�ky ukon�en� `/`, resp. `\` (nap�. `photos/`), ten bude odpov�dat v�em soubor�m v dan� slo�ce (nerekurzivn�)
    - pro rekurzivn� proch�zen� podadres��� ukon�ete n�zev slo�ky `/...`, resp. `\...`
    - seznam soubor� m��e taky obsahovat intervaly soubor�, specifikovan� pomoc� `...` (nap�. `img010.jpg ... img029.jpg`, `img100.jpg...`, `...foto/img100.jpg`, `../foto/img100.jpg ... ../foto/img300.jpg`)

## Univerz�ln� parametry
- `--album-dir`, `-d` - nastav� adres�� s albem, se kter�m budeme pracovat. Defaultn� hodnota je aktu�ln� adres��.
- `--verbose`, `-v` - program vypisuje v�ce informac�

## P��kazy
- `help` - zobraz� dostupn� p��kazy a parametry
- `import <files>`
