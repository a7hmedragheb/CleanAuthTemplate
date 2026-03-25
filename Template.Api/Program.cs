using Hangfire;
using Hangfire.Dashboard;
using HangfireBasicAuthenticationFilter;
using Template.Api;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddDependencies(builder.Configuration);

var app = builder.Build();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// https://localhost:7131/jobs
app.UseHangfireDashboard("/jobs", new DashboardOptions
{
	Authorization =
	[
		new HangfireCustomBasicAuthenticationFilter
		{
			User = builder.Configuration.GetValue<string>("HangfireSettings:Username"),
			Pass = builder.Configuration.GetValue<string>("HangfireSettings:Password"),
		}
	],
	DashboardTitle = "Auth Template Service - Job Dashboard",
	IsReadOnlyFunc = (DashboardContext context) => true
});

app.UseCors();

app.UseAuthorization();

app.UseExceptionHandler();

app.MapControllers();

app.Run();
