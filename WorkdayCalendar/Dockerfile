# Use the official .NET SDK image to build the application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /app

# Copy the .csproj file and restore any dependencies
COPY *.csproj ./
RUN dotnet restore

# Copy the rest of the application files
COPY . ./

# Publish the application to the /out directory
RUN dotnet publish -c Release -o /out

# Use the official .NET runtime image to run the application
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime

WORKDIR /app

# Expose the port the app will listen on
EXPOSE 8080

# Copy the published output from the build image
COPY --from=build /out .

# Set the entry point for the application
ENTRYPOINT ["dotnet", "WorkdayCalendar.dll"]
