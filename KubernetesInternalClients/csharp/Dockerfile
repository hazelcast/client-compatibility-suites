FROM mcr.microsoft.com/dotnet/runtime:8.0

COPY clientSourceCode/src/KubernetesTest/bin/Release/net8.0 net8.0/

WORKDIR /net8.0

CMD ["dotnet", "KubernetesTest.dll"]