FROM microsoft/aspnetcore
WORKDIR /VesselService
ADD /bin/Debug/netcoreapp1.1/publish /VesselService
COPY appsettings.json /VesselService/appsettings.json
EXPOSE 5003
ENTRYPOINT ["dotnet", "VesselService.dll"]