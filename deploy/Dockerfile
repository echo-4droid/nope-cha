FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src

# Restore dependencies
COPY src/CloudflareCaptchaSolver/CloudflareCaptchaSolver.csproj /src/CloudflareCaptchaSolver/
RUN dotnet restore /src/CloudflareCaptchaSolver/CloudflareCaptchaSolver.csproj

# Copy source code and build host project
COPY . /src
RUN dotnet build /src/CloudflareCaptchaSolver/CloudflareCaptchaSolver.csproj -c Release -o /app/build

# Make final image
FROM mcr.microsoft.com/dotnet/aspnet:7.0
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "CloudflareCaptchaSolver.dll"]
ENV ASPNETCORE_URLS http://+:80
EXPOSE 80
