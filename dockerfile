#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

# Use the SDK image to build the app
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /app

# Copy csproj and restore dependencies
COPY *.csproj ./
RUN dotnet restore

# Copy all files and build the app
COPY . ./
RUN dotnet publish -c Release -o out

# Use runtime image to run the app
FROM mcr.microsoft.com/dotnet/aspnet:7.0
WORKDIR /app
COPY --from=build /app/out ./
ENTRYPOINT ["dotnet", "NameCheapDNSUpdateV2.dll"]


#environment variables
ENV intCheckTimerSEC=300
ENV domain=""
ENV hosts=""
ENV dynamicDNSPassword=""



