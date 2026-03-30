namespace App.Root.Voip;
using App.Root.Packets;
using NAudio.Wave;
using Concentus.Structs;
using Concentus.Enums;

class VoiceController {
    private static VoiceController? instance;
    
    public static VoiceController getInstance() {
        instance ??= new VoiceController();
        return instance;
    }
}