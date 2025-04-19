using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RS.HMIServer.DAL.Migrations
{
    /// <inheritdoc />
    public partial class RSAppMigration1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDisabled",
                table: "User",
                type: "bit",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDisabled",
                table: "User");
        }
    }
}
