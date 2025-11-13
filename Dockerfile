ARG DOTNET_VERSION=10.0

FROM mcr.microsoft.com/dotnet/sdk:${DOTNET_VERSION}-alpine AS build
RUN dotnet workload install wasm-tools
WORKDIR /src

# tailwind install
RUN set -ex; \
    apkArch="$(apk --print-arch)"; \
    case "$apkArch" in \
        aarch64) arch='linux-arm64' ;; \
        x86_64) arch='linux-x64' ;; \
    esac; \
    echo Downloading tailwindcss for $apkArch ; \
    curl -sL -o tailwindcss https://github.com/tailwindlabs/tailwindcss/releases/download/v3.4.17/tailwindcss-$arch; \
    chmod +x tailwindcss; \
    ls -al tailwindcss ;

RUN apk add python3

# restore
COPY *.sln .
COPY PointingParty/PointingParty.csproj PointingParty/PointingParty.csproj
COPY PointingParty.Client/PointingParty.Client.csproj PointingParty.Client/PointingParty.Client.csproj
COPY PointingParty.Client.Tests/PointingParty.Client.Tests.csproj PointingParty.Client.Tests/PointingParty.Client.Tests.csproj
COPY PointingParty.Domain/PointingParty.Domain.csproj PointingParty.Domain/PointingParty.Domain.csproj
RUN dotnet restore

# copy everything
COPY . ./

# tailwind build 
RUN set -ex; \
    ./tailwindcss -i PointingParty/wwwroot/app.css -o PointingParty/wwwroot/app.min.css --minify && rm PointingParty/wwwroot/app.css

# build
RUN dotnet publish --no-restore -c Release -o /app PointingParty

# final stage/image
FROM mcr.microsoft.com/dotnet/aspnet:${DOTNET_VERSION}-alpine
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080
WORKDIR /app
COPY --from=build /app .
ENTRYPOINT [ "./PointingParty" ]
