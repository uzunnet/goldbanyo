using System;
using System.Text.Json;
using System.IO;
using VIZITLINK3D.Ortak.Modeller.Urunler;
using VIZITLINK3D.Ortak.Modeller;
using System.Collections.Generic;

var json = File.ReadAllText("../urunler_out.json");
var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
try {
    var response = JsonSerializer.Deserialize<Cevap<List<Urun>>>(json, opts);
    Console.WriteLine($"Success: {response?.BasariliMi}, Count: {response?.Veri?.Count}");
} catch (Exception ex) {
    Console.WriteLine($"Error: {ex.Message}");
}
