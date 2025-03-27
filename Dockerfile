# 使用官方 .NET SDK 作為建置環境
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# 複製 csproj 並還原依賴
COPY *.csproj ./
RUN dotnet restore

# 複製所有內容並建置應用程式
COPY . ./
RUN dotnet publish -c Release -o /out

# 使用輕量級的 .NET 執行環境作為運行環境
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /out .

# 設定容器啟動時的執行指令
CMD ["dotnet", "LineBotTest.dll"]

# 指定應用程式運行的 Port
EXPOSE 8080
