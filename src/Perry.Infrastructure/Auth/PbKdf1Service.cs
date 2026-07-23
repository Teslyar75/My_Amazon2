using System.Security.Cryptography;
using System.Text;

namespace Perry.Infrastructure.Auth;

/// <summary>
/// PBKDF1-подобный хеш из homework (SHA1 + итерации).
/// Нужен, чтобы пароль Admin/Admin из seed совпадал.
/// </summary>
public class PbKdf1Service : IKdfService
{
    private const int Iterations = 3;
    private const int DkLen = 20;

    public string Dk(string password, string salt)
    {
        var t = Hash(password + salt);
        for (var i = 0; i < Iterations - 1; i++)
            t = Hash(t);

        return t[..DkLen];
    }

    private static string Hash(string input) =>
        Convert.ToHexString(SHA1.HashData(Encoding.UTF8.GetBytes(input)));
}
