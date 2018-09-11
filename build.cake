#tool "nuget:?package=GitVersion.CommandLine"

var target = Argument("target", "Default");

//////////////////////////////////////////////////////////////////////
//    Build Variables
/////////////////////////////////////////////////////////////////////
var solution = "./CHG.Extensions.Security.sln";
var project = "./src/CHG.Extensions.Security.Txt.csproj";
var outputDir = "./buildArtifacts/";
var outputDirNuget = outputDir+"NuGet/";

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task("Clean")
    .Description("Removes the output directory")
	.Does(() => {
	  
	if (DirectoryExists(outputDir))
	{
		DeleteDirectory(outputDir, new DeleteDirectorySettings {
			Recursive = true,
			Force = true
		});
	}
	CreateDirectory(outputDir);
});

GitVersion versionInfo = null;
Task("Version")
  .Description("Retrieves the current version from the git repository")
  .Does(() => {
		
	versionInfo = GitVersion(new GitVersionSettings {
		UpdateAssemblyInfo = false
	});
		
	Information("Version: "+ versionInfo.FullSemVer);
  });

Task("Build")
	.IsDependentOn("Clean")
	.IsDependentOn("Version")
    .Does(() => {
 		
	var settings = new DotNetCoreBuildSettings
     {        
         Configuration = "Release",
        
		 ArgumentCustomization = args => args.Append("/p:SemVer=" + versionInfo.NuGetVersionV2 + " /p:SourceLinkCreate=true")
     };

     DotNetCoreBuild(project, settings);
		
		
    });

Task("Test")
    .IsDependentOn("Build")
    .Does(() =>
    {
        var projectFiles = GetFiles("./tests/**/*.csproj");
        foreach(var file in projectFiles)
        {
            DotNetCoreTest(file.FullPath);
        }
    });

Task("Pack")
    .IsDependentOn("Test")
	.IsDependentOn("Version")
    .Does(() => {
        
		var packSettings = new DotNetCorePackSettings
		 {			
			 Configuration = "Release",
			 OutputDirectory = outputDirNuget,
			 ArgumentCustomization = args => args.Append("/p:PackageVersion=" + versionInfo.NuGetVersionV2+ " /p:SourceLinkCreate=true")
		 };
		 
		 DotNetCorePack(project, packSettings);			
    });

Task("Default")
    .IsDependentOn("Test");

RunTarget(target);