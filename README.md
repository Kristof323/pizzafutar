# Pizzafutár – C# konzolalkalmazás

A 2018. május 14-i emelt informatika adatbázis-feladat C# / SQLite átdolgozása.

## Indítás
.NET 8 SDK szükséges; az első csomag-visszaállításhoz internetkapcsolat kell.

```sh
dotnet run --project Pizzafutar.csproj
```

Egyéni kimeneti mappa:

```sh
dotnet run --project Pizzafutar.csproj -- ./eredmenyek
```

## Feladatok
1. A csatolt Pizzafutar.sql INSERT adataiból előállított UTF-8, tabulátoros Data/*.txt fájlokat importálja a pizzeria.db adatbázisba. Az eredeti azonosítókat megtartja, elsődleges és idegen kulcsokat hoz létre. A házszám szöveg, a szállítási idő normalizált HH:mm:ss szöveg (SQLite).
2. Az ár szám marad az adatbázisban; konzolon egész értékként, Ft egységgel jelenik meg. Ez megjelenítési formázás, nem Access-mezőtulajdonság.
3–8. A Queries/*.sql lekérdezéseket a kért néven nézetként is elmenti, végrehajtja, konzolra és pontosvesszős CSV-fájlokba írja.
4. Holtverseny esetén minden legtöbbet rendelt pizzát felsorol; a darabokat összegzi, nem a rendelési sorokat számolja.
5. A 18:00:00 és 19:00:00 határok is beletartoznak.
6. Az eredeti IN részlekérdezést egészíti ki.
7. A feladat szó szerinti „egy címre” feltétele alapján időpont + utca + házszám szerint adja össze a darabokat. Ha ugyanott, ugyanakkor több személy rendel, a jogosult címhez mindegyik érintett név megjelenik. Az eredmény nem üdítőital-darabszám.
8. A fizetendőt időpont + személyazonosító szerint összegzi, ezért az azonos nevű emberek nem olvadnak össze. A 8osszeg.html címe Fizetési jegyzék; időpontonként csoportosítva a minta mezősorrendjét követi. Böngészőből nyomtatható.

A program nem Access-adatbázist és nem Access-jelentésobjektumot készít; a feladat konzolalkalmazásos átdolgozása. A célmappában meglévő három adattáblát és hat feladatnézetet minden futás újratölti. Saját adatokhoz külön kimeneti mappa használható.

## Ellenőrzés
A lekérdezéseket a csatolt adatokkal Python SQLite környezetben ellenőriztem: 117 pizza, 388 cím, 414 rendelési sor; az idegen kulcsok rendben, a jelentés összege megegyezik a rendelések összértékével. A 4nepszeru eredménye: Gyros, 22, 15.
A C# projekt ebben a környezetben .NET SDK hiányában nem lett lefordítva vagy futtatva. A magyar rendezés a C# alkalmazásban hu-HU összehasonlítást használ; a Python-ellenőrzés nem hitelesíti ezt a rendezést.
