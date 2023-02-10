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
    - odkaz na posledn� upraven� soubory `@last`, pop�. `@lastN` kde N je po�et operac�, kter� n�s zaj�maj�, tedy `@last3` bude odpov�dat v�em soubor�m, kter� jsme upravili v posledn�ch t�ech operac�ch (tedy p�ikazech)

## Univerz�ln� parametry
- `--album-dir`, `-d` - nastav� adres�� s albem, se kter�m budeme pracovat. Defaultn� hodnota je aktu�ln� adres��.
- `--verbose`, `-v` - program vypisuje v�ce informac�

## Speci�ln� parametry
- P��kazy `metadata`, `import` a `export`
    - parameter `--extensions` o�ek�v� seznam povolen�ch typ� soubor� odd�len�ch ��rkami, nap�. `jpg,.png` (typy soubor� mohou ale nemus� obsahovat te�ku)
- P��kazy `import` a `export`
    - parameter `--template` obsahuje template n�zvu v�sledn�ch soubor�. Defaultn� hodnota je `{YYYY}/{MM}/{YYYY}{MM}{DD}-{hh}{mm}{ss}`.
        - Template se m��e skl�dat z v�ce vlastn�ch templat� odd�len�ch ��rkou, pou�ije se prvn�, kter� jde.
        - Template m��e obsahovat placeholdery kter� se specifikuj� `{type:options#width}`, kde `options` a `width` jsou nepovinn�. `width` specifikuje po�adovanou minim�ln� ���ku polo�ky. `options` bl�e specifikuje, co bude v�sledkem nahrazen� placeholderu.
        - Dostupn� hodnoty typ� jsou: `year`/`Y`, `YYYY`, `month`/`M`, `MM`, `day`/`D`, `DD`, `hour`/`h`, `hh`, `minute`/`m`, `mm`, `second`/`s`, `ss`, `device:name`, `device:manufacturer`, `device:model`, `noextension`/`noext`, `extension`/`ext`/`.`, `file`/`file:name`, `file:extension`/`file:ext`, `file:relativePath`/`file:relPath`/`file:rel`
        - Pokud n�kter� hodnota nen� dostupn� (nap�. `device:model`), template se nepou�ije
        - Placeholdery data a �asu m��ou m�t `options` n�sleduj�c�: `exif` (defaultn�), `create`, `modify`. Pokud nen� `options` nastaveno, pou�ije se bu� `exif` nebo `create`, podle toho, co existuje
        - Placeholdery `extension` a `noextension` umo��uj� filtrovat podle koncovek soubor�, nap�. `{YYYY}/{MM}/{YYYY}{MM}{DD}-{hh}{mm}{ss}{ext:.jpg},other/{YYYY}{MM}{DD}-{hh}{mm}{ss}`. Rozd�l je v tom, �e `extension` se nahrad� specifikovanou koncovkou souboru (p�esn� jak je v templatu), ale `noextension` z�stane pr�zdn�. Samotn� `{ext}` se nahrad� koncovkou p�vodn�ho souboru.
        - Pokud nen� specifikovan� `extension` nebo `noextension` na konec n�zvu se p�id� koncovka p�vodn�ho souboru
    - parametry `--after-date` a `--before-date` umo��uj� filtrovat fotky pomoc� data a �asu, kdy byly po��zeny
    - parametr `--time-shift` umo��uje posunout �as, kdy byly fotky po��zeny. Tato operace ale nijak nep�episuje EXIF (kv�li he�ov�n� a lenosti)
    - parametru `filter` se p�ed�v� template n�zvu souboru. Pokud se template �sp�n� vyhodnot� do tvaru `xxx=yyy` nebo `aaa=bbb=ccc`, bude soubor pou�it jen tehdy, pokud rovnosti plat�

## P��kazy
- `help` - zobraz� dostupn� p��kazy a parametry
- `metadata <files>`
- `import <files>`
- `export <files>`
