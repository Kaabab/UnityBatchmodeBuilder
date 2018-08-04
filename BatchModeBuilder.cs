using UnityEngine;
using UnityEditor;
using System.Linq;
using System;
using System.Collections.Generic;
using UnityEditor.Callbacks;

/**
 *  Editor script to automate Unity build for Continuous Integration
 *  
 *  Do not include -quit, batchmodebuilder will exit the application after the build process is complete
 *  Usage : %unity_path% -projectPath "%project_path%" -batchmode -batchmodebuilder -logfile /dev/stdout -buildtarget %build_target% -buildpath "%build_path%" %isDev% %isDebug% | tee %unity_log%
 *
 **/
[InitializeOnLoad]
public static class BatchmodeBuilder
{
    // Unitys command line arguments
    public const string FLAG_BATCH_MODE = "-batchmode";
    public const string FLAG_BATCH_MODE_BUILDER = "-batchmodebuilder";
    // extensions
    public const string FLAG_DEV_BUILD = "-development"; // EditorUserBuildSettings.development when passed in set to true
    public const string FLAG_DEBUG_BUILD = "-debug"; //  EditorUserBuildSettings.allowDebugging when passed in set to true
    public const string ARG_BUILD_TARGET = "-buildtarget"; // same as BuildTarget enum (case sensitive)
    public const string ARG_BUILD_OPTIONS = "-buildopts"; // same as BuildOptions enum (case sensitive)
    public const string ARG_BUILD_PATH = "-buildpath"; // relative to project

    private static List<String> args = null;

    private class BuildConfiguration
    {
        public readonly BuildOptions buildOptions;
        public readonly BuildTarget buildTarget;
        public readonly bool development;
        public readonly bool allowDebugging;
        public readonly string buildPath;
        public readonly string[] scenes;

        public BuildConfiguration(BuildTarget buildTarget, string buildPath, BuildOptions buildOptions, bool development, bool allowDebugging)
        {
            this.buildTarget = buildTarget;
            this.buildPath = buildPath;
            this.buildOptions = buildOptions;
            this.development = development;
            this.allowDebugging = allowDebugging;
            this.scenes = EditorBuildSettings.scenes.Select(l => l.path).ToArray();
        }
    }

    static BatchmodeBuilder()
    {
        EditorApplication.update += Init; /* This serves as callback to init after Unity has fully loaded */
    }

    private static void Init()
    {
        EditorApplication.update -= Init;
        Build();
    }

    private static void Build()
    {
        if (!InitArgs() || BuildPipeline.isBuildingPlayer)
        {
            return;
        }
        Debug.LogError("BatchmodeBuilder : Batchmode detected, builder trying to parse command line");
        BuildConfiguration configuration = ParseBuildConfiguration();
        if (configuration == null)
        {
            Debug.LogError("BatchmodeBuilder : failed to parse the command line, check your logs and try again");
            EditorApplication.Exit(-1);
            return;
        }
        Build(configuration);
    }

    private static bool InitArgs()
    {
        args = System.Environment.GetCommandLineArgs().ToList();
        if (!GetFlag(FLAG_BATCH_MODE, false) || !GetFlag(FLAG_BATCH_MODE_BUILDER, false))
        {
            args = null;
            return false;
        }
        return true;
    }

    private static void Build(BuildConfiguration configuration)
    {
        Debug.Log("BatchmodeBuilder : Starting build");
        Debug.Log(string.Format("BatchmodeBuilder Configuration : \n target path : {0}\n build target : {1}\n build options : {2}\n isDebug : {3}\n isDev : {4}\n",
            configuration.buildPath,
            configuration.buildTarget.ToString(),
            configuration.buildOptions.ToString(),
            configuration.allowDebugging,
            configuration.development));
        EditorUserBuildSettings.development = configuration.development;
        EditorUserBuildSettings.allowDebugging = configuration.allowDebugging;
        BuildPipeline.BuildPlayer(configuration.scenes, configuration.buildPath, configuration.buildTarget, configuration.buildOptions);
        EditorApplication.Exit(0);
    }

    private static BuildConfiguration ParseBuildConfiguration()
    {
        String buildTargetArg = GetArg(ARG_BUILD_TARGET, null);
        if (buildTargetArg == null)
        {
            Debug.LogError(string.Format("BatchmodeBuilder : No {0} argument set BatchmodeBuilder aborting", ARG_BUILD_TARGET));
            return null;
        }
        if (!Enum.GetNames(typeof(BuildTarget)).Any(n => n.Equals(buildTargetArg)))
        {
            Debug.LogError(string.Format("BatchmodeBuilder : {0} argument unrecognized value {1}; BatchmodeBuilder aborting", ARG_BUILD_TARGET, buildTargetArg));
            return null;
        }
        String buildPath = GetArg(ARG_BUILD_PATH, null);
        if (buildPath == null)
        {
            Debug.LogError(string.Format("BatchmodeBuilder : No {0} argument set BatchmodeBuilder aborting", ARG_BUILD_TARGET));
            return null;
        }
        BuildTarget buildTarget = (BuildTarget)Enum.Parse(typeof(BuildTarget), buildTargetArg);
        bool isDev = GetFlag(FLAG_DEV_BUILD, false);
        bool isDebug = GetFlag(FLAG_DEBUG_BUILD, false);
        String buildOptionsArgs = GetArg(ARG_BUILD_OPTIONS, null);
        BuildOptions buildOptions = BuildOptions.None;
        if (buildOptionsArgs != null)
        {
            buildOptions = (BuildOptions)Enum.Parse(typeof(BuildOptions), buildOptionsArgs);
        }
        if (isDev)
        {
            buildOptions |= BuildOptions.Development;
        }
        if (isDebug)
        {
            buildOptions |= BuildOptions.AllowDebugging;
        }
        return new BuildConfiguration(buildTarget, buildPath, buildOptions, isDev, isDebug);
    }

    private static bool GetFlag(string name, bool defaultValue)
    {
        int index = args.IndexOf(name);
        if (index >= 0)
        {
            return true;
        }
        return defaultValue;
    }

    private static string GetArg(string name, string defaultValue)
    {
        int index = args.IndexOf(name);
        if (index >= 0 && index < args.Count - 1)
        {
            return args[index + 1];
        }
        return defaultValue;
    }
}
