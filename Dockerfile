# Adjust DOTNET_OS_VERSION as desired
ARG DOTNET_SDK_VERSION=8.0

FROM mcr.microsoft.com/dotnet/sdk:${DOTNET_SDK_VERSION}-alpine AS build
WORKDIR /src

# tailwind install
RUN set -ex; \
    apkArch="$(apk --print-arch)"; \
    case "$apkArch" in \
        aarch64) arch='linux-arm64' ;; \
        x86_64) arch='linux-x64' ;; \
    esac; \
    echo Downloading tailwindcss for $apkArch ; \
    curl -sL -o tailwindcss https://github.com/tailwindlabs/tailwindcss/releases/latest/download/tailwindcss-$arch; \
    chmod +x tailwindcss; \
    ls -al tailwindcss ;

# restore
COPY *.csproj .
RUN dotnet restore -r linux-musl-amd64 -p:PublishReadyToRun=true

# copy everything
COPY . ./

# tailwind build 
RUN set -ex; \
    ./tailwindcss -i wwwroot/app.css -o wwwroot/app.min.css --minify && rm wwwroot/app.css

# build
RUN dotnet publish --no-restore -c Release -r linux-musl-amd64 -p:PublishReadyToRun=true -o /app

# final stage/image
FROM mcr.microsoft.com/dotnet/aspnet:${DOTNET_SDK_VERSION}-alpine
ENV ASPNETCORE_URLS http://+:8080
ENV ASPNETCORE_ENVIRONMENT Production
EXPOSE 8080
WORKDIR /app
COPY --from=build /app .
ENTRYPOINT [ "./PointingParty" ]
