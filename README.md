# Gravity-Toy
Very old XAML/UWP app. Porting to various platforms to test and learn.


![Screenshot of UWP app](Images/UWP-Screenshot.png)

## Completed and potential ports
- [x] UWP on Windows 10
- [ ] Unity (change to spheres in 3D)
- [ ] Win2D on UWP
- [ ] WinUI 3 on .NET 5
- [ ] Xamarin/MAUI on Android Phone or iPad

## Numeric Issues
From the beginning of this project there have been many different strange and unexpected dynamic behaviors in the simulation. These are caused by issues with the use of floating point values for all of the quantities being modeled.

I created a branch (numeric-investigations) to invasively investigate the root causes of these issues and to implement and test fixes.

There are four major issues that I've uncovered so far. These issues and their fixes are noted in
 [NumericIssues.md](https://github.com/ezocher/Gravity-Toy/blob/master/Design%20and%20Notes/NumericIssues.md).

## General TBDs
***Active bugs, new features, and investigations are all now in Issues and in the Project board***
- [x] Get the simulation running again
- [x] Move bugs, investigations and new work into issues
- [x] Create project board for managing work and backlog


