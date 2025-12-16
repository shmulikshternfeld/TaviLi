# שלב הבנייה (Build)
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# העתקת קבצי הפרויקט בלבד כדי לנצל את ה-Cache של דוקר
COPY ["TaviLi.sln", "./"]
COPY ["TaviLi.Api/TaviLi.Api.csproj", "TaviLi.Api/"]
COPY ["TaviLi.Application/TaviLi.Application.csproj", "TaviLi.Application/"]
COPY ["TaviLi.Domain/TaviLi.Domain.csproj", "TaviLi.Domain/"]
COPY ["TaviLi.Infrastructure/TaviLi.Infrastructure.csproj", "TaviLi.Infrastructure/"]

# שחזור התלויות (Restore)
RUN dotnet restore "TaviLi.sln"

# העתקת שאר הקוד ובנייה
COPY . .
WORKDIR "/src/TaviLi.Api"
RUN dotnet build "TaviLi.Api.csproj" -c Release -o /app/build

# פרסום (Publish)
FROM build AS publish
RUN dotnet publish "TaviLi.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# שלב הריצה (Runtime)
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

# חשיפת פורט 8080 (ברירת המחדל)
EXPOSE 8080
ENV ASPNETCORE_HTTP_PORTS=8080

ENTRYPOINT ["dotnet", "TaviLi.Api.dll"]
