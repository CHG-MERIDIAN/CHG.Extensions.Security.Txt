﻿using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddSecurityText(textBuilder => textBuilder.ReadFromFile(builder.Environment.WebRootFileProvider.GetFileInfo("companySecurityinfo.txt")));

builder.Services.AddSecurityText(textBuilder => textBuilder.ReadFromConfiguration(builder.Configuration.GetSection("SecurityText")));

builder.Services.AddSecurityText(textBuilder =>
{
	textBuilder
		.SetContact("mailto:security@example.com")
		.SetPolicy("https://example.com/security-policy.html");
});

builder.Services.AddSecurityText(textBuilder =>
{
	textBuilder
		.SetRedirect("https://securitytxt.org/.well-known/security.txt");
});

var app = builder.Build();

app.MapControllers();
app.UseSecurityText();
app.Run();

// Exposes Program.cs to integration tests
public partial class Program { }