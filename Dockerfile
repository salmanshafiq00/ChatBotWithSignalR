#This is a test demo practice
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build

WORKDIR /app

EXPOSE 80

COPY ./ChatBotWithSignalR/*.csproj ./

RUN dotnet restore

COPY . ./

RUN dotnet publish -c release -o ./publish

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS final

WORKDIR /app

COPY --from=build /app/publish .

ENTRYPOINT [ "dotnet", "ChatBotWithSignalR.dll" ]



