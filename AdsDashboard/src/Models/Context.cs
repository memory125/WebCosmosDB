namespace AdsDashboard.Models
{
    using System.Data.Entity;

    public class Context : DbContext
    {
        public Context() : base("name=Context")
        {
        }

        public DbSet<AdsItem> Items { get; set; }
    }
}
