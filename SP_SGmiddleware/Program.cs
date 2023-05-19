var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
//using Microsoft.AspNetCore.Hosting;
//using Microsoft.AspNetCore;
//using Microsoft.OpenApi.Models;
//using Serilog;
//using System.Reflection;

//namespace SP_SGmiddleware
//{

//    public class Program
//    {

//        public static void Main(string[] args)
//        {
//            var builder = WebApplication.CreateBuilder(args);
//            //var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
//            //XmlConfigurator.Configure(logRepository, new FileInfo("log4net.config"));
//            //builder.Services.AddControllersWithViews();
//            builder.Services.AddControllers();
//            builder.Services.AddHttpClient();
//            builder.Services.AddEndpointsApiExplorer();
//            builder.Services.AddSwaggerGen();

//            //New
//            builder.Services.AddSwaggerGen(option =>
//            {
//                option.SwaggerDoc("v1", new OpenApiInfo { Title = "Sharepoint APIs", Version = "v1" });
//                option.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
//                {
//                    In = ParameterLocation.Header,
//                    Description = "Please enter a valid token",
//                    Name = "Authorization",
//                    Type = SecuritySchemeType.Http,
//                    BearerFormat = "JWT",
//                    Scheme = "Bearer"
//                });
//                option.AddSecurityRequirement(new OpenApiSecurityRequirement
//                {
//                    {
//                    new OpenApiSecurityScheme
//                    {
//                        Reference = new OpenApiReference
//                            {
//                                Type=ReferenceType.SecurityScheme,
//                                Id="Bearer"
//                            }
//                    },
//                    new string[]{}
//                    }
//                });
//            });


//            var devCorsPolicy = "devCorsPolicy";
//            builder.Services.AddCors(options =>
//            {
//                options.AddPolicy(devCorsPolicy, builder => {
//                    builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
//                });
//            });
//            var app = builder.Build();
//            // Configure the HTTP request pipeline.

//            if (!app.Environment.IsDevelopment())
//            {
//                app.UseExceptionHandler("/Home/Error");
//                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
//                app.UseHsts();
//            }
//            if(app.Environment.IsDevelopment())
//            {
//                app.UseSwagger();
//                app.UseSwaggerUI(c =>
//                {
//                    c.SwaggerEndpoint("v1/swagger.json", "Sharepoint API");
//                    //c.RoutePrefix = "docs";
//                    c.DocumentTitle = "My API - Swagger UI";
//                    //c.InjectStylesheet("/swagger-ui/custom.css");
//                    c.EnableFilter();
//                });
//            }

//            app.UseHttpsRedirection();
//            app.UseStaticFiles();

//            app.UseRouting();

//            app.UseAuthorization();

//            app.MapControllerRoute(
//                name: "default",
//                pattern: "{controller=Home}/{action=Index}/{id?}");

//            app.Run();
//            CreateWebHostBuilder(args).Build().Run();
//        }



//        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
//        WebHost.CreateDefaultBuilder(args);


//    }
//}






