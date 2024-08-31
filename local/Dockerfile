# Custom docker image based on https://github.com/abarichello/godot-ci
# Godot-CI image had some issues with running ImGUI as it's using an older version of GLIBC and it's mono base is unmaintained

FROM ubuntu:22.04

# When in doubt see the downloads page
# https://downloads.tuxfamily.org/godotengine/
ARG GODOT_VERSION="4.3"

# Example values: stable, beta3, rc1, alpha2, etc.
# Also change the SUBDIR property when NOT using stable
ARG RELEASE_NAME="stable"

# This is only needed for non-stable builds (alpha, beta, RC)
# e.g. SUBDIR "/beta3"
# Use an empty string "" when the RELEASE_NAME is "stable"
ARG SUBDIR=""

ARG ZIP_GODOT_PLATFORM="linux_x86_64"
ARG FILENAME_GODOT_PLATFORM="linux.x86_64"

RUN apt-get update && apt-get install -y --no-install-recommends \
    ca-certificates \
    unzip \
    wget \
    dotnet-sdk-8.0 \
    libimgui-dev

RUN wget --progress=dot:giga https://downloads.tuxfamily.org/godotengine/${GODOT_VERSION}${SUBDIR}/mono/Godot_v${GODOT_VERSION}-${RELEASE_NAME}_mono_${ZIP_GODOT_PLATFORM}.zip \
    && mkdir -p ~/.config/godot \
    && unzip Godot_v${GODOT_VERSION}-${RELEASE_NAME}_mono_${ZIP_GODOT_PLATFORM}.zip \
    && mv Godot_v${GODOT_VERSION}-${RELEASE_NAME}_mono_${ZIP_GODOT_PLATFORM}/Godot_v${GODOT_VERSION}-${RELEASE_NAME}_mono_${FILENAME_GODOT_PLATFORM} /usr/local/bin/godot \
    && mv Godot_v${GODOT_VERSION}-${RELEASE_NAME}_mono_${ZIP_GODOT_PLATFORM}/GodotSharp /usr/local/bin/GodotSharp

WORKDIR /app

COPY . .

EXPOSE 9050

# ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1

RUN dotnet build -p:GodotTargetPlatform=linuxbsd -c Debug

CMD [ "godot", "--headless", "--verbose", "server"]