using System;
using System.Text.Json;
using System.Collections.Generic;
using Desadoor.Ortak.Modeller.Urunler;
using Desadoor.Ortak.Modeller;

var json = System.IO.File.ReadAllText(""urunler_out.json"");
var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
try {
    var response = JsonSerializer.Deserialize<Cevap<List<Urun>>>(json, opts);
    Console.WriteLine($""Success: {response?.BasariliMi}, Count: {response?.Veri?.Count}"");
} catch (Exception ex) {
    Console.WriteLine($""Error: {ex.Message}"");
}
