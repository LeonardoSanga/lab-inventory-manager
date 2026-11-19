using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LabRobotica.Migrations
{
    /// <inheritdoc />
    public partial class PopulaItens : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder mb)
        {
            // Dados de teste removidos: o banco de produção começa vazio.
            // (Migration mantida para não quebrar o histórico de migrations.)
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder mb)
        {
        }
    }
}
