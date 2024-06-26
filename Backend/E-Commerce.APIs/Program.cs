
using E_Commerce.APIs.Controllers;
using E_Commerce.APIs.Helpers;
using E_Commerce.Core.Entities;
using E_Commerce.Core.Entities.Identity;
using E_Commerce.Core.Repositories.Contract;
using E_Commerce.Core.Services.Contract;
using E_Commerce.Repository;
using E_Commerce.Repository.Data;
using E_Commerce.Service;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using StackExchange.Redis;
using Talabat.APIs.Extensions;

namespace E_Commerce.APIs
{
    public class Program
    {
        public  static async Task Main(string[] args)
        {
            var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";
            var builder = WebApplication.CreateBuilder(args);

           

            // Add services to the container.
            builder.Services.AddCors(options =>
            {
                options.AddPolicy(MyAllowSpecificOrigins,
                builder =>
                {
                    builder.AllowAnyOrigin();
                    builder.AllowAnyMethod();
                    builder.AllowAnyHeader();
                });
            });

            builder.Services.AddControllers();
            builder.Services.AddSwaggerServices();


            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddDbContext<ProjectContext>(op => 
            op.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")
            ));


            //builder.Services.AddSingleton<IConnectionMultiplexer>((ServiceProvider) =>
            //{
            //    var connection = builder.Configuration.GetConnectionString("Redis");
            //    return ConnectionMultiplexer.Connect(connection);
            //});

            // Configure StackExchange.Redis
            builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
            {
                var configuration = ConfigurationOptions.Parse(builder.Configuration.GetConnectionString("Redis"));
                return ConnectionMultiplexer.Connect(configuration);
            });

            // Register IDatabase from IConnectionMultiplexer
            builder.Services.AddScoped(sp =>
            {
                var connectionMultiplexer = sp.GetRequiredService<IConnectionMultiplexer>();
                return connectionMultiplexer.GetDatabase();
            });



            builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericReposity<>));
            builder.Services.AddScoped(typeof(IUnitOfWork), typeof(UnitOfWork));
            builder.Services.AddScoped(typeof(IAuthService), typeof(AuthService));
            builder.Services.AddScoped(typeof(IOrderService), typeof(OrderService));

            builder.Services.AddScoped(typeof(IReviewRepository<>), typeof(ReviewRepository<>));
            builder.Services.AddScoped(typeof(IGenericRepositoryUser<>), typeof(GenericRepoUser<>));

            builder.Services.AddScoped<IColorRepository, ColorRepository>();
            builder.Services.AddScoped<ICartRepositery,CartReposetory>();
            builder.Services.AddScoped<IWishlistRepository,WishlistRepository>();
            builder.Services.AddScoped<IOrderRepo, OrderRepo>();
            builder.Services.AddScoped<ISellerRepository, SellerRepository>();


            builder.Services.AddAutoMapper(typeof(MappingProfile));

            builder.Services.AddIdentity<AppUser, IdentityRole>
            (options =>
            {

            } ).AddEntityFrameworkStores<ProjectContext>();


            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAllOrigins",
                    builder =>
                    {
                        builder.AllowAnyOrigin()
                               .AllowAnyMethod()
                               .AllowAnyHeader();
                    });
            });

          




            builder.Services.AddApplicationServices();

     builder.Services.AddCors(options =>
            {
                options.AddPolicy(MyAllowSpecificOrigins,
                builder =>
                {
                    builder.AllowAnyHeader();
                    builder.AllowAnyMethod();
                    builder.AllowAnyOrigin();

                });

            });


            var app = builder.Build();
            var staticPath = Path.Combine(Environment.CurrentDirectory, "Images");
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(staticPath),
                RequestPath = "/Images"

            });

            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var loggerfactory = services.GetRequiredService<ILoggerFactory>();

                var dbContext = services.GetRequiredService<ProjectContext>();
                var _manager = services.GetRequiredService<UserManager<AppUser>>();

                try
                {
                    await dbContext.Database.MigrateAsync();//update-database

                    var _userManager = services.GetRequiredService<UserManager<AppUser>>();
                    await AppIdentityDbContextDataSeed.SeedUserAsync(_userManager);
                    //await ProjectContextSeed.SeedAsync(dbContext);

                }
                catch (Exception ex)
                {

                    var logger = loggerfactory.CreateLogger<Program>();
                    logger.LogError(ex, "an error occured while apply migration");
                }
                finally
                {
                    scope.Dispose();

                }
            }
            app.UseCors(MyAllowSpecificOrigins);

            app.UseMiddleware<ExceptionMiddleware>();
          


            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            app.UseCors("AllowAllOrigins");


            app.UseCors(MyAllowSpecificOrigins);
            app.UseHttpsRedirection();

            app.UseAuthorization();
            app.UseStaticFiles();

            app.MapControllers();

            app.Run();
        }


      
    }
}
