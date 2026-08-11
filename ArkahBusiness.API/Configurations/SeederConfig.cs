using ArkahBusiness.API.Data;
using ArkahBusiness.API.Models;
using Microsoft.EntityFrameworkCore;

namespace ArkahBusiness.API.Configurations;

public static class SeederConfig
{
    public static void UseDataSeeder(this IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        context.Database.Migrate();

        if (!context.Empresas.Any())
        {
            var empresa = new Empresa
            {
                NomeFantasia = "Clínica Arkah Teste",
                Cnpj = "00.000.000/0001-00",
                Ativo = true,
                DataCadastro = DateTime.UtcNow
            };

            context.Empresas.Add(empresa);
            context.SaveChanges(); 

            var usuario = new Usuario
            {
                Nome = "Administrador Sistema",
                Email = "admin@arkah.com",
                SenhaHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                EmpresaId = empresa.Id 
            };

            context.Usuarios.Add(usuario);
            context.SaveChanges();
        }
    }
}