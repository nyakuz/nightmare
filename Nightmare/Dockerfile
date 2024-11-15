#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:9.0-bookworm-slim AS base
#USER app
COPY resolv.conf /etc/resolv.conf
#RUN sed -i 's@archive.ubuntu.com@mirror.kakao.com@g' /etc/apt/sources.list
#RUN apt install -y iproute2 wireguard wireguard-tools openresolv
ENV EXTSHM=ON \
    LD_LIBRARY_PATH="/app/lib:/app/Modules/:/usr/local/lib/:/usr/local/lib64/:${LD_LIBRARY_PATH}" \
    XPHP_CWD=/app/vhost/null \
    XPHP_INI=/app/config/php.ini
RUN echo "Types: deb" >> /etc/apt/sources.list.d/microsoft.sources && \
    echo "URIs: https://packages.microsoft.com/debian/12/prod/" >> /etc/apt/sources.list.d/microsoft.sources && \
    echo "Suites: bookworm" >> /etc/apt/sources.list.d/microsoft.sources && \
    echo "Components: main" >> /etc/apt/sources.list.d/microsoft.sources && \
    echo "trusted: yes" >> /etc/apt/sources.list.d/microsoft.sources && \
    apt clean && apt update && \
    apt install -y supervisor libbrotli1 libmsquic libxml2 libpq5 libsqlite3-0 libonig5 libargon2-1 libpsl-dev && \
    dpkg -r --force-depends openssl libssl3 libssl-dev && \
    mkdir -p /app/session && mkdir -p /app/vhost/null
COPY /DependencyPackages/build/*.deb /
RUN dpkg -i /openssl.deb /libssl3.deb /libssl-dev.deb /libnghttp3.deb /libnghttp2.deb /libngtcp2.deb /curl.deb /php.deb && \
    rm *.deb && \
    cd /usr/lib/*-linux-gnu/ && ln -s libssl.so libssl.so.3 && \
    cd /usr/lib/*-linux-gnu/ && ln -s libcrypto.so libcrypto.so.3
ENV LD_PRELOAD="/usr/local/lib/libphp.so:${LD_PRELOAD}"
WORKDIR /app
EXPOSE 8080 8443
EXPOSE 8443/udp

# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Nightmare/Nightmare.csproj", "Nightmare/"]
RUN dotnet restore "Nightmare/Nightmare.csproj"
COPY . .
WORKDIR "/src/Nightmare"
RUN dotnet build "Nightmare.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Nightmare.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=true

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
CMD ["supervisord", "-c", "/app/config/supervisord.conf"]
#CMD ["dotnet","Nightmare.dll"]
