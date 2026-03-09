using Microsoft.EntityFrameworkCore;
using CycleTrust.Core.Entities;
using CycleTrust.Core.Enums;

namespace CycleTrust.Infrastructure.Data;

public class CycleTrustDbContext : DbContext
{
    public CycleTrustDbContext(DbContextOptions<CycleTrustDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Brand> Brands => Set<Brand>();
    public DbSet<BikeCategory> BikeCategories => Set<BikeCategory>();
    public DbSet<SizeOption> SizeOptions => Set<SizeOption>();
    public DbSet<Listing> Listings => Set<Listing>();
    public DbSet<ListingMedia> ListingMedia => Set<ListingMedia>();
    public DbSet<Inspection> Inspections => Set<Inspection>();
    public DbSet<Wishlist> Wishlists => Set<Wishlist>();
    public DbSet<DepositPolicy> DepositPolicies => Set<DepositPolicy>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<ViolationReport> ViolationReports => Set<ViolationReport>();
    public DbSet<Dispute> Disputes => Set<Dispute>();
    public DbSet<DisputeEvent> DisputeEvents => Set<DisputeEvent>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<ChatConversation> ChatConversations => Set<ChatConversation>();
    public DbSet<ChatMessage> ChatMessages => Set<ChatMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Email).HasColumnName("email").HasMaxLength(255);
            entity.Property(e => e.Phone).HasColumnName("phone").HasMaxLength(30);
            entity.Property(e => e.PasswordHash).HasColumnName("password_hash");
            entity.Property(e => e.Role).HasColumnName("role").HasConversion<string>();
            entity.Property(e => e.FullName).HasColumnName("full_name").HasMaxLength(255).IsRequired();
            entity.Property(e => e.AvatarUrl).HasColumnName("avatar_url");
            entity.Property(e => e.IsActive).HasColumnName("is_active");
            entity.Property(e => e.ApprovalStatus).HasColumnName("approval_status").HasConversion<int?>();
            entity.Property(e => e.RatingAvg).HasColumnName("rating_avg").HasPrecision(3, 2);
            entity.Property(e => e.RatingCount).HasColumnName("rating_count");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasIndex(e => e.Phone).IsUnique();
            entity.HasIndex(e => e.Role);
        });

        // Brand
        modelBuilder.Entity<Brand>(entity =>
        {
            entity.ToTable("brands");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(120).IsRequired();
            entity.Property(e => e.IsActive).HasColumnName("is_active");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            
            entity.HasIndex(e => e.Name).IsUnique();
        });

        // BikeCategory
        modelBuilder.Entity<BikeCategory>(entity =>
        {
            entity.ToTable("bike_categories");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(120).IsRequired();
            entity.Property(e => e.IsActive).HasColumnName("is_active");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            
            entity.HasIndex(e => e.Name).IsUnique();
        });

        // SizeOption
        modelBuilder.Entity<SizeOption>(entity =>
        {
            entity.ToTable("size_options");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Label).HasColumnName("label").HasMaxLength(50).IsRequired();
            entity.Property(e => e.IsActive).HasColumnName("is_active");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            
            entity.HasIndex(e => e.Label).IsUnique();
        });

        // Listing
        modelBuilder.Entity<Listing>(entity =>
        {
            entity.ToTable("listings");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.SellerId).HasColumnName("seller_id");
            entity.Property(e => e.Title).HasColumnName("title").HasMaxLength(255).IsRequired();
            entity.Property(e => e.Description).HasColumnName("description").IsRequired();
            entity.Property(e => e.UsageHistory).HasColumnName("usage_history");
            entity.Property(e => e.LocationText).HasColumnName("location_text").HasMaxLength(255);
            entity.Property(e => e.BrandId).HasColumnName("brand_id");
            entity.Property(e => e.CategoryId).HasColumnName("category_id");
            entity.Property(e => e.SizeOptionId).HasColumnName("size_option_id");
            entity.Property(e => e.PriceAmount).HasColumnName("price_amount");
            entity.Property(e => e.Currency).HasColumnName("currency").HasMaxLength(10);
            entity.Property(e => e.ConditionNote).HasColumnName("condition_note").HasMaxLength(255);
            entity.Property(e => e.YearModel).HasColumnName("year_model");
            entity.Property(e => e.Status).HasColumnName("status").HasConversion<string>();
            entity.Property(e => e.ApprovedBy).HasColumnName("approved_by");
            entity.Property(e => e.ApprovedAt).HasColumnName("approved_at");
            entity.Property(e => e.RejectedReason).HasColumnName("rejected_reason");
            entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

            entity.HasOne(e => e.Seller).WithMany(u => u.Listings).HasForeignKey(e => e.SellerId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Brand).WithMany(b => b.Listings).HasForeignKey(e => e.BrandId).OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(e => e.Category).WithMany(c => c.Listings).HasForeignKey(e => e.CategoryId).OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(e => e.SizeOption).WithMany(s => s.Listings).HasForeignKey(e => e.SizeOptionId).OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(e => e.ApprovedByUser).WithMany().HasForeignKey(e => e.ApprovedBy).OnDelete(DeleteBehavior.SetNull);

            entity.HasIndex(e => e.SellerId);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.BrandId);
            entity.HasIndex(e => e.CategoryId);
        });

        // ListingMedia
        modelBuilder.Entity<ListingMedia>(entity =>
        {
            entity.ToTable("listing_media");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ListingId).HasColumnName("listing_id");
            entity.Property(e => e.Type).HasColumnName("type").HasConversion<string>();
            entity.Property(e => e.Url).HasColumnName("url").IsRequired();
            entity.Property(e => e.SortOrder).HasColumnName("sort_order");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");

            entity.HasOne(e => e.Listing).WithMany(l => l.Media).HasForeignKey(e => e.ListingId).OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => e.ListingId);
        });

        // Inspection
        modelBuilder.Entity<Inspection>(entity =>
        {
            entity.ToTable("inspections");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ListingId).HasColumnName("listing_id");
            entity.Property(e => e.InspectorId).HasColumnName("inspector_id");
            entity.Property(e => e.Summary).HasColumnName("summary").IsRequired();
            entity.Property(e => e.ChecklistJson).HasColumnName("checklist_json").HasColumnType("json");
            entity.Property(e => e.ReportUrl).HasColumnName("report_url");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

            entity.HasOne(e => e.Listing).WithOne(l => l.Inspection).HasForeignKey<Inspection>(e => e.ListingId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Inspector).WithMany(u => u.Inspections).HasForeignKey(e => e.InspectorId).OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(e => e.ListingId).IsUnique();
            entity.HasIndex(e => e.InspectorId);
        });

        // Wishlist
        modelBuilder.Entity<Wishlist>(entity =>
        {
            entity.ToTable("wishlists");
            entity.HasKey(e => new { e.BuyerId, e.ListingId });
            entity.Property(e => e.BuyerId).HasColumnName("buyer_id");
            entity.Property(e => e.ListingId).HasColumnName("listing_id");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");

            entity.HasOne(e => e.Buyer).WithMany().HasForeignKey(e => e.BuyerId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Listing).WithMany().HasForeignKey(e => e.ListingId).OnDelete(DeleteBehavior.Cascade);
        });

        // DepositPolicy
        modelBuilder.Entity<DepositPolicy>(entity =>
        {
            entity.ToTable("deposit_policies");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.IsActive).HasColumnName("is_active");
            entity.Property(e => e.PolicyName).HasColumnName("policy_name").HasMaxLength(120).IsRequired();
            entity.Property(e => e.Mode).HasColumnName("mode").HasConversion<string>();
            entity.Property(e => e.PercentValue).HasColumnName("percent_value").HasPrecision(5, 2);
            entity.Property(e => e.FixedAmount).HasColumnName("fixed_amount");
            entity.Property(e => e.MinAmount).HasColumnName("min_amount");
            entity.Property(e => e.MaxAmount).HasColumnName("max_amount");
            entity.Property(e => e.Note).HasColumnName("note");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
        });

        // Order
        modelBuilder.Entity<Order>(entity =>
        {
            entity.ToTable("orders");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ListingId).HasColumnName("listing_id");
            entity.Property(e => e.BuyerId).HasColumnName("buyer_id");
            entity.Property(e => e.SellerId).HasColumnName("seller_id");
            entity.Property(e => e.Status).HasColumnName("status").HasConversion<string>();
            entity.Property(e => e.PriceAmount).HasColumnName("price_amount");
            entity.Property(e => e.Currency).HasColumnName("currency").HasMaxLength(10);
            entity.Property(e => e.DepositRequired).HasColumnName("deposit_required");
            entity.Property(e => e.DepositAmount).HasColumnName("deposit_amount");
            entity.Property(e => e.DepositDueAt).HasColumnName("deposit_due_at");
            entity.Property(e => e.DepositPaidAt).HasColumnName("deposit_paid_at");
            entity.Property(e => e.ReserveExpiresAt).HasColumnName("reserve_expires_at");
            entity.Property(e => e.ShippingNote).HasColumnName("shipping_note");
            entity.Property(e => e.DeliveredAt).HasColumnName("delivered_at");
            entity.Property(e => e.CompletedAt).HasColumnName("completed_at");
            entity.Property(e => e.CanceledReason).HasColumnName("canceled_reason");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

            entity.HasOne(e => e.Listing).WithMany(l => l.Orders).HasForeignKey(e => e.ListingId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Buyer).WithMany(u => u.OrdersAsBuyer).HasForeignKey(e => e.BuyerId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Seller).WithMany(u => u.OrdersAsSeller).HasForeignKey(e => e.SellerId).OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => e.BuyerId);
            entity.HasIndex(e => e.SellerId);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.ListingId);
        });

        // Payment
        modelBuilder.Entity<Payment>(entity =>
        {
            entity.ToTable("payments");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.OrderId).HasColumnName("order_id");
            entity.Property(e => e.Type).HasColumnName("type").HasConversion<string>();
            entity.Property(e => e.Status).HasColumnName("status").HasConversion<string>();
            entity.Property(e => e.Amount).HasColumnName("amount");
            entity.Property(e => e.Currency).HasColumnName("currency").HasMaxLength(10);
            entity.Property(e => e.Provider).HasColumnName("provider").HasMaxLength(50);
            entity.Property(e => e.ProviderTxnId).HasColumnName("provider_txn_id").HasMaxLength(120);
            entity.Property(e => e.PaidAt).HasColumnName("paid_at");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");

            entity.HasOne(e => e.Order).WithMany(o => o.Payments).HasForeignKey(e => e.OrderId).OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => e.OrderId);
            entity.HasIndex(e => e.Status);
        });

        // Review
        modelBuilder.Entity<Review>(entity =>
        {
            entity.ToTable("reviews");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.OrderId).HasColumnName("order_id");
            entity.Property(e => e.BuyerId).HasColumnName("buyer_id");
            entity.Property(e => e.SellerId).HasColumnName("seller_id");
            entity.Property(e => e.Rating).HasColumnName("rating");
            entity.Property(e => e.Comment).HasColumnName("comment");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");

            entity.HasOne(e => e.Order).WithOne(o => o.Review).HasForeignKey<Review>(e => e.OrderId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Buyer).WithMany(u => u.ReviewsGiven).HasForeignKey(e => e.BuyerId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Seller).WithMany(u => u.ReviewsReceived).HasForeignKey(e => e.SellerId).OnDelete(DeleteBehavior.Restrict);
            
            entity.HasIndex(e => e.OrderId).IsUnique();
            entity.HasIndex(e => e.SellerId);
        });

        // ViolationReport
        modelBuilder.Entity<ViolationReport>(entity =>
        {
            entity.ToTable("violation_reports");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ReporterId).HasColumnName("reporter_id");
            entity.Property(e => e.ListingId).HasColumnName("listing_id");
            entity.Property(e => e.ReportedUserId).HasColumnName("reported_user_id");
            entity.Property(e => e.Reason).HasColumnName("reason").HasMaxLength(255).IsRequired();
            entity.Property(e => e.Details).HasColumnName("details");
            entity.Property(e => e.Status).HasColumnName("status").HasConversion<string>();
            entity.Property(e => e.HandledBy).HasColumnName("handled_by");
            entity.Property(e => e.HandledAt).HasColumnName("handled_at");
            entity.Property(e => e.ResolutionNote).HasColumnName("resolution_note");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

            entity.HasOne(e => e.Reporter).WithMany().HasForeignKey(e => e.ReporterId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Listing).WithMany().HasForeignKey(e => e.ListingId).OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(e => e.ReportedUser).WithMany().HasForeignKey(e => e.ReportedUserId).OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(e => e.Handler).WithMany().HasForeignKey(e => e.HandledBy).OnDelete(DeleteBehavior.SetNull);

            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.ListingId);
            entity.HasIndex(e => e.ReportedUserId);
        });

        // Dispute
        modelBuilder.Entity<Dispute>(entity =>
        {
            entity.ToTable("disputes");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.OrderId).HasColumnName("order_id");
            entity.Property(e => e.OpenedBy).HasColumnName("opened_by");
            entity.Property(e => e.Status).HasColumnName("status").HasConversion<string>();
            entity.Property(e => e.AssignedInspectorId).HasColumnName("assigned_inspector_id");
            entity.Property(e => e.AssignedAdminId).HasColumnName("assigned_admin_id");
            entity.Property(e => e.Summary).HasColumnName("summary").IsRequired();
            entity.Property(e => e.Resolution).HasColumnName("resolution");
            entity.Property(e => e.ResolvedAt).HasColumnName("resolved_at");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

            entity.HasOne(e => e.Order).WithOne(o => o.Dispute).HasForeignKey<Dispute>(e => e.OrderId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.OpenedByUser).WithMany().HasForeignKey(e => e.OpenedBy).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.AssignedInspector).WithMany().HasForeignKey(e => e.AssignedInspectorId).OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(e => e.AssignedAdmin).WithMany().HasForeignKey(e => e.AssignedAdminId).OnDelete(DeleteBehavior.SetNull);

            entity.HasIndex(e => e.OrderId).IsUnique();
            entity.HasIndex(e => e.Status);
        });

        // DisputeEvent
        modelBuilder.Entity<DisputeEvent>(entity =>
        {
            entity.ToTable("dispute_events");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.DisputeId).HasColumnName("dispute_id");
            entity.Property(e => e.ActorId).HasColumnName("actor_id");
            entity.Property(e => e.Message).HasColumnName("message").IsRequired();
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");

            entity.HasOne(e => e.Dispute).WithMany(d => d.Events).HasForeignKey(e => e.DisputeId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Actor).WithMany().HasForeignKey(e => e.ActorId).OnDelete(DeleteBehavior.SetNull);

            entity.HasIndex(e => e.DisputeId);
        });

        // Notification
        modelBuilder.Entity<Notification>(entity =>
        {
            entity.ToTable("notifications");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.Type).HasColumnName("type").HasConversion<string>().IsRequired();
            entity.Property(e => e.Title).HasColumnName("title").HasMaxLength(255).IsRequired();
            entity.Property(e => e.Message).HasColumnName("message").IsRequired();
            entity.Property(e => e.IsRead).HasColumnName("is_read");
            entity.Property(e => e.RelatedEntityId).HasColumnName("related_entity_id");
            entity.Property(e => e.RelatedEntityType).HasColumnName("related_entity_type").HasMaxLength(50);
            entity.Property(e => e.ActionUrl).HasColumnName("action_url");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");

            entity.HasOne(e => e.User).WithMany().HasForeignKey(e => e.UserId).OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.IsRead);
            entity.HasIndex(e => e.CreatedAt);
        });

        // ChatConversation
        modelBuilder.Entity<ChatConversation>(entity =>
        {
            entity.ToTable("chat_conversations");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ListingId).HasColumnName("listing_id");
            entity.Property(e => e.BuyerId).HasColumnName("buyer_id");
            entity.Property(e => e.SellerId).HasColumnName("seller_id");
            entity.Property(e => e.LastMessageAt).HasColumnName("last_message_at");
            entity.Property(e => e.LastMessage).HasColumnName("last_message");
            entity.Property(e => e.LastMessageSenderId).HasColumnName("last_message_sender_id");
            entity.Property(e => e.UnreadCountBuyer).HasColumnName("unread_count_buyer");
            entity.Property(e => e.UnreadCountSeller).HasColumnName("unread_count_seller");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");

            entity.HasOne(e => e.Listing).WithMany().HasForeignKey(e => e.ListingId).OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(e => e.Buyer).WithMany().HasForeignKey(e => e.BuyerId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Seller).WithMany().HasForeignKey(e => e.SellerId).OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => e.BuyerId);
            entity.HasIndex(e => e.SellerId);
            entity.HasIndex(e => e.ListingId);
            entity.HasIndex(e => new { e.BuyerId, e.SellerId, e.ListingId }).IsUnique();
        });

        // ChatMessage
        modelBuilder.Entity<ChatMessage>(entity =>
        {
            entity.ToTable("chat_messages");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ConversationId).HasColumnName("conversation_id");
            entity.Property(e => e.SenderId).HasColumnName("sender_id");
            entity.Property(e => e.Content).HasColumnName("content").IsRequired();
            entity.Property(e => e.IsRead).HasColumnName("is_read");
            entity.Property(e => e.ReadAt).HasColumnName("read_at");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");

            entity.HasOne(e => e.Conversation).WithMany(c => c.Messages).HasForeignKey(e => e.ConversationId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Sender).WithMany().HasForeignKey(e => e.SenderId).OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => e.ConversationId);
            entity.HasIndex(e => e.CreatedAt);
        });
    }
}
