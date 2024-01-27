FROM mcr.microsoft.com/playwright/dotnet:latest AS build

# Restore dependencies
COPY src/CloudflareCaptchaSolver/CloudflareCaptchaSolver.csproj /src/CloudflareCaptchaSolver/
RUN dotnet restore /src/CloudflareCaptchaSolver/CloudflareCaptchaSolver.csproj

# Copy source code and build host project
COPY src /src
RUN dotnet build /src/CloudflareCaptchaSolver/CloudflareCaptchaSolver.csproj -c Release -o /app/build
RUN dotnet publish /src/CloudflareCaptchaSolver/CloudflareCaptchaSolver.csproj -c Release -o /app/publish

# Make final image
FROM mcr.microsoft.com/playwright/dotnet:latest
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "CloudflareCaptchaSolver.dll"]
ENV ASPNETCORE_URLS http://+:80
EXPOSE 80
