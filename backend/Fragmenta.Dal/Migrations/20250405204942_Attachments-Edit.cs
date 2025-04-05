using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fragmenta.Dal.Migrations
{
    /// <inheritdoc />
    public partial class AttachmentsEdit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Value",
                table: "Attachments",
                newName: "OriginalName");

            migrationBuilder.AlterColumn<long>(
                name: "ParentId",
                table: "AttachmentTypes",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AddColumn<string>(
                name: "FileName",
                table: "Attachments",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FileName",
                table: "Attachments");

            migrationBuilder.RenameColumn(
                name: "OriginalName",
                table: "Attachments",
                newName: "Value");

            migrationBuilder.AlterColumn<long>(
                name: "ParentId",
                table: "AttachmentTypes",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);
        }
    }
}
