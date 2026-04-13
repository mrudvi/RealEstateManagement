using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Scaffolding.Internal;

namespace RealEstateManagement.Models;

public partial class RealestatemanagementContext : DbContext
{
    public RealestatemanagementContext()
    {
    }

    public RealestatemanagementContext(DbContextOptions<RealestatemanagementContext> options)
        : base(options)
    {
    }

    public virtual DbSet<ActivityLog> ActivityLogs { get; set; }
    public virtual DbSet<Enquiry> Enquiries { get; set; }
    public virtual DbSet<Role> Roles { get; set; }
    public virtual DbSet<UserRole> UserRoles { get; set; }
    public virtual DbSet<EnquiryResponse> Enquiryresponses { get; set; }

    public virtual DbSet<Favorite> Favorites { get; set; }

    public virtual DbSet<Locality> Localities { get; set; }

    public virtual DbSet<Notification> Notifications { get; set; }

    public virtual DbSet<Otp> Otps { get; set; }

    public virtual DbSet<Property> Properties { get; set; }

    public virtual DbSet<Propertyimage> Propertyimages { get; set; }
    public DbSet<Propertyvideo> PropertyVideos { get; set; }
    public virtual DbSet<Propertytype> Propertytypes { get; set; }

    public virtual DbSet<SiteVisit> Sitevisits { get; set; }

    public virtual DbSet<Transaction> Transactions { get; set; }
    public DbSet<BrokerDetails> BrokerDetails { get; set; }
    public DbSet<BuilderDetails> BuilderDetails { get; set; }
    public virtual DbSet<User> Users { get; set; }
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseMySql("server=localhost;port=3306;database=realestatemanagement;uid=root;pwd=Root123@", Microsoft.EntityFrameworkCore.ServerVersion.Parse("8.0.44-mysql"));

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb4_0900_ai_ci")
            .HasCharSet("utf8mb4");

        modelBuilder.Entity<ActivityLog>(entity =>
        {
            entity.HasKey(e => e.LogId).HasName("PRIMARY");

            entity.ToTable("activitylog");

            entity.HasIndex(e => e.CreatedDate, "idx_date");

            entity.HasIndex(e => e.UserId, "idx_user");

            entity.Property(e => e.Action).HasMaxLength(255);
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime");
            entity.Property(e => e.Description).HasColumnType("text");
            entity.Property(e => e.EntityType).HasMaxLength(50);
            entity.Property(e => e.IPAddress)
                .HasMaxLength(45)
                .HasColumnName("IPAddress");

            entity.HasOne(d => d.User).WithMany(p => p.ActivityLogs)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("activitylog_ibfk_1");
        });

        modelBuilder.Entity<Enquiry>(entity =>
        {
            entity.HasKey(e => e.EnquiryId).HasName("PRIMARY");

            entity.ToTable("enquiries");

            entity.HasIndex(e => e.RespondedBy, "RespondedBy");

            entity.HasIndex(e => e.CustomerId, "idx_customer");

            entity.HasIndex(e => e.PropertyId, "idx_property");

            entity.HasIndex(e => e.EnquiryStatus, "idx_status");

            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime");
            entity.Property(e => e.EnquiryMessage).HasColumnType("text");
            entity.Property(e => e.EnquiryStatus)
                .HasDefaultValueSql("'New'")
                .HasColumnType("enum('New','InProgress','Contacted','Closed','Rejected')");
            entity.Property(e => e.Priority)
                .HasDefaultValueSql("'Medium'")
                .HasColumnType("enum('Low','Medium','High')");
            entity.Property(e => e.RespondedDate).HasColumnType("datetime");
            entity.Property(e => e.UpdatedDate)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Customer).WithMany(p => p.EnquiryCustomers)
                .HasForeignKey(d => d.CustomerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("enquiries_ibfk_2");

            entity.HasOne(d => d.Property).WithMany(p => p.Enquiry)
                .HasForeignKey(d => d.PropertyId)
                .HasConstraintName("enquiries_ibfk_1");

            entity.HasOne(d => d.RespondedByUser)
      .WithMany(u => u.EnquiryRespondedByNavigations)
      .HasForeignKey(d => d.RespondedBy)
      .OnDelete(DeleteBehavior.SetNull)
      .HasConstraintName("enquiries_ibfk_3");
        });

        modelBuilder.Entity<EnquiryResponse>(entity =>
        {
            entity.HasKey(e => e.ResponseId).HasName("PRIMARY");

            entity.ToTable("enquiryresponses");

            entity.HasIndex(e => e.RespondedBy, "RespondedBy");

            entity.HasIndex(e => e.EnquiryId, "idx_enquiry");

            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime");
            entity.Property(e => e.ResponseMessage).HasColumnType("text");

            entity.HasOne(d => d.Enquiry).WithMany(p => p.Responses)
                .HasForeignKey(d => d.EnquiryId)
                .HasConstraintName("enquiryresponses_ibfk_1");

            entity.HasOne(d => d.RespondedByNavigation).WithMany(p => p.EnquiryResponses)
                .HasForeignKey(d => d.RespondedBy)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("enquiryresponses_ibfk_2");
        });

        modelBuilder.Entity<Favorite>(entity =>
        {
            entity.HasKey(e => e.FavoriteId).HasName("PRIMARY");

            entity.ToTable("favorites");

            entity.HasIndex(e => e.PropertyId, "PropertyId");

            entity.HasIndex(e => e.CustomerId, "idx_customer");

            entity.HasIndex(e => new { e.CustomerId, e.PropertyId }, "unique_favorite").IsUnique();

            entity.Property(e => e.AddedDate)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Customer).WithMany(p => p.Favorites)
                .HasForeignKey(d => d.CustomerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("favorites_ibfk_1");

            entity.HasOne(d => d.Property).WithMany(p => p.Favorites)
                .HasForeignKey(d => d.PropertyId)
                .HasConstraintName("favorites_ibfk_2");
        });

        modelBuilder.Entity<Locality>(entity =>
        {
            entity.HasKey(e => e.LocalityId).HasName("PRIMARY");

            entity.ToTable("localities");

            entity.HasIndex(e => e.LocalityName, "idx_locality_name").IsUnique();

            entity.Property(e => e.Area).HasMaxLength(100);
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime");
            entity.Property(e => e.Description).HasColumnType("text");
            entity.Property(e => e.IsActive).HasDefaultValueSql("'1'");
            entity.Property(e => e.LocalityName).HasMaxLength(100);
            entity.Property(e => e.ZipCode).HasMaxLength(10);
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.NotificationId).HasName("PRIMARY");

            entity.ToTable("notifications");

            entity.HasIndex(e => e.IsRead, "idx_read");

            entity.HasIndex(e => e.UserId, "idx_user");

            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime");
            entity.Property(e => e.IsRead).HasDefaultValueSql("'0'");
            entity.Property(e => e.Message).HasMaxLength(1000);
            entity.Property(e => e.NotificationType).HasMaxLength(50);
            entity.Property(e => e.ReadDate).HasColumnType("datetime");
            entity.Property(e => e.Subject).HasMaxLength(255);

            entity.HasOne(d => d.User).WithMany(p => p.Notifications)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("notifications_ibfk_1");
        });

        modelBuilder.Entity<Otp>(entity =>
        {
            entity.HasKey(e => e.Otpid).HasName("PRIMARY");

            entity.ToTable("otps");

            entity.HasIndex(e => e.Email, "idx_email");

            entity.HasIndex(e => e.UserId, "idx_user");

            entity.Property(e => e.Otpid).HasColumnName("OTPId");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime");
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.ExpiryDate).HasColumnType("datetime");
            entity.Property(e => e.IsVerified).HasDefaultValueSql("'0'");
            entity.Property(e => e.Otpcode)
                .HasMaxLength(6)
                .HasColumnName("OTPCode");

            entity.HasOne(d => d.User).WithMany(p => p.Otps)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("otps_ibfk_1");
        });

        modelBuilder.Entity<Property>(entity =>
        {
            entity.HasKey(e => e.PropertyId).HasName("PRIMARY");

            entity.ToTable("properties");

            entity.HasIndex(e => e.ApprovedBy, "ApprovedBy");

            entity.HasIndex(e => e.BrokerId, "BrokerId");

            entity.HasIndex(e => e.BudderId, "BudderId");

            entity.HasIndex(e => e.LocalityId, "idx_locality");

            entity.HasIndex(e => e.OwnerId, "idx_owner");

            entity.HasIndex(e => e.PropertyTypeId, "idx_property_type");

            entity.HasIndex(e => e.Status, "idx_status");

            entity.HasIndex(e => e.TransactionType, "idx_transaction");

            entity.Property(e => e.Address).HasMaxLength(255);
            entity.Property(e => e.ApprovalDate).HasColumnType("datetime");
            entity.Property(e => e.AreaUnit)
                .HasDefaultValueSql("'SqFt'")
                .HasColumnType("enum('SqFt','SqMeter')");
            entity.Property(e => e.Bathrooms).HasDefaultValueSql("'0'");
            entity.Property(e => e.Bedrooms).HasDefaultValueSql("'0'");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime");
            entity.Property(e => e.Facing)
                .HasDefaultValueSql("'North'")
                .HasColumnType("enum('North','South','East','West','NorthEast','NorthWest','SouthEast','SouthWest')");
            entity.Property(e => e.Features).HasColumnType("text");
            entity.Property(e => e.Furnishing)
                .HasDefaultValueSql("'Unfurnished'")
                .HasColumnType("enum('Furnished','SemiFurnished','Unfurnished')");
            entity.Property(e => e.IsVerified).HasDefaultValueSql("'0'");
            entity.Property(e => e.LandmarkDescription).HasColumnType("text");
            entity.Property(e => e.MainImage).HasMaxLength(255);
            entity.Property(e => e.Parking).HasDefaultValueSql("'0'");
            entity.Property(e => e.PropertyArea).HasPrecision(10, 2);
            entity.Property(e => e.PropertyDescription).HasColumnType("text");
            entity.Property(e => e.PropertyPrice).HasPrecision(15, 2);
            entity.Property(e => e.PropertyTitle).HasMaxLength(200);
            entity.Property(e => e.RejectionReason).HasColumnType("text");
            entity.Property(e => e.Status)
                .HasDefaultValueSql("'Pending'")
                .HasColumnType("enum('Active','Inactive','Sold','Rented','Pending')");
            entity.Property(e => e.TransactionType).HasColumnType("enum('Buy','Sell','Rent')");
            entity.Property(e => e.UpdatedDate)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime");

            entity.HasOne(d => d.ApprovedByNavigation).WithMany(p => p.PropertyApprovedByNavigations)
     .HasForeignKey(d => d.ApprovedBy)
     .OnDelete(DeleteBehavior.SetNull)
     .HasConstraintName("properties_ibfk_6");

            entity.HasOne(d => d.Broker).WithMany(p => p.BrokedProperties)
    .HasForeignKey(d => d.BrokerId)
    .OnDelete(DeleteBehavior.SetNull)
    .HasConstraintName("properties_ibfk_4");

            entity.HasOne(d => d.Budder).WithMany(p => p.PropertyBudders)
     .HasForeignKey(d => d.BudderId)
     .OnDelete(DeleteBehavior.SetNull)
     .HasConstraintName("properties_ibfk_5");

            entity.HasOne(d => d.Locality).WithMany(p => p.Properties)
                .HasForeignKey(d => d.LocalityId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("properties_ibfk_2");

            entity.HasOne(d => d.Owner).WithMany(p => p.PropertyOwners)
                .HasForeignKey(d => d.OwnerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("properties_ibfk_3");

            entity.HasOne(d => d.PropertyType).WithMany(p => p.Properties)
                .HasForeignKey(d => d.PropertyTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("properties_ibfk_1");
        });

        modelBuilder.Entity<Propertyimage>(entity =>
        {
            entity.HasKey(e => e.ImageId).HasName("PRIMARY");

            entity.ToTable("propertyimages");

            entity.HasIndex(e => e.PropertyId, "idx_property");

            entity.Property(e => e.ImagePath).HasMaxLength(255);
            entity.Property(e => e.IsPrimary).HasDefaultValueSql("'0'");
            entity.Property(e => e.UploadedDate)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Property).WithMany(p => p.PropertyImages)
                .HasForeignKey(d => d.PropertyId)
                .HasConstraintName("propertyimages_ibfk_1");
        });

        modelBuilder.Entity<Propertytype>(entity =>
        {
            entity.HasKey(e => e.PropertyTypeId).HasName("PRIMARY");

            entity.ToTable("propertytypes");

            entity.HasIndex(e => e.TypeName, "TypeName").IsUnique();

            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime");
            entity.Property(e => e.Description).HasColumnType("text");
            entity.Property(e => e.IsActive).HasDefaultValueSql("'1'");
            entity.Property(e => e.TypeName).HasMaxLength(50);
        });

        modelBuilder.Entity<SiteVisit>(entity =>
        {
            entity.HasKey(e => e.VisitId).HasName("PRIMARY");

            entity.ToTable("sitevisits");

            entity.HasIndex(e => e.ScheduledBy, "ScheduledBy");

            entity.HasIndex(e => e.CustomerId, "idx_customer");

            entity.HasIndex(e => e.ScheduledDate, "idx_date");

            entity.HasIndex(e => e.PropertyId, "idx_property");

            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime");
            entity.Property(e => e.Notes).HasColumnType("text");
            entity.Property(e => e.ScheduledDate).HasColumnType("datetime");
            entity.Property(e => e.ScheduledTime).HasMaxLength(10);
            entity.Property(e => e.UpdatedDate)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime");
            entity.Property(e => e.VisitStatus)
                .HasDefaultValueSql("'Scheduled'")
                .HasColumnType("enum('Scheduled','Completed','Cancelled','NoShow')");

            entity.HasOne(d => d.Customer).WithMany(p => p.SitevisitCustomers)
                .HasForeignKey(d => d.CustomerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("sitevisits_ibfk_2");

            entity.HasOne(d => d.Property).WithMany(p => p.SiteVisits)
                .HasForeignKey(d => d.PropertyId)
                .HasConstraintName("sitevisits_ibfk_1");

            entity.HasOne(d => d.ScheduledByUser)
    .WithMany()
    .HasForeignKey(d => d.ScheduledBy)
    .HasConstraintName("sitevisits_ibfk_3");
        });

        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.HasKey(e => e.TransactionId).HasName("PRIMARY");

            entity.ToTable("transactions");

            entity.HasIndex(e => e.ApprovedBy, "ApprovedBy");

            entity.HasIndex(e => e.BuyerId, "idx_buyer");

            entity.HasIndex(e => e.PropertyId, "idx_property");

            entity.HasIndex(e => e.SellerId, "idx_seller");

            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime");
            entity.Property(e => e.DocumentPath).HasMaxLength(255);
            entity.Property(e => e.Notes).HasColumnType("text");
            entity.Property(e => e.PaymentMethod).HasMaxLength(100);
            entity.Property(e => e.TransactionAmount).HasPrecision(15, 2);
            entity.Property(e => e.TransactionDate).HasColumnType("datetime");
            entity.Property(e => e.TransactionStatus)
                .HasDefaultValueSql("'Pending'")
                .HasColumnType("enum('Pending','Completed','Cancelled')");

            entity.HasOne(d => d.ApprovedByNavigation).WithMany(p => p.TransactionApprovedByNavigations)
                .HasForeignKey(d => d.ApprovedBy)
                .HasConstraintName("transactions_ibfk_4");

            entity.HasOne(d => d.Buyer).WithMany(p => p.TransactionBuyers)
                .HasForeignKey(d => d.BuyerId)
                .HasConstraintName("transactions_ibfk_2");

            entity.HasOne(d => d.Property).WithMany(p => p.Transactions)
                .HasForeignKey(d => d.PropertyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("transactions_ibfk_1");

            entity.HasOne(d => d.Seller).WithMany(p => p.TransactionSellers)
                .HasForeignKey(d => d.SellerId)
                .HasConstraintName("transactions_ibfk_3");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PRIMARY");

            entity.ToTable("users");

            entity.HasIndex(e => e.Email, "Email").IsUnique();

            entity.HasIndex(e => e.City, "idx_city");

            entity.Property(e => e.Address).HasMaxLength(255);
            entity.Property(e => e.City)
                .HasMaxLength(50)
                .HasDefaultValueSql("'Surat'");
            entity.Property(e => e.Country).HasMaxLength(50);
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime");
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.FirstName).HasMaxLength(100);
            entity.Property(e => e.IsActive).HasDefaultValueSql("'1'");
            entity.Property(e => e.IsVerified).HasDefaultValueSql("'0'");
            entity.Property(e => e.LastName).HasMaxLength(100);
            entity.Property(e => e.PasswordHash).HasMaxLength(255);
            entity.Property(e => e.PhoneNumber).HasMaxLength(15);
            entity.Property(e => e.ProfileImage).HasMaxLength(255);
            entity.Property(e => e.State).HasMaxLength(50);
            entity.Property(e => e.UpdatedDate)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
