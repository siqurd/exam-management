using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Exam.Api.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Lessons",
                columns: table => new
                {
                    Code = table.Column<string>(type: "char(3)", maxLength: 3, nullable: false, collation: "Latin1_General_100_CI_AS_SC_UTF8"),
                    Name = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false, collation: "Latin1_General_100_CI_AS_SC_UTF8"),
                    Class = table.Column<decimal>(type: "decimal(2,0)", precision: 2, scale: 0, nullable: false),
                    TeacherFirstName = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false, collation: "Latin1_General_100_CI_AS_SC_UTF8"),
                    TeacherLastName = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false, collation: "Latin1_General_100_CI_AS_SC_UTF8")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Lessons", x => x.Code);
                    table.CheckConstraint("CK_Lessons_Class", "[Class] BETWEEN 1 AND 99");
                    table.CheckConstraint("CK_Lessons_Code", "LEN([Code]) = 3");
                });

            migrationBuilder.CreateTable(
                name: "Students",
                columns: table => new
                {
                    Number = table.Column<decimal>(type: "decimal(5,0)", precision: 5, scale: 0, nullable: false),
                    FirstName = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false, collation: "Latin1_General_100_CI_AS_SC_UTF8"),
                    LastName = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false, collation: "Latin1_General_100_CI_AS_SC_UTF8"),
                    Class = table.Column<decimal>(type: "decimal(2,0)", precision: 2, scale: 0, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Students", x => x.Number);
                    table.CheckConstraint("CK_Students_Class", "[Class] BETWEEN 1 AND 99");
                    table.CheckConstraint("CK_Students_Number", "[Number] BETWEEN 1 AND 99999");
                });

            migrationBuilder.CreateTable(
                name: "Exams",
                columns: table => new
                {
                    LessonCode = table.Column<string>(type: "char(3)", maxLength: 3, nullable: false, collation: "Latin1_General_100_CI_AS_SC_UTF8"),
                    StudentNumber = table.Column<decimal>(type: "decimal(5,0)", precision: 5, scale: 0, nullable: false),
                    ExamDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Grade = table.Column<decimal>(type: "decimal(1,0)", precision: 1, scale: 0, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Exams", x => new { x.LessonCode, x.StudentNumber, x.ExamDate });
                    table.CheckConstraint("CK_Exams_Grade", "[Grade] BETWEEN 0 AND 9");
                    table.ForeignKey(
                        name: "FK_Exams_Lessons_LessonCode",
                        column: x => x.LessonCode,
                        principalTable: "Lessons",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Exams_Students_StudentNumber",
                        column: x => x.StudentNumber,
                        principalTable: "Students",
                        principalColumn: "Number",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Exams_StudentNumber",
                table: "Exams",
                column: "StudentNumber");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Exams");

            migrationBuilder.DropTable(
                name: "Lessons");

            migrationBuilder.DropTable(
                name: "Students");
        }
    }
}
