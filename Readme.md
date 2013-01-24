Goose
=====

Goose is a visual studio extension that automatically runs a powershell script any time a .less file is saved. The command to run is defined in goose.config in the project root folder. You will need a separate goose.config for each project. 
The config file has the following structure:
<?xml version="1.0" encoding="utf-8" ?>
<compile-less>
  <build-directory>Build</build-directory>
  <compile-command>$now = Get-Date ; Add-Content build.log "Last build: $now"</compile-command> 
</compile-less>
  


build-directory: specifies the build directory relative to the project root folder. The on save command will be run in this directory.
compile-command: the command that will be run whenever a less file is saved.

Note that the extension will run anything you put in compile-command with the same priviliges as visual studio. You have been warned.



The extension could be extended to run any command whenever a file with a certain extension is saved. Shouldn't be too difficult so feel free to fix it and make a pull request if you do.


We hope it helps you shave of a few seconds in your daily work.
