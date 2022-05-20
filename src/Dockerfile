#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["PlanningApi/PlanningApi.csproj", "src/PlanningApi/"]
COPY ["CalendarServices/CalendarServices.csproj", "src/CalendarServices/"]
COPY ["Crm.Link.RabbitMq/Crm.Link.RabbitMq.csproj", "src/Crm.Link.RabbitMq/"]
COPY ["Crm.Link.UUID/Crm.Link.UUID.csproj", "src/Crm.Link.UUID/"]
RUN dotnet restore "src/PlanningApi/PlanningApi.csproj"
COPY . .
WORKDIR "/src/src/PlanningApi"
RUN dotnet build "PlanningApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "PlanningApi.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PlanningApi.dll"]