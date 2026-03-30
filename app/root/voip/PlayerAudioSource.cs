namespace App.Root.Voip;
using NAudio.Wave;
using Concentus.Structs;

class PlayerAudioSource {
    private const int SAMPLE_RATE = 16000;
    private const int FRAME_SIZE = 960;

    private OpusDecoder decoder;
    private BufferedWaveProvider buffer;
    private WaveOutEvent waveOut;
    private VolumeWaveProvider16 volumeProvider;

    private float volume = 1.0f;

    public PlayerAudioSource() {
        decoder = OpusDecoder.Create(SAMPLE_RATE, 1);

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
            encodedAudio, 0, encodedAudio.Length,
            pcm, 0,
            FRAME_SIZE
        );

        byte[] bytes = new byte[pcm.Length * 2];
        Buffer.BlockCopy(pcm, 0, bytes, 0, bytes.Length);
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