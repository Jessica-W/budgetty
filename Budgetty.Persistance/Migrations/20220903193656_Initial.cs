using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Budgetty.Persistance.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "BankAccounts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BankAccounts", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "FinancialsSnapshots",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    UnallocatedIncomeInPennies = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FinancialsSnapshots", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "SequenceNumbers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    SequenceNo = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SequenceNumbers", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "SnapshotLocks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    LockedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SnapshotLocks", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "BudgetaryPools",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Type = table.Column<int>(type: "int", nullable: false),
                    BankAccountId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BudgetaryPools", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BudgetaryPools_BankAccounts_BankAccountId",
                        column: x => x.BankAccountId,
                        principalTable: "BankAccounts",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "BankAccountSnapShot",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    BankAccountId = table.Column<int>(type: "int", nullable: false),
                    BalanceInPennies = table.Column<int>(type: "int", nullable: false),
                    FinancialsSnapshotId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BankAccountSnapShot", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BankAccountSnapShot_BankAccounts_BankAccountId",
                        column: x => x.BankAccountId,
                        principalTable: "BankAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BankAccountSnapShot_FinancialsSnapshots_FinancialsSnapshotId",
                        column: x => x.FinancialsSnapshotId,
                        principalTable: "FinancialsSnapshots",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "BudgetaryEvents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    SequenceNumber = table.Column<int>(type: "int", nullable: false),
                    EventType = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FinancialsSnapshotId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    PoolId = table.Column<int>(type: "int", nullable: true),
                    Description = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AmountInPennies = table.Column<int>(type: "int", nullable: true),
                    IncomeAllocationEvent_PoolId = table.Column<int>(type: "int", nullable: true),
                    DebtPoolId = table.Column<int>(type: "int", nullable: true),
                    SourcePoolId = table.Column<int>(type: "int", nullable: true),
                    DestinationPoolId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BudgetaryEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BudgetaryEvents_BudgetaryPools_DebtPoolId",
                        column: x => x.DebtPoolId,
                        principalTable: "BudgetaryPools",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_BudgetaryEvents_BudgetaryPools_DestinationPoolId",
                        column: x => x.DestinationPoolId,
                        principalTable: "BudgetaryPools",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BudgetaryEvents_BudgetaryPools_IncomeAllocationEvent_PoolId",
                        column: x => x.IncomeAllocationEvent_PoolId,
                        principalTable: "BudgetaryPools",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BudgetaryEvents_BudgetaryPools_PoolId",
                        column: x => x.PoolId,
                        principalTable: "BudgetaryPools",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BudgetaryEvents_BudgetaryPools_SourcePoolId",
                        column: x => x.SourcePoolId,
                        principalTable: "BudgetaryPools",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BudgetaryEvents_FinancialsSnapshots_FinancialsSnapshotId",
                        column: x => x.FinancialsSnapshotId,
                        principalTable: "FinancialsSnapshots",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "PoolSnapshot",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    PoolId = table.Column<int>(type: "int", nullable: false),
                    BalanceInPennies = table.Column<int>(type: "int", nullable: false),
                    FinancialsSnapshotId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PoolSnapshot", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PoolSnapshot_BudgetaryPools_PoolId",
                        column: x => x.PoolId,
                        principalTable: "BudgetaryPools",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PoolSnapshot_FinancialsSnapshots_FinancialsSnapshotId",
                        column: x => x.FinancialsSnapshotId,
                        principalTable: "FinancialsSnapshots",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_BankAccountSnapShot_BankAccountId",
                table: "BankAccountSnapShot",
                column: "BankAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_BankAccountSnapShot_FinancialsSnapshotId",
                table: "BankAccountSnapShot",
                column: "FinancialsSnapshotId");

            migrationBuilder.CreateIndex(
                name: "IX_BudgetaryEvents_DebtPoolId",
                table: "BudgetaryEvents",
                column: "DebtPoolId");

            migrationBuilder.CreateIndex(
                name: "IX_BudgetaryEvents_DestinationPoolId",
                table: "BudgetaryEvents",
                column: "DestinationPoolId");

            migrationBuilder.CreateIndex(
                name: "IX_BudgetaryEvents_FinancialsSnapshotId",
                table: "BudgetaryEvents",
                column: "FinancialsSnapshotId");

            migrationBuilder.CreateIndex(
                name: "IX_BudgetaryEvents_IncomeAllocationEvent_PoolId",
                table: "BudgetaryEvents",
                column: "IncomeAllocationEvent_PoolId");

            migrationBuilder.CreateIndex(
                name: "IX_BudgetaryEvents_PoolId",
                table: "BudgetaryEvents",
                column: "PoolId");

            migrationBuilder.CreateIndex(
                name: "IX_BudgetaryEvents_SequenceNumber",
                table: "BudgetaryEvents",
                column: "SequenceNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BudgetaryEvents_SourcePoolId",
                table: "BudgetaryEvents",
                column: "SourcePoolId");

            migrationBuilder.CreateIndex(
                name: "IX_BudgetaryPools_BankAccountId",
                table: "BudgetaryPools",
                column: "BankAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_PoolSnapshot_FinancialsSnapshotId",
                table: "PoolSnapshot",
                column: "FinancialsSnapshotId");

            migrationBuilder.CreateIndex(
                name: "IX_PoolSnapshot_PoolId",
                table: "PoolSnapshot",
                column: "PoolId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BankAccountSnapShot");

            migrationBuilder.DropTable(
                name: "BudgetaryEvents");

            migrationBuilder.DropTable(
                name: "PoolSnapshot");

            migrationBuilder.DropTable(
                name: "SequenceNumbers");

            migrationBuilder.DropTable(
                name: "SnapshotLocks");

            migrationBuilder.DropTable(
                name: "BudgetaryPools");

            migrationBuilder.DropTable(
                name: "FinancialsSnapshots");

            migrationBuilder.DropTable(
                name: "BankAccounts");
        }
    }
}
