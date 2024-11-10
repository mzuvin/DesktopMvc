using System.Reflection;
using Web.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;

namespace Web;

public class Startup
{
    private readonly IConfiguration _configuration;

    public Startup(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public void ConfigureServices(IServiceCollection services)
    {
        var connectionString = _configuration.GetConnectionString("DefaultConnection")
                               ?? throw new InvalidOperationException(
                                   "Connection string 'DefaultConnection' not found.");

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlite(connectionString));

        services.AddDatabaseDeveloperPageExceptionFilter();

        services.AddDefaultIdentity<IdentityUser>(options => { options.SignIn.RequireConfirmedAccount = true; })
            .AddEntityFrameworkStores<ApplicationDbContext>();

        services.AddControllersWithViews();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
            app.UseMigrationsEndPoint(); 
        }
        else
        {
            app.UseExceptionHandler("/Home/Error");
            app.UseHsts(); 
        }

        app.UseHttpsRedirection();
        var embeddedProvider = new Web.Helper.EmbeddedFileProvider(Assembly.GetExecutingAssembly());
        app.UseFileServer(new FileServerOptions
        {
            FileProvider = embeddedProvider,
            RequestPath = "",
            EnableDirectoryBrowsing = false
        });
        //app.UseStaticFiles();
        app.UseRouting(); 

        app.UseAuthorization(); 

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");
            endpoints.MapRazorPages(); 
        });
    }
}