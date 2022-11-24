# Použití
`album <command> [parameters]`

Každý pøíkaz má svoje parametry a nastavení, nìkterá nastavení jdou použít na všechny pøíkazy.

## Parametry
Parametry mají dlouhou (napø. `--album-dir`, `--verbose`) a krátkou verzi (`-d`, `-v`). Nìkteré parametry poø´žadují další hodnotu (tøeba název souboru nebo èíslo), které následuje hned po názvu parametru. Krátké verze paramentrù jdou spojovat (`-vd`). Pokud spojíme krátké verze parametrù, které potøebují dodateènou hodnotu, musí tyto hodnoty následovat ve stejném poøadí.

## Typy parametrù
Parametry jsou ètyø typù:
- `flag` - buï je nastaven nebo ne, napø. `--verbose`
- `number` - èíslo
- `string` - text, napø. `--album-dir`
- `files` - seznam souborù, ukonèeno `-`, napø. `--import-files abc.jpg def.jpg -`

## Univerzální parametry
- `--album-dir`, `-d` - nastaví adresáø s albem, se kterým budeme pracovat. Defaultní hodnota je aktuální adresáø.
- `--verbose`, `-v` - program vypisuje více informací

## Pøíkazy
- `help` - zobrazí dostupné pøíkazy a parametry
- `import <files>`
