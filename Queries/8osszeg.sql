SELECT r.szallitas, c.nev, c.utca, c.hsz, SUM(r.darab*p.ar) AS osszeg
FROM rendeles r JOIN cim c ON c.id=r.cimid JOIN pizza p ON p.id=r.pizzaid
GROUP BY r.szallitas, c.id, c.nev, c.utca, c.hsz
ORDER BY r.szallitas, c.nev COLLATE HUNGARIAN, c.id;