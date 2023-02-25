# Konzolová èást
Soubory starající se o chod konzolové aplikace - zpracování argumentù, spuštìní pøísloušného pøíkazu a ošetøení chyb.

## AlbumConsole/Program.cs
Vstupní bod programu. Naète config file, naparasuje argumenty a spustí pøíslušnı pøíkaz. Ošetøuje nezachycené chyby.

## AlbumConsole/ArgumentsProcessor.cs
Stará se o parsování argumentù (pouívá `AlbumConsole/CLIArguments.cs`). Definuje tøídu pro `CommandArguments` pro snadnou práci s naparsovanımi argumenty. (Napøíklad snadno umoní zkontrolovat jestli je nastaveno `--verbose` nebo z argumentù zjistí, jakı je adresáø aktuálního alba.)

## AlbumConsole/CLIArguments.cs
Definuje obecnı zpùsob deklarování oèekávánıch argumentù. Stará se o vlastní extrakci a parsování argumentù.

## AlbumConsole/ConfigFileReader.cs
Definuje tøídy a funkce pro naètení config souboru a pouití config souboru pro nastavení defaultních hodnot argument.

## AlbumConsole/Commands.cs
Definuje moné pøíkazy, jejich oèekávané argumenty, jejich popisy a funkce, které se mají zavolat pro jednotlivé pøíkazy.

# Uiteèné vìci z AlbumLibrary/

## AlbumLibrary/ErrorHandler.cs
Definuje tøídy, které se starají o pøípady, kdy nastane chyba (napø. soubor neexistuje nebo nejde zkopírovat). Umoòuje chybu vypsat na konzoli, uloit do seznamu nebo vyhodit vıjimku.

## AlbumLibrary/Logger.cs
Definuje tøídy, které umoòují prùbìnì vypisovat informace o prùbìhu pøíkazu.

## AlbumLibrary/FileSystemProvider.cs
Definuje tøídy a funkce pro práci se soubory. Implementuje operace UNDO a REDO - tj. wrapper, kterı automaticky zaznamenává operace provedené se soubory a ukládá tyto operace do souborù, aby šly vrátit zpìt.
Tøídu pro práci se soubory definuje i `AlbumLibrary/FileIndex.cs` a to tøídu, která ukládá informace o souborech a jejich EXIF datech.

# Soubory starající se o zpracování souborù
Následující soubory se (v tomto poøadí) pouívají pøi zpracování souborù (import, export atd.).
Jedná se o soubory kde je implementována vlastní logika zpracovávání souborù, zatímco `AlbumConsole/` definuje rùzné zpùsoby jak k této funkcionalitì pøistupovat.

## AlbumLibrary/ImportFilePathProvider.cs
Definuje tøídy a funkce, které se starají získání cest ke všem souborùm, které mají bıt zpracovány. Stará se jen o cesty zdrojovıch souborù.
Obsahuje nìkolik jednoduchıch providerù - jeden soubor, jeden adresáø (ne)rekurzivnì, jeden adresáø s omezením na název souboru a naposledy pouité soubory.

## AlbumLibrary/FileInfoProvider.cs
Definuje tøídy, které zjišují informace o danıch souborech, napøíklad ètením EXIFu.
Je zde provider kterı naète data z EXIFu, potom jeden, kterı naète data z EXIFu ale ingoruje datum a èas, a taky jeden, kterı k získanım informacím pøipojí relativní cestu souboru k adresáøi alba.

## AlbumLibrary/FileFilter.cs
Definuje tøídy, které umoòují soubory filtrovat na základìch informací o nich získanıch.
Nachází se zde filtry `BeforeDateFilter` a `AfterDateFilter`, které kontrolují datum a èas poøízení fotky a taky `TemplateFilter`, kterı souboru pøiøadí jméno pomocí vhodného `FileNameProvider`u a toto jméno porovná se zadanou hodnotou.
Dále se zde nachází i filtr `TimeShiftFilter`, kterı se stará o posouvání èasù získanıch z EXIFu.

## AlbumLibrary/FileNameProvider.cs
Definuje tøídy, které zpracovávanım souborùm na základì informací o nich (získanıch pomocí `IFileNameProvider`u) pøiøazují jména (vlastnì relativní cesty v rámci alba).

## AlbumLibrary/ImportListProvider.cs
Vyuívá tøíd pøedchozích souborù k získání seznamu zdrojovıch a cílovıch cest souborù ke zpracování.

## AlbumLibrary/DuplicateResolver.cs
Definuje tøídy, které reagují na pøípad, kdy dva soubory mají stejné jméno. Vıbìrem správné tøídy mùeme tyto soubory pøeskoèit, pøepsat, pøidat suffix ke jménu nebo spoèítat hash souborù a porovnat.

## AlbumLibrary/FileImporter.cs
Definuje tøídy, které se starají o vlastní zpracování souborù. Toto zpracování mùe bıt zkopírování, pøesunutí nebo smazání (popø. smazání, pokud cílovı soubor neexistuje - vyuito pøi backupu). Spolupracuje s `AlbumLibrary/DuplicateResolver.cs`.

# Testy
Sloka `AlbumTest/` obsahuje testy, které kontrolují nìkteré funkcionality, jestli fungují správnì. Ne všechny funkcionality mají testy a ne všechny testy fungují na Linuxu.
