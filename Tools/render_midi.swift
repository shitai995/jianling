// macOS build-time tool. Render the original MIDI through Apple's General MIDI synth.
// The Unity runtime plays the resulting audio files and does not depend on macOS APIs.
import AVFoundation
import AudioToolbox
import Foundation

let source = URL(fileURLWithPath: CommandLine.arguments[1])
let destination = URL(fileURLWithPath: CommandLine.arguments[2])
let description = AudioComponentDescription(componentType: kAudioUnitType_MusicDevice,
    componentSubType: kAudioUnitSubType_DLSSynth, componentManufacturer: kAudioUnitManufacturer_Apple,
    componentFlags: 0, componentFlagsMask: 0)
let engine = AVAudioEngine()
let instrument = AVAudioUnitMIDIInstrument(audioComponentDescription: description)
engine.attach(instrument)
let format = AVAudioFormat(standardFormatWithSampleRate: 44100, channels: 2)!
engine.connect(instrument, to: engine.mainMixerNode, format: format)
let sequence = AVAudioSequencer(audioEngine: engine)
try sequence.load(from: source, options: [])
for track in sequence.tracks { track.destinationAudioUnit = instrument }
let duration = (sequence.tracks.map { $0.lengthInSeconds }.max() ?? 0) + 3
if duration <= 3 || duration > 1800 { fatalError("Unexpected MIDI duration: \(duration)") }
try engine.enableManualRenderingMode(.offline, format: format, maximumFrameCount: 2048)
try engine.start()
sequence.prepareToPlay()
try sequence.start()
let settings: [String:Any] = [AVFormatIDKey: kAudioFormatLinearPCM, AVSampleRateKey: 44100,
    AVNumberOfChannelsKey: 2, AVLinearPCMBitDepthKey: 16, AVLinearPCMIsFloatKey: false, AVLinearPCMIsBigEndianKey: false]
let output = try AVAudioFile(forWriting: destination, settings: settings)
let buffer = AVAudioPCMBuffer(pcmFormat: format, frameCapacity: 2048)!
let end = AVAudioFramePosition(duration * 44100)
var failures = 0
while engine.manualRenderingSampleTime < end {
    let frames = AVAudioFrameCount(min(2048, end - engine.manualRenderingSampleTime))
    let status = try engine.renderOffline(frames, to: buffer)
    switch status {
    case .success: try output.write(from: buffer); failures = 0
    case .error: fatalError("Offline audio render failed")
    default: failures += 1
    }
    if failures > 100 { fatalError("Offline audio render stalled") }
}
sequence.stop()
engine.stop()
print("\(source.lastPathComponent): \(String(format: "%.2f", duration)) seconds")
