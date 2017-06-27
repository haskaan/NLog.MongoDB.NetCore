$projectFile = "src\NLog.MongoDB.NetCore\NLog.MongoDB.NetCore.xproj"
$github = "ByNeo/NLog.MongoDB.NetCore"
$sonarQubeID = "NLog.MongoDB.NetCore"
$sonarQubeHost = "https://sonarcloud.io"

if($env:APPVEYOR_REPO_NAME -eq $github){

	echo $env:sonar_token

    if(-not $env:sonar_token){
        Write-warning "not running SonarQube, no sonar_token"
        return;
    }

    choco install "msbuild-sonarqube-runner" -y
     
    if ($env:APPVEYOR_PULL_REQUEST_NUMBER) { 
        MSBuild.SonarQube.Runner.exe begin /k:"$sonarQubeID" /d:"sonar.host.url=$sonarQubeHost" /d:"sonar.organization=byneo-github" /d:"sonar.login=2388c8ee17b65bc2fcc7d45e7ecd6988fe2d5825"
    }
    else {
        MSBuild.SonarQube.Runner.exe begin /k:"$sonarQubeID" /d:"sonar.host.url=$sonarQubeHost" /d:"sonar.organization=byneo-github" /d:"sonar.login=2388c8ee17b65bc2fcc7d45e7ecd6988fe2d5825"
    }

    MsBuild.exe $projectFile /verbosity:minimal
    MSBuild.SonarQube.Runner.exe end /d:"sonar.login=2388c8ee17b65bc2fcc7d45e7ecd6988fe2d5825"

}else {
    Write-Output "not running SonarQube as we're on '$env:APPVEYOR_REPO_NAME'"
}