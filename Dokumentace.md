# Konzolov� ��st
Soubory staraj�c� se o chod konzolov� aplikace - zpracov�n� argument�, spu�t�n� p��slou�n�ho p��kazu a o�et�en� chyb.

## AlbumConsole/Program.cs
Vstupn� bod programu. Na�te config file, naparasuje argumenty a spust� p��slu�n� p��kaz. O�et�uje nezachycen� chyby.

## AlbumConsole/ArgumentsProcessor.cs
Star� se o parsov�n� argument� (pou��v� `AlbumConsole/CLIArguments.cs`). Definuje t��du pro `CommandArguments` pro snadnou pr�ci s naparsovan�mi argumenty. (Nap��klad snadno umo�n� zkontrolovat jestli je nastaveno `--verbose` nebo z argument� zjist�, jak� je adres�� aktu�ln�ho alba.)

## AlbumConsole/CLIArguments.cs
Definuje obecn� zp�sob deklarov�n� o�ek�v�n�ch argument�. Star� se o vlastn� extrakci a parsov�n� argument�.

## AlbumConsole/ConfigFileReader.cs
Definuje t��dy a funkce pro na�ten� config souboru a pou�it� config souboru pro nastaven� defaultn�ch hodnot argument.

## AlbumConsole/Commands.cs
Definuje mo�n� p��kazy, jejich o�ek�van� argumenty, jejich popisy a funkce, kter� se maj� zavolat pro jednotliv� p��kazy.

# U�ite�n� v�ci z AlbumLibrary/

## AlbumLibrary/ErrorHandler.cs
Definuje t��dy, kter� se staraj� o p��pady, kdy nastane chyba (nap�. soubor neexistuje nebo nejde zkop�rovat). Umo��uje chybu vypsat na konzoli, ulo�it do seznamu nebo vyhodit v�jimku.

## AlbumLibrary/Logger.cs
Definuje t��dy, kter� umo��uj� pr�b�n� vypisovat informace o pr�b�hu p��kazu.

## AlbumLibrary/FileSystemProvider.cs
Definuje t��dy a funkce pro pr�ci se soubory. Implementuje operace UNDO a REDO - tj. wrapper, kter� automaticky zaznamen�v� operace proveden� se soubory a ukl�d� tyto operace do soubor�, aby �ly vr�tit zp�t.
T��du pro pr�ci se soubory definuje i `AlbumLibrary/FileIndex.cs` a to t��du, kter� ukl�d� informace o souborech a jejich EXIF datech.

# Soubory staraj�c� se o zpracov�n� soubor�
N�sleduj�c� soubory se (v tomto po�ad�) pou��vaj� p�i zpracov�n� soubor� (import, export atd.).
Jedn� se o soubory kde je implementov�na vlastn� logika zpracov�v�n� soubor�, zat�mco `AlbumConsole/` definuje r�zn� zp�soby jak k t�to funkcionalit� p�istupovat.

## AlbumLibrary/ImportFilePathProvider.cs
Definuje t��dy a funkce, kter� se staraj� z�sk�n� cest ke v�em soubor�m, kter� maj� b�t zpracov�ny. Star� se jen o cesty zdrojov�ch soubor�.
Obsahuje n�kolik jednoduch�ch provider� - jeden soubor, jeden adres�� (ne)rekurzivn�, jeden adres�� s omezen�m na n�zev souboru a naposledy pou�it� soubory.

## AlbumLibrary/FileInfoProvider.cs
Definuje t��dy, kter� zji��uj� informace o dan�ch souborech, nap��klad �ten�m EXIFu.
Je zde provider kter� na�te data z EXIFu, potom jeden, kter� na�te data z EXIFu ale ingoruje datum a �as, a taky jeden, kter� k z�skan�m informac�m p�ipoj� relativn� cestu souboru k adres��i alba.

## AlbumLibrary/FileFilter.cs
Definuje t��dy, kter� umo��uj� soubory filtrovat na z�klad�ch informac� o nich z�skan�ch.
Nach�z� se zde filtry `BeforeDateFilter` a `AfterDateFilter`, kter� kontroluj� datum a �as po��zen� fotky a taky `TemplateFilter`, kter� souboru p�i�ad� jm�no pomoc� vhodn�ho `FileNameProvider`u a toto jm�no porovn� se zadanou hodnotou.
D�le se zde nach�z� i filtr `TimeShiftFilter`, kter� se star� o posouv�n� �as� z�skan�ch z EXIFu.

## AlbumLibrary/FileNameProvider.cs
Definuje t��dy, kter� zpracov�van�m soubor�m na z�klad� informac� o nich (z�skan�ch pomoc� `IFileNameProvider`u) p�i�azuj� jm�na (vlastn� relativn� cesty v r�mci alba).

## AlbumLibrary/ImportListProvider.cs
Vyu��v� t��d p�edchoz�ch soubor� k z�sk�n� seznamu zdrojov�ch a c�lov�ch cest soubor� ke zpracov�n�.

## AlbumLibrary/DuplicateResolver.cs
Definuje t��dy, kter� reaguj� na p��pad, kdy dva soubory maj� stejn� jm�no. V�b�rem spr�vn� t��dy m��eme tyto soubory p�esko�it, p�epsat, p�idat suffix ke jm�nu nebo spo��tat hash soubor� a porovnat.

## AlbumLibrary/FileImporter.cs
Definuje t��dy, kter� se staraj� o vlastn� zpracov�n� soubor�. Toto zpracov�n� m��e b�t zkop�rov�n�, p�esunut� nebo smaz�n� (pop�. smaz�n�, pokud c�lov� soubor neexistuje - vyu�ito p�i backupu). Spolupracuje s `AlbumLibrary/DuplicateResolver.cs`.

# Testy
Slo�ka `AlbumTest/` obsahuje testy, kter� kontroluj� n�kter� funkcionality, jestli funguj� spr�vn�. Ne v�echny funkcionality maj� testy a ne v�echny testy funguj� na Linuxu.
