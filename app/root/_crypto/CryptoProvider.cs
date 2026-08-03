namespace App.Root._Crypto;

public class CryptoProvider {
    private byte[] key;
    private byte[] iv;

    public CryptoProvider(byte[] key, byte[] iv) {
        this.key = key;
        this.iv = iv;
    }
}