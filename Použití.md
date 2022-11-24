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
- `files` - seznam soubor�, ukon�eno `-`, nap�. `--import-files abc.jpg def.jpg -`

## Univerz�ln� parametry
- `--album-dir`, `-d` - nastav� adres�� s albem, se kter�m budeme pracovat. Defaultn� hodnota je aktu�ln� adres��.
- `--verbose`, `-v` - program vypisuje v�ce informac�

## P��kazy
- `help` - zobraz� dostupn� p��kazy a parametry
- `import <files>`
