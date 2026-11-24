using System.Globalization;
using System.Net;
using System.Text;
using Microsoft.Data.Sqlite;

Console.OutputEncoding = Encoding.UTF8;
var hu = CultureInfo.GetCultureInfo("hu-HU");
var output = Path.GetFullPath(args.Length > 0 ? args[0] : "eredmenyek");
Directory.CreateDirectory(output);
try
{
    using var db = new SqliteConnection(new SqliteConnectionStringBuilder
    {
        DataSource = Path.Combine(output, "pizzeria.db"), ForeignKeys = true
    }.ToString());
    db.Open();
    db.CreateCollation("HUNGARIAN", (a, b) => hu.CompareInfo.Compare(a, b));
    using var transaction = db.BeginTransaction();
    Execute("""
        DROP TABLE IF EXISTS rendeles;
        DROP TABLE IF EXISTS pizza;
        DROP TABLE IF EXISTS cim;
        CREATE TABLE pizza(id INTEGER PRIMARY KEY, nev TEXT NOT NULL,
            meret INTEGER NOT NULL CHECK(meret>0), ar INTEGER NOT NULL CHECK(ar>=0));
        CREATE TABLE cim(id INTEGER PRIMARY KEY, nev TEXT NOT NULL,
            utca TEXT NOT NULL, hsz TEXT NOT NULL);
        CREATE TABLE rendeles(id INTEGER PRIMARY KEY,
            pizzaid INTEGER NOT NULL REFERENCES pizza(id),
            darab INTEGER NOT NULL CHECK(darab>0),
            cimid INTEGER NOT NULL REFERENCES cim(id), szallitas TEXT NOT NULL);
        """, transaction);
    Import("pizza", 4, new[] {0, 2, 3}, transaction);
    Import("cim", 4, new[] {0}, transaction);
    Import("rendeles", 5, new[] {0, 1, 2, 3}, transaction);
    foreach (var path in Directory.GetFiles(Path.Combine(AppContext.BaseDirectory, "Queries"), "*.sql"))
    {
        var name = Path.GetFileNameWithoutExtension(path);
        Execute($"DROP VIEW IF EXISTS [{name}]; CREATE VIEW [{name}] AS " + File.ReadAllText(path), transaction);
    }
    transaction.Commit();
    Console.WriteLine("1. feladat: pizzeria.db létrehozva; táblák importálva, kulcsok beállítva.");
    Console.WriteLine("2. feladat: az árak megjelenítése egész forintban történik (Ft).");
    foreach (var name in new[] {"3olcso", "4nepszeru", "5esti", "6jolfogy", "7reklam", "8osszeg"})
    {
        using var cmd = db.CreateCommand();
        cmd.CommandText = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Queries", name + ".sql"));
        using var reader = cmd.ExecuteReader();
        var headings = Enumerable.Range(0, reader.FieldCount).Select(reader.GetName).ToArray();
        var rows = new List<string[]>();
        while (reader.Read())
            rows.Add(Enumerable.Range(0, reader.FieldCount)
                .Select(i => Convert.ToString(reader.GetValue(i), CultureInfo.InvariantCulture) ?? "").ToArray());
        Console.WriteLine($"\n{name}");
        Console.WriteLine(string.Join(" | ", headings));
        foreach (var row in rows)
            Console.WriteLine(string.Join(" | ", row.Select((value, i) =>
                headings[i] == "ar" || headings[i] == "osszeg"
                    ? long.Parse(value).ToString("N0", hu) + " Ft" : value)));
        using var writer = new StreamWriter(Path.Combine(output, name + ".csv"), false, new UTF8Encoding(true));
        writer.WriteLine(string.Join(";", headings.Select(Csv)));
        foreach (var row in rows) writer.WriteLine(string.Join(";", row.Select(Csv)));
        if (name == "8osszeg") WriteReport(rows);
    }
    Console.WriteLine($"\nAz eredmények helye: {output}");

    void Execute(string sql, SqliteTransaction tx)
    {
        using var cmd = db.CreateCommand();
        cmd.Transaction = tx;
        cmd.CommandText = sql;
        cmd.ExecuteNonQuery();
    }

    void Import(string table, int count, int[] numeric, SqliteTransaction tx)
    {
        var lines = File.ReadAllLines(Path.Combine(AppContext.BaseDirectory, "Data", table + ".txt"), Encoding.UTF8);
        using var cmd = db.CreateCommand();
        cmd.Transaction = tx;
        cmd.CommandText = $"INSERT INTO {table} VALUES ({string.Join(",", Enumerable.Range(0, count).Select(i => "$p" + i))})";
        for (int i = 0; i < count; i++) cmd.Parameters.Add(new SqliteParameter("$p" + i, numeric.Contains(i) ? SqliteType.Integer : SqliteType.Text));
        for (int line = 1; line < lines.Length; line++)
        {
            if (string.IsNullOrWhiteSpace(lines[line])) continue;
            var cells = lines[line].Split('\t');
            if (cells.Length != count) throw new FormatException($"{table}.txt, {line + 1}. sor: hibás mezőszám.");
            for (int i = 0; i < count; i++)
                cmd.Parameters[i].Value = numeric.Contains(i) ? (object)int.Parse(cells[i], CultureInfo.InvariantCulture) : cells[i];
            if (table == "rendeles")
                cmd.Parameters[4].Value = TimeOnly.ParseExact(cells[4], "HH:mm:ss", CultureInfo.InvariantCulture).ToString("HH:mm:ss");
            cmd.ExecuteNonQuery();
        }
    }

    void WriteReport(List<string[]> rows)
    {
        var html = new StringBuilder("""
            <!doctype html><html lang="hu"><meta charset="utf-8">
            <title>Fizetési jegyzék</title>
            <style>body{font-family:Arial;margin:2rem;color:#222}table{border-collapse:collapse;width:100%}
            th,td{text-align:left;padding:8px;border-bottom:1px solid #ddd}td:last-child{text-align:right}
            .ido{background:#eee}tr{break-inside:avoid}@media print{body{margin:0}}</style>
            <h1>Fizetési jegyzék</h1><table><thead><tr><th>szállítási idő</th><th>név</th>
            <th>utca</th><th>házszám</th><th>összeg (Ft)</th></tr></thead><tbody>
            """);
        string? previous = null;
        foreach (var row in rows)
        {
            if (row[0] != previous)
            {
                html.Append($"<tr class='ido'><td colspan='5'>{WebUtility.HtmlEncode(row[0])}</td></tr>");
                previous = row[0];
            }
            html.Append("<tr><td></td>");
            foreach (var value in row.Skip(1)) html.Append("<td>" + WebUtility.HtmlEncode(value) + "</td>");
            html.Append("</tr>");
        }
        html.Append("</tbody></table></html>");
        File.WriteAllText(Path.Combine(output, "8osszeg.html"), html.ToString(), Encoding.UTF8);
    }
}
catch (Exception ex)
{
    Console.Error.WriteLine("Hiba: " + ex.Message);
    Environment.ExitCode = 1;
}

static string Csv(string value) => "\"" + value.Replace("\"", "\"\"") + "\"";
