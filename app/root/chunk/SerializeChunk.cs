namespace App.Root.Chunk;
using App.Root.Info;
using App.Root.Utils;
using System.Security.Cryptography;
using System.Text;

/**

    Config Helper

    */
static class Config {
    public static bool ENCRYPT_SAVES = false;
    public static string CHUNK_KEY => Env.GetOrThrow("CHUNK_KEY");
}

/**

    Main Serialize Chunk class

    */
class SerializeChunk {
    private static readonly byte[] MAGIC = Encoding.ASCII.GetBytes("SCS1");
    private static readonly int VERSION = 1;

    private static readonly string SAVE_DIR = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "root", ".INFO-DATA"));
    private static readonly string SAVE_FILE = Path.Combine(SAVE_DIR, "c.d.mp.scs");

    // Encrypt
    private static byte[] encrypt(byte[] data, byte[] iv) {
        using var aes = Aes.Create();
        
        aes.Key = deriveKey(iv);
        aes.IV = iv;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        using var encryptor = aes.CreateEncryptor();
        byte[] val = encryptor.TransformFinalBlock(data, 0, data.Length);
        return val;
    }

    // Decrypt
    private static byte[] decrypt(byte[] data, byte[] iv) {
        using var aes = Aes.Create();
        
        aes.Key = deriveKey(iv);
        aes.IV = iv;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        using var decryptor = aes.CreateDecryptor();
        byte[] val = decryptor.TransformFinalBlock(data, 0, data.Length);
        return val;
    }

    // Derive Key
    private static byte[] deriveKey(byte[] iv) {
        byte[] secret = Convert.FromBase64String(Config.CHUNK_KEY);
        
        string playerId = InfoController.getInstance().userInfo.getId();
        byte[] playerBytes = Encoding.UTF8.GetBytes(playerId);

        byte[] salt = Encoding.UTF8.GetBytes(playerId);

        Buffer.BlockCopy(playerBytes, 0, salt, 0, playerBytes.Length);
        Buffer.BlockCopy(iv, 0, salt, playerBytes.Length, iv.Length);

        int i = 100_000;
        using var pbkdf2 = new Rfc2898DeriveBytes(secret, salt, i, HashAlgorithmName.SHA256);
        byte[] val = pbkdf2.GetBytes(32);
        return val;
    }

    /**
     *
     * Save
     * 
     */
    public static void save(Dictionary<ChunkCoord, ChunkData> chunks) {
        Directory.CreateDirectory(SAVE_DIR);

        var chunkList = chunks.Values.Where(c => c.isGenerated).ToList();

        using var ms = new MemoryStream();
        using var writer = new BinaryWriter(ms);

        foreach(var chunk in chunkList) {
            writer.Write(chunk.coord.cx);
            writer.Write(chunk.coord.cz);
            writer.Write(chunk.hasMore);
        }

        byte[] payload = ms.ToArray();

        using var file = new FileStream(SAVE_FILE, FileMode.Create, FileAccess.Write);
        using var fileWriter = new BinaryWriter(file);

        fileWriter.Write(MAGIC);
        fileWriter.Write(VERSION);
        fileWriter.Write(chunkList.Count);

        if(Config.ENCRYPT_SAVES && !string.IsNullOrEmpty(Config.CHUNK_KEY)) {
            byte[] iv = RandomNumberGenerator.GetBytes(16);
            byte[] encrypted = encrypt(payload, iv);
            fileWriter.Write(iv);
            fileWriter.Write(encrypted.Length);
            fileWriter.Write(encrypted);
        } else {
            fileWriter.Write(new byte[16]);
            fileWriter.Write(payload.Length);
            fileWriter.Write(payload);
        }

        Console.WriteLine($"[ChunkSerializer] Saved {chunkList.Count} chunks to {SAVE_FILE}");
    }

    /**
     *
     * Load
     * 
     */
    public static Dictionary<ChunkCoord, ChunkData> load() {
        var result = new Dictionary<ChunkCoord, ChunkData>();

        if(!File.Exists(SAVE_FILE)) {
            Console.WriteLine("[ChunkSerializer] No save file found, starting with new one.");
            return result;
        }

        try {
            using var file = new FileStream(SAVE_FILE, FileMode.Open, FileAccess.Read);
            using var reader = new BinaryReader(file);

            byte[] magic = reader.ReadBytes(4);
            if(!magic.SequenceEqual(MAGIC)) {
                Console.Error.WriteLine("[ChunkSerializer] Invalid save file!");
                return result;
            }

            int version = reader.ReadInt32();
            int chunkCount = reader.ReadInt32();
            byte[] iv = reader.ReadBytes(16);
            int payloadLength = reader.ReadInt32();
            byte[] payload = reader.ReadBytes(payloadLength);

            if(Config.ENCRYPT_SAVES && !string.IsNullOrEmpty(Config.CHUNK_KEY)) {
                payload = decrypt(payload, iv);
            }

            using var ms = new MemoryStream(payload);
            using var payloadReader = new BinaryReader(ms);

            for(int i = 0; i < chunkCount; i++) {
                int cx = payloadReader.ReadInt32();
                int cz = payloadReader.ReadInt32();

                bool hasMore = payloadReader.ReadBoolean();

                var coord = new ChunkCoord(cx, 0, cz);

                result[coord] = new ChunkData(coord, hasMore);
            }

            Console.WriteLine($"[ChunkSerializer] Loaded {result.Count} chunks.");
        } catch(Exception ex) {
            Console.Error.WriteLine($"[ChunkSerializer] Failed to load: {ex.Message}");
        }

        return result;
    }
}

