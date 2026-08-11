using ArkahBusiness.API.Data;
using ArkahBusiness.API.Models;
using ArkahBusiness.API.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using System.Net;

namespace ArkahBusiness.Tests.Services;

public class WhatsAppServiceTests
{
    [Fact]
    public async Task EnviarLembreteAsync_DeveEnviarMensagemComTextoDinamicoDaEmpresa()
    {
        var nomeCliente = "Marcela";
        var telefone = "5533984662125";
        var dataHora = "06/08/2026 às 19:00";
        var empresaId = 1;

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: "BancoDeTeste_WhatsApp")
            .Options;

        using var context = new AppDbContext(options);

        context.Empresas.Add(new Empresa
        {
            Id = empresaId,
            NomeFantasia = "Clínica Teste",
            Cnpj = "00000000000000",
            MensagemLembreteWhatsApp = "Oi {{Nome}}! É da Clínica Teste. Seu horário é {{Data}}."
        });
        await context.SaveChangesAsync();

        var mockConfig = new Mock<IConfiguration>();
        mockConfig.Setup(c => c["EvolutionAPI:BaseUrl"]).Returns("http://localhost:8080");
        mockConfig.Setup(c => c["EvolutionAPI:ApiKey"]).Returns("senha123");
        mockConfig.Setup(c => c["EvolutionAPI:InstanceName"]).Returns("arkah_teste");

        var mockLogger = new Mock<ILogger<WhatsAppService>>();

        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.OK });

        var httpClient = new HttpClient(mockHttpMessageHandler.Object);
        var mockHttpClientFactory = new Mock<IHttpClientFactory>();
        mockHttpClientFactory.Setup(_ => _.CreateClient("EvolutionAPI")).Returns(httpClient);

        var whatsAppService = new WhatsAppService(
            mockHttpClientFactory.Object,
            mockConfig.Object,
            mockLogger.Object,
            context);

        await whatsAppService.EnviarLembreteAsync(empresaId, nomeCliente, telefone, dataHora);

        mockHttpMessageHandler.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req =>
                req.Method == HttpMethod.Post &&
                req.RequestUri!.ToString() == "http://localhost:8080/message/sendText/arkah_teste"
            ),
            ItExpr.IsAny<CancellationToken>()
        );
    }
}