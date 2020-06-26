#tool "nuget:?package=GitVersion.CommandLine&version=5.3.6"
#tool "nuget:?package=MSBuild.SonarQube.Runner.Tool&version=4.8.0"

#addin "nuget:?package=Cake.Coverlet&version=2.4.2"
#addin "nuget:?package=Cake.Sonar&version=1.1.25"

var target = Argument("target", "Default");
var sonarLogin = Argument("sonarLogin", EnvironmentVariable("SONAR_LOGIN") ?? "");
var nugetApiKey = Argument("nugetApiKey", EnvironmentVariable("NUGET_API_KEY") ?? "");

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
var codeCoverageResultFile = "CodeCoverageResults.xml";
var codeCoverageResultPath = System.IO.Path.Combine(System.IO.Path.GetFullPath(outputDir), codeCoverageResultFile);
var testResultsPath = System.IO.Path.Combine(System.IO.Path.GetFullPath(outputDir), "TestResults.xml");
var nugetPublishFeed = "https://api.nuget.org/v3/index.json";

var isLocalBuild = BuildSystem.IsLocalBuild;
var isMasterBranch = StringComparer.OrdinalIgnoreCase.Equals("refs/heads/master", BuildSystem.GitHubActions.Environment.Workflow.Ref);
var isPullRequest = BuildSystem.GitHubActions.Environment.PullRequest.IsPullRequest;
var runSonar = !string.IsNullOrWhiteSpace(sonarLogin);

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Setup(context =>
{
	Information($"Local build: {isLocalBuild}");
	Information($"Master branch: {isMasterBranch}");
	Information($"Pull request: {isPullRequest}");	
	Information($"Run sonar: {runSonar}");
	Information($"ref: {BuildSystem.GitHubActions.Environment.Workflow.Ref}");
});

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
		
		var settings = new DotNetCoreBuildSettings {        
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
			Logger = "trx;logfilename=" + testResultsPath
		};
		
		var coveletSettings = new CoverletSettings {
			CollectCoverage = true,
			CoverletOutputFormat = CoverletOutputFormat.opencover,
			CoverletOutputDirectory = outputDir,
			CoverletOutputName = codeCoverageResultFile,
		};
				
		DotNetCoreTest("./tests/CHG.Extensions.Security.Txt.Tests", settings, coveletSettings);		
	});
	
Task("SonarBegin")
	.WithCriteria(runSonar)
	.Does(() => {
		SonarBegin(new SonarBeginSettings {
			Key = sonarProjectKey,
			Url = sonarUrl,
			Organization = sonarOrganization,
			Login = sonarLogin,
			UseCoreClr = true,
			VsTestReportsPath = testResultsPath,
			OpenCoverReportsPath = codeCoverageResultPath
		});
	});

Task("SonarEnd")
	.WithCriteria(runSonar)
	.Does(() => {
		SonarEnd(new SonarEndSettings {
			Login = sonarLogin
		});
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
	
Task("Publish")
	.WithCriteria(!isPullRequest && isMasterBranch)
	.IsDependentOn("Pack")	
	.Description("Pushes the created NuGet packages to nuget.org")  
	.Does(() => {
	
		// Get the paths to the packages.
		var packages = GetFiles(outputDirNuget + "*.nupkg");

		// Push the package.
		NuGetPush(packages, new NuGetPushSettings {
			Source = nugetPublishFeed,
			ApiKey = nugetApiKey
		});	

		// Upload symbols
		var packages = GetFiles(outputDirNuget + "*.snupkg");

		// Push the package.
		NuGetPush(packages, new NuGetPushSettings {
			Source = nugetPublishFeed,
			ApiKey = nugetApiKey
		});	
	});
	
Task("Default")
	.IsDependentOn("SonarBegin")
	.IsDependentOn("Test")	
	.IsDependentOn("SonarEnd")
	.IsDependentOn("Pack")
	.IsDependentOn("Publish");	

RunTarget(target);
