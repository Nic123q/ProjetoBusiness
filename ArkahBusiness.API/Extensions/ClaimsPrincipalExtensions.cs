using System.Security.Claims;

namespace ArkahBusiness.API.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static int GetEmpresaId(this ClaimsPrincipal user)
    {
        var claim = user.FindFirst("EmpresaId");

        if (claim == null)
            throw new UnauthorizedAccessException("Token inválido ou sem EmpresaId.");

        return int.Parse(claim.Value);
    }
}