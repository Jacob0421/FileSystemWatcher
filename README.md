# FilesystemWatcher

## Introduction:
This project is meant to be an expandable version of the base File System watcher class that can be configured to have active windows set both by Day of the week, and by specific time. This system when running will load the config file and watch a specified directory, and on file creation will wait for that file write to be complete, then move the file to a specified output directory.The project has also allowed to me to further refine JIRA skills as progress is currently tracked through a Kanban board.

### Libraries Used:
- NLog (Logging)
- TopShelf (System Service implementation)

### Guides Followed:
- [Intro to Windows Services in C# - How to create, install, and use a service using Topshelf By IAmTimCorey](https://www.youtube.com/watch?v=y64L-3HKuP0&t=1491s)

### Future Implementations
- Pre-Process any currently existing files in the directory
- Validation for BatchJob configuration XML
- Rolling LogFiles (> 10MB)
- LogFile Archiving and zipping

## How To Use
### Configuration
#### FileSystemWatcher
##### Batch Jobs
#### NLog
  There are two NLog outputs console output and logfile. foreach output there is a default layout and a layout specific for errors. In the below image, the top two targets are examples of ouput targets using the default layout. the bottom two are examples of the outputs with error layouts.
  
  <img width="679" alt="image" src="https://github.com/Jacob0421/FileSystemWatcher/assets/48600024/498fd963-f630-44b7-8789-4ae7eb7f0e7a">

  ##### NLog Variables
  NLog allows you to create variables and two of these that I use are specific for Layouts, but there are many baked into the logging. See NLog Configuration file Docs [here](https://github.com/nlog/nlog/wiki/Configuration-file)

  **Variables Used:**
  - ${Level} :
  - ${longdate} : 
  - ${newline} :
  - ${message} :
  - ${shortdate} :
  - ${FileName} :
  - ${DefaultLayout} :
  - ${ErrorLayout} :
