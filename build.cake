#tool "nuget:?package=GitVersion.CommandLine&version=4.0.0"
#tool "nuget:?package=MSBuild.SonarQube.Runner.Tool&version=4.3.1"

#addin "nuget:?package=Cake.Coverlet&version=2.1.2"
#addin "nuget:?package=Cake.Sonar&version=1.1.18"

var target = Argument("target", "Default");

//////////////////////////////////////////////////////////////////////
//    Build Variables
/////////////////////////////////////////////////////////////////////
var solution = "./CHG.Extensions.Security.sln";
var project = "./src/CHG.Extensions.Security.Txt.csproj";
var outputDir = "./buildArtifacts/";
var outputDirNuget = outputDir+"NuGet/";
var sonarProjectKey = "CHG-MERIDIAN_CHG.Extensions.Security.Txt";
var sonarUrl = "https://sonarcloud.io";
var sonarOrganization = "chg-meridian";
var sonarLogin = Argument("sonarLogin", "");
var codeCoverageResultFile = "CodeCoverageResults.xml";
var codeCoverageResultPath = System.IO.Path.Combine(System.IO.Path.GetFullPath(outputDir), codeCoverageResultFile)
var testResultsPath = System.IO.Path.Combine(System.IO.Path.GetFullPath(outputDir), "TestResults.xml");

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
        var settings = new DotNetCoreTestSettings {
			Configuration = configuration,
			Logger = "trx;logfilename=" + testResultsPath
		};
		
		var coveletSettings = new CoverletSettings
        {
            CollectCoverage = true,
            CoverletOutputFormat = CoverletOutputFormat.opencover,
            CoverletOutputDirectory = outputDir,
            CoverletOutputName = codeCoverageResultFile,
        };
				
        DotNetCoreTest("./tests/CHG.Extensions.Security.Txt.Tests", settings, coveletSettings);
        
    });
	
Task("SonarBegin")
  .Does(() => {
     SonarBegin(new SonarBeginSettings{
        Key = sonarProjectKey,
        Url = sonarUrl,
        Organization = sonarOrganization,
        Login = sonarLogin,
		UseCoreClr = true,
		VsTestReportsPath = testResultsPath,
		OpenCoverReportsPath = codeCoverageResultPath,
        Verbose = true
     });
  });

Task("SonarEnd")
  .Does(() => {
     SonarEnd(new SonarEndSettings{
        Login = sonarLogin
     });
  });

Task("Pack")
	.IsDependentOn("SonarBegin")
    .IsDependentOn("Test")
	.IsDependentOn("Version")
	.IsDependentOn("SonarEnd")
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
	.IsDependentOn("SonarBegin")
    .IsDependentOn("Test")
	.IsDependentOn("SonarEnd");

RunTarget(target);