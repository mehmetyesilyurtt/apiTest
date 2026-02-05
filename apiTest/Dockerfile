# Base SDK image for building
FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src

# Copy csproj and restore as distinct layers
COPY ["apiTest.csproj", "./"]
RUN dotnet restore "apiTest.csproj"

# Copy everything else and build
COPY . .
RUN dotnet publish "apiTest.csproj" -c Release -o /app/publish

# Final stage/image
FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS final
WORKDIR /app
EXPOSE 80
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "apiTest.dll"]
