FROM mcr.microsoft.com/dotnet/aspnet:7.0

COPY clientSourceCode/src/KubernetesTest/bin/Release/net7.0 net7.0/

WORKDIR /net7.0

CMD ["dotnet", "KubernetesTest.dll"]