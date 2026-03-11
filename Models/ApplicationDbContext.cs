using BackendApi.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BackendApi.Models;

public class ApplicationDbContext : IdentityDbContext<IdentityUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) :
        base(options)
    { }

    public DbSet<Category> Categories { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Tag> Tags { get; set; }
    public DbSet<ProductTag> ProductTags { get; set; }
    public DbSet<Inventory> Inventories { get; set; }
    public DbSet<InventoryEvent> InventoryEvents { get; set; }
    public DbSet<Reminder> Reminders { get; set; }
    public DbSet<ReminderFeedback> ReminderFeedbacks { get; set; }

    // 預算相關資料表
    public DbSet<BudgetAlert> BudgetAlerts { get; set; } // 每個月份、每個類別的目前預算警示狀態以及「總預算警示」
    public DbSet<BudgetCategoryLimit> BudgetCategoryLimits { get; set; } // 每月類別預算額度
    public DbSet<BudgetMonthlyTotal> BudgetMonthlyTotals { get; set; }  // 每月總預算額度
    public DbSet<Expenditure> Expenditures { get; set; } // 用來記錄實際的金錢支出，與現有的庫存數量變化（InventoryEvent）分開管理
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasOne(e => e.Parent)
                  .WithMany(e => e.SubCategories)
                  .HasForeignKey(e => e.ParentId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ProductTag>(entity =>
        {
            entity.HasKey(e => new { e.ProductId, e.TagId });
        });

        modelBuilder.Entity<BudgetAlert>(entity =>
        {
            entity.HasIndex(e => new { e.CategoryId, e.YearMonth })
                  .IsUnique();  // 每個類別+月份最多一筆狀態

            entity.HasIndex(e => e.YearMonth); // 方便按月查詢

            entity.HasOne(e => e.Category)
                  .WithMany()                     // Category 不需要反向集合
                  .HasForeignKey(e => e.CategoryId)
                  .OnDelete(DeleteBehavior.SetNull); // 類別刪除時警示保留（或改 Restrict）
        });

        modelBuilder.Entity<BudgetCategoryLimit>(entity =>
        {
            entity.HasIndex(e => new { e.CategoryId, e.YearMonth })
                  .IsUnique();

            entity.HasOne(e => e.Category)
                  .WithMany()
                  .HasForeignKey(e => e.CategoryId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<BudgetMonthlyTotal>(entity =>
        {
            entity.HasIndex(e => e.YearMonth)
                  .IsUnique();
        });

        modelBuilder.Entity<Expenditure>(entity =>
        {
            entity.HasIndex(e => e.CategoryId);
            entity.HasIndex(e => e.ExpenditureDate);           // 方便按月查詢
            entity.HasIndex(e => new { e.CategoryId, e.ExpenditureDate.Year, e.ExpenditureDate.Month })
                  .HasDatabaseName("IX_Expenditure_Category_Month"); // 加速月統計

            entity.HasOne(e => e.Category)
                  .WithMany()
                  .HasForeignKey(e => e.CategoryId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.InventoryEvent)
                  .WithMany()
                  .HasForeignKey(e => e.InventoryEventId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.Product)
                  .WithMany()
                  .HasForeignKey(e => e.ProductId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.Inventory)
                  .WithMany()
                  .HasForeignKey(e => e.InventoryId)
                  .OnDelete(DeleteBehavior.SetNull);
        });
    }
}