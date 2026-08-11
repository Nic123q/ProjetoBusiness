using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ArkahBusiness.API.Migrations
{
    /// <inheritdoc />
    public partial class AddMensagemLembreteEmpresa : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MensagemLembreteWhatsApp",
                table: "Empresas",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MensagemLembreteWhatsApp",
                table: "Empresas");
        }
    }
}
