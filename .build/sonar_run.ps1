$projectFile = "src\NLog.MongoDB.NetCore\NLog.MongoDB.NetCore.xproj"
$github = "ByNeo/NLog.MongoDB.NetCore"
$sonarQubeID = "NLog.MongoDB.NetCore"
$sonarQubeHost = "https://sonarcloud.io"

choco install "msbuild-sonarqube-runner" -y

SonarQube.Scanner.MSBuild.exe begin /k:"$sonarQubeID" /d:"sonar.host.url=$sonarQubeHost" /d:"sonar.organization=byneo-github" /d:"sonar.login=2388c8ee17b65bc2fcc7d45e7ecd6988fe2d5825"

MsBuild.exe $projectFile /t:Rebuild

SonarQube.Scanner.MSBuild.exe end /d:"sonar.login=2388c8ee17b65bc2fcc7d45e7ecd6988fe2d5825"
