namespace App.Root._Crypto;
using System.Security.Cryptography;
using System.Text;

public class CryptoProvider {
    private byte[] key;
    private byte[] iv;

    public CryptoProvider(byte[] key, byte[] iv) {
        this.key = key;
        this.iv = iv;
    }

    // Generate Session Key
    public static (byte[] key, byte[] iv) GenerateSessionKey() {
        var key = new byte[32];
        var iv = new byte[16];

        var num = RandomNumberGenerator.Create();
        using(var rng = num) {
            rng.GetBytes(key);
            rng.GetBytes(iv);
        }

        return (key, iv);
    }

    // Compute Hash
    public static byte[] ComputeHash(byte[] data) {
        var sha256 = SHA256.Create();
        using(var s = sha256) {
            byte[] val = s.ComputeHash(data);
            return val;
        }
    }

    public static byte[] ComputeHash(string data) {
        var b = Encoding.UTF8.GetBytes(data);
        byte[] val = ComputeHash(b);
        return val;
    }

    // Verify Hash
    public static bool VerifyHash(byte[] data, byte[] hash) {
        var computed = ComputeHash(data);
        if(computed.Length != hash.Length) return false;

        for(int i = 0; i < computed.Length; i++) {
            if(computed[i] != hash[i]) return false;
        }

        return true;
    }

    /**
     *
     * Encrypt
     *
     */
    public byte[] Encrypt(byte[] data) {
        if(data == null || data.Length == 0) return data!;

        byte[] result = new byte[data.Length];
        
        for(int i = 0; i < data.Length; i++) {
            byte keyByte = key[i % key.Length];
            byte ivByte = iv[i % iv.Length];
            byte encryptByte = (byte)((keyByte ^ ivByte) ^ (i & 0x0FF));

            result[i] = (byte)(data[i] ^ encryptByte);
        }

        return result;
    }

    /**
     *
     * Decrypt
     *
     */
    public byte[] Decrypt(byte[] data) {
        byte[] val = Encrypt(data);
        return val;
    }
}