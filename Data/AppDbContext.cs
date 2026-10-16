using DentusClinic.API.Enums;
using DentusClinic.API.Models;
using Microsoft.EntityFrameworkCore;

namespace DentusClinic.API.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Login> Logins { get; set; }
    public DbSet<Especialidade> Especialidades { get; set; }
    public DbSet<Funcionario> Funcionarios { get; set; }
    public DbSet<Dentista> Dentistas { get; set; }
    public DbSet<Paciente> Pacientes { get; set; }
    public DbSet<Consulta> Consultas { get; set; }
    public DbSet<Atendimento> Atendimentos { get; set; }
    public DbSet<Prontuario> Prontuarios { get; set; }
    public DbSet<Servico> Servicos { get; set; }
    public DbSet<Planos> Planos { get; set; }
    public DbSet<Dispositivo> Dispositivos { get; set; }
    public DbSet<Notificacao> Notificacoes { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Login
        modelBuilder.Entity<Login>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Email).IsRequired().HasMaxLength(150);
            e.HasIndex(x => x.Email).IsUnique();
            e.Property(x => x.Senha).IsRequired();
            e.Property(x => x.TipoAcesso).IsRequired().HasMaxLength(20);
            e.Property(x => x.PrimeiroAcesso).IsRequired().HasDefaultValue(true);
        });

        // Especialidade
        modelBuilder.Entity<Especialidade>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Nome).IsRequired().HasMaxLength(100);
            e.HasIndex(x => x.Nome).IsUnique();
        });

        // Funcionario
        modelBuilder.Entity<Funcionario>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Nome).IsRequired().HasMaxLength(150);
            e.Property(x => x.Cpf).IsRequired().HasMaxLength(14);
            e.HasIndex(x => x.Cpf).IsUnique();
            e.Property(x => x.Cargo).IsRequired().HasMaxLength(20);
            e.HasOne(x => x.Login)
             .WithOne(x => x.Funcionario)
             .HasForeignKey<Funcionario>(x => x.IdAcesso)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // Dentista
        modelBuilder.Entity<Dentista>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Nome).IsRequired().HasMaxLength(150);
            e.Property(x => x.Cpf).IsRequired().HasMaxLength(14);
            e.HasIndex(x => x.Cpf).IsUnique();
            e.Property(x => x.Cro).IsRequired().HasMaxLength(20);
            e.HasIndex(x => x.Cro).IsUnique();
            e.HasOne(x => x.Especialidade)
             .WithMany(x => x.Dentistas)
             .HasForeignKey(x => x.IdEspecialidade)
             .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Login)
             .WithOne(x => x.Dentista)
             .HasForeignKey<Dentista>(x => x.IdAcesso)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // Paciente
        modelBuilder.Entity<Paciente>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Nome).IsRequired().HasMaxLength(150);
            e.Property(x => x.Cpf).IsRequired().HasMaxLength(14);
            e.HasIndex(x => x.Cpf).IsUnique();
            e.HasOne(x => x.Login)
             .WithOne(x => x.Paciente)
             .HasForeignKey<Paciente>(x => x.IdAcesso)
             .IsRequired(false)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // Consulta
        modelBuilder.Entity<Consulta>(e =>
        {
            e.HasKey(x => x.Id);
            e.ToTable(t =>
            {
                t.HasTrigger("trg_ImpedirConflitoHorarioDentista");
                t.HasTrigger("trg_ImpedirAlteracaoConsultaRealizada");
                t.HasTrigger("trg_CancelamentoAutomaticoRecusa");
            });
            e.Property(x => x.Status).IsRequired().HasMaxLength(20);
            e.Property(x => x.StatusConfirmacao).IsRequired().HasDefaultValue(StatusConfirmacaoEnum.Pendente);
            e.HasOne(x => x.Dentista)
             .WithMany(x => x.Consultas)
             .HasForeignKey(x => x.IdDentista)
             .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Paciente)
             .WithMany(x => x.Consultas)
             .HasForeignKey(x => x.IdPaciente)
             .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Servico)
             .WithMany()
             .HasForeignKey(x => x.IdServico)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // Dispositivo
        modelBuilder.Entity<Dispositivo>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.TokenPush).IsRequired();
            e.Property(x => x.Plataforma).IsRequired().HasMaxLength(10);
            e.Property(x => x.Ativo).HasDefaultValue(true);
            e.HasOne(x => x.Paciente)
             .WithMany(x => x.Dispositivos)
             .HasForeignKey(x => x.IdPaciente)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // Notificacao
        modelBuilder.Entity<Notificacao>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.TipoNotificacao).IsRequired().HasMaxLength(100);
            e.Property(x => x.StatusEnvio).IsRequired().HasDefaultValue(StatusEnvioEnum.Pendente);
            e.Property(x => x.Tentativas).HasDefaultValue(0);
            e.HasOne(x => x.Consulta)
             .WithMany()
             .HasForeignKey(x => x.IdConsulta)
             .OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(x => new { x.IdConsulta, x.StatusEnvio });
        });

        // Atendimento
        modelBuilder.Entity<Atendimento>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasOne(x => x.Consulta)
             .WithOne(x => x.Atendimento)
             .HasForeignKey<Atendimento>(x => x.IdConsulta)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // Prontuario
        modelBuilder.Entity<Prontuario>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasOne(x => x.Paciente)
             .WithOne(x => x.Prontuario)
             .HasForeignKey<Prontuario>(x => x.IdPaciente)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // Servico
        modelBuilder.Entity<Servico>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Nome).IsRequired().HasMaxLength(100);
            e.HasIndex(x => x.Nome).IsUnique();
            e.HasOne(x => x.Especialidade)
             .WithMany()
             .HasForeignKey(x => x.IdEspecialidade)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // Planos
        modelBuilder.Entity<Planos>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Status).IsRequired().HasMaxLength(20);
            e.HasOne(x => x.Prontuario)
             .WithMany(x => x.Planos)
             .HasForeignKey(x => x.IdProntuario)
             .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Servico)
             .WithMany(x => x.Planos)
             .HasForeignKey(x => x.IdServico)
             .IsRequired(false)
             .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Dentista)
             .WithMany()
             .HasForeignKey(x => x.IdDentista)
             .IsRequired(false)
             .OnDelete(DeleteBehavior.Restrict);
            e.Property(x => x.VersaoAtual).HasDefaultValue(true);
            e.HasOne(x => x.PlanoAnterior)
             .WithMany()
             .HasForeignKey(x => x.PlanoAnteriorId)
             .IsRequired(false)
             .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
