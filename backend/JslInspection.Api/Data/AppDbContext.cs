using Microsoft.EntityFrameworkCore;
using JslInspection.Api.Models;

namespace JslInspection.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // Existing table (we keep it for future; demo may not use it now)
    public DbSet<Equipment> Equipments => Set<Equipment>();

    // NEW: Checklist template master (dropdown list)
    public DbSet<ChecklistTemplate> ChecklistTemplates => Set<ChecklistTemplate>();

    // NEW: Checklist items (questions/parameters inside a checklist)
    public DbSet<ChecklistItemTemplate> ChecklistItemTemplates => Set<ChecklistItemTemplate>();


    // NEW: Submissions (inspector filled checklists)
    public DbSet<ChecklistSubmission> ChecklistSubmissions => Set<ChecklistSubmission>();

    // NEW: Answers inside a submission
    public DbSet<ChecklistSubmissionItem> ChecklistSubmissionItems => Set<ChecklistSubmissionItem>();

    // NEW: Photos linked to answered items
    public DbSet<SubmissionPhoto> SubmissionPhotos => Set<SubmissionPhoto>();

    // NEW: Email outbox (demo-safe email queue)
    public DbSet<EmailOutbox> EmailOutbox => Set<EmailOutbox>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        var seedCreatedAt = new DateTimeOffset(2026, 2, 12, 0, 0, 0, TimeSpan.Zero);

        // ----------------------------
        // Equipment rule (existing)
        // ----------------------------
        modelBuilder.Entity<Equipment>()
            .HasIndex(e => new { e.Plant, e.Area, e.EquipmentTag })
            .IsUnique();

        // FIX: define decimal precision so SQL Server knows how to store Min/Max values
        modelBuilder.Entity<ChecklistItemTemplate>()
           .Property(x => x.MinValue)
           .HasPrecision(10, 2);

        modelBuilder.Entity<ChecklistItemTemplate>()
           .Property(x => x.MaxValue)
           .HasPrecision(10, 2);
        // ----------------------------
        // Submission relationships
        // ----------------------------

        // One ChecklistTemplate -> many Submissions
        modelBuilder.Entity<ChecklistSubmission>()
            .HasOne(s => s.ChecklistTemplate)
            .WithMany()
            .HasForeignKey(s => s.ChecklistTemplateId)
            .OnDelete(DeleteBehavior.Restrict);

        // One Submission -> many SubmissionItems
        modelBuilder.Entity<ChecklistSubmissionItem>()
            .HasOne(i => i.ChecklistSubmission)
            .WithMany(s => s.Items)
            .HasForeignKey(i => i.ChecklistSubmissionId)
            .OnDelete(DeleteBehavior.Cascade);

        // Helpful index when fetching answers
        modelBuilder.Entity<ChecklistSubmissionItem>()
            .HasIndex(i => new { i.ChecklistSubmissionId, i.ChecklistItemTemplateId });

        // Photo index
        modelBuilder.Entity<SubmissionPhoto>()
            .HasIndex(p => p.ChecklistSubmissionItemId);

        // Decimal precision for numeric answer
        modelBuilder.Entity<ChecklistSubmissionItem>()
            .Property(x => x.ValueNumber)
            .HasPrecision(10, 2);
        // ----------------------------
        // Email Outbox index
        // ----------------------------
        modelBuilder.Entity<EmailOutbox>()
            .HasIndex(e => new { e.Status, e.CreatedAtUtc });

        // ----------------------------
        // Checklist relationship:
        // One ChecklistTemplate has many ChecklistItemTemplate rows.
        // Cascade delete means: deleting a template deletes its items too.
        // ----------------------------
        modelBuilder.Entity<ChecklistItemTemplate>()
            .HasOne(i => i.ChecklistTemplate)
            .WithMany(t => t.Items)
            .HasForeignKey(i => i.ChecklistTemplateId)
            .OnDelete(DeleteBehavior.Cascade);

        // Helpful index for fast loading: filter by template and sort by sequence
        modelBuilder.Entity<ChecklistItemTemplate>()
            .HasIndex(i => new { i.ChecklistTemplateId, i.SeqNo });

        // ----------------------------
        // DEMO SEED DATA
        // This inserts checklist names + items automatically into SQL Server
        // when you run: dotnet ef database update
        // ----------------------------

        // Seed two checklist templates (these appear in dropdown)
        modelBuilder.Entity<ChecklistTemplate>().HasData(
            new ChecklistTemplate
            {
                Id = 1,
                Name = "CRM Daily Inspection",
                Description = "Daily visual & basic parameter checks for CRM area.",
                IsActive = true,
                CreatedAtUtc = seedCreatedAt
            },
            new ChecklistTemplate
            {
                Id = 2,
                Name = "CRM Conveyor Weekly Check",
                Description = "Weekly detailed checklist for conveyor health.",
                IsActive = true,
                CreatedAtUtc = seedCreatedAt
            }
        );

        // Seed items for template 1
        modelBuilder.Entity<ChecklistItemTemplate>().HasData(
            new ChecklistItemTemplate
            {
                Id = 101,
                ChecklistTemplateId = 1,
                SeqNo = 1,
                ItemText = "Any oil leakage observed?",
                InputType = "PassFail",
                IsRequired = true,
                RequiresPhotoIfNotOk = true
            },
            new ChecklistItemTemplate
            {
                Id = 102,
                ChecklistTemplateId = 1,
                SeqNo = 2,
                ItemText = "Housekeeping condition",
                InputType = "Dropdown",
                IsRequired = true,
                DropdownOptionsCsv = "Good,Average,Poor"
            },
            new ChecklistItemTemplate
            {
                Id = 103,
                ChecklistTemplateId = 1,
                SeqNo = 3,
                ItemText = "Motor temperature (°C)",
                InputType = "Number",
                IsRequired = true,
                MinValue = 0,
                MaxValue = 90
            },
            new ChecklistItemTemplate
            {
                Id = 104,
                ChecklistTemplateId = 1,
                SeqNo = 4,
                ItemText = "Remarks (if any)",
                InputType = "Text",
                IsRequired = false
            }
        );

        // Seed items for template 2
        modelBuilder.Entity<ChecklistItemTemplate>().HasData(
            new ChecklistItemTemplate
            {
                Id = 201,
                ChecklistTemplateId = 2,
                SeqNo = 1,
                ItemText = "Belt alignment OK?",
                InputType = "PassFail",
                IsRequired = true,
                RequiresPhotoIfNotOk = true
            },
            new ChecklistItemTemplate
            {
                Id = 202,
                ChecklistTemplateId = 2,
                SeqNo = 2,
                ItemText = "Abnormal noise/vibration?",
                InputType = "PassFail",
                IsRequired = true,
                RequiresPhotoIfNotOk = true
            },
            new ChecklistItemTemplate
            {
                Id = 203,
                ChecklistTemplateId = 2,
                SeqNo = 3,
                ItemText = "Any damaged idlers/rollers visible?",
                InputType = "PassFail",
                IsRequired = true,
                RequiresPhotoIfNotOk = true
            },
            new ChecklistItemTemplate
            {
                Id = 204,
                ChecklistTemplateId = 2,
                SeqNo = 4,
                ItemText = "General comments",
                InputType = "Text",
                IsRequired = false
            }
        );
    }
}
