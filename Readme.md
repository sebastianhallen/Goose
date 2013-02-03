Goose
=====

Goose is a visual studio extension that automatically runs a powershell script any time a file matching a specified glob is saved.

##Usage
Install the plugin and place a goose.config file in the project folder. The goose.config does not have to be included in the project.
Each project should have it's own goose.config. 

Note that the extension will run anything you put in compile-command with the same priviliges as visual studio. You have been warned.

##Configuration
The config file has the following structure:
```xml
<?xml version="1.0" encoding="utf-8" ?>
<goose version="1">
  <action on="save" glob="*.less">
    <!-- relative path from the project directory to the working directory of the command -->
    <working-directory>Build</working-directory>
    <command>$now = Get-Date ; Add-Content build.log "Last build: $now"</command> 
  </action>
</goose>
```

**action.on**: when the action should be triggered. on="save" is currently the only supported action.

**action.glob**: file pattern to monitor for changes.

**working-directory**: specifies the build directory relative to the project root folder. The on save command will be run in this directory.

**command**: the command that will be run whenever a less file is saved.


###Old configuration format
Don't use it. No new features will be supported for this format. The plugin was initially hard coded to only monitor changes to *.less files.
```xml
<?xml version="1.0" encoding="utf-8" ?>
<compile-less>
  <build-directory>Build</build-directory>
  <compile-command>$now = Get-Date ; Add-Content build.log "Last build: $now"</compile-command> 
</compile-less>
```

**build-directory**: specifies the build directory relative to the project root folder. The on save command will be run in this directory.
**compile-command**: the command that will be run whenever a less file is saved.

