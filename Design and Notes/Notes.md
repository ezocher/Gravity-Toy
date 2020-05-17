# Gravity Sandbox Notes
_Ordered from latest to oldest notes_

## Previous issues with the simulation (Fixed)
* For the UWP app, there was an issue with unconstrained process memory growth and the UI freezing.
* It looks like these issues were due to running the entire simulation on the UI thread.
* These issues are tentatively all resolved by fixing the long calculations to run in the worker threads and marshalling only the UI updates onto the UI thread.
* With this fix, the worker threads are automatically being run on multiple cores and the simulation will still run pretty well even with gravity calculations that require three "frames" of time to process. The animation is also much smoother then ever before.
* The UI is now continuously responsive, but this has created a new bug where sometimes when scenarios are cleared there is still a worker thread trying to work on the simulation that has just been cleared. This has resulted in an index out of range exception trying to access bodies in the simulation that have been deallocated.

### Unknown status
* There was an issue with even as few as 9 body simulations getting into a state where the number of threads rapidly rises, it may have been related to running in debug in Visual Studio. This crashed VS after a 15 minute run with 9 bodies. This may all be fixed by the change described above, but still needs to be checked.

## Numerical Instability
* The scenarios with perfectly symetrical starting positions eventually break out of symetrical motion due to numerical instability.
* It would probably require several fundamental changes to the foundations of the simulation to fix this. Some possibilities: higher precision math, much finer simulation steps, calculus to calculate 2-body acceleration during a time interval (instead of current linear approximation), re-thinking and re-engineering of acceleration limits when objects "pass through" one another.

## Issue with Flatbody struct [Resolved]
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
