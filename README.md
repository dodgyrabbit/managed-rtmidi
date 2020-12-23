# managed-rtmidi
managed-rtmidi is based on the (now abandonned) project [managed-midi](https://github.com/atsushieno/managed-midi). Credit to [Atsushi Eno](https://github.com/atsushieno) and all the work that was done on this.
This is a much more simplified version of that work, that specifically targets rt-midi as the underlying API.

## What is special about managed-rtmidi?
* It is simple to use in your Linux or Raspberry PI projects
* There is no nuget magic
* Works on 32 and 64 bit CPUs
* Supports virtual output port on rtmidi
* More comments and documentation in the code to understand what's going on

## Prerequisites
You need rtmidi on your system.
