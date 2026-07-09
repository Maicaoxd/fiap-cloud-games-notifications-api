FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/NotificationsAPI/NotificationsAPI.csproj", "src/NotificationsAPI/"]
RUN dotnet restore "src/NotificationsAPI/NotificationsAPI.csproj"
COPY . .
WORKDIR "/src/src/NotificationsAPI"
RUN dotnet publish "NotificationsAPI.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM runtime AS final
USER $APP_UID
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "NotificationsAPI.dll"]
