# Gravity Toy Notes
* _Ordered from most recent to oldest notes_
* _These notes were created before I started using GitHub Issues to manage this project_

## Previous issues with the simulation (Closed)
* For the UWP app, there were issues with crashes, unexpected rendering behaviour, unconstrained process memory growth, and the UI freezing.
* Made three fixes that seem to have addressed these issues:
  * The entire simulation was running on the UI thread--changed it to run the calculations on a worker thread which marshalls the UI updates onto the UI thread
  * The per-frame computations and rendering which are started by a recurring timer now check to see if the previous frame has finished before starting a new tick of the simulation. It does this by skipping frames (as many as necessary) to allow the one in progress to finish. This has coarse granularity, but also allows the UI rendering to catch up before being updated again.
  * The third fix is below the next bullet.

* The UI is now continuously responsive, but this has created a new bug where sometimes when scenarios are cleared there is still a worker thread trying to work on the simulation that has just been cleared. This has resulted in an index out of range exception trying to access bodies in the simulation that have been deallocated.
  * Fixed by introducing a one frame wait when scenarios are changed

### Suspend/resume-related issues (Closed)
* There was an issue with even as few as 9 body simulations getting into a state where the number of threads rapidly rises, it may have been related to running in debug in Visual Studio or may be related to the problems described above where the UI would get behind on updates and then fall further and further behind while also exploding the nunber of threads. This crashed VS after a 15 minute run with 9 bodies. This may all be fixed by the changes described above, but still needs to be checked some more.
* This may be related to crashes that seem to happen when resuming after the OS tiomeout has turned off the screen (and idled the GPU?)

## Issue with Flatbody struct (Closed)
* In the GS10 version I changed the Flatbody class to a struct, which broke things because of the change from a reference type to a value type. Not sure why I tried this in the first place.
* I restored the Flatbody class and got things working again by going back to an earlier version (in \Original Code\Flatbody-class.cs).

## Application Types of the four old versions (~2013 - ~2016)
* None of the old .csproj's can be opened in Visual Studio 2017 or 2019
* Not sure if I ever got all of these running (defintely not on Windows Phone). Original ran fine on Win8 but eventually had issues with animation/memory (~2015 ??).
* All four old versions were for various versions of UWP
  * All have "OutputType = AppContainerExe" in their .csproj's XML
* GravitySandbox: Original UWP version targeting Windows 8
* GravitySandbox81: Windows 8.1 UWP
* GravitySandbox-Mercury (Combined Win8.1/Win Phone 8.1)
  * "Apps for Windows Phone 8.1 can now be created using the same application model as Windows Store apps for Windows 8.1, based on the Windows Runtime, and the file extension for WP apps is now ".appx" (which is used for Windows Store apps), instead of Windows Phone's traditional ".xap" file format."
* GravitySandbox10: UWP for Windows 10 - targeting 10.0.17763.0
