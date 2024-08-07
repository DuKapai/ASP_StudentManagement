using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using StudentManagement.Data;

namespace StudentManagement
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Configure services
            builder.Services.AddDbContext<StudentManagementContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("StudentManagementContext") ?? throw new InvalidOperationException("Connection string 'StudentManagementContext' not found.")));

            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = "/Accounts/Login";
                    options.AccessDeniedPath = "/Accounts/AccessDenied";
                    options.LogoutPath = "/Accounts/Logout";
                    options.Events = new CookieAuthenticationEvents
                    {
                        OnRedirectToLogin = context =>
                        {
                            context.Response.Redirect(context.RedirectUri);
                            return Task.CompletedTask;
                        }
                    };
                });

            builder.Services.AddAuthorization();
            builder.Services.AddControllersWithViews();

            var app = builder.Build();

            // Configure middleware
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Accounts}/{action=Login}/{id?}");

            app.MapControllerRoute(
                name: "teachergroups",
                pattern: "{controller=TeacherGroups}/{action=Index}/{teacherId?}");

            app.Run();
        }
    }
}
