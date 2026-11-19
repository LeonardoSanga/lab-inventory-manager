using LabRobotica.Context;
using LabRobotica.Data;
using LabRobotica.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login";
        options.AccessDeniedPath = "/Home/AcessoNegado";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(20);
        options.Cookie.Name = "LabRobotica.Auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
    });

// Chaves de criptografia dos cookies de login. Em produção (Docker) elas são gravadas em um
// volume, para que os usuários não sejam deslogados toda vez que o container reiniciar.
var dataProtectionPath = builder.Configuration["DataProtection:KeysPath"];
if (!string.IsNullOrWhiteSpace(dataProtectionPath))
{
    builder.Services.AddDataProtection()
        .SetApplicationName("LabRobotica")
        .PersistKeysToFileSystem(new DirectoryInfo(dataProtectionPath));
}

// Add services to the container.
builder.Services.AddControllersWithViews();

var mySqlConnection = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(mySqlConnection))
{
    throw new InvalidOperationException(
        "Connection string 'DefaultConnection' não configurada. " +
        "Defina-a no appsettings.Development.json (desenvolvimento) ou na variável de ambiente " +
        "ConnectionStrings__DefaultConnection (produção).");
}

// Versão fixa do MySQL (em vez de AutoDetect) para que a aplicação consiga iniciar
// mesmo que o banco ainda não esteja pronto no momento do boot.
var mySqlVersion = Version.TryParse(builder.Configuration["Database:ServerVersion"], out var v) ? v : new Version(8, 4, 0);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(mySqlConnection, new MySqlServerVersion(mySqlVersion), mysql =>
        mysql.EnableRetryOnFailure(maxRetryCount: 5, maxRetryDelay: TimeSpan.FromSeconds(10), errorNumbersToAdd: null)));

builder.Services.AddScoped<IItemRepository, ItemRepository>();
builder.Services.AddScoped<IAuthRepository, AuthRepository>();
builder.Services.AddScoped<IEstoqueRepository, EstoqueRepository>();
builder.Services.AddScoped<IMovimentacaoRepository, MovimentacaoRepository>();
builder.Services.AddScoped<IEmprestimoRepository, EmprestimoRepository>();
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<ILogAuditoriaRepository, LogAuditoriaRepository>();

// Necessário caso, no futuro, a aplicação fique atrás de um proxy reverso (Nginx, IIS etc.).
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

var app = builder.Build();

await InicializarBancoAsync(app);

app.UseForwardedHeaders();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseStatusCodePagesWithReExecute("/Home/Error", "?codigo={0}");

// HTTPS só é forçado se configurado (Https:Enabled = true). No servidor da faculdade
// o acesso é via HTTP na rede local, então fica desligado por padrão.
if (app.Configuration.GetValue<bool>("Https:Enabled"))
{
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();
app.MapGet("/health", () => Results.Ok("ok")).AllowAnonymous();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Item}/{action=List}/{id?}");

app.Run();

// Aplica as migrations pendentes e cria o usuário administrador inicial (se não houver nenhum).
// Controlado pelas configurações Database:MigrateOnStartup e Admin:Login / Admin:Senha.
static async Task InicializarBancoAsync(WebApplication app)
{
    var config = app.Configuration;
    var logger = app.Logger;

    if (!config.GetValue<bool>("Database:MigrateOnStartup"))
    {
        return;
    }

    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    // Aguarda o MySQL ficar disponível (ex.: logo após o "docker compose up").
    const int tentativas = 30;
    for (var i = 1; ; i++)
    {
        try
        {
            logger.LogInformation("Aplicando migrations no banco de dados (tentativa {Tentativa}/{Total})...", i, tentativas);
            await db.Database.MigrateAsync();
            logger.LogInformation("Banco de dados atualizado.");
            break;
        }
        catch (Exception ex) when (i < tentativas)
        {
            logger.LogWarning("Banco ainda indisponível: {Mensagem}. Nova tentativa em 5s.", ex.Message);
            await Task.Delay(TimeSpan.FromSeconds(5));
        }
    }

    var adminLogin = config["Admin:Login"];
    var adminSenha = config["Admin:Senha"];

    if (await db.Usuarios!.AnyAsync(u => u.Administrador && u.Status != "I"))
    {
        return;
    }

    if (string.IsNullOrWhiteSpace(adminLogin) || string.IsNullOrWhiteSpace(adminSenha))
    {
        logger.LogWarning("Nenhum administrador ativo no banco e Admin:Login/Admin:Senha não configurados. " +
                          "Não será possível acessar o sistema até que um administrador seja criado.");
        return;
    }

    // Se já existir um usuário com esse login (ex.: admin inativado por engano), ele é reativado.
    var existente = await db.Usuarios!.FirstOrDefaultAsync(u => u.Login == adminLogin);
    if (existente != null)
    {
        existente.Senha = BCrypt.Net.BCrypt.HashPassword(adminSenha);
        existente.Administrador = true;
        existente.Status = "A";
        logger.LogInformation("Usuário '{Login}' reativado como administrador.", adminLogin);
    }
    else
    {
        db.Usuarios!.Add(new Usuario
        {
            Login = adminLogin,
            Senha = BCrypt.Net.BCrypt.HashPassword(adminSenha),
            Administrador = true,
            Status = "A"
        });
        logger.LogInformation("Usuário administrador inicial '{Login}' criado.", adminLogin);
    }
    await db.SaveChangesAsync();
}
