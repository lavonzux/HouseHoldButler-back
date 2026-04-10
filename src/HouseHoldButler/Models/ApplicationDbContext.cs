using HouseHoldButler.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace HouseHoldButler.Models;

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
            entity.HasIndex(e => e.CategoryId);                // 方便按類別查詢
            entity.HasIndex(e => e.ExpenditureDate);           // 方便按月查詢

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

        // ── Seed: 內建分類 ──
        var seedTime = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

        // 主分類 IDs
        var foodId       = new Guid("a0000000-0000-0000-0000-000000000001");
        var beverageId   = new Guid("a0000000-0000-0000-0000-000000000002");
        var cleaningId   = new Guid("a0000000-0000-0000-0000-000000000003");
        var personalId   = new Guid("a0000000-0000-0000-0000-000000000004");
        var householdId  = new Guid("a0000000-0000-0000-0000-000000000005");
        var medicineId   = new Guid("a0000000-0000-0000-0000-000000000006");
        var petId        = new Guid("a0000000-0000-0000-0000-000000000007");
        var stationeryId = new Guid("a0000000-0000-0000-0000-000000000008");

        modelBuilder.Entity<Category>().HasData(
            // ── 主分類 ──
            new Category { Id = foodId,       Name = "食品",     Icon = "food",        CreatedAt = seedTime },
            new Category { Id = beverageId,   Name = "飲品",     Icon = "beverage",    CreatedAt = seedTime },
            new Category { Id = cleaningId,   Name = "清潔用品", Icon = "cleaning",    CreatedAt = seedTime },
            new Category { Id = personalId,   Name = "個人護理", Icon = "personal",    CreatedAt = seedTime },
            new Category { Id = householdId,  Name = "家居用品", Icon = "household",   CreatedAt = seedTime },
            new Category { Id = medicineId,   Name = "醫藥保健", Icon = "medicine",    CreatedAt = seedTime },
            new Category { Id = petId,        Name = "寵物用品", Icon = "pet",         CreatedAt = seedTime },
            new Category { Id = stationeryId, Name = "文具雜貨", Icon = "stationery",  CreatedAt = seedTime },

            // ── 食品 子分類 ──
            new Category { Id = new Guid("b0000000-0000-0000-0000-000000000001"), ParentId = foodId, Name = "生鮮蔬果",   Icon = "vegetable",    CreatedAt = seedTime },
            new Category { Id = new Guid("b0000000-0000-0000-0000-000000000002"), ParentId = foodId, Name = "肉類海鮮",   Icon = "meat",         CreatedAt = seedTime },
            new Category { Id = new Guid("b0000000-0000-0000-0000-000000000003"), ParentId = foodId, Name = "乳製品",     Icon = "dairy",        CreatedAt = seedTime },
            new Category { Id = new Guid("b0000000-0000-0000-0000-000000000004"), ParentId = foodId, Name = "調味料",     Icon = "seasoning",    CreatedAt = seedTime },
            new Category { Id = new Guid("b0000000-0000-0000-0000-000000000005"), ParentId = foodId, Name = "零食點心",   Icon = "snack",        CreatedAt = seedTime },
            new Category { Id = new Guid("b0000000-0000-0000-0000-000000000006"), ParentId = foodId, Name = "冷凍食品",   Icon = "frozen",       CreatedAt = seedTime },
            new Category { Id = new Guid("b0000000-0000-0000-0000-000000000007"), ParentId = foodId, Name = "米麵穀物",   Icon = "grain",        CreatedAt = seedTime },
            new Category { Id = new Guid("b0000000-0000-0000-0000-000000000008"), ParentId = foodId, Name = "罐頭食品",   Icon = "canned",       CreatedAt = seedTime },

            // ── 飲品 子分類 ──
            new Category { Id = new Guid("b0000000-0000-0000-0000-000000000011"), ParentId = beverageId, Name = "水",       Icon = "water",   CreatedAt = seedTime },
            new Category { Id = new Guid("b0000000-0000-0000-0000-000000000012"), ParentId = beverageId, Name = "茶類",     Icon = "tea",     CreatedAt = seedTime },
            new Category { Id = new Guid("b0000000-0000-0000-0000-000000000013"), ParentId = beverageId, Name = "咖啡",     Icon = "coffee",  CreatedAt = seedTime },
            new Category { Id = new Guid("b0000000-0000-0000-0000-000000000014"), ParentId = beverageId, Name = "果汁飲料", Icon = "juice",   CreatedAt = seedTime },
            new Category { Id = new Guid("b0000000-0000-0000-0000-000000000015"), ParentId = beverageId, Name = "酒精飲品", Icon = "alcohol", CreatedAt = seedTime },

            // ── 清潔用品 子分類 ──
            new Category { Id = new Guid("b0000000-0000-0000-0000-000000000021"), ParentId = cleaningId, Name = "洗衣用品",   Icon = "laundry",      CreatedAt = seedTime },
            new Category { Id = new Guid("b0000000-0000-0000-0000-000000000022"), ParentId = cleaningId, Name = "廚房清潔",   Icon = "kitchen_clean", CreatedAt = seedTime },
            new Category { Id = new Guid("b0000000-0000-0000-0000-000000000023"), ParentId = cleaningId, Name = "浴廁清潔",   Icon = "bathroom_clean", CreatedAt = seedTime },
            new Category { Id = new Guid("b0000000-0000-0000-0000-000000000024"), ParentId = cleaningId, Name = "地板清潔",   Icon = "floor_clean",  CreatedAt = seedTime },

            // ── 個人護理 子分類 ──
            new Category { Id = new Guid("b0000000-0000-0000-0000-000000000031"), ParentId = personalId, Name = "沐浴用品",   Icon = "bath",       CreatedAt = seedTime },
            new Category { Id = new Guid("b0000000-0000-0000-0000-000000000032"), ParentId = personalId, Name = "口腔護理",   Icon = "oral_care",  CreatedAt = seedTime },
            new Category { Id = new Guid("b0000000-0000-0000-0000-000000000033"), ParentId = personalId, Name = "護膚保養",   Icon = "skincare",   CreatedAt = seedTime },
            new Category { Id = new Guid("b0000000-0000-0000-0000-000000000034"), ParentId = personalId, Name = "衛生用品",   Icon = "hygiene",    CreatedAt = seedTime },

            // ── 家居用品 子分類 ──
            new Category { Id = new Guid("b0000000-0000-0000-0000-000000000041"), ParentId = householdId, Name = "紙類用品",   Icon = "paper",      CreatedAt = seedTime },
            new Category { Id = new Guid("b0000000-0000-0000-0000-000000000042"), ParentId = householdId, Name = "廚房用品",   Icon = "kitchen",    CreatedAt = seedTime },
            new Category { Id = new Guid("b0000000-0000-0000-0000-000000000043"), ParentId = householdId, Name = "收納整理",   Icon = "storage",    CreatedAt = seedTime },
            new Category { Id = new Guid("b0000000-0000-0000-0000-000000000044"), ParentId = householdId, Name = "照明電池",   Icon = "battery",    CreatedAt = seedTime },

            // ── 醫藥保健 子分類 ──
            new Category { Id = new Guid("b0000000-0000-0000-0000-000000000051"), ParentId = medicineId, Name = "常備藥品",   Icon = "pill",        CreatedAt = seedTime },
            new Category { Id = new Guid("b0000000-0000-0000-0000-000000000052"), ParentId = medicineId, Name = "保健食品",   Icon = "supplement",  CreatedAt = seedTime },
            new Category { Id = new Guid("b0000000-0000-0000-0000-000000000053"), ParentId = medicineId, Name = "急救用品",   Icon = "first_aid",   CreatedAt = seedTime },

            // ── 寵物用品 子分類 ──
            new Category { Id = new Guid("b0000000-0000-0000-0000-000000000061"), ParentId = petId, Name = "寵物食品",   Icon = "pet_food",    CreatedAt = seedTime },
            new Category { Id = new Guid("b0000000-0000-0000-0000-000000000062"), ParentId = petId, Name = "寵物清潔",   Icon = "pet_clean",   CreatedAt = seedTime },
            new Category { Id = new Guid("b0000000-0000-0000-0000-000000000063"), ParentId = petId, Name = "寵物保健",   Icon = "pet_health",  CreatedAt = seedTime }
        );
    }
}