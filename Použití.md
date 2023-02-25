# Pouití
`album <command> [parameters]`

Kadı pøíkaz má svoje parametry a nastavení, nìkterá nastavení jdou pouít na všechny pøíkazy.

Pro získaní nápovìdy spuste pøíkaz `help` nebo program spuste bez pøíkazu.
Tam by mìlo bıt vše potøebné.

(Informace v tomto souboru mohou bıt zastaralé.)

## Parametry
Parametry mají dlouhou (napø. `--album-dir`, `--verbose`) a krátkou verzi (`-d`, `-v`). Nìkteré parametry poadují další hodnotu (tøeba název souboru nebo èíslo), které následuje hned po názvu parametru. Krátké verze paramentrù jdou spojovat (`-vd`). Pokud spojíme krátké verze parametrù, které potøebují dodateènou hodnotu, musí tyto hodnoty následovat ve stejném poøadí.

## Typy parametrù
Parametry jsou ètyø typù:
- `flag` - buï je nastaven nebo ne, napø. `--verbose`
- `number` - èíslo
- `string` - text, napø. `--album-dir`
- `files` - seznam souborù, ukonèeno jakmile se narazí na další parametr, napø. `--import-files abc.jpg def.jpg -x xyz.jpg` bude odpovídat souborùm `abc.jpg def.jpg`
    - seznam souborù mùe obsahovat název sloky ukonèenı `/`, resp. `\` (napø. `photos/`), ten bude odpovídat všem souborùm v dané sloce (nerekurzivní)
    - pro rekurzivní procházení podadresáøù ukonèete název sloky `/...`, resp. `\...`
    - seznam souborù mùe taky obsahovat intervaly souborù, specifikované pomocí `...` (napø. `img010.jpg ... img029.jpg`, `img100.jpg...`, `...foto/img100.jpg`, `../foto/img100.jpg ... ../foto/img300.jpg`)
    - odkaz na poslednì upravené soubory `@last`, popø. `@lastN` kde N je poèet operací, které nás zajímají, tedy `@last3` bude odpovídat všem souborùm, které jsme upravili v posledních tøech operacích (tedy pøikazech)
    - Na Windows je moné pouít syntaxi `:\cesta\k\souborum\` pro importování ze všech diskù, které danou cestu obsahují

## Univerzální parametry
- `--album-dir`, `-d` - nastaví adresáø s albem, se kterım budeme pracovat. Defaultní hodnota je aktuální adresáø.
- `--verbose`, `-v` - program vypisuje více informací

## Speciální parametry
- Pøíkazy `metadata`, `import` a `export`
    - parameter `--extensions` oèekává seznam povolenıch typù souborù oddìlenıch èárkami, napø. `jpg,.png` (typy souborù mohou ale nemusí obsahovat teèku)
- Pøíkazy `import` a `export`
    - parameter `--template` obsahuje template názvu vıslednıch souborù. Defaultní hodnota je `{YYYY}/{MM}/{YYYY}{MM}{DD}-{hh}{mm}{ss}`.
        - Template se mùe skládat z více vlastních templatù oddìlenıch èárkou, pouije se první, kterı jde.
        - Template mùe obsahovat placeholdery které se specifikují `{type:options#width}`, kde `options` a `width` jsou nepovinné. `width` specifikuje poadovanou minimální šíøku poloky. `options` blíe specifikuje, co bude vısledkem nahrazení placeholderu.
        - Dostupné hodnoty typù jsou: `year`/`Y`, `YYYY`, `month`/`M`, `MM`, `day`/`D`, `DD`, `hour`/`h`, `hh`, `minute`/`m`, `mm`, `second`/`s`, `ss`, `device:name`, `device:manufacturer`, `device:model`, `noextension`/`noext`, `extension`/`ext`/`.`, `file`/`file:name`, `file:extension`/`file:ext`, `file:relativePath`/`file:relPath`/`file:rel`
        - Pokud nìkterá hodnota není dostupná (napø. `device:model`), template se nepouije
        - Placeholdery data a èasu mùou mít `options` následující: `exif` (defaultní), `create`, `modify`. Pokud není `options` nastaveno, pouije se buï `exif` nebo `create`, podle toho, co existuje
        - Placeholdery `extension` a `noextension` umoòují filtrovat podle koncovek souborù, napø. `{YYYY}/{MM}/{YYYY}{MM}{DD}-{hh}{mm}{ss}{ext:.jpg},other/{YYYY}{MM}{DD}-{hh}{mm}{ss}`. Rozdíl je v tom, e `extension` se nahradí specifikovanou koncovkou souboru (pøesnì jak je v templatu), ale `noextension` zùstane prázdnı. Samotnı `{ext}` se nahradí koncovkou pùvodního souboru.
        - Pokud není specifikovanı `extension` nebo `noextension` na konec názvu se pøidá koncovka pùvodního souboru
    - parametry `--after-date` a `--before-date` umoòují filtrovat fotky pomocí data a èasu, kdy byly poøízeny
    - parametr `--time-shift` umoòuje posunout èas, kdy byly fotky poøízeny. Tato operace ale nijak nepøepisuje EXIF (kvùli hešování a lenosti)
    - parametru `filter` se pøedává template názvu souboru. Pokud se template úspìšnì vyhodnotí do tvaru `xxx=yyy` nebo `aaa=bbb=ccc`, bude soubor pouit jen tehdy, pokud rovnosti platí

## Pøíkazy
- `help` - zobrazí dostupné pøíkazy a parametry
- `metadata <files>`
- `import <files>`
- `export <files>`
