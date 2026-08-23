namespace Template.Core.App.Common;

public class BCryptPasswordHasher : IPasswordHasher
{
    public string Hash(string senha) => BCrypt.Net.BCrypt.HashPassword(senha);

    public bool Verificar(string senha, string hash)
    {
        try
        {
            return BCrypt.Net.BCrypt.Verify(senha, hash);
        }
        catch
        {
            // Hash malformado/legado — trata como não-correspondente em vez de estourar.
            return false;
        }
    }
}
