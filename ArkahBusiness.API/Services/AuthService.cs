using ArkahBusiness.API.Data;
using ArkahBusiness.API.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ArkahBusiness.API.Services;

public interface IAuthService
{
    Task<LoginResponse?> LoginAsync(LoginRequest request);
}

public class AuthService : IAuthService
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthService> _logger; 

    public AuthService(AppDbContext context, IConfiguration configuration, ILogger<AuthService> logger)
    {
        _context = context;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<LoginResponse?> LoginAsync(LoginRequest request)
    {
        var usuario = await _context.Usuarios
            .Include(u => u.Empresa)
            .FirstOrDefaultAsync(u => u.Email == request.Email);

        if (usuario == null)
        {
            _logger.LogWarning("Tentativa de login falhou. Usuário não encontrado para o email: {Email}", request.Email);
            return null;
        }

        bool senhaValida = BCrypt.Net.BCrypt.Verify(request.Senha, usuario.SenhaHash);

        if (!senhaValida)
        {
            _logger.LogWarning("Tentativa de login falhou. Senha inválida para o email: {Email}", request.Email);
            return null;
        }

        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_configuration["JwtSettings:SecretKey"]!);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                new Claim(ClaimTypes.Email, usuario.Email),
                new Claim("EmpresaId", usuario.EmpresaId.ToString())
            }),
            Expires = DateTime.UtcNow.AddHours(8),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);

        _logger.LogInformation("Login realizado com sucesso. Usuário: {Email}, EmpresaId: {EmpresaId}", usuario.Email, usuario.EmpresaId);

        return new LoginResponse
        {
            Token = tokenHandler.WriteToken(token),
            Nome = usuario.Nome,
            EmpresaId = usuario.EmpresaId
        };
    }
}