# Stage 1: Development (with hot reload + EF tools)
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS development
WORKDIR /app
RUN dotnet tool install -g dotnet-ef
ENV PATH="${PATH}:/root/.dotnet/tools"
COPY . .
RUN dotnet restore "src/InvoiceSaaS.Api/InvoiceSaaS.Api.csproj"
EXPOSE 5000
CMD ["dotnet", "watch", "run", "--project", "src/InvoiceSaaS.Api/InvoiceSaaS.Api.csproj", "--urls", "http://+:5000"]

# Stage 2: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /source
COPY ["src/InvoiceSaaS.Api/InvoiceSaaS.Api.csproj", "src/InvoiceSaaS.Api/"]
COPY ["src/InvoiceSaaS.Application/InvoiceSaaS.Application.csproj", "src/InvoiceSaaS.Application/"]
COPY ["src/InvoiceSaaS.Domain/InvoiceSaaS.Domain.csproj", "src/InvoiceSaaS.Domain/"]
COPY ["src/InvoiceSaaS.Infrastructure/InvoiceSaaS.Infrastructure.csproj", "src/InvoiceSaaS.Infrastructure/"]
RUN dotnet restore "src/InvoiceSaaS.Api/InvoiceSaaS.Api.csproj"
COPY . .
RUN dotnet publish "src/InvoiceSaaS.Api/InvoiceSaaS.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 3: Production
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS production
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 5000
HEALTHCHECK --interval=30s --timeout=10s --start-period=40s --retries=3 \
  CMD curl -f http://localhost:5000/health || exit 1
ENTRYPOINT ["dotnet", "InvoiceSaaS.Api.dll"]
