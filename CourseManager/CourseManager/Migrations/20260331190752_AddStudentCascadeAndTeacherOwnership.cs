using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CourseManager.Migrations
{
    /// <inheritdoc />
    public partial class AddStudentCascadeAndTeacherOwnership : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Students",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByTeacherId",
                table: "Students",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Students_CreatedByTeacherId",
                table: "Students",
                column: "CreatedByTeacherId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentAssignmentStatuses_CourseParticipantId",
                table: "StudentAssignmentStatuses",
                column: "CourseParticipantId");

            migrationBuilder.CreateIndex(
                name: "IX_RuleOccurrences_PersonId",
                table: "RuleOccurrences",
                column: "PersonId");

            migrationBuilder.AddForeignKey(
                name: "FK_RuleOccurrences_Students_PersonId",
                table: "RuleOccurrences",
                column: "PersonId",
                principalTable: "Students",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_StudentAssignmentStatuses_Students_CourseParticipantId",
                table: "StudentAssignmentStatuses",
                column: "CourseParticipantId",
                principalTable: "Students",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Students_Teachers_CreatedByTeacherId",
                table: "Students",
                column: "CreatedByTeacherId",
                principalTable: "Teachers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RuleOccurrences_Students_PersonId",
                table: "RuleOccurrences");

            migrationBuilder.DropForeignKey(
                name: "FK_StudentAssignmentStatuses_Students_CourseParticipantId",
                table: "StudentAssignmentStatuses");

            migrationBuilder.DropForeignKey(
                name: "FK_Students_Teachers_CreatedByTeacherId",
                table: "Students");

            migrationBuilder.DropIndex(
                name: "IX_Students_CreatedByTeacherId",
                table: "Students");

            migrationBuilder.DropIndex(
                name: "IX_StudentAssignmentStatuses_CourseParticipantId",
                table: "StudentAssignmentStatuses");

            migrationBuilder.DropIndex(
                name: "IX_RuleOccurrences_PersonId",
                table: "RuleOccurrences");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "CreatedByTeacherId",
                table: "Students");
        }
    }
}
