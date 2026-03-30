namespace App.Root.Voip;
using NAudio.Wave;
using Concentus.Structs;
using Concentus;

class PlayerAudioSource {
    private const int SAMPLE_RATE = 16000;
    private const int FRAME_SIZE = 960;

    private IOpusDecoder decoder;
    private BufferedWaveProvider buffer;
    private WaveOutEvent waveOut;
    private VolumeWaveProvider16 volumeProvider;

    private float volume = 1.0f;

    public PlayerAudioSource() {
        decoder = OpusCodecFactory.CreateDecoder(SAMPLE_RATE, 1);

        buffer = new BufferedWaveProvider(new WaveFormat(SAMPLE_RATE, 16, 1));
        buffer.BufferDuration = TimeSpan.FromSeconds(1);
        buffer.DiscardOnBufferOverflow = true;

        volumeProvider = new VolumeWaveProvider16(buffer);
        volumeProvider.Volume = volume;

        waveOut = new WaveOutEvent();
        waveOut.Init(volumeProvider);
        waveOut.Play();
    }

    ///
    /// Play
    /// 
    public void play(byte[] encodedAudio) {
        short[] pcm = new short[FRAME_SIZE];
        decoder.Decode(
            encodedAudio.AsSpan(),
            pcm.AsSpan(),
            FRAME_SIZE
        );

        short[] pcmShort = new short[FRAME_SIZE];
        for(int i = 0; i < FRAME_SIZE; i++) {
            pcmShort[i] = (short)(pcm[i] * short.MaxValue);
        }

        byte[] bytes = new byte[pcmShort.Length * 2];
        Buffer.BlockCopy(pcmShort, 0, bytes, 0, bytes.Length);
        buffer.AddSamples(bytes, 0, bytes.Length);
    }

    ///
    /// Dispose
    /// 
    public void dispose() {
        waveOut.Stop();
        waveOut.Dispose();
    }
}