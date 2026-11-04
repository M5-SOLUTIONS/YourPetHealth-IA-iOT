using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YourPetHealth.Migrations
{
    public partial class MigrateToYpSchemaAndAuth : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(name: "T_RESPONSAVEIS", newName: "YP_T_RESPONSAVEIS");
            migrationBuilder.RenameTable(name: "T_VETERINARIOS", newName: "YP_T_VETERINARIOS");
            migrationBuilder.RenameTable(name: "T_PETS", newName: "YP_T_PETS");
            migrationBuilder.RenameTable(name: "T_CONSULTAS", newName: "YP_T_CONSULTAS");
            migrationBuilder.RenameTable(name: "T_HISTORICO_CLINICO", newName: "YP_T_HISTORICO_CLINICO");

            migrationBuilder.RenameColumn(name: "SEXO", table: "YP_T_PETS", newName: "SEXO_PET");

            migrationBuilder.DropColumn(name: "SENHA", table: "YP_T_RESPONSAVEIS");
            migrationBuilder.DropColumn(name: "SENHA", table: "YP_T_VETERINARIOS");

           
            migrationBuilder.Sql("ALTER TABLE YP_T_CONSULTAS ADD CONSTRAINT CK_CONSULTA_TIPO CHECK (TIPO IN ('ROTINA','VACINACAO','EMERGENCIA','RETORNO','EXAME'))");

            migrationBuilder.CreateTable(
                name: "YP_T_USUARIOS",
                columns: table => new
                {
                    ID = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    EMAIL = table.Column<string>(type: "NVARCHAR2(150)", maxLength: 150, nullable: false),
                    SENHA_HASH = table.Column<string>(type: "NVARCHAR2(255)", maxLength: 255, nullable: false),
                    PERFIL = table.Column<string>(type: "NVARCHAR2(20)", maxLength: 20, nullable: false),
                    ATIVO = table.Column<bool>(type: "NUMBER(1)", nullable: false, defaultValue: true),
                    CRIADO_EM = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false, defaultValueSql: "SYSTIMESTAMP"),
                    RESPONSAVEL_ID = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    VETERINARIO_ID = table.Column<int>(type: "NUMBER(10)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_YP_T_USUARIOS", x => x.ID);

                    table.ForeignKey(
                        name: "FK_USUARIO_RESPONSAVEL",
                        column: x => x.RESPONSAVEL_ID,
                        principalTable: "YP_T_RESPONSAVEIS",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);

                    table.ForeignKey(
                        name: "FK_USUARIO_VETERINARIO",
                        column: x => x.VETERINARIO_ID,
                        principalTable: "YP_T_VETERINARIOS",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.Sql(
                "ALTER TABLE YP_T_USUARIOS ADD CONSTRAINT CK_USUARIO_PERFIL " +
                "CHECK (PERFIL IN ('RESPONSAVEL','VETERINARIO','ADMIN'))");

            migrationBuilder.Sql(
                "ALTER TABLE YP_T_USUARIOS ADD CONSTRAINT CK_USUARIO_VINCULO CHECK (" +
                "(PERFIL = 'RESPONSAVEL' AND RESPONSAVEL_ID IS NOT NULL AND VETERINARIO_ID IS NULL) " +
                "OR (PERFIL = 'VETERINARIO' AND VETERINARIO_ID IS NOT NULL AND RESPONSAVEL_ID IS NULL) " +
                "OR (PERFIL = 'ADMIN' AND RESPONSAVEL_ID IS NULL AND VETERINARIO_ID IS NULL))");

            migrationBuilder.CreateIndex(
                name: "UK_USUARIO_EMAIL",
                table: "YP_T_USUARIOS",
                column: "EMAIL",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UK_USUARIO_RESPONSAVEL",
                table: "YP_T_USUARIOS",
                column: "RESPONSAVEL_ID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UK_USUARIO_VETERINARIO",
                table: "YP_T_USUARIOS",
                column: "VETERINARIO_ID",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "YP_T_USUARIOS");

            migrationBuilder.DropCheckConstraint(name: "CK_CONSULTA_TIPO", table: "YP_T_CONSULTAS");

            migrationBuilder.AddColumn<string>(
                name: "SENHA",
                table: "YP_T_VETERINARIOS",
                type: "NVARCHAR2(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SENHA",
                table: "YP_T_RESPONSAVEIS",
                type: "NVARCHAR2(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.RenameColumn(name: "SEXO_PET", table: "YP_T_PETS", newName: "SEXO");

            migrationBuilder.RenameTable(name: "YP_T_HISTORICO_CLINICO", newName: "T_HISTORICO_CLINICO");
            migrationBuilder.RenameTable(name: "YP_T_CONSULTAS", newName: "T_CONSULTAS");
            migrationBuilder.RenameTable(name: "YP_T_PETS", newName: "T_PETS");
            migrationBuilder.RenameTable(name: "YP_T_VETERINARIOS", newName: "T_VETERINARIOS");
            migrationBuilder.RenameTable(name: "YP_T_RESPONSAVEIS", newName: "T_RESPONSAVEIS");
        }
    }
}
