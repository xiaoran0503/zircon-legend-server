# 皓石传奇三服务端镜像：直接从本仓库源码多阶段构建（.NET 10 / linux-x64）
# 若要复刻上游"从 GitHub Release 拉包"的做法，参考 Dockerfile.upstream-release

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# 先拷工程文件，最大化依赖层缓存
COPY Directory.Build.props ./
COPY Server/Server.csproj Server/
COPY Library/Library/Library.csproj Library/Library/
RUN dotnet restore Server/Server.csproj -r linux-x64

# 再拷源码（datas/.build/bin/obj/.git 已由 .dockerignore 排除）
COPY . .
RUN dotnet publish Server/Server.csproj -c Release -r linux-x64 \
      -p:SolutionDir=/src/ \
      -p:DebugType=None -p:DebugSymbols=false \
      -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /zircon
ENV TZ=Asia/Shanghai \
    DOTNET_GCServer=1
COPY --from=build /app/publish/ ./
RUN mkdir -p /zircon/datas
EXPOSE 7000 3000 7080 7081
VOLUME ["/zircon/datas"]
ENTRYPOINT ["./Server"]
