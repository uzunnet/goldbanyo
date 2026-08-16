using Microsoft.EntityFrameworkCore;
using VizitLink3D.Konfigurator.Api.Moduller.Kategoriler.Modeller;
using VizitLink3D.Konfigurator.Api.Moduller.Kimlik.Modeller;
using VizitLink3D.Konfigurator.Api.Moduller.Modeller.Modeller;

namespace VizitLink3D.Konfigurator.Api.VeriTabani;

public class KonfiguratorDbContext : DbContext
{
    public KonfiguratorDbContext(DbContextOptions<KonfiguratorDbContext> secenekler)
        : base(secenekler) { }

    public DbSet<KonfiguratorKullanicisi> Kullanicilar => Set<KonfiguratorKullanicisi>();
    public DbSet<UcBoyutModel> UcBoyutModeller => Set<UcBoyutModel>();
    public DbSet<UcBoyutModelParcasi> UcBoyutModelParcalari => Set<UcBoyutModelParcasi>();
    public DbSet<SifreSifirlamaIstegi> SifreSifirlamaIstekleri => Set<SifreSifirlamaIstegi>();
    public DbSet<Kategori> Kategoriler => Set<Kategori>();
    public DbSet<ParcaKategorisi> ParcaKategorileri => Set<ParcaKategorisi>();
    public DbSet<KonfiguratorFirma> Firmalar => Set<KonfiguratorFirma>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<KonfiguratorKullanicisi>(entity =>
        {
            entity.HasIndex(k => k.KullaniciAdi).IsUnique();
            entity.HasIndex(k => k.Eposta).IsUnique();
            entity.HasQueryFilter(k => !k.SilindiMi);
        });

        modelBuilder.Entity<UcBoyutModel>(entity =>
        {
            entity.HasIndex(m => m.Slug).IsUnique();
            entity.HasQueryFilter(m => !m.SilindiMi);
            entity.Property(m => m.Ad).HasMaxLength(200);
            entity.Property(m => m.Slug).HasMaxLength(200);
            entity.Property(m => m.Aciklama).HasMaxLength(2000);
        });

        modelBuilder.Entity<UcBoyutModelParcasi>(entity =>
        {
            entity.HasIndex(p => new { p.ModelId, p.MeshAdi }).IsUnique();
            entity.HasQueryFilter(p => !p.SilindiMi);
            entity.Property(p => p.MeshAdi).HasMaxLength(300);
            entity.Property(p => p.GorunenAd).HasMaxLength(300);
            entity.Property(p => p.VarsayilanRenk).HasMaxLength(9);
            entity.Property(p => p.VarsayilanMalzeme).HasMaxLength(100);

            entity.HasOne(p => p.Model)
                .WithMany()
                .HasForeignKey(p => p.ModelId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<SifreSifirlamaIstegi>(entity =>
        {
            entity.HasIndex(i => i.TokenHash);
            entity.HasIndex(i => i.KullaniciId);
            entity.HasQueryFilter(i => !i.SilindiMi);

            entity.HasOne(i => i.Kullanici)
                .WithMany()
                .HasForeignKey(i => i.KullaniciId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Kategori agaci (Kategoriler tablosu — 20260723021905 migration)
        modelBuilder.Entity<Kategori>(entity =>
        {
            entity.HasQueryFilter(k => !k.SilindiMi);
            entity.Property(k => k.Ad).HasMaxLength(100);
            entity.Property(k => k.Slug).HasMaxLength(150);
            entity.Property(k => k.Aciklama).HasMaxLength(500);
            entity.HasIndex(k => k.Slug).IsUnique();

            entity.HasOne(k => k.UstKategori)
                .WithMany(k => k.AltKategoriler)
                .HasForeignKey(k => k.UstKategoriId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Firma bazli parca kategorisi (20260722231927 migration)
        modelBuilder.Entity<ParcaKategorisi>(entity =>
        {
            entity.HasQueryFilter(p => !p.SilindiMi);
            entity.Property(p => p.Ad).HasMaxLength(200);
            entity.Property(p => p.Aciklama).HasMaxLength(500);
            entity.HasIndex(p => p.FirmaId);
            entity.HasIndex(p => new { p.FirmaId, p.Ad }).IsUnique();
        });

        // Konfigurator firmasi / tenant (20260722231927 migration)
        modelBuilder.Entity<KonfiguratorFirma>(entity =>
        {
            entity.HasQueryFilter(f => !f.SilindiMi);
            entity.Property(f => f.Ad).HasMaxLength(200);
            entity.Property(f => f.Slug).HasMaxLength(100);
            entity.Property(f => f.Domain).HasMaxLength(300);
            entity.Property(f => f.YedekDomain).HasMaxLength(300);
            entity.HasIndex(f => f.Domain);
            entity.HasIndex(f => f.Slug).IsUnique();
        });

        // UcBoyutModel -> FirmaId / KategoriId (tenant + kategori)
        modelBuilder.Entity<UcBoyutModel>(entity =>
        {
            entity.HasIndex(m => m.FirmaId);
            entity.HasIndex(m => m.KategoriId);

            entity.HasOne(m => m.Kategori)
                .WithMany()
                .HasForeignKey(m => m.KategoriId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // UcBoyutModelParcasi -> FirmaId / ParcaKategoriId (tenant + kategori)
        modelBuilder.Entity<UcBoyutModelParcasi>(entity =>
        {
            entity.HasIndex(p => p.FirmaId);
            entity.HasIndex(p => p.ParcaKategoriId);

            entity.HasOne(p => p.ParcaKategori)
                .WithMany()
                .HasForeignKey(p => p.ParcaKategoriId)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }
}
