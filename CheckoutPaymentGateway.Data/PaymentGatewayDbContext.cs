namespace CheckoutPaymentGateway.Data
{
    using Microsoft.EntityFrameworkCore;
    using Model.Database;

    public class PaymentGatewayDbContext : DbContext
    {
        public PaymentGatewayDbContext(DbContextOptions<PaymentGatewayDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            BuildPaymentsIndexes(modelBuilder);
        }

        private static void BuildPaymentsIndexes(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Payment>().HasKey(p => p.Id);
            modelBuilder.Entity<Payment>().HasIndex(p => p.TransactionId).IsUnique();
        }

        public DbSet<Payment> Payments { get; set; }
    }
}