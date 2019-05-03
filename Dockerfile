FROM microsoft/dotnet:2.1-sdk AS build
WORKDIR /app

FROM microsoft/dotnet:2.1-sdk
COPY EVCSCenterServer/bin/Debug/netcoreapp2.1/  /root/
WORKDIR /root/
EXPOSE 2010/tcp
ENTRYPOINT dotnet /root/EVCS.dll
