namespace Template.Core.App.Common;

/// <summary>Hash e verificação de senhas (implementação atual: BCrypt).</summary>
public interface IPasswordHasher
{
    string Hash(string senha);
    bool Verificar(string senha, string hash);
}
