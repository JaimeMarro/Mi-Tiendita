using InventarioV1.Data;
using Microsoft.EntityFrameworkCore;

namespace InventarioV1
{
    public partial class App : Application
    {
        public App(IDbContextFactory<AppDbContext> dbFactory)
        {
            InitializeComponent();
            using var db = dbFactory.CreateDbContext();
            db.Database.EnsureCreated();
            MainPage = new MainPage();
        }
    }

}
