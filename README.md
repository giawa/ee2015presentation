# ee2015presentation
This is the code for a presentation that I am giving at my local University.  This presentation is meant to be an interactive demonstration of some audio, digital and DSP course work.

This project emulates many types of PowerPoint slides, and could be re-themed very easily.  Check the presentation/Slides/Common.cs code for all of the common elements used by the theme.

## License
Check the included [LICENSE.md](https://github.com/giawa/ee2015presentation/blob/master/LICENSE.md) file for the license associated with this code.  This license covers my work (the code, text within the code and Visual Studio solution) only.  See 'use of copyrighted works' for more information.

## Use of Copyrighted Works
The presentation is set up to emulate a PowerPoint presentation, and uses some elements from a PowerPoint theme.  I own a full copy of PowerPoint, so I believe I am acting within my rights of using the PowerPoint theme in my own software as well (especially since this software is not being sold, etc).  Some of the pictures in the presentation have been obtained via Google, and their respective rights holders maintain their copyright over those images.  Since this is open source and educational, I believe that I am acting within fair use laws in my use of the images.

If you are the copyright holder of an image, theme or other content of this presentation and do not approve of the use of your content, please submit an issue to this Git repository to have your content removed.  Please provide proof that you are the copyright holder so that the process can be expeditious.

## Building the Project
This project includes a .sln and .csproj file.  The /media folder must be moved into the executable directory before launching.  The contents of /presentation/libs must also be copied to the output directory (specifically the SDL and FFTW compiled libraries).

## Use of Open Source Software
This project uses a few pieces of open source software, and the project would have been difficult (or impossible) to complete without them!  Here's a list of the open-source software that I used:
* FFTW - http://www.fftw.org/
* SDL 2 - https://www.libsdl.org/
* opengl4csharp - https://github.com/giawa/opengl4csharp