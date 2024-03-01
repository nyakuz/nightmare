#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0-bookworm-slim AS base

#RUN sed -i 's@archive.ubuntu.com@mirror.kakao.com@g' /etc/apt/sources.list

COPY resolv.conf /etc/resolv.conf
RUN apt clean && apt update
RUN apt install -y supervisor
RUN apt install -y wireguard openresolv iproute2

RUN echo "Types: deb" >> /etc/apt/sources.list.d/microsoft.sources
RUN echo "URIs: https://packages.microsoft.com/ubuntu/22.10/prod/" >> /etc/apt/sources.list.d/microsoft.sources
RUN echo "Suites: kinetic" >> /etc/apt/sources.list.d/microsoft.sources
RUN echo "Components: main" >> /etc/apt/sources.list.d/microsoft.sources
RUN echo "trusted: yes" >> /etc/apt/sources.list.d/microsoft.sources

RUN apt clean && apt update
RUN apt install -y libbrotli1 libmsquic
RUN apt install -y libxml2 libpq5 libsqlite3-0 libonig5 libargon2-1 libpsl-dev
RUN dpkg -r --force-depends openssl libssl3 libssl-dev

RUN rm /etc/apt/sources.list.d/microsoft.sources

ENV EXTSHM ON
ENV LD_LIBRARY_PATH "/app/lib:/app/Modules/:/usr/local/lib/:/usr/local/lib64/:${LD_LIBRARY_PATH}"
ENV XPHP_CWD=/app/vhost/null
ENV XPHP_INI=/app/config/php.ini
RUN mkdir -p /app/session && mkdir -p /app/vhost/null
COPY *.deb/ /
RUN dpkg -i /openssl.deb /libssl3.deb /libssl-dev.deb /libnghttp3.deb /libnghttp2.deb /libngtcp2.deb /curl.deb /php.deb
RUN rm *.deb
RUN cd /usr/lib/*-linux-gnu/ && ln -s libssl.so libssl.so.3
RUN cd /usr/lib/*-linux-gnu/ && ln -s libcrypto.so libcrypto.so.3
ENV LD_PRELOAD "/usr/local/lib/libphp.so:${LD_PRELOAD}"
WORKDIR /app
EXPOSE 8080 8443
EXPOSE 8443/udp

FROM mcr.microsoft.com/dotnet/sdk:latest AS build
WORKDIR /src
COPY ["Nightmare/Nightmare.csproj", "Nightmare/"]
RUN dotnet restore "Nightmare/Nightmare.csproj"
COPY . .
WORKDIR "/src/Nightmare"
RUN dotnet build "Nightmare.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Nightmare.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
CMD ["supervisord", "-c", "/app/config/supervisord.conf"]
#CMD ["dotnet","Nightmare.dll"]