namespace DuSoleil.Infrastructure.Auth;

/// <summary>Сервис производного ключа пароля (как в homework PbKdf1Service).</summary>
public interface IKdfService
{
    string Dk(string password, string salt);
}
