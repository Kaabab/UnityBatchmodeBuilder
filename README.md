# UnityBatchmodeBuilder

Automatically extends Unity's command line arguments to support build related options, allowing for an easier CI tooling integration. 

# Installation 

Add the script to an "Editor" folder before invoking Unity with ``-batchmode``, the script is active only in batch mode.

# Arguments 
#### Required
* ``-buildtarget iOs`` : Set the target platform  
* ``-buildpath   "Build"`` : Set the resulting build path (relative to the project)   

#### Optional 
* ``-buildopts   "Development, AllowDebugging, EnableHeadlessMode, BuildScriptsOnly"`` : Set the build options
* ``-development`` : Sets EditorUserBuildSettings.development to true and adds the build option Development
* ``-debug``       : Sets EditorUserBuildSettings.allowDebugging to true and adds the build option AllowDebugging
