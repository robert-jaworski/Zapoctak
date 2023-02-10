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
- `files` - seznam souborù, ukonèeno jakmile se narazí na další parametr, napø. `--import-files abc.jpg def.jpg -x xyz.jpg` bude odpovídat souborùm `abc.jpg def.jpg`
    - seznam souborù mùže obsahovat název složky ukonèený `/`, resp. `\` (napø. `photos/`), ten bude odpovídat všem souborùm v dané složce (nerekurzivní)
    - pro rekurzivní procházení podadresáøù ukonèete název složky `/...`, resp. `\...`
    - seznam souborù mùže taky obsahovat intervaly souborù, specifikované pomocí `...` (napø. `img010.jpg ... img029.jpg`, `img100.jpg...`, `...foto/img100.jpg`, `../foto/img100.jpg ... ../foto/img300.jpg`)
    - odkaz na poslednì upravené soubory `@last`, popø. `@lastN` kde N je poèet operací, které nás zajímají, tedy `@last3` bude odpovídat všem souborùm, které jsme upravili v posledních tøech operacích (tedy pøikazech)

## Univerzální parametry
- `--album-dir`, `-d` - nastaví adresáø s albem, se kterým budeme pracovat. Defaultní hodnota je aktuální adresáø.
- `--verbose`, `-v` - program vypisuje více informací

## Speciální parametry
- Pøíkazy `metadata`, `import` a `export`
    - parameter `--extensions` oèekává seznam povolených typù souborù oddìlených èárkami, napø. `jpg,.png` (typy souborù mohou ale nemusí obsahovat teèku)
- Pøíkazy `import` a `export`
    - parameter `--template` obsahuje template názvu výsledných souborù. Defaultní hodnota je `{YYYY}/{MM}/{YYYY}{MM}{DD}-{hh}{mm}{ss}`.
        - Template se mùže skládat z více vlastních templatù oddìlených èárkou, použije se první, který jde.
        - Template mùže obsahovat placeholdery které se specifikují `{type:options#width}`, kde `options` a `width` jsou nepovinné. `width` specifikuje požadovanou minimální šíøku položky. `options` blíže specifikuje, co bude výsledkem nahrazení placeholderu.
        - Dostupné hodnoty typù jsou: `year`/`Y`, `YYYY`, `month`/`M`, `MM`, `day`/`D`, `DD`, `hour`/`h`, `hh`, `minute`/`m`, `mm`, `second`/`s`, `ss`, `device:name`, `device:manufacturer`, `device:model`, `noextension`/`noext`, `extension`/`ext`/`.`, `file`/`file:name`, `file:extension`/`file:ext`, `file:relativePath`/`file:relPath`/`file:rel`
        - Pokud nìkterá hodnota není dostupná (napø. `device:model`), template se nepoužije
        - Placeholdery data a èasu mùžou mít `options` následující: `exif` (defaultní), `create`, `modify`. Pokud není `options` nastaveno, použije se buï `exif` nebo `create`, podle toho, co existuje
        - Placeholdery `extension` a `noextension` umožòují filtrovat podle koncovek souborù, napø. `{YYYY}/{MM}/{YYYY}{MM}{DD}-{hh}{mm}{ss}{ext:.jpg},other/{YYYY}{MM}{DD}-{hh}{mm}{ss}`. Rozdíl je v tom, že `extension` se nahradí specifikovanou koncovkou souboru (pøesnì jak je v templatu), ale `noextension` zùstane prázdný. Samotný `{ext}` se nahradí koncovkou pùvodního souboru.
        - Pokud není specifikovaný `extension` nebo `noextension` na konec názvu se pøidá koncovka pùvodního souboru
    - parametry `--after-date` a `--before-date` umožòují filtrovat fotky pomocí data a èasu, kdy byly poøízeny
    - parametr `--time-shift` umožòuje posunout èas, kdy byly fotky poøízeny. Tato operace ale nijak nepøepisuje EXIF (kvùli hešování a lenosti)
    - parametru `filter` se pøedává template názvu souboru. Pokud se template úspìšnì vyhodnotí do tvaru `xxx=yyy` nebo `aaa=bbb=ccc`, bude soubor použit jen tehdy, pokud rovnosti platí

## Pøíkazy
- `help` - zobrazí dostupné pøíkazy a parametry
- `metadata <files>`
- `import <files>`
- `export <files>`
