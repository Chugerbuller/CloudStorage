using System.Security.Cryptography;
using System.Text;

namespace CloudStore.WebApi.Helpers;

public class HashHelper
{
    private readonly MD5 _md5Hash = MD5.Create();

    public string ConvertStringToHash(string str)
    {
        var bytes = Encoding.Unicode.GetBytes(str);
        var hash = _md5Hash.ComputeHash(bytes);
        return Convert.ToHexString(hash);
    }
    public string ConvertStringWishShuffleToHash(string str)
    {
        var shuffleString = this.shuffleString(str);
        var bytes = Encoding.Unicode.GetBytes(str);
        var hash = _md5Hash.ComputeHash(bytes);
        return Convert.ToHexString(hash);
    }
    private string shuffleString(string str)
    {
        char[] array = str.ToCharArray();
        Random rng = new Random();
        int n = array.Length;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            var value = array[k];
            array[k] = array[n];
            array[n] = value;
        }
        return new string(array);
    }
}
