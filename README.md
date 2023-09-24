# Windows Audio Session (WASAPI) BASS.Net sample

A sample of usage of Windows Audio Session WASAPI using BASS.NET - WPF C# .NET Framework 4.8

- colored bars (vu-meters) showing captured device sound FFT , Sound Level and sound wave

<p align="center">
<img src="https://github.com/KRtekTM/WindowsAudioSessionSample/blob/main/Doc/softronics.gif?raw=true" align="center" alt="a FFT having 512 bars + FFT with 16 bars and peak bars + stereo sound level">
<br>
<i>a FFT having 512 bars + FFT with 16 bars and peak bars + stereo sound level + sound wave</i>
</p>

Run the project, select your audio device, and this tool will listen to the device internal output and animates some of the sound properties. The library BASS.NET is used to interface C# with the WASAPI Windows library.

Clean architecture approach: complete discoupling between data,data providers,data transformers,ui controls,drawers,view models,application logic. 
Relying on the patterns MVVM, Command, Chain of responsability, Dependency injection, Mediator.

Implements several importants aspects of WPF programming:
- User Controls, Resources/Styles, Converters, View Models, Data annotations, Data validators, Data binding, Commands, Visual Drawings

## Components dependencies:

<a href="https://app.diagrams.net/?mode=github#Hfranck-gaspoz%2FWindowsAudioSessionSample%2Fmain%2FDoc%2Fcomponents1.drawio.svg">
<img src="/Doc/components1.drawio.svg">
</a>

