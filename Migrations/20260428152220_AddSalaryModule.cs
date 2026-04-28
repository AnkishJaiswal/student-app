using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace student_app.Migrations
{
    /// <inheritdoc />
    public partial class AddSalaryModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Students_Email",
                table: "Students");

            migrationBuilder.AlterColumn<string>(
                name: "Grade",
                table: "Students",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Students",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.Sql("""
                IF OBJECT_ID(N'[Teachers]', N'U') IS NULL
                BEGIN
                    CREATE TABLE [Teachers] (
                        [Id] int NOT NULL IDENTITY,
                        [FirstName] nvarchar(100) NOT NULL,
                        [LastName] nvarchar(100) NOT NULL,
                        [Email] nvarchar(200) NULL,
                        [Age] int NOT NULL,
                        [Subject] nvarchar(100) NULL,
                        [CreatedAt] datetime2 NOT NULL,
                        CONSTRAINT [PK_Teachers] PRIMARY KEY ([Id])
                    );
                END
                """);

            migrationBuilder.CreateTable(
                name: "Salaries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TeacherId = table.Column<int>(type: "int", nullable: false),
                    BasicSalary = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Allowances = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Deductions = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    NetSalary = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    SalaryMonth = table.Column<int>(type: "int", nullable: false),
                    SalaryYear = table.Column<int>(type: "int", nullable: false),
                    PaymentDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Salaries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Salaries_Teachers_TeacherId",
                        column: x => x.TeacherId,
                        principalTable: "Teachers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Students_Email",
                table: "Students",
                column: "Email",
                unique: true,
                filter: "[Email] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Salaries_TeacherId_SalaryMonth_SalaryYear",
                table: "Salaries",
                columns: new[] { "TeacherId", "SalaryMonth", "SalaryYear" },
                unique: true);

            migrationBuilder.Sql("""
                IF NOT EXISTS (
                    SELECT 1
                    FROM sys.indexes
                    WHERE name = N'IX_Teachers_Email'
                        AND object_id = OBJECT_ID(N'[Teachers]')
                )
                BEGIN
                    CREATE UNIQUE INDEX [IX_Teachers_Email]
                    ON [Teachers] ([Email])
                    WHERE [Email] IS NOT NULL;
                END
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Salaries");

            migrationBuilder.Sql("""
                IF OBJECT_ID(N'[Teachers]', N'U') IS NOT NULL
                    AND NOT EXISTS (SELECT 1 FROM [Teachers])
                BEGIN
                    DROP TABLE [Teachers];
                END
                """);

            migrationBuilder.DropIndex(
                name: "IX_Students_Email",
                table: "Students");

            migrationBuilder.AlterColumn<string>(
                name: "Grade",
                table: "Students",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Students",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Students_Email",
                table: "Students",
                column: "Email",
                unique: true);
        }
    }
}
