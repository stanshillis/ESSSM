properties {
    $base_directory = Resolve-Path .. 
	$publish_directory = "$base_directory\publish-net35"
	$build_directory = "$base_directory\build"
	$src_directory = "$base_directory\src"
	$output_directory = "$base_directory\output"
	
	$sln_file = "$src_directory\ESSSM.sln"
	$target_config = "Release"
	$framework_version = "v3.5"
	$version = "2.0.0.0"

	$nuget_dir = "$base_directory\tools\.nuget"
}

task default -depends Build

task Build -depends Clean, UpdateVersion, Compile

task Clean {
	Clean-Item $publish_directory -ea SilentlyContinue
    Clean-Item $output_directory -ea SilentlyContinue
}

task UpdateVersion {
	$vSplit = $version.Split('.')
	
	if($vSplit.Length -ne 4)
	{
		throw "Version number is invalid. Must be in the form of 0.0.0.0"
	}

	$major = $vSplit[0]
	$minor = $vSplit[1]

	$assemblyFileVersion = $version
	$assemblyVersion = "$major.$minor.0.0"

	$versionAssemblyInfoFile = "$src_directory/VersionAssemblyInfo.cs"
	"using System.Reflection;" > $versionAssemblyInfoFile
	"" >> $versionAssemblyInfoFile
	"[assembly: AssemblyVersion(""$assemblyVersion"")]" >> $versionAssemblyInfoFile
	"[assembly: AssemblyFileVersion(""$assemblyFileVersion"")]" >> $versionAssemblyInfoFile
}

task Compile {
	exec { msbuild /nologo /verbosity:quiet $sln_file /p:Platform="Any CPU" /p:Configuration=$target_config /t:Clean }

	"exec { msbuild /nologo /verbosity:quiet $sln_file /p:Platform=""Any CPU"" /p:Configuration=$target_config /p:TargetFrameworkVersion=$framework_version }"
	exec { msbuild /nologo /verbosity:quiet $sln_file  /p:Platform="Any CPU" /p:Configuration=$target_config /p:TargetFrameworkVersion=$framework_version }
}

task Package -depends Build, PackageESSSM, PackageDocs {
	move $output_directory $publish_directory
}

task PackageDocs {
	copy "$base_directory\README.md" "$output_directory\README.txt"
}

task PackageESSSM {
	mkdir "$output_directory\bin" | out-null
	copy "$src_directory\ESSSM\bin\Release\*.dll" "$output_directory\bin"
}

task NuGetPack -depends Package {
	gci -r -i "*.nuspec" "$build_directory" |% { .$nuget_dir\nuget.exe pack $_ -basepath $base_directory -o $publish_directory -version $version }
}
