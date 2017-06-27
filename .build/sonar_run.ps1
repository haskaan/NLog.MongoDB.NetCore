$projectFile = "src\NLog.MongoDB.NetCore\NLog.MongoDB.NetCore.xproj"
$github = "ByNeo/NLog.MongoDB.NetCore"
$sonarQubeID = "NLog.MongoDB.NetCore"
$sonarQubeHost = "https://sonarcloud.io"
$sonarToken = "2388c8ee17b65bc2fcc7d45e7ecd6988fe2d5825"

if($env:APPVEYOR_REPO_NAME -eq $github){

    if(-not $sonarToken){
        Write-warning "not running SonarQube, no sonar_token"
        return;
    }

    choco install "msbuild-sonarqube-runner" -y
     
    if ($env:APPVEYOR_PULL_REQUEST_NUMBER) { 
        MSBuild.SonarQube.Runner.exe begin /k:"$sonarQubeID" /d:"sonar.host.url=$sonarQubeHost" /d:"sonar.organization=byneo-github" /d:"sonar.login=$sonarToken"
    }
    else {
        MSBuild.SonarQube.Runner.exe begin /k:"$sonarQubeID" /d:"sonar.host.url=$sonarQubeHost" /d:"sonar.organization=byneo-github" /d:"sonar.login=$sonarToken"
    }

    MsBuild.exe $projectFile /verbosity:minimal
    MSBuild.SonarQube.Runner.exe end /d:"sonar.login=$sonarToken"

}else {
    Write-Output "not running SonarQube as we're on '$env:APPVEYOR_REPO_NAME'"
}