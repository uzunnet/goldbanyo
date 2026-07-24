using Microsoft.EntityFrameworkCore;
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
    }
}
