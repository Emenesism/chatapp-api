namespace ChatApp.Application.Auth.Login;

public record LoginOrRegisterCommand(string Name, string Phone, string Password);
