Goose
=====

Goose is a visual studio extension that automatically runs a powershell script any time a .less file is saved. The command to run is defined in goose.config in the project root folder. You will need a separate goose.config for each project. 


##Configuration
The config file has the following structure:
```xml
<?xml version="1.0" encoding="utf-8" ?>
<action on="save">
  <!-- relative path from the project directory to the working directory of the command -->
  <working-directory>Build</working-directory>
  <command>$now = Get-Date ; Add-Content build.log "Last build: $now"</command> 
</action>
```
**working-directory**: specifies the build directory relative to the project root folder. The on save command will be run in this directory.
**command**: the command that will be run whenever a less file is saved.


###Old configuration format
Don't use it. No new features will be supported for this format.
```xml
<?xml version="1.0" encoding="utf-8" ?>
<compile-less>
  <build-directory>Build</build-directory>
  <compile-command>$now = Get-Date ; Add-Content build.log "Last build: $now"</compile-command> 
</compile-less>
```

**build-directory**: specifies the build directory relative to the project root folder. The on save command will be run in this directory.
**compile-command**: the command that will be run whenever a less file is saved.

Note that the extension will run anything you put in compile-command with the same priviliges as visual studio. You have been warned.




#####KTHXBAI
We hope this helps you shave of a few seconds in your daily work. 
Let us know of any issues/bugs/feature requests.
