# 使用 .NET SDK 作為基底映像檔
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

# 設定工作目錄
WORKDIR /app

# 複製 .csproj 檔案到容器中的 /app 目錄
COPY ./LineBotTest/LineBotTest.csproj ./LineBotTest/

# 還原專案的依賴
RUN dotnet restore ./LineBotTest/LineBotTest.csproj

# 複製整個專案檔案
COPY ./LineBotTest/. ./LineBotTest/

# 建置應用程式
RUN dotnet publish ./LineBotTest/LineBotTest.csproj -c Release -o /app/publish

# 使用 ASP.NET Core 映像檔作為執行環境
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime

# 設定工作目錄
WORKDIR /app

# 複製已建置好的檔案
COPY --from=build /app/publish .

# 設定容器啟動時執行的命令
ENTRYPOINT ["dotnet", "LineBotTest.dll"]

# 開放容器的 8080 端口
EXPOSE 8080
