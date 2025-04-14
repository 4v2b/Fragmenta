using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Fragmenta.Dal.Migrations
{
    /// <inheritdoc />
    public partial class SeedAttachmentTypes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "AttachmentTypes",
                columns: new[] { "Id", "ParentId", "Value" },
                values: new object[,]
                {
                    { 1L, null, "Any" },
                    { 11L, 1L, "Documents" },
                    { 12L, 1L, "Images" },
                    { 13L, 1L, "Audio" },
                    { 14L, 1L, "Video" },
                    { 15L, 1L, "Archives" },
                    { 16L, 1L, "Code" },
                    { 17L, 1L, "Data" },
                    { 18L, 1L, "Design" },
                    { 101L, 11L, ".txt" },
                    { 102L, 11L, ".pdf" },
                    { 103L, 11L, ".doc" },
                    { 104L, 11L, ".docx" },
                    { 105L, 11L, ".xls" },
                    { 106L, 11L, ".xlsx" },
                    { 107L, 11L, ".ppt" },
                    { 108L, 11L, ".pptx" },
                    { 109L, 11L, ".odt" },
                    { 110L, 11L, ".ods" },
                    { 111L, 11L, ".odp" },
                    { 112L, 11L, ".md" },
                    { 113L, 11L, ".rtf" },
                    { 201L, 12L, ".jpg" },
                    { 202L, 12L, ".jpeg" },
                    { 203L, 12L, ".png" },
                    { 204L, 12L, ".gif" },
                    { 205L, 12L, ".bmp" },
                    { 206L, 12L, ".svg" },
                    { 207L, 12L, ".webp" },
                    { 208L, 12L, ".tiff" },
                    { 209L, 12L, ".ico" },
                    { 301L, 13L, ".mp3" },
                    { 302L, 13L, ".wav" },
                    { 303L, 13L, ".ogg" },
                    { 304L, 13L, ".flac" },
                    { 305L, 13L, ".m4a" },
                    { 306L, 13L, ".aac" },
                    { 401L, 14L, ".mp4" },
                    { 402L, 14L, ".avi" },
                    { 403L, 14L, ".mkv" },
                    { 404L, 14L, ".mov" },
                    { 405L, 14L, ".wmv" },
                    { 406L, 14L, ".webm" },
                    { 501L, 15L, ".zip" },
                    { 502L, 15L, ".rar" },
                    { 503L, 15L, ".7z" },
                    { 504L, 15L, ".tar" },
                    { 505L, 15L, ".gz" },
                    { 601L, 16L, ".cs" },
                    { 602L, 16L, ".js" },
                    { 603L, 16L, ".ts" },
                    { 604L, 16L, ".html" },
                    { 605L, 16L, ".css" },
                    { 606L, 16L, ".json" },
                    { 607L, 16L, ".xml" },
                    { 608L, 16L, ".py" },
                    { 609L, 16L, ".java" },
                    { 610L, 16L, ".php" },
                    { 611L, 16L, ".sql" },
                    { 612L, 16L, ".sh" },
                    { 701L, 17L, ".csv" },
                    { 702L, 17L, ".xls" },
                    { 703L, 17L, ".xlsx" },
                    { 704L, 17L, ".xml" },
                    { 705L, 17L, ".json" },
                    { 706L, 17L, ".sqlite" },
                    { 707L, 17L, ".accdb" },
                    { 801L, 18L, ".psd" },
                    { 802L, 18L, ".ai" },
                    { 803L, 18L, ".xd" },
                    { 804L, 18L, ".fig" },
                    { 805L, 18L, ".sketch" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AttachmentTypes",
                keyColumn: "Id",
                keyValue: 101L);

            migrationBuilder.DeleteData(
                table: "AttachmentTypes",
                keyColumn: "Id",
                keyValue: 102L);

            migrationBuilder.DeleteData(
                table: "AttachmentTypes",
                keyColumn: "Id",
                keyValue: 103L);

            migrationBuilder.DeleteData(
                table: "AttachmentTypes",
                keyColumn: "Id",
                keyValue: 104L);

            migrationBuilder.DeleteData(
                table: "AttachmentTypes",
                keyColumn: "Id",
                keyValue: 105L);

            migrationBuilder.DeleteData(
                table: "AttachmentTypes",
                keyColumn: "Id",
                keyValue: 106L);

            migrationBuilder.DeleteData(
                table: "AttachmentTypes",
                keyColumn: "Id",
                keyValue: 107L);

            migrationBuilder.DeleteData(
                table: "AttachmentTypes",
                keyColumn: "Id",
                keyValue: 108L);

            migrationBuilder.DeleteData(
                table: "AttachmentTypes",
                keyColumn: "Id",
                keyValue: 109L);

            migrationBuilder.DeleteData(
                table: "AttachmentTypes",
                keyColumn: "Id",
                keyValue: 110L);

            migrationBuilder.DeleteData(
                table: "AttachmentTypes",
                keyColumn: "Id",
                keyValue: 111L);

            migrationBuilder.DeleteData(
                table: "AttachmentTypes",
                keyColumn: "Id",
                keyValue: 112L);

            migrationBuilder.DeleteData(
                table: "AttachmentTypes",
                keyColumn: "Id",
                keyValue: 113L);

            migrationBuilder.DeleteData(
                table: "AttachmentTypes",
                keyColumn: "Id",
                keyValue: 201L);

            migrationBuilder.DeleteData(
                table: "AttachmentTypes",
                keyColumn: "Id",
                keyValue: 202L);

            migrationBuilder.DeleteData(
                table: "AttachmentTypes",
                keyColumn: "Id",
                keyValue: 203L);

            migrationBuilder.DeleteData(
                table: "AttachmentTypes",
                keyColumn: "Id",
                keyValue: 204L);

            migrationBuilder.DeleteData(
                table: "AttachmentTypes",
                keyColumn: "Id",
                keyValue: 205L);

            migrationBuilder.DeleteData(
                table: "AttachmentTypes",
                keyColumn: "Id",
                keyValue: 206L);

            migrationBuilder.DeleteData(
                table: "AttachmentTypes",
                keyColumn: "Id",
                keyValue: 207L);

            migrationBuilder.DeleteData(
                table: "AttachmentTypes",
                keyColumn: "Id",
                keyValue: 208L);

            migrationBuilder.DeleteData(
                table: "AttachmentTypes",
                keyColumn: "Id",
                keyValue: 209L);

            migrationBuilder.DeleteData(
                table: "AttachmentTypes",
                keyColumn: "Id",
                keyValue: 301L);

            migrationBuilder.DeleteData(
                table: "AttachmentTypes",
                keyColumn: "Id",
                keyValue: 302L);

            migrationBuilder.DeleteData(
                table: "AttachmentTypes",
                keyColumn: "Id",
                keyValue: 303L);

            migrationBuilder.DeleteData(
                table: "AttachmentTypes",
                keyColumn: "Id",
                keyValue: 304L);

            migrationBuilder.DeleteData(
                table: "AttachmentTypes",
                keyColumn: "Id",
                keyValue: 305L);

            migrationBuilder.DeleteData(
                table: "AttachmentTypes",
                keyColumn: "Id",
                keyValue: 306L);

            migrationBuilder.DeleteData(
                table: "AttachmentTypes",
                keyColumn: "Id",
                keyValue: 401L);

            migrationBuilder.DeleteData(
                table: "AttachmentTypes",
                keyColumn: "Id",
                keyValue: 402L);

            migrationBuilder.DeleteData(
                table: "AttachmentTypes",
                keyColumn: "Id",
                keyValue: 403L);

            migrationBuilder.DeleteData(
                table: "AttachmentTypes",
                keyColumn: "Id",
                keyValue: 404L);

            migrationBuilder.DeleteData(
                table: "AttachmentTypes",
                keyColumn: "Id",
                keyValue: 405L);

            migrationBuilder.DeleteData(
                table: "AttachmentTypes",
                keyColumn: "Id",
                keyValue: 406L);

            migrationBuilder.DeleteData(
                table: "AttachmentTypes",
                keyColumn: "Id",
                keyValue: 501L);

            migrationBuilder.DeleteData(
                table: "AttachmentTypes",
                keyColumn: "Id",
                keyValue: 502L);

            migrationBuilder.DeleteData(
                table: "AttachmentTypes",
                keyColumn: "Id",
                keyValue: 503L);

            migrationBuilder.DeleteData(
                table: "AttachmentTypes",
                keyColumn: "Id",
                keyValue: 504L);

            migrationBuilder.DeleteData(
                table: "AttachmentTypes",
                keyColumn: "Id",
                keyValue: 505L);

            migrationBuilder.DeleteData(
                table: "AttachmentTypes",
                keyColumn: "Id",
                keyValue: 601L);

            migrationBuilder.DeleteData(
                table: "AttachmentTypes",
                keyColumn: "Id",
                keyValue: 602L);

            migrationBuilder.DeleteData(
                table: "AttachmentTypes",
                keyColumn: "Id",
                keyValue: 603L);

            migrationBuilder.DeleteData(
                table: "AttachmentTypes",
                keyColumn: "Id",
                keyValue: 604L);

            migrationBuilder.DeleteData(
                table: "AttachmentTypes",
                keyColumn: "Id",
                keyValue: 605L);

            migrationBuilder.DeleteData(
                table: "AttachmentTypes",
                keyColumn: "Id",
                keyValue: 606L);

            migrationBuilder.DeleteData(
                table: "AttachmentTypes",
                keyColumn: "Id",
                keyValue: 607L);

            migrationBuilder.DeleteData(
                table: "AttachmentTypes",
                keyColumn: "Id",
                keyValue: 608L);

            migrationBuilder.DeleteData(
                table: "AttachmentTypes",
                keyColumn: "Id",
                keyValue: 609L);

            migrationBuilder.DeleteData(
                table: "AttachmentTypes",
                keyColumn: "Id",
                keyValue: 610L);

            migrationBuilder.DeleteData(
                table: "AttachmentTypes",
                keyColumn: "Id",
                keyValue: 611L);

            migrationBuilder.DeleteData(
                table: "AttachmentTypes",
                keyColumn: "Id",
                keyValue: 612L);

            migrationBuilder.DeleteData(
                table: "AttachmentTypes",
                keyColumn: "Id",
                keyValue: 701L);

            migrationBuilder.DeleteData(
                table: "AttachmentTypes",
                keyColumn: "Id",
                keyValue: 702L);

            migrationBuilder.DeleteData(
                table: "AttachmentTypes",
                keyColumn: "Id",
                keyValue: 703L);

            migrationBuilder.DeleteData(
                table: "AttachmentTypes",
                keyColumn: "Id",
                keyValue: 704L);

            migrationBuilder.DeleteData(
                table: "AttachmentTypes",
                keyColumn: "Id",
                keyValue: 705L);

            migrationBuilder.DeleteData(
                table: "AttachmentTypes",
                keyColumn: "Id",
                keyValue: 706L);

            migrationBuilder.DeleteData(
                table: "AttachmentTypes",
                keyColumn: "Id",
                keyValue: 707L);

            migrationBuilder.DeleteData(
                table: "AttachmentTypes",
                keyColumn: "Id",
                keyValue: 801L);

            migrationBuilder.DeleteData(
                table: "AttachmentTypes",
                keyColumn: "Id",
                keyValue: 802L);

            migrationBuilder.DeleteData(
                table: "AttachmentTypes",
                keyColumn: "Id",
                keyValue: 803L);

            migrationBuilder.DeleteData(
                table: "AttachmentTypes",
                keyColumn: "Id",
                keyValue: 804L);

            migrationBuilder.DeleteData(
                table: "AttachmentTypes",
                keyColumn: "Id",
                keyValue: 805L);

            migrationBuilder.DeleteData(
                table: "AttachmentTypes",
                keyColumn: "Id",
                keyValue: 11L);

            migrationBuilder.DeleteData(
                table: "AttachmentTypes",
                keyColumn: "Id",
                keyValue: 12L);

            migrationBuilder.DeleteData(
                table: "AttachmentTypes",
                keyColumn: "Id",
                keyValue: 13L);

            migrationBuilder.DeleteData(
                table: "AttachmentTypes",
                keyColumn: "Id",
                keyValue: 14L);

            migrationBuilder.DeleteData(
                table: "AttachmentTypes",
                keyColumn: "Id",
                keyValue: 15L);

            migrationBuilder.DeleteData(
                table: "AttachmentTypes",
                keyColumn: "Id",
                keyValue: 16L);

            migrationBuilder.DeleteData(
                table: "AttachmentTypes",
                keyColumn: "Id",
                keyValue: 17L);

            migrationBuilder.DeleteData(
                table: "AttachmentTypes",
                keyColumn: "Id",
                keyValue: 18L);

            migrationBuilder.DeleteData(
                table: "AttachmentTypes",
                keyColumn: "Id",
                keyValue: 1L);
        }
    }
}
