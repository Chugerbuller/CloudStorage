using System.Security.Cryptography;
using System.Text;

namespace CloudStore.WebApi.Helpers;

public class HashHelper
{
    private readonly MD5 _md5Hash = MD5.Create();

    public string ConvertPasswordToHash(string password)
    {
        var bytes = Encoding.Unicode.GetBytes(password);
        var hash = _md5Hash.ComputeHash(bytes);
        return Convert.ToHexString(hash);
    }
}
