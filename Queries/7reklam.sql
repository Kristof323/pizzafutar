WITH ajandek AS (
 SELECT r.szallitas, c.utca, c.hsz
 FROM rendeles r JOIN cim c ON c.id=r.cimid
 GROUP BY r.szallitas, c.utca, c.hsz HAVING SUM(r.darab)>3
)
SELECT DISTINCT r.szallitas, c.nev, c.utca, c.hsz
FROM rendeles r JOIN cim c ON c.id=r.cimid
JOIN ajandek a ON a.szallitas=r.szallitas AND a.utca=c.utca AND a.hsz=c.hsz
ORDER BY r.szallitas, c.utca COLLATE HUNGARIAN, c.hsz, c.nev COLLATE HUNGARIAN;