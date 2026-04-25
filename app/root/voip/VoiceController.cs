namespace App.Root.Voip;
using App.Root.Packets;
using NAudio.Wave;
using Concentus;
using Concentus.Enums;

class VoiceController {
    private static VoiceController? instance;
    private Dictionary<string, PlayerAudioSource> audioSources = new();

    private const int SAMPLE_RATE = 16000;
    private const int CHANNELS = 1;
    private const int FRAME_SIZE = 960;
    
    private int sendSequence = 0;

    private WaveInEvent? waveIn;
    private IOpusEncoder? encoder;
    private Network? network;

    public static VoiceController getInstance() {
        instance ??= new VoiceController();
        return instance;
    }

    // Set Network
    public void setNetwork(Network network) {
        this.network = network;
    }

    // On Audio Captured
    private void onAudioCaptured(object? sender, WaveInEventArgs e) {
        if(encoder == null || network == null) return;

        short[] pcm = new short[e.BytesRecorded / 2];
        Buffer.BlockCopy(e.Buffer, 0, pcm, 0, e.BytesRecorded);
        if(pcm.Length < FRAME_SIZE) return;

        byte[] encoded = new byte[1275];
        int len = encoder.Encode(
            pcm.AsSpan(0, FRAME_SIZE),
            FRAME_SIZE,
            encoded.AsSpan(),
            encoded.Length
        );

        network.getClient()?.send(new PacketVoice {
            userId = network.userId,
            audio = encoded[..len],
            sequence = sendSequence++
        });
    }

    // Remove Player
    public void removePlayer(string userId) {
        if(audioSources.TryGetValue(userId, out var source)) {
            source.dispose();
            audioSources.Remove(userId);
        }
    }

    /**
    
        Receive

        */
    public void receive(string userId, byte[] encodedAudio, int sequence) {
        if(!audioSources.TryGetValue(userId, out var source)) {
            source = new PlayerAudioSource();
            audioSources[userId] = source;
        }
        source.play(encodedAudio, sequence);
    }

    /**
    
        Start

        */
    public void start() {
        encoder = OpusCodecFactory.CreateEncoder(
            SAMPLE_RATE,
            CHANNELS,
            OpusApplication.OPUS_APPLICATION_VOIP
        );

        waveIn = new WaveInEvent();
        waveIn.WaveFormat = new WaveFormat(SAMPLE_RATE, 16, CHANNELS);
        waveIn.BufferMilliseconds = 60;
        waveIn.DataAvailable += onAudioCaptured;
        waveIn.StartRecording();

        Console.WriteLine("VoiceController -- capture started");
    }

    /**
    
        Stop

        */
    public void stop() {
        waveIn?.StopRecording();
        waveIn?.Dispose();
        waveIn = null;

        foreach(var source in audioSources.Values) source.dispose();
        audioSources.Clear();

        Console.WriteLine("VoiceController -- capture stopped");
    }
}