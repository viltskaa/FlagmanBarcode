using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BarcodeScannerDatabaseImplement.Migrations
{
    /// <inheritdoc />
    public partial class InitCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BarcodeProducts",
                columns: table => new
                {
                    Gtin = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Filename = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BarcodeProducts", x => x.Gtin);
                });

            migrationBuilder.CreateTable(
                name: "QrStuffs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Gtin = table.Column<int>(type: "INTEGER", nullable: false),
                    Timestamp = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QrStuffs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QrStuffs_BarcodeProducts_Gtin",
                        column: x => x.Gtin,
                        principalTable: "BarcodeProducts",
                        principalColumn: "Gtin",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_QrStuffs_Gtin",
                table: "QrStuffs",
                column: "Gtin");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "QrStuffs");

            migrationBuilder.DropTable(
                name: "BarcodeProducts");
        }
    }
}
