# Použití
`album <command> [parameters]`

Každý příkaz má svoje parametry a nastavení, některá nastavení jdou použít na všechny příkazy.

Pro získaní nápovědy spusťte příkaz `help` nebo program spusťte bez příkazu.
Tam by mělo být vše potřebné.

(Informace v tomto souboru mohou být zastaralé.)

## Parametry
Parametry mají dlouhou (např. `--album-dir`, `--verbose`) a krátkou verzi (`-d`, `-v`). Některé parametry požadují další hodnotu (třeba název souboru nebo číslo), které následuje hned po názvu parametru. Krátké verze paramentrů jdou spojovat (`-vd`). Pokud spojíme krátké verze parametrů, které potřebují dodatečnou hodnotu, musí tyto hodnoty následovat ve stejném pořadí.

## Typy parametrů
Parametry jsou čtyř typů:
- `flag` - buď je nastaven nebo ne, např. `--verbose`
- `number` - číslo
- `string` - text, např. `--album-dir`
- `files` - seznam souborů, ukončeno jakmile se narazí na další parametr, např. `--import-files abc.jpg def.jpg -x xyz.jpg` bude odpovídat souborům `abc.jpg def.jpg`
    - seznam souborů může obsahovat název složky ukončený `/`, resp. `\` (např. `photos/`), ten bude odpovídat všem souborům v dané složce (nerekurzivní)
    - pro rekurzivní procházení podadresářů ukončete název složky `/...`, resp. `\...`
    - seznam souborů může taky obsahovat intervaly souborů, specifikované pomocí `...` (např. `img010.jpg ... img029.jpg`, `img100.jpg...`, `...foto/img100.jpg`, `../foto/img100.jpg ... ../foto/img300.jpg`)
    - odkaz na posledně upravené soubory `@last`, popř. `@lastN` kde N je počet operací, které nás zajímají, tedy `@last3` bude odpovídat všem souborům, které jsme upravili v posledních třech operacích (tedy přikazech)
    - Na Windows je možné použít syntaxi `:\cesta\k\souborum\` pro importování ze všech disků, které danou cestu obsahují

## Univerzální parametry
- `--album-dir`, `-d` - nastaví adresář s albem, se kterým budeme pracovat. Defaultní hodnota je aktuální adresář.
- `--verbose`, `-v` - program vypisuje více informací

## Speciální parametry
- Příkazy `metadata`, `import` a `export`
    - parameter `--extensions` očekává seznam povolených typů souborů oddělených čárkami, např. `jpg,.png` (typy souborů mohou ale nemusí obsahovat tečku)
- Příkazy `import` a `export`
    - parameter `--template` obsahuje template názvu výsledných souborů. Defaultní hodnota je `{YYYY}/{MM}/{YYYY}{MM}{DD}-{hh}{mm}{ss}`.
        - Template se může skládat z více vlastních templatů oddělených čárkou, použije se první, který jde.
        - Template může obsahovat placeholdery které se specifikují `{type:options#width}`, kde `options` a `width` jsou nepovinné. `width` specifikuje požadovanou minimální šířku položky. `options` blíže specifikuje, co bude výsledkem nahrazení placeholderu.
        - Dostupné hodnoty typů jsou: `year`/`Y`, `YYYY`, `month`/`M`, `MM`, `day`/`D`, `DD`, `hour`/`h`, `hh`, `minute`/`m`, `mm`, `second`/`s`, `ss`, `device:name`, `device:manufacturer`, `device:model`, `noextension`/`noext`, `extension`/`ext`/`.`, `file`/`file:name`, `file:extension`/`file:ext`, `file:relativePath`/`file:relPath`/`file:rel`
        - Pokud některá hodnota není dostupná (např. `device:model`), template se nepoužije
        - Placeholdery data a času můžou mít `options` následující: `exif` (defaultní), `create`, `modify`. Pokud není `options` nastaveno, použije se buď `exif` nebo `create`, podle toho, co existuje
        - Placeholdery `extension` a `noextension` umožňují filtrovat podle koncovek souborů, např. `{YYYY}/{MM}/{YYYY}{MM}{DD}-{hh}{mm}{ss}{ext:.jpg},other/{YYYY}{MM}{DD}-{hh}{mm}{ss}`. Rozdíl je v tom, že `extension` se nahradí specifikovanou koncovkou souboru (přesně jak je v templatu), ale `noextension` zůstane prázdný. Samotný `{ext}` se nahradí koncovkou původního souboru.
        - Pokud není specifikovaný `extension` nebo `noextension` na konec názvu se přidá koncovka původního souboru
    - parametry `--after-date` a `--before-date` umožňují filtrovat fotky pomocí data a času, kdy byly pořízeny
    - parametr `--time-shift` umožňuje posunout čas, kdy byly fotky pořízeny. Tato operace ale nijak nepřepisuje EXIF (kvůli hešování a lenosti)
    - parametru `filter` se předává template názvu souboru. Pokud se template úspěšně vyhodnotí do tvaru `xxx=yyy` nebo `aaa=bbb=ccc`, bude soubor použit jen tehdy, pokud rovnosti platí

## Příkazy
- `help` - zobrazí dostupné příkazy a parametry
- `metadata <files>`
- `import <files>`
- `export <files>`
