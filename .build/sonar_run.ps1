$projectFile = "src\NLog.MongoDB.NetCore\NLog.MongoDB.NetCore.xproj"
$github = "ByNeo/NLog.MongoDB.NetCore"
$sonarQubeID = "NLog.MongoDB.NetCore"
$sonarQubeHost = "https://sonarcloud.io"

SonarQube.Scanner.MSBuild.exe begin \
                              /k:"$sonarQubeID" \
                              /d:"sonar.host.url=$sonarQubeHost" \
                              /d:"sonar.login=$env:sonar_token"

MsBuild.exe $projectFile /t:Rebuild

SonarQube.Scanner.MSBuild.exe end /d:"sonar.login=$env:sonar_token"
