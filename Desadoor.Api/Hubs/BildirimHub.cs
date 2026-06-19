using Microsoft.AspNetCore.SignalR;

namespace Desadoor.Api.Hubs;

public class BildirimHub : Hub
{
    public async Task AdminBildirimGonder(string baslik, string mesaj, string tur = "bilgi")
    {
        await Clients.All.SendAsync("BildirimGeldi", new
        {
            Baslik = baslik,
            Mesaj = mesaj,
            Tur = tur,
            Zaman = DateTime.UtcNow
        });
    }
}
