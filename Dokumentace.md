# Konzolová část
Soubory starající se o chod konzolové aplikace - zpracování argumentů, spuštění přísloušného příkazu a ošetření chyb.

## AlbumConsole/Program.cs
Vstupní bod programu. Načte config file, naparasuje argumenty a spustí příslušný příkaz. Ošetřuje nezachycené chyby.

## AlbumConsole/ArgumentsProcessor.cs
Stará se o parsování argumentů (používá `AlbumConsole/CLIArguments.cs`). Definuje třídu pro `CommandArguments` pro snadnou práci s naparsovanými argumenty. (Například snadno umožní zkontrolovat jestli je nastaveno `--verbose` nebo z argumentů zjistí, jaký je adresář aktuálního alba.)

## AlbumConsole/CLIArguments.cs
Definuje obecný způsob deklarování očekáváných argumentů. Stará se o vlastní extrakci a parsování argumentů.

## AlbumConsole/ConfigFileReader.cs
Definuje třídy a funkce pro načtení config souboru a použití config souboru pro nastavení defaultních hodnot argument.

## AlbumConsole/Commands.cs
Definuje možné příkazy, jejich očekávané argumenty, jejich popisy a funkce, které se mají zavolat pro jednotlivé příkazy.

# Užitečné věci z AlbumLibrary/

## AlbumLibrary/ErrorHandler.cs
Definuje třídy, které se starají o případy, kdy nastane chyba (např. soubor neexistuje nebo nejde zkopírovat). Umožňuje chybu vypsat na konzoli, uložit do seznamu nebo vyhodit výjimku.

## AlbumLibrary/Logger.cs
Definuje třídy, které umožňují průběžně vypisovat informace o průběhu příkazu.

## AlbumLibrary/FileSystemProvider.cs
Definuje třídy a funkce pro práci se soubory. Implementuje operace UNDO a REDO - tj. wrapper, který automaticky zaznamenává operace provedené se soubory a ukládá tyto operace do souborů, aby šly vrátit zpět.
Třídu pro práci se soubory definuje i `AlbumLibrary/FileIndex.cs` a to třídu, která ukládá informace o souborech a jejich EXIF datech.

# Soubory starající se o zpracování souborů
Následující soubory se (v tomto pořadí) používají při zpracování souborů (import, export atd.).
Jedná se o soubory kde je implementována vlastní logika zpracovávání souborů, zatímco `AlbumConsole/` definuje různé způsoby jak k této funkcionalitě přistupovat.

## AlbumLibrary/ImportFilePathProvider.cs
Definuje třídy a funkce, které se starají získání cest ke všem souborům, které mají být zpracovány. Stará se jen o cesty zdrojových souborů.
Obsahuje několik jednoduchých providerů - jeden soubor, jeden adresář (ne)rekurzivně, jeden adresář s omezením na název souboru a naposledy použité soubory.

## AlbumLibrary/FileInfoProvider.cs
Definuje třídy, které zjišťují informace o daných souborech, například čtením EXIFu.
Je zde provider který načte data z EXIFu, potom jeden, který načte data z EXIFu ale ingoruje datum a čas, a taky jeden, který k získaným informacím připojí relativní cestu souboru k adresáři alba.

## AlbumLibrary/FileFilter.cs
Definuje třídy, které umožňují soubory filtrovat na základěch informací o nich získaných.
Nachází se zde filtry `BeforeDateFilter` a `AfterDateFilter`, které kontrolují datum a čas pořízení fotky a taky `TemplateFilter`, který souboru přiřadí jméno pomocí vhodného `FileNameProvider`u a toto jméno porovná se zadanou hodnotou.
Dále se zde nachází i filtr `TimeShiftFilter`, který se stará o posouvání časů získaných z EXIFu.

## AlbumLibrary/FileNameProvider.cs
Definuje třídy, které zpracovávaným souborům na základě informací o nich (získaných pomocí `IFileNameProvider`u) přiřazují jména (vlastně relativní cesty v rámci alba).

## AlbumLibrary/ImportListProvider.cs
Využívá tříd předchozích souborů k získání seznamu zdrojových a cílových cest souborů ke zpracování.

## AlbumLibrary/DuplicateResolver.cs
Definuje třídy, které reagují na případ, kdy dva soubory mají stejné jméno. Výběrem správné třídy můžeme tyto soubory přeskočit, přepsat, přidat suffix ke jménu nebo spočítat hash souborů a porovnat.

## AlbumLibrary/FileImporter.cs
Definuje třídy, které se starají o vlastní zpracování souborů. Toto zpracování může být zkopírování, přesunutí nebo smazání (popř. smazání, pokud cílový soubor neexistuje - využito při backupu). Spolupracuje s `AlbumLibrary/DuplicateResolver.cs`.

# Testy
Složka `AlbumTest/` obsahuje testy, které kontrolují některé funkcionality, jestli fungují správně. Ne všechny funkcionality mají testy a ne všechny testy fungují na Linuxu.
