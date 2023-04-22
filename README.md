# Learn to code with Microsoft HoloLens

This repo is dedicated to my third year project (dissertation) completed as part of BSc Computer Science degree at Lancaster University (2020-2023).

My C# is self-taught, and I have learnt a great deal over the course of this project. The most elegant code is notably:
> <b>Objects/ObjectInstanceManager.cs</b> - for creating "assembly-independent" objects.
> <b>Compilation_Saving/FileManagement.cs</b> - for compilation.


# Dissertation Abstract
Microsoft HoloLens 2 is an Augmented Reality (AR) headset released by Microsoft in 2019. Since its release, there has been adoption in professional and educational settings, yet it has had limited impact in the context of Computer Science (CS) education. This project aims to prototype an AR code editor for HoloLens, capable of compiling and executing C# code, to be used in CS education. The core concept of this application is that instead of creating object instances in computer memory, object instances will exist as virtual 3D objects. Instead of being passed by reference, object instances will quite literally be passed by hand. The final application shows promise and could be valuable in CS education, but many challenges were faced during development. Most notably, not being able to dynamically compile on HoloLens.

The app is streamed to HoloLens from the Unity Editor using Holographic remoting. Video demos available at:
https://www.youtube.com/@SamuelNugent/videos
