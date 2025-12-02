using RoslynBlocklyTranspiler;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services.AddCors(options => {
    options.AddPolicy(name: "corspolicy", policy => policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

WebApplication app = builder.Build();

app.UseCors("corspolicy");

if (app.Environment.IsDevelopment()) {
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapPost("/code-to-blocks", (CodeRequest request) => Transpiler.textToBlocks(request.code));

app.Run();

record CodeRequest(string code);

record BlockResponse(string blocks);


