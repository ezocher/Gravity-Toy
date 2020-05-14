# Gravity Sandbox Notes

## Numerical Instability
* The scenarios with perfectly symetrical starting positions eventually break out of symetrical motion due to numerical instability.
* It would probably require several fundamental changes to the foundations of the simulation to fix this. Some possibilities: higher precision math, much finer simulation steps, calculus to calculate 2-body acceleration during a time interval (instead of current linear approximation), re-thinking and re-engineering of acceleration limits when objects pass through one another.

## Previous issues with the simulation
* For the UWP app, there was an issue with unconstrained process memory growth during long runs that started to happen/worsen (??) with Windows 8.1 (??). That issue seems to be totally resolved in current Windows 10 (v. 1909, build 18363.836).

## Issue with Flatbody struct
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
