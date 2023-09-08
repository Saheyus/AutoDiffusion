using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoDiffusion.Migrations
{
    /// <inheritdoc />
    public partial class InitialSnapshot : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Names",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CountryCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CountryName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Names", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Probabilities",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LastLetters = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NextLetter = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Probability = table.Column<double>(type: "float", nullable: false),
                    Discriminator = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CountryName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Probabilities", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "WordParameters",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Langue = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Categorie = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MinLetters = table.Column<int>(type: "int", nullable: false),
                    MaxLetters = table.Column<int>(type: "int", nullable: false),
                    AccentModifier = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WordParameters", x => x.ID);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Names");

            migrationBuilder.DropTable(
                name: "Probabilities");

            migrationBuilder.DropTable(
                name: "WordParameters");
        }
    }
}
