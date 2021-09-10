FROM mcr.microsoft.com/dotnet/aspnet:3.1

COPY clientSourceCode/src/KubernetesTest/bin/Release/netcoreapp3.1 netcoreapp3.1/

WORKDIR /netcoreapp3.1

CMD ["dotnet", "KubernetesTest.dll"]