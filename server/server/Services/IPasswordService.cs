namespace server.Services
{
    public interface IPasswordService
    {
        string Encrypt(string plainText);
        string Decrypt(string cipherText);
    }
}

