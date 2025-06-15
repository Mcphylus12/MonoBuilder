dotnet publish ConsoleRunner/ConsoleRunner.csproj --ucr
cp ./ConsoleRunner/bin/Release/net9.0/win-x64/publish/ConsoleRunner.exe MonoBuilder.exe
echo "git diff --name-only | ./MonoBuilder.exe --config=config.json"
