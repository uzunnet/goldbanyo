using MediatR;
using VizitLink3D.Ortak.Modeller;

namespace VizitLink3D.Api.Moduller.Konfigurasyon.Komutlar;

/// <summary>
/// SuperAdmin tarafından 3D model sürümünü onaylama komutu.
/// Onaylanan model public konfigüratörde görünür hale gelir.
/// </summary>
public record ModelOnaylaKomutu(int ModelId) : IRequest<Cevap<bool>>;
