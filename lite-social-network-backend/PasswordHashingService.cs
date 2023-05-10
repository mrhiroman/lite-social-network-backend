using System.Security.Cryptography;
using System.Text;

public class PasswordHashingService
{
    private string CreateSalt()
    {
        char[] letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789".ToCharArray();

        Random rand = new Random();
        string result = "";
        for (int j = 1; j <= 32; j++)
        {
            int number = rand.Next(0, letters.Length - 1);
            result += letters[number];
        }

        return result;
        
    }

    public string GetSHA256(string password)
    {
        using (SHA256 sha256Hash = SHA256.Create())  
        {
            byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(password));
            StringBuilder builder = new StringBuilder();  
            
            for (int i = 0; i < bytes.Length; i++)  
            {  
                builder.Append(bytes[i].ToString("x2"));  
            }  
            
            return builder.ToString();  
        }  
    }
}