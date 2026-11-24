SELECT p.nev, p.meret, SUM(r.darab) AS darabszam
FROM pizza p JOIN rendeles r ON r.pizzaid=p.id
WHERE r.szallitas BETWEEN '18:00:00' AND '19:00:00'
GROUP BY p.nev, p.meret
ORDER BY p.nev COLLATE HUNGARIAN, p.meret;