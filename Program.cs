using Microsoft.Data.Sqlite;
var db = @"I:\desedoorweb\Desadoor.Api\desadoor.db";
using var c = new SqliteConnection($"Data Source={db}");
c.Open();
var cmd = c.CreateCommand();
cmd.CommandText = "SELECT name FROM sqlite_master WHERE type='table' ORDER BY name";
using var r = cmd.ExecuteReader();
while (r.Read()) Console.WriteLine(r.GetString(0));