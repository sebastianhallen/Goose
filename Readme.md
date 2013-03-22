Goose
=====

Goose is a visual studio extension that automatically runs a powershell script any time a file matching a specified glob is saved. If your script outputs data in a way <a href="https://github.com/sebastianhallen/Goose/wiki/Script-output">goose can understand</a> you will be able to write to Visual Studios error list or the regular message output window. In case Goose does not understand the output, you will always get the script output in the message window.

##Usage
<a href="http://sebastianhallen.github.com/Goose/">Download</a> and install the plugin, place a goose.config file in the project folder. The goose.config does not have to be included in the project.
Each project should have it's own goose.config. 

Note that the extension will run anything you put in compile-command with the same priviliges as visual studio. You have been warned.

##Why?
Goose was born when we needed to trigger an external build script for less files each time a .less file was saved. It later evolved to be able to trigger a jslint-script so we could get compiliation failed style errors whenever a javascript was not up to par.
Theese are just some things you could use it for. 
Goose uses Goose itself to bump the version number - just save goosebump.txt to ++ the version number. 

##Configuration
You can configure multiple actions, just add another action block in the config. If multiple actions are triggered at the same time Goose will queue them and run them one by one.

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

