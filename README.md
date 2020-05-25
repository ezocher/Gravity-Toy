# Gravity-Sandbox
Very old XAML/UWP app. Porting to various platforms to test and learn.


![Screenshot of UWP app](Images/UWP-Screenshot.png)

## Completed and potential ports
- [x] UWP on Windows 10
- [ ] Unity (change to spheres in 3D)
- [ ] Win2D on UWP
- [ ] WinUI 3 on .NET 5
- [ ] (Low priority) Xamarin/MAUI on Android Phone or iPad

## General TBDs
***Active bugs, new features, and investigations are all now in issues and in the Project board***
- [x] Get the simulation running again
- [x] Move bugs, investigations and new work into issues
- [x] Create project board for managing work and backlog

### TBD: Create and close issues for these
- [x] Perf of gravity calculations (done), XAML modifications (some idea, but these run async)
- [x] Run gravity calculations on multiple threads? Not yet, for now we're automatically slowing the simulation when it gets too big to run at full frame rate
- [x] Crashing bug when scenarios are cleared and a worker thread is still working
- [x] Bug: Runaway explosion of UI threads when simulation doesn't complete in per frame time (Was: Figure out the animation/memory problem that started ~2016)
- [x] Bug: Suspend/resume crashes (??) when resuming after system turns off screen
- [x] Display loaded scenario name when first loaded
