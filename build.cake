#tool "dotnet:?package=GitVersion.Tool&version=5.8.1"
#tool "nuget:?package=NuGet.CommandLine&version=5.11.0"
#tool "nuget:?package=dotnet-sonarscanner&version=5.4.0"

#addin "nuget:?package=Cake.Sonar&version=1.1.29"

var target = Argument("target", "Default");
var sonarLogin = Argument("sonarLogin", EnvironmentVariable("SONAR_LOGIN") ?? "");
var nugetApiKey = Argument("nugetApiKey", EnvironmentVariable("NUGET_API_KEY") ?? "");

//////////////////////////////////////////////////////////////////////
//    Build Variables
/////////////////////////////////////////////////////////////////////
var solution = "./CHG.Extensions.Security.sln";
var project = "./src/CHG.Extensions.Security.Txt.csproj";
var outputDir = new DirectoryPath("./buildArtifacts/").MakeAbsolute(Context.Environment);
var outputDirNuget = outputDir.Combine("NuGet");
var sonarProjectKey = "CHG-MERIDIAN_CHG.Extensions.Security.Txt";
var sonarUrl = "https://sonarcloud.io";
var sonarOrganization = "chg-meridian";

var outputDirTemp = outputDir.Combine("Temp");
var outputDirTests = outputDirTemp.Combine("Tests");

var codeCoverageResultFilePath = MakeAbsolute(outputDirTests).Combine("**/").CombineWithFilePath("coverage.opencover.xml");
var testResultsPath = MakeAbsolute(outputDirTests).CombineWithFilePath("*.trx");

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
	  
	EnsureDirectoryDoesNotExist(outputDir, new DeleteDirectorySettings {
		Recursive = true,
		Force = true
	});
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
		
		var msBuildSettings = new DotNetMSBuildSettings()
		{
			Version =  versionInfo.AssemblySemVer,
			InformationalVersion = versionInfo.InformationalVersion,
			PackageVersion = versionInfo.NuGetVersionV2
		}.WithProperty("PackageOutputPath", outputDirNuget.FullPath);	

		var settings = new DotNetBuildSettings {
			Configuration = "Release",					
			MSBuildSettings = msBuildSettings
		};	

		DotNetBuild(solution, settings);			
	});

Task("Test")
	.IsDependentOn("Build")
	.Does(() =>
	{

		var settings = new DotNetTestSettings {
			Configuration = "Release",
			Loggers = new[]{"trx;"},
			ResultsDirectory = outputDirTests,
			Collectors = new[] {"XPlat Code Coverage"},	
			ArgumentCustomization = a => a.Append("-- DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Format=opencover"),
			NoBuild = true
		};		
				
		DotNetTest(solution, settings);		
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
			VsTestReportsPath = testResultsPath.ToString(),
			OpenCoverReportsPath = codeCoverageResultFilePath.ToString()
		});
	});

Task("SonarEnd")
	.WithCriteria(runSonar)
	.Does(() => {
		SonarEnd(new SonarEndSettings {
			Login = sonarLogin
		});
	});
	
Task("Publish")
	.WithCriteria(!isPullRequest && isMasterBranch)
	.IsDependentOn("Test")	
	.IsDependentOn("Version")
	.Description("Pushes the created NuGet packages to nuget.org")  
	.Does(() => {
	
		// Get the paths to the packages.
		var packages = GetFiles(outputDirNuget + "*.nupkg");

		// Push the package and symbols
		NuGetPush(packages, new NuGetPushSettings {
			Source = nugetPublishFeed,
			ApiKey = nugetApiKey,
			SkipDuplicate = true
		});	
	});
	
Task("Default")
	.IsDependentOn("SonarBegin")
	.IsDependentOn("Test")	
	.IsDependentOn("SonarEnd")
	.IsDependentOn("Publish");	

RunTarget(target);
