$projectFile = "src\NLog.MongoDB.NetCore\NLog.MongoDB.NetCore.xproj"
$github = "ByNeo/NLog.MongoDB.NetCore"
$sonarQubeID = "NLog.MongoDB.NetCore"
$sonarQubeHost = "https://sonarcloud.io"

choco install "msbuild-sonarqube-runner" -y

SonarQube.Scanner.MSBuild.exe begin /k:"$sonarQubeID" /d:"sonar.host.url=$sonarQubeHost" /d:"sonar.login=GBaeujJgXPbcDto3kyJXpwrTJdPNKNOjwzyzAGNvvKYg+X/S3anpG+8qKMBjjGFI"

MsBuild.exe $projectFile /t:Rebuild

SonarQube.Scanner.MSBuild.exe end /d:"sonar.login=GBaeujJgXPbcDto3kyJXpwrTJdPNKNOjwzyzAGNvvKYg+X/S3anpG+8qKMBjjGFI"
