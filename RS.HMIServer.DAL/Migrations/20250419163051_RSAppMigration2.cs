using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RS.HMIServer.DAL.Migrations
{
    /// <inheritdoc />
    public partial class RSAppMigration2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Test",
                table: "LogOn",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Test",
                table: "LogOn");
        }
    }
}
