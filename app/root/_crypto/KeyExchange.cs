namespace App.Root._Crypto;
using App.Root._Binary;
using System.Security.Cryptography;

public class KeyExchange {
    private const long PRIME = 2147483647;
    private const long GENERATOR = 5;

    private static Random random = new Random();

    // Generate Private Key
    public static long GeneratePrivateKey() {
        long val = random.Next(2, 1000000);
        return val;
    }

    // Generate Session Id
    public static string GenerateSessionId() {
        string val = Guid.NewGuid().ToString();
        return val;
    }

    // Compute Public Key
    public static long ComputePublicKey(long privateKey) {
        long val = ModPow(GENERATOR, privateKey, PRIME);
        return val;
    }

    // COompute Shared Secret
    public static long ComputeSharedSecret(long privateKey, long otherPubicKey) {
        long val = ModPow(otherPubicKey, privateKey, PRIME);
        return val;
    }

    // Mod Pow
    private static long ModPow(long baseValue, long exponent, long modulus) {
        long result = 1;
        baseValue %= modulus;

        while(exponent > 0) {
            if((exponent & 1) == 1) {
                result = (result * baseValue) % modulus;
            }

            exponent >>= 1;
            baseValue = (baseValue * baseValue) % modulus;
        }

        return result;
    }

    // Derive Key Iv
    public static (byte[] key, byte[] iv) DeriveKeyIv(long sharedSecret) {
        byte[] secretBytes = BitConverter.GetBytes(sharedSecret);
        
        var sha = SHA256.Create();
        using(var s = sha) {
            byte[] hash = s.ComputeHash(secretBytes);

            byte[] key = new byte[32];
            byte[] iv = new byte[16];

            Array.Copy(hash, 0, key, 0, 32);
            Array.Copy(hash, 16, iv, 0, 16);

            return (key, iv);
        } 
    }

    /**
     *
     * Create Handshake
     *
     */
    public static byte[] CreateHandshake(long publicKey, string sessionId) {
        using var writer = new BinaryWriter();
        
        writer.Write(publicKey);
        writer.Write(sessionId ?? "");
        
        return writer.GetBytes();
    }

    /**
     *
     * Parse Handshake
     *
     */
    public static (long publicKey, string sessionId) ParseHandshake(byte[] data) {
        using var reader = new BinaryReader(data);
        
        long publicKey = reader.ReadLong();
        string sessionId = reader.ReadString();

        return (publicKey, sessionId);
    }
}