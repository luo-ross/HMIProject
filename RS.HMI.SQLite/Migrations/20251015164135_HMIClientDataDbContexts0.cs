using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RS.HMI.SQLite.Migrations
{
    /// <inheritdoc />
    public partial class HMIClientDataDbContexts0 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CommuStation",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommuStation", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ModbusCommuConfig",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    CommuStationId = table.Column<string>(type: "TEXT", nullable: false),
                    SerialPortConfigId = table.Column<string>(type: "TEXT", nullable: false),
                    DataId = table.Column<string>(type: "TEXT", nullable: false),
                    StationNumber = table.Column<byte>(type: "INTEGER", nullable: false),
                    FunctionCode = table.Column<int>(type: "INTEGER", nullable: false),
                    Address = table.Column<string>(type: "TEXT", nullable: false),
                    DataType = table.Column<int>(type: "INTEGER", nullable: false),
                    CharacterLength = table.Column<int>(type: "INTEGER", nullable: true),
                    IsStringInverse = table.Column<bool>(type: "INTEGER", nullable: true),
                    ReadWritePermission = table.Column<int>(type: "INTEGER", nullable: false),
                    DataGroup = table.Column<byte>(type: "INTEGER", nullable: false),
                    DataDescription = table.Column<string>(type: "TEXT", nullable: false),
                    ByteOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    DataValue = table.Column<double>(type: "REAL", nullable: true),
                    MinValue = table.Column<double>(type: "REAL", nullable: true),
                    MaxValue = table.Column<double>(type: "REAL", nullable: true),
                    DigitalNumber = table.Column<byte>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ModbusCommuConfig", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SerialPortConfig",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    CommuStationId = table.Column<string>(type: "TEXT", nullable: false),
                    PortName = table.Column<string>(type: "TEXT", nullable: true),
                    BaudRate = table.Column<int>(type: "INTEGER", nullable: false),
                    DataBits = table.Column<int>(type: "INTEGER", nullable: false),
                    StopBits = table.Column<int>(type: "INTEGER", nullable: false),
                    Parity = table.Column<int>(type: "INTEGER", nullable: false),
                    IsAutoConnect = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SerialPortConfig", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CommuStation");

            migrationBuilder.DropTable(
                name: "ModbusCommuConfig");

            migrationBuilder.DropTable(
                name: "SerialPortConfig");
        }
    }
}
