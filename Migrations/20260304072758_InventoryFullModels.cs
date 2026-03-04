using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackendApi.Migrations
{
    /// <inheritdoc />
    public partial class InventoryFullModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Inventory_Product_ProductId",
                table: "Inventory");

            migrationBuilder.DropForeignKey(
                name: "FK_InventoryEvent_Inventory_InventoryId",
                table: "InventoryEvent");

            migrationBuilder.DropForeignKey(
                name: "FK_Product_Categories_CategoryId",
                table: "Product");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductTag_Product_ProductId",
                table: "ProductTag");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductTag_Tag_TagId",
                table: "ProductTag");

            migrationBuilder.DropForeignKey(
                name: "FK_Reminder_Inventory_InventoryId",
                table: "Reminder");

            migrationBuilder.DropForeignKey(
                name: "FK_ReminderFeedback_Reminder_ReminderId",
                table: "ReminderFeedback");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Tag",
                table: "Tag");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ReminderFeedback",
                table: "ReminderFeedback");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Reminder",
                table: "Reminder");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProductTag",
                table: "ProductTag");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Product",
                table: "Product");

            migrationBuilder.DropPrimaryKey(
                name: "PK_InventoryEvent",
                table: "InventoryEvent");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Inventory",
                table: "Inventory");

            migrationBuilder.RenameTable(
                name: "Tag",
                newName: "Tags");

            migrationBuilder.RenameTable(
                name: "ReminderFeedback",
                newName: "ReminderFeedbacks");

            migrationBuilder.RenameTable(
                name: "Reminder",
                newName: "Reminders");

            migrationBuilder.RenameTable(
                name: "ProductTag",
                newName: "ProductTags");

            migrationBuilder.RenameTable(
                name: "Product",
                newName: "Products");

            migrationBuilder.RenameTable(
                name: "InventoryEvent",
                newName: "InventoryEvents");

            migrationBuilder.RenameTable(
                name: "Inventory",
                newName: "Inventories");

            migrationBuilder.RenameIndex(
                name: "IX_ReminderFeedback_ReminderId",
                table: "ReminderFeedbacks",
                newName: "IX_ReminderFeedbacks_ReminderId");

            migrationBuilder.RenameIndex(
                name: "IX_Reminder_InventoryId",
                table: "Reminders",
                newName: "IX_Reminders_InventoryId");

            migrationBuilder.RenameIndex(
                name: "IX_ProductTag_TagId",
                table: "ProductTags",
                newName: "IX_ProductTags_TagId");

            migrationBuilder.RenameIndex(
                name: "IX_Product_CategoryId",
                table: "Products",
                newName: "IX_Products_CategoryId");

            migrationBuilder.RenameIndex(
                name: "IX_InventoryEvent_InventoryId",
                table: "InventoryEvents",
                newName: "IX_InventoryEvents_InventoryId");

            migrationBuilder.RenameIndex(
                name: "IX_Inventory_ProductId",
                table: "Inventories",
                newName: "IX_Inventories_ProductId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Tags",
                table: "Tags",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ReminderFeedbacks",
                table: "ReminderFeedbacks",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Reminders",
                table: "Reminders",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProductTags",
                table: "ProductTags",
                columns: new[] { "ProductId", "TagId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_Products",
                table: "Products",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_InventoryEvents",
                table: "InventoryEvents",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Inventories",
                table: "Inventories",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Inventories_Products_ProductId",
                table: "Inventories",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_InventoryEvents_Inventories_InventoryId",
                table: "InventoryEvents",
                column: "InventoryId",
                principalTable: "Inventories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Categories_CategoryId",
                table: "Products",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductTags_Products_ProductId",
                table: "ProductTags",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductTags_Tags_TagId",
                table: "ProductTags",
                column: "TagId",
                principalTable: "Tags",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ReminderFeedbacks_Reminders_ReminderId",
                table: "ReminderFeedbacks",
                column: "ReminderId",
                principalTable: "Reminders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Reminders_Inventories_InventoryId",
                table: "Reminders",
                column: "InventoryId",
                principalTable: "Inventories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Inventories_Products_ProductId",
                table: "Inventories");

            migrationBuilder.DropForeignKey(
                name: "FK_InventoryEvents_Inventories_InventoryId",
                table: "InventoryEvents");

            migrationBuilder.DropForeignKey(
                name: "FK_Products_Categories_CategoryId",
                table: "Products");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductTags_Products_ProductId",
                table: "ProductTags");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductTags_Tags_TagId",
                table: "ProductTags");

            migrationBuilder.DropForeignKey(
                name: "FK_ReminderFeedbacks_Reminders_ReminderId",
                table: "ReminderFeedbacks");

            migrationBuilder.DropForeignKey(
                name: "FK_Reminders_Inventories_InventoryId",
                table: "Reminders");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Tags",
                table: "Tags");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Reminders",
                table: "Reminders");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ReminderFeedbacks",
                table: "ReminderFeedbacks");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProductTags",
                table: "ProductTags");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Products",
                table: "Products");

            migrationBuilder.DropPrimaryKey(
                name: "PK_InventoryEvents",
                table: "InventoryEvents");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Inventories",
                table: "Inventories");

            migrationBuilder.RenameTable(
                name: "Tags",
                newName: "Tag");

            migrationBuilder.RenameTable(
                name: "Reminders",
                newName: "Reminder");

            migrationBuilder.RenameTable(
                name: "ReminderFeedbacks",
                newName: "ReminderFeedback");

            migrationBuilder.RenameTable(
                name: "ProductTags",
                newName: "ProductTag");

            migrationBuilder.RenameTable(
                name: "Products",
                newName: "Product");

            migrationBuilder.RenameTable(
                name: "InventoryEvents",
                newName: "InventoryEvent");

            migrationBuilder.RenameTable(
                name: "Inventories",
                newName: "Inventory");

            migrationBuilder.RenameIndex(
                name: "IX_Reminders_InventoryId",
                table: "Reminder",
                newName: "IX_Reminder_InventoryId");

            migrationBuilder.RenameIndex(
                name: "IX_ReminderFeedbacks_ReminderId",
                table: "ReminderFeedback",
                newName: "IX_ReminderFeedback_ReminderId");

            migrationBuilder.RenameIndex(
                name: "IX_ProductTags_TagId",
                table: "ProductTag",
                newName: "IX_ProductTag_TagId");

            migrationBuilder.RenameIndex(
                name: "IX_Products_CategoryId",
                table: "Product",
                newName: "IX_Product_CategoryId");

            migrationBuilder.RenameIndex(
                name: "IX_InventoryEvents_InventoryId",
                table: "InventoryEvent",
                newName: "IX_InventoryEvent_InventoryId");

            migrationBuilder.RenameIndex(
                name: "IX_Inventories_ProductId",
                table: "Inventory",
                newName: "IX_Inventory_ProductId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Tag",
                table: "Tag",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Reminder",
                table: "Reminder",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ReminderFeedback",
                table: "ReminderFeedback",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProductTag",
                table: "ProductTag",
                columns: new[] { "ProductId", "TagId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_Product",
                table: "Product",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_InventoryEvent",
                table: "InventoryEvent",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Inventory",
                table: "Inventory",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Inventory_Product_ProductId",
                table: "Inventory",
                column: "ProductId",
                principalTable: "Product",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_InventoryEvent_Inventory_InventoryId",
                table: "InventoryEvent",
                column: "InventoryId",
                principalTable: "Inventory",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Product_Categories_CategoryId",
                table: "Product",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductTag_Product_ProductId",
                table: "ProductTag",
                column: "ProductId",
                principalTable: "Product",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductTag_Tag_TagId",
                table: "ProductTag",
                column: "TagId",
                principalTable: "Tag",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Reminder_Inventory_InventoryId",
                table: "Reminder",
                column: "InventoryId",
                principalTable: "Inventory",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ReminderFeedback_Reminder_ReminderId",
                table: "ReminderFeedback",
                column: "ReminderId",
                principalTable: "Reminder",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
