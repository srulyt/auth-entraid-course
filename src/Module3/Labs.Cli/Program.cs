using System.CommandLine;
using Labs.Cli.Commands;
using Labs.Cli.Services;

// Initialize services
var configService = new ConfigService();
var tokenCacheService = new TokenCacheService();
var authService = new AuthService(configService, tokenCacheService);
var graphService = new GraphService(authService, configService);

// Create root command
var rootCommand = new RootCommand("Entra Lab CLI - Microsoft Entra ID Public Client Authentication");

// Add all commands
rootCommand.AddCommand(LoginCommand.Create(configService, authService));
rootCommand.AddCommand(LogoutCommand.Create(authService));
rootCommand.AddCommand(AccountCommand.Create(authService));
rootCommand.AddCommand(TokenCommand.Create(authService, tokenCacheService));
rootCommand.AddCommand(GraphCommand.Create(graphService));
rootCommand.AddCommand(ConfigCommand.Create(configService));
rootCommand.AddCommand(DiagnoseCommand.Create(configService, tokenCacheService, authService));

// Execute
return await rootCommand.InvokeAsync(args);
