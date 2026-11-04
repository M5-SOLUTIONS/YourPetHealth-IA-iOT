using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YourPetHealth.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "T_RESPONSAVEIS",
                columns: table => new
                {
                    ID = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    NOME = table.Column<string>(type: "VARCHAR2(100)", maxLength: 100, nullable: false),
                    EMAIL = table.Column<string>(type: "VARCHAR2(150)", maxLength: 150, nullable: false),
                    SENHA = table.Column<string>(type: "VARCHAR2(255)", maxLength: 255, nullable: false),
                    TELEFONE = table.Column<string>(type: "VARCHAR2(20)", maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_T_RESPONSAVEIS", x => x.ID);
                    table.UniqueConstraint("UQ_RESPONSAVEL_EMAIL", x => x.EMAIL);
                });

            migrationBuilder.CreateTable(
                name: "T_VETERINARIOS",
                columns: table => new
                {
                    ID = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    NOME = table.Column<string>(type: "VARCHAR2(100)", maxLength: 100, nullable: false),
                    EMAIL = table.Column<string>(type: "VARCHAR2(150)", maxLength: 150, nullable: false),
                    SENHA = table.Column<string>(type: "VARCHAR2(255)", maxLength: 255, nullable: false),
                    TELEFONE = table.Column<string>(type: "VARCHAR2(20)", maxLength: 20, nullable: true),
                    CRMV = table.Column<string>(type: "VARCHAR2(30)", maxLength: 30, nullable: false),
                    ESPECIALIDADE = table.Column<string>(type: "VARCHAR2(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_T_VETERINARIOS", x => x.ID);
                    table.UniqueConstraint("UQ_VETERINARIO_EMAIL", x => x.EMAIL);
                    table.UniqueConstraint("UQ_VETERINARIO_CRMV", x => x.CRMV);
                });

            migrationBuilder.CreateTable(
                name: "T_PETS",
                columns: table => new
                {
                    ID = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    RESPONSAVEL_ID = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    NOME = table.Column<string>(type: "VARCHAR2(100)", maxLength: 100, nullable: false),
                    RACA = table.Column<string>(type: "VARCHAR2(100)", maxLength: 100, nullable: true),
                    IDADE = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    PESO = table.Column<decimal>(type: "NUMBER(5,2)", nullable: true),
                    SEXO = table.Column<string>(type: "VARCHAR2(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_T_PETS", x => x.ID);
                    table.ForeignKey(
                        name: "FK_PET_RESPONSAVEL",
                        column: x => x.RESPONSAVEL_ID,
                        principalTable: "T_RESPONSAVEIS",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            
            migrationBuilder.Sql("ALTER TABLE T_PETS ADD CONSTRAINT CK_PET_SEXO CHECK (SEXO IN ('MACHO','FEMEA'))");

            migrationBuilder.CreateTable(
                name: "T_CONSULTAS",
                columns: table => new
                {
                    ID = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    PET_ID = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    VETERINARIO_ID = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    TIPO = table.Column<string>(type: "VARCHAR2(100)", maxLength: 100, nullable: false),
                    DESCRICAO = table.Column<string>(type: "VARCHAR2(1000)", maxLength: 1000, nullable: true),
                    DATA = table.Column<DateTime>(type: "DATE", nullable: false),
                    OBSERVACOES = table.Column<string>(type: "VARCHAR2(1000)", maxLength: 1000, nullable: true),
                    STATUS = table.Column<string>(type: "VARCHAR2(30)", maxLength: 30, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_T_CONSULTAS", x => x.ID);
                    table.ForeignKey(
                        name: "FK_CONSULTA_PET",
                        column: x => x.PET_ID,
                        principalTable: "T_PETS",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CONSULTA_VETERINARIO",
                        column: x => x.VETERINARIO_ID,
                        principalTable: "T_VETERINARIOS",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.Sql("ALTER TABLE T_CONSULTAS ADD CONSTRAINT CK_CONSULTA_STATUS CHECK (STATUS IN ('AGENDADA','REALIZADA','CANCELADA'))");

            migrationBuilder.CreateTable(
                name: "T_HISTORICO_CLINICO",
                columns: table => new
                {
                    ID = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    PET_ID = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    TIPO = table.Column<string>(type: "VARCHAR2(50)", maxLength: 50, nullable: false),
                    DESCRICAO = table.Column<string>(type: "VARCHAR2(1000)", maxLength: 1000, nullable: true),
                    DATA = table.Column<DateTime>(type: "DATE", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_T_HISTORICO_CLINICO", x => x.ID);
                    table.ForeignKey(
                        name: "FK_HISTORICO_PET",
                        column: x => x.PET_ID,
                        principalTable: "T_PETS",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.Sql("ALTER TABLE T_HISTORICO_CLINICO ADD CONSTRAINT CK_HISTORICO_TIPO CHECK (TIPO IN ('CONSULTA'))");

            migrationBuilder.CreateIndex("IX_T_PETS_RESPONSAVEL_ID", "T_PETS", "RESPONSAVEL_ID");
            migrationBuilder.CreateIndex("IX_T_CONSULTAS_PET_ID", "T_CONSULTAS", "PET_ID");
            migrationBuilder.CreateIndex("IX_T_CONSULTAS_VETERINARIO_ID", "T_CONSULTAS", "VETERINARIO_ID");
            migrationBuilder.CreateIndex("IX_T_HISTORICO_PET_ID", "T_HISTORICO_CLINICO", "PET_ID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "T_HISTORICO_CLINICO");
            migrationBuilder.DropTable(name: "T_CONSULTAS");
            migrationBuilder.DropTable(name: "T_PETS");
            migrationBuilder.DropTable(name: "T_VETERINARIOS");
            migrationBuilder.DropTable(name: "T_RESPONSAVEIS");
        }
    }
}
