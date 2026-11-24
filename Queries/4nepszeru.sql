WITH osszes AS (
 SELECT p.id, p.nev, p.meret, SUM(r.darab) AS darabszam
 FROM pizza p JOIN rendeles r ON r.pizzaid=p.id
 GROUP BY p.id, p.nev, p.meret
)
SELECT nev, meret, darabszam FROM osszes
WHERE darabszam=(SELECT MAX(darabszam) FROM osszes)
ORDER BY nev COLLATE HUNGARIAN, meret;