namespace App.Root.Voip;
using NAudio.Wave;
using Concentus;

class PlayerAudioSource {
    private const int SAMPLE_RATE = 16000;
    private const int FRAME_SIZE = 960;
    private const int MIN_BUFFER_MS = 120;

    private IOpusDecoder decoder;
    private BufferedWaveProvider buffer;
    private WaveOutEvent waveOut;
    private VolumeWaveProvider16 volumeProvider;

    private float volume = 1.0f;
    private int lastSequence = -1;
    private bool playbackStarted = false;

    public PlayerAudioSource() {
        decoder = OpusCodecFactory.CreateDecoder(SAMPLE_RATE, 1);

        buffer = new BufferedWaveProvider(new WaveFormat(SAMPLE_RATE, 16, 1));
        buffer.BufferDuration = TimeSpan.FromSeconds(1);
        buffer.DiscardOnBufferOverflow = true;

        volumeProvider = new VolumeWaveProvider16(buffer);
        volumeProvider.Volume = volume;

        waveOut = new WaveOutEvent();
        waveOut.Init(volumeProvider);
        Console.WriteLine($"buffer buffered: {buffer.BufferedBytes} duration: {buffer.BufferedDuration}");
    }

    /**
    
        Play

        */
    public void play(byte[] encodedAudio, int sequence) {
        if(sequence <= lastSequence) return;
        lastSequence = sequence;

        short[] pcmShort = new short[FRAME_SIZE];
        decoder.Decode(encodedAudio.AsSpan(), pcmShort.AsSpan(), FRAME_SIZE);

        byte[] bytes = new byte[pcmShort.Length * 2];
        Buffer.BlockCopy(pcmShort, 0, bytes, 0, bytes.Length);
        buffer.AddSamples(bytes, 0, bytes.Length);

        if(!playbackStarted && buffer.BufferDuration.TotalMilliseconds >= MIN_BUFFER_MS) {
            waveOut.Play();
            playbackStarted = true;
        }
    }

    /**
    
        Dispose

        */ 
    public void dispose() {
        waveOut.Stop();
        waveOut.Dispose();
    }
}